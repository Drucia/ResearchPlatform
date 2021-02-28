using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ResearchPlatform.Models;
using ResearchPlatform.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ResearchPlatform.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : CustomDialog
    {
        public SettingsWindow(Configuration configuration)
        {
            InitializeComponent();

            DataContext = new SettingsDialogViewModel(configuration);
        }
    }
}
