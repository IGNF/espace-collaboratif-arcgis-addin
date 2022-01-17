using ArcGisProEspaceCollaboratif.Core;
using ArcGisProEspaceCollaboratif.Utils;
using ArcGisProEspaceCollaboratif.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ArcGisProEspaceCollaboratif.ViewModels
{
    class GroupChoiceViewModel : ViewModelBase
    {
        #region Parameters
        /// <summary>
        /// L'instance du dialogue "Choix du groupe"
        /// </summary>
        public GroupChoiceView groupChoiceView;

        /// <summary>
        /// Le profil de l'utilisateur
        /// </summary>
        public Profile Profile { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initialisation du dialogue "Choix du groupe"
        /// </summary>
        /// <param name="activeGroup"></param>
        /// <param name="profile"></param>
        public GroupChoiceViewModel(string activeGroup, Profile profil)
        {
            this.Profile = profil;
            this.groupChoiceView = new GroupChoiceView();
            this.InitializeGroupChoiceView(activeGroup);  
        }
        #endregion

        #region Bindings
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<string> GroupItemsSourceGroupComboBox { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string GroupSelectedItemComboBox { get; set; }

        #endregion

        #region Commands
        public ICommand CancelButtonCmd { get { return new RelayCommand(OnCancel, AlwaysTrue); } }

        /// <summary>
        /// L'utilisateur a cliqué sur le bouton "Annuler".
        /// Il faut fermer la boite de paramétrage
        /// </summary>
        private void OnCancel()
        {
            this.groupChoiceView.Close();
        }

        public ICommand RegisterButtonCmd { get { return new RelayCommand(OnRegister, AlwaysTrue); } }

        /// <summary>
        /// L'utilisateur a cliqué sur le bouton "Enregistrer".
        /// Il faut sauvegarder le choix du profil
        /// </summary>
        private void OnRegister()
        {
            int index = this.groupChoiceView.GroupComboBox.SelectedIndex;
            string igGroup = this.Profile.Geogroupes[index].Id;
            string nomGroup = this.Profile.Geogroupes[index].Name;
            this.Profile.IdNameGroup = (igGroup, nomGroup);
        }

        private bool AlwaysTrue() { return true; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        private void InitializeGroupChoiceView(string activeGroup)
        {
            this.SetItemsSourceGroupComboBox();
            this.SetGroupSelectedItemComboBox(activeGroup);
        }
        /// <summary>
        /// 
        /// </summary>
        public void SetItemsSourceGroupComboBox()
        {
            // Ajout des noms de groupes trouvés pour l'utilisateur
            ObservableCollection<string> GroupNames = new ObservableCollection<string>();
            foreach (GeoGroup geogroup in this.Profile.Geogroupes)
            {
                GroupNames.Add(geogroup.Name);
            }
            this.GroupItemsSourceGroupComboBox = GroupNames;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activeGroup"></param>
        public void SetGroupSelectedItemComboBox(string activeGroup)
        {
            if (!string.IsNullOrEmpty(activeGroup))
            {
                this.GroupSelectedItemComboBox = activeGroup;
            }
        }
        #endregion
    }
}
