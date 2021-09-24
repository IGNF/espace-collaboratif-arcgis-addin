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
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CommentaryTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
