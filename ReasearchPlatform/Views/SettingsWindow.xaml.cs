using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ResearchPlatform.Models;
using ResearchPlatform.ViewModels;
using System;
using System.Collections.Generic;

namespace ResearchPlatform.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        public SettingsWindow(Configuration configuration, List<string> inputFilesList)
        {
            InitializeComponent();

            DataContext = new SettingsDialogViewModel(configuration, DialogCoordinator.Instance, inputFilesList);
        }

        private void OnCommitBindingGroup(object sender, EventArgs e)
        {
            WeightsGrid.BindingGroup.CommitEdit();
        }

        private void OnCommitMatrixBindingGroup(object sender, EventArgs e)
        {
            MatrixPanel.BindingGroup.CommitEdit();
        }
    }
}
