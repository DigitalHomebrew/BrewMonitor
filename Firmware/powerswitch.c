/*
 * PowerControl.c
 *
 * Created: 27/11/2015 2:25:19 PM
 *  Author: Michael
 */ 
#include "powerswitch.h"
#include "PCB13.h"
#include <util/delay.h>

#define HEATER_ON_BYTE_1 0b11100110
#define HEATER_ON_BYTE_2 0b01110000
#define HEATER_ON_BYTE_3 0b00010100
#define HEATER_OFF_BYTE_1 0b11100110
#define HEATER_OFF_BYTE_2 0b01110000
#define HEATER_OFF_BYTE_3 0b00000100
#define COOLER_ON_BYTE_1 0
#define COOLER_ON_BYTE_2 0
#define COOLER_ON_BYTE_3 0
#define COOLER_OFF_BYTE_1 0
#define COOLER_OFF_BYTE_2 0
#define COOLER_OFF_BYTE_3 0

void PowerSwitch_Setup(void) {
	// set the data pin as an output
	POWERSWITCH_DATA_DDR |= (1 << POWERSWITCH_DATA_PIN);
}

void PowerSwitch_ConnectHeater(void) {
	
}

void PowerSwitch_ConnectCooler(void) {
	
}

void PowerSwitch_HeaterOn(void) {
	// send sequence 6 times
	for(int i = 0; i < 6; i++) {
		StartSequence();
		SendByte(HEATER_ON_BYTE_1);
		SendByte(HEATER_ON_BYTE_2);
		SendByte(HEATER_ON_BYTE_3);
		_delay_ms(10);
	}
}

void PowerSwitch_HeaterOff(void) {
	// send sequence 6 times
	for(int i = 0; i < 6; i++) {
		StartSequence();
		SendByte(HEATER_OFF_BYTE_1);
		SendByte(HEATER_OFF_BYTE_2);
		SendByte(HEATER_OFF_BYTE_3);
		_delay_ms(10);
	}
}

void PowerControl_CoolerOn(void) {
	
}

void PowerControl_CoolerOff(void) {
	
}

void StartSequence() {
	// start sequence with a 0
	POWERSWITCH_DATA_PORT |= (1 << POWERSWITCH_DATA_PIN);
	_delay_us(300);
	POWERSWITCH_DATA_PORT &= ~(1 <<POWERSWITCH_DATA_PIN);
	_delay_us(900);
}

void SendByte(uint8_t byte) {	
	// send byte MSB first
	for(int i = 7; i > -1; i--) {
		if(byte & (1 << i)) {
			//send 1
			POWERSWITCH_DATA_PORT |= (1 << POWERSWITCH_DATA_PIN);
			_delay_us(900);
			POWERSWITCH_DATA_PORT &= ~(1 <<POWERSWITCH_DATA_PIN);
			_delay_us(300);
		}
		else {
			// send 0
			POWERSWITCH_DATA_PORT |= (1 << POWERSWITCH_DATA_PIN);
			_delay_us(300);
			POWERSWITCH_DATA_PORT &= ~(1 <<POWERSWITCH_DATA_PIN);
			_delay_us(900);
		}
	}
}