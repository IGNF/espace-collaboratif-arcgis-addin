using ArcGisProEspaceCollaboratif.Core;
using ArcGisProEspaceCollaboratif.Views;
using ArcGisProEspaceCollaboratif.Utils;
using System.Collections.Generic;
using System.Windows.Input;
using ArcGIS.Desktop.Mapping;
using System.Threading.Tasks;
using System;

namespace ArcGisProEspaceCollaboratif.ViewModels
{
    /// <summary>
    /// La classe de gestion de la view LoadGatewayView
    /// </summary>
    class LoadGatewayViewModel : ViewModelBase
    {
        #region Parameters

        /// <summary>
        /// L'instance du dialogue "Charger les couches de mon groupe"
        /// </summary>
        public LoadGatewayView loadGatewayView;

        /// <summary>
        /// Toutes les données contextuelles liées à la carte et à l'Espace collaboratif
        /// </summary>
        public Context Context { get; set; }

        /// <summary>
        /// La liste de toutes les couches disponibles pour le groupe de l'utilisateur 
        /// </summary>
        public List<LayerGateway> ListLayers { get; set; } = new List<LayerGateway>();

        /// <summary>
        /// Le profil de l'utilisateur
        /// </summary>
        public Profile UserProfile { get; set; }

        public const string strLabel = "Groupe actif : ";

        /// <summary>
        /// Balises <ROLE>
        /// Role de la couche dans le cadre d'un guichet
        /// - visu = couche servant de fond uniquement
        /// - ref = couche servant de référence pour la saisie (snapping ou routing)
        /// - edit, ref-edit = couche éditable sur le guichet
        ///
        /// Le dictionnaire est de la forme "clé xml":"valeur affichage boite"
        /// </summary>
        readonly Dictionary<string, string> roleKeyValue = new Dictionary<string, string>
        {
            { "edit" , "Edition" },
            { "ref-edit" , "Edition" },
            { "visu" , "Visualisation" },
            { "ref" , "Visualisation" }
        };
        #endregion

        #region Constructors
        /// <summary>
        /// Initialisation du dialogue "Charger les couches de mon groupe"
        /// </summary>
        public LoadGatewayViewModel(Context context)
        {
            this.Context = context;
            this.loadGatewayView = new LoadGatewayView();
            this.InitializeLoadGatewayView();
        }
        #endregion

        #region Bindings
        /// <summary>
        /// Indication du groupe actif, c'est à dire le groupe
        /// à partir duquel l'utilisateur va pouvoir sélectionner ses couches WFS et WMTS
        /// </summary>
        public string ActiveGroupLabelContent { get; set; } = strLabel;

        /// <summary>
        /// Remplissage de la listView ListViewGateway
        /// </summary>
        public List<ItemsListViewGateway> ItemsSourceLayersGateway { get; set; }

        /// <summary>
        /// Remplissage de la listView ItemsListViewGeoservices
        /// </summary>
        public List<ItemsListViewGeoservices> ItemsSourceLayersGeoservices { get; set; }
        #endregion

        #region Class
        /// <summary>
        /// La classe qui permet de remplir les colonnes "Nom de la couche", "Rôle" et "Charger"
        /// pour un item (une couche à charger) et permettre à l'utilisateur de choisir
        /// les couches qu'il veut afficher.
        /// </summary>
        public class ItemsListViewGateway
        {
            /// <summary>
            /// Remplissage du nom d'une couche pour un item
            /// de la listView "LayersGatewayListView"
            /// </summary>
            public string GatewayName { get; set; } = "";

            /// <summary>
            /// Remplissage du rôle (Visualisation/Edition d'une couche pour un item
            /// de la listView "LayersGatewayListView"
            /// </summary>
            public string GatewayRole { get; set; } = "";
        }

        /// <summary>
        /// 
        /// </summary>
        public class ItemsListViewGeoservices
        {
            /// <summary>
            /// Remplissage du nom d'une couche pour un item
            /// de la listView "LayersGeoservicesListView"
            /// </summary>
            public string GeoservicesName { get; set; } = "";

            /// <summary>
            /// Remplissage du rôle (Visualisation/Edition d'une couche pour un item
            /// de la listView "LayersGeoservicesListView"
            /// </summary>
            public string GeoservicesRole { get; set; } = "";
        }
        
        #endregion

        #region Commands
        public ICommand RegisterButtonCmd { get { return new RelayCommand(OnRegister, AlwaysTrue); } }

        /// <summary>
        /// Import des couches sélectionnées par l'utilisateur dans ArcGIS
        /// </summary>
        private void OnRegister()
        {
            try
            {
                //Liste des couches à afficher après la sélection de l'utilisateur
                List<LayerGateway> layersToAppend = new List<LayerGateway>();
                foreach (string layerCheck in this.GetLayersSelected())
                {
                    // layerCheck est sous la forme 'troncon_de_voie_ferree' ou 'Cartes IGN (GEOGRAPHICALGRIDSYSTEMS.MAPS)'
                    string name;
                    if (layerCheck.Contains("("))
                    {
                        string[] layerCheckName = layerCheck.Split('(');
                        name = layerCheckName[0].Remove(layerCheckName[0].Length - 1);
                    }
                    else
                    {
                        name = layerCheck;
                    }

                    int index = this.ListLayers.FindIndex(x => x.Name.Equals(name));
                    if (index == -1)
                    {
                        continue;
                    }
                    layersToAppend.Add(this.ListLayers[index]);
                }

                // Import des couches WFS et WMTS dans ArcGIS
                LoadLayersAsync(layersToAppend);
            }
            catch (Exception e)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                        e.Message,
                        Constantes.ERROR
                    );
                string message = string.Format("{0}\n{1}", e.Message, e.StackTrace);
                logger.Error(string.Format("LoadGatewayViewModel.OnRegister : {0}\n", message));
            }
            
        }

        private bool AlwaysTrue() { return true; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        public void InitializeLoadGatewayView()
        {
            // Remplissage des différentes ListView avec les couches disponibles
            this.GetInfosLayers();
            this.SetActiveGroupLabelContent();
            this.SetListViewMyGateway();
            this.SetListViewFondsGeoservices();
        }

        /// <summary>
        /// Récupération des couches de l'Espace collaboratif pour le groupe choisi par l'utilisateur
        /// </summary>
        private void GetInfosLayers()
        {
            this.ListLayers.Clear();
            foreach (GeoGroup groupe in this.Context.Profil.Geogroupes)
            {
                if (groupe.Id != this.Context.Profil.Group.Id)
                {
                    continue;
                }
                foreach (LayerGateway layer in groupe.Layers)
                {
                    this.ListLayers.Add(layer);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetActiveGroupLabelContent()
        {
            this.ActiveGroupLabelContent = string.Format("{0}{1}", strLabel, this.Context.Profil.Group.Name);
        }

        /// <summary>
        /// Mise à jour de la ListView "Mon guichet" contenant
        /// les couches du groupe utilisateur
        /// </summary>
        private void SetListViewMyGateway()
        {
            List<ItemsListViewGateway> items = new List<ItemsListViewGateway>();
            foreach (LayerGateway layer in this.ListLayers)
            {
                if (layer.Type != Constantes.WFS)
                {
                    continue;
                }

                if (!layer.Url.Contains(Constantes.COLLABORATIF))
                {
                    continue;
                }

                items.Add(new ItemsListViewGateway() { GatewayName = layer.Name, GatewayRole = this.roleKeyValue[layer.Role] });
            }
            ItemsSourceLayersGateway = items;
        }

        /// <summary>
        /// Mise à jour de la ListView "ItemsListViewGeoservices"
        /// </summary>
        private void SetListViewFondsGeoservices()
        {
            List<ItemsListViewGeoservices> items = new List<ItemsListViewGeoservices>();
            foreach (LayerGateway layer in this.ListLayers)
            {
                if (layer.Type != Constantes.WMTS)
                {
                    continue;
                }
                string LayerName = string.Format("{0} ({1})", layer.Name, layer.Description);
                items.Add(new ItemsListViewGeoservices() { GeoservicesName = LayerName, GeoservicesRole = this.roleKeyValue[layer.Role] });
            }
            ItemsSourceLayersGeoservices = items;
        }

        /// <summary>
        /// Retourne l'ensemble des couches qui ont été cochées par l'utilisateur
        /// </summary>
        /// <returns>La liste des noms de couches</returns>
        private List<string> GetLayersSelected()
        {
            List<string> checked_list = new List<string>();
            foreach (object item in this.loadGatewayView.LayersGatewayListView.SelectedItems)
            {
                checked_list.Add(((ArcGisProEspaceCollaboratif.ViewModels.LoadGatewayViewModel.ItemsListViewGateway)item).GatewayName);
            }
            foreach (object item in this.loadGatewayView.LayersGeoservicesListView.SelectedItems)
            {
                checked_list.Add(((ArcGisProEspaceCollaboratif.ViewModels.LoadGatewayViewModel.ItemsListViewGeoservices)item).GeoservicesName);
            }
            return checked_list;
        }

        /// <summary>
        /// Récupère dans une liste les noms des couches existantes de la carte active
        /// et change le nom des couches Geoservices car dans certains cas les valeurs des balises DESCRIPTION et Title sont les mêmes
        /// dans d'autres elles sont différentes, il faut donc récupérer la valeur de la balise Name
        ///
        /// Espace collaboratif versus Geoservice
        /// <NOM>CADASTRALPARCELS.PARCELLAIRE_EXPRESS</NOM> == <Name>CADASTRALPARCELS.PARCELLAIRE_EXPRESS</Name>
        /// <DESCRIPTION>Plan cadastral informatisé vecteur de la DGFIP.</DESCRIPTION> != <Title>PCI vecteur</Title>
        /// autre exemple
        /// <NOM>ORTHOIMAGERY.ORTHOPHOTOS</NOM> == <Name>ORTHOIMAGERY.ORTHOPHOTOS</Name>
        /// <DESCRIPTION>Photographies aériennes</DESCRIPTION> == <Title>Photographies aériennes</Title>
        /// </summary>
        private List<string> LayersInMap
        {
            get
            {
                System.Collections.ObjectModel.ReadOnlyObservableCollection<Layer> observableLayers = this.Context.MapActiveView.Map.Layers;
                
                List<string> layersInMap = new List<string>();
                foreach (Layer observableLayer in observableLayers)
                {
                    layersInMap.Add(observableLayer.Name);
                }
                return layersInMap;
            }
        }

        /// <summary>
        /// Import des couches WFS et WMTS sélectionnées par l'utilisateur dans ArcGIS
        /// </summary>
        /// <param name="layersToLoad">La liste de toutes les couches sélectionnées à importer avec leurs caractéristiques</param>
        public async Task LoadLayersAsync(List<LayerGateway> layersToLoad)
        {
            // Quelles sont les couches existantes dans la carte ?
            List<string> layersInMap = LayersInMap;

            // Les couches WFS de l'Espace collaboratif
            WebFeatureService wfs = new WebFeatureService()
            {
                Layers = layersToLoad,
                LayersInMap = layersInMap,
                Login = this.Context.Login,
                Password = this.Context.Password,
                ActiveGroup = this.Context.Groupeactif
            };
            await wfs.AddLayersAsync();

            // Les couches WMTS du Geoservices
            WebMapTileService wmts = new WebMapTileService()
            {
                Layers = layersToLoad,
                LayersInMap = layersInMap,
            };
            await wmts.AddLayersAsync();
        }
        #endregion
    }
}
