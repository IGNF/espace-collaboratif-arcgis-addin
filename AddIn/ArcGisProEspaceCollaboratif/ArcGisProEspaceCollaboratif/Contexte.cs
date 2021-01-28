using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Collections;

//using ESRI.ArcGIS.Carto;
//using ESRI.ArcGIS.GeoDatabase;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Data;
using ArcGIS.Core.Internal.Data.DDL;
//using ESRI.ArcGIS.ArcMapUI;
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

namespace ArcGisProEspaceCollaboratif
{
    public sealed class Contexte
    {
        private static Contexte instance = null;
        private static readonly object padlock = new object();

        //public IActiveView ActiveView; // Les paramètres concernant l'affichage de la carte en cours.
        //public IMap Map; // Les paramètres cartographiques (projection) de la carte en cours
        //public Map map;
        public MapView mapActiveView;
        //public MapView activeView;
        //public ArcGIS.Desktop.Mapping.MapView Map;//mapView;
        //public ArcGIS.Desktop.Mapping.MapTool mapTool;
        //public IFeatureWorkspace FeatureWorkspace;
        public string gdbPath = CoreModule.CurrentProject.DefaultGeodatabasePath;

        public string repertoireTravail; // Le répertoire où est la carte ArcGIS Pro sur laquelle on travaille.
        public string fichierCarteTravail; // Le fichier de la carte ArcGIS Pro sur laquelle on travaille.

        public string URLHostEspaceCollaboratif; // l'URL d'accès au service de l'espace collaboratif.
        public string LoginEspaceCollaboratif; // Le login à utiliser pour se connecter au service de l'espace collaboratif.
        public string PwdEspaceCollaboratif; // Le mot de passe associé au login pour se connecter au service de l'espace collaboratif.

        //public List<IFeatureLayer> calquesEspaceCollaboratif = new List<IFeatureLayer>(); // La liste des calques dédiés pour l'espace collaboratif dans la carte en cours.
        public List<FeatureLayer> calquesEspaceCollaboratif = new List<FeatureLayer>(); // La liste des calques dédiés pour l'espace collaboratif dans la carte en cours.
        public List<FeatureLayer> collaborativeSpaceLayers = new List<FeatureLayer>(); // La liste des calques dédiés pour l'espace collaboratif dans la carte en cours.

        //public ISpatialReference spatialReferenceEspaceCollaboratif; // Le système géodésique employé par le service de l'espace collaboratif.
        public ArcGIS.Core.Geometry.SpatialReference spatialReferenceEspaceCollaboratif; // Le système géodésique employé par le service de l'espace collaboratif
        public FormConnecter loginWindow; // Le login à utiliser pour connecter au service de l'espace collaboratif.

        private readonly EspaceCollaboratifLogger riplogger = EspaceCollaboratifLogger.Instance;
        static ILog logger = LogManager.GetLogger(typeof(Contexte));

        public Profil profil=null;
        public Client ripClient= null;

        public static Contexte Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new Contexte();
                            logger.Debug("Instance de contexte créée");
                        }
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Constructeur pour un contexte à partir de la carte courante
        /// </summary>
        private Contexte()
        {
            //IMxDocument mxDocument = ArcMap.Application.Document as IMxDocument;
            //IActiveView activeView = mxDocument.ActiveView;

            this.spatialReferenceEspaceCollaboratif = SpatialReferenceBuilder.CreateSpatialReference(4326);
            //Project project = Project.Current; //Lien entre project et mapview ?
            this.mapActiveView = MapView.Active;
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
        public async void Init(MapView activeView)
        {
            this.mapActiveView = activeView;

            this.LoginEspaceCollaboratif = "";
            this.PwdEspaceCollaboratif = "";
            this.URLHostEspaceCollaboratif = "";

            //IMapDocument mapDocument = ArcMap.Application.Document as IMapDocument;
            Project project = Project.Current;
            if (project.Name.Length == 0)
            {
                throw new Exception(@"Votre projet doit être enregistré avant de pouvoir utiliser l'add-in Espace collaboratif");
            }

            this.repertoireTravail = System.IO.Path.GetDirectoryName(project.Path);
            this.fichierCarteTravail = System.IO.Path.GetFileNameWithoutExtension(project.Name);

            this.CheckConfigFile();

            // récupération ou création de EspaceCollaboratif.gdb -> a priori inutile car une gdb est créée avec le projet ArcGIS
   //         var bCreated = await GetOrCreateFileGeodatabase();

            //création ou chargement des couches ripart
            var bLayersLoaded = await CreateOrLoadReportLayers();

            logger.Debug("Initialisation du contexte et des éléments de l'Espace collaboratif");
        }

            /// <summary>
            /// Teste si le fichier de configuration EspaceCollaboratif.xml n'existe pas dans le répertoire de travail, on le copie 
            /// du répertoire d'installation 
            /// </summary>
            /// <returns>True si il existe le fichier de configuration EspaceCollaboratif.xml à côté de la carte en cours.</returns>
            public bool CheckConfigFile()
        {

            if (!File.Exists(this.repertoireTravail + "\\" + EspaceCollaboratifHelper.nom_Fichier_Parametres_EspaceCollaboratif))
            {
                try
                {
                    File.Copy(EspaceCollaboratifHelper.EspaceCollaboratifAssemblyDir + EspaceCollaboratifHelper.nom_Fichier_Parametres_EspaceCollaboratif, this.repertoireTravail + "\\" + EspaceCollaboratifHelper.nom_Fichier_Parametres_EspaceCollaboratif);
                }
                catch (Exception e)
                {
                    logger.Error(e.Message + "\n" + e.StackTrace);
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// création ou chargement des couches de signalement de l'espace collaboratif
        /// </summary>
        private async Task<bool> CreateOrLoadReportLayers()
        {

            // Création ou chargement des calques dédiés à de l'espace collaboratif s'ils sont absents de la carte en cours.

            string polygonSketchLayer = EspaceCollaboratifHelper.nom_Calque_Croquis_Polygone;
            string lineSketchLayer = EspaceCollaboratifHelper.nom_Calque_Croquis_Ligne;
            string pointSketchLayer = EspaceCollaboratifHelper.nom_Calque_Croquis_Point;

            string reportLayer = EspaceCollaboratifHelper.nom_Calque_Signalement;

            // Signalements
            if (!this.IsLayerInMap(reportLayer))
            {
                if (!System.IO.Directory.Exists(this.gdbPath + "\\" + reportLayer))
                {
                    bool b = await EspaceCollaboratifHelper.CreerCalqueSignalementEspaceCollaboratif();
                }
                this.LoadLayer(pointSketchLayer);
            }

            // Croquis polygones
            if (!this.IsLayerInMap(polygonSketchLayer))
            {
                if (!System.IO.Directory.Exists(this.gdbPath + "\\" + polygonSketchLayer))
                {
                    bool b = await EspaceCollaboratifHelper.CreerCalqueCroquisEspaceCollaboratif(polygonSketchLayer, "POLYGON");
                }
                this.LoadLayer(polygonSketchLayer);
            }

            // Croquis lignes
            if (!this.IsLayerInMap(lineSketchLayer))
            {
                if (!System.IO.Directory.Exists(this.gdbPath + "\\" + lineSketchLayer))
                {
                    bool b = await EspaceCollaboratifHelper.CreerCalqueCroquisEspaceCollaboratif(lineSketchLayer, "POLYLINE");
                }
                this.LoadLayer(lineSketchLayer);
            }

            // Croquis points
            if (!this.IsLayerInMap(pointSketchLayer))
            {
                if (!System.IO.Directory.Exists(this.gdbPath + "\\" + pointSketchLayer))
                {
                    bool b = await EspaceCollaboratifHelper.CreerCalqueCroquisEspaceCollaboratif(pointSketchLayer, "POINT");
                }
                this.LoadLayer(pointSketchLayer);
            }
          

            // Ajout des couches à la liste collaboratifSpaceLayers
            this.collaborativeSpaceLayers.Clear();
            this.collaborativeSpaceLayers.Add(GetLayerByName(reportLayer) as FeatureLayer);
            this.collaborativeSpaceLayers.Add(GetLayerByName(pointSketchLayer) as FeatureLayer);
            this.collaborativeSpaceLayers.Add(GetLayerByName(lineSketchLayer) as FeatureLayer);
            this.collaborativeSpaceLayers.Add(GetLayerByName(polygonSketchLayer) as FeatureLayer);

            return true;

            // Ne semble pas nécessaire - à confirmer et supprimer
            //this.activeView.Redraw();
        }




        /// <summary>
        /// Essaie de charger une couche de la geodatabase
        /// </summary>
        /// <param name="layerName">nom de la couche</param> Change en layerPath
        /// <returns>bool true si la couche a pu être charchée, false sinon (la couche n'existe pas dans la gdb)</returns>
        private FeatureLayer LoadLayer(String layerName, bool doLoad = true)
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

                result = layer;
            }
            catch (Exception e)
            {
                logger.Info(layerName + " n'existe pas dans la gdb\n" + e.Message);
                result = null;
            }

            return result;
        }


        /// <summary>
        /// Récupère un calque par son nom.
        /// </summary>
        /// <param name="name">Le nom du calque qu'il faut récupérer.</param>
        /// <returns>Le calque ou null si non trouvé</returns>
        public Layer GetLayerByName(string layerName)
        {
            // Enumération des couches et groupes de couches
            IReadOnlyList<Layer> mapLayers = this.mapActiveView.Map.GetLayersAsFlattenedList();
            foreach (var layer in mapLayers)
            {
                if (layer.Name == layerName)
                    return layer;
            }

            return null;
        }


        /// <summary>
        /// Teste si l'existence d'un calque dans la carte en cours.Récupère un calque par son nom
        /// </summary>
        /// <param name="name">Le nom du calque dont on veut connaître son existence.</param>
        /// <returns>True si le calque existe, False dans le cas contraire.</returns>
        public bool IsLayerInMap(string layerName)
        {
            IReadOnlyList<Layer> mapLayers = this.mapActiveView.Map.GetLayersAsFlattenedList();
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

                var fGdbPath = this.repertoireTravail;
                var fGdbName = this.fichierCarteTravail + "_EspaceCollaboratif.gdb";

                var fGdb = fGdbPath + "\\" + fGdbName;

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
                        //return true;
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
        /*       public void EffacerCompletCalquesRipart()
               {
                   this.CreateOrLoadEspaceCollaboratifLayer();
                   foreach (IFeatureLayer calqueRipart in this.calquesRipart)
                   {
                       IQueryFilter queryFilter = new QueryFilter();
                       ITable table = (ITable)calqueEspaceCollaboratif;
                       try
                       {
                           if (table != null)
                           {

                               if (table.RowCount(queryFilter) != 0)
                               {
                                   table.DeleteSearchedRows(queryFilter);
                                   this.ActiveView.Refresh();
                               }
                           }
                       }
                       catch (Exception e)
                       {
                           logger.Error(e.Message + "\n" + e.StackTrace);
                       }
                   }
               }

       */
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
        public void CreerPointSignalement(ArcGisProEspaceCollaboratif.Core.Signalement newReport)
        {
            try
            {
                FeatureLayer featureLayer = this.calquesEspaceCollaboratif.First() as FeatureLayer;
                FeatureClass featureClass = featureLayer.GetFeatureClass();
                QueuedTask.Run(() =>
                {
                    Dictionary<string, string> dico = new Dictionary<string, string>
                    {
                        { EspaceCollaboratifHelper.nom_Champ_IdRemarque, newReport.Id.ToString() },
                        { EspaceCollaboratifHelper.nom_Champ_Auteur, newReport.Auteur.Nom },
                        { EspaceCollaboratifHelper.nom_Champ_Departement, newReport.Departement.Nom },
                        { EspaceCollaboratifHelper.nom_Champ_IDDepartement, newReport.Departement.Id},
                        { EspaceCollaboratifHelper.nom_Champ_Commune, newReport.Commune },
                        { EspaceCollaboratifHelper.nom_Champ_DateCreation, newReport.DateCreation.ToString() },
                        { EspaceCollaboratifHelper.nom_Champ_DateMAJ, newReport.DateMiseAJour.ToString() },
                        { EspaceCollaboratifHelper.nom_Champ_DateValidation, newReport.DateValidation.ToString() },
                        { EspaceCollaboratifHelper.nom_Champ_Statut, newReport.Statut.ToString() },
                        { EspaceCollaboratifHelper.nom_Champ_Themes, newReport.ConcatenateThemes()},
                        { EspaceCollaboratifHelper.nom_Champ_Url, newReport.Lien},
                        { EspaceCollaboratifHelper.nom_Champ_UrlPrive, newReport.LienPrive},
                        { EspaceCollaboratifHelper.nom_Champ_Document, newReport.GetFirstDocument() },
                        { EspaceCollaboratifHelper.nom_Champ_Message, EspaceCollaboratifHelper.Limite(newReport.Commentaire)},
                        { EspaceCollaboratifHelper.nom_Champ_Reponse, EspaceCollaboratifHelper.Limite(newReport.ConcatenateReponse())},
                        { EspaceCollaboratifHelper.nom_Champ_Autorisation, newReport.Autorisation }
                    };
                    RowBuffer rowBuffer = EspaceCollaboratifHelper.UpdateReportFields(dico);
                    Feature featureSignalement = featureClass.CreateRow(rowBuffer);

                    // Placement géographique du point d'application de la remarque Ripart
                    featureSignalement.SetShape(EspaceCollaboratifHelper.TransformPoint(newReport.Position));

                    featureSignalement.Store();
                });

                //  Traitement du ou des croquis associé(s) au signalement     
                if (!newReport.IsCroquisEmpty())
                {
                    foreach (ArcGisProEspaceCollaboratif.Core.Sketch oneSketch in newReport.Sketch)
                    {
                        if (oneSketch.Points.Count == 0)
                        {
                            //   this.debugForm.WriteLine("Croquis sans coordonnées dans la remarque n°" + uneRemarque.Id);
                            continue;
                        }
                        else
                        {
                            QueuedTask.Run(() =>
                            {
                                // on cast le featureLayer en fonction du type du croquis pour utiliser le bon calque associé
                                FeatureLayer featureLayerCroquis = this.calquesEspaceCollaboratif[(int)oneSketch.Type] as FeatureLayer;
                                FeatureClass featureClassCroquis = featureLayerCroquis.GetFeatureClass();
                                QueuedTask.Run(() =>
                                {
                                    String attributs = "";
                                    foreach (ArcGisProEspaceCollaboratif.Core.Attribut attribut in oneSketch.Attributs)
                                    {
                                        attributs += attribut.Nom + " = '" + attribut.Valeur + "' | ";
                                    }

                                    if (oneSketch.Attributs.Count != 0)
                                    {
                                        attributs = attributs.Substring(0, attributs.Length - 3);
                                    }

                                    Dictionary<string, string> dico = new Dictionary<string, string>
                                    {
                                        { EspaceCollaboratifHelper.nom_Champ_LienRemarque, newReport.Id.ToString() },
                                        { EspaceCollaboratifHelper.nom_Champ_NomCroquis, oneSketch.Nom },
                                        { EspaceCollaboratifHelper.nom_Champ_Attributs, EspaceCollaboratifHelper.Limite(attributs) }
                                    };
                                    RowBuffer rowBuffer = EspaceCollaboratifHelper.UpdateReportFields(dico);
                                    Feature featureCroquis = featureClassCroquis.CreateRow(rowBuffer);

                                    //Polyline polylineCroquis = new Polyline() as Polyline;
                                    //Polygon polygonCroquis = new Polygon() as Polygon;
                                    ArcGIS.Core.Geometry.MapPoint pointCroquis = EspaceCollaboratifHelper.TransformPoint(oneSketch.Points.First());
                                    // Construction géométrique du croquis en fonction de son type et à partir du vecteur de vertex du croquis.
                                    switch (oneSketch.Type)
                                    {
                                        default:
                                            break;

                                        case ArcGisProEspaceCollaboratif.Core.Sketch.SketchType.Point:
                                            featureCroquis.SetShape(pointCroquis);
                                            break;

                                            /*case ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Ligne:
                                                featureCroquis.Shape = EspaceCollaboratifHelper.GeometryFromCroquis(polylineCroquis, unCroquis);
                                                break;

                                            case ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Polygone:
                                                featureCroquis.Shape = EspaceCollaboratifHelper.GeometryFromCroquis(polygonCroquis, unCroquis);
                                                break;

                                            case ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Fleche:
                                                featureCroquis.Shape = EspaceCollaboratifHelper.GeometryFromCroquis(polylineCroquis, unCroquis);
                                                break;

                                            case ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Texte:
                                                featureCroquis.Shape = pointCroquis;
                                                break;*/
                                    }
                                    featureCroquis.Store();
                                });
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message + "\n" + e.ToString());
            }
        }


        /// <summary>
        /// Calcule la BBox Ripart qui enveloppe une liste d'objects géométriques.
        /// </summary>
        /// <param name="geometriesFiltres">La liste des Geometry dont on veut obtenir l'enveloppe globale.</param>
        /// <returns>Ripart.Core.Box qui enveloppe tous les Geometry de <paramref name="geometriesFiltres"/>.</returns>
        /*       public ArcGisProEspaceCollaboratif.Core.Box GetBBox(List<Geometry> geometriesFiltres)
               {
                   if (geometriesFiltres.Count == 0) { return new ArcGisProEspaceCollaboratif.Core.Box(); }

                   IEnvelope2 bbox = (IEnvelope2)new Envelope();

                   foreach (Geometry geometrie in geometriesFiltres)
                   {
                       IEnvelope bboxTemp = geometrie.Envelope;
                       bbox.Union(bboxTemp);
                   }

                   return new ArcGisProEspaceCollaboratif.Core.Box(bbox.XMin, bbox.YMin, bbox.XMax, bbox.YMax);
               }
       */
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

        /// <summary>
        /// Zoom à l'écran sur une emprise donnée.
        /// </summary>
        /// <param name="emprise">L'object Ripart.Core.Box sur laquelle il faut faire le zoom à l'écran.</param>
        /*       public void Zoom(ArcGisProEspaceCollaboratif.Core.Box emprise)
               {
                   IEnvelope2 bbox = (IEnvelope2)new Envelope();
                   bbox.SpatialReference = this.spatialReferenceEspaceCollaboratif;
                   bbox.PutCoords(emprise.XMin, emprise.YMin, emprise.XMax, emprise.YMax);
                   this.ActiveView.Extent = bbox;
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
        /// <summary>
        /// Retourne la liste des géométries destinées à servir au filtrage spatial lors de l'importation des remarques.
        /// </summary>
        /// <returns>Liste d'Geometry contenant les géométries devant servir pour le filtrage spatial lors de l'importation des remarques.</returns>
        /*        public List<Geometry> GetGeometryFiltreSpatial()
                {
                    List<Geometry> geometryFiltreSpatial = new List<Geometry>();

                    // Récupération de la liste des géométries servant pour le filtrage spatial à partir des objects sélectionnés dans la carte en cours.
                    geometryFiltreSpatial = this.GetGeometryFiltreSpatial_from_selection();

                    // Si la récupération par sélection est vide (car aucun object séléectionné ou aucun ayant la géométrie adéquate), alors on récupère les géométries contenues dans le calque définit par le fichier de paramètre.
                    if (geometryFiltreSpatial.Count == 0)
                    {
                        geometryFiltreSpatial = this.GetGeometryFiltreSpatial_from_XML();
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
        /*        public List<Geometry> GetGeometryFiltreSpatial(string calqueFiltrage)
                {
                    List<Geometry> geometryFiltreSpatial = new List<Geometry>();

                    ILayer layerFiltrage = this.GetLayerByName(calqueFiltrage);

                    if (layerFiltrage == null) { return geometryFiltreSpatial; }

                    IFeatureLayer featureLayerFiltrageSpatial = layerFiltrage as IFeatureLayer;
                    IFeatureClass featureClassFiltrageSpatial = featureLayerFiltrageSpatial.FeatureClass;
                    IQueryFilter filtreSpatial = new QueryFilter();

                    IFeatureCursor cursor = featureClassFiltrageSpatial.Search(
                                filtreSpatial,
                                false // important : sinon on n'obtient qu'un seul objet
                            );
                    Feature featureFiltrageSpatial = cursor.NextFeature();

                    while (featureFiltrageSpatial != null)
                    {
                        Geometry contourFiltrageSpatial = featureFiltrageSpatial.GetShape();
                        contourFiltrageSpatial.Project(this.spatialReferenceEspaceCollaboratif);
                        geometryFiltreSpatial.Add(contourFiltrageSpatial);
                        featureFiltrageSpatial = cursor.NextFeature();
                    }

                    return geometryFiltreSpatial;
                }
        */
        /// <summary>
        /// Récupère à partir du calque indiqué dans le fichier XML de configuration, la liste des géométries destinées à servir au filtrage spatial lors de l'importation des remarques .
        /// </summary>
        /// <returns>Liste d'Geometry contenant les géométries devant servir pour le filtrage spatial lors de l'importation des remarques.</returns>
        /*        public List<Geometry> GetGeometryFiltreSpatial_from_XML()
                {
                    List<Geometry> geometryFiltreSpatial = new List<Geometry>();

            string nom_FichierParametre = EspaceCollaboratifHelper.XML_NameFile();

                    XmlDocument doc = new XmlDocument();
                    doc.Load(nom_FichierParametre);

            // XmlNodeList elemCalqueExtraction = doc.GetElementsByTagName("Zone_extraction");
            XmlNodeList elemCalqueExtraction = doc.GetElementsByTagName(EspaceCollaboratifHelper.XML_Suffixe(EspaceCollaboratifHelper.xml_Zone_extraction));
                    IEnumerator ienum;

                    // Parcour des calques contenant les objects de filtrage spatial d'après de le XML de paramétrage
                    for (int i = 0; i < elemCalqueExtraction.Count; i++)
                    {
                        string nomCalqueExtraction = elemCalqueExtraction[i].Attributes["calque"].Value;

                        if (nomCalqueExtraction.Length == 0)
                        { continue; }

                        ILayer calqueExtraction = this.GetLayerByName(nomCalqueExtraction);
                        if (calqueExtraction == null)
                        { continue; }

                        ienum = elemCalqueExtraction[i].GetEnumerator();

                        // Parcour objects de filtrage spatial au sein du même calque
                        while (ienum.MoveNext())
                        {
                            XmlNode noeud = (XmlNode)ienum.Current;

                            string idObjectExtraction = noeud.Attributes["ID"].Value;
                            string valObjectExtraction = noeud.InnerText;

                            IFeatureLayer featureLayerFiltrageSpatial = calqueExtraction as IFeatureLayer;
                            IFeatureClass featureClassFiltrageSpatial = featureLayerFiltrageSpatial.FeatureClass;

                            IQueryFilter filtreSpatial = new QueryFilter
                            {

                                // Recherche de l'object filtrant spatial d'après le nom et la valeur de son identifiant
                                WhereClause = idObjectExtraction + "=" + valObjectExtraction
                            };

                            IFeatureCursor cursor = featureClassFiltrageSpatial.Search(
                                filtreSpatial,
                                false // important : sinon, on a un seul objet
                            );
                            Feature featureFiltrageSpatial = cursor.NextFeature();

                            while (featureFiltrageSpatial != null)
                            {
                                Geometry contourFiltrageSpatial = featureFiltrageSpatial.GetShape();
                                contourFiltrageSpatial.Project(this.spatialReferenceEspaceCollaboratif);
                                geometryFiltreSpatial.Add(contourFiltrageSpatial);
                                featureFiltrageSpatial = cursor.NextFeature();
                            }

                        }

                        ienum.Reset();
                    }

                    return geometryFiltreSpatial;
                }
        */
        /// <summary>
        /// Récupère à partir des objects sélectionnés dans la carte en cours, la liste des géométries destinées à servir au filtrage spatial lors de l'importation des remarques .
        /// </summary>
        /// <returns>Liste Geometry contenant les géométries devant servir pour le filtrage spatial lors de l'importation des remarques.</returns>
        /*        public List<Geometry> GetGeometryFiltreSpatial_from_selection()
                {
                    List<Geometry> geometryFiltreSpatial = new List<Geometry>();

                    // Obtention des objects sélectionnés
                    IEnumFeature enumFeature = this.Map.FeatureSelection as IEnumFeature;
                    Feature feature = enumFeature.Next();

                    while (feature != null)
                    {
                        if (EspaceCollaboratifHelper.TestGeometrieFiltrageSpatial(feature))
                        {
                            Geometry contourFiltrageSpatial = feature.GetShape();
                            contourFiltrageSpatial.Project(this.spatialReferenceEspaceCollaboratif);
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
        public ArcGisProEspaceCollaboratif.Core.IClient GetConnexionEspaceCollaboratif()
        {
            logger.Debug("GetConnexionEspaceCollaboratif ");

            //this.URLHostEspaceCollaboratif = EspaceCollaboratifHelper.Load_Urlhost();
            this.URLHostEspaceCollaboratif = "https://espacecollaboratif.ign.fr";
//            this.URLHostEspaceCollaboratif = "https://espacecollaboratif.ign.fr";

            logger.Debug("this.URLHostEspaceCollaboratif " + this.URLHostEspaceCollaboratif);

            bool premiereConnexion = false;

            this.loginWindow = new FormConnecter();

            // Recherche du login par défaut dans le fichier XML de paramétrage
            //this.LoginEspaceCollaboratif = EspaceCollaboratifHelper.Load_Login();

            // Lancement du formulaire de saisie du login et mot de passe                
            this.loginWindow.SetLogin(this.LoginEspaceCollaboratif);


            for (int tentativeConnexion = 0; tentativeConnexion < 3; tentativeConnexion++)          
            {
                logger.Debug("tentative de connexion ");
                // Si il n'y a pas de login ou de mot de passe enregistré, on lance le formulaire de saisi LogEspaceCollaboratif
                if (this.LoginEspaceCollaboratif.Length == 0 || this.PwdEspaceCollaboratif.Length == 0)
                {
                    // Recherche du login par défaut dans le fichier XML de paramétrage
                    //this.LoginEspaceCollaboratif = EspaceCollaboratifHelper.Load_Login();

                    // Lancement du formulaire de saisi du login et mot de passe                
                    this.loginWindow.SetLogin(this.LoginEspaceCollaboratif);

                    // On s'arrrête là si l'utilisateur a cliqué sur le bouton "abandonner"
                    if (this.loginWindow.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        this.loginWindow.Close();
                        this.loginWindow = null;

                        return null;
                    }

                    // Récupération du login et mot de passe introduits.
                    this.LoginEspaceCollaboratif = this.loginWindow.GetLogin();
                    this.PwdEspaceCollaboratif = this.loginWindow.GetPassword();

                    premiereConnexion = true;
                }

                try
                {
                    // Création de la connexion au serveur.
                    ArcGisProEspaceCollaboratif.Core.IClient uneConnexionEspaceCollaboratif = new Client(
                        this.URLHostEspaceCollaboratif,
                        this.LoginEspaceCollaboratif,
                        this.PwdEspaceCollaboratif
                    );

                    this.profil = uneConnexionEspaceCollaboratif.GetProfil();
                   
                    if (premiereConnexion)
                    {
                        this.loginWindow.Close();
                        this.loginWindow = null;

                        logger.Info("Création de la connexion au serveur " + uneConnexionEspaceCollaboratif.ToString());

                        FormInfo popupEspaceCollaboratif = new FormInfo();

                        popupEspaceCollaboratif.SetLogo(profil.Logo);

                        popupEspaceCollaboratif.SetMessage("Connexion réussie à l'Espace collaboratif.");
                        popupEspaceCollaboratif.AddMessage("");
                        popupEspaceCollaboratif.AddMessage(" Serveur: " + this.URLHostEspaceCollaboratif);
                        popupEspaceCollaboratif.AddMessage(" Login: " + this.LoginEspaceCollaboratif);
                        popupEspaceCollaboratif.AddMessage(" Profil: " + profil.Titre);
                        popupEspaceCollaboratif.AddMessage(" Zone: " + profil.Zone);

                        popupEspaceCollaboratif.StartCountDown(10);
                        popupEspaceCollaboratif.ShowDialog();

                        //EspaceCollaboratifHelper.Save_Login(this.LoginEspaceCollaboratif);
                    }
                    return uneConnexionEspaceCollaboratif;
                }
                catch (Exception erreurConnexion)
                {
                    this.PwdEspaceCollaboratif = "";

                    switch (erreurConnexion.Message.ToString())
                    {
                        case "(401) Unauthorized":
                            this.loginWindow.Notifier("Login et/ou mot de passe incorrects");

                            break;
                        case "Login inconnu":
                            this.loginWindow.Notifier("''" + this.LoginEspaceCollaboratif + "'' n'est pas un utilisateur enregistré dans le service de l'Espace collaboratif.");
                            break;

                        case "no_group":
                            this.loginWindow.Notifier("Accès refusé. L'utilisateur n'appartient à aucun groupe.");
                            break;

                        default:
                            MessageBox.Show("Impossible d'accéder au service de l'Espace collaboratif à l'adresse suivante: " + this.URLHostEspaceCollaboratif +
                                            "\n\nVeuillez contacter le support de l'Espace collaboratif: \n" + erreurConnexion.Message.ToString() + ".", "IGN Espace collaboratif",
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                    }
                }
            }
            this.loginWindow.Close();
            this.loginWindow = null;
            return null;
        }

        /// <summary>
        /// Transforme en croquis Ripart les object sélectionnés dans la carte en cours.
        /// </summary>
        /// <returns>Liste de croquis Ripart créés à partir des objects sélectionnés.</returns>
        public List<ArcGisProEspaceCollaboratif.Core.Sketch> MakeCroquis_from_Selection()
        {
            // TODO : on ne peut pas modifier la status bar dans arcgis pro,
            // question Noémie, on remplace par quoi ?
            /*ESRI.ArcGIS.esriSystem.IStatusBar mess; 
            ESRI.ArcGIS.Framework.IApplication application = ArcMap.Application;
            mess = application.StatusBar;*/

            if (this.mapActiveView == null)
            {
                return null;
            }

            List<ArcGisProEspaceCollaboratif.Core.Sketch> listCroquis = new List<ArcGisProEspaceCollaboratif.Core.Sketch>();
            System.Windows.Forms.TreeNode treeAttributs = EspaceCollaboratifHelper.Load_AttributsCroquis();

            // Get the currently selected features in the map
            QueuedTask.Run(() =>
            {
                var selectedFeatures = this.mapActiveView.Map.GetSelection();
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
                                    ArcGisProEspaceCollaboratif.Core.Sketch croquisTemp = EspaceCollaboratifHelper.MakeSketch(pointGeom);
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
                       

                        



                  
        




            /*IEnumFeature enumFeature = this.Map.FeatureSelection as IEnumFeature;
            IEnumFeatureSetup pEnumFeatureSetup = (IEnumFeatureSetup)enumFeature;
            pEnumFeatureSetup.AllFields = true;

            Feature feature = enumFeature.Next();
            //int total = this.Map.SelectionCount;
            int step = 0;

            //mess.ShowProgressBar("Génération des croquis de l'Espace collaboratif à partir des objets sélectionnés...", 0, total, 1, true);
            //mess.ProgressBar.Position = 0;
            while (feature != null)
            {
                step++;
                //mess.ProgressBar.Position = step;
                //mess.set_Message(0, "Génération des croquis de l'Espace collaboratif n°" + step + "/" + total + "...");
                Geometry geometryFeature = feature.GetShape() as Geometry;
                geometryFeature.Project(this.spatialReferenceEspaceCollaboratif);
                if (geometryFeature.GeometryType == ArcGIS.Core.Geometry.GeometryType.Point)
                {
                    ArcGIS.Core.Geometry.MapPoint pointGeom = geometryFeature as ArcGIS.Core.Geometry.MapPoint;
                    ArcGisProEspaceCollaboratif.Core.Croquis croquisTemp = EspaceCollaboratifHelper.MakeCroquis(pointGeom);
                    EspaceCollaboratifHelper.AddAttributs(ref croquisTemp, feature, treeAttributs);
                    listCroquis.Add(croquisTemp);
                }
                else
                {
                    IPolycurve3 courbe = feature.GetShape() as IPolycurve3;

                    courbe.Project(this.spatialReferenceEspaceCollaboratif);
                    ArcGIS.Core.Geometry.GeometryType type = feature.GetShape().GeometryType;

                    switch (type)
                    {
                        case ArcGIS.Core.Geometry.GeometryType.Polyline:
                            courbe.DensifyByAngle(150.00, Math.PI / 180 * 2);

                            IGeometryCollection collectionPolyline = feature.GetShape() as IGeometryCollection;

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

                        case ArcGIS.Core.Geometry.GeometryType.Polygon:
                            courbe.Densify(250, 0);
                            IPolygon4 polygon = courbe as IPolygon4;
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
                            break;

                        default:
                            System.Windows.Forms.MessageBox.Show("Géométrie non-prise en charge pour la transformer en croquis de l'Espace collaboratif.", "IGN Espace collaboratif", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                            break;
                    }
                }
                feature = enumFeature.Next();
            }
            //mess.HideProgressBar();*/
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
        /// <param name="statut">Le statut des remarques Ripart qu'on veut dénombrer.</param>
        /// <returns>Le décompte de remarques Ripart sur la carte ayant le statut indiqué.</returns>
/*        public int Count_Remarque_by_Statut(int statut)
        {
            FeatureLayer calqueEspaceCollaboratif = this.calquesEspaceCollaboratif.First();
            FeatureClass featureClass = calqueEspaceCollaboratif.GetFeatureClass();
            QueryFilter queryFilter = new QueryFilter
            {
                WhereClause = EspaceCollaboratifHelper.nom_Champ_Statut + " = " + statut
            };
           
            return featureClass.GetCount(queryFilter);
        }
*/
        /// <summary>
        /// Donne le décompte de remarques Ripart présentes sur la carte en cours ayant le statut indiqué.
        /// </summary>
        /// <param name="statut">Le statut des remarques Ripart qu'on veut dénombrer.</param>
        /// <returns>Le décompte de remarques Ripart sur la carte ayant le statut indiqué.</returns>
/*        public int Count_Remarque_by_Statut(ArcGisProEspaceCollaboratif.Core.Statut statut)
        {
            return this.Count_Remarque_by_Statut((int)statut);
        }
*/

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