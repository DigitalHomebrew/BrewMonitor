using System.Threading.Tasks;
using System.Timers;
using BrewMonitor;
using BrewMonitor.Models;
using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;
using Timer = System.Timers.Timer;

namespace DashboardWPF.ViewModels.Configure
{
    public sealed class ConfigureViewModel : Screen, IMainScreenTabItem
    {
        private readonly IBrewMonitorService _brewMonitorService;
        private readonly DialogCoordinator _dialogCoordinator;


        //private readonly Timer _timer = new Timer(ConfigurationTimerCallback, null, 500, Timeout.Infinite);
        private readonly Timer _timer = new Timer();

        private bool _showProgressRing;
        public bool ShowProgressRing
        {
            get { return _showProgressRing; }
            set
            {
                if (_showProgressRing.Equals(value)) return;
                _showProgressRing = value;
                if (value)
                {
                    ShowControls = false;
                }
                NotifyOfPropertyChange(() => ShowProgressRing);
            }
        }

        private bool _showControls;
        public bool ShowControls
        {
            get { return _showControls; }
            set
            {
                if (_showControls.Equals(value)) return;
                _showControls = value;
                if (value)
                {
                    ShowProgressRing = false;
                }
                NotifyOfPropertyChange(() => ShowControls);
            }
        }

        private int _adcValue;
        public int AdcValue
        {
            get { return _adcValue; }
            set
            {
                if (value == _adcValue) return;
                _adcValue = value;
                NotifyOfPropertyChange(() => AdcValue);
            }
        }

        private double _temperature;
        public double Temperature
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

        private int _lowThreshold;
        public int LowThreshold
        {
            get { return _lowThreshold; }
            set
            {
                if (value == _lowThreshold) return;
                _lowThreshold = value;
                NotifyOfPropertyChange(() => LowThreshold);
                QueueConfigurationUpdate();
            }
        }

        private int _highThreshold;
        public int HighThreshold
        {
            get { return _highThreshold; }
            set
            {
                if (value == _highThreshold) return;
                _highThreshold = value;
                NotifyOfPropertyChange(() => HighThreshold);
                QueueConfigurationUpdate();
            }
        }

        private int _bubblesPerMin;
        public int BubblesPerMin
        {
            get { return _bubblesPerMin; }
            set
            {
                if (value == _bubblesPerMin) return;
                _bubblesPerMin = value;
                NotifyOfPropertyChange(() => BubblesPerMin);
                QueueConfigurationUpdate();
            }
        }

        private Configuration _storedConfig; // keep track of what's stored in the brewmonitor

        public ConfigureViewModel(IBrewMonitorService brewMonitorService, DialogCoordinator dialogCoordinator)
        {
            _brewMonitorService = brewMonitorService;
            _dialogCoordinator = dialogCoordinator;
            DisplayName = "configure";
            ShowProgressRing = true;
            _brewMonitorService.SampleReceived += OnSampleReceived;
        }

        private void OnSampleReceived(object o, SampleReceivedEventArgs args)
        {
            Bubbling = args.Sample.Bubbling;
            AdcValue = args.Sample.RawAdc;
            Temperature = args.Sample.Celsius;
            Bubbling = args.Sample.Bubbling;
        }

        private void QueueConfigurationUpdate()
        {
            if (_storedConfig.HighThreshold == HighThreshold && _storedConfig.LowThreshold == LowThreshold &&
                _storedConfig.BubblesPerMin == BubblesPerMin) return;
            _timer.Stop();
            _timer.Start();
        }

        /// <summary>
        /// This writes configuration to the decive after changes have been made.
        /// The write is delayed until no changes have been made for a short time.
        /// This prevents unnecessary updates e.g. when sliding a slider.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="e"></param>
        private void ConfigurationTimerCallback(object state, ElapsedEventArgs e)
        {
            _timer.Stop();
            _storedConfig.HighThreshold = HighThreshold;
            _storedConfig.LowThreshold = LowThreshold;
            _storedConfig.BubblesPerMin = BubblesPerMin;
            _brewMonitorService.WriteConfiguration(_storedConfig);
        }

        protected override void OnActivate()
        {
            ShowProgressRing = true;
            if (_brewMonitorService != null && _brewMonitorService.IsConnected())
            {
                //var firstStep = new Task(() => Configuration = _brewMonitorService.ReadConfiguration());
                var firstStep = new Task(() =>
                {
                    _storedConfig = _brewMonitorService.ReadConfiguration();
                    LowThreshold = _storedConfig.LowThreshold;
                    HighThreshold = _storedConfig.HighThreshold;
                    BubblesPerMin = _storedConfig.BubblesPerMin;
                });
                var secondStep = firstStep.ContinueWith(t => ShowControls = true);
                secondStep.ContinueWith(t => _brewMonitorService.StartStreaming());
                firstStep.Start();

                _timer.AutoReset = false;
                _timer.Interval = 500;
                _timer.Elapsed += ConfigurationTimerCallback;
            }
            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            _brewMonitorService.StopStreaming();
            base.OnDeactivate(close);
        }
    }
}
