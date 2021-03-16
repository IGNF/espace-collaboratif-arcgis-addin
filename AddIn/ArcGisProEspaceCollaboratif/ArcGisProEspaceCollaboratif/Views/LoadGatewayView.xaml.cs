using System.Collections.Generic;
using System.Windows;

namespace ArcGisProEspaceCollaboratif.Views
{
    /// <summary>
    /// Logique d'interaction pour LoadGatewayView.xaml
    /// </summary>
    public partial class LoadGatewayView : Window
    {
        #region Constructors
        /// <summary>
        /// La classe du dialogue "Charger les couches de mon groupe" qui permet
        /// à l'utilisateur de choisir les différentes couches qu'il veut afficher
        /// dans sa carte ArcGIS pro
        /// </summary>
        public LoadGatewayView()
        {
            InitializeComponent();
        }
        #endregion

        #region Events
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        #endregion
    }
}
