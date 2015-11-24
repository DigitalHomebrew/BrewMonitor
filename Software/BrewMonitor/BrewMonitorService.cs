using System;
using System.Linq;
using System.Threading;
using BrewMonitor.Models;
using HidLibrary;

namespace BrewMonitor
{
    public class BrewMonitorService : IBrewMonitorService
    {
        private const int BrewmonitorVid = 0x16D0;
        private const int BrewmonitorPid = 0x07AA;
        private readonly Timer _connectionListener;
        private HidDevice _hidDevice;
        private bool _streaming;

        // these allow function calls to wait for USB response to come back
        private readonly AutoResetEvent _readMemoryConpletedEvent;
        private readonly AutoResetEvent _eraseMemoryCompletedEvent;
        private readonly AutoResetEvent _readConfigurationCompletedEvent;
        private readonly AutoResetEvent _writeConfigurationCompletedEvent;
        private readonly AutoResetEvent _readVersionCompletedEvent;
        private readonly AutoResetEvent _getDebugVariablesCompletedEvent;
        private readonly AutoResetEvent _startStreamingCompletedEvent;
        private readonly AutoResetEvent _stopStreamingCompletedEvent;

        // when data arrives asynchronously is is stored in these variables
        // then the waiting thread is then signalled to make use of them
        private MemorySample _memorySample;
        private Configuration _configuration;
        private string _versionNumber;
        private byte[] _debugVariables;

        // currently supported commands
        private enum BrewMonitorCommand
        {
            ReadSample = 0x01,
            EraseMemory = 0x02,
            ReadConfig = 0x03,
            WriteConfig = 0x04,
            StartStreaming = 0x05,
            StopStreaming = 0x06,
            DataSampleArrived = 0x07,
            ReadFirmwareVersion = 0x08,
            GetDebugVariables = 0x09
        }

        // these are the only publically visible events
        public event EventHandler<ConnectionChangedEventArgs> ConnectionChanged;
        public event EventHandler<SampleReceivedEventArgs> SampleReceived;

        public BrewMonitorService()
        {
            _readMemoryConpletedEvent = new AutoResetEvent(false);
            _eraseMemoryCompletedEvent = new AutoResetEvent(false);
            _readConfigurationCompletedEvent = new AutoResetEvent(false);
            _writeConfigurationCompletedEvent = new AutoResetEvent(false);
            _readVersionCompletedEvent = new AutoResetEvent(false);
            _getDebugVariablesCompletedEvent = new AutoResetEvent(false);
            _startStreamingCompletedEvent = new AutoResetEvent(false);
            _stopStreamingCompletedEvent = new AutoResetEvent(false);
            _connectionListener = new Timer(ConnectionTimerCallback, null, 500, Timeout.Infinite);
        }

        /// <summary>
        /// Used to keep monitoring BrewMonitos's connection state
        /// </summary>
        /// <param name="state"></param>
        private void ConnectionTimerCallback(object state)
        {
            _hidDevice = HidDevices.Enumerate(BrewmonitorVid, BrewmonitorPid).FirstOrDefault();
            if (_hidDevice == null)
            {
                _connectionListener.Change(500, Timeout.Infinite);
                return;
            }

            _hidDevice.OpenDevice();
            //if (ConnectionChanged != null)
            _hidDevice.Inserted += () =>
            {
                if (ConnectionChanged != null)
                    ConnectionChanged(this, new ConnectionChangedEventArgs { Connected = true });
            }; // love the anonymous function
            _hidDevice.Removed += () =>
            {
                if (ConnectionChanged != null)
                    ConnectionChanged(this, new ConnectionChangedEventArgs { Connected = false });
            };
            _hidDevice.MonitorDeviceEvents = true;
            _hidDevice.ReadReport(ReadReportCallback);
        }

        /// <summary>
        /// Main incoming USB data handling routine -
        /// interprets command and takes appropriate action
        /// </summary>
        /// <param name="report"></param>
        private void ReadReportCallback(HidReport report)
        {
            try
            {
                if (report == null)
                    throw new Exception("HidReport was null, cannot process usb data.");
                var command = (BrewMonitorCommand)report.Data[0];
                switch (command)
                {
                    case BrewMonitorCommand.ReadSample:
                        {
                            _memorySample = new MemorySample(report.Data, true);
                            _readMemoryConpletedEvent.Set();
                            break;
                        }
                    case BrewMonitorCommand.EraseMemory:
                        {
                            _eraseMemoryCompletedEvent.Set();
                            break;
                        }
                    case BrewMonitorCommand.ReadConfig:
                        {
                            _configuration = new Configuration(report.Data);
                            _readConfigurationCompletedEvent.Set();
                            break;
                        }
                    case BrewMonitorCommand.WriteConfig:
                        {
                            _writeConfigurationCompletedEvent.Set();
                            break;
                        }
                    case BrewMonitorCommand.StartStreaming:
                        {
                            _startStreamingCompletedEvent.Set();
                            break;
                        }
                    case BrewMonitorCommand.StopStreaming:
                        {
                            _stopStreamingCompletedEvent.Set();
                            break;
                        }
                    case BrewMonitorCommand.DataSampleArrived:
                        {
                            if (SampleReceived != null)
                                SampleReceived(this, new SampleReceivedEventArgs { Sample = new MemorySample(report.Data, false) });
                            break;
                        }
                    case BrewMonitorCommand.ReadFirmwareVersion:
                        {
                            _versionNumber = report.Data[1] + "." + report.Data[2];
                            _readVersionCompletedEvent.Set();
                            break;
                        }
                    case BrewMonitorCommand.GetDebugVariables:
                        {
                            _debugVariables = report.Data;
                            _getDebugVariablesCompletedEvent.Set();
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                //todo: dont hide this throw new Exception("Error processing USB data", ex);
            }
            finally
            {
                _hidDevice.ReadReport(ReadReportCallback);
            }
        }

        /// <summary>
        /// Reads a memory sample from the address provided
        /// Valid addresses are 0 to 1023.
        /// </summary>
        /// <param name="sampleNumber"></param>
        /// <returns></returns>
        public MemorySample ReadMemory(int sampleNumber)
        {
            try
            {
                // convert sample number to raw memory address bytes (multiply by 4 since a sample is 4 bytes)
                var addressH = (byte)(sampleNumber / 64);
                var addressL = (byte)(sampleNumber * 4 % 256);

                _memorySample = null;

                // build usb command into Hid report and send it away
                var outgoingReport = new HidReport(8, new HidDeviceData(new byte[] { 0x00, (byte)BrewMonitorCommand.ReadSample, addressH, addressL, 0x00, 0x00, 0x00, 0x00 }, HidDeviceData.ReadStatus.Success));
                _hidDevice.WriteReport(outgoingReport);

                // wait for response from brewmonitor
                _readMemoryConpletedEvent.WaitOne(new TimeSpan(0, 0, 0, 0, 500));

                return _memorySample;
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading memory", ex);
            }
        }

        /// <summary>
        /// Read the configuration and current status of the brewmonitor
        /// </summary>
        /// <returns></returns>
        public Configuration ReadConfiguration()
        {
            try
            {
                // build usb command into Hid report and send it away
                var outgoingReport = new HidReport(8, new HidDeviceData(new byte[] { 0x00, (byte)BrewMonitorCommand.ReadConfig, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, HidDeviceData.ReadStatus.Success));
                _hidDevice.WriteReport(outgoingReport);

                // get the response back from brewmonitor
                _readConfigurationCompletedEvent.WaitOne(500);

                //Thread.Sleep(2000); // for debugging async stuff todo: remove this
                return _configuration;
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading configuration", ex);
            }
        }

        public byte[] GetDebugVariables()
        {
            try
            {
                // build usb command into Hid report and send it away
                var outgoingReport = new HidReport(8, new HidDeviceData(new byte[] { 0x00, (byte)BrewMonitorCommand.GetDebugVariables, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, HidDeviceData.ReadStatus.Success));
                _hidDevice.WriteReport(outgoingReport);

                // get the response back from brewmonitor
                _getDebugVariablesCompletedEvent.WaitOne(500);
                return _debugVariables;
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting debug variables", ex);
            }
        }

        /// <summary>
        /// Update the non-volatile configuration parameters of the brewmonitor
        /// </summary>
        /// <param name="config"></param>
        public void WriteConfiguration(Configuration config)
        {
            try
            {
                // build usb command into Hid report and send it away
                var outgoingReport = new HidReport(8, new HidDeviceData(new byte[] { 0x00, (byte)BrewMonitorCommand.WriteConfig, Convert.ToByte(config.HighThreshold), Convert.ToByte(config.LowThreshold), config.DelayHighByte, config.DelayLowByte, 0x00, 0x00 }, HidDeviceData.ReadStatus.Success));
                _hidDevice.WriteReport(outgoingReport);

                // get the response back from brewmonitor
                _writeConfigurationCompletedEvent.WaitOne(500);
            }
            catch (Exception ex)
            {
                throw new Exception("Error writing configuration", ex);
            }
        }

        /// <summary>
        /// Wipe the external memory (not the configuration)
        /// </summary>
        public void EraseMemory()
        {
            try
            {
                // build usb command into Hid report and send it away
                var outgoingReport = new HidReport(8, new HidDeviceData(new byte[] { 0x00, (byte)BrewMonitorCommand.EraseMemory, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, HidDeviceData.ReadStatus.Success));
                _hidDevice.WriteReport(outgoingReport);

                // get the response back from brewmonitor
                _eraseMemoryCompletedEvent.WaitOne(2500);
            }
            catch (Exception ex)
            {
                throw new Exception("Error erasing memory", ex);
            }
        }

        /// <summary>
        /// Tell brewmonitor to start streaming data (for the configuration page).
        /// This will fire the SampleReceived event every 50ms
        /// </summary>
        public void StartStreaming()
        {
            try
            {
                // build usb command into Hid report and send it away
                var outgoingReport = new HidReport(8, new HidDeviceData(new byte[] { 0x00, (byte)BrewMonitorCommand.StartStreaming, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, HidDeviceData.ReadStatus.Success));
                _hidDevice.WriteReport(outgoingReport);

                // get the response back from brewmonitor
                _startStreamingCompletedEvent.WaitOne(500);
                _streaming = true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error starting streaming", ex);
            }
        }

        /// <summary>
        /// Tell brewmonitor to stop streaming data.
        /// </summary>
        public void StopStreaming()
        {
            try
            {
                // build usb command into Hid report and send it away
                var outgoingReport = new HidReport(8, new HidDeviceData(new byte[] { 0x00, (byte)BrewMonitorCommand.StopStreaming, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, HidDeviceData.ReadStatus.Success));
                _hidDevice.WriteReport(outgoingReport);

                // get the response back from brewmonitor
                _stopStreamingCompletedEvent.WaitOne(500);
                _streaming = false;
            }
            catch (Exception ex)
            {
                throw new Exception("Error stopping streaming", ex);
            }
        }

        /// <summary>
        /// Read the firmware version from the brewmonitor.
        /// </summary>
        /// <returns></returns>
        public string GetVersion()
        {
            try
            {
                // build usb command into Hid report and send it away
                var outgoingReport = new HidReport(8, new HidDeviceData(new byte[] { 0x00, (byte)BrewMonitorCommand.ReadFirmwareVersion, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, HidDeviceData.ReadStatus.Success));
                _hidDevice.WriteReport(outgoingReport);

                // get the response back from brewmonitor
                _readVersionCompletedEvent.WaitOne(500);
                return _versionNumber;
            }
            catch (Exception ex)
            {
                throw new Exception("Error stopping streaming", ex);
            }
        }

        public bool IsConnected()
        {
            return _hidDevice != null && _hidDevice.IsConnected;
        }

        public bool IsStreaming()
        {
            return _streaming;
        }

        public void Dispose()
        {
            if (_streaming)
                StopStreaming();
            if (_hidDevice != null) { _hidDevice.Dispose(); }
            if (_connectionListener != null) { _connectionListener.Dispose(); }
            if (_readMemoryConpletedEvent != null) { _readMemoryConpletedEvent.Dispose(); }
            if (_eraseMemoryCompletedEvent != null) { _eraseMemoryCompletedEvent.Dispose(); }
            if (_readConfigurationCompletedEvent != null) { _readConfigurationCompletedEvent.Dispose(); }
            if (_writeConfigurationCompletedEvent != null) { _writeConfigurationCompletedEvent.Dispose(); }
            if (_readVersionCompletedEvent != null) { _readVersionCompletedEvent.Dispose(); }
            if (_startStreamingCompletedEvent != null) { _startStreamingCompletedEvent.Dispose(); }
            if (_stopStreamingCompletedEvent != null) { _stopStreamingCompletedEvent.Dispose(); }
        }
    }
}
