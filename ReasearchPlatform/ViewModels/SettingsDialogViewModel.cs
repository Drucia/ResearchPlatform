using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Text.Json;

namespace ResearchPlatform.ViewModels
{
    class SettingsDialogViewModel : ViewModelBase
    {
        private Configuration _originalConfiguration;
        private Configuration _configuration;
        private IDialogCoordinator _dialogCoordinator;

        public ICommand SaveConfigurationCommand { get; set; }
        public ICommand ResetConfigurationCommand { get; set; }

        public Configuration Configuration
        {
            get => _configuration;
            set => SetProperty(ref _configuration, value);
        }
        public List<string> PossibleComparisionValues {get; set;}

        public SettingsDialogViewModel(Configuration configuration, IDialogCoordinator dialogCoordinator)
        {
            _originalConfiguration = configuration;
            _dialogCoordinator = dialogCoordinator;

            // make deep copy of configuration
            var json = JsonSerializer.Serialize(_originalConfiguration);
            _configuration = JsonSerializer.Deserialize<Configuration>(json);

            PossibleComparisionValues = new List<string>{
                 "1", "2", "3", "4", "5", "6", "7", "8", "9", "1/2", "1/3", "1/4", "1/5", "1/6", "1/7", "1/8", "1/9"
            };

            SaveConfigurationCommand = new RelayCommand(new Action(SaveConfiguration));
            ResetConfigurationCommand = new RelayCommand(new Action(ResetToDefaultConfiguration));
        }

        private void SaveConfiguration()
        {
            _configuration.fillMatrix();
            _configuration.CopyTo(_originalConfiguration);
            _dialogCoordinator.ShowMessageAsync(this, "Info", Messages.SAVE_CONFIGURATION_MSG);
        }

        private void ResetToDefaultConfiguration()
        {
            Configuration = Configuration.Create;
            _dialogCoordinator.ShowMessageAsync(this, "Info", Messages.RESET_CONFIGURATION_MSG);
        }
    }
}
