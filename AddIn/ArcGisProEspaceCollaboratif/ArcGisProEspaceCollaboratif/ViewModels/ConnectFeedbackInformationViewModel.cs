using ArcGisProEspaceCollaboratif.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGisProEspaceCollaboratif.ViewModels
{
    class ConnectFeedbackInformationViewModel : ViewModelBase
    {
        #region Parameters
        /// <summary>
        /// La boite qui relaye les informations de connexion ou de création de remarques
        /// </summary>
        public ConnectFeedbackInformationView connectFeedbackInformationView;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialisation de la boite d'informations
        /// </summary>
        public ConnectFeedbackInformationViewModel()
        {
            this.connectFeedbackInformationView = new ConnectFeedbackInformationView();
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
    }
}
