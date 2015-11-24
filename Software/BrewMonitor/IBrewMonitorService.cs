using System;
using BrewMonitor.Models;

namespace BrewMonitor
{
    public interface IBrewMonitorService : IDisposable
    {
        MemorySample ReadMemory(int sampleNumber);
        Configuration ReadConfiguration();
        void WriteConfiguration(Configuration config);
        void EraseMemory();
        void StartStreaming();
        void StopStreaming();
        event EventHandler<SampleReceivedEventArgs> SampleReceived;
        event EventHandler<ConnectionChangedEventArgs> ConnectionChanged;
        bool IsStreaming();
        bool IsConnected();
        byte[] GetDebugVariables();
    }
}
