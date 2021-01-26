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

using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;

using log4net;
using ArcGisProEspaceCollaboratif.Core;


namespace ArcGisProEspaceCollaboratif
{
    public sealed class Contexte
    {
        private static Contexte instance = null;
        private static readonly object padlock = new object();

        //public IActiveView ActiveView; // Les paramètres concernant l'affichage de la carte en cours.
        //public IMap Map; // Les paramètres cartographiques (projection) de la carte en cours
        public Map map;
        public MapView mapView;
        public MapView activeView;
        //public ArcGIS.Desktop.Mapping.MapView Map;//mapView;
        //public ArcGIS.Desktop.Mapping.MapTool mapTool;
        //public IFeatureWorkspace FeatureWorkspace;

        public string repertoireTravail; // Le répertoire où est la carte ArcGIS Pro sur laquelle on travaille.
        public string fichierCarteTravail; // Le fichier de la carte ArcGIS Pro sur laquelle on travaille.

        public string URLHostEspaceCollaboratif; // l'URL d'accès au service de l'espace collaboratif.
        public string LoginEspaceCollaboratif; // Le login à utiliser pour se connecter au service de l'espace collaboratif.
        public string PwdEspaceCollaboratif; // Le mot de passe associé au login pour se connecter au service de l'espace collaboratif.

        //public List<IFeatureLayer> calquesEspaceCollaboratif = new List<IFeatureLayer>(); // La liste des calques dédiés pour l'espace collaboratif dans la carte en cours.
        public List<FeatureLayer> calquesEspaceCollaboratif = new List<FeatureLayer>(); // La liste des calques dédiés pour l'espace collaboratif dans la carte en cours.
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
            this.activeView = MapView.Active;
            this.Init(this.activeView);

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
        private void Init(MapView activeView)
        {
            this.activeView = activeView;
            //this.Map = activeView.FocusMap;

            this.LoginEspaceCollaboratif = "";
            this.PwdEspaceCollaboratif = "";
            this.URLHostEspaceCollaboratif = "";

            //IMapDocument mapDocument = ArcMap.Application.Document as IMapDocument;
            Project project = Project.Current;
            if (project.Name.Length == 0)
            {
                throw new Exception(@"Votre document mxd doit être enregistré avant de pouvoir utiliser l'add-in Espace collaboratif");
            }

            this.repertoireTravail = System.IO.Path.GetDirectoryName(project.Path);
            this.fichierCarteTravail = System.IO.Path.GetFileNameWithoutExtension(project.Name);

            this.CheckConfigFile();

            // récupération ou création de RIPART.gdb
// TO-DO            this.FeatureWorkspace = this.GetOrCreateFeatureWorkspace();

            //création ou chargement des couches ripart
// TO-DO            this.CreateOrLoadEspaceCollaboratifLayer();

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
        /// création ou chargement des couches de l'espace collaboratif
        /// </summary>
/*        private void CreateOrLoadEspaceCollaboratifLayer()
        {

            // Création ou chargement des calques dédiés à de l'espace collaboratif s'ils sont absents de la carte en cours.
            if (!this.IsPresentLayerByName(EspaceCollaboratifHelper.nom_Calque_Croquis_Fleche))
            {
                if (this.LoadLayer(EspaceCollaboratifHelper.nom_Calque_Croquis_Fleche) == null)
                {
                    Map.AddLayer(EspaceCollaboratifHelper.CreerCalqueCroquisEspaceCollaboratif(EspaceCollaboratifHelper.nom_Calque_Croquis_Fleche,
                                                                        this.FeatureWorkspace,
                                                                        this.spatialReferenceEspaceCollaboratif,
                                                                        ArcGIS.Core.Geometry.GeometryType.Polyline));
                }
            }
            if (!this.IsPresentLayerByName(EspaceCollaboratifHelper.nom_Calque_Croquis_Texte))
            {
                if (this.LoadLayer(EspaceCollaboratifHelper.nom_Calque_Croquis_Texte) == null)
                {
                    Map.AddLayer(EspaceCollaboratifHelper.CreerCalqueCroquisEspaceCollaboratif(EspaceCollaboratifHelper.nom_Calque_Croquis_Texte,
                                                                        this.FeatureWorkspace,
                                                                        this.spatialReferenceEspaceCollaboratif,
                                                                        ArcGIS.Core.Geometry.GeometryType.Point));
                }
            }
            if (!this.IsPresentLayerByName(EspaceCollaboratifHelper.nom_Calque_Croquis_Polygone))
            {
                if (this.LoadLayer(EspaceCollaboratifHelper.nom_Calque_Croquis_Polygone) == null)
                {
                    Map.AddLayer(EspaceCollaboratifHelper.CreerCalqueCroquisEspaceCollaboratif(EspaceCollaboratifHelper.nom_Calque_Croquis_Polygone,
                                                                        this.FeatureWorkspace,
                                                                        this.spatialReferenceEspaceCollaboratif,
                                                                        ArcGIS.Core.Geometry.GeometryType.Polygon));
                }
            }
            if (!this.IsPresentLayerByName(EspaceCollaboratifHelper.nom_Calque_Croquis_Ligne))
            {
                if (this.LoadLayer(EspaceCollaboratifHelper.nom_Calque_Croquis_Ligne) == null)
                {
                    Map.AddLayer(EspaceCollaboratifHelper.CreerCalqueCroquisEspaceCollaboratif(EspaceCollaboratifHelper.nom_Calque_Croquis_Ligne,
                                                                        this.FeatureWorkspace,
                                                                        this.spatialReferenceEspaceCollaboratif,
                                                                        ArcGIS.Core.Geometry.GeometryType.Polyline));
                }
            }
            if (!this.IsPresentLayerByName(EspaceCollaboratifHelper.nom_Calque_Croquis_Point))
            {

                if (this.LoadLayer(EspaceCollaboratifHelper.nom_Calque_Croquis_Point) == null)
                {
                    Map.AddLayer(EspaceCollaboratifHelper.CreerCalqueCroquisEspaceCollaboratif(EspaceCollaboratifHelper.nom_Calque_Croquis_Point,
                                                                        this.FeatureWorkspace,
                                                                        this.spatialReferenceEspaceCollaboratif,
                                                                        ArcGIS.Core.Geometry.GeometryType.Point));
                }

            }
            if (!this.IsPresentLayerByName(EspaceCollaboratifHelper.nom_Calque_Remarque))
            {
                FeatureLayer fl = this.LoadLayer(EspaceCollaboratifHelper.nom_Calque_Remarque, false);
                if (fl == null)
                {
                    fl = EspaceCollaboratifHelper.CreerCalqueRemarqueEspaceCollaboratif(this.FeatureWorkspace, this.spatialReferenceEspaceCollaboratif);
                }


                //importation du fichier lyr  
                //vérifie que le fichier Remarque_EspaceCollaboratif.lyr existe dans le répertoire de travail. Sinon le copie depuis EspaceCollaboratifDir
                if (!File.Exists(this.repertoireTravail + "/" + EspaceCollaboratifHelper.calque_Remarque_Lyr))
                {
                    if (!File.Exists(EspaceCollaboratifHelper.EspaceCollaboratifAssemblyDir + EspaceCollaboratifHelper.calque_Remarque_Lyr))
                    {
                        MessageBox.Show(@"Le fichier " + EspaceCollaboratifHelper.EspaceCollaboratifAssemblyDir + EspaceCollaboratifHelper.calque_Remarque_Lyr + @" n'existe pas.Le style du calque des remarques ne peut donc pas être chargé.", "IGN Espace collaboratif", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        Map.AddLayer(fl);
                    }
                    else
                    {
                        File.Copy(EspaceCollaboratifHelper.EspaceCollaboratifAssemblyDir + EspaceCollaboratifHelper.calque_Remarque_Lyr, this.repertoireTravail + "/" + EspaceCollaboratifHelper.calque_Remarque_Lyr);


                    }
                }
                ESRI.ArcGIS.Catalog.IGxLayer gxLayer = new ESRI.ArcGIS.Catalog.GxLayer();
                ESRI.ArcGIS.Catalog.IGxFile gxFile = (ESRI.ArcGIS.Catalog.IGxFile)gxLayer;

                // set du path du fichier lyr
                gxFile.Path = this.repertoireTravail + "/" + EspaceCollaboratifHelper.calque_Remarque_Lyr;

                if (!(gxLayer.Layer == null))
                {
                    IGeoFeatureLayer f = (IGeoFeatureLayer)gxLayer.Layer;
                    IGeoFeatureLayer fr = (IGeoFeatureLayer)fl;
                    fr.Renderer = f.Renderer;

                    Map.AddLayer(fr);
                }

            }

            // récupération ou création des couches
            calquesEspaceCollaboratif.Clear();
            this.calquesEspaceCollaboratif.Add(GetLayerByName(EspaceCollaboratifHelper.nom_Calque_Remarque) as FeatureLayer);
            this.calquesEspaceCollaboratif.Add(GetLayerByName(EspaceCollaboratifHelper.nom_Calque_Croquis_Point) as FeatureLayer);
            this.calquesEspaceCollaboratif.Add(GetLayerByName(EspaceCollaboratifHelper.nom_Calque_Croquis_Ligne) as FeatureLayer);
            this.calquesEspaceCollaboratif.Add(GetLayerByName(EspaceCollaboratifHelper.nom_Calque_Croquis_Polygone) as FeatureLayer);
            this.calquesEspaceCollaboratif.Add(GetLayerByName(EspaceCollaboratifHelper.nom_Calque_Croquis_Texte) as FeatureLayer);
            this.calquesEspaceCollaboratif.Add(GetLayerByName(EspaceCollaboratifHelper.nom_Calque_Croquis_Fleche) as FeatureLayer);


            ActiveView.Refresh();
        }
*/



        /// <summary>
        /// Essaie de charger une couche de la geodatabase
        /// </summary>
        /// <param name="layerName">nom de la couche</param> Change en layerPath
        /// <returns>bool true si la couche a pu être charchée, false sinon (la couche n'existe pas dans la gdb)</returns>
/*        private FeatureLayer LoadLayer(String layerPath, bool doLoad = true)
        {
            //FeatureLayer result = null;
            //FeatureClass featclass;
            //ArcGIS.Desktop.Mapping.LayerFactory layerFactory = ArcGIS.Desktop.Mapping.LayerFactory.Instance();
            //ArcGIS.Desktop.Mapping.FeatureLayer featureLayer = (ArcGIS.Desktop.Mapping.FeatureLayer)layer;

            FeatureLayer result = null;

            //TO-DO : récupérer seulement le nom de la couche et pas le chemin complet
            string layerName = layerPath;

            try
            {
                int indexNumber = 0;
                System.Uri layerUri = new System.Uri(layerPath);



                FeatureLayer layer = LayerFactory.Instance.CreateFeatureLayer(
                    layerUri,
                    this.mapView.Map,
                    indexNumber,
                    layerName);

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
/*      public ILayer GetLayerByName(string name)
        {
            // Enumération des calques et des groupes de calques.
            IEnumLayer listLayer = this.Map.get_Layers(null, true);
            Layer layer = listLayer.Next();
            while (layer != null)
            {
                if (layer.Name.Equals(name)) { return layer; }
                layer = listLayer.Next();
            }
            return null;
        }
 */

 /*       public Layer GetLayerByName(string name)
        {
            // Enumération des calques et des groupes de calques.
            EnumLayer listLayer = this.Map.get_Layers(null, true);
            Layer layer = listLayer.Next();
            while (layer != null)
            {
                if (layer.Name.Equals(name)) { return layer; }
                layer = listLayer.Next();
            }
            return null;
        }
*/
        /// <summary>
        /// Teste si l'existence d'un calque dans la carte en cours.Récupère un calque par son nom
        /// </summary>
        /// <param name="name">Le nom du calque dont on veut connaître son existence.</param>
        /// <returns>True si le calque existe, False dans le cas contraire.</returns>
/*        public bool IsPresentLayerByName(string layerName)
        {
            int numberOfLayers = this.Map.LayerCount;
            for (System.Int32 i = 0; i < numberOfLayers; i++)
            {
                if (layerName == this.Map.get_Layer(i).Name)
                { return true; }
            }

            return false;
        }
*/

        /// <summary>
        /// Ouvre les fichiers géodatabase EspaceCollaboratif.gdb contenant les données de l'espace collaboratif dans la carte en cours.
        /// Si ces fichiers n'existent pas, ils sont préalablement créés dans le même répertoire où se situe la carte en cours.
        /// </summary>
        /// <returns>L'IFeatureWorkspace de l'espace de travail des calques dédiés à l'espace collaboratif.</returns>
/*        private IFeatureWorkspace GetOrCreateFeatureWorkspace()
        {
            string folder = this.repertoireTravail;
            string nomFichierEspaceCollaboratif = this.fichierCarteTravail + "_EspaceCollaboratif";
            string nomRepFicGdb = folder + "\\" + nomFichierEspaceCollaboratif + ".gdb";


            // Instantiate a file geodatabase workspace factory and create a file geodatabase.
            // The Create method returns a workspace name object.
            Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);

            IWorkspaceName workspaceName = null;
            IWorkspace workspace = null;

            bool bFichierEspaceCollaboratifExistant = System.IO.Directory.Exists(nomRepFicGdb);

            if (bFichierEspaceCollaboratifExistant)
            {
                // Create a FileGeodatabaseConnectionPath with the name of the file geodatabase you wish to create
                FileGeodatabaseConnectionPath fileGeodatabaseConnectionPath = new FileGeodatabaseConnectionPath(new Uri(nomRepFicGdb));

//                ESRI.ArcGIS.esriSystem.PropertySet fichierPropertySet = new ESRI.ArcGIS.esriSystem.PropertySet();
//                workspace = workspaceFactory.OpenFromFile(nomRepFicGdb, 0) as IWorkspace;

                logger.Debug("Ouverture geodatabase existante : " + nomRepFicGdb);
            }
            else
            {
                // Create and use the file geodatabase


                FileGeodatabaseConnectionPath fileGeodatabaseConnectionPath = new FileGeodatabaseConnectionPath(new Uri(nomRepFicGdb));
                using (Geodatabase geodatabase = SchemaBuilder.CreateGeodatabase(fileGeodatabaseConnectionPath))
                {
                    
                }
                workspaceName = workspaceFactory.Create(folder, nomFichierEspaceCollaboratif, null, 0);

                // Cast the workspace name object to the IName interface and open the workspace.
                ESRI.ArcGIS.esriSystem.IName name = (ESRI.ArcGIS.esriSystem.IName)workspaceName;
                workspace = (IWorkspace)name.Open();

                logger.Debug("Création geodatabase :" + nomRepFicGdb);

            }

            IFeatureWorkspace featureWorkspace = workspace as IFeatureWorkspace;
            return featureWorkspace;
        }
*/

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
        /// <param name="uneRemarque">La remarque Ripart qu'il faut placer sur la carte en cours.</param>
/*        public void CreerPointSignalement(ArcGisProEspaceCollaboratif.Core.Signalement unSignalement)
        {

            // on cast en featureLayer
            FeatureLayer featureLayer = this.calquesEspaceCollaboratif.First() as FeatureLayer;
            IFeatureClass featureClass = featureLayer.FeatureClass;
            IFeature featureRemarque = featureClass.CreateFeature();

            // Placement géographique du point d'application de la remarque Ripart
            featureRemarque.Shape = EspaceCollaboratifHelper.TransformPoint(unSignalement.Position);

            // Remplissage des attributs de la remarque Ripart
            EspaceCollaboratifHelper.CompleteChampEspaceCollaboratif(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_IdRemarque, unSignalement.Id);
            EspaceCollaboratifHelper.CompleteChampEspaceCollaboratif(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_Auteur, unSignalement.Auteur.Nom);
            EspaceCollaboratifHelper.CompleteChampEspaceCollaboratif(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_Departement, unSignalement.Departement.Nom);
            EspaceCollaboratifHelper.CompleteChampEspaceCollaboratif(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_IDDepartement, unSignalement.Departement.Id);
            EspaceCollaboratifHelper.CompleteChampEspaceCollaboratif(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_Commune, unSignalement.Commune);
            EspaceCollaboratifHelper.CompleteChampEspaceCollaboratif(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_DateCreation, unSignalement.DateCreation);
            EspaceCollaboratifHelper.CompleteChampEspaceCollaboratif(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_DateMAJ, unSignalement.DateMiseAJour);
            EspaceCollaboratifHelper.CompleteChampEspaceCollaboratif(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_DateValidation, unSignalement.DateValidation);
            EspaceCollaboratifHelper.CompleteChampEspaceCollaboratif(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_Statut, (int)unSignalement.Statut);
            EspaceCollaboratifHelper.CompleteChampEspaceCollaboratif(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_Themes, unSignalement.ConcatenateThemes());
            EspaceCollaboratifHelper.CompleteChampEspaceCollaboratif(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_Url, unSignalement.Lien);
            EspaceCollaboratifHelper.CompleteChampEspaceCollaboratif(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_UrlPrive, unSignalement.LienPrive);
            EspaceCollaboratifHelper.CompleteChampEspaceCollaboratif(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_Document, unSignalement.GetFirstDocument());
            EspaceCollaboratifHelper.CompleteChampEspaceCollaboratif(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_Message, EspaceCollaboratifHelper.Limite(unSignalement.Commentaire));
            EspaceCollaboratifHelper.CompleteChampEspaceCollaboratif(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_Reponse, EspaceCollaboratifHelper.Limite(unSignalement.ConcatenateReponse()));
            EspaceCollaboratifHelper.CompleteChampEspaceCollaboratif(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_Autorisation, unSignalement.Autorisation);

            featureRemarque.Store();


            //  Traitement du ou des croquis associé(s) à la remarque            
            if (!unSignalement.IsCroquisEmpty())
            {

                foreach (ArcGisProEspaceCollaboratif.Core.Croquis unCroquis in unSignalement.Croquis)
                {
                    if (unCroquis.Points.Count == 0)
                    {
                        //   this.debugForm.WriteLine("Croquis sans coordonnées dans la remarque n°" + uneRemarque.Id);
                    }
                    else
                    {
                        // on cast le featureLayer en fonction du type du croquis pour utiliser le bon calque associé
                        FeatureLayer featureLayerCroquis = this.calquesEspaceCollaboratif[(int)unCroquis.Type] as FeatureLayer;
                        //IFeatureClass featureClassCroquis = featureLayerCroquis.FeatureClass;
                        FeatureClass featureClassCroquis = featureLayerCroquis.GetFeatureClass();

                        IFeature featureCroquis = featureClassCroquis.CreateFeature();
                        //Feature featureCroquis = featureClassCroquis.CreateRow(); -> a mettre a la fin ?

                        Polyline polylineCroquis = new Polyline() as Polyline;
                        Polygon polygonCroquis = new Polygon() as Polygon;
                        ArcGIS.Core.Geometry.MapPoint pointCroquis = EspaceCollaboratifHelper.TransformPoint(unCroquis.Points.First());

                        try
                        {
                            // Construction géométrique du croquis en fonction de son type et à partir du vecteur de vertex du croquis.
                            switch (unCroquis.Type)
                            {
                                case ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Point:
                                    featureCroquis.Shape = pointCroquis;
                                    break;

                                case ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Ligne:
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
                                    break;
                            }

                            EspaceCollaboratifHelper.CompleteChampEspaceCollaboratif(featureClassCroquis, featureCroquis, EspaceCollaboratifHelper.nom_Champ_LienRemarque, uneRemarque.Id);
                            EspaceCollaboratifHelper.CompleteChampEspaceCollaboratif(featureClassCroquis, featureCroquis, EspaceCollaboratifHelper.nom_Champ_NomCroquis, unCroquis.Nom);

                            String attributs = "";

                            foreach (ArcGisProEspaceCollaboratif.Core.Attribut attribut in unCroquis.Attributs)
                            {
                                attributs += attribut.Nom + " = '" + attribut.Valeur + "' | ";
                            }

                            if (unCroquis.Attributs.Count != 0)
                            {
                                attributs = attributs.Substring(0, attributs.Length - 3);
                            }

                            EspaceCollaboratifHelper.CompleteChampEspaceCollaboratif(featureClassCroquis, featureCroquis, EspaceCollaboratifHelper.nom_Champ_Attributs, EspaceCollaboratifHelper.Limite(attributs));

                            featureCroquis.Store();
                        }
                        catch (Exception e)
                        {
                            logger.Error(e.Message + "\n" + e.ToString());
                        }
                    }
                }
            }
        }
*/

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

            string nom_FichierParametre = EspaceCollaboratifHelper.EspaceCollaboratifXML_NameFile();

            XmlDocument doc = new XmlDocument();
            doc.Load(nom_FichierParametre);

            // XmlNodeList elemCalqueExtraction = doc.GetElementsByTagName("Zone_extraction");
            XmlNodeList elemCalqueExtraction = doc.GetElementsByTagName(EspaceCollaboratifHelper.EspaceCollaboratifXML_Suffixe(EspaceCollaboratifHelper.xml_Zone_extraction));

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
/*        public List<ArcGisProEspaceCollaboratif.Core.Croquis> MakeCroquis_from_Selection()
        {
            ESRI.ArcGIS.esriSystem.IStatusBar mess; 
            ESRI.ArcGIS.Framework.IApplication application = ArcMap.Application;
            mess = application.StatusBar;

            List<ArcGisProEspaceCollaboratif.Core.Croquis> listCroquis = new List<ArcGisProEspaceCollaboratif.Core.Croquis>();
            System.Windows.Forms.TreeNode treeAttributs = EspaceCollaboratifHelper.Load_AttributsCroquis();

            // Obtention des objects sélectionnés
            IEnumFeature enumFeature = this.Map.FeatureSelection as IEnumFeature;
            IEnumFeatureSetup pEnumFeatureSetup = (IEnumFeatureSetup)enumFeature;
            pEnumFeatureSetup.AllFields = true;

            Feature feature = enumFeature.Next();

            int total = this.Map.SelectionCount;
            int step = 0;

            mess.ShowProgressBar("Génération des croquis de l'Espace collaboratif à partir des objets sélectionnés...", 0, total, 1, true);
            mess.ProgressBar.Position = 0;


            while (feature != null)
            {
                step++;

                mess.ProgressBar.Position = step;
                mess.set_Message(0, "Génération des croquis de l'Espace collaboratif n°" + step + "/" + total + "...");


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

            mess.HideProgressBar();

            return listCroquis;
        }
*/
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