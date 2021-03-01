using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using ResearchPlatform.Models;
using ResearchPlatform.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ResearchPlatform.ViewModels
{
    class MainWindowViewModel
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly Configuration _configuration;

        public ICommand LaunchSettingsCommand { get; set; }

        public MainWindowViewModel(IDialogCoordinator coordinator)
        {
            _dialogCoordinator = coordinator;
            _configuration = Configuration.CurrentConfiguration;
            LaunchSettingsCommand = new RelayCommand(new Action(LaunchSetting));
        }

        private void LaunchSetting()
        {
            new SettingsWindow(_configuration).ShowDialog();
        }
    }
}
