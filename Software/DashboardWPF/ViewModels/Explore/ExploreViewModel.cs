using System.Windows.Media;
using BrewMonitor;
using BrewMonitor.Models;
using Caliburn.Micro;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using OxyPlot.Series;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using LineSeries = OxyPlot.Series.LineSeries;

namespace DashboardWPF.ViewModels.Explore
{
    public sealed class ExploreViewModel : Screen, IMainScreenTabItem
    {
        private readonly IBrewMonitorService _brewMonitorService;
        private readonly DialogCoordinator _dialogCoordinator;

        private readonly BackgroundWorker _downloadWorker;

        private LineSeries _tempSeries;
        private LineSeries _bubbleSeries;
        private LinearAxis _temperatureValueAxis;
        private LinearAxis _bubblesValueAxis;
        private TimeSpanAxis _timeAxis;

        private readonly List<List<MemorySample>> _recordings;

        private List<MemorySample> _samples;

        public List<MemorySample> Samples
        {
            get { return _samples; }
            set
            {
                _samples = value;
                NotifyOfPropertyChange(() => Samples);
            }
        }

        private PlotModel _plotModel;

        public PlotModel PlotModel
        {
            get { return _plotModel; }
            set
            {
                _plotModel = value;
                NotifyOfPropertyChange(() => PlotModel);
            }
        }

        private int _readingMemoryPercent;
        public int ReadingMemoryPercent
        {
            get { return _readingMemoryPercent; }
            set
            {
                if (_readingMemoryPercent == value)
                    return;
                _readingMemoryPercent = value;
                NotifyOfPropertyChange(() => ReadingMemoryPercent);
            }
        }

        private int _numberOfRecordings;
        public int NumberOfRecordings
        {
            get { return _numberOfRecordings; }
            set
            {
                if (_numberOfRecordings == value)
                    return;
                _numberOfRecordings = value;
                NotifyOfPropertyChange(() => NumberOfRecordings);
                NotifyOfPropertyChange(() => RecordingText);
            }
        }

        private int _currentRecording;
        public int CurrentRecording
        {
            get { return _currentRecording; }
            set
            {
                if (_currentRecording == value)
                    return;
                _currentRecording = value;
                NotifyOfPropertyChange(() => CurrentRecording);
                NotifyOfPropertyChange(() => RecordingText);
            }
        }

        #region Button enabled properties

        private bool _canSaveToFile;

        public bool CanSaveToFile
        {
            get { return _canSaveToFile; }
            set
            {
                if (_canSaveToFile == value)
                    return;
                _canSaveToFile = value;
                NotifyOfPropertyChange(() => CanSaveToFile);
            }
        }

        private bool _canRefreshData;

        public bool CanRefreshData
        {
            get { return _canRefreshData; }
            set
            {
                if (_canRefreshData == value)
                    return;
                _canRefreshData = value;
                NotifyOfPropertyChange(() => CanRefreshData);
            }
        }

        private bool _canGoToPrevious;

        public bool CanGoToPrevious
        {
            get { return _canGoToPrevious; }
            set
            {
                if (_canGoToPrevious == value)
                    return;
                _canGoToPrevious = value;
                NotifyOfPropertyChange(() => CanGoToPrevious);
            }
        }

        private bool _canGoToNext;

        public bool CanGoToNext
        {
            get { return _canGoToNext; }
            set
            {
                if (_canGoToNext == value)
                    return;
                _canGoToNext = value;
                NotifyOfPropertyChange(() => CanGoToNext);
            }
        }

        private bool _canEraseDevice;

        public bool CanEraseDevice
        {
            get { return _canEraseDevice; }
            set
            {
                if (_canEraseDevice == value)
                    return;
                _canEraseDevice = value;
                NotifyOfPropertyChange(() => CanEraseDevice);
            }
        }

        #endregion

        public string RecordingText
        {
            get { return "recording " + CurrentRecording + " of " + NumberOfRecordings; }
        }

        private Visibility _loadingGridVisibility;

        public Visibility LoadingGridVisibility
        {
            get { return _loadingGridVisibility; }
            set
            {
                if (_loadingGridVisibility == value)
                    return;
                _loadingGridVisibility = value;
                NotifyOfPropertyChange(() => LoadingGridVisibility);
            }
        }

        private Visibility _chartGridVisibility;

        public Visibility ChartGridVisibility
        {
            get { return _chartGridVisibility; }
            set
            {
                if (_chartGridVisibility == value)
                    return;
                _chartGridVisibility = value;
                NotifyOfPropertyChange(() => ChartGridVisibility);
            }
        }

        public ExploreViewModel(IBrewMonitorService brewMonitorService, DialogCoordinator dialogCoordinator)
        {
            _brewMonitorService = brewMonitorService;
            _dialogCoordinator = dialogCoordinator;
            _recordings = new List<List<MemorySample>>();
            DisplayName = "explore";
            PlotModel = new PlotModel();
            _samples = new List<MemorySample>();
            ChartGridVisibility = Visibility.Hidden;
            LoadingGridVisibility = Visibility.Hidden;
            SetUpChart();
            CanRefreshData = true;
            ThemeManager.IsThemeChanged += (sender, args) =>
            {
                var tempColor = (Color)args.Accent.Resources["HighlightColor"];
                var tempOxyColor = OxyColor.FromArgb(tempColor.A, tempColor.R, tempColor.G, tempColor.B);
                _tempSeries.Color = tempOxyColor;

                tempColor = (Color)args.AppTheme.Resources["BlackColor"];
                tempOxyColor = OxyColor.FromArgb(tempColor.A, tempColor.R, tempColor.G, tempColor.B);
                _timeAxis.TextColor = tempOxyColor;
                _timeAxis.TitleColor = tempOxyColor;
                _timeAxis.TicklineColor = tempOxyColor;

                var minorGridColor = (Color)args.Accent.Resources["AccentColor3"];
                var minorGridOxyColor = OxyColor.FromArgb(minorGridColor.A, minorGridColor.R, minorGridColor.G, minorGridColor.B);
                _temperatureValueAxis.TextColor = tempOxyColor;
                _temperatureValueAxis.TitleColor = tempOxyColor;
                _temperatureValueAxis.TicklineColor = tempOxyColor;
                _temperatureValueAxis.MinorGridlineColor = minorGridOxyColor;

                _bubblesValueAxis.TextColor = tempOxyColor;
                _bubblesValueAxis.TitleColor = tempOxyColor;
                _bubblesValueAxis.TicklineColor = tempOxyColor;
                
                _plotModel.InvalidatePlot(false);
            };
            _downloadWorker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            _downloadWorker.DoWork += BeginReadingMemory;
            _downloadWorker.ProgressChanged += ReadMemoryProgressChanged;
            _downloadWorker.RunWorkerCompleted += ReadMemoryCompleted;
            //_brewMonitorService.ConnectionChanged += (sender, args) =>
            //{
            //    if (!_downloadWorker.IsBusy)
            //        _downloadWorker.RunWorkerAsync();
            //};
        }


        protected override void OnInitialize()
        {
            DisplayName = "explore"; // this will show the name of the tabitem that hosts the control
            if (_brewMonitorService.IsConnected())
            {
                _brewMonitorService.StopStreaming();
            }
        }

        protected override void OnViewLoaded(object view)
        {
            RefreshData();
            
            base.OnViewLoaded(view);
        }

        private void SetUpChart()
        {
            PlotModel.LegendOrientation = LegendOrientation.Horizontal;
            PlotModel.LegendPlacement = LegendPlacement.Inside;
            PlotModel.LegendPosition = LegendPosition.TopRight;
            PlotModel.LegendBackground = OxyColors.LightGray;
            PlotModel.LegendBorder = OxyColors.Transparent;
            PlotModel.PlotAreaBorderColor = OxyColors.LightGray;
            PlotModel.Axes.Clear();

            var foregroundColor = (Color)ThemeManager.GetResourceFromAppStyle(Application.Current.MainWindow, "BlackColor");
            var foregroundOxyColor = OxyColor.FromArgb(foregroundColor.A, foregroundColor.R, foregroundColor.G, foregroundColor.B);
            var minorGridColor = (Color)ThemeManager.GetResourceFromAppStyle(Application.Current.MainWindow, "AccentColor3");
            var minorGridOxyColor = OxyColor.FromArgb(minorGridColor.A, minorGridColor.R, minorGridColor.G, minorGridColor.B);

            _timeAxis = new TimeSpanAxis
            {
                TitleFontSize = 20,
                AxisTitleDistance = 15,
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None,
                IntervalLength = 80,
                Title = "time (hh:mm:ss)",
                TextColor = foregroundOxyColor,
                TitleColor = foregroundOxyColor,
                TicklineColor = foregroundOxyColor,
                MaximumPadding = 0,
                MinimumPadding = 0
            };
            PlotModel.Axes.Add(_timeAxis);

            var temperatureAxisTitle = Properties.Settings.Default["Units"].ToString() == "Metric"
                ? "temperature (°C)"
                : "temperature (°F)";
            _temperatureValueAxis = new LinearAxis
            {                
                MajorStep = 2,
                MinorStep = 0.5,
                TitleFontSize = 20,
                AxisTitleDistance = 22,
                StartPosition = 0,
                Key = "LeftAxis",
                Position = AxisPosition.Left,
                MajorGridlineColor = OxyColors.Gray,
                MinorGridlineColor = minorGridOxyColor,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Title = temperatureAxisTitle,
                TextColor = foregroundOxyColor,
                TitleColor = foregroundOxyColor,
                TicklineColor = foregroundOxyColor,
                MinimumRange = 10
            };
            PlotModel.Axes.Add(_temperatureValueAxis);
            _bubblesValueAxis = new LinearAxis
            {
                MajorStep = 20,
                MinorStep = 10,
                MinimumRange = 20,
                TitleFontSize = 20,
                AxisTitleDistance = 15,
                StartPosition = 0,
                Key = "RightAxis",
                Position = AxisPosition.Right,
                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None,
                Title = "bubble rate (bpm)",
                TextColor = foregroundOxyColor,
                TitleColor = foregroundOxyColor,
                TicklineColor = foregroundOxyColor  
            };
            PlotModel.Axes.Add(_bubblesValueAxis);

            var highlightColor = (Color)ThemeManager.GetResourceFromAppStyle(Application.Current.MainWindow, "HighlightColor");
            var highlightOxyColor = OxyColor.FromArgb(highlightColor.A, highlightColor.R, highlightColor.G, highlightColor.B);

            PlotModel.Series.Clear();
            _tempSeries = new LineSeries
            {
                YAxisKey = "LeftAxis",
                StrokeThickness = 5,                
                MarkerSize = 3,
                MarkerStroke = OxyColors.Red,
                MarkerType = MarkerType.None,
                CanTrackerInterpolatePoints = false,
                Color = highlightOxyColor,
                Title = "Temperature",
                Smooth = true,
            };

            _bubbleSeries = new AreaSeries
            {
                YAxisKey = "RightAxis",
                StrokeThickness = 1,
                Color = OxyColors.Gray,
                CanTrackerInterpolatePoints = false,
                Title = "Bubble rate",
                Smooth = true,
                DataFieldX = "Sample",
                DataFieldX2 = "Sample",
                DataFieldY = "Value",
                DataFieldY2 = "Zero"
            };

            PlotModel.Series.Add(_bubbleSeries);
            PlotModel.Series.Add(_tempSeries);

        }

        #region Button Press handlers

        public void RefreshData()
        {
            if (!_brewMonitorService.IsConnected()) 
                return;

            if (!_downloadWorker.IsBusy)
                _downloadWorker.RunWorkerAsync();
        }

        public void GoToPrevious()
        {
            if (CurrentRecording <= 1) return;
            CurrentRecording--;
            LoadRecording(CurrentRecording);
        }

        public void GoToNext()
        {
            if (CurrentRecording >= NumberOfRecordings) return;
            CurrentRecording++;
            LoadRecording(CurrentRecording);
        }

        public void SaveToFile()
        {
            if (_recordings == null || _recordings.Count <= 0)
                return;
            var sfd = new SaveFileDialog
            {
                DefaultExt = "csv",
                Filter = "Comma Separated Variable Files | *.csv",
                FileName = "BrewData " + CurrentRecording.ToString("00") + ".csv"
            };
            sfd.ShowDialog();
            if (String.IsNullOrEmpty(sfd.FileName))
                return;
            File.Delete(sfd.FileName);

            for (var i = 0; i < _recordings[CurrentRecording - 1].Count; i++)
            {
                var ms = _recordings[CurrentRecording - 1][i];
                File.AppendAllText(sfd.FileName, i + ',' + ConvertUnits(ms.Celsius) + ',' + ms.BubbleCount + Environment.NewLine);
            }
        }

        public void EraseDevice()
        {
            var settings = new MetroDialogSettings
            {
                AffirmativeButtonText = "Yep",
                NegativeButtonText = "Aw hell no!"
            };

            // get confirmation first            
            _dialogCoordinator.ShowMessageAsync(this, "Erase BrewMonitor?", "Sure you want to erase the stored data?",
                MessageDialogStyle.AffirmativeAndNegative, settings)
                .ContinueWith(t =>
                {
                    if (t.Result != MessageDialogResult.Affirmative) return;
                    CanRefreshData = false;
                    LoadingGridVisibility = Visibility.Visible;
                    ChartGridVisibility = Visibility.Collapsed;
                    _brewMonitorService.EraseMemory();
                    RefreshData();
                });
        }

        #endregion

        private void BeginReadingMemory(object sender, DoWorkEventArgs e)
        {
            // show the progress bars while loading
            CanRefreshData = false;
            LoadingGridVisibility = Visibility.Visible;
            ChartGridVisibility = Visibility.Collapsed;

            // clear the projects collection
            _recordings.Clear();

            // get the number of memory splits
            var config = _brewMonitorService.ReadConfiguration();
            var divisor = config.SampleRateDivisor;

            var bgw = (BackgroundWorker)sender;
            for (var i = 0; i < config.NumberOfSamples; i++)
            {
                if (bgw.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                var sample = _brewMonitorService.ReadMemory(i);

                if (sample == null || sample.IsBlank)
                    break;
                sample.Divisor = divisor;
                var percent = ((i + 1) * 100) / config.NumberOfSamples;
                bgw.ReportProgress(percent, sample);
            }
        }

        private void ReadMemoryProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var sample = (MemorySample)e.UserState;
            if (sample.IsStartStopMarker || _recordings.Count == 0)
            {
                _recordings.Add(new List<MemorySample>());
            }
            else
            {
                var sampleNumber = _recordings.Last().Count;
                sample.SampleNumber = sampleNumber;
                _recordings.Last().Add(sample);
            }

            ReadingMemoryPercent = e.ProgressPercentage;
        }

        private void ReadMemoryCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ReadingMemoryPercent = 100;
            SetUpChart();

            // update the number of recordings text
            NumberOfRecordings = _recordings.Count;
            // currentrecording must be at least 1
            if (CurrentRecording == 0)
                CurrentRecording++;
            if (NumberOfRecordings >= CurrentRecording)
                LoadRecording(CurrentRecording);
            else if (NumberOfRecordings > 0)
                LoadRecording(1);
            else
            {
                //clear the chart
                _tempSeries.Points.Clear();
                _bubbleSeries.Points.Clear();
                PlotModel.InvalidatePlot(true);
            }

            // hide the progress bars when we're done loading
            LoadingGridVisibility = Visibility.Hidden;
            ChartGridVisibility = Visibility.Visible;
            CanRefreshData = true;
            CanSaveToFile = CanEraseDevice = NumberOfRecordings > 0;

            //_refreshing = false;
        }

        /// <summary>
        /// Loads a recording into the chart control
        /// </summary>
        /// <param name="recordingNumber">the recording number (not index)</param>
        private void LoadRecording(int recordingNumber)
        {
            // clear grid's data
            try
            {
                SetUpChart();
            }
            catch { } // naughty
            _tempSeries.Points.Clear();
            _bubbleSeries.Points.Clear();

            if (_recordings.Count >= recordingNumber) // check recording exists
            {
                CurrentRecording = recordingNumber;

                if (_recordings[CurrentRecording - 1].Any()) // check recording contains samples
                {                    
                    // work out the divisor
                    var actualDivisor = Math.Pow(2, _recordings[CurrentRecording - 1][0].Divisor - 1); // s.divisor is the number of splits 
                    
                    // add points to the chart   
                    var bubbleAreaPoints = new List<AreaPoint>();
                    for (var i = 0; i < _recordings[CurrentRecording - 1].Count; i++)
                    {
                        var currentSample = _recordings[CurrentRecording - 1][i];
                        _tempSeries.Points.Add(new DataPoint(i * actualDivisor, ConvertUnits(currentSample.Celsius)));

                        var averagedBubbleValue = GetAverageBubblesFromSample(i, (int)(128 / actualDivisor)); // average over about a 2 minuted period
                        
                        bubbleAreaPoints.Add(new AreaPoint(i * actualDivisor, averagedBubbleValue * 60 / actualDivisor));                        
                    }
                    _bubbleSeries.ItemsSource = bubbleAreaPoints;

                    // set up the axes
                    var minimumTemp = ConvertUnits(_recordings[CurrentRecording - 1].Min(r => r.Celsius));
                    var axisMinimum = Math.Floor(minimumTemp/2.0)*2.0; // round down to nearest 2
                    _temperatureValueAxis.Minimum = axisMinimum;
                    var maximumTemp = ConvertUnits(_recordings[CurrentRecording - 1].Max(r => r.Celsius));
                    var axisMaximum = Math.Ceiling(maximumTemp/2.0)*2.0; // round up to nearest 2
                    _temperatureValueAxis.Maximum = axisMaximum;

                    var minimumBubbles = _recordings[CurrentRecording - 1].Min(r => r.BubbleCount) * 60 / actualDivisor;
                    var bubbleAxisMinimum = Math.Floor(minimumBubbles/20.0)*20.0; // round down to nearest 10-2
                    _bubblesValueAxis.Minimum = bubbleAxisMinimum;
                    var maximumBubbles = _recordings[CurrentRecording - 1].Max(r => r.BubbleCount) * 60 / actualDivisor;
                    var bubbleAxisMaximum = Math.Ceiling(maximumBubbles/20.0)*20.0; // round up to nearest 10+2
                    _bubblesValueAxis.Maximum = bubbleAxisMaximum; 
                }
            }

            // update the chart
            PlotModel.InvalidatePlot(true);

            // set the recordings buttons
            CanGoToNext = NumberOfRecordings > recordingNumber;
            CanGoToPrevious = recordingNumber > 1;
            CanSaveToFile = _recordings[CurrentRecording - 1].Any();
        }

        private double GetAverageBubblesFromSample(int currentIndex, int numberOfPreviousSamples)
        {
            var samples = new List<MemorySample> {_recordings[CurrentRecording - 1][currentIndex]};
            for (var i = 0; i < numberOfPreviousSamples; i++)
            {
                if(_recordings[CurrentRecording - 1].Count > currentIndex + i + 1)
                    samples.Add(_recordings[CurrentRecording - 1][currentIndex + i + 1]);
            }
            var result = samples.Average(s => s.BubbleCount);
            return result;
        }

        private double ConvertUnits(double c)
        {
            if (Properties.Settings.Default["Units"].ToString() == "Metric")
                return c;

            return (c * 9 / 5) + 32;
        }

        public class AreaPoint
        {
            public double Zero { get { return 0; } }
            public double Sample { get; set; }
            public double Value { get; set; }

            public AreaPoint(double sample, double value)
            {
                Sample = sample;
                Value = value;
            }
        }
    }
}
