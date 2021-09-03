using ArcGisProEspaceCollaboratif.Utils;
using ArcGisProEspaceCollaboratif.Views;
using System.Windows.Input;

namespace ArcGisProEspaceCollaboratif.ViewModels
{
    class FeedbackInformationViewModel : ViewModelBase
    {
        #region Parameters
        /// <summary>
        /// L'instance du dialogue "IGN Espace collaboratif"
        /// La boite qui relaye les informations de connexion ou de création de remarques
        /// </summary>
        public FeedbackInformationView feedbackInformationView;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialisation de la boite d'informations
        /// </summary>
        public FeedbackInformationViewModel()
        {
            this.feedbackInformationView = new FeedbackInformationView();
        }
        #endregion

        #region Bindings
        /// <summary>
        ///  Affiche une image (logo) dans l'espace réservé de la fenêtre d'informations de l'espace collaboratif.
        ///  Par défaut mis à "No Logo"
        /// </summary>
        public string Logo { get; set; } = "/ArcGisProEspaceCollaboratif;component/Resources/no_logo.gif";

        /// <summary>
        /// Le message qui affiche à l'utilisateur les informations de connexion
        /// à l'Espace collaboratif après connexion et choix de son groupe de travail.
        /// ou les informations de création d'une nouvelle remarque.
        /// </summary>
        public string MessageFeedback { get; set; }
        #endregion

        #region Commands
        public ICommand OkButtonCmd { get { return new RelayCommand(OkButton, AlwaysTrue); } }

        /// <summary>
        /// L'utilisateur a cliqué sur le bouton "OK"
        /// La boite d'information est fermée
        /// </summary>
        private void OkButton()
        {
            this.feedbackInformationView.Close();
        }

        private bool AlwaysTrue() { return true; }
        #endregion
    }
}
