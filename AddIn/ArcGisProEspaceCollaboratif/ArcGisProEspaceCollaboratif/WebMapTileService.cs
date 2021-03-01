using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

using ArcGisProEspaceCollaboratif.Core;

namespace ArcGisProEspaceCollaboratif
{
    /// <summary>
    /// Création d'un service de connexion permettant de télécharger
    ///  des couches WMTS à partir du GéoPortail
    /// </summary>
    class WebMapTileService
    {
        /// <summary>
        /// Les références de la carte active
        /// </summary>
        public Map Map { get; set; } = MapView.Active.Map;

        /// <summary>
        /// La liste des couches à afficher dans ArcGis
        /// </summary>
        public List<LayerGateway> Layers { get; set; }

        public List<LayerGeoportail> LayersGeoportail { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private string _keyGeoportail = "";

        /// <summary>
        /// 
        /// </summary>
        public string KeyGeoportail
        {
            get => _keyGeoportail;
            set {
                if (string.IsNullOrEmpty(value) || value == Constantes.DEMO)
                {
                    _keyGeoportail = Constantes.CLEGEOPORTAILSTANDARD;
                }
                else
                {
                    _keyGeoportail = value;
                }    
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public CIMInternetServerConnection InternetServerConnection { get; set; }

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
        public List<string> LayersInMap
        {
            get
            {
                System.Collections.ObjectModel.ReadOnlyObservableCollection<Layer> observableLayers = Map.Layers;
                List<string> layersInMap = new List<string>();
                foreach (Layer observableLayer in observableLayers)
                {
                    int index = LayersGeoportail.FindIndex(x => x.Title.Equals(observableLayer.Name));
                    if (index != -1)
                    {
                        layersInMap.Add(LayersGeoportail[index].Name);
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
        /// 
        /// </summary>
        /// <param name="keyGeoportail"></param>
        /// <returns></returns>
        public async Task AddLayersAsync()
        {           
            // L'url doit être de la forme https://wxs.ign.fr/xxxxx/geoportail/wmts?SERVICE=WMTS&REQUEST=GetCapabilities
            Url = string.Format("https://wxs.ign.fr/{0}/geoportail/wmts?SERVICE=WMTS&REQUEST=GetCapabilities", KeyGeoportail);

            // Création des couches sélectionnées par l'utilisateur dans ArcGIS 
            foreach (LayerGateway layer in Layers)
            {
                if (layer.Type != Constantes.GEOPORTAIL)
                {
                    // La couche n'est pas de type "GeoPortail", on passe à la suivante
                    continue;
                }

                int index = LayersInMap.FindIndex(x => x.Equals(layer.Nom));
                if (index != -1)
                {
                    // La couche existe déjà, on passe à la suivante
                    continue;
                }

                await QueuedTask.Run(() =>
                {
                    if (InternetServerConnection == null)
                    {
                        InternetServerConnection = new CIMInternetServerConnection
                        {
                            URL = Url
                        };
                    }

                    // WMTS service connection.
                    var connection = new CIMWMTSServiceConnection
                    {
                        Description = layer.Description,
                        LayerName = layer.Nom,
                        ServerConnection = InternetServerConnection,
                        Version = "1.0.0"
                    };

                    // Ajout de la couche Geoportail dans la carte
                    ILayerFactory lf = LayerFactory.Instance;
                    var layerWMTScreate = lf.CreateLayer(connection, Map);
                });
            }
        }
    }
}
