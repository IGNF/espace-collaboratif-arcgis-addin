using ArcGisProEspaceCollaboratif.Views;
using ArcGisProEspaceCollaboratif.Utils;
using System.Windows.Input;
using ArcGisProEspaceCollaboratif.Core;
using System;

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

        public Client ConnexionServer { get; set; } = null;

        /// <summary>
        /// Le contexte de travail qui contient le résultat de la requete
        /// vers l'espace collaboratif Profil/GeoGroupes/Groupes/Thèmes/Attributs
        /// </summary>
        public Context Context { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Iniialisation du dialogue "Connexion à https://espacecollaboratif.ign.fr"
        /// </summary>
        /// <param name="uri"></param>
        public ConnectViewModel(Context context)
        {
            this.connectView = new ConnectView();
            this.Context = context;
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

        private bool AlwaysTrue() { return true; }

        /// <summary>
        /// L'utilisateur a cliqué sur le bouton "Connecter".
        /// Il faut sauvegarder le login et le password
        /// pour établir une connexion au service
        /// </summary>
        private void OnConnect()
        {
            this.Login = this.connectView.LoginTextBox.Text;
            this.Password = this.connectView.PasswordBox.Password;
            this.ConnexionServer = this.Connexion();
        }

        private Client Connexion()
        {
            Client connexionServer = new(
                        this.Uri,
                        this.Login,
                        this.Password
                    );
            try
            {
                logger.Info("Création de la connexion au serveur " + connexionServer.ToString());

                // Récupération du profil utilisateur
                this.Context.Profil = connexionServer.GetProfile();
                if (this.Context.Profil == null)
                {
                    string message = "Récupération du profil utilisateur impossible";
                    logger.Error(string.Format("Context.GetConnexionEspaceCollaboratif : {0}\n", message));
                    throw new ArgumentNullException(message);
                }

                // Affichage de la boite du choix du groupe à l'utilisateur
                if (!this.Context.DisplayFormChoiceGroup(ref connexionServer))
                {
                    return null;
                }

                // Affichage des infos suite à la connexion à l'Espace collaboratif
                this.Context.DisplayInformationsAfterConnection();

                return connexionServer;
            }
            catch (Exception erreurConnexion)
            {
                this.Password = "";

                switch (erreurConnexion.Message.ToString())
                {
                    case "(401) Unauthorized":
                        string message = "Login et/ou mot de passe incorrect(s)";
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.ERROR);
                        break;

                    case "Login inconnu":
                        message = string.Format("''{0}'' n'est pas un utilisateur enregistré dans un groupe de l'Espace collaboratif.", this.Login);
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.ERROR);
                        break;

                    case "no_group":
                        message = "Accès refusé. L'utilisateur n'appartient à aucun groupe.";
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.ERROR);
                        break;

                    default:
                        message = string.Format("Impossible d'accéder au service de l'Espace collaboratif à l'adresse suivante : {0}\n\nVeuillez contacter le support. Erreur : {1}\n", this.Uri, erreurConnexion.Message.ToString());
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.ERROR);
                        break;
                }
            }
            return connexionServer;
        }
    }
    #endregion
}
