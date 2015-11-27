using System;
using System.Net;
using System.Timers;
using BrewMonitor;
using BrewMonitor.Models;
using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;
using PushoverClient;


namespace DashboardWPF.ViewModels.Control
{
    public sealed class ControlViewModel : Screen, IMainScreenTabItem
    {
        private readonly IBrewMonitorService _brewMonitorService;
        private readonly DialogCoordinator _dialogCoordinator;

        private readonly Timer _uploadTimer = new Timer();

        #region Properties

        private string _temperature;
        public string Temperature
        {
            get { return _temperature; }
            set
            {
                if (value.Equals(_temperature)) return;
                _temperature = value;
                NotifyOfPropertyChange(() => Temperature);
            }
        }

        private bool _bubbling;
        public bool Bubbling
        {
            get { return _bubbling; }
            set
            {
                if (value == _bubbling) return;
                _bubbling = value;
                NotifyOfPropertyChange(() => Bubbling);
            }
        }

        private int _bubbleCount;
        public int BubbleCount
        {
            get { return _bubbleCount; }
            set
            {
                if (value == _bubbleCount) return;
                _bubbleCount = value;
                NotifyOfPropertyChange(() => BubbleCount);
            }
        }

        private string _thingSpeakWriteKey;
        public string ThingSpeakWriteKey
        {
            get { return _thingSpeakWriteKey; }
            set
            {
                if (value == _thingSpeakWriteKey) return;
                _thingSpeakWriteKey = value;
                NotifyOfPropertyChange(() => ThingSpeakWriteKey);
            }
        }

        private string _thingSpeakChannelId;
        public string ThingSpeakChannelId
        {
            get { return _thingSpeakChannelId; }
            set
            {
                if (value == _thingSpeakChannelId) return;
                _thingSpeakChannelId = value;
                NotifyOfPropertyChange(() => ThingSpeakChannelId);
            }
        }

        private int _uploadCountdown;
        public int UploadCountDown
        {
            get { return _uploadCountdown; }
            set
            {
                if (value == _uploadCountdown) return;
                _uploadCountdown = value;
                NotifyOfPropertyChange(() => UploadCountDown);
            }
        }

        private bool _thingSpeakIsEnabled;
        public bool ThingSpeakIsEnabled
        {
            get { return _thingSpeakIsEnabled; }
            set
            {
                if (value == _thingSpeakIsEnabled) return;
                _thingSpeakIsEnabled = value;
                NotifyOfPropertyChange(() => ThingSpeakIsEnabled);
            }
        }

        private bool _pushoverIsEnabled;
        public bool PushoverIsEnabled
        {
            get { return _pushoverIsEnabled; }
            set
            {
                if (value == _pushoverIsEnabled) return;
                _pushoverIsEnabled = value;
                NotifyOfPropertyChange(() => PushoverIsEnabled);
                NotifyOfPropertyChange(() => CanTestPushover);
            }
        }

        public bool CanTestPushover
        {
            get { return PushoverIsEnabled; }
        }

        private string _pushoverUserKey;
        public string PushoverUserKey
        {
            get { return _pushoverUserKey; }
            set
            {
                if (value == _pushoverUserKey) return;
                _pushoverUserKey = value;
                NotifyOfPropertyChange(() => PushoverUserKey);
            }
        }

        private bool _temperatureBelowIsEnabled;
        public bool TemperatureBelowIsEnabled
        {
            get { return _temperatureBelowIsEnabled; }
            set
            {
                if (value == _temperatureBelowIsEnabled) return;
                _temperatureBelowIsEnabled = value;
                NotifyOfPropertyChange(() => TemperatureBelowIsEnabled);
            }
        }

        private int _temperatureBelowValue;
        public int TemperatureBelowValue
        {
            get { return _temperatureBelowValue; }
            set
            {
                if (value == _temperatureBelowValue) return;
                _temperatureBelowValue = value;
                NotifyOfPropertyChange(() => TemperatureBelowValue);
            }
        }

        private bool _temperatureAboveIsEnabled;
        public bool TemperatureAboveIsEnabled
        {
            get { return _temperatureAboveIsEnabled; }
            set
            {
                if (value == _temperatureAboveIsEnabled) return;
                _temperatureAboveIsEnabled = value;
                NotifyOfPropertyChange(() => TemperatureAboveIsEnabled);
            }
        }

        private int _temperatureAboveValue;
        public int TemperatureAboveValue
        {
            get { return _temperatureAboveValue; }
            set
            {
                if (value == _temperatureAboveValue) return;
                _temperatureAboveValue = value;
                NotifyOfPropertyChange(() => TemperatureAboveValue);
            }
        }

        private bool _bubbleAboveIsEnabled;
        public bool BubbleAboveIsEnabled
        {
            get { return _bubbleAboveIsEnabled; }
            set
            {
                if (value == _bubbleAboveIsEnabled) return;
                _bubbleAboveIsEnabled = value;
                NotifyOfPropertyChange(() => BubbleAboveIsEnabled);
            }
        }

        private int _bubbleAboveValue;
        public int BubbleAboveValue
        {
            get { return _bubbleAboveValue; }
            set
            {
                if (value == _bubbleAboveValue) return;
                _bubbleAboveValue = value;
                NotifyOfPropertyChange(() => BubbleAboveValue);
            }
        }

        private bool _bubbleBelowIsEnabled;
        public bool BubbleBelowIsEnabled
        {
            get { return _bubbleBelowIsEnabled; }
            set
            {
                if (value == _bubbleBelowIsEnabled) return;
                _bubbleBelowIsEnabled = value;
                NotifyOfPropertyChange(() => BubbleBelowIsEnabled);
            }
        }

        private int _bubbleBelowValue;
        public int BubbleBelowValue
        {
            get { return _bubbleBelowValue; }
            set
            {
                if (value == _bubbleBelowValue) return;
                _bubbleBelowValue = value;
                NotifyOfPropertyChange(() => BubbleBelowValue);
            }
        }

        private int _bubbleBelowAfterValue;
        public int BubbleBelowAfterValue
        {
            get { return _bubbleBelowAfterValue; }
            set
            {
                if (value == _bubbleBelowAfterValue) return;
                _bubbleBelowAfterValue = value;
                NotifyOfPropertyChange(() => BubbleBelowAfterValue);
            }
        }

        private int _bubbleMaxTracker;

        #endregion

        public ControlViewModel(IBrewMonitorService brewMonitorService, DialogCoordinator dialogCoordinator)
        {
            _brewMonitorService = brewMonitorService;
            _dialogCoordinator = dialogCoordinator;
            DisplayName = "control";
            _brewMonitorService.SampleReceived += OnSampleReceived;
            _brewMonitorService.ConnectionChanged += OnConnectionChanged;

            // setup the upload timer but don't start it yet
            _uploadTimer.AutoReset = true;
            _uploadTimer.Interval = 1000;
            _uploadTimer.Elapsed += UploadTimerCallback;
            UploadCountDown = 60;

            GetConfigSettings();
        }

        /// <summary>
        /// Gets the configuration settings.
        /// This is here because we want to load them both in the constructor and every
        /// other time the view is activated to puch up changes that were waves last
        /// time it was deactivated.
        /// </summary>
        private void GetConfigSettings()
        {
            ThingSpeakWriteKey = Properties.Settings.Default.ThingSpeakWriteKey;
            ThingSpeakChannelId = Properties.Settings.Default.ThingSpeakChannelId;
            ThingSpeakIsEnabled = Properties.Settings.Default.ThingSpeakEnabled;
            PushoverIsEnabled = Properties.Settings.Default.PushoverEnabled;
            PushoverUserKey = Properties.Settings.Default.PushoverUserKey;
            TemperatureBelowIsEnabled = Properties.Settings.Default.NotifyOnTemperatureBelowEnabled;
            TemperatureBelowValue = Properties.Settings.Default.NotifyOnTemperatureBelowValue;
            TemperatureAboveIsEnabled = Properties.Settings.Default.NotifyOnTemperatureAboveEnabled;
            TemperatureAboveValue = Properties.Settings.Default.NotifyOnTemperatureAboveValue;
            BubbleAboveIsEnabled = Properties.Settings.Default.NotifyOnBubbleAboveEnabled;
            BubbleAboveValue = Properties.Settings.Default.NotifyOnBubbleAboveValue;
            BubbleBelowIsEnabled = Properties.Settings.Default.NotifyOnBubbleBelowEnabled;
            BubbleBelowValue = Properties.Settings.Default.NotifyOnBubbleBelowValue;
            BubbleBelowAfterValue = Properties.Settings.Default.NotifyOnBubbleBelowAfterValue;
        }

        private void SaveConfigSettings()
        {
            Properties.Settings.Default.ThingSpeakWriteKey = ThingSpeakWriteKey;
            Properties.Settings.Default.ThingSpeakChannelId = ThingSpeakChannelId;
            Properties.Settings.Default.ThingSpeakEnabled = ThingSpeakIsEnabled;
            Properties.Settings.Default.PushoverEnabled = PushoverIsEnabled;
            Properties.Settings.Default.PushoverUserKey = PushoverUserKey;
            Properties.Settings.Default.NotifyOnTemperatureBelowEnabled = TemperatureBelowIsEnabled;
            Properties.Settings.Default.NotifyOnTemperatureBelowValue = TemperatureBelowValue;
            Properties.Settings.Default.NotifyOnTemperatureAboveEnabled = TemperatureAboveIsEnabled;
            Properties.Settings.Default.NotifyOnTemperatureAboveValue = TemperatureAboveValue;
            Properties.Settings.Default.NotifyOnBubbleAboveEnabled = BubbleAboveIsEnabled;
            Properties.Settings.Default.NotifyOnBubbleAboveValue = BubbleAboveValue;
            Properties.Settings.Default.NotifyOnBubbleBelowEnabled = BubbleBelowIsEnabled;
            Properties.Settings.Default.NotifyOnBubbleBelowValue = BubbleBelowValue;
            Properties.Settings.Default.NotifyOnBubbleBelowAfterValue = BubbleBelowAfterValue;
            Properties.Settings.Default.Save();
        }

        private void OnConnectionChanged(object sender, ConnectionChangedEventArgs e)
        {
            if (_brewMonitorService != null && _brewMonitorService.IsConnected())
            {
                _brewMonitorService.StartStreaming();
                _uploadTimer.Start();
            }
            else
            {
                _uploadTimer.Stop();
            }
        }

        private void OnSampleReceived(object o, SampleReceivedEventArgs args)
        {
            Bubbling = args.Sample.Bubbling;
            Temperature = args.Sample.Celsius.ToString("0.0000");
            BubbleCount += args.Sample.BubbleCount;
        }

        protected override void OnActivate()
        {
            GetConfigSettings();

            if (_brewMonitorService != null && _brewMonitorService.IsConnected())
            {
                _brewMonitorService.StartStreaming();
                _uploadTimer.Start();
            }
            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            SaveConfigSettings();


            // stop brewmonitor from streaming samples
            _brewMonitorService.StopStreaming();
            _uploadTimer.Stop();
            base.OnDeactivate(close);
        }

        private void UploadTimerCallback(object state, ElapsedEventArgs e)
        {
            UploadCountDown--;
            if (UploadCountDown <= 0)
            {
                UploadCountDown = 60;

                // handle thingspeak
                if (ThingSpeakIsEnabled)
                    SendDataToThingSpeak();

                // handle pushover
                if (PushoverIsEnabled)
                {
                    if (TemperatureBelowIsEnabled && Convert.ToDouble(Temperature) < TemperatureBelowValue)
                    {
                        SendPushoverNotification("Your wort is too cold", "it's below " + TemperatureBelowValue);
                        TemperatureBelowIsEnabled = false;
                    }
                    if (TemperatureAboveIsEnabled && Convert.ToDouble(Temperature) > TemperatureAboveValue)
                    {
                        SendPushoverNotification("Your wort is too hot", "it's above " + TemperatureAboveValue);
                        TemperatureAboveIsEnabled = false;
                    }
                    if (BubbleAboveIsEnabled && BubbleCount > BubbleAboveValue)
                    {
                        SendPushoverNotification("Bubble rate exceeded",
                            "Your bubble rate has exceeded " + BubbleAboveValue);
                        BubbleAboveIsEnabled = false;
                    }
                    if (!BubbleBelowIsEnabled) return;
                    if (BubbleCount > _bubbleMaxTracker)
                        _bubbleMaxTracker = BubbleCount;
                    if (BubbleCount < BubbleBelowValue && _bubbleMaxTracker > BubbleBelowAfterValue)
                    {
                        SendPushoverNotification("Bubble rate dropping",
                            "Below " + BubbleBelowValue + " after exceeding " + BubbleBelowAfterValue);
                        BubbleBelowIsEnabled = false;
                    }
                }

                BubbleCount = 0;
            }
        }

        public void TestPushover()
        {
            SendPushoverNotification("Pushover is working", "(baby!)");
        }

        void SendPushoverNotification(string title, string message)
        {
            var pclient = new Pushover("ab1oS822aYwUHDVj1RbRBeezt2ZHx3"); // aplication api token
            var response = pclient.Push(
              title,
              message,
              PushoverUserKey
          );
        }

        public void SendDataToThingSpeak()
        {
            try
            {
                var strUpdateUri = "http://api.thingspeak.com/update?key=" + _thingSpeakWriteKey;
                strUpdateUri += "&field1=" + Temperature;
                strUpdateUri += "&field2=" + BubbleCount;

                var thingsSpeakReq = (HttpWebRequest)WebRequest.Create(strUpdateUri);
                var thingsSpeakResp = (HttpWebResponse)thingsSpeakReq.GetResponse();

                if (string.Equals(thingsSpeakResp.StatusDescription, "OK")) { return; }

                var exData = new Exception(thingsSpeakResp.StatusDescription);
                throw exData;
            }
            catch (Exception ex)
            {
                _dialogCoordinator.ShowMessageAsync(this, "Thingspeak error", ex.Message);
            }
        }
    }
}
