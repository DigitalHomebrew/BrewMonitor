using System;

namespace BrewMonitor.Models
{
    public class SampleReceivedEventArgs : EventArgs
    {
        public MemorySample Sample { get; set; }
    }    
}
