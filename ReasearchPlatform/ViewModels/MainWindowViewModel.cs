using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;
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
            var mySettings = new MetroDialogSettings
            {
                AffirmativeButtonText = "Yes"
            };

            var cd = new CustomDialog
            {
                VerticalAlignment = VerticalAlignment.Top,
                VerticalContentAlignment = VerticalAlignment.Top
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, cd, mySettings);
        }
    }
}
