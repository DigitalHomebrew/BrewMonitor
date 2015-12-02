/*
 * powercontrol.h
 *
 * Created: 27/11/2015 2:25:38 PM
 *  Author: Michael
 */ 


#ifndef POWERSWITCH_H_
#define POWERSWITCH_H_

#include <stdint.h>

void PowerSwitch_Setup(void);
void PowerSwitch_ConnectHeater(void);
void PowerSwitch_ConnectCooler(void);
void PowerSwitch_HeaterOn(void);
void PowerSwitch_HeaterOff(void);
void PowerSwitch_CoolerOn(void);
void PowerSwitch_CoolerOff(void);
void StartSequence(void);
void SendByte(uint8_t byte);

#endif /* POWERCONTROL_H_ */