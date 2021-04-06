using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Series;
using ResearchPlatform.Helpers;
using ResearchPlatform.Models;
using ResearchPlatform.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows.Input;

namespace ResearchPlatform.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        private static readonly string INPUT_FILE = "Input";

        private readonly IDialogCoordinator _dialogCoordinator;

        private Configuration _configuration;
        private Models.Input _input;

        private List<string> _inputFileList;
        private string _selectedInputFile;

        private ObservableCollection<string> _algorithmsResults;
        private string _selectedAlgorithmResult;

        private bool _inProgress = false;
        private List<Job> _jobResults;
        private List<Break> _breaksResults;

        private readonly Dictionary<string, Result> _allResDict;

        public Configuration Configuration
        {
            get => _configuration;
            set => SetProperty(ref _configuration, value);
        }
        public Models.Input Input
        {
            get => _input;
            set => SetProperty(ref _input, value);
        }

        public List<string> InputFileList
        {
            get => _inputFileList;
            set => SetProperty(ref _inputFileList, value);
        }

        public string SelectedInputFile
        {
            get => _selectedInputFile;
            set
            {
                if (SetProperty(ref _selectedInputFile, value))
                    ReadInputFile();
            }
        }
        public ObservableCollection<string> AlgorithmsResults
        {
            get => _algorithmsResults;
            set => SetProperty(ref _algorithmsResults, value);
        }

        public string SelectedAlgorithmResult
        {
            get => _selectedAlgorithmResult;
            set
            {
                if (SetProperty(ref _selectedAlgorithmResult, value))
                    SetProperResult(_selectedAlgorithmResult);
            }
        }

        private void SetProperResult(string selectedAlgorithmResult)
        {
            if (selectedAlgorithmResult != null)
            {
                JobResults = _allResDict[selectedAlgorithmResult].Jobs.Cast<Job>().ToList();
                BreaksResults = _allResDict[selectedAlgorithmResult].Breaks;
            }
        }

        public bool InProgress
        {
            get => _inProgress;
            set => SetProperty(ref _inProgress, value);
        }

        public List<Job> JobResults
        {
            get => _jobResults;
            set => SetProperty(ref _jobResults, value);
        }

        public List<Break> BreaksResults
        {
            get => _breaksResults;
            set => SetProperty(ref _breaksResults, value);
        }

        public PlotModel Model { get; private set; }

        public ICommand LaunchSettingsCommand { get; set; }
        public ICommand GenerateInputCommand { get; set; }
        public ICommand RunAlgorithmsCommand { get; set; }
        public ICommand RefreshFileListCommand { get; set; }

        public MainWindowViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;
            _configuration = Configuration.Create;

            LaunchSettingsCommand = new RelayCommand(new Action(LaunchSetting));
            GenerateInputCommand = new RelayCommand(new Action(GenerateInput));
            RunAlgorithmsCommand = new RelayCommand(new Action(RunAlgorithms));
            RefreshFileListCommand = new RelayCommand(new Action(() => { 
                InputFileList = GetInputFileList();
                SelectedInputFile = _inputFileList.First();
            }));

            _inputFileList = GetInputFileList();
            SelectedInputFile = _inputFileList.First();

            _algorithmsResults = new ObservableCollection<string>();
            _allResDict = new Dictionary<string, Result>();
            _breaksResults = new List<Break>();
            _selectedAlgorithmResult = null;

            PreparePlot();
        }

        private void PreparePlot()
        {
            // Create the plot model
            var tmp = new PlotModel { Title = "Simple example", Subtitle = "using OxyPlot" };

            // Create two line series (markers are hidden by default)
            var series1 = new LineSeries { Title = "Series 1", MarkerType = MarkerType.Circle };
            series1.Points.Add(new DataPoint(0, 0));
            series1.Points.Add(new DataPoint(10, 18));
            series1.Points.Add(new DataPoint(20, 12));
            series1.Points.Add(new DataPoint(30, 8));
            series1.Points.Add(new DataPoint(40, 15));

            var series2 = new LineSeries { Title = "Series 2", MarkerType = MarkerType.Square };
            series2.Points.Add(new DataPoint(0, 4));
            series2.Points.Add(new DataPoint(10, 12));
            series2.Points.Add(new DataPoint(20, 16));
            series2.Points.Add(new DataPoint(30, 25));
            series2.Points.Add(new DataPoint(40, 5));


            // Add the series to the plot model
            tmp.Series.Add(series1);
            tmp.Series.Add(series2);

            // Axes are created automatically if they are not defined

            // Set the Model property, the INotifyPropertyChanged event will make the WPF Plot control update its content
            this.Model = tmp;
        }

        private async void RunAlgorithms()
        {
            var progress = await _dialogCoordinator.ShowProgressAsync(this, "Info", Messages.CALCULATIONS_IN_PROGRESS);
            progress.SetIndeterminate();
            await Run();
            await progress.CloseAsync();
        }

        private async System.Threading.Tasks.Task Run()
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                SetupListOfResults(AlgorithmsManager.RunWith(Configuration, Input));
            });
        }

        private void SetupListOfResults(List<Dictionary<SearchTreeAlgorithm, Result>> allRes)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                _algorithmsResults.Clear();
            });

            _allResDict.Clear();
            BreaksResults.Clear();
            SelectedAlgorithmResult = null;

            var ahp = allRes[(int)MultiCriteriaAlgorithm.AHP];
            var pro = allRes[(int)MultiCriteriaAlgorithm.PROMETHEE];
            var ele = allRes[(int)MultiCriteriaAlgorithm.ELECTRE];
            var own = allRes[(int)MultiCriteriaAlgorithm.OwnWeights];

            AddResultsToList(ahp, "AHP");
            AddResultsToList(own, "Own Weights");
            AddResultsToList(pro, "PROMETHEE");
            AddResultsToList(ele, "ELECTRE");

            SelectedAlgorithmResult = _allResDict.First().Key;
        }

        private void AddResultsToList(Dictionary<SearchTreeAlgorithm, Result> results, string nameOfCriteriaAlg)
        {
            if (results.Count != 0)
            {
                foreach(var res in results)
                {
                    var name = $"{res.Key} + {nameOfCriteriaAlg}";
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        _algorithmsResults.Add(name);
                    });
                    _allResDict.Add(name, res.Value);
                }
            }
        }

        private List<string> GetInputFileList()
        {
            return Directory.GetFiles("./")
                .Where(filename => filename.Contains(INPUT_FILE))
                .Select(filename => filename.Split("./")[1])
                .ToList();
        }

        private void ReadInputFile()
        {
            if (SelectedInputFile != null)
            {
                using StreamReader r = new StreamReader(SelectedInputFile);
                string json = r.ReadToEnd();
                Input = JsonConvert.DeserializeObject<Models.Input>(json);
            }
        }

        private async void GenerateInput()
        {
            var progressBar = await _dialogCoordinator.ShowProgressAsync(this, "Info", Messages.GENERATING_MSG);

            var generator = InputGenerator.GetInstance();
            await generator.GenerateAsync(Configuration.Postcode);

            await progressBar.CloseAsync();

            if (generator.Input == null)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Error", Messages.POSTCODE_ERROR_MSG);
            } else
            {
                var serializerOptions = new JsonSerializerOptions() { 
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement, UnicodeRanges.LatinExtendedA)
                };

                var serializeInput = System.Text.Json.JsonSerializer.Serialize(generator.Input, serializerOptions);
                File.WriteAllText($"{INPUT_FILE}_{DateTime.Now:yyyy-MM-dd hhmmss}.json", serializeInput.ToString(), Encoding.UTF8);

                await _dialogCoordinator.ShowMessageAsync(this, "Info", Messages.POSTCODE_SAVE_MSG);
            }
        }

        private void LaunchSetting()
        {
            new SettingsWindow(_configuration, InputFileList).ShowDialog();
        }
    }
}
