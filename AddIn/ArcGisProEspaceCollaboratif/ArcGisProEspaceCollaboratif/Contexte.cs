using System;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using log4net;
using ArcGisProEspaceCollaboratif.Core;
using System.Threading;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Editing;
using static ArcGisProEspaceCollaboratif.Core.Sketch;
using System.Xml;
using ArcGisProEspaceCollaboratif.ViewModels;
using System.Security;

namespace ArcGisProEspaceCollaboratif
{
    public sealed class Contexte
    {
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
        public string GeoDatabasePath { get; set; } = CoreModule.CurrentProject.DefaultGeodatabasePath;

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
        /// 
        /// </summary>
        public Logger Logger { get; set; } = Logger.Instance;

        /// <summary>
        /// 
        /// </summary>
        static ILog ILog { get; set; } = LogManager.GetLogger(typeof(Contexte));

        /// <summary>
        /// 
        /// </summary>
        public Profil Profil { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Client Client { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private static Contexte _instance = null;
        public static Contexte Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Padlock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Contexte();
                            ILog.Debug("Instance de contexte créée");
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Constructeur pour un contexte à partir de la carte courante
        /// </summary>
        private Contexte()
        {
            this.Init(MapView.Active);
        }

        /// <summary>
        /// Constructeur à partir d'une vue active
        /// </summary>
        /// <param name="activeView">L'activeView associée à la carte en cours.</param>
        //private Contexte(IActiveView activeView)
        private Contexte(MapView activeView)
        {
            this.Init(activeView);
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
                throw new Exception(@"Votre projet doit être enregistré avant de pouvoir utiliser l'add-in Espace collaboratif");
            }

            this.DirectoryWorking = System.IO.Path.GetDirectoryName(project.Path);
            this.FileMapWorking = System.IO.Path.GetFileNameWithoutExtension(project.Name);

            this.CheckConfigFile();

            //création ou chargement des couches ripart
            //TODO : question Noémie pourquoi cette création ici ?
            //var bLayersLoaded = CreateOrLoadReportLayers();

            ILog.Debug("Initialisation du contexte et des éléments de l'Espace collaboratif");
        }

        /// <summary>
        /// Teste si le fichier de configuration espaceco.xml n'existe pas dans le répertoire de travail, on le copie 
        /// du répertoire d'installation 
        /// </summary>
        /// <returns>true si le fichier de configuration espaceco.xml est à côté de la carte en cours.</returns>
        public bool CheckConfigFile()
        {
            string fileConfiguration = this.DirectoryWorking + "\\" + Helper.nom_Fichier_Parametres_EspaceCollaboratif;
            if (!File.Exists(fileConfiguration))
            {
                try
                {
                    File.Copy(Helper.EspaceCollaboratifAssemblyDir + Helper.nom_Fichier_Parametres_EspaceCollaboratif, fileConfiguration);
                }
                catch (Exception e)
                {
                    ILog.Error(e.Message + "\n" + e.StackTrace);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// création ou chargement des couches de signalement de l'espace collaboratif
        /// </summary>
        public async Task CreateOrLoadReportLayers()
        {
            // Création ou chargement des calques dédiés à de l'espace collaboratif s'ils sont absents de la carte en cours.
            string polygonSketchLayer = Helper.nom_Calque_Croquis_Polygone;
            string lineSketchLayer = Helper.nom_Calque_Croquis_Ligne;
            string pointSketchLayer = Helper.nom_Calque_Croquis_Point;
            string reportLayer = Helper.nom_Calque_Signalement;

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

            // Croquis linéaires
            await Helper.LoadOrCreateCollaborativeSpaceLayer(
                polygonSketchLayer,
                "POLYGON",
                Helper.sketchAttributes,
                3
                );

            // Ajout des couches à la liste collaboratifSpaceLayers
            this.CollaborativeSpaceLayers.Clear();
            this.CollaborativeSpaceLayers.Add(GetLayerByName(reportLayer) as FeatureLayer);
            this.CollaborativeSpaceLayers.Add(GetLayerByName(pointSketchLayer) as FeatureLayer);
            this.CollaborativeSpaceLayers.Add(GetLayerByName(lineSketchLayer) as FeatureLayer);
            this.CollaborativeSpaceLayers.Add(GetLayerByName(polygonSketchLayer) as FeatureLayer);

        }

        /// <summary>
        /// Essaie de charger une couche de la geodatabase
        /// </summary>
        /// <param name="layerName">nom de la couche</param> Change en layerPath
        /// <returns>bool true si la couche a pu être charchée, false sinon (la couche n'existe pas dans la gdb)</returns>
/*        private FeatureLayer LoadLayer(String layerName, string symbolName = "")
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
            IReadOnlyList<Layer> mapLayers = this.MapActiveView.Map.GetLayersAsFlattenedList();
            foreach (var layer in mapLayers)
            {
                if (layer.Name == layerName) return true;
            }
            return false;
        }

        //
        // INUTILE si on travaille dans la gdb créée automatiquement avec le projet ArcGIS Pro
        // A CONFIRMER ET SUPPRIMER

        /// <summary>
        /// Ouvre les fichiers géodatabase EspaceCollaboratif.gdb contenant les données de l'espace collaboratif dans la carte en cours.
        /// Si ces fichiers n'existent pas, ils sont préalablement créés dans le même répertoire où se situe la carte en cours.
        /// </summary>
        /// <returns>L'IFeatureWorkspace de l'espace de travail des calques dédiés à l'espace collaboratif.</returns>
/*       Remplace : private Geodatabase GetOrCreateFeatureWorkspace()
         A priori inutile avec ArcGIS Pro car une geodatabase est automatiquement créée et associée au projet ArcGIS Pro -> on l'utilise pour
         stocker les signalements (et à terme les couches guichets).*/
        private async Task<bool> GetOrCreateFileGeodatabase()
        {
            try
            {
                return await QueuedTask.Run(() =>
                {

                    var fGdbPath = this.DirectoryWorking;
                    var fGdbName = string.Format("{0}_EspaceCollaboratif.gdb", this.FileMapWorking);
                    var fGdb = string.Format("{0}\\{1}", fGdbPath, fGdbName);

                    // Si la gdb n'existe pas, on la crée
                    if (!System.IO.Directory.Exists(fGdb))
                    {
                        var fGdbVersion = "Current";  // create the 'latest' version of file Geodatabase
                        System.Diagnostics.Debug.WriteLine($@"create {fGdbPath} {fGdbName}");

                        var parameters = Geoprocessing.MakeValueArray(fGdbPath, fGdbName, fGdbVersion);
                        var cts = new CancellationTokenSource();
                        var results = Geoprocessing.ExecuteToolAsync("management.CreateFileGDB", parameters, null, cts.Token,
                            (eventName, o) =>
                            {
                                System.Diagnostics.Debug.WriteLine($@"GP event: {eventName}");
                            });
                    }

                    // Sinon, on ouvre la gdb existante
                    Geodatabase geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(fGdb)));
                    return true;     
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }



        /// <summary>
        // Vide les calques de l'espace collaboratif de tous leurs contenus.
        /// </summary>
        public void EmptyCollaborativeSpaceLayers()
        {
            try
            {
                foreach (FeatureLayer layer in this.CollaborativeSpaceLayers)
                {
                    FeatureClass fcCollabSpace = layer.GetFeatureClass();
                    var result = Geoprocessing.ExecuteToolAsync("TruncateTable_management", Geoprocessing.MakeValueArray(fcCollabSpace));
                }
            }
            catch (Exception e)
            {
                ILog.Error(e.Message + "\n" + e.StackTrace);
            }

        }

        /// <summary>
        // Efface de la carte en cours la remarque (et ses croquis associés s'ils existent) donnée par son identifiant.
        /// </summary>
        /// <param name="idRemarque">Le numéro de la remarque qu'on souhaite effacer de la carte en cours.</param>
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
        /// Dessine sur la carte en cours un signalement donné (avec ses éventuels croquis associés).
        /// </summary>
        /// <param name="newReport">Le signalement qu'il faut placer sur la carte en cours.</param>
        public async Task<bool> CreerPointSignalement(ArcGisProEspaceCollaboratif.Core.Signalement newReport)
        {
            try
            {
                return await QueuedTask.Run(() =>
                {

                    FeatureLayer reportLayer = this.GetLayerByName(Helper.nom_Calque_Signalement);
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

                            rowBuffer[Helper.nom_Champ_IdRemarque] = newReport.Id;
                            rowBuffer[Helper.nom_Champ_Auteur] = newReport.Auteur.Nom;
                            rowBuffer[Helper.nom_Champ_Commune] = newReport.Commune;
                            rowBuffer[Helper.nom_Champ_Departement] = newReport.Departement.Nom;
                            rowBuffer[Helper.nom_Champ_IDDepartement] = newReport.Departement.Id;
                            rowBuffer[Helper.nom_Champ_DateCreation] = newReport.DateCreation;
                            rowBuffer[Helper.nom_Champ_DateMAJ] = newReport.DateMiseAJour;
                            rowBuffer[Helper.nom_Champ_DateValidation] = newReport.DateValidation;
                            rowBuffer[Helper.nom_Champ_Statut] = newReport.Statut;
                            rowBuffer[Helper.nom_Champ_Themes] = newReport.ConcatenateThemes();
                            rowBuffer[Helper.nom_Champ_Url] = newReport.Lien;
                            rowBuffer[Helper.nom_Champ_UrlPrive] = newReport.LienPrive;
                            rowBuffer[Helper.nom_Champ_Document] = newReport.GetFirstDocument();
                            rowBuffer[Helper.nom_Champ_Message] = Helper.Limite(newReport.Commentaire);
                            rowBuffer[Helper.nom_Champ_Reponse] = Helper.Limite(newReport.ConcatenateReponse());
                            rowBuffer[Helper.nom_Champ_Autorisation] = newReport.Autorisation;

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
                            Console.WriteLine(exObj);
                        }
                        finally
                        {
                            if (rowBuffer != null)
                                rowBuffer.Dispose();

                            if (featureReport != null)
                                featureReport.Dispose();
                        }
                    }, reportFeatureClass);

                    bool editResult = editOperation.Execute();


                    //Récupération des croquis associés au signalement

                    //  Traitement du ou des croquis associé(s) au signalement     
                    if (!newReport.IsCroquisEmpty())
                    {
                        foreach (ArcGisProEspaceCollaboratif.Core.Sketch currSketch in newReport.Sketch)
                        {
                            if (currSketch.Points.Count == 0)
                            {
                                //   this.debugForm.WriteLine("Croquis sans coordonnées dans la remarque n°" + uneRemarque.Id);
                                continue;
                            }
                            else
                            {
                                // on cast le featureLayer en fonction du type du croquis pour utiliser la bonne couche associée
                                FeatureLayer sketchFeatureLayer = this.CollaborativeSpaceLayers[(int)currSketch.Type] as FeatureLayer;
                                FeatureClass sketchFeatureClass = sketchFeatureLayer.GetFeatureClass();

                                // Création de l'objet croquis dans la classe correpondant à son type
                                CreateSketchObject(currSketch, sketchFeatureClass, newReport.Id);
                            }
                        }
                    }

                    // If the table is non-versioned this is a no-op. If it is versioned, we need the Save to be done for the edits to be persisted.
                    Project.Current.SaveEditsAsync();

                    return true;
                });
            }

            catch (Exception e)
            {
                ILog.Error(e.Message + "\n" + e.ToString());
                return false;
            }
        }


        public bool CreateSketchObject(ArcGisProEspaceCollaboratif.Core.Sketch currSketch, FeatureClass sketchFeatureClass, ulong idNewReport)
        {
            EditOperation editOperation = new EditOperation();
            editOperation.Callback(context =>
            {
                RowBuffer rowBuffer = null;
                Feature sketchFeature = null;

                try
                {
                    // Récupération des attributs du croquis transmis par l'API (champ attributes)
                    String attributes = "";
                    foreach (ArcGisProEspaceCollaboratif.Core.SketchAttributes attribut in currSketch.Attributes)
                        attributes += attribut.Nom + " = '" + attribut.Valeur + "' | ";

                    if (currSketch.Attributes.Count != 0)
                        attributes = attributes.Substring(0, attributes.Length - 3);

                    // Préparation des attributs de l'objet croquis à créer
                    rowBuffer = sketchFeatureClass.CreateRowBuffer();

                    rowBuffer[Helper.nom_Champ_LienRemarque] = idNewReport;
                    rowBuffer[Helper.nom_Champ_NomCroquis] = currSketch.Name;
                    rowBuffer[Helper.nom_Champ_Attributs] = Helper.Limite(attributes);

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

                    // Enregristrement
                    sketchFeature.Store();

                    //To Indicate that the attribute table has to be updated
                    context.Invalidate(sketchFeature);
                }

                catch (GeodatabaseException exObj)
                {
                    Console.WriteLine(exObj);
                }
                finally
                {
                    if (rowBuffer != null)
                        rowBuffer.Dispose();

                    if (sketchFeature != null)
                        sketchFeature.Dispose();
                }
            }, sketchFeatureClass);

            bool editResult = editOperation.Execute();
            return editResult;
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
        /// Zoom à l'écran sur l'étendue de l'ensemble d'une liste de remarques Ripart.
        /// </summary>
        /// <param name="remarques">La liste des remarques Ripart sur lesquelles il faut faire le zoom à l'écran.</param>
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
        /// Retourne la liste des géométries destinées à servir au filtrage spatial lors de l'importation des remarques.
        /// </summary>
        /// <returns>Liste d'Geometry contenant les géométries devant servir pour le filtrage spatial lors de l'importation des remarques.</returns>
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
        /// Récupère à partir d'un calque donné par nom, la liste des géométries destinées à servir au filtrage spatial lors de l'importation des remarques .
        /// </summary>
        /// <param name="calqueFiltrage">Nom du calque devant contenir les objects utiles pour le filtrage spatial.</param>
        /// <returns>Liste d'Geometry contenant les géométries devant servir pour le filtrage spatial lors de l'importation des remarques.</returns>
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
        /// Récupère à partir des objects sélectionnés dans la carte en cours, la liste des géométries destinées à servir au filtrage spatial lors de l'importation des remarques .
        /// </summary>
        /// <returns>Liste Geometry contenant les géométries devant servir pour le filtrage spatial lors de l'importation des remarques.</returns>

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
            ILog.Debug("GetConnexionEspaceCollaboratif ");
            this.CleGeoportail = Helper.Load_CleGeoportail();
            this.URLHost = Helper.Load_Urlhost();
            ILog.Debug("URLHost : " + this.URLHost);

            var connectViewModel = new ConnectViewModel(this.URLHost);
            connectViewModel.connectView.DataContext = connectViewModel;

            // Recherche du login par défaut dans le fichier XML de paramétrage
            connectViewModel.Login = Helper.Load_Login();

            // Lancement du formulaire de saisi du login et mot de passe 
            bool? dialogResult = connectViewModel.connectView.ShowDialog();
            // Si l'utilisateur a cliqué sur le bouton "Annuler"
            // il n'y aura pas de connexion
            if (dialogResult == false)
            {
                return null;
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
                
                ILog.Info("Création de la connexion au serveur " + connexionServer.ToString());
                
                // Récupération du profil utilisateur
                this.Profil = connexionServer.GetProfil();
                if (this.Profil == null)
                {
                    throw new Exception("Connexion impossible au serveur de l'Espace collaboratif");
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
                        //connectViewModel.Error = "Login et/ou mot de passe incorrect(s)";
                        string message = "Login et/ou mot de passe incorrect(s)";
                        MessageBox.Show(message, "IGN Espace collaboratif", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;

                    case "Login inconnu":
                        //connectViewModel.Error = string.Format("''{0}'' n'est pas un utilisateur enregistré dans un groupe de l'Espace collaboratif.", this.Login);
                        message = string.Format("''{0}'' n'est pas un utilisateur enregistré dans un groupe de l'Espace collaboratif.", this.Login);
                        MessageBox.Show(message, "IGN Espace collaboratif", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;

                    case "no_group":
                        //connectViewModel.Error = "Accès refusé. L'utilisateur n'appartient à aucun groupe.";
                        message = "Accès refusé. L'utilisateur n'appartient à aucun groupe.";
                        MessageBox.Show(message, "IGN Espace collaboratif", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;

                    default:
                        message = string.Format("Impossible d'accéder au service de l'Espace collaboratif à l'adresse suivante : {0}\n\nVeuillez contacter le support. Erreur : {1}\n", this.URLHost, erreurConnexion.Message.ToString());
                        MessageBox.Show(message, "IGN Espace collaboratif", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            var connectInfoViewModel = new ConnectFeedbackInformationViewModel();
            connectInfoViewModel.connectFeedbackInformationView.DataContext = connectInfoViewModel;

            // Le logo du groupe auquel l'utilisateur appartient
            if (!string.IsNullOrEmpty(Profil.Logo))
            {
                connectInfoViewModel.Logo = this.URLHost + Profil.Logo;
            }
            string message = "Connexion réussie à l'Espace collaboratif\n\n";
            message += string.Format(" Serveur : {0}\n", this.URLHost);
            message += string.Format(" Login : {0}\n", this.Login);
            message += string.Format(" Groupe : {0}\n", Profil.Titre);
            if (Profil.Zone == ZoneGeographique.UNDEFINED)
            {
                string zoneExtraction = Helper.Load_FilterLayer();
                if (zoneExtraction == "" || zoneExtraction.Length == 0)
                {
                    message += " Zone : pas de zone définie\n";
                }
                else
                {
                    message += string.Format(" Zone : {0}", zoneExtraction);
                }
            }
            else
            {
                message += string.Format(" Zone : {0}", Profil.Zone);
            }
            message += string.Format(" Clé Géoportail : {0}", this.CleGeoportail);
            connectInfoViewModel.MessageFeedback = message;
            connectInfoViewModel.connectFeedbackInformationView.ShowDialog();

            Helper.Save_Login(this.Login);
            Helper.Save_GroupeActif(Profil.Titre);
            Helper.Save_CleGeoportail(this.CleGeoportail);
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
                if (this.Profil.Titre == "défaut")
                {
                    Helper.Save_GroupeActif("Aucun");
                }
                else
                {
                    Helper.Save_GroupeActif(this.Profil.Groupe.Nom);

                    // On enregistre le groupe comme groupe préféré (par défaut) pour la création de signalement
                    // Si ce n'est pas le même qu'avant, on vide les thèmes préférés
                    string preferredGroup = Helper.Load_PreferredGroup();
                    if (preferredGroup != Profil.Groupe.Nom)
                    {
                        Helper.Save_PreferredThemes(new List<string>());
                    }
                    Helper.Save_PreferredGroup(Profil.Groupe.Nom);
                }
                // Par défaut, on enregistre la clé Géoportail de démonstration
                Helper.Save_CleGeoportail(Constantes.DEMO);
            }
            else
            {
                // sinon le choix d'un autre groupe est présenté à l'utilisateur
                // le formulaire est proposé même si l'utilisateur n'appartient qu'à un groupe
                // afin qu'il puisse remplir sa clé Géoportail
                var groupChoiceViewModel = new GroupChoiceViewModel(this.CleGeoportail, Profil.Groupe.Nom, Profil);
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
                (string, string, string) idNomGroupeCleGeoPortail = groupChoiceViewModel._profile.IdNomGroupeCleGeoPortail;
                this.CleGeoportail = idNomGroupeCleGeoPortail.Item3;

                // si l'utilisateur n'appartient qu'à un seul groupe, le profil chargé reste actif
                if (groupChoiceViewModel._profile.Geogroupes.Count == 1)
                {
                    this.Profil = groupChoiceViewModel._profile;
                }
                else
                {
                    // récupère le profil et un message dans un tuple
                    (Profil, string) profilMessage = connexionServer.SetChangeUserProfil(idNomGroupeCleGeoPortail.Item1);
                    string messTmp = profilMessage.Item2;

                    // SetChangeUserProfil retourne un message "Le profil pour le groupe xxx est déjà actif"
                    if (messTmp.Contains("actif"))
                    {
                        // le profil chargé reste actif
                        this.Profil = groupChoiceViewModel._profile;
                    }
                    else
                    {
                        // setChangeUserProfil retourne un message vide le nouveau profil devient actif
                        this.Profil = profilMessage.Item1;
                    }
                }

                // Sauvegarde de la clé Géoportail et du groupe actif
                // dans le xml du projet utilisateur
                Helper.Save_CleGeoportail(idNomGroupeCleGeoPortail.Item3);
                this.Groupeactif = idNomGroupeCleGeoPortail.Item2;
                Helper.Save_GroupeActif(this.Groupeactif);

                // On enregistre le groupe comme groupe préféré pour la création de signalement
                // Si ce n'est pas le même qu'avant, on vide les thèmes préférés
                string preferredGroup = Helper.Load_PreferredGroup();
                if (preferredGroup != Profil.Groupe.Nom)
                {
                    Helper.Save_PreferredThemes(new List<string>());
                }
                Helper.Save_PreferredGroup(Profil.Groupe.Nom);
            }
            // Récupération des layers GéoPortail valides en fonction
            // de la clé Geoportail utilisateur
            this.Profil.LayersCleGeoportail = connexionServer.GetLayersFromCleGeoportailUser(this.CleGeoportail);
            return true;
        }

        /// <summary>
        /// Transforme en croquis Ripart les object sélectionnés dans la carte en cours.
        /// </summary>
        /// <returns>Liste de croquis Ripart créés à partir des objects sélectionnés.</returns>
        public List<ArcGisProEspaceCollaboratif.Core.Sketch > MakeCroquis_from_Selection()
        {
            // TODO : on ne peut pas modifier la status bar dans arcgis pro,
            // question Noémie, on remplace par quoi ?
            /*ESRI.ArcGIS.esriSystem.IStatusBar mess; 
            ESRI.ArcGIS.Framework.IApplication application = ArcMap.Application;
            mess = application.StatusBar;*/

                        if (this.MapActiveView == null)
            {
                return null;
            }

            List<ArcGisProEspaceCollaboratif.Core.Sketch> listCroquis = new List<ArcGisProEspaceCollaboratif.Core.Sketch>();
            System.Windows.Forms.TreeNode treeAttributs = Helper.Load_AttributsCroquis();

            // Get the currently selected features in the map
            QueuedTask.Run(()=>
            {
                var selectedFeatures = this.MapActiveView.Map.GetSelection();
                foreach (KeyValuePair<MapMember, List<long>> kvp in selectedFeatures)
                {
                    //get the layer of the selected feature
                    var featureLayer = kvp.Key as FeatureLayer;
                    List<long> lOid = kvp.Value;
                    foreach (long oid in lOid)
                    {
                        QueuedTask.Run(() =>
                        {
                            var feature = featureLayer.Inspect(oid);
                            var geometryFeature = feature.Shape;
                            var geometryFeatureType = geometryFeature.GeometryType;
                            switch (geometryFeatureType)
                            {
                                default:
                                    System.Windows.Forms.MessageBox.Show("Géométrie non-prise en charge pour la transformer en croquis de l'Espace collaboratif.",
                                        "IGN Espace collaboratif - WARNING",
                                        System.Windows.Forms.MessageBoxButtons.OK,
                                        System.Windows.Forms.MessageBoxIcon.Warning);
                                    break;

                                case GeometryType.Point:
                                    ArcGIS.Core.Geometry.MapPoint pointGeom = geometryFeature as ArcGIS.Core.Geometry.MapPoint;
                                    ArcGisProEspaceCollaboratif.Core.Sketch croquisTemp = Helper.MakeSketch(pointGeom);
                                    //TODO Ajouter les champs du croquis
                                    //EspaceCollaboratifHelper.AddAttributs(croquisTemp, feature, treeAttributs);
                                    listCroquis.Add(croquisTemp);
                                    break;

                                    /*case GeometryType.Polyline:
                                        for (int i = 0; i < collectionPolyline.GeometryCount; i++)
                                        {
                                            Geometry geomPath = collectionPolyline.Geometry[i];
                                            IPath path = geomPath as IPath;
                                            ArcGisProEspaceCollaboratif.Core.Croquis croquisTemp = EspaceCollaboratifHelper.MakeCroquis(path);
                                            EspaceCollaboratifHelper.AddAttributs(ref croquisTemp, feature, treeAttributs);

                                            if (collectionPolyline.GeometryCount > 1)
                                            {
                                                string multigeom = "" + (i + 1) + "/" + collectionPolyline.GeometryCount;
                                                EspaceCollaboratifHelper.AddAttributs(ref croquisTemp, "Multigéométrie", multigeom);
                                            }
                                            listCroquis.Add(croquisTemp);
                                        }
                                        break;

                                    case GeometryType.Polygon:
                                        IGeometryCollection collectionPolygon = feature.GetShape() as IGeometryCollection;

                                        for (int i = 0; i < collectionPolygon.GeometryCount; i++)
                                        {
                                            IRing ring = collectionPolygon.Geometry[i] as IRing;

                                            if (ring.IsExterior)
                                            {
                                                ArcGisProEspaceCollaboratif.Core.Croquis croquisTemp = EspaceCollaboratifHelper.MakeCroquis(ring);
                                                EspaceCollaboratifHelper.AddAttributs(ref croquisTemp, feature, treeAttributs);

                                                if (collectionPolygon.GeometryCount > 1)
                                                {
                                                    string multigeom = "" + (i + 1) + "/" + collectionPolygon.GeometryCount;
                                                    EspaceCollaboratifHelper.AddAttributs(ref croquisTemp, "Multigéométrie", multigeom);
                                                }
                                                listCroquis.Add(croquisTemp);
                                            }
                                        }
                                        break;*/
                            }
                        }); 
                    }
                }
            });
                       
            return listCroquis;
        }

        /// <summary>
        /// Renvoie la date de mise-à-jour la plus récente contenue dans les remarques présentes sur la carte.
        /// </summary>
        /// <returns>La date de mise-à-jour la plus récente contenue dans les remarques présentes sur la carte.</returns>
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
        /// Donne le décompte de remarques Ripart présentes sur la carte en cours ayant le statut indiqué.
        /// </summary>
        /// <param name="status">Le statut des remarques Ripart qu'on veut dénombrer.</param>
        /// <returns>Le décompte de remarques Ripart sur la carte ayant le statut indiqué.</returns>
        public int CountReportsByStatus(int status)
        {
            FeatureLayer reportLayer = this.GetLayerByName(Helper.nom_Calque_Signalement);
            FeatureClass reportFeatureClass = reportLayer.GetFeatureClass();
            QueryFilter queryFilter = new QueryFilter
            {
                WhereClause = Helper.nom_Champ_Statut + " = " + status
            };
           
            return reportFeatureClass.GetCount(queryFilter);
        }

        /// <summary>
        /// Donne le décompte de remarques Ripart présentes sur la carte en cours ayant le statut indiqué.
        /// </summary>
        /// <param name="statut">Le statut des remarques Ripart qu'on veut dénombrer.</param>
        /// <returns>Le décompte de remarques Ripart sur la carte ayant le statut indiqué.</returns>
        public int CountReportsByStatus(ArcGisProEspaceCollaboratif.Core.Statut status)
        {
            return this.CountReportsByStatus((int)status);
        }


        /// <summary>
        /// Met dans la sélection courante, les remarques Ripart présentes sur la carte et ayants un des statuts indiqués. 
        /// </summary>
        /// <param name="statut">La liste des statuts des remarques Ripart qu'on veut mettre en sélection.</param>
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
                IFeatureSelection remarqueSelect = calqueEspaceCollaboratif as IFeatureSelection;   // Sélection des remarques     

                remarqueSelect.SelectFeatures(queryFilter, esriSelectionResultEnum.esriSelectionResultNew, false);
            }
            else
            {
                foreach (int statutTemp in statut)
                {
                    queryFilter.WhereClause = EspaceCollaboratifHelper.nom_Champ_Statut + " = " + statutTemp; // Requête pour trouver les remarques au statut voulu.

                    IFeatureCursor pFeatureCursor = featureClass.Search(queryFilter, false);
                    IFeature pFeature = pFeatureCursor.NextFeature();
                    IFeatureSelection remarqueSelect = calqueEspaceCollaboratif as IFeatureSelection;   // Sélection des remarques     

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

            // Option pour zoomer sur les remarques sélectionnées
            if (zoom_to_selected_Remarque && coordX.Count != 0)
            {
                ArcGisProEspaceCollaboratif.Core.Box emprise = new ArcGisProEspaceCollaboratif.Core.Box(coordX.Min(), coordY.Min(), coordX.Max(), coordY.Max());
                this.Zoom(emprise);
            }

            this.ActiveView.Refresh();

            EspaceCollaboratifHelper.MessageBar(" " + coordX.Count + " remarque(s) sélectionnée(s).");
        }
*/

        /// <summary>
        /// Met dans la sélection courante, les remarques Ripart présentes sur la carte et ayants un des statuts indiqués.
        /// </summary>
        /// <param name="statut">La liste des statuts des remarques Ripart qu'on veut mettre en sélection.</param>       
/*        public void Select_Remarque_by_Statut(List<int> statut)
        {
            this.Select_Remarque_by_Statut(statut, false);
        }
        /// <summary>
        /// Met dans la sélection courante, les remarques Ripart présentes sur la carte et ayants un des statuts indiqués. 
        /// </summary>
        /// <param name="statut">La liste des statuts des remarques Ripart qu'on veut mettre en sélection.</param>
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
        /// Met dans la sélection courante, les remarques Ripart présentes sur la carte et ayants un des statuts indiqués. 
        /// </summary>
        /// <param name="statut">La liste des statuts des remarques Ripart qu'on veut mettre en sélection.</param>     
/*        public void Select_Remarque_by_Statut(List<ArcGisProEspaceCollaboratif.Core.Statut> statut)
        {
            this.Select_Remarque_by_Statut(statut);
        }
*/
        /// <summary>
        /// Met dans la sélection courante, les remarques Ripart présentes sur la carte et ayants le statut indiqué. 
        /// </summary>
        /// <param name="statut">Le statut des remarques Ripart qu'on veut mettre en sélection.</param>
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
        /// Met dans la sélection courante, les remarques Ripart présentes sur la carte et ayants le statut indiqué. 
        /// </summary>
        /// <param name="statut">Le statut des remarques Ripart qu'on veut mettre en sélection.</param>        
/*        public void Select_Remarque_by_Statut(int statut)
        {
            this.Select_Remarque_by_Statut(statut, false);
        }
*/
        /// <summary>
        /// Met dans la sélection courante, les remarques Ripart présentes sur la carte et ayants le statut indiqué. 
        /// </summary>
        /// <param name="statut">Le statut des remarques Ripart qu'on veut mettre en sélection.</param>
        /// <param name="zoom_to_selected_Remarque">Option pour zoomer sur la nouvelle sélection.</param> 
/*        public void Select_Remarque_by_Statut(ArcGisProEspaceCollaboratif.Core.Statut statut, bool zoom_to_selected_Remarque)
        {
            this.Select_Remarque_by_Statut((int)statut, zoom_to_selected_Remarque);
        }
*/
        /// <summary>
        /// Met dans la sélection courante, les remarques Ripart présentes sur la carte et ayants le statut indiqué. 
        /// </summary>
        /// <param name="statut">Le statut des remarques Ripart qu'on veut mettre en sélection.</param>       
/*        public void Select_Remarque_by_Statut(ArcGisProEspaceCollaboratif.Core.Statut statut)
        {
            this.Select_Remarque_by_Statut((int)statut, false);
        }
*/
    }
}