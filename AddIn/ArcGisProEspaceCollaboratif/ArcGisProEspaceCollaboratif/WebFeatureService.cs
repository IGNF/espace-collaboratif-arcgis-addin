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
    /// des couches WFS à partir de l'Espace collaboratif
    /// </summary>
    class WebFeatureService
    {
        /// <summary>
        /// Les références de la carte active
        /// </summary>
        public Map Map { get; set; } = MapView.Active.Map;

        /// <summary>
        /// La liste des couches WFS et WMTS à afficher dans ArcGis
        /// </summary>
        public List<LayerGateway> Layers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Url { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        public List<string> LayersInMap
        {
            get
            {
                System.Collections.ObjectModel.ReadOnlyObservableCollection<Layer> observableLayers = Map.Layers;
                List<string> layersInMap = new List<string>();
                foreach (Layer observableLayer in observableLayers)
                {
                    layersInMap.Add(observableLayer.Name);
                }
                return layersInMap;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public CIMInternetServerConnection InternetServerConnection { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activeGroup"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task AddLayersAsync(string activeGroup)
        {
            // Création des couches sélectionnées par l'utilisateur dans ArcGIS 
            foreach (LayerGateway layer in Layers)
            {
                int index = LayersInMap.FindIndex(x => x.Equals(layer.Nom));
                if (index != -1)
                {
                    // La couche existe déjà, on passe à la suivante
                    continue;
                }

                if (layer.Type != Constantes.WFS)
                {
                    // La couche n'est pas de type WFS, on passe à la suivante
                    continue;
                }

                if (InternetServerConnection == null)
                {
                    InternetServerConnection = new CIMInternetServerConnection
                    {
                        //URL = "https://espacecollaboratif.ign.fr/gcms/wfs?SERVICE=WFS&REQUEST=GetCapabilities"
                        URL = "https://qlf-collaboratif.ign.fr/collaboratif-3.0/gcms/wfs?service=wfs&request=GetFeature&typename=test:Surfaces"
                        //URL = "https://espacecollaboratif.ign.fr/gcms/wfs?service=wfs&request=DescribeFeatureType&typename=test:Surfaces"
                    };
                }

                // WFS service connection.
                var connection = new CIMWFSServiceConnection
                {
                    //CapabilitiesParameters
                    LayerName = layer.Nom,
                    ServerConnection = InternetServerConnection,
                    Version = "1.0.0"
                };

                await QueuedTask.Run(() =>
                {     
                    ILayerFactory lf = LayerFactory.Instance;
                    //GroupLayer grpNew = lf.CreateGroupLayer(Map, 0, activeGroup);
                    Layer layerWFScreate = lf.CreateLayer(connection, Map);
                });
            }
        }
    }
}
