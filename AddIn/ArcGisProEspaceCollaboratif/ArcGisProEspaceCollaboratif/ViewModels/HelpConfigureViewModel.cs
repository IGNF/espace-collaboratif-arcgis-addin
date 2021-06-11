using ArcGisProEspaceCollaboratif.Utils;
using ArcGisProEspaceCollaboratif.Views;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ArcGisProEspaceCollaboratif.ViewModels
{
    class HelpConfigureViewModel
    {
        #region Parameters

        /// <summary>
        /// L'instance du dialogue "Configuration de l'AddIn Espace Collaboratif"
        /// </summary>
        public HelpConfigureView helpConfigureView;

        /// <summary>
        /// Le contexte de travail
        /// </summary>
        public Context Context { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialisation du dialogue "Répondre à un signalement"
        /// </summary>
        public HelpConfigureViewModel(Context context)
        {
            this.Context = context;
            this.helpConfigureView = new HelpConfigureView();
            this.InitializeHelpConfigureView();
        }

        #endregion

        #region Initialize Dialog

        /// <summary>
        /// Initialisation du contenu du dialogue de configuration de l'AddIn
        /// </summary>
        private void InitializeHelpConfigureView()
        {
            this.GetUrl();
            this.GetLogin();
            this.GetPagination();
            this.GetExtractionForGroup();
            this.GetActiveGroup();
            this.GetGeoportalKey();
        }
        #endregion

        #region Binding

        /// <summary>
        /// L'url vers le site de l'espace collaboratif
        /// Par défaut l'outil affiche le lien vers le site officiel
        /// </summary>
        public string Url { get; set; } = "";

        /// <summary>
        /// Le login par défaut de l'utilisateur
        /// </summary>
        public string Login { get; set; } = "";

        public ObservableCollection<uint> PaginationItemsSource { get; set; }

        public uint PaginationSelectedItem { get; set; }

        /// <summary>
        /// Le proxy pour accéder au site de l'espace collaboratif
        /// </summary>
        public string Proxy { get; set; } = "";

        /// <summary>
        /// Le nom du groupe pour extraction
        /// </summary>
        public string ExtractionForGroupContent { get; set; } = "";

        /// <summary>
        /// Le groupe que l'utilisateur à activé
        /// </summary>
        public string ActiveGroup { get; set; } = "";

        /// <summary>
        /// La clé Géoportail de l'utilisateur
        /// </summary>
        public string GeoportalKey { get; set; } = "";
        #endregion

        #region Commands

        public ICommand SendResponseButtonCmd { get { return new RelayCommand(OnSend, AlwaysTrue); } }

        /// <summary>
        /// L'utilisateur a cliqué sur le bouton "Enregistrer"
        /// La configuration de l'AddIn est enregistrée dans le fichier espaceco.xml
        /// </summary>
        private void OnSend()
        {
            Helper.SaveUrlhost(this.Url);
            Helper.SaveLogin(this.Login);
            Helper.SavePagination(Convert.ToUInt32(this.PaginationSelectedItem));
            if (this.helpConfigureView.ExtractionForGroupCheckBox.IsChecked == true)
            {
                Helper.SaveExtractionForGroup("true");
            }
            else
            {
                Helper.SaveExtractionForGroup("false");
            }
            Helper.SaveActiveGroup(this.ActiveGroup);
            Helper.SaveGeoportalKey(this.GeoportalKey);
        }

        private bool AlwaysTrue() { return true; }

        #endregion

        #region Methods

        private void GetUrl()
        {
            this.Url = Helper.LoadUrlhost();
        }
        /// <summary>
        /// En fonction du dernier login de l'utilisateur
        /// Si la chaine est vide
        ///  - la case à cocher est décochée
        ///  - la zone de texte est grisée
        /// sinon
        /// - la case à cocher est cochée
        /// - la zone de texte est remplie
        /// </summary>
        private void GetLogin()
        {
            string login = Helper.LoadLogin();
            this.Login = login;
            if (!string.IsNullOrEmpty(login))
            {
                this.helpConfigureView.LoginCheckBox.IsChecked = true;
            }
        }

        public void GetPagination()
        {
            // Ajout des noms de groupes trouvés pour l'utilisateur
            ObservableCollection<uint> pagination = new ObservableCollection<uint>()
            {
                0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100

            };         
            this.PaginationItemsSource = pagination;
            this.PaginationSelectedItem = Helper.LoadPagination();
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetExtractionForGroup()
        {
            if (Helper.LoadExtractionForGroup() == "true")
            {
                this.helpConfigureView.ExtractionForGroupCheckBox.IsChecked = true;
                this.ExtractionForGroupContent = this.Context.Profil.Group.Name;
            }
            
        }

        private void GetActiveGroup()
        {
            this.ActiveGroup = Helper.LoadActiveGroup();
        }

        private void GetGeoportalKey()
        {
            this.GeoportalKey = Helper.LoadGeoportalKey();
        }

        #endregion
    }
}
