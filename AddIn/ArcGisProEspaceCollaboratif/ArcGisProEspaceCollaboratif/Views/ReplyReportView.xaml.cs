using System.Windows;

namespace ArcGisProEspaceCollaboratif.Views
{
    /// <summary>
    /// Logique d'interaction pour ReplyReportView.xaml
    /// </summary>
    public partial class ReplyReportView : Window
    {
        /// <summary>
        /// La classe du dialogue "Répondre à un signalement"
        /// </summary>
        public ReplyReportView()
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
