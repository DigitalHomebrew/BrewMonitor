using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace BrewMonitor.Models
{
    public class Configuration
    {
        /// <summary>
        /// [0-255] The airlock sensor much show a value lower than this to register a bubble
        /// </summary>
        public int LowThreshold { get; set; }
        /// <summary>
        /// [0-255] The airlock sensor much show a value higher than this to register a bubble
        /// </summary>
        public int HighThreshold { get; set; }
        /// <summary>
        /// This is related to the number of times the memory has filled up and been compacted
        /// SampleRateDivisor = NumberOfCompactions + 1
        /// </summary>
        public int SampleRateDivisor { get; set; }
        /// <summary>
        /// This is the number of samples in the brewmonitor including "New Recording" flags which count as 1 sample
        /// This is useful for scaling the progressbar when downloading the memory contents.
        /// </summary>
        public int NumberOfSamples { get; set; }
        /// <summary>
        /// This is the most significant half of the delay word which is in
        /// units of about 1/8th of a millisecond (8MHz CPU / 1024 prescaler)
        /// </summary>
        public byte DelayHighByte { get; private set; }
        /// <summary>
        /// This is the least significant half of the delay word which is in
        /// units of about 1/8th of a millisecond (8MHz CPU / 1024 prescaler)
        /// </summary>
        public byte DelayLowByte { get; private set; }

        public Configuration()
        {
        }

        public Configuration(IList<byte> dataBytes)
        {
            HighThreshold = dataBytes[1];
            LowThreshold = dataBytes[2];
            DelayHighByte = dataBytes[3];
            DelayLowByte = dataBytes[4];
            SampleRateDivisor = dataBytes[5] + 1;
            NumberOfSamples = ((dataBytes[6] * 256) + dataBytes[7]) / 4;
        }

        public int BubblesPerMin
        {
            // TIMER1 is divided by 1024. F_CPU is 16MHz.
            get
            {
                double countsPerBubble = (DelayHighByte * 256) + DelayLowByte;
                var clocksPerBubble = countsPerBubble * 1024;
                var millisecondsPerBubble = clocksPerBubble / 16000;
                var bubblesPerMilliSecond = 1 / millisecondsPerBubble;
                var bubblesPerSecond = bubblesPerMilliSecond*1000;
                var bubblesPerMinute = bubblesPerSecond*60;
                return Convert.ToInt32(bubblesPerMinute);
            }
            set
            {
                var bubblesPerSec = (double)value / 60;
                var bubbesPerMillisecond = bubblesPerSec/1000;
                var millisecondsPerBubble = 1 / bubbesPerMillisecond;
                var clocksPerBubble = millisecondsPerBubble * 16000;
                var countsPerBubble = clocksPerBubble / 1024;
                var rawDelay = Convert.ToInt32(countsPerBubble);
                DelayHighByte = Convert.ToByte(rawDelay / 256);
                DelayLowByte = Convert.ToByte(rawDelay % 256);
            }
        }
    }
}
