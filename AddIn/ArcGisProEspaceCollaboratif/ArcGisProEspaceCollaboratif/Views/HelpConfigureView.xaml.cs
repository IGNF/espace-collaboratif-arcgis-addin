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
            /*
            Si cette fonction sort en erreur après le build, il faut supprimer le répertoire \obj
            qui est au même niveau que le dossier Resources et refaire un build de la solution
            */
            InitializeComponent();
        }

        private void SendResponse_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

    }
}
