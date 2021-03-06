using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ResearchPlatform.Models;
using ResearchPlatform.ViewModels;
using System;

namespace ResearchPlatform.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        public SettingsWindow(Configuration configuration)
        {
            InitializeComponent();

            DataContext = new SettingsDialogViewModel(configuration, DialogCoordinator.Instance);
        }

        private void OnCommitBindingGroup(object sender, EventArgs e)
        {
            WeightsGrid.BindingGroup.CommitEdit();
        }
    }
}
