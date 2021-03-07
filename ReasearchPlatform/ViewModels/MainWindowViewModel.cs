using GalaSoft.MvvmLight.Command;
using ResearchPlatform.Models;
using ResearchPlatform.Views;
using System;
using System.Windows.Input;

namespace ResearchPlatform.ViewModels
{
    class MainWindowViewModel
    {
        private Configuration _configuration;

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
