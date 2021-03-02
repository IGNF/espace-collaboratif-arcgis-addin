using System.Collections.Generic;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGisProEspaceCollaboratif.Core;

namespace ArcGisProEspaceCollaboratif
{
    /// <summary>
    /// Création d'un service de connexion WFS permettant
    ///  de télécharger des couches à partir de l'Espace collaboratif
    /// </summary>
    class WebFeatureService
    {
        /// <summary>
        /// Le login utilisateur
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Le password du login utilisateur
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Le groupe de l'utilisateur, le fameux groupe actif
        /// </summary>
        public string ActiveGroup { get; set; }

        /// <summary>
        /// Les références de la carte active
        /// </summary>
        public Map Map { get; set; } = MapView.Active.Map;

        /// <summary>
        /// La liste complète des couches WFS et WMTS à afficher dans ArcGis
        /// </summary>
        public List<LayerGateway> Layers { get; set; }

        /// <summary>
        /// L'url de connexion à l'Espace collaboratif
        /// </summary>
        private string _url = "";

        /// <summary>
        /// Le nom de la couche à télécharger sur l'Espace collaboratif
        /// </summary>
        private string LayerName { get; set; }

        /// <summary>
        /// Le "typename" dans l'url de connexion à l'Espace collaboratif
        /// qui est le nom de la base suivi du nom de la table
        /// Exemple typename=test:Surfaces
        /// Est initialisé avec le set de l'url
        /// </summary>
        private string TypeName { get; set; }

        /// <summary>
        /// Les accesseurs à l'url de connexion à l'espace collaboratif
        /// </summary>
        public string Url
        {
            get => _url;
            set
            {
                if (value.Contains("&"))
                {
                    string[] tmp = value.Split('&');
                    string[] database = tmp[1].Split('=');
                    TypeName = string.Format("{0}:{1}", database[1], LayerName);
                    _url = tmp[0];
                }
            }
        }

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
        /// Ajout des couches WFS dans la carte ArcGIS
        /// </summary>
        public async System.Threading.Tasks.Task AddLayersAsync()
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

                LayerName = layer.Nom;
                Url = layer.Url;
                if (InternetServerConnection == null)
                {
                    InternetServerConnection = new CIMInternetServerConnection
                    {
                        URL = string.Format("{0}?SERVICE=WFS&REQUEST=GetCapabilities", Url),
                        User = Login,
                        Password = Password
                    };
                }

                // WFS service connection.
                var connection = new CIMWFSServiceConnection
                {
                    //CapabilitiesParameters
                    LayerName = LayerName,
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
