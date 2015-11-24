using Caliburn.Micro;
using MahApps.Metro;
using MahApps.Metro.Controls;
using System.Windows;

namespace DashboardWPF.ViewModels.SettingsFlyout
{
    public class SettingsFlyoutViewModel : PropertyChangedBase
    {
        private string _header;
        public string Header
        {
            get
            {
                return _header;
            }

            set
            {
                if (value == _header)
                {
                    return;
                }

                _header = value;
                NotifyOfPropertyChange(() => Header);
            }
        }

        private bool _isOpen;
        public bool IsOpen
        {
            get
            {
                return _isOpen;
            }

            set
            {
                if (value.Equals(_isOpen))
                {
                    return;
                }

                _isOpen = value;
                NotifyOfPropertyChange(() => IsOpen);
            }
        }

        private Position _position;
        public Position Position
        {
            get
            {
                return _position;
            }

            set
            {
                if (value == _position)
                {
                    return;
                }

                _position = value;
                NotifyOfPropertyChange(() => Position);
            }
        }

        private bool _metricChecked;
        public bool Metric
        {
            get { return _metricChecked; }
            set
            {
                if (_metricChecked.Equals(value)) return;
                _metricChecked = value;
                Imperial = !value;
                Properties.Settings.Default["Units"] = "Metric";
                Properties.Settings.Default.Save();
                NotifyOfPropertyChange(() => Metric);
            }
        }

        private bool _imperialChecked;
        public bool Imperial
        {
            get { return _imperialChecked; }
            set
            {
                if (_imperialChecked.Equals(value)) return;
                _imperialChecked = value;
                Metric = !value;
                Properties.Settings.Default["Units"] = "Imperial";
                Properties.Settings.Default.Save();
                NotifyOfPropertyChange(() => Imperial);
            }
        }

        private bool _darkThemeChecked;
        public bool DarkTheme
        {
            get { return _darkThemeChecked; }
            set
            {
                if (_darkThemeChecked.Equals(value)) return;
                _darkThemeChecked = value;
                LightTheme = !value;
                SetTheme(false);
                NotifyOfPropertyChange(() => DarkTheme);
            }
        }

        private bool _lightThemeChecked;
        public bool LightTheme
        {
            get { return _lightThemeChecked; }
            set
            {
                if (_lightThemeChecked.Equals(value)) return;
                _lightThemeChecked = value;
                DarkTheme = !value;
                SetTheme(true);
                NotifyOfPropertyChange(() => LightTheme);
            }
        }

        private bool _blueAccentChecked;
        public bool BlueAccent
        {
            get { return _blueAccentChecked; }
            set
            {
                if (_blueAccentChecked.Equals(value)) return;
                _blueAccentChecked = value;
                if (value)
                {
                    GreenAccent = false;
                    OrangeAccent = false;
                    RedAccent = false;
                    SetAccent("Blue");
                }
                NotifyOfPropertyChange(() => BlueAccent);
            }
        }

        private bool _greenAccentChecked;
        public bool GreenAccent
        {
            get { return _greenAccentChecked; }
            set
            {
                if (_greenAccentChecked.Equals(value)) return;
                _greenAccentChecked = value;
                if (value)
                {
                    BlueAccent = false;
                    OrangeAccent = false;
                    RedAccent = false;
                    SetAccent("Green");
                }
                NotifyOfPropertyChange(() => GreenAccent);
            }
        }

        private bool _orangeAccentChecked;
        public bool OrangeAccent
        {
            get { return _orangeAccentChecked; }
            set
            {
                if (_orangeAccentChecked.Equals(value)) return;
                _orangeAccentChecked = value;
                if (value)
                {
                    BlueAccent = false;
                    GreenAccent = false;
                    RedAccent = false;
                    SetAccent("Orange");
                }
                NotifyOfPropertyChange(() => OrangeAccent);
            }
        }

        private bool _redAccentChecked;
        public bool RedAccent
        {
            get { return _redAccentChecked; }
            set
            {
                if (_redAccentChecked.Equals(value)) return;
                _redAccentChecked = value;
                if (value)
                {
                    BlueAccent = false;
                    GreenAccent = false;
                    OrangeAccent = false;
                    SetAccent("Red");
                }
                NotifyOfPropertyChange(() => RedAccent);
            }
        }

        public SettingsFlyoutViewModel()
        {
            Header = "";
            Position = Position.Right;
            BlueAccent = (Properties.Settings.Default["Accent"].ToString() == "Blue");
            GreenAccent = (Properties.Settings.Default["Accent"].ToString() == "Green");
            OrangeAccent = (Properties.Settings.Default["Accent"].ToString() == "Orange");
            RedAccent = (Properties.Settings.Default["Accent"].ToString() == "Red");
            LightTheme = (Properties.Settings.Default["Theme"].ToString() == "BaseLight");
            DarkTheme = (Properties.Settings.Default["Theme"].ToString() == "BaseDark");
            Imperial = (Properties.Settings.Default["Units"].ToString() == "Imperial");
            Metric = (Properties.Settings.Default["Units"].ToString() == "Metric");
        }

        public void SetAccent(string colour)
        {
            var currentTheme = ThemeManager.DetectAppStyle(Application.Current).Item1;
            var newAccent = ThemeManager.GetAccent(colour);
            ThemeManager.ChangeAppStyle(Application.Current, newAccent, currentTheme);

            Properties.Settings.Default["Accent"] = colour;
            Properties.Settings.Default.Save();
        }

        public void SetTheme(bool lightTheme)
        {
            var themeName = lightTheme ? "BaseLight" : "BaseDark";

            var currentAccent = ThemeManager.DetectAppStyle(Application.Current).Item2;
            var newTheme = ThemeManager.GetAppTheme(themeName);
            ThemeManager.ChangeAppStyle(Application.Current, currentAccent, newTheme);

            Properties.Settings.Default["Theme"] = themeName;
            Properties.Settings.Default.Save();
        }


        public void SetDemoMode()
        {
            Properties.Settings.Default["Demo"] = "True";
            Properties.Settings.Default.Save();
        }

        public void SetRealMode()
        {
            Properties.Settings.Default["Demo"] = "False";
            Properties.Settings.Default.Save();
        }
    }
}
