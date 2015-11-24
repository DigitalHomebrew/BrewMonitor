﻿using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Caliburn.Micro.WinRT.Sample.ViewModels;
using Caliburn.Micro.WinRT.Sample.Views;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.UI.Xaml.Controls;

namespace Caliburn.Micro.WinRT.Sample
{
    public sealed partial class App
    {
        private WinRTContainer container;
        private INavigationService navigationService;

        public App()
        {
            InitializeComponent();
        }

        protected override void Configure()
        {
            LogManager.GetLog = t => new DebugLog(t);

            container = new WinRTContainer();
            container.RegisterWinRTServices();

            container.RegisterSharingService();

            var settingsService = container.RegisterSettingsService();
                
            settingsService.RegisterFlyoutCommand<SampleSettingsViewModel>("Custom");
            settingsService.RegisterUriCommand("View Website", new Uri("http://caliburnmicro.codeplex.com"));

            container
                .PerRequest<ActionsViewModel>()
                .PerRequest<BindingsViewModel>()
                .PerRequest<CoroutineViewModel>()
                .PerRequest<ExecuteViewModel>()
                .PerRequest<MenuViewModel>()
                .PerRequest<NavigationTargetViewModel>()
                .PerRequest<NavigationViewModel>()
                .PerRequest<SampleSettingsViewModel>()
                .PerRequest<SearchViewModel>()
                .PerRequest<SettingsViewModel>()
                .PerRequest<SetupViewModel>()
                .PerRequest<ShareSourceViewModel>()
                .PerRequest<ShareTargetViewModel>()
                .PerRequest<ConventionsViewModel>()
                .PerRequest<HubViewModel>();

            // We want to use the Frame in OnLaunched so set it up here

            PrepareViewFirst();
        }

        protected override object GetInstance(Type service, string key)
        {
            var instance = container.GetInstance(service, key);
            if (instance != null)
                return instance;

            throw new Exception("Could not locate any instances.");
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }

        protected override void PrepareViewFirst(Frame rootFrame)
        {
            navigationService = container.RegisterNavigationService(rootFrame);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Initialize();

            var resumed = false;

            if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                resumed = navigationService.ResumeState();
            }

            if (!resumed)
                DisplayRootView<MenuView>();
        }

        protected override void OnSearchActivated(SearchActivatedEventArgs args)
        {
            DisplayRootView<SearchView>(args.QueryText);
        }

        protected override void OnShareTargetActivated(ShareTargetActivatedEventArgs args)
        {
            // Normally wouldn't need to do this but need the container to be initialised
            Initialize();

            // replace the share operation in the container
            container.UnregisterHandler(typeof(ShareOperation), null);
            container.Instance(args.ShareOperation);

            DisplayRootViewFor<ShareTargetViewModel>();
        }

        protected override void OnSuspending(object sender, SuspendingEventArgs e)
        {
            navigationService.SuspendState();
        }
    }
}
