using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using ResearchPlatform.Models;
using System;
using System.Windows.Input;
using System.Text.Json;
using System.Collections.ObjectModel;

namespace ResearchPlatform.ViewModels
{
    class SettingsDialogViewModel : ViewModelBase
    {
        private Configuration _originalConfiguration;
        private Configuration _configuration;
        private IDialogCoordinator _dialogCoordinator;

        public ICommand SaveConfigurationCommand { get; set; }
        public ICommand ResetConfigurationCommand { get; set; }

        public ObservableCollection<int> Test { get; set; } = new ObservableCollection<int> { 10 };

        public Configuration Configuration
        {
            get => _configuration;
            set => SetProperty(ref _configuration, value);
        }

        public SettingsDialogViewModel(Configuration configuration, IDialogCoordinator dialogCoordinator)
        {
            _originalConfiguration = configuration;
            _dialogCoordinator = dialogCoordinator;

            // make deep copy of configuration
            var json = JsonSerializer.Serialize(_originalConfiguration);
            _configuration = JsonSerializer.Deserialize<Configuration>(json);

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
