using System;

namespace BrewMonitor.Models
{
    public class ConnectionChangedEventArgs : EventArgs
    {
        public bool Connected { get; set; }
    }
}
