using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using ResearchPlatform.Models;
using System;
using System.Windows.Input;
using System.Text.Json;
using System.Collections.ObjectModel;
using ResearchPlatform.Helpers;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;

namespace ResearchPlatform.ViewModels
{
    class SettingsDialogViewModel : ViewModelBase
    {
        private readonly string DISTANCES_FILE = "Distances";
        private readonly string INPUT_FILE = "Input";

        private Configuration _originalConfiguration;
        private Configuration _configuration;
        private List<string> _inputFilesList;
        private string _selectedInputFile;
        private string _selectedInputFileForDistances;
        private Models.Input _input;
        private int _numberOfJobsToGenerate;
        private int _maxNumberOfJobsToGenerate;

        private IDialogCoordinator _dialogCoordinator;

        public ICommand SaveConfigurationCommand { get; set; }
        public ICommand ResetConfigurationCommand { get; set; }
        public ICommand GenerateDistancesCommand { get; set; }
        public ICommand GenerateInputCommand { get; set; }
        public ICommand GenerateMoreJobsCommand { get; set; }

        public ObservableCollection<int> Test { get; set; } = new ObservableCollection<int> { 10 };

        public Configuration Configuration
        {
            get => _configuration;
            set => SetProperty(ref _configuration, value);
        }

        public int NumberOfJobsToGenerate
        {
            get => _numberOfJobsToGenerate;
            set => SetProperty(ref _numberOfJobsToGenerate, value);
        }

        public int MaxNumberOfJobsToGenerate
        {
            get => _maxNumberOfJobsToGenerate;
            set => SetProperty(ref _maxNumberOfJobsToGenerate, value);
        }

        public List<string> InputFilesList
        {
            get => _inputFilesList;
            set => SetProperty(ref _inputFilesList, value);
        }

        public string SelectedInputFile
        {
            get => _selectedInputFile;
            set => SetProperty(ref _selectedInputFile, value);
        }

        public string SelectedInputFileForDistances
        {
            get => _selectedInputFileForDistances;
            set => SetProperty(ref _selectedInputFileForDistances, value);
        }

        public SettingsDialogViewModel(Configuration configuration, IDialogCoordinator dialogCoordinator, 
            List<string> inputFilesList)
        {
            _originalConfiguration = configuration;
            _dialogCoordinator = dialogCoordinator;
            _inputFilesList = inputFilesList;
            _selectedInputFile = inputFilesList.Count > 0 ? inputFilesList[0] : "";
            _selectedInputFileForDistances = inputFilesList.Count > 0 ? inputFilesList[0] : "";

            // make deep copy of configuration
            var json = System.Text.Json.JsonSerializer.Serialize(_originalConfiguration);
            _configuration = System.Text.Json.JsonSerializer.Deserialize<Configuration>(json);

            SaveConfigurationCommand = new RelayCommand(new Action(SaveConfiguration));
            ResetConfigurationCommand = new RelayCommand(new Action(ResetToDefaultConfiguration));
            GenerateDistancesCommand = new RelayCommand(new Action(GenerateDistances));
            GenerateInputCommand = new RelayCommand(new Action(GenerateWholeInput));
            GenerateMoreJobsCommand = new RelayCommand(new Action(GenerateMoreJobs));
        }

        private void SaveConfiguration()
        {
            Configuration.fillMatrix();
            Configuration.CopyTo(_originalConfiguration);
            _dialogCoordinator.ShowMessageAsync(this, "Info", Messages.SAVE_CONFIGURATION_MSG);
        }

        private void ResetToDefaultConfiguration()
        {
            Configuration = Configuration.Create;
            _dialogCoordinator.ShowMessageAsync(this, "Info", Messages.RESET_CONFIGURATION_MSG);
        }

        private async void GenerateDistances()
        {
            var progressBar = await _dialogCoordinator.ShowProgressAsync(this, "Info", Messages.GENERATING_MSG);
            ReadInputFile(_selectedInputFileForDistances);
            var generator = InputGenerator.GetInstance();
            _input.Logs.Clear();
            var distances = await generator.GetAllDistancesForAsync(_input.Base, _input.Nodes, _input.DistanceMatrix, _input.Logs);

            await progressBar.CloseAsync();

            if (distances.Count == 0)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Error", Messages.POSTCODE_ERROR_MSG);
            }
            else
            {
                var serializerOptions = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement, UnicodeRanges.LatinExtendedA)
                };

                _input.DistanceMatrix = _input.DistanceMatrix.Concat(distances).ToList();

                var serializeInput = System.Text.Json.JsonSerializer.Serialize(_input, serializerOptions);
                File.WriteAllText($"{DISTANCES_FILE}_{DateTime.Now:yyyy-MM-dd HHmmss}.json", serializeInput.ToString(), Encoding.UTF8);

                await _dialogCoordinator.ShowMessageAsync(this, "Info", Messages.POSTCODE_SAVE_MSG);
            }
        }

        private async void GenerateWholeInput()
        {
            var progressBar = await _dialogCoordinator.ShowProgressAsync(this, "Info", Messages.GENERATING_MSG);
            var generator = InputGenerator.GetInstance();
            await generator.GenerateAsync(Configuration.Postcode, _maxNumberOfJobsToGenerate);
            await progressBar.CloseAsync();

            if (generator.Input == null)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Error", Messages.POSTCODE_ERROR_MSG);
            }
            else
            {
                SaveInput(generator.Input);
            }
        }

        private async void GenerateMoreJobs()
        {
            var progressBar = await _dialogCoordinator.ShowProgressAsync(this, "Info", Messages.GENERATING_MSG);
            ReadInputFile(_selectedInputFile);
            var generator = InputGenerator.GetInstance();
            generator.GenerateJobs(_input, _numberOfJobsToGenerate);
            generator.GenerateClientsWithOpinions();
            await progressBar.CloseAsync();
            SaveInput(generator.Input);
            InputFilesList = GetInputFileList();
        }

        private async void SaveInput(Models.Input input)
        {
            var serializerOptions = new JsonSerializerOptions()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement, UnicodeRanges.LatinExtendedA)
            };

            var allDistances = CalculateCombination(input.Nodes.Count);

            var serializeInput = System.Text.Json.JsonSerializer.Serialize(input, serializerOptions);
            File.WriteAllText($"{INPUT_FILE}_{DateTime.Now:yyyy-MM-dd HH-mm}{(allDistances == input.DistanceMatrix.Count ? "" : "_damaged")}.json", 
                serializeInput.ToString(), Encoding.UTF8);

            await _dialogCoordinator.ShowMessageAsync(this, "Info", Messages.POSTCODE_SAVE_MSG);
        }

        private int CalculateCombination(int countOfNodes)
        {
            return (((countOfNodes - 1)*(countOfNodes))/2) + countOfNodes;
        }

        private void ReadInputFile(string inputFile)
        {
            using StreamReader r = new StreamReader(inputFile);
            string json = r.ReadToEnd();
            _input = JsonConvert.DeserializeObject<Models.Input>(json);
        }

        private List<string> GetInputFileList()
        {
            return Directory.GetFiles("./")
                .Where(filename => filename.Contains(INPUT_FILE))
                .Select(filename => filename.Split("./")[1])
                .ToList();
        }
    }
}
