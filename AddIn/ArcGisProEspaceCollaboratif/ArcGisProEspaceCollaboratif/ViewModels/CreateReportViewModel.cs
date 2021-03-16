using ArcGisProEspaceCollaboratif.Core;
using ArcGisProEspaceCollaboratif.Utils;
using ArcGisProEspaceCollaboratif.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ArcGisProEspaceCollaboratif.ViewModels
{
    class CreateReportViewModel
    {
        #region Parameters
        /// <summary>
        /// L'instance du dialogue "Créer un nouveau signalement"
        /// </summary>
        public CreateReportView createReportView;

        /// <summary>
        /// 
        /// </summary>
        public Contexte Context { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int SketchNumber { get; set; }

        /// <summary>
        /// Liste des fichiers en pièce-jointe
        /// </summary>
        public List<string> ListAttachedFiles { get; set; }

        /// <summary>
        /// Taille maximale des fichiers en pièce-jointe
        /// </summary>
        public int MaxSizeAttachedFile { get; set; }

        /// <summary>
        /// Le groupe préféré de l'utilisateur car il peut changer
        /// en cours de saisie des signalements
        /// </summary>
        public string PreferredGroup { get; set; } = "";

        /// <summary>
        /// Liste des thèmes préférés de l'utilisateur cochés
        /// lors de la saisie d'un nouveau signalement
        /// et présents dans le fichier de configuration espace_co.xml
        /// </summary>
        public List<string> ListPreferredThemes { get; set; } = new List<string>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initialisation du dialogue "Créer un nouveau signalement"
        /// </summary>
        public CreateReportViewModel(Contexte context)
        {
            this.Context = context;
            this.ListPreferredThemes = Helper.Load_PreferredThemes();
            this.PreferredGroup = Helper.Load_PreferredGroup();
            this.createReportView = new CreateReportView();
            InitializeCreateReportView();
        }
        #endregion

        #region Bindings
        /// <summary>
        /// 
        /// </summary>
        public string UserProfileHeaderGroupBox { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<string> GroupItemsSourceComboBox { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string MessageTextBox { get; set; }
        #endregion

        #region Class
        #endregion

        #region Commands
        public ICommand SendButtonCmd { get { return new RelayCommand(OnSend, AlwaysTrue); } }

        /// <summary>
        /// Import des couches sélectionnées par l'utilisateur dans ArcGIS
        /// </summary>
        private void OnSend()
        {

        }

        private bool AlwaysTrue() { return true; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        private void InitializeCreateReportView()
        {
            this.UserProfileHeaderGroupBox = this.SetUserProfileHeaderGroupBox();
            this.GroupItemsSourceComboBox = this.SetGroupItemsSourceComboBox();
            this.SetGroupSelectedItemComboBox();
            this.MessageTextBox = this.SetMessageTextBox();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string SetUserProfileHeaderGroupBox()
        {
            string textGroupBox = "";
            if (string.IsNullOrEmpty(this.Context.Profil.Group.Name))
            {
                textGroupBox = string.Format("{0} (Profil par défaut)", this.Context.Profil.Author.Nom);
            }
            else
            {
                textGroupBox = string.Format("{0} ({1})", this.Context.Profil.Author.Nom, this.Context.Profil.Group.Name);
            }
            return textGroupBox;
        }

        /// <summary>
        /// 
        /// </summary>
        private ObservableCollection<string> SetGroupItemsSourceComboBox()
        {
            ObservableCollection<string> listGroup = new ObservableCollection<string>
            {
                Constantes.AUCUN
            };
            foreach (GeoGroupe geogroupe in this.Context.Profil.Geogroupes)
            {
                listGroup.Add(geogroupe.Name);
            }
            return listGroup;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string SetMessageTextBox()
        {
            string message = "";
            if (!string.IsNullOrEmpty(this.PreferredGroup))
            {
                message = this.Context.Profil.Geogroupes.Find(x => x.Name.Equals(PreferredGroup)).CommentaryGeorem;
            }
            else
            {
                message = this.Context.Profil.Geogroupes.Find(x => x.Name.Equals(this.Context.Groupeactif)).CommentaryGeorem;
            }
            return message;
        }

        private void SetGroupSelectedItemComboBox()
        {
            // Par défaut la liste des groupes de la combobox est positionnée sur "Aucun"
            string item = Constantes.AUCUN;

            // ou sur le groupe préféré
            if (!string.IsNullOrEmpty(this.PreferredGroup))
            {
                // Si le groupe préféré fait parti de la liste des groupes utilisateur
                if (this.Context.Profil.Geogroupes.FindIndex(x => x.Name.Equals(PreferredGroup)) != -1)
                {
                    item = PreferredGroup;
                }
            }
            else
            {
                // ou sur le groupe actif qui fait parti de la liste des groupes utilisateur
                if (!string.IsNullOrEmpty(this.Context.Groupeactif))
                {
                    item = this.Context.Groupeactif;
                }
            }
            this.createReportView.GroupComboBox.SelectedItem = item;
        }
        #endregion
    }
}
