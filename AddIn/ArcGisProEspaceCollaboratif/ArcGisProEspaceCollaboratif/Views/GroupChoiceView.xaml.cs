using System.Windows;

namespace ArcGisProEspaceCollaboratif.Views
{
    /// <summary>
    /// Logique d'interaction pour GroupChoiceView.xaml
    /// </summary>
    public partial class GroupChoiceView : Window
    {
        /// <summary>
        /// La classe du dialogue "Choix du groupe"
        /// </summary>
        public GroupChoiceView()
        {
            /*
            Si cette fonction sort en erreur après le build, il faut supprimer le répertoire \obj
            qui est au même niveau que le dossier Resources et refaire un build de la solution
            */
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
