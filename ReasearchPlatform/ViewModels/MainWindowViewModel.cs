using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
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
using System.Threading;
using System.Windows.Input;

namespace ResearchPlatform.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        private static readonly string INPUT_FILE = "Input";

        private IDialogCoordinator _dialogCoordinator;

        private Configuration _configuration;
        private Models.Input _input;

        private List<string> _inputFileList;
        private string _selectedInputFile;
        private bool _inProgress = false;
        private List<Job> _results;

        private AlgorithmsManager _algManager;

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

        public bool InProgress
        {
            get => _inProgress;
            set => SetProperty(ref _inProgress, value);
        }

        public List<Job> Results
        {
            get => _results;
            set => SetProperty(ref _results, value);
        }

        public ICommand LaunchSettingsCommand { get; set; }
        public ICommand GenerateInputCommand { get; set; }
        public ICommand RunAlgorithmsCommand { get; set; }

        public MainWindowViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;
            _configuration = Configuration.Create;

            LaunchSettingsCommand = new RelayCommand(new Action(LaunchSetting));
            GenerateInputCommand = new RelayCommand(new Action(GenerateInput));
            RunAlgorithmsCommand = new RelayCommand(new Action(RunAlgorithms));

            _inputFileList = GetInputFileList();
            SelectedInputFile = _inputFileList.First();
        }

        private void RunAlgorithms()
        {
            InProgress = true;
            var allRes = AlgorithmsManager.RunWith(Configuration, Input);
            Results = allRes[(int)MultiCriteriaAlgorithm.AHP][SearchTreeAlgorithm.DFS].Jobs.Cast<Job>().ToList();
            InProgress = false;
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
                using (StreamReader r = new StreamReader(SelectedInputFile))
                {
                    string json = r.ReadToEnd();
                    Input = JsonSerializer.Deserialize<Models.Input>(json);
                }
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

                var serializeInput = JsonSerializer.Serialize<Models.Input>(generator.Input, serializerOptions);
                File.WriteAllText($"{INPUT_FILE}_{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}.json", serializeInput.ToString(), Encoding.UTF8);

                await _dialogCoordinator.ShowMessageAsync(this, "Info", Messages.POSTCODE_SAVE_MSG);
            }
        }

        private void LaunchSetting()
        {
            new SettingsWindow(_configuration).ShowDialog();
        }
    }
}
