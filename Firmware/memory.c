/*
* memory.c
*
* Created: 31/05/2014 2:49:04 PM
*  Author: Michael
*/

#include "memory.h"
#include "i2cmaster.h"

void LC32_Init(void)
{
	i2c_init();
}

void LC32_Clear(void)
{	
	// break up the address into bytes
	for (uint16_t i = 0; i < 128; i++)
	{		
		// set device address and write mode
		i2c_start_wait(LC32_ADDRESS + I2C_WRITE);
			
		// calculate address bytes
		uint8_t addressh = (uint8_t)((i * 32) / 256);
		uint8_t addressl = (uint8_t)((i * 32) % 256);
		
		// send address
		i2c_write(addressh);
		i2c_write(addressl);
		
		// send data
		for(uint8_t j = 0; j < 8; j++)
		{
			i2c_write(0xFF);//th
			i2c_write(0xFF);//tl
			i2c_write(0xFF);//bh
			i2c_write(0xFF);//bl
		}
		
		// set stop condition = release bus
		i2c_stop();
	}
}

void LC32_WriteSample(uint16_t address, Sample sample)
{
	// set device address and write mode
	i2c_start_wait(LC32_ADDRESS + I2C_WRITE);
	
	// break up the address into bytes
	uint8_t addressh = address / 256;
	uint8_t addressl = address % 256;
	
	// send address
	i2c_write(addressh);
	i2c_write(addressl);
	
	// send data
	i2c_write(sample.temperature / 256);
	i2c_write(sample.temperature % 256);
	i2c_write(sample.bubbles / 256);
	i2c_write(sample.bubbles % 256);
	
	// set stop condition = release bus
	i2c_stop();
}

uint8_t LC32_ReadByte(uint16_t address)
{
	// set device address and write mode
	i2c_start(LC32_ADDRESS + I2C_WRITE);
	
	// break up the address into bytes
	uint8_t addressh = address / 256;
	uint8_t addressl = address % 256;
	
	// send address
	i2c_write(addressh);
	i2c_write(addressl);
	
	// read byte
	i2c_rep_start(LC32_ADDRESS + I2C_READ);
	uint8_t dataResponse = i2c_readNak(); // read one byte
	i2c_stop(); // set stop condition = release bus

	return dataResponse;
}

Sample LC32_ReadSample(uint16_t address)
{
	// set device address and write mode
	i2c_start_wait(LC32_ADDRESS + I2C_WRITE);
		
	// break up the address into bytes
	uint8_t addressh = address / 256;
	uint8_t addressl = address % 256;
	
	// send address
	i2c_write(addressh);
	i2c_write(addressl);
		
	// initiate read
	i2c_rep_start(LC32_ADDRESS + I2C_READ);	
	
	// read data
	Sample sample;
	sample.temperature = i2c_readAck() * 256;
	sample.temperature += i2c_readAck();
	sample.bubbles = i2c_readAck() * 256;
	sample.bubbles += i2c_readNak();
	
	// set stop condition to release bus
	i2c_stop(); 
	
	return sample;
}

void LC32_WritePage(uint16_t address, Sample samples[])
{
	// set device address and write mode
	i2c_start_wait(LC32_ADDRESS + I2C_WRITE);
		
	// break up the address into bytes
	uint8_t addressh = address / 256;
	uint8_t addressl = address % 256;
	
	// send address
	i2c_write(addressh);
	i2c_write(addressl);
		
	// send data
	for(int i = 0; i < 8; i++)
	{
		i2c_write(samples[i].temperature / 256);
		i2c_write(samples[i].temperature % 256);
		i2c_write(samples[i].bubbles / 256);
		i2c_write(samples[i].bubbles % 256);
	}
		
	// set stop condition to release bus
	i2c_stop();
}