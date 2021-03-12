using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
