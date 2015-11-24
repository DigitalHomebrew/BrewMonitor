using System;
using System.Collections.Generic;

namespace BrewMonitor.Models
{
    public class MemorySample
    {

        public byte RawAdc { get; private set; }
        public bool Bubbling { get; private set; }
        public int Divisor { get; set; }

        private readonly byte _temperatureHighByte;
        private readonly byte _temperatureLowByte;
        private readonly byte _bubblesHighByte;
        private readonly byte _bubblesLowByte;

        /// <summary>
        /// Build object from USB data array. 
        /// The arrays have different order depending on whether memory was read 
        /// or it was sent streaming hence the "streaming" parameter
        /// </summary>
        /// <param name="dataBytes">HID report from BrewMonitor</param>
        /// <param name="fromMemory">true if array is from a ReadMemoryAddress command, false if it's from streamed data</param>
        public MemorySample(IList<byte> dataBytes, bool fromMemory)
        {
            if (fromMemory)
            {
                _temperatureHighByte = dataBytes[3];
                _temperatureLowByte = dataBytes[4];
                _bubblesHighByte = dataBytes[5];
                _bubblesLowByte = dataBytes[6];
            }
            else
            {
                // data sample arrived
                _temperatureHighByte = dataBytes[1];
                _temperatureLowByte = dataBytes[2];
                _bubblesHighByte = dataBytes[3];
                _bubblesLowByte = dataBytes[4];
                Bubbling = dataBytes[5] > 0;
                RawAdc = dataBytes[6];
            }
        }


        /// <summary>
        /// Simple constructor for easy mocking
        /// </summary>
        /// <param name="temperature"></param>
        /// <param name="bubbles"></param>
        public MemorySample(double temperature, int bubbles)
        {
            _temperatureHighByte = (byte)(temperature / 256);
            _temperatureLowByte = (byte)(temperature % 256);
            _bubblesHighByte = (byte)(bubbles / 256);
            _bubblesLowByte = (byte)(bubbles % 256);
        }

        public int SampleNumber { get; set; }

        public double Celsius
        {
            get
            {
                //todo: make this cater for negative numbers
                var msb = (_temperatureHighByte % 8);
                var tempMagnitude = (msb * 256) + _temperatureLowByte;
                var celsius = (double)tempMagnitude / 16;
                return celsius;
            }
        }

        public int BubbleCount
        {
            get { return (_bubblesHighByte * 256) + _bubblesLowByte; }
        }

        public int Seconds
        {
            get
            {
                var seconds = SampleNumber * Math.Pow(2, Divisor - 1);
                return Convert.ToInt32(seconds);
            }
        }

        public bool IsStartStopMarker
        {
            get
            {
                return (_temperatureHighByte == 0 && _temperatureLowByte == 0 && _bubblesHighByte == 255 && _bubblesLowByte == 255);
            }
        }

        public bool IsBlank
        {
            get
            {
                return (_temperatureHighByte == 255 && _temperatureLowByte == 255 && _bubblesHighByte == 255 && _bubblesLowByte == 255);
            }
        }
    }
}
