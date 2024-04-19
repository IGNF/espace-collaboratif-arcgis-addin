using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Mapping;
using ArcGisProEspaceCollaboratif.Utils;
using ArcGisProEspaceCollaboratif.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace ArcGisProEspaceCollaboratif.ViewModels
{
    class HelpConfigureViewModel : ViewModelBase
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
        /// Initialisation du contenu des items du dialogue de configuration de l'AddIn
        /// </summary>
        private void InitializeHelpConfigureView()
        {
            this.DisplayUrl();
            this.DisplayLogin();
            this.DisplayDatesExtraction();
            this.DisplaySpatialFilter();
            //this.DisplayProxy();
            this.DisplayActiveGroup();
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

        public ObservableCollection<string> SpatialFilterItemsSource { get; set; }

        /// <summary>
        /// La couche sélectionnée pour le filtrage des signalements à extraire
        /// </summary>
        public string SpatialFilterSelectedItem { get; set; }

        /// <summary>
        /// La date de début d'extraction des signalements à afficher sur le calendrier
        /// </summary>
        public DateTime ExtractDisplayStartDate { get; set; }

        /// <summary>
        /// La date de fin d'extraction des signalements à afficher sur le calendrier
        /// </summary>
        public DateTime ExtractDisplayEndDate { get; set; }

        /// <summary>
        /// La date de début d'extraction des signalements sélectionnée dans le calendrier
        /// </summary>
        public DateTime ExtractSelectedStartDate { get; set; }

        /// <summary>
        /// La date de fin d'extraction des signalements sélectionnée dans le calendrier
        /// </summary>
        public DateTime ExtractSelectedEndDate { get; set; }

        /// <summary>
        /// Le proxy pour accéder au site de l'espace collaboratif
        /// </summary>
        public string Proxy { get; set; } = "";

        /// <summary>
        /// Le groupe que l'utilisateur a activé
        /// </summary>
        public string ActiveGroup { get; set; } = "";

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

            // Par défaut, on enregistre un login à vide
            Helper.SaveLogin("");
            // Sauf si l'utilisateur à cocher la case "Login par défaut"
            if (this.helpConfigureView.LoginCheckBox.IsChecked == true)
            {
                Helper.SaveLogin(this.Login);
            }

            // Par défaut, on enregistre une date de début et de fin d'extraction au 01/01/2000
            DateTime defaultDate = new DateTime(2000, 1, 1);
            //Helper.SaveStartDateExtraction(defaultDate);
            //Helper.SaveEndDateExtraction(defaultDate);
            // Sauf si l'utilisateur à cocher "Date de début"
            if (this.helpConfigureView.ExtractCheckBoxStart.IsChecked == true)
            {
                Helper.SaveStartDateExtraction(this.ExtractSelectedStartDate.ToString());
            }
            else
            {
                Helper.SaveStartDateExtraction("");
            }
            // et/ou si l'utilisateur à cocher "Date de fin"
            
            if (this.helpConfigureView.ExtractCheckBoxEnd.IsChecked == true)
            {
                Helper.SaveEndDateExtraction(this.ExtractSelectedEndDate.ToString());
            }
            else
            {
                Helper.SaveEndDateExtraction("");
            }

            // Par défaut, on enregistre un filtre spatial à vide
            Helper.SaveNameLayerForSpatialFilter("");
            // Sauf si l'utilisateur à cocher la case "Couche pour filtrage spatial"
            if (this.helpConfigureView.SpecialFilterCheckBox.IsChecked == true)
            {
                Helper.SaveNameLayerForSpatialFilter(this.SpatialFilterSelectedItem);
            }

            // Par défaut, on enregistre un proxy à vide
            Helper.SaveProxy("");
            // Sauf si l'utilisateur à cocher "Proxy"
            /*if (this.helpConfigureView.ProxyCheckBox.IsChecked == true)
            {
                Helper.SaveProxy(this.Proxy);
            }*/

            Helper.SaveActiveGroup(this.ActiveGroup);
        }

        private bool AlwaysTrue() { return true; }

        #endregion

        #region Methods

        /// <summary>
        /// Chargement de l'url à partir du fichier espaceco.xml
        /// </summary>
        private void DisplayUrl()
        {
            this.Url = Helper.LoadUrlhost();
        }

        /// <summary>
        /// En fonction du dernier login de l'utilisateur
        /// - la case à cocher est cochée
        /// - la zone de texte est remplie
        /// </summary>
        private void DisplayLogin()
        {
            string login = Helper.LoadLogin();
            this.Login = login;
            if (!string.IsNullOrEmpty(login))
            {
                this.helpConfigureView.LoginCheckBox.IsChecked = true;
            }
        }

        /// <summary>
        /// Chargement de la dernière date d'extraction à partir du fichier espaceco.xml
        /// </summary>
        private void DisplayDatesExtraction()
        {
            DateTime startDate = Helper.LoadStartDateExtraction();
            if(!string.IsNullOrEmpty(startDate.ToString()))
            {
                if(startDate.Equals(Convert.ToDateTime(Helper.dateDefault))) this.helpConfigureView.ExtractCheckBoxStart.IsChecked = false;
                else this.helpConfigureView.ExtractCheckBoxStart.IsChecked = true;

                this.ExtractSelectedStartDate = startDate;
                this.ExtractDisplayStartDate = this.ExtractSelectedStartDate;
            }

            DateTime endDate = Helper.LoadEndDateExtraction();
            if (!string.IsNullOrEmpty(endDate.ToString()))
            {
                if (endDate.Equals(DateTime.Now)) this.helpConfigureView.ExtractCheckBoxEnd.IsChecked = false;
                else this.helpConfigureView.ExtractCheckBoxEnd.IsChecked = true;
                
                this.ExtractSelectedEndDate = endDate;
                this.ExtractDisplayEndDate = this.ExtractSelectedEndDate;
            }
        }

        /// <summary>
        /// Chargement du proxy à partir du fichier espaceco.xml
        /// </summary>
        /*private void DisplayProxy()
        {
            string proxy = Helper.LoadProxy();
            if (!string.IsNullOrEmpty(proxy))
            {
                this.Proxy = proxy;
                this.helpConfigureView.ProxyCheckBox.IsChecked = true;
            }
            
        }*/

        /// <summary>
        /// Chargement du nom du dernier groupe utilisé par l'utilisateur
        /// à partir du fichier espaceco.xml
        /// </summary>
        private void DisplayActiveGroup()
        {
            this.ActiveGroup = Helper.LoadActiveGroup();
        }

        /// <summary>
        /// Initialisation de la liste des couches en fonction du projet ouvert par l'utilisateur
        /// Affichage de la dernière couche utilisée par l'utilisateur pour extraire les signalements
        /// à partir du fichier espaceco.xml si celle-ci se trouve dans la liste des couches
        /// </summary>
        private void DisplaySpatialFilter()
        {
            ObservableCollection<string> layersNameForSpatialFilter = GetLayersNameForSpatialFilterFromMap();
            this.SpatialFilterItemsSource = layersNameForSpatialFilter;

            string nameLayerForSpatialFilter = Helper.LoadNameLayerForSpatialFilter();
            if (layersNameForSpatialFilter.IndexOf(nameLayerForSpatialFilter) != -1)
            {
                this.SpatialFilterSelectedItem = Helper.LoadNameLayerForSpatialFilter();
                this.helpConfigureView.SpecialFilterCheckBox.IsChecked = true;
            }          
        }

        /// <summary>
        /// Retourne la liste des noms de couches de type polygone contenues dans la carte
        /// </summary>
        /// <returns>La liste des couches</returns>
        private ObservableCollection<string> GetLayersNameForSpatialFilterFromMap()
        {
            ObservableCollection<string> layersName = new ();
            Map map = this.Context.GetMap();
            if (map == null)
            {
                return layersName;
            }
            IReadOnlyList<Layer> layers = map.GetLayersAsFlattenedList();
            foreach (var layer in layers)
            {
                // Si une couche WFS/WMTS a perdu sa connexion
                if (layer.ConnectionStatus == ConnectionStatus.Broken)
                {
                    continue;
                }
                // Si c'est la couche des croquis surfaciques
                if (layer.Name == Helper.name_layer_Croquis_Polygone)
                {
                    continue;
                }
                
                // Si une couche est du type WFS/WMTS ou si une couche est différente de polygone
                if (layer is not FeatureLayer featureLayer || featureLayer.ShapeType != esriGeometryType.esriGeometryPolygon)
                {
                    continue;
                }
                layersName.Add(layer.Name);
            } 
            return layersName;
        }

        #endregion
    }
}
