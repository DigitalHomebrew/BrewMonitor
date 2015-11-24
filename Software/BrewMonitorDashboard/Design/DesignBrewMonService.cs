using System;
using System.Collections.Generic;
using System.Threading;
using BrewMonitor;
using BrewMonitor.Models;

namespace BrewMonitorDashboard.Design
{
    class DesignBrewMonService : IBrewMonitorService
    {
        private bool _streaming;

        public event EventHandler<ConnectionChangedEventArgs> ConnectionChanged;
        public event EventHandler<SampleReceivedEventArgs> SampleReceived;

        public MemorySample ReadMemory(int address)
        {
            var data = new List<MemorySample>
            {
                new MemorySample(20, 2),
                new MemorySample(18, 5),
                new MemorySample(19, 22),
                new MemorySample(18, 24),
                new MemorySample(20, 22),
                new MemorySample(22, 3),
            };

            return address > data.Count - 1 ? new MemorySample(21, 2) : data[address];
        }

        public Configuration ReadConfiguration() { throw new NotImplementedException(); }
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
