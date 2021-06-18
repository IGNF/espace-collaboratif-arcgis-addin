using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace ArcGisProEspaceCollaboratif.Views
{
    /// <summary>
    /// Logique d'interaction pour SeeReportView.xaml
    /// </summary>
    public partial class SeeReportView : Window
    {
        public SeeReportView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

    }
}
