using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using BrewMonitorDashboard.ViewModel;

namespace BrewMonitorDashboard.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
        }
    }
}