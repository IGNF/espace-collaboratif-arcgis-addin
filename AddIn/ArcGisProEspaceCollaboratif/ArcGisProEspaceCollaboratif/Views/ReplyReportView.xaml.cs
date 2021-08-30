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
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
