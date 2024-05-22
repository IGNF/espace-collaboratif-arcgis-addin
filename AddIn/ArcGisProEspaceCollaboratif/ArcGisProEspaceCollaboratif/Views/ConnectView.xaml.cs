using System.IO;
using System.Windows;

namespace ArcGisProEspaceCollaboratif.Views
{
    /// <summary>
    /// Logique d'interaction pour ConnectView.xaml
    /// </summary>
    public partial class ConnectView : Window
    {
        /// <summary>
        /// La classe du dialogue "Connexion à https://espacecollaboratif.ign.fr"
        /// </summary>
        public ConnectView()
        {
            /*
            Si cette fonction sort en erreur après le build, il faut supprimer le répertoire \obj
            qui est au même niveau que le dossier Resources et refaire un build de la solution
            */

            this.InitializeComponent();
        }

        /// <summary>
        /// Le bouton 'Se connecter" indique true si l'utilisateur a cliqué dessus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            // Accept the dialog and return the dialog result
            this.DialogResult = true;
        }

    }
}
