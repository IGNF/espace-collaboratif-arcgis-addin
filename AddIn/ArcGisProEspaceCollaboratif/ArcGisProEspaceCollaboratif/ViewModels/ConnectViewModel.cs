using ArcGisProEspaceCollaboratif.Views;
using ArcGisProEspaceCollaboratif.Utils;
using System.Windows.Input;

namespace ArcGisProEspaceCollaboratif.ViewModels
{
    public class ConnectViewModel : ViewModelBase
    {
        #region Parameters

        /// <summary>
        /// L'instance du dialogue "Connexion à https://espacecollaboratif.ign.fr"
        /// </summary>
        public ConnectView connectView;

        /// <summary>
        /// Le login pas défaut récupéré
        /// dans le fichier xml de configuration espace_co.xml
        /// qui apparait à l'ouverture de la boite de connexion
        /// </summary>
        public string Login { get; set; } = "";

        /// <summary>
        /// Le password de l'utilisateur pour se loguer au service
        /// </summary>
        public string Password { get; set; } = "";

        /// <summary>
        /// L'URL de connexion au service 
        /// </summary>
        public string Uri { get; set; } = "";
        #endregion

        #region Constructors
        /// <summary>
        /// Iniialisation du dialogue "Connexion à https://espacecollaboratif.ign.fr"
        /// </summary>
        /// <param name="uri"></param>
        public ConnectViewModel()
        {
            this.connectView = new ConnectView(); 
        }
        #endregion

        #region Bindings
        /// <summary>
        /// Modifie le titre de la boite par le nom du service
        /// auquel l'utilisateur se connecte
        /// </summary>
        public string Title
        {
            get { return string.Format("Connexion à {0}", this.Uri); }
        }

        /// <summary>
        /// Logo de l'établissement appliqué au dialogue "Connexion à https://espacecollaboratif.ign.fr"
        /// </summary>
        public string Logo { get; set; } = "/ArcGisProEspaceCollaboratif;component/Resources/LogoIGN.gif";

        /// <summary>
        /// Info-bulle sur la zone de texte du login
        /// </summary>
        public static string LoginToolTip => "Entrez votre login utilisateur";

        /// <summary>
        /// Info-bulle sur la zone de texte du password
        /// </summary>
        public static string PasswordToolTip => "Entrez votre password utilisateur";

        /// <summary>
        /// 
        /// </summary>
        public string Error { get; set; } = "";
        #endregion

        #region Commands
        public ICommand CancelButtonCmd { get { return new RelayCommand(OnCancel, AlwaysTrue); } }

        /// <summary>
        /// L'utilisateur a cliqué sur le bouton "Annuler".
        /// Il faut fermer la boite de connexion au service
        /// </summary>
        private void OnCancel()
        {
            this.connectView.Close();
        }

        public ICommand ConnectButtonCmd { get { return new RelayCommand(OnConnect, AlwaysTrue); } }

        /// <summary>
        /// L'utilisateur a cliqué sur le bouton "Connecter".
        /// Il faut sauvegarder le login et le password
        /// pour établir une connexion au service
        /// </summary>
        private void OnConnect()
        {
            this.Login = this.connectView.LoginTextBox.Text;
            this.Password = this.connectView.PasswordBox.Password;
        }

        private bool AlwaysTrue() { return true; }
        #endregion
    }
}
