using GalaSoft.MvvmLight.Command;
using ResearchPlatform.Models;
using ResearchPlatform.Views;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ResearchPlatform.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        private Configuration _configuration;
        private Input _input;
        public Configuration Configuration
        {
            get => _configuration;
            set => SetProperty(ref _configuration, value);
        }
        public Input Input
        {
            get => _input;
            set => SetProperty(ref _input, value);
        }

        public ObservableCollection<Job> Jobs { get; set; } = new ObservableCollection<Job>(Job.GetSimpleList());

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
