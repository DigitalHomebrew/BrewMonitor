using System;
using System.Collections.Generic;
using System.Windows;
using BrewMonitor;
using Caliburn.Micro;
using DashboardWPF.ViewModels;

namespace DashboardWPF
{
    public class AppBootstrapper : BootstrapperBase
    {
        private SimpleContainer _container;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            var instance = _container.GetInstance(service, key);
            if (instance != null)
                return instance;

            throw new Exception("Could not locate any instances.");
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.GetAllInstances(serviceType);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override void Configure()
        {
            _container = new SimpleContainer();
            _container.PerRequest<MainViewModel>();
            _container.Instance<IWindowManager>(new WindowManager());
            _container.Singleton<IEventAggregator, EventAggregator>();

            if (Execute.InDesignMode)
            {
                _container.Instance<IBrewMonitorService>(new BrewMonitorServiceMock());
            }
            else
            {
                _container.Instance<IBrewMonitorService>(new BrewMonitorService());
                //_container.Instance<IBrewMonitorService>(new BrewMonitorServiceMock()); // debug
            }

        }
    }
}
