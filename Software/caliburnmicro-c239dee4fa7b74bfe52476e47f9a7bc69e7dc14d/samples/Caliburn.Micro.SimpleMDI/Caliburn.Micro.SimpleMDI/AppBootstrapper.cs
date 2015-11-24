﻿namespace Caliburn.Micro.SimpleMDI
{
    using System.Windows;

    public class AppBootstrapper : BootstrapperBase
    {
        public AppBootstrapper()
        {
            Start();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
