using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Mapping;
using ArcGisProEspaceCollaboratif.Core;
using ArcGisProEspaceCollaboratif.Utils;
using ArcGisProEspaceCollaboratif.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace ArcGisProEspaceCollaboratif.ViewModels
{
    class GroupChoiceViewModel : ViewModelBase
    {
        #region Parameters
        /// <summary>
        /// L'instance du dialogue "Paramètres de travail"
        /// </summary>
        public GroupChoiceView groupChoiceView;

        /// <summary>
        /// Le profil de l'utilisateur
        /// </summary>
        public Profile Profile { get; set; }

        #endregion

        #region Constructors
        /// <summary>
        /// Initialisation du dialogue "Paramètres de travail"
        /// </summary>
        /// <param name="activeGroup"></param>
        /// <param name="profile"></param>
        public GroupChoiceViewModel(Profile profil)
        {
            this.Profile = profil;
            this.groupChoiceView = new GroupChoiceView();
            this.InitializeGroupChoiceView();
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

        /// <summary>
        /// Le nom du fichier shape et son chemin
        /// </summary>
        private Dictionary<string, string> NewShapeFile { get; set; }

        private string WorkZone { get; set; }

        private ObservableCollection<string> workZoneItemsSourceGroupComboBox = new();
        public ObservableCollection<string> WorkZoneItemsSourceGroupComboBox
        {
            get { return workZoneItemsSourceGroupComboBox; }
            set
            {
                workZoneItemsSourceGroupComboBox = value;
                NotifyPropertyChanged(nameof(WorkZoneItemsSourceGroupComboBox));
            }
        }

        private string WorkZoneSelectedItem;
        /// <summary>
        /// 
        /// </summary>
        public string WorkZoneSelectedItemComboBox {
            get { return WorkZoneSelectedItem; }
            set
            {
                WorkZoneSelectedItem = value;
                NotifyPropertyChanged(nameof(WorkZoneSelectedItemComboBox));
            }
        }

        #endregion

        #region Commands
        public ICommand BrowseButtonCmd { get { return new RelayCommand(OnBrowse, AlwaysTrue); } }

        /// <summary>
        /// L'utilisateur a cliqué sur le bouton "Parcourir".
        /// Il faut ouvrir le dialogue de sélection de fichier
        /// </summary>
        private void OnBrowse()
        {
            string fileName = "";
            List<string> Formats = new()
            {
                ".shp",
                ".SHP"
            };
            string defaultExtension = string.Format("{0}|{1}", Formats[0], Formats[1]);
            string filters = string.Format("ESRI ShapeFile (*{0},*{1}|*{2};*{3})", Formats[0], Formats[1], Formats[0], Formats[1]);
            Microsoft.Win32.OpenFileDialog dlg = new()
            {
                DefaultExt = defaultExtension,
                Filter = filters,
                Multiselect = false,
                Title = "Nouvelle zone de travail Shapefile"
            };
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                string extension = dlg.FileName.Substring(dlg.FileName.Length - 4, 4);
                if (!string.IsNullOrEmpty(extension))
                {
                    if (!Formats.Contains(extension))
                    {
                        string message = string.Format("Le fichier de type [{0}] n'est pas un fichier Shapefile.", extension);
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                            message,
                            Constantes.ERROR
                        );
                    }
                }
                fileName = dlg.SafeFileName.Replace(extension, "");
                this.NewShapeFile[fileName] = dlg.FileName;
                // Evitons les doublons
                if (!this.WorkZoneItemsSourceGroupComboBox.Contains(fileName))
                {
                    this.WorkZoneItemsSourceGroupComboBox.Insert(0, fileName);
                }
            }
            if (!string.IsNullOrEmpty(fileName))
            {
                if (this.WorkZoneItemsSourceGroupComboBox.Contains(fileName))
                {
                    this.WorkZone = fileName;
                    this.WorkZoneSelectedItemComboBox = fileName;
                }
            }           
        }

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
        /// Il faut sauvegarder le choix du profil et/ou importer/sauvegarder la zone de travail
        /// </summary>
        private async void OnRegister()
        {
            string zone = this.groupChoiceView.WorkZoneComboBox.Text;

            if (zone != null)
            {
                Helper.SaveWorkZone(zone);
            }

            // Récupération du nom du groupe que l'utilisateur a choisi
            int index = this.groupChoiceView.GroupComboBox.SelectedIndex;
            if (index == -1)
            {
                return;
            }
            string igGroup = this.Profile.Geogroupes[index].Id;
            string nomGroup = this.Profile.Geogroupes[index].Name;
            this.Profile.IdNameGroup = (igGroup, nomGroup);
            Helper.SaveActiveGroup(nomGroup);

            // Est-ce qu'un changement de groupe ou de zone de travail est intervenu au moment de la sauvegarde ?
            // Si oui, il faut supprimer l'ensemble des couches et le groupe
            DeleteMapsAndGroup(zone);

            // Création d'une nouvelle zone de travail si le dictionnaire est rempli avec un shape
            // que l'utilisateur a chargé avec le bouton 'Parcourir'
            if (this.NewShapeFile.Count > 0)
            {
                if (this.NewShapeFile.ContainsKey(zone))
                {
                    string pathToSource = this.NewShapeFile[zone];
                    if (string.IsNullOrEmpty(pathToSource))
                    {
                        string message = string.Format("Impossible d'importer le fichier {0}", pathToSource);
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.WARNING);
                    }
                    await Helper.AddLayer(pathToSource);
                    Helper.SaveWorkZone(zone);
                } 
            }  
        }

        private void DeleteMapsAndGroup(string userZone)
        {
            // TODO Si c'est un projet nouvellement créé, il faut vérifier si la table des tables existe
            // Le nom de la zone stocké dans le xml .../xxx_espaceco.xml
            string storedZone = Helper.LoadWorkZone();
            string storedGroup = Helper.LoadActiveGroup();
            bool bNewZone = storedZone != userZone && !(storedZone == null && userZone == "");
            bool bNewGroup = storedGroup != this.Profile.Group.Name;
            // Si rien n'a changé, on sort
            if (!bNewGroup && !bNewZone)
            {
                return;
            }
        }

        private bool AlwaysTrue() { return true; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        private void InitializeGroupChoiceView()
        {
            this.SetItemsSourceGroupComboBox();
            this.SetGroupSelectedItemComboBox();
            this.SetItemsSourceWorkZoneComboBox();
            this.SetWorkZoneSelectedItemComboBox();
        }
        /// <summary>
        /// 
        /// </summary>
        public void SetItemsSourceGroupComboBox()
        {
            // Ajout des noms de groupes trouvés pour l'utilisateur
            ObservableCollection<string> GroupNames = new ();
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
        public void SetGroupSelectedItemComboBox()
        {
            string groupeActif = Helper.LoadActiveGroup();
            if (this.GroupItemsSourceGroupComboBox.Contains(groupeActif))
            {
                this.GroupSelectedItemComboBox = groupeActif;
            }
            else
            {
                this.GroupSelectedItemComboBox = this.Profile.Geogroupes[0].Name;
            }           
        }

        public void SetItemsSourceWorkZoneComboBox()
        {
            this.WorkZone = Helper.LoadWorkZone();
            this.NewShapeFile = new Dictionary<string, string>();
            this.WorkZoneItemsSourceGroupComboBox = new ObservableCollection<string>
            {
                ""
            };
            // Quelles sont les couches de polygone qui existent dans la carte ?
            if (MapView.Active.Map == null)
            {
                string mess = "Pas de carte active, impossible de récupérer les couches de celle-ci";
                logger.Error(string.Format("GroupChoiceViewModel.SetItemsSourceWorkZoneComboBox : {0}\n", mess));
                throw new Exception(mess);
            }
            IReadOnlyList<Layer> mapLayers = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(l => l.ShapeType == esriGeometryType.esriGeometryPolygon).ToList();
            foreach (var layer in mapLayers)
            {
                string layerName = layer.Name;
                if (layerName == Helper.name_layer_Croquis_Polygone)
                {
                    continue;
                }
                if (!this.WorkZoneItemsSourceGroupComboBox.Contains(layerName))
                {
                    this.WorkZoneItemsSourceGroupComboBox.Add(layerName);
                }
            }
            
            if (!string.IsNullOrEmpty(this.WorkZone))
            {
                if (this.WorkZoneItemsSourceGroupComboBox.Contains(this.WorkZone))
                {
                    this.WorkZoneSelectedItemComboBox = this.WorkZone;
                }                
            }
        }

        public void SetWorkZoneSelectedItemComboBox()
        {
            this.WorkZoneSelectedItemComboBox = this.WorkZone;

        }
        #endregion
    }
}
