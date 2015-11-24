#include "BrewMonitor.h"
#include "onewire.h"
#include "ds18x20.h"
#include "pcb13.h"
#include "memory.h"
#include <math.h>
#include "uart.h"

#define UART_BAUD_RATE  9600


#define THINGSPEAK_KEY "VSSYPQ8OSSJGPUDO"

// Buffer to hold the previously generated HID report, for comparison purposes inside the HID class driver.
static uint8_t PrevHIDReportBuffer[GENERIC_REPORT_SIZE];

// settings
uint8_t _recording; // you can only record when disconnected from usb.
//uint8_t _usbIsConnected; // you will always slow stream when connected to usb.
uint8_t _fastStreaming; // you can only fast stream when on usb. slow streaming will be disabled.

uint16_t _nextMemoryPosition; // points to the next address to write to in eeprom
uint8_t _compactionCount; // the number of data compactions that have been done
uint16_t _divisorCounter; // once this reaches the _compactionCount we are ready to store another sample
uint8_t _adcLowThreshold;
uint8_t _adcHighThreshold;
uint16_t _bubbleDelay;
uint8_t _bubbleOccurring;
uint16_t _downButtonCounter; // counts number of interrupts while rec button held down
uint8_t _debounceCounter;
uint8_t _recordLedTurnOffCounter; // asynchronously turn off leds

// main loop async processing flags
uint8_t _measureTempFlag; // flag to tell main loop to take a measurement
uint8_t _recordSampleFlag; // tells main loop to record a sample to memory
uint8_t _sendSampleFlag; // tells main loop to send a sample over USB
uint8_t _startRecordingFlag; // tells main loop to initiate recording mode
uint8_t _resumeRecordingFlag; // tells main loop to initiate recording mode continuing on from the previous recording
uint8_t _stopRecordingFlag; // tells main loop to cancel recording mode
uint8_t _clearMemoryFlag; // tell main loop to clear external memory

// USB communication variables
uint8_t _usbArg0, _usbArg1, _usbArg2, _usbArg3, _usbArg4, _usbArg5, _usbArg6, _usbArg7, _usbArg8, _usbArg9, _usbArg10, _usbArg11, _dataReady;

// thermometer setup
uint8_t _sensorIDs[MAXSENSORS][OW_ROMCODE_SIZE];

// temperature and bubble globals
uint16_t _temperature; // stores the current temperature value
uint16_t _recordBubbleCount; // gets cleared every time a sample is stored
uint16_t _usbBubbleCount; // gets cleared every time a sample is sent
uint8_t _readyForBubble; // need a delay between bubbles
uint8_t _bubbleLow1Detected; // need a low before waiting for a high
uint8_t _bubbleHighDetected; // need a high before waiting for next low
uint8_t _bubbleLow2Detected; // after 2nd low, a bubble has occurred
uint8_t _adcValue; // raw adc value is stored here
uint8_t _secondsCounter; // used to upload data to server every n seconds

// LUFA HID Class driver interface configuration and state information. This structure is
// passed to all HID Class driver functions, so that multiple instances of the same class
// within a device can be differentiated from one another.
USB_ClassInfo_HID_Device_t Generic_HID_Interface =
{
	.Config =
	{
		.InterfaceNumber              = 0,
		.ReportINEndpoint             =
		{
			.Address              = GENERIC_IN_EPADDR,
			.Size                 = GENERIC_EPSIZE,
			.Banks                = 1,
		},
		.PrevReportINBuffer           = PrevHIDReportBuffer,
		.PrevReportINBufferSize       = sizeof(PrevHIDReportBuffer),
	},
};

// main program entry point
int main(void)
{
	output_high(RECORD_LED_PORT, RECORD_LED_PIN);
	output_high(BUBBLE_LED_PORT, BUBBLE_LED_PIN);
	SetupHardware();
	_delay_ms(200); // make sure the lights flash for a while even if everything else initialises fast
	LoadSettings();
	uart_init( UART_BAUD_SELECT(UART_BAUD_RATE,F_CPU) ); 
	//uart_init( UART_BAUD_SELECT(UART_BAUD_RATE,F_CPU) ); 
	USB_Init();
	
	//output_high(BUBBLE_LED_PORT, BUBBLE_LED_PIN);
	//FillMemoryWithDebugData(); // this is to test the memory splitting algorithm
	//output_low(BUBBLE_LED_PORT, BUBBLE_LED_PIN);
	
	SetupTimerInterrupts();
	output_low(RECORD_LED_PORT, RECORD_LED_PIN);
	output_low(BUBBLE_LED_PORT, BUBBLE_LED_PIN);

	for (;;)
	{
		HID_Device_USBTask(&Generic_HID_Interface);
		USB_USBTask();
		
		if(_recordSampleFlag)
		{
			RecordSample(); // write to eeprom
		}
		else if(_sendSampleFlag)
		{
			SendSample(); // write to usb
		}
		else if(_startRecordingFlag)
		{
			// start a new recording
			StartRecording(1);
		}
		else if(_resumeRecordingFlag)
		{
			// resume recording
			StartRecording(0);
		}
		else if(_stopRecordingFlag)
		{
			StopRecording();
		}
		else if(_measureTempFlag)
		{
			_temperature = DS18X20_read_raw_single();
			DS18X20_start_meas(DS18X20_POWER_EXTERN, NULL); // start next temperature conversion
			_measureTempFlag = 0;
		}
		else if(_clearMemoryFlag)
		{
			ClearExternalMemory();
		}
	}
}

// load settings from eeprom
void LoadSettings(void)
{
	// ensure initial EEPROM settings have been defined on first boot
	if(eeprom_read_byte((uint8_t*)COMPACTION_COUNT) == 0xFF)
	{
		eeprom_update_byte((uint8_t*)RECORDING_FLAG, 0); // set recording off
		eeprom_update_byte((uint8_t*)COMPACTION_COUNT, 0); // number of memory compacts
		eeprom_update_byte((uint8_t*)ADC_LOW_VALUE, 20); // set default low value = 20
		eeprom_update_byte((uint8_t*)ADC_HIGH_VALUE, 50); // set default high value = 50
		eeprom_update_byte((uint8_t*)BUBBLE_DELAY_LOW, 0); // set bubble delay to 8192
		eeprom_update_byte((uint8_t*)BUBBLE_DELAY_HIGH, 32);
	}
	
	// initialize volatile variables
	_nextMemoryPosition = GetNextEmptyAddress(); // work out first blank memory location
	_compactionCount = eeprom_read_byte((uint8_t*)COMPACTION_COUNT);
	_adcLowThreshold = eeprom_read_byte((uint8_t*)ADC_LOW_VALUE); // store adc low setting from memory
	_adcHighThreshold = eeprom_read_byte((uint8_t*)ADC_HIGH_VALUE); // store adc high setting from memory
	_bubbleDelay = eeprom_read_byte((uint8_t*)BUBBLE_DELAY_HIGH); // bubble delay value
	_bubbleDelay *= 256;
	_bubbleDelay += eeprom_read_byte((uint8_t*)BUBBLE_DELAY_LOW);
	
	// check if we were previously recording
	if(eeprom_read_byte((uint8_t*)RECORDING_FLAG) == 1)
	{
		// if button is pressed, resume previous recording without starting a new one
		if(!bit_get(RECORD_BUTTON_INPORT, RECORD_BUTTON_PIN))
		{
			// button is pressed so set resume flag
			WaitForButtonRelease();
			_resumeRecordingFlag = 1;			
		}
		else
		{
			// button not pressed so start a new recording
			_startRecordingFlag  = 1;
		}		
	}			
}

// de bounce button, ensure its really released
void WaitForButtonRelease(void)
{
	uint8_t consecutiveReleasedCount = 0;
	while(1)
	{
		_delay_ms(10);
		if(!bit_get(RECORD_BUTTON_INPORT, RECORD_BUTTON_PIN))
		{
			// switch is pressed, reset the counter
			consecutiveReleasedCount = 0;
		}
		
		if(consecutiveReleasedCount == 20)
		{
			break;
		}
		
		consecutiveReleasedCount++;
	}
	_delay_ms(10);
}

// configure registers and ports
void SetupHardware(void)
{
	// Disable clock division
	clock_prescale_set(clock_div_1);
	
	// Turn off JTAG so we can use portF
	MCUCR=(1<<JTD);
	MCUCR=(1<<JTD);
	
	// disable watchdog if enabled by boot loader/fuses
	MCUSR &= ~(1 << WDRF);
	wdt_disable();
	
	// setup DDR registers
	set_output(RECORD_LED_DDR, RECORD_LED_PIN);
	set_output(BUBBLE_LED_DDR, BUBBLE_LED_PIN);
	set_output(ONEWIRE_POWER_DDR, ONEWIRE_POWER_PIN);
	set_output(BUBBLE_SENSOR_PULLUP_DDR, BUBBLE_SENSOR_PULLUP_PIN);
	set_output(BUBBLE_IR_LED_DDR, BUBBLE_IR_LED_PIN);
	set_input(RECORD_BUTTON_DDR, RECORD_BUTTON_PIN);
	set_input(BUBBLE_SENSOR_DDR, BUBBLE_SENSOR_PIN);
	set_output(EEPROM_WP_DDR, EEPROM_WP_PIN);
	set_output(EEPROM_SCL_DDR, EEPROM_SCL_PIN);
	set_output(EEPROM_SDA_DDR, EEPROM_SDA_PIN);
	
	// turn on 1wire power and pull up
	output_high(ONEWIRE_POWER_PORT, ONEWIRE_POWER_PIN);
	output_high(ONEWIRE_PULLUP_PORT, ONEWIRE_PULLUP_PIN);
	
	// turn on bubble IR led and sensor pullup
	output_high(BUBBLE_IR_LED_PORT, BUBBLE_IR_LED_PIN);
	output_high(BUBBLE_SENSOR_PULLUP_PORT, BUBBLE_SENSOR_PULLUP_PIN);
	
	// set up temperature sensors
	_delay_ms(400); // wait for sensors to come online
	ow_set_bus(&ONEWIRE_DATA_INPORT, &ONEWIRE_DATA_PORT, &ONEWIRE_DATA_DDR, ONEWIRE_DATA_PIN);
	
	LC32_Init();
	
	// setup ADC (multiplexer is set to ch0 by default)
	ADCSRA |= (1 << ADPS2); // set ADC prescaler to 128 (=125KHz (5ksps))
	ADCSRA |= (1 << ADPS1); // set ADC prescaler to 128 (=125KHz (5ksps))
	ADCSRA |= (1 << ADPS0); // set ADC prescaler to 128 (=125KHz (5ksps))
	ADMUX |= (1 << REFS0); // set reference voltage to AVcc
	ADMUX |= (1 << ADLAR); // left align the ADC value so we can read it from ADCH
	ADMUX |= (1 << MUX0); // set MUX to ADC1

	ADCSRA |= (1 << ADEN); // enable the ADC
	// ADCSRA |= (1 << ADIE); // enable ADC interrupt
	ADCSRA |= (1 << ADSC); // start a conversion
}

// enable timers etc.
void SetupTimerInterrupts(void)
{
	// configure timer0
	TCCR0B |= (1 << CS00); // divide clock by 1024 (about 70ms)
	TCCR0B |= (1 << CS02);
	//TCNT0 is the 8 bit counter
	//OCR0A and OCR0B are teh compare registers
	
	// configure timer1
	TCCR1B |= (1 << CS10); // prescale timer1 by 1024
	TCCR1B |= (1 << CS12); // prescale timer1 by 1024
	TIMSK1 |= (1 << OCIE1A); // oc1a interrupt each second for temperature and storing data
	TIMSK1 |= (1 << OCIE1B); // oc1b interrupt 25 per second for adc, fast streaming and bubble calculations
	//TIMSK1 |= (1 << OCIE1C); // oc1c interrupt for inter-bubble delay
	
	// enable record button interrupt
	PCICR |= (1 << PCIE0);
	PCMSK0 |= (1 << 7);
	
	// enable the counter increment interrupt
	TIMSK0 |= (1 << OCIE0B);
	
	sei(); // enable interrupts
	
	// enable bubble sensing
	_readyForBubble = 1;
}

// record button interrupt
ISR(PCINT0_vect)
{
	if(!bit_get(RECORD_BUTTON_INPORT, RECORD_BUTTON_PIN)) // high-low transition (push) means start
	{
		// reset the counter
		_downButtonCounter = 0;
		return;
	}
	else // low-high transition (release) means process command
	{
		if(_downButtonCounter > 290) // ignore and reset counter
		{
			_clearMemoryFlag = 1;
			
			// lock out button for a while
			PCICR &= ~(1 << PCIE0); // turn off this interrupt
			_debounceCounter = 0;
		}
		else if(_downButtonCounter > 0) // record and reset counter
		{
			// toggle recording mode
			if(_recording)
			{
				_stopRecordingFlag = 1;
			}
			else
			{
				_startRecordingFlag = 1;
			}
			
			// lock out button for a while
			PCICR &= ~(1 << PCIE0); // turn off this interrupt
			_debounceCounter = 0;
		}
	}
}

// for record button timing
ISR(TIMER0_COMPB_vect)
{
	if(!bit_get(RECORD_BUTTON_INPORT, RECORD_BUTTON_PIN)) // button is down
	{
		_downButtonCounter++;
		//output_low(BUBBLE_LED_PORT, BUBBLE_LED_PIN);
	}
	else
	{
		_downButtonCounter = 0;
	}
	
	if(_debounceCounter++ == 3)
	{
		// re enable button interrupts
		PCICR |= (1 << PCIE0);
	}
	
	if(_downButtonCounter > 290)
	{
		// we are ready to erase memory, start flashing the REC LED
		output_toggle(RECORD_LED_PORT, RECORD_LED_PIN);
	}
}

// persist recording flag and start/continue recording
void StartRecording(uint8_t newRecording)
{
	// acknowledge function call
	_startRecordingFlag = 0;
	_resumeRecordingFlag = 0;
	
	// remember we are recording so we can continue after reboot
	eeprom_update_byte((uint8_t*)RECORDING_FLAG, 1);	
	
	if(newRecording || _nextMemoryPosition == 0)
	{
		// write a stop/start marker to memory
		Sample sample;
		sample.temperature = 0;
		sample.bubbles = 0xFFFF;
		LC32_WriteSample(_nextMemoryPosition, sample);
		_nextMemoryPosition += 4;
	}
	
	// manually record a sample immediately
	_recordSampleFlag = 1;
	_measureTempFlag = 1;
	// next record interrrupt in 1 second
	OCR1A = TCNT1 + 0x3D09;
	
	// enable recording flag
	_recording = 1;
		
	// reset the bubble counter
	_recordBubbleCount = 0;
	
	// reset the divisor (probably not necessary)
	_divisorCounter = 0;
}

// set the recording flag to false
void StopRecording(void)
{
	// acknowledge function call
	_stopRecordingFlag = 0;
	
	eeprom_update_byte((uint8_t*)RECORDING_FLAG, 0);
	_recording = 0;
	
	output_low(RECORD_LED_PORT, RECORD_LED_PIN);
}

// overwrite memory portion of EEPROM with 0xFF's
void ClearExternalMemory(void)
{
	_clearMemoryFlag = 0;
	StopRecording();
	output_high(RECORD_LED_PORT, RECORD_LED_PIN);
	LC32_Clear();
	eeprom_update_byte((uint8_t*)COMPACTION_COUNT, 0); // set divisor back to 0
	_compactionCount = 0;
	_nextMemoryPosition = 0; // begin recording from start
	_divisorCounter = 0;
	output_low(RECORD_LED_PORT, RECORD_LED_PIN);
	_dataReady = 1; // send usb acknowledgement
}

// scan through external memory to find next empty address
uint16_t GetNextEmptyAddress(void)
{
	uint16_t address = 0;
	for(uint16_t i = 0; i < 1024; i ++)
	{
		Sample sample = LC32_ReadSample(i * 4);
		if((sample.temperature == 0xFFFF) && (sample.bubbles == 0xFFFF))
		{
			address = i * 4;
			break;
		}
	}
	return address;
}

// find DS18X20 devices and store their addresses in _sensorIds.
uint8_t Search_sensors(void)
{
	ow_reset();

	uint8_t nSensors = 0;
	uint8_t diff = OW_SEARCH_FIRST;
	uint8_t id[OW_ROMCODE_SIZE];
	while (diff != OW_LAST_DEVICE && nSensors < MAXSENSORS)
	{
		DS18X20_find_sensor( &diff, &id[0] );
		if( diff == OW_PRESENCE_ERR ) // no sensor found
		break;
		if( diff == OW_DATA_ERR ) // bus error
		break;
		uint8_t i;
		for (i=0; i < OW_ROMCODE_SIZE; i++)
		_sensorIDs[nSensors][i] = id[i];
		nSensors++;
	}
	
	return nSensors;
}

// turns off recording led after 320ms.
ISR(TIMER0_COMPA_vect)
{
	if(_recordLedTurnOffCounter++ == 20)
	{
		// turn off record LED
		output_low(RECORD_LED_PORT, RECORD_LED_PIN);
		
		// clear counter
		_recordLedTurnOffCounter = 0;
		
		// disable oc0a interrupt
		TIMSK0 &= ~(1 << OCIE0A);
	}
}

// record values once per second
ISR(TIMER1_COMPA_vect)
{
	// set next interrupt in 1 second
	OCR1A += 0x3D09; 
	
	// tell main loop to record a sample outside of this ISR
	if(_recording)
	{
		_recordSampleFlag = 1;
	}
	
	// keep measuring temperature regardless of whether we're recording
	_measureTempFlag = 1;
}

// store current values into next available EEPROM location
void RecordSample(void)
{	
	// flash the recording led, turn it off later
	output_high(RECORD_LED_PORT, RECORD_LED_PIN);
	TIMSK0 |= (1 << OCIE0A);
	
	_recordSampleFlag = 0;
	
	// handle uploading data
	_secondsCounter++;
	if(_secondsCounter == UPLOAD_INTERVAL) {
		_secondsCounter = 0;
		UploadSample();
	}
	
	// only store when divisor counter = (2 ^ compactions) - 1	
	if(_divisorCounter < pow(2, _compactionCount) - 1)
	{
		_divisorCounter++;
		return;
	}
	_divisorCounter = 0;
	
	// write new sample to eeprom
	Sample sample = {_temperature, _recordBubbleCount};
	LC32_WriteSample(_nextMemoryPosition, sample);
	_nextMemoryPosition += 4;
	
	// reset the bubble counter
	_recordBubbleCount = 0;
	
	if(_nextMemoryPosition == 4096)
		CompactMemory();
}

void UploadSample(void) {
	// convert raw temp reading to Celsius string
	double temperatureCelsius = (double)_temperature * 0.0625;
	char temperatureString[16];
    dtostrf(temperatureCelsius, 1, 4, temperatureString);
	uint8_t temperatureStringLength = strlen(temperatureString);
	
	
	int8_t totalMessageLength = temperatureStringLength + 52;
	char totalMessageLengthString[12];
	itoa(totalMessageLength, totalMessageLengthString, 10);

	uart_puts("AT+RST\n");
	_delay_ms(1000); // wait for OK
	uart_puts("AT+CIPMUX=0\n");
	_delay_ms(200); // wait for OK
	uart_puts("AT+CIPSTART=\"TCP\",\"184.106.153.149\",80\n");
	_delay_ms(1000); // wait for OK Linked
	uart_puts("AT+CIPSEND=");
	uart_puts(totalMessageLengthString);
	uart_puts("\n");
	_delay_ms(200); // wait for >
	uart_puts("GET /update?key=");
	uart_puts(THINGSPEAK_KEY);
	uart_puts("&field1=");
	uart_puts(temperatureString);
	uart_puts("&field2=4\n");

	//_delay_ms(10000); // wait for OK Unlink	
}

// send current values over usb
void SendSample(void)
{
	// send data to usb instead of EEPROM
	_sendSampleFlag = 0;
	_usbArg0 = DATA_SAMPLE;
	_usbArg1 = _temperature / 256;
	_usbArg2 = _temperature % 256;
	_usbArg3 = _usbBubbleCount / 256;
	_usbArg4 = _usbBubbleCount % 256;
	_usbArg5 = _bubbleOccurring;
	_usbArg6 = _adcValue;
	_dataReady = 1; // send data back to usb host
	
	// reset the bubble counter
	_usbBubbleCount = 0;
	return;
}

// take every pair of adjacent samples, write them into the first half of the memory, clear the second half of memory
void CompactMemory(void)
{	
	// next recording posision will be half way through memory contents
	_nextMemoryPosition = 2048;
	
	// update the memory compacts counter	
	_compactionCount++;	
	eeprom_update_byte((uint8_t*)COMPACTION_COUNT, _compactionCount);
	
	// move the data around. (write 64 pages with 8 samples each)
	// by writing a whole page at a time we improve performance 
	// and avoid unnecessary EEPROM wear
	for(uint8_t pageIndex = 0; pageIndex < 64; pageIndex++)
	{
		// 8 samples (32 bytes) will go into output page
		Sample outputSamples[8];
		for (uint8_t sampleIndex = 0; sampleIndex < 8; sampleIndex++)
		{
			// read 2 samples (8 bytes)
			Sample sample1 = LC32_ReadSample((64 * pageIndex) + (8 * sampleIndex));
			Sample sample2 = LC32_ReadSample((64 * pageIndex) + (8 * sampleIndex) + 4);
			
			if((sample1.temperature == 0x0000 && sample1.bubbles == 0xFFFF) || (sample2.temperature == 0x0000 && sample2.bubbles == 0xFFFF))
			{
				// keep stop/start marker
				outputSamples[sampleIndex].temperature = 0x0000;
				outputSamples[sampleIndex].bubbles = 0xFFFF;
			}
			else
			{
				// compact into one sample (first temp, sum bubbles)
				// we don't average the temp since it's not so easy with raw DS18B20 data
				outputSamples[sampleIndex].temperature = sample1.temperature;
				outputSamples[sampleIndex].bubbles = sample1.bubbles + sample2.bubbles;
			}
		}
		
		// write whole page (32 bytes) of output data
		LC32_WritePage((32 * pageIndex), outputSamples);
	}
	
	// clear the second half of the memory space now that it is no longer needed
	Sample blankSamples[8];
	for(uint8_t i = 0; i < 8; i++)
	{
		blankSamples[i].temperature = 0xFFFF;
		blankSamples[i].bubbles = 0xFFFF;
	}
	for(uint8_t i = 64; i < 128; i++)
	{
		LC32_WritePage((32 * i), blankSamples);
	}
}

// This interrupt occurs 25 times per second 15625
ISR(TIMER1_COMPB_vect)
{
	OCR1B += 138; // 138 = 25 times per second 276 = 12.5 times
	_adcValue = ADCH; // store ADC value
	ADCSRA |= (1 << ADSC); // initiate next ADC
	
	// Bubble Logic - A bubble has occurred when:
	// 1. bubble delay has passed since last bubble occurrence
	// 2. ADC sample below minThreshold has been detected, then
	// 3. ADC sample above maxThreshold has been detected, then
	// 4. ADC sample below minThreshold has been detected again
	if(_readyForBubble)
	{
		// wait for first low reading after the delay has elapsed
		if(_bubbleLow1Detected == 0 && _adcValue < _adcLowThreshold)
		{
			_bubbleLow1Detected = 1;
		}
		
		// wait for first high reading after first low reading
		if(_bubbleLow1Detected && _bubbleHighDetected == 0 && _adcValue > _adcHighThreshold)
		{
			_bubbleHighDetected = 1;
		}
		
		// wait for next low reading after the high reading
		if(_bubbleLow1Detected && _bubbleHighDetected && _bubbleLow2Detected == 0 && _adcValue < _adcLowThreshold)
		{
			_bubbleLow2Detected = 1;
		}
		
		// check if bubble occurred
		if(_bubbleLow1Detected && _bubbleHighDetected && _bubbleLow2Detected)
		{
			_recordBubbleCount++; // increment the bubble count
			_usbBubbleCount++; // increment the bubble count
			_bubbleLow1Detected = 0;
			_bubbleHighDetected = 0;
			_bubbleLow2Detected = 0;
			_readyForBubble = 0;
			
			// turn on bubble LED and update bubble status flag
			output_high(BUBBLE_LED_PORT, BUBBLE_LED_PIN);
			_bubbleOccurring = 1;  // 1 means bubble is occurring
			
			// enable oc1c interrupt which will re-enable bubble sensing later
			// set up the timer compare register
			OCR1C = TCNT1 + _bubbleDelay;
			// clear the interrupt that would send us straight to the TIMER1_COMPC_vect
			TIFR1 |= (1 << OCF1C);
			// enable the interrupt
			TIMSK1 |= (1 << OCIE1C);
		}
	}
	if(_fastStreaming)
	{
		_sendSampleFlag = 1;
	}
}

// This interrupt occurs "bubbledelay" after a bubble
ISR(TIMER1_COMPC_vect)
{	
	// turn off bubble LED
	output_low(BUBBLE_LED_PORT, BUBBLE_LED_PIN);
	
	// 0 means bubble is finished
	_bubbleOccurring = 0;
	
	// allow sensing of next bubble
	_readyForBubble = 1;

	// disable oc1c interrupt
	TIMSK1 &= ~(1 << OCIE1C);
}

// Event handler for the library USB Connection event.
void EVENT_USB_Device_Connect(void)
{
}

// Event handler for the library USB Disconnection event.
void EVENT_USB_Device_Disconnect(void)
{
	_fastStreaming = 0;
}

// Event handler for the library USB Configuration Changed event.
void EVENT_USB_Device_ConfigurationChanged(void)
{
	bool ConfigSuccess = true;
	ConfigSuccess &= HID_Device_ConfigureEndpoints(&Generic_HID_Interface);
	USB_Device_EnableSOFEvents();
}

// Event handler for the library USB Control Request reception event.
void EVENT_USB_Device_ControlRequest(void)
{
	HID_Device_ProcessControlRequest(&Generic_HID_Interface);
}

// Event handler for the USB device Start Of Frame event.
void EVENT_USB_Device_StartOfFrame(void)
{
	HID_Device_MillisecondElapsed(&Generic_HID_Interface);
}

// if _dataReady is set, sends _usbArg* values and clears _dataReady flag
bool CALLBACK_HID_Device_CreateHIDReport(USB_ClassInfo_HID_Device_t* const HIDInterfaceInfo, uint8_t* const ReportID, const uint8_t ReportType, void* ReportData, uint16_t* const ReportSize)
{
	uint8_t* Data = (uint8_t*)ReportData;
	if(_dataReady)
	{
		*ReportSize = GENERIC_REPORT_SIZE;
		Data[0] = _usbArg0;
		Data[1] = _usbArg1;
		Data[2] = _usbArg2;
		Data[3] = _usbArg3;
		Data[4] = _usbArg4;
		Data[5] = _usbArg5;
		Data[6] = _usbArg6;
		Data[7] = _usbArg7;
		_dataReady = 0;
		return true;
	}
	return false;
}

// Stores _arg* values and starts appropriate action
void CALLBACK_HID_Device_ProcessHIDReport(USB_ClassInfo_HID_Device_t* const HIDInterfaceInfo, const uint8_t ReportID, const uint8_t ReportType, const void* ReportData, const uint16_t ReportSize)
{
	// store report data
	uint8_t* Data = (uint8_t*)ReportData;
	_usbArg0 = Data[0];
	_usbArg1 = Data[1];
	_usbArg2 = Data[2];
	_usbArg3 = Data[3];
	_usbArg4 = Data[4];
	_usbArg5 = Data[5];
	_usbArg6 = Data[6];
	_usbArg7 = Data[7];
	
	// process instruction
	switch(_usbArg0)
	{
		case START_FAST_STREAMING :
		{
			_fastStreaming = 1;
			_dataReady = 1; // echo back the command
			break;
		}
		case STOP_FAST_STREAMING :
		{
			_fastStreaming = 0;
			_dataReady = 1; // echo back the command
			break;
		}
		case CLEAR_MEMORY :
		{
			_clearMemoryFlag = 1;
			_dataReady = 1; // echo back the command
			break;
		}
		case READ_MEMORY :
		{
			// flash the record led, turn it off later
			output_high(RECORD_LED_PORT, RECORD_LED_PIN);
			TIMSK0 |= (1 << OCIE0A);
			uint16_t addressWord = (_usbArg1 * 256) + _usbArg2;
			Sample sample1 = LC32_ReadSample(addressWord);
			_usbArg3 = sample1.temperature / 256;
			_usbArg4 = sample1.temperature % 256;
			_usbArg5 = sample1.bubbles / 256;
			_usbArg6 = sample1.bubbles % 256;
			_dataReady = 1; // send back response
			break;
		}
		case SET_CONFIG :
		{
			uint8_t previouslyStreaming = _fastStreaming;
			_fastStreaming = 0; // stop fast streaming
			eeprom_update_byte((uint8_t*)ADC_HIGH_VALUE, _usbArg1);
			_adcHighThreshold = _usbArg1;
			eeprom_update_byte((uint8_t*)ADC_LOW_VALUE, _usbArg2);
			_adcLowThreshold = _usbArg2;
			eeprom_update_byte((uint8_t*)BUBBLE_DELAY_HIGH, _usbArg3);
			_bubbleDelay = _usbArg3;
			_bubbleDelay *= 256;
			eeprom_update_byte((uint8_t*)BUBBLE_DELAY_LOW, _usbArg4);
			_bubbleDelay += _usbArg4;
			_dataReady = 1; // send back response
			_fastStreaming = previouslyStreaming; // resume fast streaming if we were doing that
			break;
		}
		case GET_CONFIG :
		{
			_fastStreaming = 0; // stop fast streaming
			_usbArg1 = _adcHighThreshold;
			_usbArg2 = _adcLowThreshold;
			_usbArg3 = (uint8_t)(_bubbleDelay / 256);
			_usbArg4 = (uint8_t)(_bubbleDelay % 256);
			_usbArg5 = _compactionCount;
			_usbArg6 = (uint8_t)(_nextMemoryPosition / 256);
			_usbArg7 = (uint8_t)(_nextMemoryPosition % 256);
			_dataReady = 1; // send back response
			break;
		}
		case GET_VERSION :
		{
			_usbArg1 = VERSION_MAJOR;
			_usbArg2 = VERSION_MINOR;
			_usbArg3 = 99;
			_usbArg4 = 99;
			_usbArg5 = 99;
			_usbArg6 = 99;
			_usbArg7 = 99;
			_dataReady = 1; // send back response
			break;
		}
		case DEBUG_VARIABLES :
		{
			_usbArg1 = _compactionCount;
			_usbArg2 = _divisorCounter / 256;
			_usbArg3 = _divisorCounter % 256;
			_usbArg4 = _nextMemoryPosition / 256;
			_usbArg5 = _nextMemoryPosition % 256;
			_usbArg6 = 99;
			_usbArg7 = 99;
			_dataReady = 1; // send back response
			break;
		}
	}
}
