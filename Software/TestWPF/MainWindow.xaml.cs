using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using BrewMonitor;
using BrewMonitor.Models;

namespace TestWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IBrewMonitorService _bm;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainButton_OnClick(object sender, RoutedEventArgs e)
        {
            var config = _bm.ReadConfiguration();
            if (_bm.IsConnected())
            {
                var list = new List<MemorySample>();
                File.Delete("C:\\test.csv");
                for (var i = 0; i < 1024; i++)
                {
                    var s = _bm.ReadMemory(i);
                    list.Add(s);
                    File.AppendAllText("c:\\test.csv", s.SampleNumber + "," + s.Celcius + "," + s.BubbleCount + Environment.NewLine);
                }
            }
            
        }

        private void LoadButton_OnClick(object sender, RoutedEventArgs e)
        {
            _bm = new BrewMonitorService();
        }
    }
}
