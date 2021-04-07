using GalaSoft.MvvmLight.Command;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
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
        private string _axisYName;
        private int _stepSize;
        private double _minYValue;

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

        public string AxisYName
        {
            get => _axisYName;
            set => SetProperty(ref _axisYName, value);
        }

        public int StepSize
        {
            get => _stepSize;
            set => SetProperty(ref _stepSize, value);
        }

        public double MinYValue
        {
            get => _minYValue;
            set => SetProperty(ref _minYValue, value);
        }

        public ICommand LaunchSettingsCommand { get; set; }
        public ICommand GenerateInputCommand { get; set; }
        public ICommand RunAlgorithmsCommand { get; set; }
        public ICommand RefreshFileListCommand { get; set; }
        public ICommand DrawDurationPlotCommand { get; set; }
        public ICommand DrawNodesPlotCommand { get; set; }
        public ICommand DrawBreaksPlotCommand { get; set; }

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
            DrawDurationPlotCommand = new RelayCommand(new Action(() => PreparePlot("Duration")));
            DrawNodesPlotCommand = new RelayCommand(new Action(() => PreparePlot("Nodes")));
            DrawBreaksPlotCommand = new RelayCommand(new Action(() => PreparePlot("Breaks")));

            _inputFileList = GetInputFileList();
            SelectedInputFile = _inputFileList.First();

            _algorithmsResults = new ObservableCollection<string>();
            _allResDict = new Dictionary<string, Result>();
            _breaksResults = new List<Break>();
            _selectedAlgorithmResult = null;

            Series = new SeriesCollection();
        }
        public Func<double, string> Formatter { get; set; }
        public SeriesCollection Series { get; set; }

        private void PreparePlot(string name)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                Series.Clear();
                switch (name)
                {
                    case "Duration":
                        Series.Configuration = Mappers.Xy<Result>()
                                                    .X(resConfig => resConfig.AmountOfJobs)
                                                    .Y(resConfig => resConfig.Duration);
                        AxisYName = "Duration [ms]";
                        StepSize = 100;
                        MinYValue = double.NaN;
                        break;
                    case "Nodes":
                        Series.Configuration = Mappers.Xy<Result>()
                                                    .X(resConfig => resConfig.AmountOfJobs)
                                                    .Y(resConfig => resConfig.VisitedNodes);
                        AxisYName = "Visited nodes";
                        StepSize = 100;
                        MinYValue = double.NaN;
                        break;
                    case "Breaks":
                        Series.Configuration = Mappers.Xy<Result>()
                                                    .X(resConfig => resConfig.AmountOfJobs)
                                                    .Y(resConfig => resConfig.Breaks.Count);
                        AxisYName = "Breaks count";
                        StepSize = 10;
                        MinYValue = 0;
                        break;
                }
            });

            foreach (var result in _allResDict)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    Series.Add(new LiveCharts.Wpf.ColumnSeries
                    {
                        Title = result.Key,

                        Values = new ChartValues<Result>
                        {
                            result.Value
                        }
                    });
                });
            }
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
                PreparePlot("Duration");
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
