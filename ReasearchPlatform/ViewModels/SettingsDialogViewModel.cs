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

namespace ResearchPlatform.ViewModels
{
    class SettingsDialogViewModel : ViewModelBase
    {
        private readonly string DISTANCES_FILE = "Distances";

        private Configuration _originalConfiguration;
        private Configuration _configuration;
        private readonly string _inputFile;
        private Models.Input _input;

        private IDialogCoordinator _dialogCoordinator;

        public ICommand SaveConfigurationCommand { get; set; }
        public ICommand ResetConfigurationCommand { get; set; }
        public ICommand GenerateDistancesCommand { get; set; }

        public ObservableCollection<int> Test { get; set; } = new ObservableCollection<int> { 10 };

        public Configuration Configuration
        {
            get => _configuration;
            set => SetProperty(ref _configuration, value);
        }

        public SettingsDialogViewModel(Configuration configuration, IDialogCoordinator dialogCoordinator, string inputFile)
        {
            _originalConfiguration = configuration;
            _dialogCoordinator = dialogCoordinator;
            _inputFile = inputFile;

            // make deep copy of configuration
            var json = System.Text.Json.JsonSerializer.Serialize(_originalConfiguration);
            _configuration = System.Text.Json.JsonSerializer.Deserialize<Configuration>(json);

            SaveConfigurationCommand = new RelayCommand(new Action(SaveConfiguration));
            ResetConfigurationCommand = new RelayCommand(new Action(ResetToDefaultConfiguration));
            GenerateDistancesCommand = new RelayCommand(new Action(GenerateDistances));
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
            //var progressBar = await _dialogCoordinator.ShowProgressAsync(this, "Info", Messages.GENERATING_MSG);
            ReadInputFile();
            //var generator = InputGenerator.GetInstance();
            //var distances = await generator.GetAllDistancesForAsync(_input.Base, _input.Jobs, _input.DistanceMatrix);

            //await progressBar.CloseAsync();

            //if (distances.Count == 0)
            //{
            //    await _dialogCoordinator.ShowMessageAsync(this, "Error", Messages.POSTCODE_ERROR_MSG);
            //}
            //else
            //{
            var serializerOptions = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement, UnicodeRanges.LatinExtendedA)
                };
                var maxPossibleDistance = _input.DistanceMatrix.Select(d => d.DistanceInMeters).Max();
                _input.Jobs.ForEach(job => job.Price += ((maxPossibleDistance) / 1000) * (3.15 + new Random().NextDouble() * 6.75));
                var serializeInput = System.Text.Json.JsonSerializer.Serialize(_input, serializerOptions);
                File.WriteAllText($"{"Input-bigger-prices"}_{DateTime.Now:yyyy-MM-dd hhmmss}.json", serializeInput.ToString(), Encoding.UTF8);

                await _dialogCoordinator.ShowMessageAsync(this, "Info", Messages.POSTCODE_SAVE_MSG);
            //}
        }

        private void ReadInputFile()
        {
            using StreamReader r = new StreamReader(_inputFile);
            string json = r.ReadToEnd();
            _input = JsonConvert.DeserializeObject<Models.Input>(json);
        }
    }
}
