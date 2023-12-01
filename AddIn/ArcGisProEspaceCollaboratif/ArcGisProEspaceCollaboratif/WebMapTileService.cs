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
    /// des couches du Geoservices
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
        /// La liste des noms des couches existantes de la carte active
        /// </summary>
        public List<string> LayersInMap { get; set; }

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
                if (layer.Type != Constantes.WMTS)
                {
                    // La couche n'est pas de type "WMTS", on passe à la suivante
                    continue;
                }

                int index = LayersInMap.FindIndex(x => x.Equals(layer.Name));
                if (index != -1)
                {
                    // La couche existe déjà, on passe à la suivante
                    continue;
                }

                // Connexion au service
                InternetServerConnection = new CIMInternetServerConnection
                {
                    URL = string.Format("{0}?SERVICE=WMTS&REQUEST=GetCapabilities", layer.Url)
                };

                // WMTS service connexion.
                List<CIMWMTSServiceConnection> serviceConnections = new ();
                var connection = new CIMWMTSServiceConnection
                {
                    Description = layer.Description,
                    LayerName = layer.ServiceName,
                    ServerConnection = InternetServerConnection,
                    Version = "1.0.0"
                };
                serviceConnections.Add(connection);

                await QueuedTask.Run(() =>
                {
                    ILayerFactory lf = LayerFactory.Instance;
                    foreach (CIMWMTSServiceConnection serviceConnection in serviceConnections)
                    {
                        // Ajout de la couche Geoservices dans la carte
                        //lf.CreateLayer(serviceConnection, Map);
                    }                    
                });
            }
        }
    }
}
