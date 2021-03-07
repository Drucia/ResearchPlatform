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
        private static readonly string MATRIX_CHECK_START_ICON = "ExclamationTriangleSolid";
        private static readonly string MATRIX_CHECK_ERROR_ICON = "ThumbsDownSolid";
        private static readonly string MATRIX_CHECK_SUCCESS_ICON = "ThumbsUpSolid";
        private static readonly string MATRIX_CHECK_START_COLOR_ICON = "White";
        private static readonly string MATRIX_CHECK_ERROR_COLOR_ICON = "Red";
        private static readonly string MATRIX_CHECK_SUCCESS_COLOR_ICON = "Green";

        private Configuration _originalConfiguration;
        private Configuration _configuration;
        private bool _isMatrixConsistent = true;
        private string _matrixCheckIcon = MATRIX_CHECK_START_ICON;
        private string _matrixCheckColor = MATRIX_CHECK_START_COLOR_ICON;

        private IDialogCoordinator _dialogCoordinator;

        public ICommand SaveConfigurationCommand { get; set; }
        public ICommand ResetConfigurationCommand { get; set; }
        public ICommand CheckMatrixConsistencyCommand { get; set; }

        public ObservableCollection<int> Test { get; set; } = new ObservableCollection<int> { 10 };

        public Configuration Configuration
        {
            get => _configuration;
            set => SetProperty(ref _configuration, value);
        }

        public bool IsMatrixConsistent
        {
            get => _isMatrixConsistent;
            set => SetProperty(ref _isMatrixConsistent, value);
        }

        public string MatrixError { get; set; } = Messages.MATRIX_ERROR_MSG;
        public string MatrixCheckIcon
        {
            get => _matrixCheckIcon;
            set => SetProperty(ref _matrixCheckIcon, value);
        }
        public string MatrixCheckColor
        {
            get => _matrixCheckColor;
            set => SetProperty(ref _matrixCheckColor, value);
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
            CheckMatrixConsistencyCommand = new RelayCommand(new Action(CheckMatrixConsistency));
        }

        private void CheckMatrixConsistency()
        {
            Configuration.fillMatrix();
            IsMatrixConsistent = AlgorithmsManager.GetInstance().CheckMatrixConsistency(Configuration.ComparisionMatrix);

            MatrixCheckIcon = IsMatrixConsistent ? MATRIX_CHECK_SUCCESS_ICON : MATRIX_CHECK_ERROR_ICON;
            MatrixCheckColor = IsMatrixConsistent ? MATRIX_CHECK_SUCCESS_COLOR_ICON : MATRIX_CHECK_ERROR_COLOR_ICON;
        }

        private void SaveConfiguration()
        {
            CheckMatrixConsistency();
            if (IsMatrixConsistent)
            {
                Configuration.fillMatrix();
                Configuration.CopyTo(_originalConfiguration);
                _dialogCoordinator.ShowMessageAsync(this, "Info", Messages.SAVE_CONFIGURATION_MSG);
            } else
            {
                _dialogCoordinator.ShowMessageAsync(this, "Error", Messages.SAVE_CONFIGURATION_ERROR_MSG);
            }
        }

        private void ResetToDefaultConfiguration()
        {
            Configuration = Configuration.Create;
            IsMatrixConsistent = true;
            MatrixCheckIcon = MATRIX_CHECK_START_ICON;
            MatrixCheckColor = MATRIX_CHECK_START_COLOR_ICON;
            _dialogCoordinator.ShowMessageAsync(this, "Info", Messages.RESET_CONFIGURATION_MSG);
        }
    }
}
