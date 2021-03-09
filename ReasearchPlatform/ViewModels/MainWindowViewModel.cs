using GalaSoft.MvvmLight.Command;
using ResearchPlatform.Models;
using ResearchPlatform.Views;
using System;
using System.Windows.Input;

namespace ResearchPlatform.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        private Configuration _configuration;
        public Configuration Configuration
        {
            get => _configuration;
            set => SetProperty(ref _configuration, value);
        }

        public ICommand LaunchSettingsCommand { get; set; }

        public MainWindowViewModel()
        {
            _configuration = Configuration.Create;

            LaunchSettingsCommand = new RelayCommand(new Action(LaunchSetting));
        }

        private void LaunchSetting()
        {
            new SettingsWindow(_configuration).ShowDialog();
        }
    }
}
