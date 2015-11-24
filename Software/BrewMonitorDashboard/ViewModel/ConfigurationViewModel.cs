using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using BrewMonitor;
using BrewMonitor.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace BrewMonitorDashboard.ViewModel
{
    public class ConfigurationViewModel : ViewModelBase, IDisposable
    {
        private IBrewMonitorService _brewMonService;

        private readonly Timer _bubbleRadioButtonTimer;

        public const string PlugInGridVisibilityPropertyName = "PlugInGridVisibility";
        private Visibility _plugInGridVisibility;
        public Visibility PlugInGridVisibility
        {
            get { return _plugInGridVisibility; }
            set
            {
                if (_plugInGridVisibility == value)
                    return;
                _plugInGridVisibility = value;
                RaisePropertyChanged(PlugInGridVisibilityPropertyName);
            }
        }

        public const string ConfigurationGridVisibilityPropertyName = "ConfigurationGridVisibility";
        private Visibility _configurationGridVisibility;
        public Visibility ConfigurationGridVisibility
        {
            get { return _configurationGridVisibility; }
            set
            {
                if (_configurationGridVisibility == value)
                    return;
                _configurationGridVisibility = value;
                RaisePropertyChanged(ConfigurationGridVisibilityPropertyName);
            }
        }

        public const string MinThresholdPropertyName = "MinThreshold";
        private string _minThreshold;
        public string MinThreshold
        {
            get { return _minThreshold; }
            set
            {
                if (_minThreshold == value)
                    return;
                _minThreshold = value;
                RaisePropertyChanged(MinThresholdPropertyName);
            }
        }

        public const string MaxThresholdPropertyName = "MaxThreshold";
        private string _maxThreshold;
        public string MaxThreshold
        {
            get { return _maxThreshold; }
            set
            {
                if (_maxThreshold == value)
                    return;
                _maxThreshold = value;
                RaisePropertyChanged(MaxThresholdPropertyName);
            }
        }

        public const string TimeDelayPropertyName = "TimeDelay";
        private string _timeDelay;
        public string TimeDelay
        {
            get { return _timeDelay; }
            set
            {
                if (_timeDelay == value)
                    return;
                _timeDelay = value;
                RaisePropertyChanged(TimeDelayPropertyName);
            }
        }

        public const string AdcValuePropertyName = "AdcValue";
        private int _adcValue;
        public int AdcValue
        {
            get { return _adcValue; }
            set
            {
                if (_adcValue == value)
                    return;
                _adcValue = value;
                RaisePropertyChanged(AdcValuePropertyName);
            }
        }

        public const string BubbleCountPropertyName = "BubbleCount";
        private string _bubbleCount;
        public string BubbleCount
        {
            get { return _bubbleCount; }
            set
            {
                if (_bubbleCount == value)
                    return;
                _bubbleCount = value;
                RaisePropertyChanged(BubbleCountPropertyName);
            }
        }

        public const string BubblingPropertyName = "Bubbling";
        private bool _bubbling;
        public bool Bubbling
        {
            get { return _bubbling; }
            set
            {
                if (_bubbling == value)
                    return;
                _bubbling = value;
                RaisePropertyChanged(BubblingPropertyName);
            }
        }

        public RelayCommand SaveClicked { get; private set; }
        private void SaveConfiguration()
        {
            //_brewMonService.StopStreaming();            
            _brewMonService.WriteConfiguration(new Configuration(MaxThreshold, MinThreshold, TimeDelay));
            MessageBox.Show("Configuration saved.");
        }

        public RelayCommand ReloadClicked { get; private set; }
        private void ReloadConfiguration()
        {
            try
            {
                var configuration = _brewMonService.ReadConfiguration();
                MinThreshold = configuration.LowThreshold.ToString(CultureInfo.InvariantCulture);
                MaxThreshold = configuration.HighThreshold.ToString(CultureInfo.InvariantCulture);
                TimeDelay = configuration.BubblesPerMin.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                MessageBox.Show("Error loading configuration.");
            }

            try
            {
                _brewMonService.StartStreaming();
            }
            catch (Exception)
            {
                MessageBox.Show("Error restarting streaming.");
            }
        }

        public RelayCommand AppLoadedCommand { get; private set; }
        private void AppStarted()
        {
            //if (_brewMonService.IsConnected())
                //LoadConfiguration();
            if(_brewMonService.IsConnected())
                _brewMonService.StartStreaming();
        }

        public ConfigurationViewModel(IBrewMonitorService brewMonService)
        {
            _bubbleRadioButtonTimer = new Timer(o => Bubbling = false);
            SaveClicked = new RelayCommand(SaveConfiguration);
            ReloadClicked = new RelayCommand(ReloadConfiguration);
            AppLoadedCommand = new RelayCommand(AppStarted);
            _brewMonService = brewMonService;
            _brewMonService.ConnectionChanged += BrewMonServiceOnConnectionChanged;
            _brewMonService.SampleReceived += BrewMonServiceOnSampleReceived;
            PlugInGridVisibility = _brewMonService.IsConnected() ? Visibility.Hidden : Visibility.Visible;
            ConfigurationGridVisibility = _brewMonService.IsConnected() ? Visibility.Visible : Visibility.Hidden;
        }

        private void BrewMonServiceOnSampleReceived(object sender, SampleReceivedEventArgs sampleReceivedEventArgs)
        {
            var dataSample = sampleReceivedEventArgs.Sample;
            AdcValue = dataSample.RawAdc;
            if (!dataSample.Bubbling)
                return;

            Bubbling = true;
            BubbleCount = (Convert.ToInt32(BubbleCount) + 1).ToString(CultureInfo.InvariantCulture);            
            _bubbleRadioButtonTimer.Change(500, Timeout.Infinite);
        }

        private void BrewMonServiceOnConnectionChanged(object sender, ConnectionChangedEventArgs connectionChangedEventArgs)
        {
            PlugInGridVisibility = connectionChangedEventArgs.Connected ? Visibility.Hidden : Visibility.Visible;
            ConfigurationGridVisibility = connectionChangedEventArgs.Connected ? Visibility.Visible : Visibility.Hidden;
            if (_brewMonService.IsConnected())
            {
                //LoadConfiguration();
                _brewMonService.StartStreaming();
            }
            else
            {
                MaxThreshold = "0";
                MinThreshold = "0";
                TimeDelay = "0";
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool managed)
        {
            if (!managed)
                return;
            _brewMonService.Dispose();
            _brewMonService = null;
            if (_bubbleRadioButtonTimer != null)
            {
                _bubbleRadioButtonTimer.Dispose();
            }
        }
    }
}
