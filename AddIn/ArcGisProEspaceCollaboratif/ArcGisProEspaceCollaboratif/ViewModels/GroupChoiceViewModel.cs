using ArcGisProEspaceCollaboratif.Core;
using ArcGisProEspaceCollaboratif.Utils;
using ArcGisProEspaceCollaboratif.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public Profil _profile { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initialisation du dialogue "Choix du groupe"
        /// </summary>
        /// <param name="keyGeoportail"></param>
        /// <param name="activeGroup"></param>
        /// <param name="profile"></param>
        public GroupChoiceViewModel(string keyGeoportail, string activeGroup, Profil profile)
        {
            this._profile = profile;
            this.groupChoiceView = new GroupChoiceView();

            // Ajout des noms de groupes trouvés pour l'utilisateur
            ObservableCollection<string> tmp = new ObservableCollection<string>();
            foreach (GeoGroupe geogroup in this._profile.Geogroupes)
            {
                tmp.Add(geogroup.Name);
            }
            this.ItemsSourceGroupComboBox = tmp;

            if (!string.IsNullOrEmpty(activeGroup))
            {
                this.groupChoiceView.GroupComboBox.SelectedItem = activeGroup;
            }

            // Quelle clé GeoPortail pour l'utilisateur ?
            if (keyGeoportail == Constantes.DEMO || keyGeoportail == "")
            {
                this.groupChoiceView.NoRadioButton.IsChecked = true;
            }
            if (keyGeoportail != Constantes.DEMO && keyGeoportail != "")
            {
                this.groupChoiceView.YesRadioButton.IsChecked = true;
                KeyGeoportailTextBox = keyGeoportail;
            }
        }
        #endregion

        #region Bindings
        /// <summary>
        /// Clé géoportail de l'utilisateur
        /// </summary>
        public string KeyGeoportailTextBox { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<string> ItemsSourceGroupComboBox {get;set;}
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
        /// Il faut sauvegarder le choix du profil et de la clé Géoportail
        /// </summary>
        private void OnRegister()
        {
            int index = this.groupChoiceView.GroupComboBox.SelectedIndex;
            string igGroup = this._profile.Geogroupes[index].Id;
            string nomGroup = this._profile.Geogroupes[index].Name;
            string cleGeoportail = "";
            if (this.groupChoiceView.YesRadioButton.IsChecked == true)
            {
                cleGeoportail = KeyGeoportailTextBox;
            }
            if (this.groupChoiceView.NoRadioButton.IsChecked == true)
            {
                cleGeoportail = Constantes.DEMO;
            }
            this._profile.IdNameGroupKeyGeoPortail = (igGroup, nomGroup, cleGeoportail);
        }

        private bool AlwaysTrue() { return true; }
        #endregion
    }
}
