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

        public ICommand LaunchSettingsCommand { get; set; }

        public MainWindowViewModel(IDialogCoordinator coordinator)
        {
            _dialogCoordinator = coordinator;
            LaunchSettingsCommand = new RelayCommand(new Action(LaunchSetting));
        }

        public async void LaunchSetting()
        {
            var dialog = new SettingsWindow(Configuration.CurrentConfiguration);

            await _dialogCoordinator.ShowMetroDialogAsync(this, dialog);

            await Task.Delay(13000);

            await _dialogCoordinator.HideMetroDialogAsync(this, dialog);
        }
    }
}
