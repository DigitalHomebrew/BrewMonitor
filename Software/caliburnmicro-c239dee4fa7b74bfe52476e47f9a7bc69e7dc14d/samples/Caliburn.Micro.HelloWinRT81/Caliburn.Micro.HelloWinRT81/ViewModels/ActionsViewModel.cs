﻿using System;

namespace Caliburn.Micro.WinRT.Sample.ViewModels
{
    public class ActionsViewModel : ViewModelBase
    {
        private string input;
        private string output;

        public ActionsViewModel(INavigationService navigationService)
            : base(navigationService)
        {
        }

        public string Output
        {
            get
            {
                return output;
            }
            set
            {
                this.Set(ref output, value);
            }
        }

        public string Input
        {
            get
            {
                return input;
            }
            set
            {
                this.Set(ref input, value);
            }
        }

        public void SimpleSayHello()
        {
            Output = "Hello from Caliburn.Micro";
        }

        public void SayHello(string name)
        {
            Output = String.Format("Hello {0} from Caliburn.Micro", Input);
        }

        public bool CanSayHello(string name)
        {
            return !String.IsNullOrEmpty(name);
        }

        public void AppBarHello()
        {
            Output = "Hello from the App Bar.";
        }
    }
}
