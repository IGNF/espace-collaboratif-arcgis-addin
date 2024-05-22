using System.Windows;

namespace ArcGisProEspaceCollaboratif.Views
{
    /// <summary>
    /// Logique d'interaction pour FeedbackInformationView.xaml
    /// </summary>
    public partial class FeedbackInformationView : Window
    {
        /// <summary>
        /// La classe du dialogue "IGN Espace collaboratif"
        /// qui relaye toutes les informations envoyées à l'utilisateur
        /// comme les caractéristiques de connexion ou de création de remarques
        /// </summary>
        public FeedbackInformationView()
        {
            /*
            Si cette fonction sort en erreur après le build, il faut supprimer le répertoire \obj
            qui est au même niveau que le dossier Resources et refaire un build de la solution
            */
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
