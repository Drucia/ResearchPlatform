﻿using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ResearchPlatform.ViewModels
{
    class SettingsDialogViewModel : ViewModelBase
    {
        private Configuration _configuration;

        public Configuration Configuration
        {
            get => _configuration;
            set => SetProperty(ref _configuration, value);
        }
        public List<string> PossibleComparisionValues {get; set;}

        public SettingsDialogViewModel(Configuration configuration)
        {
            _configuration = configuration;

            PossibleComparisionValues = new List<string>{
                 "1", "2", "3", "4", "5", "6", "7", "8", "9", "1/2", "1/3", "1/4", "1/5", "1/6", "1/7", "1/8", "1/9"
            };
        }
    }
}