#ifndef _GENERICHID_H_
#define _GENERICHID_H_

// Includes:
#include <avr/io.h>
#include <avr/wdt.h>
#include <avr/power.h>
#include <avr/interrupt.h>
#include <string.h>
#include "Descriptors.h"
#include "Config/AppConfig.h"
#include <LUFA/Drivers/USB/USB.h>

// Function Prototypes:
void SetupHardware(void);
void SetupTimerInterrupts(void);
void LoadSettings(void);
void StartRecording(uint8_t newRecording);
void StopRecording(void);

uint16_t GetNextEmptyAddress(void);
uint8_t Search_sensors(void);
void RecordSample(void);
void SendSample(void);
void CompactMemory(void);
void ClearExternalMemory(void);
void WaitForButtonRelease(void);
void UploadSample(void);

void EVENT_USB_Device_Connect(void);
void EVENT_USB_Device_Disconnect(void);
void EVENT_USB_Device_ConfigurationChanged(void);
void EVENT_USB_Device_ControlRequest(void);
void EVENT_USB_Device_StartOfFrame(void);
bool CALLBACK_HID_Device_CreateHIDReport(USB_ClassInfo_HID_Device_t* const HIDInterfaceInfo, uint8_t* const ReportID, const uint8_t ReportType, void* ReportData, uint16_t* const ReportSize);
void CALLBACK_HID_Device_ProcessHIDReport(USB_ClassInfo_HID_Device_t* const HIDInterfaceInfo, const uint8_t ReportID, const uint8_t ReportType, const void* ReportData, const uint16_t ReportSize);

// software version
#define VERSION_MAJOR			0x01
#define VERSION_MINOR			0x04

// define memory positions
#define RECORDING_FLAG			0x00
#define COMPACTION_COUNT		0x06
#define ADC_LOW_VALUE			0x08
#define ADC_HIGH_VALUE			0x0a
#define BUBBLE_DELAY_LOW		0x0c
#define BUBBLE_DELAY_HIGH		0x0e

// configuration settings
#define UPLOAD_INTERVAL	30 //seconds

// define usb functions
// the host sends a function in Arg0 and optional arguments in remaining fields
// the brew monitor echoes the message back with any response data in the
// additional fields marked with a "*" prefix
//		Function				Arg0	Arg1			Arg2			Arg3			Arg4			Arg5			Arg6
//--------------------------------------------------------------------------------------------------------------------------------
#define READ_MEMORY				0x01	//addressH		addressL		*Th1			*Tl1			*Bh1			*Bl1
#define CLEAR_MEMORY			0x02	//n/a			n/a				n/a				n/a				n/a				n/a
#define GET_CONFIG				0x03	//*adcMax		*adcMin			*delayH			*delayL			*recording		*compactions
#define SET_CONFIG				0x04	//adcMax		adcMin			delayH			delayL			n/a				n/a
#define START_FAST_STREAMING	0x05	//n/a			n/a				n/a				n/a				n/a				n/a
#define STOP_FAST_STREAMING		0x06	//n/a			n/a				n/a				n/a				n/a				n/a
#define DATA_SAMPLE				0x07	//*Th			*Tl				*Bh				*Bl				*bubbling		*ADC
#define GET_VERSION				0x08	//Major			Minor			99				99				99				99
#define DEBUG_VARIABLES			0x09

// Some macros that make the code more readable
#define output_toggle(port,pin) port ^= (1<<pin)
#define output_low(port,pin) port &= ~(1<<pin)
#define output_high(port,pin) port |= (1<<pin)
#define set_input(portdir,pin) portdir &= ~(1<<pin)
#define set_output(portdir,pin) portdir |= (1<<pin)
#define bit_get(p,m) ((p) & (1 << m))
//#define bit_set(p,m) ((p) |= (m))

#define MAXSENSORS 2  // allow up to 2 temperature sensors
#endif

