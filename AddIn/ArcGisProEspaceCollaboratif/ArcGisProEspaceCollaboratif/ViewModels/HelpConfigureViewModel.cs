using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Mapping;
using ArcGisProEspaceCollaboratif.Utils;
using ArcGisProEspaceCollaboratif.Views;
using System;
using System.Collections.Generic;
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
            this.GetSpatialFilter();
            this.GetProxy();
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

        public ObservableCollection<string> SpatialFilterItemsSource { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SpatialFilterSelectedItem { get; set; }

        /// <summary>
        /// Le proxy pour accéder au site de l'espace collaboratif
        /// </summary>
        public string Proxy { get; set; } = "";

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
            Helper.SaveNameLayerForSpatialFilter(this.SpatialFilterSelectedItem);
            Helper.SaveProxy(this.Proxy);
            Helper.SaveActiveGroup(this.ActiveGroup);
            Helper.SaveGeoportalKey(this.GeoportalKey);
        }

        private bool AlwaysTrue() { return true; }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
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

        private void GetProxy()
        {
            this.Proxy = Helper.LoadProxy();
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetActiveGroup()
        {
            this.ActiveGroup = Helper.LoadActiveGroup();
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetGeoportalKey()
        {
            this.GeoportalKey = Helper.LoadGeoportalKey();
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetSpatialFilter()
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
        /// 
        /// </summary>
        /// <returns></returns>
        private ObservableCollection<string> GetLayersNameForSpatialFilterFromMap()
        {
            ObservableCollection<string> layersName = new ObservableCollection<string>();
            IReadOnlyList<Layer> layers = this.Context.MapActiveView.Map.GetLayersAsFlattenedList();
            foreach (var layer in layers)
            {
                if (layer.ConnectionStatus == ConnectionStatus.Broken)
                {
                    continue;
                }
                if (layer.Name == Helper.name_layer_Croquis_Polygone)
                {
                    continue;
                }
                FeatureLayer featureLayer = layer as FeatureLayer;
                if (featureLayer == null || featureLayer.ShapeType != esriGeometryType.esriGeometryPolygon)
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
