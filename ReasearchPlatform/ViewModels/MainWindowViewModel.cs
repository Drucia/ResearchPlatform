using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using ResearchPlatform.Helpers;
using ResearchPlatform.Input;
using ResearchPlatform.Models;
using ResearchPlatform.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows.Input;

namespace ResearchPlatform.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        private static readonly string CENTRAL_NODES_FILE = "CentralNode.json";
        private static readonly string GENERATED_NODES_AROUND_FILE = "NodesAround.json";
        private static readonly string INPUT_FILE = "Input.json";

        private IDialogCoordinator _dialogCoordinator;

        private Configuration _configuration;
        private Models.Input _input;

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

        public ICommand LaunchSettingsCommand { get; set; }
        public ICommand GenerateInputCommand { get; set; }

        public MainWindowViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;
            _configuration = Configuration.Create;

            LaunchSettingsCommand = new RelayCommand(new Action(LaunchSetting));
            GenerateInputCommand = new RelayCommand(new Action(GenerateInput));
        }

        private async void GenerateInput()
        {
            //var node = await Fetcher.FetchCityNodeFromPostcodeAsync(Configuration.Postcode);
            //var nodes = await Fetcher.FetchCitiesNodesAroundNodeAsync(node);
            var generator = InputGenerator.GetInstance();
            await generator.GenerateAsync(Configuration.Postcode);

            if (generator.Input == null)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Error", Messages.POSTCODE_ERROR_MSG);
            } else
            {
                var serializerOptions = new JsonSerializerOptions() { 
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement, UnicodeRanges.LatinExtendedA)
                };

                //var serializeNode = JsonSerializer.Serialize<Node>(node, serializerOptions);
                //var serializeNodes = JsonSerializer.Serialize<List<Node>>(nodes, serializerOptions);

                //File.AppendAllText(CENTRAL_NODES_FILE, serializeNode.ToString(), Encoding.UTF8);
                //File.AppendAllText(GENERATED_NODES_AROUND_FILE, serializeNodes.ToString(), Encoding.UTF8);
                var serializeInput = JsonSerializer.Serialize<Models.Input>(generator.Input, serializerOptions);
                File.AppendAllText(INPUT_FILE, serializeInput.ToString(), Encoding.UTF8);

                await _dialogCoordinator.ShowMessageAsync(this, "Info", Messages.POSTCODE_SAVE_MSG);
            }
        }

        private void LaunchSetting()
        {
            new SettingsWindow(_configuration).ShowDialog();
        }
    }
}
