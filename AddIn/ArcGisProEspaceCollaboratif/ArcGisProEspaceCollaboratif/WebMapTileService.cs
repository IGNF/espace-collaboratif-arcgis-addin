using System.Collections.Generic;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGisProEspaceCollaboratif.Core;

namespace ArcGisProEspaceCollaboratif
{
    /// <summary>
    /// Création d'un service de connexion WMTS permettant de télécharger
    /// des couches du GéoPortail
    /// </summary>
    class WebMapTileService
    {
        /// <summary>
        /// Les références de la carte active
        /// </summary>
        public Map Map { get; set; } = MapView.Active.Map;

        /// <summary>
        /// La liste complète des couches WFS et WMTS à afficher dans ArcGis
        /// </summary>
        public List<LayerGateway> Layers { get; set; }

        /// <summary>
        /// La clé Géoportail de l'utilisateur
        /// </summary>
        private string _keyGeoportail = "";

        /// <summary>
        /// La liste des noms des couches existantes de la carte active
        /// </summary>
        public List<string> LayersInMap { get; set; }

        /// <summary>
        /// Les accesseurs à la clé Géoportail avec une condition
        /// Si la clé est nulle, vide ou de démonstration
        /// alors sa valeur est changée par une clé standard
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
        /// Comme son nom l'indique, il s'agit de créer une nouvelle connexion internet
        /// à l'Espace collaboratif
        /// </summary>
        public CIMInternetServerConnection InternetServerConnection { get; set; }

        /// <summary>
        /// Ajout des couches WMTS dans la carte ArcGIS
        /// </summary>
        /// <returns></returns>
        public async Task AddLayersAsync()
        {           
            // Création des couches sélectionnées par l'utilisateur dans ArcGIS 
            foreach (LayerGateway layer in Layers)
            {
                if (layer.Type != Constantes.GEOPORTAIL)
                {
                    // La couche n'est pas de type "GeoPortail", on passe à la suivante
                    continue;
                }

                int index = LayersInMap.FindIndex(x => x.Equals(layer.Name));
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
                            URL = string.Format("https://{0}/{1}/geoportail/wmts?SERVICE=WMTS&REQUEST=GetCapabilities", Constantes.WXSIGN, KeyGeoportail)
                        };
                    }

                    // WMTS service connection.
                    var connection = new CIMWMTSServiceConnection
                    {
                        Description = layer.Description,
                        LayerName = layer.Name,
                        ServerConnection = InternetServerConnection,
                        Version = "1.0.0"
                    };

                    // Ajout de la couche Geoportail dans la carte
                    ILayerFactory lf = LayerFactory.Instance;
                    lf.CreateLayer(connection, Map);
                });
            }
        }
    }
}
