using System;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using System.IO;
using System.Threading.Tasks;
using log4net;
using ArcGisProEspaceCollaboratif.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Editing;
using static ArcGisProEspaceCollaboratif.Core.Sketch;
using ArcGisProEspaceCollaboratif.ViewModels;
using ArcGIS.Core.Data.Exceptions;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Data.DDL;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Windows.Documents;
using ArcGIS.Desktop.Workflow.Exceptions;
using static ArcGIS.Desktop.Internal.Core.PortalTrafficDataService.ServiceErrorResponse;
using System.Windows.Interop;
using System.Windows.Media.Converters;

namespace ArcGisProEspaceCollaboratif
{
    public sealed class Context
    {
        #region Parameters
        /// <summary>
        /// 
        /// </summary>
        public static object Padlock { get; set; } = new object();

        /// <summary>
        /// 
        /// </summary>
        public MapView MapActiveView { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public CollaborativeSpaceGeodatabase CollaborativeSpaceGeodatabase { get; set; }

        /// <summary>
        /// Le répertoire où est la carte ArcGIS Pro sur laquelle on travaille
        /// </summary>
        public string DirectoryWorking { get; set; } = "";

        /// <summary>
        /// Le fichier de la carte ArcGIS Pro sur laquelle on travaille
        /// </summary>
        public string FileMapWorking { get; set; } = "";

        /// <summary>
        /// URL d'accès au service de l'espace collaboratif
        /// </summary>
        public string URLHost { get; set; } = "";

        /// <summary>
        /// Le login à utiliser pour se connecter au service de l'espace collaboratif
        /// </summary>
        public string Login { get; set; } = "";

        /// <summary>
        /// Le mot de passe associé au login pour se connecter au service de l'espace collaboratif
        /// </summary>
        public string Password { get; set; } = "";

        /// <summary>
        /// Le groupe sélectionné par l'utilisateur sur lequel il veut travailler
        /// </summary>
        public string Groupeactif { get; set; } = "";

        /// <summary>
        /// Le système géodésique employé par le service de l'espace collaboratif
        /// </summary>
        public ArcGIS.Core.Geometry.SpatialReference SpatialReference { get; set; } = SpatialReferenceBuilder.CreateSpatialReference(4326);

        /// <summary>
        /// Le logger qui permet d'enregistrer des informations sur le processus
        /// </summary>
        private static readonly Logger riplogger = Logger.Instance;
        public static readonly log4net.ILog logger = LogManager.GetLogger(typeof(Context));

        /// <summary>
        /// 
        /// </summary>
        public Profile Profil { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Client Client { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private static Context _instance = null;
        public static Context Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Padlock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Context();
                            logger.Debug("Instance de contexte créée");
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Constructors
        /// <summary>
        /// Constructeur pour un contexte à partir de la carte courante
        /// </summary>
        private Context()
        {
            if (MapView.Active == null)
            {
                string message = "Votre projet doit être enregistré avant de pouvoir utiliser l'add-in Espace collaboratif";
                logger.Error(string.Format("Context.Init : {0}\n", message));
                throw new ArgumentNullException(message);
            }
            this.Init();
        }


        /// <summary>
        /// initialisation du contexte et des éléments Ripart
        /// </summary>
        /// <param name="activeView">L'activeView associée à la carte en cours.</param>
        private async Task Init()
        {
            await QueuedTask.Run(() =>
            {
                if (this.CollaborativeSpaceGeodatabase is null)
                {
                    this.CollaborativeSpaceGeodatabase = new CollaborativeSpaceGeodatabase();
                }

                this.MapActiveView = MapView.Active;

                Project project = Project.Current;
                if (project.Name.Length == 0)
                {
                    string message = "Votre projet doit être enregistré avant de pouvoir utiliser l'add-in Espace collaboratif";
                    logger.Error(string.Format("Context.Init : {0}\n", message));
                    throw new ArgumentNullException(message);
                }

                this.DirectoryWorking = System.IO.Path.GetDirectoryName(project.Path);
                this.FileMapWorking = System.IO.Path.GetFileNameWithoutExtension(project.Name);

                this.CheckConfigFile();

                logger.Debug("Initialisation du contexte et des éléments de l'Espace collaboratif");
            });
        }

        #endregion

        /// <summary>
        /// Teste si le fichier de configuration espaceco.xml n'existe pas dans le répertoire de travail et le copie 
        /// à partir du répertoire d'installation 
        /// </summary>
        /// <exception>
        /// Renvoie une exception si le fichier ne peut-être copié.
        /// </exception>
        public void CheckConfigFile()
        {
            Project project = Project.Current;
            //project.SaveAsync();
            this.DirectoryWorking = project.HomeFolderPath;
            /*if (this.DirectoryWorking == "")
            {  
                this.DirectoryWorking = System.IO.Path.GetDirectoryName(project.Path);
            }*/
            string fileConfiguration = string.Format("{0}\\{1}", this.DirectoryWorking, Helper.name_file_espaceco_xml);
            if (!File.Exists(fileConfiguration))
            {
                try
                {
                    File.Copy(Helper.EspaceCollaboratifDirectoryFiles + Helper.name_file_espaceco_xml, fileConfiguration);
                }
                catch (Exception e)
                {
                    string message = string.Format("{0}\n{1}", e.Message, e.StackTrace);
                    logger.Error(string.Format("Context.CheckConfigFile : {0}\n", message));
                    throw new Exception(string.Format("Impossible de poursuivre la procédure en raison de l'absence du fichier XML de paramétrage pour se connecter au service de l'Espace collaboratif.\nLe fichier '{0}' doit se situer dans le dossier suivant :\n'{1}'", Helper.name_file_espaceco_xml, this.DirectoryWorking));
                }
            }
        }

        /// <summary>
        /// Teste si la carte active contient les couches "Signalement", "Croquis_EC_Polygone",
        /// "Croquis_EC_Ligne" et "Croquis_EC_Point".
        /// Si non demande à changer de carte active
        /// Si oui et que les couches n'existent pas, demande leur création.
        /// </summary>
        /// <exception>
        /// Retourne false si l'utilisateur n'a pas choisi la bonne carte active, true pour continuer
        /// </exception>
        public async Task <bool> CheckMapActiveWithCollaborativeLayersAsync()
        { 
            MapView activeMap = GetActiveMap();
            IReadOnlyList<Layer> layers = activeMap.Map.GetLayersAsFlattenedList();

            int nb = 0;
            int nbNotConnected = 0;
            //Les couches sont elles connectées à la bonne geodatabase ?
            List<string> layersToBeRemoved = new List<string>();
            foreach (var layer in layers)
            {    
                if (!Helper.CollaborativeSpaceLayers.Contains(layer.Name))
                {
                    continue;
                }
                // Il faut vérifier que les couches sont connectées à la source de données
                if (!this.CheckConnectionStatus(layer))
                {
                    nbNotConnected++;
                }
                else if (layer is FeatureLayer featureLayer)
                {
                    // Accède à la source de données
                    var table = featureLayer.GetTable();
                    Geodatabase workspace = table.GetDatastore() as Geodatabase;
                    if (workspace != null)
                    {
                        string actualPath = workspace.GetPath().AbsolutePath;
                        string normalizedActualPath = Path.GetFullPath(actualPath).Trim().ToLowerInvariant();
                        string normalizedExpectedPath = Path.GetFullPath(this.CollaborativeSpaceGeodatabase.GeoDatabasePath).Trim().ToLowerInvariant();
                        if (normalizedActualPath != normalizedExpectedPath)
                        {
                            layersToBeRemoved.Add(layer.Name);
                        }
                        else
                        {
                            nb++;
                        }
                    }
                }
            }
            if (nbNotConnected > 0)
            {
                string mess = "Impossible d'établir la connexion à la source de données pour les couches signalement et croquis.\nPour connecter l'ensemble des couches, cliquer sur le point d'exclamation rouge, sélectionner le fichier [répertoire projet/espaceco.gdb], sélectionner une des tables Signalement ou Croquis puis sur OK.\nSi elles n'existent pas dans la géodatabase, il faut supprimer les couches dans la carte et relancer l'import des signalements.";
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(mess, Constantes.INFORMATION);
                return true;
            }
            if (layersToBeRemoved.Count != 0)
            {
                string mesg = string.Format("La carte {0} contient les couches signalement et croquis, mais ne sont pas connectées à la bonne geodatabase. Voulez-vous détruire ces couches et les recréer ?\nSi oui, Répondre Yes.\nSi non, veuillez changer de carte active et répondre No.", activeMap.Map.Name);
                System.Windows.MessageBoxResult res = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(mesg, Constantes.QUESTION, System.Windows.MessageBoxButton.YesNo);
                if (res == System.Windows.MessageBoxResult.OK ||
                    res == System.Windows.MessageBoxResult.Yes ||
                    res == System.Windows.MessageBoxResult.None)
                {
                    Helper.RemoveLayersInMap(layersToBeRemoved);
                    await CreateOrLoadReportLayers();
                    nb = 4;
                }
                else
                {
                    return true;
                }
            }
            if (nb != 4)
            {
                string msg = string.Format("La carte {0} ne contient pas les couches signalement et croquis, est ce la bonne carte active ?\nSi oui, voulez-vous créer ces couches ? Répondre Yes.\nSi non, veuillez changer de carte active et répondre No.", activeMap.Map.Name);
                System.Windows.MessageBoxResult result = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(msg, Constantes.QUESTION, System.Windows.MessageBoxButton.YesNo);
                if (result == System.Windows.MessageBoxResult.Cancel ||
                    result == System.Windows.MessageBoxResult.No ||
                    result == System.Windows.MessageBoxResult.None)
                {
                    return true;
                }
                else
                {
                    await CreateOrLoadReportLayers();
                }
            }
            return false;
        }

        /// <summary>{
        /// Teste si pour les couches signalement et croquis la connexion à la source de données
        /// est 'Connected'. Dans tous les autres cas, la fonction tente une reconnexion automatique
        /// à la source de données générale.
        /// </summary>
        /// <exception>
        /// Renvoie une exception si la connexion est Broken, Disconnected ou Unattempted.
        /// </exception>
        public bool CheckConnectionStatus(Layer layer)
        {
            switch (layer.ConnectionStatus)
            {
                case ConnectionStatus.Connected:
                    return true;

                case ConnectionStatus.Broken:
                case ConnectionStatus.Disconnected:
                case ConnectionStatus.Unattempted:
                    return false;

                default:
                    return false;
            }
            //throw new Exception("Impossible d'établir la connexion à la source de données pour les couches signalement et croquis.\nPour connecter l'ensemble des couches, cliquer sur le point d'exclamation rouge, sélectionner le fichier ****.gdb dans Project/Databases, sélectionner une des couches Signalement ou Croquis puis sur OK.");
            //throw new Exception("La source de données est valide, mais actuellement déconnectée.");
            //throw new Exception("Aucune tentative de connexion n'a été effectuée.");
        }

        public static MapView GetActiveMap()
        {
            //Get the active map view.
            var mapView = MapView.Active;
            if (mapView == null)
                return null;

            //Return the active map view.
            return mapView;
        }

        public Map SearchMap(string layerNameToSearch)
        {
            if (this.MapActiveView == null)
            {
                if (MapView.Active == null)
                {
                    string message = "Votre projet doit être enregistré avant de pouvoir utiliser l'add-in Espace collaboratif";
                    logger.Error(string.Format("Context.GetMap : {0}\n", message));
                    throw new ArgumentNullException(message);
                }
                else
                {
                    this.MapActiveView = MapView.Active;
                }
            }

            Map map = null;
            if (this.MapActiveView.Map != null)
            {
                map = this.MapActiveView.Map;
            }
            else
            {
                Project proj = Project.Current;
                IEnumerable<MapProjectItem> mpi = proj.GetItems<MapProjectItem>();
                foreach (MapProjectItem item in mpi)
                {
                    string layerName = item.Name;
                    if (string.IsNullOrEmpty(layerName))
                    {
                        continue;
                    }
                    if (layerName.ToLower() == layerNameToSearch.ToLower())
                    {
                        QueuedTask.Run(() =>
                        {
                            map = item.GetMap();
                        });
                    }
                }
            }
            return map;
        }

        public Map GetMap()
        {
            Map map = null;
            if (this.MapActiveView == null)
            {
                if (MapView.Active == null)
                {
                    string message = "Votre projet doit être enregistré avant de pouvoir utiliser l'add-in Espace collaboratif";
                    logger.Error(string.Format("Context.GetMap : {0}\n", message));
                    throw new ArgumentNullException(message);
                }
                else
                {
                    this.MapActiveView = MapView.Active;
                    if (this.MapActiveView.Map != null)
                    {
                        map = this.MapActiveView.Map;
                    }
                }
            }
            else
            {
                map = this.MapActiveView.Map;
                /*
                Project proj = Project.Current;
                IEnumerable<MapProjectItem> mpi = proj.GetItems<MapProjectItem>();
                foreach (MapProjectItem item in mpi)
                {
                    string layerName = item.Name;
                    if (string.IsNullOrEmpty(layerName))
                    {
                        continue;
                    }
                    QueuedTask.Run(() =>
                    {
                        map = item.GetMap();
                    });
                    if (map is null)
                    {
                        return map;
                    }
                    if (layerName == map.Name)
                    {
                        break;
                    }
                }*/
            }
            return map;
        }

        /// <summary>
        /// Ajoute les champs indiqués dans le dictionnaire à la feature class en entrée.
        /// </summary>
        /// <param name="fcPath">Chemin de la feature class dans laquelle les champs doivent être ajoutés.</param>
        /// <param name="fcAttributesDict">Dictionnaire contenant le nom, le type et la longueur éventuelle des champs à traiter.</param>
        /// <returns></returns>
        public void AddFieldsToFc(string fcPath, Dictionary<string, KeyValuePair<string, string>> fcAttributesDict)
        {
            foreach (KeyValuePair<string, KeyValuePair<string, string>> kvp in fcAttributesDict)
            {
                string fieldName = kvp.Key;
                string fieldType = kvp.Value.Key;
                string fieldLength = kvp.Value.Value;

                Geoprocessing.ExecuteToolAsync("AddField_management", Geoprocessing.MakeValueArray(fcPath, fieldName, fieldType, "", "", fieldLength));
            }
        }

        private FeatureLayer LoadCollabLayer(string fcName, int layerPosition)
        {
            FeatureLayer collabSpaceLayer;
            if (!IsLayerInMap(fcName))
            {
                // Ouverture de la feature class
                FeatureClass collabSpaceFc = this.CollaborativeSpaceGeodatabase.Geodatabase.OpenDataset<FeatureClass>(fcName);

                // Ajout en tant que FeatureLayer à la carte
                // https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference/topic76685.html
                FeatureLayerCreationParams createParams = new(new Uri(this.Client.Url))
                {
                    Name = fcName,
                    MapMemberIndex = layerPosition
                };
                collabSpaceLayer = LayerFactory.Instance.CreateLayer<FeatureLayer>(
                    createParams,
                    this.GetMap()
                );
            }
            else
            {
                collabSpaceLayer = this.GetLayerByName(fcName);
            }
            return collabSpaceLayer;
        }

        private FeatureLayer CreateCollabLayer(string fcPath, string fcName, string fcTypeGeometry, Dictionary<string, KeyValuePair<string, string>> fcAttributesDict)
        {
            List<object> arguments = new()
                {
                    this.CollaborativeSpaceGeodatabase.GeoDatabasePath, // Chemin de la geodatabase
                    fcName, // Nom de la feature class à créer                   
                    fcTypeGeometry, // type de géométrie                    
                    "", // no template                    
                    "DISABLED", // no z values                    
                    "DISABLED", // no m values
                                // Ajout de la référence spatiale
                    this.SpatialReference
                };

            // Création de la feature class
            Geoprocessing.ExecuteToolAsync("CreateFeatureclass_management", Geoprocessing.MakeValueArray(arguments.ToArray()));

            // Ajout des champs à la nouvelle feature class
            
            this.AddFieldsToFc(fcPath, fcAttributesDict);

            // La nouvelle feature class est chargée automatiquement dans la carte.
            // On récupère le FeatureLayer correspondant.
            return this.GetLayerByName(fcName);
        }
        /// <summary>
        /// Définition de la symbologie en fonction des valeurs du champ Statut.
        /// </summary>
        /// <returns></returns>
        public CIMRenderer CreateUniqueValueRendererForReportStatuses()
        {
            //Create the Unique Value Renderer
            CIMUniqueValueRenderer uniqueValueRenderer = new()
            {
                // set the value field
                Fields = new string[] { Helper.name_field_Statut }
            };

            //Construct the list of UniqueValueClasses
            List<CIMUniqueValueClass> classes = new();

            int index = 0;

            foreach (Status.EnumStatus currStatus in Enum.GetValues(typeof(Status.EnumStatus)))
            {
                List<CIMUniqueValue> statusValues = new();
                CIMUniqueValue statusValue = new() { FieldValues = new string[] { index.ToString() } };
                statusValues.Add(statusValue);

                List<double> statusRGBCodes = Status.GetStatusColor(currStatus);
                var statusColor = CIMColor.CreateRGBColor(statusRGBCodes[0], statusRGBCodes[1], statusRGBCodes[2]);

                var status = new CIMUniqueValueClass()
                {
                    Values = statusValues.ToArray(),
                    Label = Status.GetDisplayStatus(currStatus),
                    Visible = true,
                    Editable = true,
                    Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPointSymbol(statusColor, 15, SimpleMarkerStyle.Pushpin) }
                };
                classes.Add(status);
                index++;
            }

            //Add the classes to a group (by default there is only one group or "symbol level")
            // Unique value groups
            CIMUniqueValueGroup groupOne = new()
            {
                Heading = "Statuts",
                Classes = classes.ToArray()
            };
            uniqueValueRenderer.Groups = new CIMUniqueValueGroup[] { groupOne };

            return uniqueValueRenderer as CIMRenderer;
        }

        /// <summary>
        /// Applique une symbolisation à la couche des signalements.
        /// </summary>
        /// <param name="fcLayer">FeatureLayer à laquelle le symbole doit être appliqué.</param>
        /// <returns></returns>
        public async Task SetReportLayerStyle(FeatureLayer fcLayer)
        {
            await QueuedTask.Run(() =>
            {
                CIMRenderer uniqueValueRenderer = this.CreateUniqueValueRendererForReportStatuses();

                //setting the renderer to the feature layer
                fcLayer.SetRenderer(uniqueValueRenderer);
            });
        }

        public async Task LoadOrCreateCollaborativeSpaceLayers(string fcName, string fcTypeGeometry, Dictionary<string, KeyValuePair<string, string>> fcAttributesDict, int layerPosition)
        {
            // Est-ce que les tables existent dans la Geodatabase
            if (this.CollaborativeSpaceGeodatabase.IsTableExists(fcName))
            {
                // Est-ce que les colonnes dans la table sont correctement chargées ?
                if (!this.CollaborativeSpaceGeodatabase.IsFieldsInTable(fcName, fcAttributesDict))
                {
                    throw new Exception("Impossible de continuer, les tables pour télécharger les signalements sont erronées. Veuillez les détruire dans le projet et la Geodatabase puis relancer l'outil.");
                }
            }
            
            string fcPath = this.CollaborativeSpaceGeodatabase.GeoDatabasePath + "\\" + fcName;
            bool bFeatureClassExist = this.CollaborativeSpaceGeodatabase.IsFeatureClassExists(fcName);

            // Si la feature class existe dans la geodatabase existe -t'elle dans la carte ?
            FeatureLayer fl = this.GetLayerByName(fcName);
            //
            // déjà, on l'ouvre et on l'ajoute comme couche (FeatureLayer) à la carte
            FeatureLayer collabSpaceLayer;
            if (bFeatureClassExist && fl is not null)
            { 
                collabSpaceLayer = this.LoadCollabLayer(fcName, layerPosition);
            }
            // Si la feature class n'existe pas dans la geodatabase du projet ou dans la carte, on la crée
            else
            {
                // La nouvelle feature class est chargée automatiquement dans la carte.
                // On récupère le FeatureLayer correspondant.
                collabSpaceLayer = this.CreateCollabLayer(fcPath, fcName, fcTypeGeometry, fcAttributesDict);
            }
            List<object> argumentsSpatialIndex = new() { collabSpaceLayer };
            await Geoprocessing.ExecuteToolAsync("RemoveSpatialIndex_management", Geoprocessing.MakeValueArray(argumentsSpatialIndex.ToArray()));

            // Pour la couche de signalement :
            // Application d'une symbologie
            // Application d'un domaine (clés-valeurs) pour le champ statut
            if (fcName == Helper.name_layer_Signalement)
            {
                await this.SetReportLayerStyle(collabSpaceLayer);

                // Définition du domaine pour le champ Statut
                string statusDomainName = "Status_domain";

                List<object> argumentsDomain = new() {
                        this.CollaborativeSpaceGeodatabase.GeoDatabasePath,
                        statusDomainName,
                        "",
                        "LONG",
                        "CODED"
                    };
                await Geoprocessing.ExecuteToolAsync("CreateDomain_management", Geoprocessing.MakeValueArray(argumentsDomain.ToArray()));

                // Ajout des codes au domaine
                foreach (Status.EnumStatus currStatus in Enum.GetValues(typeof(Status.EnumStatus)))
                {
                    List<object> argumentsCodedValue = new() {
                            this.CollaborativeSpaceGeodatabase.GeoDatabasePath,
                            statusDomainName,
                            (long)currStatus,
                            Status.GetDisplayStatus(currStatus)
                        };
                    await Geoprocessing.ExecuteToolAsync("AddCodedValueToDomain_management", Geoprocessing.MakeValueArray(argumentsCodedValue.ToArray()));
                }

                // Application du domaine au champ statut
                List<object> argumentsAssignDomain = new() {
                            fcPath,
                            Helper.name_field_Statut,
                            statusDomainName
                        };
                await Geoprocessing.ExecuteToolAsync("AssignDomainToField_management", Geoprocessing.MakeValueArray(argumentsAssignDomain.ToArray()));
            }
            /*if (this.CollaborativeSpaceGeodatabase.IsOpen)
            {
                this.CollaborativeSpaceGeodatabase.Close();
            }*/
        }

        /// <summary>
        /// Création ou chargement des couches de signalement de l'espace collaboratif
        /// </summary>
        public async Task CreateOrLoadReportLayers()
        {
            // Signalements
            await this.LoadOrCreateCollaborativeSpaceLayers(
                Helper.name_layer_Signalement,
                "POINT",
                Helper.reportAttributes,
                0
                );

            // Croquis ponctuels
            await this.LoadOrCreateCollaborativeSpaceLayers(
                Helper.name_layer_Croquis_Point,
                "POINT",
                Helper.sketchAttributes,
                1
                );

            // Croquis linéaires
            await this.LoadOrCreateCollaborativeSpaceLayers(
                Helper.name_layer_Croquis_Ligne,
                "POLYLINE",
                Helper.sketchAttributes,
                2
                );

            // Croquis polygones
            await this.LoadOrCreateCollaborativeSpaceLayers(
                Helper.name_layer_Croquis_Polygone,
                "POLYGON",
                Helper.sketchAttributes,
                3
                );
        }

        /// <summary>
        /// Récupère une couche par son nom.
        /// </summary>
        /// <param name="layerName">Le nom de la couche qu'il faut récupérer.</param>
        /// <returns>La couche ou null si non trouvée</returns>
        public FeatureLayer GetLayerByName(string layerName)
        {
            // Enumération des couches et groupes de couches
            Map map = this.SearchMap(layerName);
            if (map == null)
            {
                return null;
            }
            IReadOnlyList<Layer> mapLayers = map.GetLayersAsFlattenedList();
            if (mapLayers == null)
            {
                return null;
            }
            foreach (var layer in mapLayers)
            {
                if (layer.Name == layerName)
                    return layer as FeatureLayer;
            }
            return null;
        }

        /// <summary>
        /// Teste l'existence d'une couche dans la carte en cours.
        /// </summary>
        /// <param name="name">Le nom de la couche dont on veut connaître l'existence.</param>
        /// <returns>true si la couche existe, false dans le cas contraire.</returns>
        public static bool IsLayerInMap(string layerName)
        {
            MapView activeMap = GetActiveMap();
            IReadOnlyList<Layer> layers = activeMap.Map.GetLayersAsFlattenedList();
            foreach (Layer layer in layers)
            {
                if (layer.Name == layerName)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// 
        /// de leur contenu.
        /// </summary>
        public void EmptyCollabFeatureClasses(string layerName)
        {
            try
            {
                if (!IsLayerInMap(layerName))
                {
                    throw new Exception(string.Format("La couche {0} n'existe pas dans la carte active, veuillez changer de carte.", layerName));
                }
                this.CollaborativeSpaceGeodatabase.EmptyFeatureClass(layerName);              
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Context.RemoveAllObjectsFromLayers : {0}\n", e.Message));
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reportUdating"></param>
        public void UpdateGeodatabase(Report reportUdating)
        {
            try
            {
                FeatureLayer reportLayer = this.GetLayerByName(Helper.name_layer_Signalement);
                FeatureClass reportFeatureClass = reportLayer.GetFeatureClass();

                EditOperation editOperation = new ();
                editOperation.Callback(context =>
                {
                    try
                    {
                        ArcGIS.Core.Data.QueryFilter queryFilter = new ()
                        {
                            WhereClause = string.Format("{0} = {1}", Helper.name_field_IdReport, reportUdating.Id)
                        };

                        using RowCursor rowCursor = reportFeatureClass.Search(queryFilter, false);
                        while (rowCursor.MoveNext())
                        {

                            using Feature feature = (Feature)rowCursor.Current;
                            // In order to update the Map and/or the attribute table.
                            // Has to be called before any changes are made to the row
                            context.Invalidate(feature);
                            feature[Helper.name_field_DateMAJ] = reportUdating.DateUpdate;
                            feature[Helper.name_field_DateValidation] = reportUdating.DateValidation;
                            feature[Helper.name_field_Reponse] = reportUdating.ConcatenateResponse();
                            feature[Helper.name_field_Statut] = reportUdating.Status;

                            feature.Store();
                            // Has to be called after the store too
                            context.Invalidate(feature);
                        }
                    }
                    catch (GeodatabaseException exObj)
                    {
                        throw new Exception (exObj.Message);
                    }

                }, reportFeatureClass);
                Helper.ExecuteEditOperation(editOperation);

                // If the table is non-versioned this is a no-op. If it is versioned, we need the Save to be done for the edits to be persisted.
                Project.Current.SaveEditsAsync();

            }
            catch (Exception e)
            {
                logger.Error(string.Format("Context.UpdateGeodatabase : {0}\n", e.Message));
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Insère sur la carte en cours une liste de signalements (avec leurs éventuels croquis associés).
        /// </summary>
        /// <param name="newReport">Le signalement qu'il faut placer sur la carte en cours.</param>
        public async Task<bool> InsertReports(List<Report> reports)
        {
            try
            {
                return await QueuedTask.Run(async () =>
                {
                    var createOperation = new ArcGIS.Desktop.Editing.EditOperation
                    {
                        Name = "Generate reports",
                        SelectNewFeatures = false
                    };

                    FeatureLayer reportLayer = GetLayerByName(Helper.name_layer_Signalement);
                    if (reportLayer is null)
                    {
                        logger.Error("Context.InsertReports : layer 'Signalement' introuvable.");
                        return false;
                    }

                    // Liste des ObjectIDs créés pour rollback
                    var createdObjectIDs = new List<(FeatureLayer layer, long objectID)>();

                    foreach (var newReport in reports)
                    {
                        var reportFields = GetFieldValuesForReport(newReport);
                        var rowToken = createOperation.Create(reportLayer, reportFields);
                        if (rowToken.ObjectID.HasValue)
                            createdObjectIDs.Add((reportLayer, rowToken.ObjectID.Value));

                        if (!newReport.IsCroquisEmpty())
                        {
                            foreach (var currSketch in newReport.Sketches)
                            {
                                if (currSketch.Points.Count == 0)
                                {
                                    logger.Warn($"Context.InsertReports : croquis ignoré (aucun point) pour le rapport {newReport.Id}.");
                                    continue;
                                }

                                int layerIndex = GetIndexLayerFromSketchType(currSketch.Type);
                                if (layerIndex == -1)
                                {
                                    logger.Error($"Context.InsertReports : type de croquis non reconnu : {currSketch.Type}. Annulation de l'opération.");
                                    createOperation.Abort();
                                    return false;
                                }

                                FeatureLayer sketchFeatureLayer = GetLayerByName(Helper.CollaborativeSpaceLayers[layerIndex]);
                                var sketchFields = GetFieldValuesForSketch(currSketch, newReport.Id);
                                if (sketchFields.Count > 0)
                                {
                                    var sketchToken = createOperation.Create(sketchFeatureLayer, sketchFields);
                                    if (sketchToken.ObjectID.HasValue)
                                        createdObjectIDs.Add((sketchFeatureLayer, sketchToken.ObjectID.Value));
                                }
                            }
                        }
                    }

                    if (createOperation.IsEmpty)
                    {
                        logger.Warn("Context.InsertReports : aucune modification détectée dans l'opération d'édition.");
                        return true;
                    }

                    bool result = createOperation.Execute();
                    if (!result)
                    {
                        logger.Error($"Context.InsertReports : erreur lors de l'exécution de l'opération - {createOperation.ErrorMessage}");
                        return false;
                    }

                    try
                    {
                        await Project.Current.SaveEditsAsync();
                        return true;
                    }
                    catch (Exception saveEx)
                    {
                        logger.Error($"Context.InsertReports : erreur lors de la sauvegarde - {saveEx.Message}");

                        // Rollback manuel : suppression des entités créées
                        var rollbackOperation = new ArcGIS.Desktop.Editing.EditOperation
                        {
                            Name = "Rollback InsertReports"
                        };

                        foreach (var (layer, objectID) in createdObjectIDs)
                        {
                            rollbackOperation.Delete(layer, objectID);
                        }

                        bool rollbackResult = rollbackOperation.Execute();
                        if (!rollbackResult)
                        {
                            logger.Error($"Context.InsertReports : rollback échoué - {rollbackOperation.ErrorMessage}");
                        }
                        else
                        {
                            logger.Warn("Context.InsertReports : rollback effectué avec succès.");
                        }

                        return false;
                    }
                });
            }
            catch (Exception e)
            {
                logger.Error($"Context.InsertReports : exception - {e.Message}\n{e}");
                return false;
            }
        }

        /// <summary>
        /// Insère sur la carte en cours une liste de signalements (avec leurs éventuels croquis associés).
        /// </summary>
        /// <param name="newReport">Le signalement qu'il faut placer sur la carte en cours.</param>
        public async Task<bool> InsertReportsSave(List<Report> reports)
        {
            try
            {
                return await QueuedTask.Run(() =>
                {
                    ArcGIS.Desktop.Editing.EditOperation createOperation = new ()
                    {
                        Name = "Generate reports",
                        SelectNewFeatures = false
                    };

                    FeatureLayer reportLayer = this.GetLayerByName(Helper.name_layer_Signalement);
                    if (reportLayer is null)
                    {
                        return false;
                    }
                    FeatureClass reportFeatureClass = reportLayer.GetFeatureClass();

                    // Placement des signalements importés et filtrés sur la carte.
                    foreach (Report newReport in reports)
                    {
                        // Signalement
                        var reportFields = new Dictionary<string, object>();
                        reportFields = GetFieldValuesForReport(newReport);

                        ArcGIS.Desktop.Editing.RowToken rowToken = createOperation.Create(reportLayer, reportFields);
                        long? objectID = rowToken.ObjectID;

                        // Croquis
                        if (!newReport.IsCroquisEmpty())
                        {
                            foreach (ArcGisProEspaceCollaboratif.Core.Sketch currSketch in newReport.Sketches)
                            {
                                if (currSketch.Points.Count == 0) continue;

                                // on caste le featureLayer en fonction du type du croquis pour utiliser la bonne couche associée
                                int layerIndex = GetIndexLayerFromSketchType(currSketch.Type);
                                if (layerIndex == -1)
                                {
                                    logger.Error(string.Format("Context.InsertReports : {0} {1}\n", "Type non reconnu : ", currSketch.Type.ToString()));
                                    continue;
                                }
                                FeatureLayer sketchFeatureLayer = GetLayerByName(Helper.CollaborativeSpaceLayers[layerIndex]);

                                // Création de l'objet croquis dans la classe correspondant à son type
                                var sketchFields = new Dictionary<string, object>();
                                sketchFields = GetFieldValuesForSketch(currSketch, newReport.Id);

                                if (sketchFields.Count > 0)
                                {
                                    createOperation.Create(sketchFeatureLayer, sketchFields);
                                }
                            }
                        }                       
                    }
                    if (createOperation.IsEmpty)
                    {
                        logger.Warn("Context.InsertReports : aucune modification détectée dans l'opération d'édition.");
                        return false;
                    }
                    bool result = createOperation.Execute();
                    if (!result)
                    {
                        string error = createOperation.ErrorMessage;
                        logger.Error(string.Format("Context.InsertReports : {0}\n", error));
                    }
                    else
                    {
                        Project.Current.SaveEditsAsync();
                    }

                    return true;

                });
            }

            catch (Exception e)
            {
                string message = string.Format("{0}\n{1}", e.Message, e.ToString());
                logger.Error(string.Format("Context.InsertReports : {0}\n", message));
                return false;
            }
        }

        /// <summary>
        /// Retourne l'index de la couche de croquis à utiliser dans la liste des couches (this.CollaborativeSpaceLayers)
        /// en fonction du type du croquis.
        /// Ordre des couches : [reportLayer, pointSketchLayer, lineSketchLayer, polygonSketchLayer]
        /// </summary>
        /// <param name="sketchType">Type du croquis à traiter</param>
        public static int GetIndexLayerFromSketchType(SketchType sketchType)
        {
            // [reportLayer, pointSketchLayer, lineSketchLayer, polygonSketchLayer]

            int indexLayer = -1;
            switch (sketchType)
            {
                case SketchType.Point:
                case SketchType.MultiPoint:
                case SketchType.Texte:
                    indexLayer = 1;
                    break;

                case SketchType.Ligne:
                case SketchType.MultiLigne:
                case SketchType.Fleche:
                    indexLayer = 2;
                    break;

                case SketchType.Polygone:
                case SketchType.Multipolygone:
                    indexLayer = 3;
                    break;

                default:
                    break;
            }

            return indexLayer;
        }

        /// <summary>
        /// Récupère les valeurs des champs de la table de signalements pour le nouveau signalement à insérer.
        /// </summary>
        /// <param name="newReport">Le signalement qu'il faut placer sur la carte en cours.</param>
        public static Dictionary<string, object> GetFieldValuesForReport(Report newReport)
        {
            // Signalement
            var reportFields = new Dictionary<string, object>
            {
                { Helper.name_field_IdReport, newReport.Id },
                { Helper.name_field_Auteur, newReport.Author.Name },
                { Helper.name_field_Insee, newReport.Insee },
                { Helper.name_field_Commune, newReport.Commune },
                { Helper.name_field_Departement, newReport.Departement.Name },
                { Helper.name_field_IDDepartement, newReport.Departement.Id },
                { Helper.name_field_DateCreation, newReport.DateCreation },
                { Helper.name_field_DateMAJ, newReport.DateUpdate },
                { Helper.name_field_DateValidation, newReport.DateValidation },
                { Helper.name_field_Statut, newReport.Status },
                { Helper.name_field_Themes, Helper.Limite(newReport.ConcatenateThemes()) },
                { Helper.name_field_Url, newReport.Lien },
                { Helper.name_field_Document, Helper.Limite(newReport.ConcatenateDocuments()) },
                { Helper.name_field_Message, Helper.Limite(newReport.Commentary) },
                { Helper.name_field_Reponse, Helper.Limite(newReport.ConcatenateResponse()) },
                { Helper.name_field_Autorisation, newReport.Authorisation },
                { Helper.name_field_Source, newReport.Source },
                { Helper.name_field_Shape, Helper.TransformPoint(newReport.Position) }
            };

            return reportFields;
        }

        /// <summary>
        /// Récupère les valeurs des champs de la table croquis concernée pour le croquis à insérer.
        /// </summary>
        /// <param name="currSketch">Croquis à ajouter.</param>
        /// <param name="idNewReport">Identifiant du signalement en cours d'ajout.</param>
        public static Dictionary<string, object> GetFieldValuesForSketch(Sketch currSketch, ulong idNewReport)
        {
            var sketchFields = new Dictionary<string, object>();

            try
            {
                // Récupération des attributs du croquis transmis par l'API (champ attributes)
                string attributes = "";
                foreach (ArcGisProEspaceCollaboratif.Core.SketchAttributes attribut in currSketch.Attributes)
                    attributes += string.Format("{0} = '{1}' | ", attribut.Name, attribut.Value);

                if (currSketch.Attributes.Count != 0)
                    attributes = attributes.Substring(0, attributes.Length - 3);

                // Préparation des attributs de l'objet croquis à créer
                sketchFields.Add(Helper.name_field_LienReport, idNewReport);
                sketchFields.Add(Helper.name_field_NomCroquis, currSketch.Name);
                sketchFields.Add(Helper.name_field_Attributs, Helper.Limite(attributes));

                // Remplissage de sa géométrie
                ArcGIS.Core.Geometry.MapPoint sketchPoint = Helper.TransformPoint(currSketch.Points.First());
                switch (currSketch.Type)
                {
                    default:
                        break;

                    case SketchType.Point:
                    case SketchType.Texte:
                    case SketchType.MultiPoint:
                        sketchFields.Add(Helper.name_field_Shape, sketchPoint);
                        break;

                    case SketchType.Ligne :
                    case SketchType.Fleche:
                    case SketchType.MultiLigne:
                        Polyline sketchLine = PolylineBuilderEx.CreatePolyline(Helper.GetPointCollectionFromSketch(currSketch));
                        sketchFields.Add(Helper.name_field_Shape, sketchLine);
                        break;

                    case SketchType.Polygone:
                    case SketchType.Multipolygone:
                        Polygon sketchPolygon = PolygonBuilderEx.CreatePolygon(Helper.GetPointCollectionFromSketch(currSketch));
                        sketchFields.Add(Helper.name_field_Shape, sketchPolygon);
                        break;
            
                }
            }

            catch (GeodatabaseException exObj)
            {
                logger.Error(string.Format("Context.CreateSketchObject : {0}\n", exObj.Message));
            }

            return sketchFields;
        }


        public static ArcGisProEspaceCollaboratif.Core.Box GetBBoxTer(List<Geometry> filterGeometries)
        {
            if (filterGeometries == null || filterGeometries.Count == 0)
                return new ArcGisProEspaceCollaboratif.Core.Box();

            SpatialReference wgs84 = SpatialReferenceBuilder.CreateSpatialReference(4326);
            EnvelopeBuilderEx builderEx = new EnvelopeBuilderEx(GeometryEngine.Instance.Project(filterGeometries[0], wgs84).Extent);

            foreach (var geom in filterGeometries.Skip(1))
            {
                if (geom == null) continue;
                var projected = GeometryEngine.Instance.Project(geom, wgs84);
                builderEx.Union(projected.Extent);
            }

            return new ArcGisProEspaceCollaboratif.Core.Box(builderEx.XMin, builderEx.YMin, builderEx.XMax, builderEx.YMax);
        }


        /// <summary>
        /// Calcule la BBox Ripart qui enveloppe une liste d'objects géométriques.
        /// </summary>
        /// <param name="geometriesFiltres">La liste des Geometry dont on veut obtenir l'enveloppe globale.</param>
        /// <returns>Ripart.Core.Box qui enveloppe tous les Geometry de <paramref name="geometriesFiltres"/>.</returns>
        public static ArcGisProEspaceCollaboratif.Core.Box GetBBoxBis(List<Geometry> filterGeometries)
        {
            if (filterGeometries.Count == 0)
                return new ArcGisProEspaceCollaboratif.Core.Box();

            // La géométrie de filtre spatial doit être en WGS84 dans tous les cas.
            SpatialReference tmpSpatialReference = SpatialReferenceBuilder.CreateSpatialReference(4326);
            // Initialisation de la bbox avec l'emprise de la première géométrie
            Geometry tmp = GeometryEngine.Instance.Project(filterGeometries[0], tmpSpatialReference);
            EnvelopeBuilderEx builderEx = new EnvelopeBuilderEx(tmp.Extent);
            int nb = 0;
            foreach (Geometry geom in filterGeometries)
            {
                if (nb == 0)
                {
                    continue;
                }
                Geometry tmpGeom = GeometryEngine.Instance.Project(geom, tmpSpatialReference);
                builderEx.Union(tmpGeom.Extent);
            }
            return new ArcGisProEspaceCollaboratif.Core.Box(builderEx.XMin, builderEx.YMin, builderEx.XMax, builderEx.YMax);
        }

        /// <summary>
        /// Calcule la BBox Ripart qui enveloppe une liste d'objects géométriques.
        /// </summary>
        /// <param name="geometriesFiltres">La liste des Geometry dont on veut obtenir l'enveloppe globale.</param>
        /// <returns>Ripart.Core.Box qui enveloppe tous les Geometry de <paramref name="geometriesFiltres"/>.</returns>
        public static ArcGisProEspaceCollaboratif.Core.Box GetBBox(List<Geometry> filterGeometries)
        {
            if (filterGeometries.Count == 0)
                return new ArcGisProEspaceCollaboratif.Core.Box();
            // La géométrie de filtre spatial doit être en WGS84 dans tous les cas.
            SpatialReference tmpSpatialReference = SpatialReferenceBuilder.CreateSpatialReference(4326);
            // Initialisation de la bbox avec l'emprise de la première géométrie
            Envelope bbox = filterGeometries[0].Extent;
            int nb = 0;
            foreach (Geometry geom in filterGeometries)
            {
                if (nb == 0)
                {
                    continue;
                }
                Geometry tmp = GeometryEngine.Instance.Project(geom, tmpSpatialReference);
                Envelope bboxTemp = tmp.Extent;
                bbox.Union(bboxTemp);
                nb++;
            }
            return new ArcGisProEspaceCollaboratif.Core.Box(bbox.XMin, bbox.YMin, bbox.XMax, bbox.YMax);
        }

        /// <summary>
        /// Récupère à partir d'une couche donnée par nom, la liste des géométries destinées à servir au filtrage spatial lors de l'importation des signalements .
        /// </summary>
        /// <param name="filterLayerName">Nom du calque devant contenir les objects utiles pour le filtrage spatial.</param>
        /// <returns>Liste d'Geometry contenant les géométries devant servir pour le filtrage spatial lors de l'importation des signalements.</returns>
        public List<Geometry> GetSpatialFilterGeometry(string filterLayerName)
        {
            List<Geometry> spatialFilterGeometry = new ();
            FeatureLayer filterLayer = this.GetLayerByName(filterLayerName);

            if (filterLayer == null)
                return spatialFilterGeometry;

            FeatureClass featureClassFilter = filterLayer.GetFeatureClass();
            QueryFilter spatialQueryFilter = new ();

            RowCursor rowCursor = featureClassFilter.Search(
                        spatialQueryFilter,
                        false // important : sinon on n'obtient qu'un seul objet
                    );

            // Si la référence spatiale de la carte est différente de celle par défaut WGS84
            // this.spatialReference = Helper.IsDefaultSpatialReference();

            // On parcourt les objets de la feature class utilisée pour le filtre spatial
            while (rowCursor.MoveNext())
            {
                Feature featureSpatialFilter = rowCursor.Current as Feature;
                //spatialFilterGeometry.Add(featureSpatialFilter.GetShape());
                Geometry geomFeature = GeometryEngine.Instance.Project(featureSpatialFilter.GetShape(), SpatialReferences.WGS84);
                spatialFilterGeometry.Add(geomFeature);
            }

            return spatialFilterGeometry;
        }

        public void GetConnexionEspaceCollaboratif(ref ArcGisProEspaceCollaboratif.Core.Client connexionServer)
        {
            logger.Debug("GetConnexionEspaceCollaboratif ");
            this.URLHost = Helper.LoadUrlhost();
            logger.Debug("URLHost : " + this.URLHost);

            ConnectViewModel connectViewModel = new (this)
            {
                Uri = this.URLHost
            };
            connectViewModel.connectView.DataContext = connectViewModel;

            // Recherche du login par défaut dans le fichier XML de paramétrage
            connectViewModel.Login = Helper.LoadLogin();

            // Lancement du formulaire de saisi du login et mot de passe
            bool? dialogResult = connectViewModel.connectView.ShowDialog();

            // Si l'utilisateur a cliqué sur le bouton "Annuler" ou sur la croix de connectViewModel
            // il n'y aura pas de connexion
            if (dialogResult == false)
            {
                connectViewModel.connectView.Close();
                return;
            }

            // Récupération du login, du mot de passe introduits et la connexion serveur.
            if (connectViewModel.ConnexionServer != null)
            {
                this.Login = connectViewModel.Login;
                this.Password = connectViewModel.Password;
                connexionServer = connectViewModel.ConnexionServer;
                return;
            }
        }

        /// <summary>
        /// Affichage des information de connexion à l'espace collaboratif 
        /// </summary>
        public void DisplayInformationsAfterConnection()
        {
            FeedbackInformationViewModel feedbackInformationViewModel = new ();
            feedbackInformationViewModel.feedbackInformationView.DataContext = feedbackInformationViewModel;
            string logoByDefault = "/ArcGisProEspaceCollaboratif;component/Resources/LogoIGN.gif";
            // L'utilisateur peut appartenir à un groupe, mais sans logo, on affiche le logo IGN
            if (Profil.Logo == "")
            {
                feedbackInformationViewModel.Logo = logoByDefault;
            }
            // Le logo du groupe auquel l'utilisateur appartient
            else if (!string.IsNullOrEmpty(Profil.Logo))
            {
                feedbackInformationViewModel.Logo = string.Format("{0}{1}", this.URLHost, Profil.Logo);
            }
            // Cas d'un profil par défaut -> logo IGN
            if (Profil.Title == "Profil par défaut")
            {
                feedbackInformationViewModel.Logo = logoByDefault;
            }
            string message = "Connexion réussie à l'Espace collaboratif\n\n";
            message += string.Format(" Serveur : {0}\n", this.URLHost);
            message += string.Format(" Login : {0}\n", this.Login);
            message += string.Format(" Groupe : {0}\n", Profil.Title);
            if (Profil.Zone == ZoneGeographique.UNDEFINED)
            {
                string zoneExtraction = Helper.LoadNameLayerForSpatialFilter();
                if (zoneExtraction == "" || zoneExtraction.Length == 0)
                {
                    message += " Zone : pas de zone définie\n";
                }
                else
                {
                    message += string.Format(" Zone : {0}\n", zoneExtraction);
                }
            }
            else
            {
                message += string.Format(" Zone : {0}\n", Profil.Zone);
            }
            feedbackInformationViewModel.MessageFeedback = message;
            bool? dialogResult = feedbackInformationViewModel.feedbackInformationView.ShowDialog();
            if (dialogResult == false)
            {
                feedbackInformationViewModel.feedbackInformationView.Close();
            }

            Helper.SaveLogin(this.Login);
            Helper.SaveActiveGroup(Profil.Title);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connexionServer"></param>
        /// <returns></returns>
        public bool DisplayFormChoiceGroup(ref ArcGisProEspaceCollaboratif.Core.Client connexionServer)
        {
            // si l'utilisateur appartient à 1 seul groupe, celui-ci est déjà actif
            // si l'utilisateur n'appartient à aucun groupe, un profil par défaut
            // est attribué mais il ne contient pas d'infosgeogroupes
            if (this.Profil.Geogroupes.Count < 1)
            {
                // si l'utilisateur n'a pas de profil, il faut indiquer que le groupe actif est vide
                if (this.Profil.Title == "défaut")
                {
                    Helper.SaveActiveGroup("Aucun");
                }
            }
            else if (this.Profil.Geogroupes.Count == 1)
            {
                Helper.SaveActiveGroup(this.Profil.Group.Name);

                // On enregistre le groupe comme groupe préféré (par défaut) pour la création de signalement
                // Si ce n'est pas le même qu'avant, on vide les thèmes préférés
                string pGroup = Helper.Load_PreferredGroup();
                if (pGroup != Profil.Group.Name)
                {
                    Helper.Save_PreferredThemes(new List<string>());
                }
                Helper.Save_PreferredGroup(Profil.Group.Name);
            }
            // sinon le choix d'un autre groupe est présenté à l'utilisateur
            // le formulaire est proposé même si l'utilisateur n'appartient qu'à un groupe
            GroupChoiceViewModel groupChoiceViewModel = new (Profil);
            groupChoiceViewModel.groupChoiceView.DataContext = groupChoiceViewModel;
            bool? dialogResult = groupChoiceViewModel.groupChoiceView.ShowDialog();
            // Si l'utilisateur a cliqué sur le bouton "Annuler"
            // dans son choix du groupe, on sort
            if (dialogResult == false)
            {
                groupChoiceViewModel.groupChoiceView.Close();
                return false;
            }
               
            // le choix du nouveau profil est validé
            // le nouvel id et nom du groupe sont retournés dans un tuple
            (string, string) idNomGroupe = groupChoiceViewModel.Profile.IdNameGroup;

            // si l'utilisateur n'appartient qu'à un seul groupe, le profil chargé reste actif
            if (groupChoiceViewModel.Profile.Geogroupes.Count == 1)
            {
                this.Profil = groupChoiceViewModel.Profile;
            }
            else if (this.Profil.Group.Name != idNomGroupe.Item2)
            {
                // récupère le profil et un message dans un tuple
                (Profile, string) profilMessage = connexionServer.SetChangeUserProfil(idNomGroupe.Item1);
                string messTmp = profilMessage.Item2;

                // SetChangeUserProfil retourne un message "Le profil pour le groupe xxx est déjà actif"
                if (messTmp.Contains("actif"))
                {
                    // le profil chargé reste actif
                    this.Profil = groupChoiceViewModel.Profile;
                }
                else
                {
                    // setChangeUserProfil retourne un message vide le nouveau profil devient actif
                    this.Profil = profilMessage.Item1;
                }
            }

            // Sauvegarde du groupe actif dans le xml du projet utilisateur
            this.Groupeactif = idNomGroupe.Item2;
            Helper.SaveActiveGroup(this.Groupeactif);

            // On enregistre le groupe comme groupe préféré pour la création de signalement
            // Si ce n'est pas le même qu'avant, on vide les thèmes préférés
            string preferredGroup = Helper.Load_PreferredGroup();
            if (preferredGroup != Profil.Group.Name)
            {
                Helper.Save_PreferredThemes(new List<string>());
            }
            Helper.Save_PreferredGroup(Profil.Group.Name);

            return true;
        }
 
        /// <summary>
        /// Donne le décompte de signalements Ripart présentes sur la carte en cours ayant le statut indiqué.
        /// </summary>
        /// <param name="status">Le statut des signalements Ripart qu'on veut dénombrer.</param>
        /// <returns>Le décompte de signalements Ripart sur la carte ayant le statut indiqué.</returns>
        public long CountReportsByStatus(int status)
        {
            FeatureLayer reportLayer = this.GetLayerByName(Helper.name_layer_Signalement);
            FeatureClass reportFeatureClass = reportLayer.GetFeatureClass();
            if (reportFeatureClass == null)
            {
                throw new Exception(string.Format("Impossible de récupérer le nombre d'objets par status pour la couche {0}.", Helper.name_layer_Signalement));
            }
            QueryFilter queryFilter = new ()
            {
                WhereClause = Helper.name_field_Statut + " = " + status
            };
           
            return reportFeatureClass.GetCount(queryFilter);
        }

        /// <summary>
        /// Donne le décompte de signalements Ripart présents sur la carte en cours ayant le statut indiqué.
        /// </summary>
        /// <param name="statut">Le statut des signalements Ripart qu'on veut dénombrer.</param>
        /// <returns>Le décompte de signalements Ripart sur la carte ayant le statut indiqué.</returns>
        public long CountReportsByStatus(ArcGisProEspaceCollaboratif.Core.Status.EnumStatus status)
        {
            return this.CountReportsByStatus((int)status);
        }
    }
}
