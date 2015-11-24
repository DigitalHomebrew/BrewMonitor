using System.Windows;
using BrewMonitor;
using BrewMonitor.Models;
using Caliburn.Micro;
using DashboardWPF.ViewModels.Configure;
using DashboardWPF.ViewModels.Explore;
using DashboardWPF.ViewModels.Monitor;
using DashboardWPF.ViewModels.SettingsFlyout;
using MahApps.Metro.Controls.Dialogs;

namespace DashboardWPF.ViewModels
{
    public class MainViewModel : Conductor<IMainScreenTabItem>.Collection.OneActive
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly IBrewMonitorService _brewMonitorService;

        private readonly IObservableCollection<SettingsFlyoutViewModel> _flyouts = new BindableCollection<SettingsFlyoutViewModel>();
        public IObservableCollection<SettingsFlyoutViewModel> Flyouts
        {
            get
            {
                return _flyouts;
            }
        }

        private const string WindowTitleDefault = "BrewMonitor Dashboard";
        private string _windowTitle = WindowTitleDefault;
        public string WindowTitle
        {
            get { return _windowTitle; }
            set
            {
                _windowTitle = value;
                NotifyOfPropertyChange(() => WindowTitle);
            }
        }

        private Visibility _plugInMessageVisibility;
        public Visibility PlugInMessageVisibility
        {
            get { return _plugInMessageVisibility; }
            set
            {
                _plugInMessageVisibility = value;
                NotifyOfPropertyChange(() => PlugInMessageVisibility);
            }
        }

        public void ToggleSettingsFlyout(int index)
        {
            var flyout = _flyouts[index];
            flyout.IsOpen = !flyout.IsOpen;
        }

        public MainViewModel(IBrewMonitorService brewMonitorService)
        {
            _brewMonitorService = brewMonitorService;
            Items.Add(new ExploreViewModel(_brewMonitorService, DialogCoordinator.Instance));
            Items.Add(new MonitorViewModel(_brewMonitorService, DialogCoordinator.Instance));
            Items.Add(new ConfigureViewModel(_brewMonitorService, DialogCoordinator.Instance));

            _brewMonitorService.ConnectionChanged += BrewMonServiceOnConnectionChanged;
            PlugInMessageVisibility = _brewMonitorService.IsConnected() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void BrewMonServiceOnConnectionChanged(object sender, ConnectionChangedEventArgs connectionChangedEventArgs)
        {
            PlugInMessageVisibility = connectionChangedEventArgs.Connected ? Visibility.Collapsed : Visibility.Visible;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Flyouts.Add(new SettingsFlyoutViewModel());
        }
    }
}
