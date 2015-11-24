using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using BrewMonitor;
using BrewMonitor.Models;
using BrewMonitorDashboard.Helpers;
using BrewMonitorDashboard.Model;
using ClosedXML.Excel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Win32;

namespace BrewMonitorDashboard.ViewModel
{
    public class ExplorerViewModel : ViewModelBase, IDisposable
    {
        private BackgroundWorker _downloadWorker;

        private bool _downloading;
        private readonly List<List<MemorySample>> _recordings;
        private IBrewMonitorService _brewMonService;
        private IMessageBoxService _messageBoxService;

        public const string LoadingGridVisibilityPropertyName = "LoadingGridVisibility";
        private Visibility _loadingGridVisibility;
        public Visibility LoadingGridVisibility
        {
            get { return _loadingGridVisibility; }
            set
            {
                if (_loadingGridVisibility == value)
                    return;
                _loadingGridVisibility = value;
                RaisePropertyChanged(LoadingGridVisibilityPropertyName);
            }
        }

        public const string ChartGridVisibilityPropertyName = "ChartGridVisibility";
        private Visibility _chartGridVisibility;
        public Visibility ChartGridVisibility
        {
            get { return _chartGridVisibility; }
            set
            {
                if (_chartGridVisibility == value)
                    return;
                _chartGridVisibility = value;
                RaisePropertyChanged(ChartGridVisibilityPropertyName);
            }
        }

        public const string PlugInGridVisibilityPropertyName = "PlugInGridVisibility";
        private Visibility _plugInGridVisibility;
        public Visibility PlugInGridVisibility
        {
            get { return _plugInGridVisibility; }
            set
            {
                if (_plugInGridVisibility == value)
                    return;
                _plugInGridVisibility = value;
                RaisePropertyChanged(PlugInGridVisibilityPropertyName);
            }
        }

        public const string ExplorerGridVisibilityPropertyName = "ExplorerGridVisibility";
        private Visibility _explorerGridVisibility;
        public Visibility ExplorerGridVisibility
        {
            get { return _explorerGridVisibility; }
            set
            {
                if (_explorerGridVisibility == value)
                    return;
                _explorerGridVisibility = value;
                RaisePropertyChanged(ExplorerGridVisibilityPropertyName);
            }
        }

        public const string TemperatureSeriesVisibilityPropertyName = "TemperatureSeriesVisibility";
        private Visibility _temperatureSeriesVisibility;
        public Visibility TemperatureSeriesVisibility
        {
            get { return _temperatureSeriesVisibility; }
            set
            {
                if (_temperatureSeriesVisibility == value)
                    return;
                _temperatureSeriesVisibility = value;
                RaisePropertyChanged(TemperatureSeriesVisibilityPropertyName);
            }
        }

        public const string BubblesSeriesVisibilityPropertyName = "BubblesSeriesVisibility";
        private Visibility _bubblesSeriesVisibility;
        public Visibility BubblesSeriesVisibility
        {
            get { return _bubblesSeriesVisibility; }
            set
            {
                if (_bubblesSeriesVisibility == value)
                    return;
                _bubblesSeriesVisibility = value;
                RaisePropertyChanged(BubblesSeriesVisibilityPropertyName);
            }
        }

        public const string CurrentRecordingPropertyName = "CurrentRecording";
        private int _currentRecording;
        public int CurrentRecording
        {
            get { return _currentRecording; }
            set
            {
                if (_currentRecording == value)
                    return;
                _currentRecording = value;
                RaisePropertyChanged(CurrentRecordingPropertyName);
            }
        }

        public const string DurationTextPropertyName = "DurationText";
        private string _durationText;
        public string DurationText
        {
            get { return _durationText; }
            set
            {
                if (_durationText == value)
                    return;
                _durationText = value;
                RaisePropertyChanged(DurationTextPropertyName);
            }
        }

        public const string TemperatureSeriesCheckedPropertyName = "TemperatureSeriesChecked";
        private bool _temperatureSeriesChecked;
        public bool TemperatureSeriesChecked
        {
            get { return _temperatureSeriesChecked; }
            set
            {
                if (_temperatureSeriesChecked == value)
                    return;
                _temperatureSeriesChecked = value;
                TemperatureSeriesVisibility = value ? Visibility.Visible : Visibility.Hidden;
                RaisePropertyChanged(TemperatureSeriesCheckedPropertyName);
            }
        }

        public const string BubblesSeriesCheckedPropertyName = "BubblesSeriesChecked";
        private bool _bubblesSeriesChecked;
        public bool BubblesSeriesChecked
        {
            get { return _bubblesSeriesChecked; }
            set
            {
                if (_bubblesSeriesChecked == value)
                    return;
                _bubblesSeriesChecked = value;
                BubblesSeriesVisibility = value ? Visibility.Visible : Visibility.Hidden;
                RaisePropertyChanged(BubblesSeriesCheckedPropertyName);
            }
        }

        public const string ChartMaxPropertyName = "ChartMax";
        private int _chartMax;
        public int ChartMax
        {
            get { return _chartMax; }
            set
            {
                if (_chartMax == value)
                    return;
                _chartMax = value;
                RaisePropertyChanged(ChartMaxPropertyName);
            }
        }

        public const string ChartMinPropertyName = "ChartMin";
        private int _chartMin;
        public int ChartMin
        {
            get { return _chartMin; }
            set
            {
                if (_chartMin == value)
                    return;
                _chartMin = value;
                RaisePropertyChanged(ChartMinPropertyName);
            }
        }

        public const string TimeAxisHeaderPropertyName = "TimeAxisHeader";
        private string _timeAxisHeader;
        public string TimeAxisHeader
        {
            get { return _timeAxisHeader; }
            set
            {
                if (_timeAxisHeader == value)
                    return;
                _timeAxisHeader = value;
                RaisePropertyChanged(TimeAxisHeaderPropertyName);
            }
        }

        public const string TimeAxisIntervalPropertyName = "TimeAxisInterval";
        private int _timeAxisInterval;
        public int TimeAxisInterval
        {
            get { return _timeAxisInterval; }
            set
            {
                if (_timeAxisInterval == value)
                    return;
                _timeAxisInterval = value;
                RaisePropertyChanged(TimeAxisIntervalPropertyName);
            }
        }

        public const string TimeAxisMaxValuePropertyName = "TimeAxisMaxValue";
        private double _timeAxisMaxValue;
        public double TimeAxisMaxValue
        {
            get { return _timeAxisMaxValue; }
            set
            {
                _timeAxisMaxValue = value;
                RaisePropertyChanged(TimeAxisMaxValuePropertyName);
            }
        }

        public const string NumberOfRecordingsPropertyName = "NumberOfRecordings";
        private int _numberOfRecordings;
        public int NumberOfRecordings
        {
            get { return _numberOfRecordings; }
            set
            {
                if (_numberOfRecordings == value)
                    return;
                _numberOfRecordings = value;
                RaisePropertyChanged(NumberOfRecordingsPropertyName);
            }
        }

        public const string NumberOfSamplesPropertyName = "NumberOfSamples";
        private int _numberOfSamples;
        public int NumberOfSamples
        {
            get { return _numberOfSamples; }
            set
            {
                if (_numberOfSamples == value)
                    return;
                _numberOfSamples = value;
                RaisePropertyChanged(NumberOfSamplesPropertyName);
            }
        }

        public const string SampleIntervalPropertyName = "SampleInterval";
        private string _sampleInterval;
        public string SampleInterval
        {
            get { return _sampleInterval; }
            set
            {
                if (_sampleInterval == value)
                    return;
                _sampleInterval = value;
                RaisePropertyChanged(SampleIntervalPropertyName);
            }
        }

        public const string MaxTemperaturePropertyName = "MaxTemperature";
        private string _maxTemperature;
        public string MaxTemperature
        {
            get { return _maxTemperature; }
            set
            {
                _maxTemperature = value;
                RaisePropertyChanged(MaxTemperaturePropertyName);
            }
        }

        public const string MinTemperaturePropertyName = "MinTemperature";
        private string _minTemperature;
        public string MinTemperature
        {
            get { return _minTemperature; }
            set
            {
                _minTemperature = value;
                RaisePropertyChanged(MinTemperaturePropertyName);
            }
        }

        public const string AvgTemperaturePropertyName = "AvgTemperature";
        private string _avgTemperature;
        public string AvgTemperature
        {
            get { return _avgTemperature; }
            set
            {
                _avgTemperature = value;
                RaisePropertyChanged(AvgTemperaturePropertyName);
            }
        }

        public const string BubbleCountPropertyName = "BubbleCount";
        private int _bubbleCount;
        public int BubbleCount
        {
            get { return _bubbleCount; }
            set
            {
                if (_bubbleCount == value)
                    return;
                _bubbleCount = value;
                RaisePropertyChanged(BubbleCountPropertyName);
            }
        }

        public const string AverageBubbleRatePropertyName = "AverageBubbleRate";
        private string _averageBubbleRate;
        public string AverageBubbleRate
        {
            get { return _averageBubbleRate; }
            set
            {
                if (_averageBubbleRate == value)
                    return;
                _averageBubbleRate = value;
                RaisePropertyChanged(AverageBubbleRatePropertyName);
            }
        }

        public const string SampleCollectionPropertyName = "SampleCollection";
        private ObservableCollection<ChartSample> _sampleCollection;
        public ObservableCollection<ChartSample> SampleCollection
        {
            get
            {
                return _sampleCollection;
            }
            set
            {
                if (_sampleCollection == value)
                    return;

                _sampleCollection = value;
                RaisePropertyChanged(SampleCollectionPropertyName);
            }
        }

        public const string ReadingMemoryPercentPropertyName = "ReadingMemoryPercent";
        private int _readingMemoryPercent;
        public int ReadingMemoryPercent
        {
            get { return _readingMemoryPercent; }
            set
            {
                if (_readingMemoryPercent == value)
                    return;
                _readingMemoryPercent = value;
                RaisePropertyChanged(ReadingMemoryPercentPropertyName);
            }
        }

        public RelayCommand RefreshClicked { get; private set; }
        private static bool CanRefresh() { return true; }
        public RelayCommand EraseClicked { get; private set; }
        private static bool CanErase() { return true; }
        public RelayCommand ExportClicked { get; private set; }
        private static bool CanExport() { return true; }
        public RelayCommand ExportAllClicked { get; private set; }
        private static bool CanExportAll() { return true; }
        public RelayCommand PreviousClicked { get; private set; }
        private bool CanShowPrevious()
        {
            return _currentRecording > 1;
        }
        public RelayCommand NextClicked { get; private set; }
        public bool CanShowNext()
        {
            return _currentRecording < _numberOfRecordings;
        }

        public ExplorerViewModel(IBrewMonitorService brewMonitorService, IMessageBoxService messageBoxService)
        {
            _brewMonService = brewMonitorService;
            _messageBoxService = messageBoxService;

            RefreshClicked = new RelayCommand(DownloadData, CanRefresh);
            EraseClicked = new RelayCommand(EraseMemory, CanErase);
            ExportClicked = new RelayCommand(ExportData, CanExport);
            PreviousClicked = new RelayCommand(ShowPrevious, CanShowPrevious);
            NextClicked = new RelayCommand(ShowNext, CanShowNext);
            ExportAllClicked = new RelayCommand(ExportAllData, CanExportAll);

            _recordings = new List<List<MemorySample>>();
            _sampleCollection = new ObservableCollection<ChartSample>();
            _brewMonService.ConnectionChanged += BrewMonServiceOnConnectionChanged;

            PlugInGridVisibility = _brewMonService.IsConnected() ? Visibility.Hidden : Visibility.Visible;
            ExplorerGridVisibility = _brewMonService.IsConnected() ? Visibility.Visible : Visibility.Hidden;
        }

        private void ShowPrevious()
        {
            if (_currentRecording > 1)
            {
                CurrentRecording--;
                DisplayCurrentRecording();
            }
        }

        private void ShowNext()
        {
            if (CurrentRecording < _recordings.Count)
            {
                CurrentRecording++;
                DisplayCurrentRecording();
            }
        }

        private void BrewMonServiceOnConnectionChanged(object sender, ConnectionChangedEventArgs connectionChangedEventArgs)
        {
            PlugInGridVisibility = connectionChangedEventArgs.Connected ? Visibility.Hidden : Visibility.Visible;
            ExplorerGridVisibility = connectionChangedEventArgs.Connected ? Visibility.Visible : Visibility.Hidden;

            if (connectionChangedEventArgs.Connected)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(DownloadData);
            }
            else
            {
                _downloadWorker.CancelAsync();
            }
        }

        private void DownloadData()
        {
            LoadingGridVisibility = Visibility.Visible;
            ChartGridVisibility = Visibility.Hidden;

            if (_downloading)
                return;
            _downloading = true;

            // ensure it isn't streaming (this would slow down our download)
            if (_brewMonService.IsConnected())
                _brewMonService.StopStreaming();

            _sampleCollection.Clear();
            _downloadWorker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            _downloadWorker.DoWork += ReadMemory;
            _downloadWorker.ProgressChanged += ReadMemoryProgressChanged;
            _downloadWorker.RunWorkerCompleted += ReadMemoryCompleted;
            _downloadWorker.RunWorkerAsync();
        }

        private void ExportData()
        {


            var dialog = new SaveFileDialog
            {
                DefaultExt = ".xlsx",
                FileName = DateTime.Now.ToString("s").Replace(":", "") + " - BrewMonitor project.xlsx",
                Filter = "Excel Spreadsheet (*.xlsx)|*.xlsx|Comma Separated Variable Files (*.csv)|*.csv|All files (*.*)|*.*",
            };
            var response = dialog.ShowDialog();
            if (response == true)
            {
                if (dialog.FileName != null)
                {
                    var ext = Path.GetExtension(dialog.FileName).ToUpper();
                    if (ext == ".XLSX")
                        SaveCurrentRecordingAsXlsx(dialog.FileName);
                    else
                        SaveCurrentRecordingAsCsv(dialog.FileName);
                    MessageBox.Show("Current recording exported successfully");
                }
            }
        }

        private void ExportAllData()
        {
            var dialog = new SaveFileDialog
            {
                DefaultExt = ".xlsx",
                FileName = DateTime.Now.ToString("s").Replace(":", "") + " - BrewMonitor project.xlsx",
                Filter = "Excel Spreadsheet (*.xlsx)|*.xlsx|All files (*.*)|*.*",
            };
            var response = dialog.ShowDialog();
            if (response == true)
            {
                if (dialog.FileName != null)
                {
                    SaveAllRecordingsAsXlsx(dialog.FileName);
                    MessageBox.Show("All recordings exported successfully");
                }
            }
        }

        private void SaveAllRecordingsAsXlsx(string fileName)
        {
            var wb = new XLWorkbook();
            for (var i = 0; i < _recordings.Count; i++)
            {
                var ws = wb.Worksheets.Add("Project " + i + 1);
                ws.Cell("A1").Value = "Time (Seconds)";
                ws.Cell("B1").Value = "Temperature (C)";
                ws.Cell("C1").Value = "Bubbles/min";

                // set the heading style
                var headerRange = ws.Range("A1:C1");
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.Black;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

                for (var j = 0; j < _recordings[i].Count; j++)
                {
                    ws.Cell(j + 2, "A").Value = _recordings[i][j].Seconds;
                    ws.Cell(j + 2, "B").Value = _recordings[i][j].Celcius;
                    ws.Cell(j + 2, "C").Value = _recordings[i][j].BubbleCount;
                }

                //Add thick borders to the contents of our spreadsheet
                // ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Dashed;

                ws.Columns().AdjustToContents();
            }
            wb.SaveAs(fileName);
        }

        private void SaveCurrentRecordingAsXlsx(string fileName)
        {
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Project " + CurrentRecording);
            ws.Cell("A1").Value = "Time (Seconds)";
            ws.Cell("B1").Value = "Temperature (C)";
            ws.Cell("C1").Value = "Bubbles/min";

            // set the heading style
            var headerRange = ws.Range("A1:C1");
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontColor = XLColor.Black;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

            for (var j = 0; j < _recordings[CurrentRecording - 1].Count; j++)
            {
                ws.Cell(j + 2, "A").Value = _recordings[CurrentRecording - 1][j].Seconds;
                ws.Cell(j + 2, "B").Value = _recordings[CurrentRecording - 1][j].Celcius;
                ws.Cell(j + 2, "C").Value = _recordings[CurrentRecording - 1][j].BubbleCount;
            }

            ws.Columns().AdjustToContents();

            wb.SaveAs(fileName);
        }

        private void SaveCurrentRecordingAsCsv(string fileName)
        {
            var csvString = "";
            foreach (var memorySample in _recordings[CurrentRecording - 1])
            {
                csvString += memorySample.Seconds + ",";
                csvString += memorySample.Celcius + ",";
                csvString += memorySample.BubbleCount + Environment.NewLine;
            }

            File.WriteAllText(fileName, csvString);
        }

        private void EraseMemory()
        {
            if (_messageBoxService.ShowMessage("Erase all contents on BrewMonitor?", "Erase"))
                _brewMonService.EraseMemory();

            DownloadData();
        }

        private void ReadMemory(object sender, DoWorkEventArgs e)
        {
            // clear the projects collection
            _recordings.Clear();
            ClearStatistics();

            // get the number of memory splits
            var config = _brewMonService.ReadConfiguration();
            var divisor = config.SampleRateDivisor;

            var bgw = (BackgroundWorker)sender;
            for (var i = 0; i < config.NumberOfSamples; i++)
            {
                if (bgw.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                var sample = _brewMonService.ReadMemory(i);
                if (sample.IsBlank)
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

            // update the number of recordings text
            NumberOfRecordings = _recordings.Count;

            // clear grid's observable collection
            _sampleCollection.Clear();

            if (_recordings.Any())
            {
                CurrentRecording = 1;
                DisplayCurrentRecording();
            }

            // hide the progress bars when we're done loading
            RaisePropertyChanged(SampleCollectionPropertyName);
            LoadingGridVisibility = Visibility.Hidden;
            ChartGridVisibility = Visibility.Visible;
            _downloading = false;
        }

        private void ClearStatistics()
        {
            DurationText = DurationFromSeconds(0);
            NumberOfSamples = 0;
            MaxTemperature = DoubleToTempString(0);
            MinTemperature = DoubleToTempString(0);
            AvgTemperature = DoubleToTempString(0);
            BubbleCount = 0;
            AverageBubbleRate = "0 bubbles/min";
            SampleInterval = "0 seconds";
        }

        private static string DurationFromSeconds(double seconds)
        {
            var t = TimeSpan.FromSeconds(seconds);
            if (t.TotalDays > 1)
                return Convert.ToInt32(t.TotalDays) + " days, " + t.Hours + " hours, " + t.Minutes + " minutes, " + t.Seconds + " seconds";
            if (t.TotalHours > 1)
                return t.Hours + " hours, " + t.Minutes + " minutes, " + t.Seconds + " seconds";
            return t.Minutes + " minutes, " + t.Seconds + " seconds";
        }

        private static string DoubleToTempString(double temp)
        {
            var val = Math.Truncate(temp * 1000) / 1000;
            var str = val.ToString(CultureInfo.InvariantCulture) + (char)176 + "C";
            return str;
        }

        private void DisplayCurrentRecording()
        {
            // clear grid's observable collection
            _sampleCollection.Clear();

            // currentrecording starts at 1, arrray starts at 0
            var currentRecordingArrayIndex = _currentRecording - 1;

            // check if there are any samples to display
            if (!_recordings[currentRecordingArrayIndex].Any())
            {
                ClearStatistics();
                return;
            }

            // calculate the sample interval text
            var divisor = _recordings[currentRecordingArrayIndex].First().Divisor;
            SampleInterval = Math.Pow(2, divisor - 1) + " seconds";

            // transform from memorysamples to chartsamples
            var chartSamples = _recordings[currentRecordingArrayIndex].Select(ms => new ChartSample
            {
                Bubbles = ms.BubbleCount,
                Temperature = ms.Celcius,
                Time = ms.Seconds
            }).ToList();

            // work out statistics
            MaxTemperature = DoubleToTempString(chartSamples.Max(s => s.Temperature));
            MinTemperature = DoubleToTempString(chartSamples.Min(s => s.Temperature));
            AvgTemperature = DoubleToTempString(chartSamples.Average(s => s.Temperature));
            BubbleCount = chartSamples.Sum(s => s.Bubbles);
            NumberOfSamples = chartSamples.Count;
            AverageBubbleRate = (BubbleCount / (chartSamples.Max(s => s.Time) + 1)).ToString("0.00") + " Bubbles/sec";

            //compact as necessary to speed up the grid
            //while (chartSampleArray.Count > 256)
            //{
            //    var compactedSamples = CompactSamples(chartSampleArray);
            //    chartSampleArray.Clear();
            //    chartSampleArray.AddRange(compactedSamples);
            //}

            // work out min and max for y axis
            if (chartSamples.Any())
            {
                var minTemp = chartSamples.Min(s => s.Temperature);
                var minBubbles = chartSamples.Min(s => s.Bubbles);
                var min = Math.Min(minTemp, minBubbles);
                var maxTemp = chartSamples.Max(s => s.Temperature);
                var maxBubbles = chartSamples.Max(s => s.Bubbles);
                var max = Math.Max(maxTemp, maxBubbles);
                ChartMin = (Convert.ToInt32(Math.Round((min) / 10)) * 10);
                ChartMax = (Convert.ToInt32(Math.Round((max + 10) / 10)) * 10);
            }
            else
            {
                ChartMin = 0;
                ChartMax = 30;
            }

            // scale the x axis for seconds
            TimeAxisHeader = "Time Elapsed (seconds)";

            var maxTimeUnits = chartSamples.Max(s => s.Time);
            var oneFifth = maxTimeUnits / 5;
            var roundedToTen = Math.Round(oneFifth / 10) * 10;
            TimeAxisInterval = Convert.ToInt32(roundedToTen);

            var numberOfSeconds = chartSamples.Max(s => s.Time) + 1;
            DurationText = DurationFromSeconds(numberOfSeconds);
            if (numberOfSeconds > 500)
            {
                // change to minutes
                TimeAxisHeader = "Time Elapsed (minutes)";
                foreach (var sample in chartSamples)
                {
                    sample.Time /= 60;
                }

                // re scale the x axis for minutes                    
                maxTimeUnits = chartSamples.Max(s => s.Time);
                oneFifth = maxTimeUnits / 5;
                roundedToTen = Math.Round(oneFifth / 5) * 5;
                TimeAxisInterval = Convert.ToInt32(roundedToTen);
            }

            var minutes = chartSamples.Max(s => s.Time);
            if (minutes > 500)
            {
                // change to hours
                TimeAxisHeader = "Time Elapsed (hours)";
                foreach (var sample in chartSamples)
                {
                    sample.Time /= 60;
                }

                // re scale the x axis for hours
                maxTimeUnits = chartSamples.Max(s => s.Time);
                oneFifth = maxTimeUnits / 5;
                roundedToTen = Math.Round(oneFifth / 5) * 5;
                TimeAxisInterval = Convert.ToInt32(roundedToTen);
            }

            var hours = chartSamples.Max(s => s.Time);
            if (hours > 192)
            {
                // change to days
                TimeAxisHeader = "Time Elapsed (days)";
                foreach (var sample in chartSamples)
                {
                    sample.Time /= 24;
                }

                // re scale the x axis for days
                maxTimeUnits = chartSamples.Max(s => s.Time);
                oneFifth = maxTimeUnits / 5;
                roundedToTen = Math.Round(oneFifth / 5) * 5;
                TimeAxisInterval = Convert.ToInt32(roundedToTen);
            }

            // copy across to the grid's observable collection
            foreach (var sample in chartSamples)
            {
                _sampleCollection.Add(sample);
            }
            TimeAxisMaxValue = _sampleCollection.Max(s => s.Time);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool managed)
        {
            if (!managed)
                return;
            _downloadWorker.CancelAsync();
            _downloadWorker.Dispose();
            _downloadWorker = null;
            _brewMonService.Dispose();
            _brewMonService = null;
        }
    }
}
