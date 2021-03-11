using ArcGisProEspaceCollaboratif.Views;
using ArcGisProEspaceCollaboratif.Utils;
using System.Windows.Input;
using MvvmDialogs;

namespace ArcGisProEspaceCollaboratif.ViewModels
{
    public class ConnectViewModel : ViewModelBase
    {
        #region Parameters

        /// <summary>
        /// La boite de connexion au service de l'espace collaboratif
        /// qui demande un login et password
        /// </summary>
        public ConnectView connectView;

        /// <summary>
        /// 
        /// </summary>
        public bool? DialogResult { get; set; }

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
        public ConnectViewModel(string uri)
        {
            this.connectView = new ConnectView();
            this.Uri = uri;
        }
        #endregion

        #region Commands
        /// <summary>
        /// Modifie le titre de la boite par le nom du service
        /// auquel l'utilisateur se connecte
        /// </summary>
        public string Title
        {
            get { return string.Format("Connexion à {0}", this.Uri); }
        }

        public ICommand CancelCmd { get { return new RelayCommand(OnCancel, AlwaysTrue); } }

        /// <summary>
        /// L'utilisateur a cliqué sur le bouton "Annuler".
        /// Il faut fermer la boite de connexion au service
        /// </summary>
        private void OnCancel()
        {
            this.connectView.Close();
        }

        public ICommand ConnectCmd { get { return new RelayCommand(OnConnect, AlwaysTrue); } }

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

        public string LoginToolTip => "Entrez votre login utilisateur";

        public string PasswordToolTip => "Entrez votre password utilisateur";

        public string Error { get; set; } = "";

        #endregion
    }
}
