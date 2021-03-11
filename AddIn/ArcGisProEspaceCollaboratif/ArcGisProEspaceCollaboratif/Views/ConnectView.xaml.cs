using System.Windows;

namespace ArcGisProEspaceCollaboratif.Views
{
    /// <summary>
    /// Logique d'interaction pour ConnectView.xaml
    /// </summary>
    public partial class ConnectView : Window
    {
        public ConnectView()
        {
            InitializeComponent();
        }

        void connectButton_Click(object sender, RoutedEventArgs e)
        {
            // Accept the dialog and return the dialog result
            this.DialogResult = true;
        }

    }
}
