using ArcGisProEspaceCollaboratif.Core;
using ArcGisProEspaceCollaboratif.Views;
using ArcGisProEspaceCollaboratif.Utils;
using System.Collections.Generic;
using System.Windows.Input;
using System.Collections.ObjectModel;

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
        public Contexte Context { get; set; }

        /// <summary>
        /// La liste de toutes les couches disponibles pour le groupe de l'utilisateur 
        /// </summary>
        public List<LayerGateway> ListLayers { get; set; } = new List<LayerGateway>();

        /// <summary>
        /// Le profil de l'utilisateur
        /// </summary>
        public Profil UserProfile { get; set; }

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
        public LoadGatewayViewModel(Contexte context)
        {
            this.Context = context;
            this.AtiveGroupLabelContent = string.Format("{0}{1}", strLabel, this.Context.Profil.Group.Name);
            this.loadGatewayView = new LoadGatewayView();
            // Remplissage des différentes ListView avec les couches disponibles
            this.GetInfosLayers();
            this.SetListViewMyGateway();
            this.SetListViewFondsGeoportail();
            this.SetListViewFondsGeoportailBis();
        }
        #endregion

        #region Bindings
        /// <summary>
        /// Indication du groupe actif, c'est à dire le groupe
        /// à partir duquel l'utilisateur va pouvoir sélectionner ses couches WFS et WMTS
        /// </summary>
        public string AtiveGroupLabelContent { get; set; } = strLabel;

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
        /// L'utilisateur a cliqué sur le bouton "Enregistrer".
        /// Il faut charger les couches choisies par l'utilisateur
        /// </summary>
        private void OnRegister()
        {

        }

        private bool AlwaysTrue() { return true; }
        #endregion

        #region Methods
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
        /// Récupération des couches de l'Espace collaboratif pour le groupe choisi par l'utilisateur
        /// </summary>
        private void GetInfosLayers()
        {
            this.ListLayers.Clear();
            foreach (GeoGroupe groupe in this.Context.Profil.Geogroupes)
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
        #endregion
    }
}
