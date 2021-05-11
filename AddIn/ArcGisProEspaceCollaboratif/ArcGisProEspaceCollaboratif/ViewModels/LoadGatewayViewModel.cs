using ArcGisProEspaceCollaboratif.Core;
using ArcGisProEspaceCollaboratif.Views;
using ArcGisProEspaceCollaboratif.Utils;
using System.Collections.Generic;
using System.Windows.Input;
using ArcGIS.Desktop.Mapping;
using System.Threading.Tasks;

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
        /// Remplissage de la listView ListViewGeoportail
        /// </summary>
        public List<ItemsListViewGeoportail> ItemsSourceLayersGeoportail { get; set; }

        /// <summary>
        /// Remplissage de la listview ListViewGeoportailBis
        /// </summary>
        public List<ItemsListViewGeoportailBis> ItemsSourceLayersGeoportailBis { get; set; }
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
        public class ItemsListViewGeoportail
        {
            /// <summary>
            /// Remplissage du nom d'une couche pour un item
            /// de la listView "LayersGeoportailListView"
            /// </summary>
            public string GeoportailName { get; set; } = "";

            /// <summary>
            /// Remplissage du rôle (Visualisation/Edition d'une couche pour un item
            /// de la listView "LayersGeoportailListView"
            /// </summary>
            public string GeoportailRole { get; set; } = "";
        }
        
        /// <summary>
        /// 
        /// </summary>
        public class ItemsListViewGeoportailBis
        {
            /// <summary>
            /// Remplissage du nom d'une couche pour un item
            /// de la listView "LayersGeoportailBisListView"
            /// </summary>
            public string GeoportailBisName { get; set; } = "";
        }
        #endregion

        #region Commands
        public ICommand RegisterButtonCmd { get { return new RelayCommand(OnRegister, AlwaysTrue); } }

        /// <summary>
        /// Import des couches sélectionnées par l'utilisateur dans ArcGIS
        /// </summary>
        private void OnRegister()
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
                    name = layerCheckName[1].Replace(")", "");
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
            this.SetListViewFondsGeoportail();
            this.SetListViewFondsGeoportailBis();
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
        /// Mise à jour de la ListView "FondsGeoportail" contenant
        /// les couches visibles avec la clé Géoportail de l'utilisateur
        /// </summary>
        private void SetListViewFondsGeoportail()
        {
            List<ItemsListViewGeoportail> items = new List<ItemsListViewGeoportail>();
            foreach (LayerGateway layer in this.ListLayers)
            {
                if (layer.Type != Constantes.GEOPORTAIL)
                {
                    continue;
                }
                int index = this.Context.Profil.LayersKeyGeoportail.FindIndex(x => x.Name.Equals(layer.Name));
                if (index == -1)
                {
                    continue;
                }

                string LayerName = string.Format("{0} ({1})", this.Context.Profil.LayersKeyGeoportail[index].Title, layer.Name);
                items.Add(new ItemsListViewGeoportail() { GeoportailName = LayerName, GeoportailRole = this.roleKeyValue[layer.Role] });
            }
            ItemsSourceLayersGeoportail = items;
        }

        /// <summary>
        /// Mise à jour de la ListView "FondsGeoportailBis" contenant
        /// les autres couches visibles avec la clé Géoportail de l'utilisateur
        /// </summary>
        private void SetListViewFondsGeoportailBis()
        {
            List<ItemsListViewGeoportailBis> items = new List<ItemsListViewGeoportailBis>();
            foreach (LayerGateway layer in this.ListLayers)
            {
                if (layer.Type != Constantes.GEOPORTAIL)
                {
                    continue;
                }

                if (this.Context.Profil.LayersKeyGeoportail.FindIndex(x => x.Name.Equals(layer.Name)) != -1)
                {
                    continue;
                }

                string LayerName = string.Format("{0} ({1})", layer.Description, layer.Name);
                items.Add(new ItemsListViewGeoportailBis() { GeoportailBisName = LayerName });
            }
            ItemsSourceLayersGeoportailBis = items;
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
            foreach (object item in this.loadGatewayView.LayersGeoportailListView.SelectedItems)
            {
                checked_list.Add(((ArcGisProEspaceCollaboratif.ViewModels.LoadGatewayViewModel.ItemsListViewGeoportail)item).GeoportailName);
            }
            foreach (object item in this.loadGatewayView.LayersGeoportailBisListView.SelectedItems)
            {
                checked_list.Add(((ArcGisProEspaceCollaboratif.ViewModels.LoadGatewayViewModel.ItemsListViewGeoportailBis)item).GeoportailBisName);
            }
            return checked_list;
        }

        /// <summary>
        /// Récupère dans une liste les noms des couches existantes de la carte active
        /// et change le nom des couches Geoportail car dans certains cas les valeurs des balises DESCRIPTION et Title sont les mêmes
        /// dans d'autres elles sont différentes, il faut donc récupérer la valeur de la balise Name
        ///
        /// Espace collaboratif versus Geoportail
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
                    int index = this.Context.Profil.LayersKeyGeoportail.FindIndex(x => x.Title.Equals(observableLayer.Name));
                    if (index != -1)
                    {
                        layersInMap.Add(this.Context.Profil.LayersKeyGeoportail[index].Name);
                    }
                    else
                    {
                        layersInMap.Add(observableLayer.Name);
                    }
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

            // Les couches WMTS du Géoportail
            WebMapTileService wmts = new WebMapTileService()
            {
                Layers = layersToLoad,
                LayersInMap = layersInMap,
                KeyGeoportail = this.Context.CleGeoportail
            };
            await wmts.AddLayersAsync();
        }
        #endregion
    }
}
