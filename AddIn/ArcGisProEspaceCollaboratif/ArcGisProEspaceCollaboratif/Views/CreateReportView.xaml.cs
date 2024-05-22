using System.Windows;

namespace ArcGisProEspaceCollaboratif.Views
{
    /// <summary>
    /// Logique d'interaction pour CreateReportView.xaml
    /// </summary>
    public partial class CreateReportView : Window
    {
        /// <summary>
        /// La classe du dialogue "Créer un nouveau signalement"
        /// </summary>
        public CreateReportView()
        {
            /*
            Si cette fonction sort en erreur après le build, il faut supprimer le répertoire \obj
            qui est au même niveau que le dossier Resources et refaire un build de la solution
            */
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

    }
}
