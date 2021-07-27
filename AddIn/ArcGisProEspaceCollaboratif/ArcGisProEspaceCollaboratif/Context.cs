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
        /// La clé Geoportail de l'utilisateur
        /// </summary>
        public string CleGeoportail { get; set; } = "";

        /// <summary>
        /// Le groupe sélectionné par l'utilisateur sur lequel il veut travailler
        /// </summary>
        public string Groupeactif { get; set; } = "";

        /// <summary>
        /// La liste des calques dédiés pour l'espace collaboratif dans la carte en cours
        /// </summary>
        public List<FeatureLayer> CollaborativeSpaceLayers { get; set; } = new List<FeatureLayer>();

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
            this.Init(MapView.Active);
        }

        /// <summary>
        /// initialisation du contexte et des éléments Ripart
        /// </summary>
        /// <param name="activeView">L'activeView associée à la carte en cours.</param>
        public void Init(MapView activeView)
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
            this.CollaborativeSpaceGeodatabase = new CollaborativeSpaceGeodatabase();

            //création ou chargement des couches ripart
            //TODO : question Noémie pourquoi cette création ici ?
            //var bLayersLoaded = CreateOrLoadReportLayers();

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
        /// Création ou chargement des couches de signalement de l'espace collaboratif
        /// </summary>
        public async Task CreateOrLoadReportLayers()
        {
            // Création ou chargement des calques dédiés à de l'espace collaboratif s'ils sont absents de la carte en cours.
            string polygonSketchLayer = Helper.name_layer_Croquis_Polygone;
            string lineSketchLayer = Helper.name_layer_Croquis_Ligne;
            string pointSketchLayer = Helper.name_layer_Croquis_Point;
            string reportLayer = Helper.name_layer_Signalement;

            // Signalements
            await Helper.LoadOrCreateCollaborativeSpaceLayer(
                reportLayer,
                "POINT",
                Helper.reportAttributes,
                0
                );

            // Croquis ponctuels
            await Helper.LoadOrCreateCollaborativeSpaceLayer(
                pointSketchLayer,
                "POINT",
                Helper.sketchAttributes,
                1
                );

            // Croquis linéaires
            await Helper.LoadOrCreateCollaborativeSpaceLayer(
                lineSketchLayer,
                "POLYLINE",
                Helper.sketchAttributes,
                2
                );

            // Croquis polygones
            await Helper.LoadOrCreateCollaborativeSpaceLayer(
                polygonSketchLayer,
                "POLYGON",
                Helper.sketchAttributes,
                3
                );

            // Ajout des couches à la liste collaboratifSpaceLayers
            this.CollaborativeSpaceLayers.Clear();
            this.CollaborativeSpaceLayers.Add(GetLayerByName(reportLayer));
            this.CollaborativeSpaceLayers.Add(GetLayerByName(pointSketchLayer));
            this.CollaborativeSpaceLayers.Add(GetLayerByName(lineSketchLayer));
            this.CollaborativeSpaceLayers.Add(GetLayerByName(polygonSketchLayer));
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
        /// Vide les couches "Signalement", "Croquis_EC_Polygone", "Croquis_EC_Ligne", "Croquis_EC_Point"
        /// de tous leurs contenus.
        /// </summary>
        public void RemoveAllObjectsFromLayers()
        {
            try
            {
                foreach (FeatureLayer layer in this.CollaborativeSpaceLayers)
                {
                    FeatureClass fcCollabSpace = layer.GetFeatureClass();
                    Geoprocessing.ExecuteToolAsync("TruncateTable_management", Geoprocessing.MakeValueArray(fcCollabSpace));
                }
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

                EditOperation editOperation = new EditOperation();
                editOperation.Callback(context =>
                {
                    try
                    {
                        QueryFilter queryFilter = new QueryFilter
                        {
                            WhereClause = string.Format("N_Remarque = {0}", reportUdating.Id)
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
                                    feature["Date_MAJ"] = reportUdating.DateUpdate;
                                    feature["Date_de_validation"] = reportUdating.DateValidation;
                                    feature["Réponses"] = reportUdating.ConcatenateResponse();
                                    feature["Statut"] = reportUdating.Status;

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
                    ArcGIS.Desktop.Editing.EditOperation createOperation = new ArcGIS.Desktop.Editing.EditOperation
                    {
                        Name = "Generate reports",
                        SelectNewFeatures = false
                    };

                    FeatureLayer reportLayer = this.GetLayerByName(Helper.name_layer_Signalement);
                    FeatureClass reportFeatureClass = reportLayer.GetFeatureClass();

                    // Barre de progression - A CHANGER
                    FormProgressDownload progressDownload = new FormProgressDownload();
                    int countReports = 0;
                    progressDownload.GetProgressBar().Maximum = reports.Count;
                    progressDownload.GetProgressBar().Step = 1;
                    progressDownload.SetMaxProgressor(reports.Count);
                    progressDownload.SetBar(1);
                    progressDownload.Show();

                    this.Client.SetProgressBar(progressDownload.GetProgressBar());

                    // Placement des signalements importés et filtrés sur la carte.
                    foreach (Report newReport in reports)
                    {
                        countReports++;
                        progressDownload.NextProgressor("Placement sur la carte du signalement " + countReports + "/" + reports.Count);

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
                                int layerIndex = this.GetIndexLayerFromSketchType(currSketch.Type);
                                if (layerIndex == -1)
                                {
                                    logger.Error(string.Format("Context.CreerPointSignalement : {0} {1}\n", "Type non reconnu : ", currSketch.Type.ToString()));
                                    continue;
                                }
                                FeatureLayer sketchFeatureLayer = this.CollaborativeSpaceLayers[layerIndex];

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
                    progressDownload.Close();

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
        public int GetIndexLayerFromSketchType(SketchType sketchType)
        {
            // [reportLayer, pointSketchLayer, lineSketchLayer, polygonSketchLayer]

            int indexLayer = -1;
            switch (sketchType)
            {
                case SketchType.Point:
                    indexLayer = 1;
                    break;

                case SketchType.Texte:
                    indexLayer = 1;
                    break;

                case SketchType.Ligne:
                    indexLayer = 2;
                    break;

                case SketchType.Fleche:
                    indexLayer = 2;
                    break;

                case SketchType.Polygone:
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
        public Dictionary<string, object> GetFieldValuesForReport(Report newReport)
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
        public Dictionary<string, object> GetFieldValuesForSketch(Sketch currSketch, ulong idNewReport)
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
                        sketchFields.Add(Helper.name_field_Shape, sketchPoint);
                        break;

                    case SketchType.Texte:
                        sketchFields.Add(Helper.name_field_Shape, sketchPoint);
                        break;

                    case SketchType.Ligne :
                        Polyline sketchLine = PolylineBuilder.CreatePolyline(Helper.GetPointCollectionFromSketch(currSketch));
                        sketchFields.Add(Helper.name_field_Shape, sketchLine);
                        break;

                    case SketchType.Fleche:
                        Polyline sketchArrow = PolylineBuilder.CreatePolyline(Helper.GetPointCollectionFromSketch(currSketch));
                        sketchFields.Add(Helper.name_field_Shape, sketchArrow);
                        break;

                    case SketchType.Polygone:
                        Polygon sketchPolygon = PolygonBuilder.CreatePolygon(Helper.GetPointCollectionFromSketch(currSketch));
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
        public ArcGisProEspaceCollaboratif.Core.Box GetBBox(List<Geometry> filterGeometries)
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
            List<Geometry> spatialFilterGeometry = new List<Geometry>();

            FeatureLayer filterLayer = this.GetLayerByName(filterLayerName);

            if (filterLayer == null)
                return spatialFilterGeometry;

            FeatureClass featureClassFilter = filterLayer.GetFeatureClass();
            QueryFilter spatialQueryFilter = new QueryFilter();

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


        public ArcGisProEspaceCollaboratif.Core.Client GetConnexionEspaceCollaboratif()
        {
            logger.Debug("GetConnexionEspaceCollaboratif ");
            this.CleGeoportail = Helper.LoadGeoportalKey();
            this.URLHost = Helper.LoadUrlhost();
            logger.Debug("URLHost : " + this.URLHost);

            var connectViewModel = new ConnectViewModel()
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
                string message = "Opération annulée par l'utilisateur";
                logger.Error(string.Format("Context.GetConnexionEspaceCollaboratif : {0}\n", message));
                throw new Exception (message);
            }
            // Récupération du login et mot de passe introduits.
            this.Login = connectViewModel.Login;
            this.Password = connectViewModel.Password;

            try
            {
                ArcGisProEspaceCollaboratif.Core.Client connexionServer = new Client(
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

                // Affichage de la boite du choix du groupe et de la clé Géoportail à l'utilisateur
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
                        string.Format("Context.GetConnexionEspaceCollaboratif : {0}\n", message);
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.ERROR);
                        break;

                    case "Login inconnu":
                        message = string.Format("''{0}'' n'est pas un utilisateur enregistré dans un groupe de l'Espace collaboratif.", this.Login);
                        string.Format("Context.GetConnexionEspaceCollaboratif : {0}\n", message);
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.ERROR);
                        break;

                    case "no_group":
                        message = "Accès refusé. L'utilisateur n'appartient à aucun groupe.";
                        string.Format("Context.GetConnexionEspaceCollaboratif : {0}\n", message);
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.ERROR);
                        break;

                    default:
                        message = string.Format("Impossible d'accéder au service de l'Espace collaboratif à l'adresse suivante : {0}\n\nVeuillez contacter le support. Erreur : {1}\n", this.URLHost, erreurConnexion.Message.ToString());
                        string.Format("Context.GetConnexionEspaceCollaboratif : {0}\n", message);
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
            var connectInfoViewModel = new FeedbackInformationViewModel();
            connectInfoViewModel.feedbackInformationView.DataContext = connectInfoViewModel;

            // Le logo du groupe auquel l'utilisateur appartient
            if (!string.IsNullOrEmpty(Profil.Logo))
            {
                connectInfoViewModel.Logo = string.Format("{0}{1}", this.URLHost, Profil.Logo);
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
            message += string.Format(" Clé Géoportail : {0}", this.CleGeoportail);
            connectInfoViewModel.MessageFeedback = message;
            connectInfoViewModel.feedbackInformationView.ShowDialog();

            Helper.SaveLogin(this.Login);
            Helper.SaveActiveGroup(Profil.Title);
            Helper.SaveGeoportalKey(this.CleGeoportail);
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
                // Par défaut, on enregistre la clé Géoportail de démonstration
                Helper.SaveGeoportalKey(Constantes.DEMO);
            }
            else
            {
                // sinon le choix d'un autre groupe est présenté à l'utilisateur
                // le formulaire est proposé même si l'utilisateur n'appartient qu'à un groupe
                // afin qu'il puisse remplir sa clé Géoportail
                var groupChoiceViewModel = new GroupChoiceViewModel(this.CleGeoportail, Profil.Group.Name, Profil);
                groupChoiceViewModel.groupChoiceView.DataContext = groupChoiceViewModel;
                bool? dialogResult = groupChoiceViewModel.groupChoiceView.ShowDialog();
                // Si l'utilisateur a cliqué sur le bouton "Annuler"
                // dans son choix du groupe, on sort
                if (dialogResult == false)
                {
                    return false;
                }
               
                // le choix du nouveau profil est validé
                // le nouvel id et nom du groupe, la clé Geoportail sont retournés dans un tuple
                (string, string, string) idNomGroupeCleGeoPortail = groupChoiceViewModel.Profile.IdNameGroupKeyGeoPortail;
                this.CleGeoportail = idNomGroupeCleGeoPortail.Item3;

                // si l'utilisateur n'appartient qu'à un seul groupe, le profil chargé reste actif
                if (groupChoiceViewModel.Profile.Geogroupes.Count == 1)
                {
                    this.Profil = groupChoiceViewModel.Profile;
                }
                else
                {
                    // récupère le profil et un message dans un tuple
                    (Profile, string) profilMessage = connexionServer.SetChangeUserProfil(idNomGroupeCleGeoPortail.Item1);
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

                // Sauvegarde de la clé Géoportail et du groupe actif
                // dans le xml du projet utilisateur
                Helper.SaveGeoportalKey(idNomGroupeCleGeoPortail.Item3);
                this.Groupeactif = idNomGroupeCleGeoPortail.Item2;
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
            // Récupération des layers GéoPortail valides en fonction
            // de la clé Geoportail utilisateur
            this.Profil.LayersKeyGeoportail = connexionServer.GetLayersFromCleGeoportailUser(this.CleGeoportail);
            return true;
        }

        /// <summary>
        /// Transforme en croquis les objets sélectionnés dans la carte en cours.
        /// </summary>
        /// <returns>Liste de croquis créés à partir des objects sélectionnés.</returns>
        public List<ArcGisProEspaceCollaboratif.Core.Sketch > MakeSketchFromSelection()
        {
            List<ArcGisProEspaceCollaboratif.Core.Sketch> sketches = new List<ArcGisProEspaceCollaboratif.Core.Sketch>();

            if (this.MapActiveView == null)
            {
                return sketches;
            }

            // Get the currently selected features in the map
            QueuedTask.Run(()=>
            {
                var selectedFeatures = this.MapActiveView.Map.GetSelection();
                foreach (KeyValuePair<MapMember, List<long>> kvp in selectedFeatures)
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
                            Geometry geometryFeature = inspector.Shape;
                            ArcGisProEspaceCollaboratif.Core.Sketch tmpSketch = Helper.MakeSketch(geometryFeature);

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
                       
            return sketches;
        }

 
        /// <summary>
        /// Donne le décompte de signalements Ripart présentes sur la carte en cours ayant le statut indiqué.
        /// </summary>
        /// <param name="status">Le statut des signalements Ripart qu'on veut dénombrer.</param>
        /// <returns>Le décompte de signalements Ripart sur la carte ayant le statut indiqué.</returns>
        public int CountReportsByStatus(int status)
        {
            FeatureLayer reportLayer = this.GetLayerByName(Helper.name_layer_Signalement);
            FeatureClass reportFeatureClass = reportLayer.GetFeatureClass();
            QueryFilter queryFilter = new QueryFilter
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
        public int CountReportsByStatus(ArcGisProEspaceCollaboratif.Core.Status.EnumStatus status)
        {
            return this.CountReportsByStatus((int)status);
        }


    }
}