using System.Windows.Controls;
using System.Windows.Media;
using BrewMonitorDashboard.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace BrewMonitorDashboard.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public const string CurrentControlPropertyName = "CurrentControl";
        private UserControl _currentControl;
        public UserControl CurrentControl
        {
            get { return _currentControl ?? (_currentControl = new ExplorerControl()); }
            set
            {
                if (_currentControl.Equals(value))
                    return;
                _currentControl = value;
                RaisePropertyChanged(CurrentControlPropertyName);
            }
        }

        public const string ExplorerLinkForegroundPropertyName = "ExplorerLinkForeground";
        private SolidColorBrush _explorerLinkForeground;
        public SolidColorBrush ExplorerLinkForeground
        {
            get { return _explorerLinkForeground; }
            set
            {
                if (_explorerLinkForeground != null && _explorerLinkForeground.Equals(value))
                    return;
                _explorerLinkForeground = value;
                RaisePropertyChanged(ExplorerLinkForegroundPropertyName);
            }
        }

        public const string ConfigurationLinkForegroundPropertyName = "ConfigurationLinkForeground";
        private SolidColorBrush _configurationLinkForeground;        
        public SolidColorBrush ConfigurationLinkForeground
        {
            get { return _configurationLinkForeground; }
            set
            {
                if (_configurationLinkForeground != null && _configurationLinkForeground.Equals(value))
                    return;
                _configurationLinkForeground = value;
                RaisePropertyChanged(ConfigurationLinkForegroundPropertyName);
            }
        }

        public RelayCommand AppStartedCommand { get; private set; }
        private void AppStarted()
        {
            ShowExplorerControl();
        }

        public RelayCommand ExplorerClicked { get; private set; }
        private void ShowExplorerControl()
        {
            CurrentControl = new ExplorerControl();
            ConfigurationLinkForeground = new SolidColorBrush(Colors.White);
            ExplorerLinkForeground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFA803D"));            
        }

        public RelayCommand ConfigurationClicked { get; private set; }
        private void ShowConfigurationControl()
        {
            CurrentControl = new ConfigurationControl();
            ExplorerLinkForeground = new SolidColorBrush(Colors.White);
            ConfigurationLinkForeground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFA803D"));
        }
        
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            AppStartedCommand = new RelayCommand(AppStarted);
            ExplorerClicked = new RelayCommand(ShowExplorerControl);
            ConfigurationClicked = new RelayCommand(ShowConfigurationControl);                                               
        }                        
    }

    public class ExplorerModel
    {
        
    }
}