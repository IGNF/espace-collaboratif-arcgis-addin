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
                0,
                "Tear pin 2"
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
        /// Essaie de charger une couche de la geodatabase
        /// </summary>
        /// <param name="layerName">nom de la couche</param> Change en layerPath
        /// <returns>bool true si la couche a pu être charchée, false sinon (la couche n'existe pas dans la gdb)</returns>
        /*        private FeatureLayer LoadLayer(string layerName, string symbolName = "")
                {

                    FeatureLayer result = null;

                    // Chemin complet de la couche
                    string layerPath = this.gdbPath + "\\" + layerName ;

                    try
                    {
                        int indexNumber = 0;
                        System.Uri layerUri = new System.Uri(layerPath);

                        // Création de la nouvelle couche (objet layer à partir d'une feature class existante)
                        FeatureLayer layer = LayerFactory.Instance.CreateFeatureLayer(
                            layerUri,
                            this.mapActiveView.Map,
                            indexNumber,
                            layerName
                        );

                        if (symbolName != "")
                            setReportLayerStyle(result, symbolName);

                        result = layer;
                    }
                    catch (Exception e)
                    {
                        logger.Info(layerName + " n'existe pas dans la gdb\n" + e.Message);
                        result = null;
                    }

                    return result;
                }
        */



        /// <summary>
        /// Récupère un calque par son nom.
        /// </summary>
        /// <param name="name">Le nom du calque qu'il faut récupérer.</param>
        /// <returns>Le calque ou null si non trouvé</returns>
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
                this.MapActiveView = MapView.Active;
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
        // Efface de la carte en cours le signalement (et ses croquis associés s'ils existent) donnée par son identifiant.
        /// </summary>
        /// <param name="idRemarque">Le numéro du signalement qu'on souhaite effacer de la carte en cours.</param>
        /*        public void EffacerPointRemarqueEspaceCollaboratif(uint idRemarque)
                {
                    int indexCalque = 0;

                    foreach (FeatureLayer calqueEspaceCollaboratif in this.calquesEspaceCollaboratif)
                    {
                        IQueryFilter queryFilter = new QueryFilter();

                        if (indexCalque == 0)
                        {
                            queryFilter.WhereClause = EspaceCollaboratifHelper.nom_Champ_IdRemarque + "=" + idRemarque;
                        }
                        else
                        {
                            queryFilter.WhereClause = EspaceCollaboratifHelper.nom_Champ_LienRemarque + "=" + idRemarque;
                        }

                        ITable table = (ITable)calqueEspaceCollaboratif;
                        table.DeleteSearchedRows(queryFilter);

                        indexCalque++;
                    }
                }
        */
 
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
                                    feature["Réponses"] = Helper.EncodeToUTF8(reportUdating.ConcatenateResponse());
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
        /// Dessine sur la carte en cours un signalement donné (avec ses éventuels croquis associés).
        /// </summary>
        /// <param name="newReport">Le signalement qu'il faut placer sur la carte en cours.</param>
        public async Task<bool> CreatingPointReport(ArcGisProEspaceCollaboratif.Core.Report newReport)
        {
            try
            {
                return await QueuedTask.Run(() =>
                {

                    FeatureLayer reportLayer = this.GetLayerByName(Helper.name_layer_Signalement);
                    FeatureClass reportFeatureClass = reportLayer.GetFeatureClass();

                    EditOperation editOperation = new EditOperation();
                    editOperation.Callback(context =>
                    {
                        RowBuffer rowBuffer = null;
                        Feature featureReport = null;

                        try
                        {
                            // Création de l'objet signalement dans la couche des signalements avec tous ses attributs
                            // On récupère le schéma de la classe
                            FeatureClassDefinition reportFcDefinition = reportFeatureClass.GetDefinition();

                            // Préparation des attributs de l'objet signalement à créer
                            rowBuffer = reportFeatureClass.CreateRowBuffer();

                            rowBuffer[Helper.name_field_IdReport] = newReport.Id;
                            rowBuffer[Helper.name_field_Auteur] = newReport.Author.Name;
                            rowBuffer[Helper.name_field_Insee] = newReport.Insee;
                            rowBuffer[Helper.name_field_Commune] = newReport.Commune;
                            rowBuffer[Helper.name_field_Departement] = newReport.Departement.Name;
                            rowBuffer[Helper.name_field_IDDepartement] = newReport.Departement.Id;
                            rowBuffer[Helper.name_field_DateCreation] = newReport.DateCreation;
                            rowBuffer[Helper.name_field_DateMAJ] = newReport.DateUpdate;
                            rowBuffer[Helper.name_field_DateValidation] = newReport.DateValidation;
                            rowBuffer[Helper.name_field_Statut] = newReport.Status;
                            rowBuffer[Helper.name_field_Themes] = Helper.Limite(newReport.ConcatenateThemes());
                            rowBuffer[Helper.name_field_Url] = newReport.Lien;
                            rowBuffer[Helper.name_field_UrlPrive] = newReport.LienPrive;
                            rowBuffer[Helper.name_field_Document] = Helper.Limite(newReport.ConcatenateDocuments());
                            rowBuffer[Helper.name_field_Message] = Helper.Limite(newReport.Commentary);
                            rowBuffer[Helper.name_field_Reponse] = Helper.Limite(newReport.ConcatenateResponse());
                            rowBuffer[Helper.name_field_Autorisation] = newReport.Authorisation;
                            rowBuffer[Helper.name_field_Source] = newReport.Source;

                            // Création de l'objet signalement dans la classe des signalements
                            featureReport = reportFeatureClass.CreateRow(rowBuffer);

                            // Remplissage de sa géométrie
                            featureReport.SetShape(Helper.TransformPoint(newReport.Position));

                            // Enregristrement
                            featureReport.Store();

                            //To Indicate that the attribute table has to be updated
                            context.Invalidate(featureReport);
                        }

                        catch (GeodatabaseException exObj)
                        {
                            logger.Error(string.Format("Context.CreerPointSignalement : {0}\n", exObj.Message));
                        }
                        finally
                        {
                            if (rowBuffer != null)
                                rowBuffer.Dispose();

                            if (featureReport != null)
                                featureReport.Dispose();
                        }
                    }, reportFeatureClass);
                    Helper.ExecuteEditOperation(editOperation);

                    if (newReport.Id == 482129)
                    {
                        int a = 1;
                    }
                    //Récupération des croquis associés au signalement   
                    if (!newReport.IsCroquisEmpty())
                    {
                        foreach (ArcGisProEspaceCollaboratif.Core.Sketch currSketch in newReport.Sketches)
                        {
                            if (currSketch.Points.Count == 0)
                            {
                                continue;
                            }
                           
                            // on caste le featureLayer en fonction du type du croquis pour utiliser la bonne couche associée
                            FeatureLayer sketchFeatureLayer = this.CollaborativeSpaceLayers[(int)currSketch.Type];
                            FeatureClass sketchFeatureClass = sketchFeatureLayer.GetFeatureClass();
                            
                            // Création de l'objet croquis dans la classe correspondant à son type
                            CreateSketchObject(currSketch, sketchFeatureClass, newReport.Id);
                           
                        }
                    }

                    // If the table is non-versioned this is a no-op. If it is versioned, we need the Save to be done for the edits to be persisted.
                    Project.Current.SaveEditsAsync();

                    return true;
                });
            }

            catch (Exception e)
            {
                string message = string.Format("{0}\n{1}", e.Message, e.ToString());
                logger.Error(string.Format("Context.CreerPointSignalement : {0}\n", message));
                return false;
            }
        }


        public void CreateSketchObject(ArcGisProEspaceCollaboratif.Core.Sketch currSketch, FeatureClass sketchFeatureClass, ulong idNewReport)
        {
            EditOperation editOperation = new EditOperation();
            editOperation.Callback(context =>
            {
                RowBuffer rowBuffer = null;
                Feature sketchFeature = null;

                try
                {
                    // Récupération des attributs du croquis transmis par l'API (champ attributes)
                    string attributes = "";
                    foreach (ArcGisProEspaceCollaboratif.Core.SketchAttributes attribut in currSketch.Attributes)
                        attributes += string.Format("{0} = '{1}' | ", attribut.Name, attribut.Value);

                    if (currSketch.Attributes.Count != 0)
                        attributes = attributes.Substring(0, attributes.Length - 3);

                    // Préparation des attributs de l'objet croquis à créer
                    rowBuffer = sketchFeatureClass.CreateRowBuffer();

                    rowBuffer[Helper.name_field_LienReport] = idNewReport;
                    rowBuffer[Helper.name_field_NomCroquis] = currSketch.Name;
                    rowBuffer[Helper.name_field_Attributs] = Helper.Limite(attributes);

                    // Création de l'objet signalement dans la classe des signalements
                    sketchFeature = sketchFeatureClass.CreateRow(rowBuffer);

                    // Remplissage de sa géométrie
                    ArcGIS.Core.Geometry.MapPoint sketchPoint = Helper.TransformPoint(currSketch.Points.First());
                    switch (currSketch.Type)
                    {
                        default:
                            break;

                        case SketchType.Point:
                            sketchFeature.SetShape(sketchPoint);
                            break;

                        case ArcGisProEspaceCollaboratif.Core.Sketch.SketchType.Ligne:
                            Polyline sketchLine = PolylineBuilder.CreatePolyline(Helper.GetPointCollectionFromSketch(currSketch));
                            sketchFeature.SetShape(sketchLine);
                            break;

                        case ArcGisProEspaceCollaboratif.Core.Sketch.SketchType.Polygone:
                            Polygon sketchPolygon = PolygonBuilder.CreatePolygon(Helper.GetPointCollectionFromSketch(currSketch));
                            sketchFeature.SetShape(sketchPolygon);
                            break;

                    }

                    // Enregistrement
                    sketchFeature.Store();

                    //To Indicate that the attribute table has to be updated
                    context.Invalidate(sketchFeature);
                }

                catch (GeodatabaseException exObj)
                {
                    logger.Error(string.Format("Context.CreateSketchObject : {0}\n", exObj.Message));
                }
                finally
                {
                    if (rowBuffer != null)
                        rowBuffer.Dispose();

                    if (sketchFeature != null)
                        sketchFeature.Dispose();
                }
            }, sketchFeatureClass);
            Helper.ExecuteEditOperation(editOperation);
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
        /// Calcule la BBox Ripart qui enveloppe un unique object géométrique.
        /// </summary>
        /// <param name="geometrieFiltre">La Geometry dont on veut obtenir l'enveloppe globale.</param>
        /// <returns>Ripart.Core.Box qui enveloppe la Geometry de <paramref name="geometrieFiltre"/>.</returns>
        /*        public ArcGisProEspaceCollaboratif.Core.Box GetBBox(Geometry geometrieFiltre)
                {
                    List<Geometry> tempGeometriesFiltres = new List<Geometry>
                    {
                        geometrieFiltre
                    };
                    return this.GetBBox(tempGeometriesFiltres);
                }
        */

        
        // INUTILE ?
        /// <summary>
        /// Zoom à l'écran sur une emprise donnée.
        /// </summary>
        /// <param name="emprise">L'object Ripart.Core.Box sur laquelle il faut faire le zoom à l'écran.</param>
/*
        public void Zoom(ArcGisProEspaceCollaboratif.Core.Box emprise)
        {
            Envelope bbox = new Envelope(emprise);
            bbox.SpatialReference = this.spatialReference;
            bbox.PutCoords(emprise.XMin, emprise.YMin, emprise.XMax, emprise.YMax);

            Camera extentCamera = new Camera()

            this.mapActiveView.ZoomTo = bbox;
            this.ActiveView.Refresh();
            return;
        }
*/
        /// <summary>
        /// Zoom à l'écran sur l'étendue de l'ensemble d'une liste d'objects Geometry.
        /// </summary>
        /// <param name="geometries">La liste des objects IGeometry sur lesquels il faut faire le zoom à l'écran.</param>
        /*       public void Zoom(List<Geometry> geometries)
               {
                   ArcGisProEspaceCollaboratif.Core.Box bbox = this.GetBBox(geometries);
                   this.Zoom(bbox);
                   return;
               }
       */
        /// <summary>
        /// Zoom à l'écran sur l'étendue un object Geometry.
        /// </summary>
        /// <param name="geometrie">L'object IGeometry sur lequel il faut faire le zoom à l'écran.</param>
        /*        public void Zoom(Geometry geometrie)
                {
                    ArcGisProEspaceCollaboratif.Core.Box bbox = this.GetBBox(geometrie);
                    this.Zoom(bbox);
                    return;
                }
        */
        /// <summary>
        /// Zoom à l'écran sur l'étendue de l'ensemble d'une liste de signalements Ripart.
        /// </summary>
        /// <param name="remarques">La liste des signalements Ripart sur lesquels il faut faire le zoom à l'écran.</param>
        /*       public void Zoom(List<ArcGisProEspaceCollaboratif.Core.Signalement> remarques)
               {
                   if (remarques.Count == 0) { return; }

                   List<double> coordX = new List<double>();
                   List<double> coordY = new List<double>();

                   foreach (ArcGisProEspaceCollaboratif.Core.Signalement remarque in remarques)
                   {
                       coordX.Add(remarque.Position.Longitude);
                       coordY.Add(remarque.Position.Latitude);
                   }

                   double supplementZoom = 5 / 100;
                   double supplementX = (coordX.Max() - coordX.Min()) * supplementZoom;
                   double supplementY = (coordY.Max() - coordY.Min()) * supplementZoom;

                   ArcGisProEspaceCollaboratif.Core.Box bbox = new ArcGisProEspaceCollaboratif.Core.Box(coordX.Min() - supplementX, coordY.Min() - supplementY, coordX.Max() + supplementX, coordY.Max() + supplementY);

                   this.Zoom(bbox);
                   return;
               }
       */



        // INUTILE ?
        /// <summary>
        /// Retourne la liste des géométries destinées à servir au filtrage spatial lors de l'importation des signalements.
        /// </summary>
        /// <returns>Liste d'Geometry contenant les géométries devant servir pour le filtrage spatial lors de l'importation des signalements.</returns>
/*        public List<Geometry> GetSpatialFilterGeometry()
        {
            List<Geometry> geometryFiltreSpatial = new List<Geometry>();

            // Récupération de la liste des géométries servant pour le filtrage spatial à partir des objects sélectionnés dans la carte en cours.
//TO-DO            geometryFiltreSpatial = this.GetSpatialFilterGeometry_from_selection();

            // Si la récupération par sélection est vide (car aucun object séléectionné ou aucun ayant la géométrie adéquate), alors on récupère les géométries contenues dans le calque définit par le fichier de paramètre.
            if (geometryFiltreSpatial.Count == 0)
            {
                geometryFiltreSpatial = this.GetSpatialFilterGeometry_from_XML();
            }

            // Si la récupération n'est pas vide, on zoom à l'écran sur celle-ci.
            if (geometryFiltreSpatial.Count != 0)
            {
                this.Zoom(geometryFiltreSpatial);
            }

            return geometryFiltreSpatial;
        }
*/

        /// <summary>
        /// Récupère à partir d'un calque donné par nom, la liste des géométries destinées à servir au filtrage spatial lors de l'importation des signalements .
        /// </summary>
        /// <param name="calqueFiltrage">Nom du calque devant contenir les objects utiles pour le filtrage spatial.</param>
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


        // INUTILE ?
        /// <summary>
        /// Récupère à partir du calque indiqué dans le fichier XML de configuration, la liste des géométries destinées à servir au filtrage spatial lors de l'import des signalements.
        /// </summary>
        /// <returns>Liste d'Geometry contenant les géométries devant servir pour le filtrage spatial lors de l'import des signalements.</returns>
/*        public List<Geometry> GetSpatialFilterGeometry_from_XML()
        {
            List<Geometry> geometryFiltreSpatial = new List<Geometry>();

            string nom_FichierParametre = Helper.XML_NameFile();

            XmlDocument doc = new XmlDocument();
            doc.Load(nom_FichierParametre);

            // XmlNodeList elemCalqueExtraction = doc.GetElementsByTagName("Zone_extraction");
            XmlNodeList elemCalqueExtraction = doc.GetElementsByTagName(Helper.XML_Suffixe(Helper.xml_Zone_extraction));
            IEnumerator<XmlNode> ienum;

            // Parcour des calques contenant les objects de filtrage spatial d'après de le XML de paramétrage
            for (int i = 0; i < elemCalqueExtraction.Count; i++)
            {
                string nomCalqueExtraction = elemCalqueExtraction[i].Attributes["calque"].Value;

                if (nomCalqueExtraction.Length == 0)
                { continue; }

                Layer calqueExtraction = this.GetLayerByName(nomCalqueExtraction);
                if (calqueExtraction == null)
                { continue; }

                ienum = elemCalqueExtraction[i].GetEnumerator() as IEnumerator<XmlNode>;

                // Parcour objects de filtrage spatial au sein du même calque
                while (ienum.MoveNext())
                {
                    XmlNode noeud = (XmlNode)ienum.Current;

                    string idObjectExtraction = noeud.Attributes["ID"].Value;
                    string valObjectExtraction = noeud.InnerText;

                    FeatureLayer featureLayerFiltrageSpatial = calqueExtraction as FeatureLayer;
                    FeatureClass featureClassFiltrageSpatial = featureLayerFiltrageSpatial.GetFeatureClass();

                    QueryFilter filtreSpatial = new QueryFilter
                    {

                        // Recherche de l'object filtrant spatial d'après le nom et la valeur de son identifiant
                        WhereClause = idObjectExtraction + "=" + valObjectExtraction
                    };

                    RowCursor rowCursor = featureClassFiltrageSpatial.Search(
                        filtreSpatial,
                        false // important : sinon, on a un seul objet
                    );

                    while (rowCursor.MoveNext())
                    {
                        Feature featureFiltrageSpatial = rowCursor.Current as Feature;
                        Geometry contourFiltrageSpatial = GeometryEngine.Instance.Project(featureFiltrageSpatial.GetShape(), this.spatialReference);
                        geometryFiltreSpatial.Add(contourFiltrageSpatial);

                    }
                }
                ienum.Reset();
            }
            return geometryFiltreSpatial;
        }
*/

        // INUTILE ?
        /// <summary>
        /// Récupère à partir des objects sélectionnés dans la carte en cours, la liste des géométries destinées à servir au filtrage spatial lors de l'importation des signalements .
        /// </summary>
        /// <returns>Liste Geometry contenant les géométries devant servir pour le filtrage spatial lors de l'importation des signalements.</returns>

/*        TO-DO
 *        public List<Geometry> GetGeometryFiltreSpatial_from_selection()
        {
            List<Geometry> geometryFiltreSpatial = new List<Geometry>();

            // Récupération des objects sélectionnés
            IEnumerator<Feature> enumFeature = this.mapActiveView..FeatureSelection as IEnumFeature;
            Feature feature = enumFeature.Next();

            while (feature != null)
            {
                if (Helper.TestGeometrieFiltrageSpatial(feature))
                {
                    Geometry contourFiltrageSpatial = GeometryEngine.Instance.Project(feature.GetShape(), this.spatialReference);
                    geometryFiltreSpatial.Add(contourFiltrageSpatial);
                }

                feature = enumFeature.Next();
            }

            return geometryFiltreSpatial;
        }
*/        


        /// <summary>
        /// Établit la connexion avec le service Ripart.
        /// </summary>
        /*public ArcGisProEspaceCollaboratif.Core.Client GetConnexionEspaceCollaboratif()
        {
            ILog.Debug("GetConnexionEspaceCollaboratif ");
            this.CleGeoportail = Helper.Load_CleGeoportail();
            this.URLHost = Helper.Load_Urlhost();
            ILog.Debug("URLHost : " + this.URLHost);

            var connectViewModel = new ConnectViewModel(this.URLHost);
            connectViewModel.connectView.DataContext = connectViewModel;

            // Recherche du login par défaut dans le fichier XML de paramétrage
            this.Login = Helper.Load_Login();
            this.Password = "";
            bool firstConnection = false;
            for (int tentativeConnexion = 0; tentativeConnexion < 3; tentativeConnexion++)          
            {
                ILog.Debug("Tentative de connexion ");
                // S'il n'y a pas de login enregistré, on lance le formulaire de connexion
                if (this.Login.Length == 0 || this.Password.Length == 0)
                {
                    // Lancement du formulaire de saisi du login et mot de passe                
                    connectViewModel.Login = this.Login;

                    // Si l'utilisateur a cliqué sur le bouton "Connecter"
                    // il faut récupérer le login et password pour établir
                    // la connexion au service de l'Espace collaboratif
                    Nullable<bool> dialogResult = connectViewModel.connectView.ShowDialog();
                    if(dialogResult == false)
                    {
                        return null;
                    }
                    // Récupération du login et mot de passe introduits.
                    this.Login = connectViewModel.Login;
                    this.Password = connectViewModel.Password;
                }
                else
                {
                    firstConnection = true;
                }

                try
                {
                    if (!firstConnection)
                    {
                        continue;
                    }

                    // Création de la connexion au serveur.
                    ArcGisProEspaceCollaboratif.Core.Client connexionServer = new Client(
                        this.URLHost,
                        this.Login,
                        this.Password
                    );
                    ILog.Info("Création de la connexion au serveur " + connexionServer.ToString());
                    //connectViewModel.connectView.Close();

                    // Récupération du profil utilisateur
                    this.Profil = connexionServer.GetProfil();

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
                    connectViewModel.connectView.Visibility = System.Windows.Visibility.Visible;

                    switch (erreurConnexion.Message.ToString())
                    {
                        case "(401) Unauthorized":
                            connectViewModel.Error = "Login et/ou mot de passe incorrect(s)";
                            
                            break;

                        case "Login inconnu":
                            connectViewModel.Error = string.Format("''{0}'' n'est pas un utilisateur enregistré dans un groupe de l'Espace collaboratif.", this.Login);
                            break;

                        case "no_group":
                            connectViewModel.Error = "Accès refusé. L'utilisateur n'appartient à aucun groupe.";
                            break;

                        default:
                            MessageBox.Show("Impossible d'accéder au service de l'Espace collaboratif à l'adresse suivante: " + this.URLHost +
                                            "\n\nVeuillez contacter le support de l'Espace collaboratif: \n" + erreurConnexion.Message.ToString() + ".", "IGN Espace collaboratif",
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                    }
                }
            }
            connectViewModel.connectView.Close();
            return null;
        }*/

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
        /// Renvoie la date de mise-à-jour la plus récente contenue dans les signalements présents sur la carte.
        /// </summary>
        /// <returns>La date de mise-à-jour la plus récente contenue dans les signalements présents sur la carte.</returns>
/*        public System.DateTime Get_LastUpdate()
        {
            FeatureLayer calqueEspaceCollaboratif = this.calquesEspaceCollaboratif.First();
            FeatureClass featureClass = calqueEspaceCollaboratif.GetFeatureClass();
            FeatureClassDefinition featureClassDefinition = featureClass.GetDefinition();
            int index = featureClassDefinition.FindField(EspaceCollaboratifHelper.nom_Champ_DateMAJ);
            QueryFilter queryFilter = new QueryFilter();
            List<System.DateTime> listDate = new List<DateTime>();
            using (RowCursor rowCursor = featureClass.Search(queryFilter, false))
            {
                while (rowCursor.MoveNext())
                {
                    /*using (Row row = rowCursor.Current)
                    {
                        string location = Convert.ToString(row[EspaceCollaboratifHelper.nom_Champ_DateMAJ]);
                        listDate.Add(DateTime.Parse(location));
                    }*/
      /*              using (Feature feature = (Feature)rowCursor.Current)
                    {
                        listDate.Add(DateTime.Parse(feature.GetOriginalValue(index).ToString()));
                    }
                }
            }
            return listDate.Max();
        }
*/
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


        /// <summary>
        /// Met dans la sélection courante, les signalements Ripart présents sur la carte et ayants un des statuts indiqués. 
        /// </summary>
        /// <param name="statut">La liste des statuts des signalements Ripart qu'on veut mettre en sélection.</param>
        /// <param name="zoom_to_selected_Remarque">Option pour zoomer sur la nouvelle sélection.</param> 
/*        public void Select_Remarque_by_Statut(List<int> statut, bool zoom_to_selected_Remarque)
        {
            //if (statut.Count == 0) { return; }

            this.Map.ClearSelection(); // Vide la sélection en cours

            FeatureLayer calqueEspaceCollaboratif = this.calquesEspaceCollaboratif.First();
            IFeatureClass featureClass = calqueEspaceCollaboratif.FeatureClass;

            int champStatut = featureClass.FindField(EspaceCollaboratifHelper.nom_Champ_Statut);
            IQueryFilter queryFilter = new QueryFilter();

            if (statut.Count == 0)
            {
                IFeatureCursor pFeatureCursor = featureClass.Search(queryFilter, false);
                IFeature pFeature = pFeatureCursor.NextFeature();
                IFeatureSelection remarqueSelect = calqueEspaceCollaboratif as IFeatureSelection;   // Sélection des signalements     

                remarqueSelect.SelectFeatures(queryFilter, esriSelectionResultEnum.esriSelectionResultNew, false);
            }
            else
            {
                foreach (int statutTemp in statut)
                {
                    queryFilter.WhereClause = EspaceCollaboratifHelper.nom_Champ_Statut + " = " + statutTemp; // Requête pour trouver les signalements au statut voulu.

                    IFeatureCursor pFeatureCursor = featureClass.Search(queryFilter, false);
                    IFeature pFeature = pFeatureCursor.NextFeature();
                    IFeatureSelection remarqueSelect = calqueEspaceCollaboratif as IFeatureSelection;   // Sélection des signalements     

                    remarqueSelect.SelectFeatures(queryFilter, esriSelectionResultEnum.esriSelectionResultAdd, false);
                }
            }

            List<double> coordX = new List<double>();
            List<double> coordY = new List<double>();

            // Obtention des objects sélectionnés
            IEnumFeature enumFeature = this.Map.FeatureSelection as IEnumFeature;
            Feature feature = enumFeature.Next();

            while (feature != null)
            {
                Geometry geometry = feature.GetShape();
                ArcGIS.Core.Geometry.MapPoint point = geometry as ArcGIS.Core.Geometry.MapPoint;
                point.Project(this.spatialReferenceEspaceCollaboratif);

                coordX.Add(point.X);
                coordY.Add(point.Y);

                feature = enumFeature.Next();
            }

            // Option pour zoomer sur les signalements sélectionnés
            if (zoom_to_selected_Remarque && coordX.Count != 0)
            {
                ArcGisProEspaceCollaboratif.Core.Box emprise = new ArcGisProEspaceCollaboratif.Core.Box(coordX.Min(), coordY.Min(), coordX.Max(), coordY.Max());
                this.Zoom(emprise);
            }

            this.ActiveView.Refresh();

            EspaceCollaboratifHelper.MessageBar(" " + coordX.Count + " signalement(s) sélectionnée(s).");
        }
*/

        /// <summary>
        /// Met dans la sélection courante, les signalements Ripart présentes sur la carte et ayants un des statuts indiqués.
        /// </summary>
        /// <param name="statut">La liste des statuts des signalements Ripart qu'on veut mettre en sélection.</param>       
/*        public void Select_Remarque_by_Statut(List<int> statut)
        {
            this.Select_Remarque_by_Statut(statut, false);
        }
        /// <summary>
        /// Met dans la sélection courante, les signalements Ripart présents sur la carte et ayants un des statuts indiqués. 
        /// </summary>
        /// <param name="statut">La liste des statuts des signalements Ripart qu'on veut mettre en sélection.</param>
        /// <param name="zoom_to_selected_Remarque">Option pour zoomer sur la nouvelle sélection.</param> 
        public void Select_Remarque_by_Statut(List<ArcGisProEspaceCollaboratif.Core.Statut> statut, bool zoom_to_selected_Remarque)
        {
            List<int> statutInt = new List<int>();

            for (int i = 0; i < statut.Count; i++)
            {
                statutInt.Add((int)statut[i]);
            }

            this.Select_Remarque_by_Statut(statutInt, zoom_to_selected_Remarque);
        }
*/

        /// <summary>
        /// Met dans la sélection courante, les signalements Ripart présents sur la carte et ayants un des statuts indiqués. 
        /// </summary>
        /// <param name="statut">La liste des statuts des signalements Ripart qu'on veut mettre en sélection.</param>     
/*        public void Select_Remarque_by_Statut(List<ArcGisProEspaceCollaboratif.Core.Statut> statut)
        {
            this.Select_Remarque_by_Statut(statut);
        }
*/
        /// <summary>
        /// Met dans la sélection courante, les signalements Ripart présents sur la carte et ayants le statut indiqué. 
        /// </summary>
        /// <param name="statut">Le statut des signalements Ripart qu'on veut mettre en sélection.</param>
        /// <param name="zoom_to_selected_Remarque">Option pour zoomer sur la nouvelle sélection.</param> 
/*        public void Select_Remarque_by_Statut(int statut, bool zoom_to_selected_Remarque)
        {
            List<int> statutList = new List<int>
            {
                statut
            };

            this.Select_Remarque_by_Statut(statutList, zoom_to_selected_Remarque);
        }
*/

        /// <summary>
        /// Met dans la sélection courante, les signalements Ripart présents sur la carte et ayants le statut indiqué. 
        /// </summary>
        /// <param name="statut">Le statut des signalements Ripart qu'on veut mettre en sélection.</param>        
/*        public void Select_Remarque_by_Statut(int statut)
        {
            this.Select_Remarque_by_Statut(statut, false);
        }
*/
        /// <summary>
        /// Met dans la sélection courante, les signalements Ripart présents sur la carte et ayants le statut indiqué. 
        /// </summary>
        /// <param name="statut">Le statut des signalements Ripart qu'on veut mettre en sélection.</param>
        /// <param name="zoom_to_selected_Remarque">Option pour zoomer sur la nouvelle sélection.</param> 
/*        public void Select_Remarque_by_Statut(ArcGisProEspaceCollaboratif.Core.Statut statut, bool zoom_to_selected_Remarque)
        {
            this.Select_Remarque_by_Statut((int)statut, zoom_to_selected_Remarque);
        }
*/
        /// <summary>
        /// Met dans la sélection courante, les signalements Ripart présents sur la carte et ayants le statut indiqué. 
        /// </summary>
        /// <param name="statut">Le statut des signalements Ripart qu'on veut mettre en sélection.</param>       
/*        public void Select_Remarque_by_Statut(ArcGisProEspaceCollaboratif.Core.Statut statut)
        {
            this.Select_Remarque_by_Statut((int)statut, false);
        }
*/
    }
}