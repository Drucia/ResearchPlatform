using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ResearchPlatform.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ResearchPlatform.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel(DialogCoordinator.Instance);
        }

        private void OnCommitBindingGroup(object sender, EventArgs e)
        {
            WeightsGrid.BindingGroup.CommitEdit();
        }

        private void OnCommitMatrixBindingGroup(object sender, EventArgs e)
        {
            MatrixGrid.BindingGroup.CommitEdit();
        }
    }
}
