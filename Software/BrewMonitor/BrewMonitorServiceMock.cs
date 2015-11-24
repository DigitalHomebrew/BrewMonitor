using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using BrewMonitor.Models;

namespace BrewMonitor
{
    public class BrewMonitorServiceMock : IBrewMonitorService
    {
        private const int NumberOfSamples = 800;

        private bool _streaming;
        private readonly Random _random;

        public event EventHandler<ConnectionChangedEventArgs> ConnectionChanged;
        public event EventHandler<SampleReceivedEventArgs> SampleReceived;

        public BrewMonitorServiceMock()
        {
            _random = new Random(DateTime.Now.Millisecond);
        }

        public MemorySample ReadMemory(int address)
        {
            var temp = (Math.Sin((double)address / 50)*40) + (20 * 16);
            var sample = address < NumberOfSamples ? new MemorySample(temp, _random.Next(0, 128)) : new MemorySample(65535, 65535);
            Thread.Sleep(2);
            return sample;
        }

        public Configuration ReadConfiguration()
        {
            var config = new Configuration
            {
                SampleRateDivisor = 8,
                BubblesPerMin = 240,
                NumberOfSamples = NumberOfSamples
            };
            return config;
        }

        public void WriteConfiguration(Configuration config) { throw new NotImplementedException(); }

        public byte[] GetDebugVariables()
        {
            return new byte[]
            {
                0x00, 0x00, 0x00
            };
        }

        public bool Connect()
        {
            return true;
        }

        public void StartStreaming()
        {
            _streaming = true;
        }

        public void StopStreaming()
        {
            _streaming = false;
        }

        public bool IsConnected()
        {
            return true;
        }

        public bool IsStreaming()
        {
            return _streaming;
        }

        public void EraseMemory()
        {
            Thread.Sleep(2000);
        }

        public void Dispose()
        { }
    }
}
