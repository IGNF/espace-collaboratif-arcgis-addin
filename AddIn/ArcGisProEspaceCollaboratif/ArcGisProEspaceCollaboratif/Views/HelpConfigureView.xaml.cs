using System.Windows;

namespace ArcGisProEspaceCollaboratif.Views
{
    /// <summary>
    /// Logique d'interaction pour HelpConfigureView.xaml
    /// </summary>
    public partial class HelpConfigureView : Window
    {
        public HelpConfigureView()
        {
            InitializeComponent();
        }

        private void SendResponse_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

    }
}
