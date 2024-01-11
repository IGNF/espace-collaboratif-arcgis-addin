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
            this.Init(MapView.Active);
        }


        /// <summary>
        /// initialisation du contexte et des éléments Ripart
        /// </summary>
        /// <param name="activeView">L'activeView associée à la carte en cours.</param>
        private async Task Init(MapView activeView)
        {
            this.MapActiveView = activeView;

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
            await this.CreateOrLoadReportLayers();

            logger.Debug("Initialisation du contexte et des éléments de l'Espace collaboratif");
        }

        #endregion

        /// <summary>
        /// Teste si le fichier de configuration espaceco.xml n'existe pas dans le répertoire de travail, on le copie 
        /// du répertoire d'installation 
        /// </summary>
        /// <returns>true si le fichier de configuration espaceco.xml est à côté de la carte en cours.</returns>
        public void CheckConfigFile()
        {
            if (this.DirectoryWorking == "")
            {
                Project project = Project.Current;
                this.DirectoryWorking = System.IO.Path.GetDirectoryName(project.Path);
            }
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
            // Pour la couche donnée en entrée, vide la geodatabase pour supprimer les objets dans la carte
            this.EmptyCollabFeatureClasses(fcName);

            FeatureLayer collabSpaceLayer;
            if (!this.IsLayerInMap(fcName))
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
                    this.MapActiveView.Map
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

            // Définition des couleurs
            var pendingColor = CIMColor.CreateRGBColor(255, 170, 0);
            var validColor = CIMColor.CreateRGBColor(0, 255, 0);
            var rejectColor = CIMColor.CreateRGBColor(255, 0, 0);

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
            string fcPath = this.CollaborativeSpaceGeodatabase.GeoDatabasePath + "\\" + fcName;
            bool bFeatureClassExist = this.CollaborativeSpaceGeodatabase.IsFeatureClassExists(fcName);
            
            // Si la feature class existe déjà, on l'ouvre et on l'ajoute comme couche (FeatureLayer) à la carte
            FeatureLayer collabSpaceLayer;
            if (bFeatureClassExist)
            { 
                collabSpaceLayer = this.LoadCollabLayer(fcName, layerPosition);
            }
            // Si la feature class n'existe pas dans la geodatabase du projet, on la crée
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
        }

        /// <summary>
        /// Création ou chargement des couches de signalement de l'espace collaboratif
        /// </summary>
        public async Task CreateOrLoadReportLayers()
        {
            if (this.CollaborativeSpaceGeodatabase is null)
            {
                this.CollaborativeSpaceGeodatabase = new CollaborativeSpaceGeodatabase();
            }
            
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
            if (this.MapActiveView == null)
            {
                this.MapActiveView = MapView.Active;
            }
            // Enumération des couches et groupes de couches
            IReadOnlyList<Layer> mapLayers = this.MapActiveView.Map.GetLayersAsFlattenedList();
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
        public bool IsLayerInMap(string layerName)
        {
            if (this.MapActiveView == null)
            {
                return false;
            }
            IReadOnlyList<Layer> mapLayers = this.MapActiveView.Map.GetLayersAsFlattenedList();
            foreach (var layer in mapLayers)
            {
                if (layer.Name == layerName) return true;
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
                if (!this.IsLayerInMap(layerName))
                {
                    return;
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

                        using (RowCursor rowCursor = reportFeatureClass.Search(queryFilter, false))
                        {
                            while (rowCursor.MoveNext())
                            {
                                
                                using (Feature feature = (Feature)rowCursor.Current)
                                {
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

                        createOperation.Create(reportLayer, reportFields);

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
                                    logger.Error(string.Format("Context.CreerPointSignalement : {0} {1}\n", "Type non reconnu : ", currSketch.Type.ToString()));
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

                    bool result = createOperation.Execute();

                    if (!result)
                    {
                        string error = createOperation.ErrorMessage;
                        logger.Error(string.Format("Context.CreerPointSignalement : {0}\n", error));
                    }
                    else
                    {
                        Project.Current.SaveEditsAsync();
                    }

                    return result;

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

        /// <summary>
        /// Calcule la BBox Ripart qui enveloppe une liste d'objects géométriques.
        /// </summary>
        /// <param name="geometriesFiltres">La liste des Geometry dont on veut obtenir l'enveloppe globale.</param>
        /// <returns>Ripart.Core.Box qui enveloppe tous les Geometry de <paramref name="geometriesFiltres"/>.</returns>
        public static ArcGisProEspaceCollaboratif.Core.Box GetBBox(List<Geometry> filterGeometries)
        {
            if (filterGeometries.Count == 0)
                return new ArcGisProEspaceCollaboratif.Core.Box();

            // Initialisation de la bbox avec l'emprise de la première géométrie
            Envelope bbox = filterGeometries[0].Extent;

            foreach (Geometry geom in filterGeometries)
            {
                Envelope bboxTemp = geom.Extent;
                bbox.Union(bboxTemp);
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

            // On parcourt les objets de la feature class utilisée pour le filtre spatial
            while (rowCursor.MoveNext())
            {
                Feature featureSpatialFilter = rowCursor.Current as Feature;
                Geometry geomFeature = GeometryEngine.Instance.Project(featureSpatialFilter.GetShape(), this.SpatialReference);
                spatialFilterGeometry.Add(geomFeature);
            }

            return spatialFilterGeometry;
        }

        public Client GetConnexionEspaceCollaboratif()
        {
            logger.Debug("GetConnexionEspaceCollaboratif ");
            this.URLHost = Helper.LoadUrlhost();
            logger.Debug("URLHost : " + this.URLHost);

            ConnectViewModel connectViewModel = new ()
            {
                Uri = this.URLHost
            };
            connectViewModel.connectView.DataContext = connectViewModel;

            // Recherche du login par défaut dans le fichier XML de paramétrage
            connectViewModel.Login = Helper.LoadLogin();

            // Lancement du formulaire de saisi du login et mot de passe
            bool? dialogResult = connectViewModel.connectView.ShowDialog();
            // Si l'utilisateur a cliqué sur le bouton "Annuler"
            // il n'y aura pas de connexion
            if (dialogResult == false)
            {
                connectViewModel.connectView.Close();
                return null;
            }
            // Récupération du login et mot de passe introduits.
            this.Login = connectViewModel.Login;
            this.Password = connectViewModel.Password;

            try
            {
                Client connexionServer = new (
                        this.URLHost,
                        this.Login,
                        this.Password
                    );
                
                logger.Info("Création de la connexion au serveur " + connexionServer.ToString());
                
                // Récupération du profil utilisateur
                this.Profil = connexionServer.GetProfile();
                if (this.Profil == null)
                {
                    string message = "Récupération du profil utilisateur impossible";
                    logger.Error(string.Format("Context.GetConnexionEspaceCollaboratif : {0}\n", message));
                    throw new ArgumentNullException(message);
                }

                // Affichage de la boite du choix du groupe à l'utilisateur
                if (!this.DisplayFormChoiceGroup(ref connexionServer))
                {
                    return null;
                }

                // Affichage des infos suite à la connexion à l'Espace collaboratif
                this.DisplayInformationsAfterConnection();

                return connexionServer;
            }
            catch (Exception erreurConnexion)
            {
                this.Password = "";
                
                switch (erreurConnexion.Message.ToString())
                {
                    case "(401) Unauthorized":
                        string message = "Login et/ou mot de passe incorrect(s)";
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.ERROR);
                        break;

                    case "Login inconnu":
                        message = string.Format("''{0}'' n'est pas un utilisateur enregistré dans un groupe de l'Espace collaboratif.", this.Login);
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.ERROR);
                        break;

                    case "no_group":
                        message = "Accès refusé. L'utilisateur n'appartient à aucun groupe.";
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.ERROR);
                        break;

                    default:
                        message = string.Format("Impossible d'accéder au service de l'Espace collaboratif à l'adresse suivante : {0}\n\nVeuillez contacter le support. Erreur : {1}\n", this.URLHost, erreurConnexion.Message.ToString());
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.ERROR);
                        break;
                }
            }
            return null;
        }

        /// <summary>
        /// Affichage des information de connexion à l'espace collaboratif 
        /// </summary>
        public void DisplayInformationsAfterConnection()
        {
            FeedbackInformationViewModel feedbackInformationViewModel = new ();
            feedbackInformationViewModel.feedbackInformationView.DataContext = feedbackInformationViewModel;

            // Le logo du groupe auquel l'utilisateur appartient
            if (!string.IsNullOrEmpty(Profil.Logo))
            {
                feedbackInformationViewModel.Logo = string.Format("{0}{1}", this.URLHost, Profil.Logo);
            }
            // L'utilisateur sans groupe à un profil par défaut, on affiche le logo IGN
            else if (Profil.Title == "Profil par défaut")
            {
                feedbackInformationViewModel.Logo = "/ArcGisProEspaceCollaboratif;component/Resources/LogoIGN.gif";
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
                else
                {
                    Helper.SaveActiveGroup(this.Profil.Group.Name);

                    // On enregistre le groupe comme groupe préféré (par défaut) pour la création de signalement
                    // Si ce n'est pas le même qu'avant, on vide les thèmes préférés
                    string preferredGroup = Helper.Load_PreferredGroup();
                    if (preferredGroup != Profil.Group.Name)
                    {
                        Helper.Save_PreferredThemes(new List<string>());
                    }
                    Helper.Save_PreferredGroup(Profil.Group.Name);
                }
            }
            else
            {
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
                else
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
            }
            return true;
        }

        /// <summary>
        /// Transforme en croquis les objets sélectionnés dans la carte en cours.
        /// </summary>
        /// <returns>Liste de croquis créés à partir des objects sélectionnés.</returns>
        public List<ArcGisProEspaceCollaboratif.Core.Sketch > MakeSketchFromSelection()
        {
            List<ArcGisProEspaceCollaboratif.Core.Sketch> sketches = new ();

            if (this.MapActiveView == null)
            {
                return sketches;
            }

            string message = "";
            // Get the currently selected features in the map
            QueuedTask.Run(()=>
            {
                SelectionSet selectedFeatures = this.MapActiveView.Map.GetSelection();
                foreach (KeyValuePair<MapMember, List<long>> kvp in selectedFeatures.ToDictionary())
                {
                    //get the layer of the selected feature
                    var featureLayer = kvp.Key as FeatureLayer;
                    List<FieldDescription> fieldDescription = featureLayer.GetFieldDescriptions();
                    List<long> lOid = kvp.Value;
                    
                    foreach (long oid in lOid)
                    {
                        QueuedTask.Run(() =>
                        {
                            // Initialisation d'un nouveau croquis avec la géométrie
                            var inspector = featureLayer.Inspect(oid);
                            ArcGIS.Core.Geometry.Geometry geometryFeature = inspector.Shape;
                            ArcGisProEspaceCollaboratif.Core.Sketch tmpSketch = Helper.MakeSketch(geometryFeature);
                            if (tmpSketch == null)
                            {
                                message += string.Format("Le croquis pour le signalement {0} n'a pu être créé.\n", oid);
                            }
                            // Ajout des attributs au nouveau croquis
                            Dictionary<string, string> attributes = Helper.GetAttributes(inspector, fieldDescription);
                            foreach (KeyValuePair<string, string> kv in attributes)
                            {
                                tmpSketch.AddAttribute(kv.Key, kv.Value);
                            }

                            sketches.Add(tmpSketch);
                        }); 
                    }
                }
            });
            System.Windows.MessageBoxResult result = System.Windows.MessageBoxResult.Cancel;
            if (!string.IsNullOrEmpty(message))
            {
                message += "Voulez-vous continuer ?";
                result = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.ERROR);
            }  
            if (result == System.Windows.MessageBoxResult.OK ||
                result == System.Windows.MessageBoxResult.Yes)
            {
                return sketches;
            }
            else { return null; }
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
