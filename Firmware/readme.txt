Internal EEPROM Structure

ATMEGA32U4 has 1024Bytes of EEPROM. Some of this space is used to store settings in case of a power failure.

Address	Data
----------------------------------------
0x00	RecordingFlag
0x02	RecordAddressLowBit
0x04	RecordAddressHighBit
0x06	SampleRateDivisor
0x08	Bubble Median Value
0x0a	Bubble Hysteresis Value
0x0c	Bubble Delay Value	
.....
0x3F6	// last usable address

