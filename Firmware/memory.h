/*
 * memory.h
 *
 * Created: 31/05/2014 2:49:21 PM
 *  Author: Michael
 */ 


#ifndef MEMORY_H_
#define MEMORY_H_

#include <stdint.h>

#define LC32_ADDRESS  0xA0
#define output_toggle(port,pin) port ^= (1<<pin)
#define output_low(port,pin) port &= ~(1<<pin)
#define output_high(port,pin) port |= (1<<pin)

typedef struct {
	uint16_t temperature;
	uint16_t bubbles;
} Sample; // typedef

void LC32_Init(void);
uint8_t LC32_ReadByte(uint16_t address);
void LC32_WriteSample(uint16_t address, Sample sample);
void LC32_Clear(void);
Sample LC32_ReadSample(uint16_t address);
void LC32_WritePage(uint16_t address, Sample samples[]);

#endif /* MEMORY_H_ */