using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using log4net;
using ArcGisProEspaceCollaboratif.Core;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Core;
using System.Windows.Forms;
using ArcGIS.Core.Internal.CIM;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace ArcGisProEspaceCollaboratif
{
    /// <summary>
    /// Classe contenant des utilitaires pour le plugin
    /// </summary>
    public class Helper
    {
        public const string name_file_espaceco_gdb = "espaceco.gdb";
        public const string name_file_espaceco_xml = "espaceco.xml";
        public const string name_file_extensions = "formats.txt";
        public const string name_file_about_status = "AboutStatusResponse.txt";
        public const string name_file_manuel = "Espace_co_Add-in_ArcGIS_Pro.pdf";

        public const string name_layer_Signalement = "Signalement";
        public const string name_layer_Croquis_Polygone = "Croquis_EC_Polygone";
        public const string name_layer_Croquis_Ligne = "Croquis_EC_Ligne";
        public const string name_layer_Croquis_Point = "Croquis_EC_Point";
        //public const string name_group_layer = "Espace collaboratif";

        public static List<string> CollaborativeSpaceLayers = new ()
        {
            name_layer_Signalement,
            name_layer_Croquis_Point,
            name_layer_Croquis_Ligne,
            name_layer_Croquis_Polygone,           
        };

        public const string name_field_IdReport = "NoSignalement";
        public const string name_field_Auteur = "Auteur";
        public const string name_field_Commune = "Commune";
        public const string name_field_Insee = "Insee";
        public const string name_field_Departement = "Département";
        public const string name_field_IDDepartement = "Département_ID";
        public const string name_field_DateCreation = "Date_de_création";
        public const string name_field_DateMAJ = "Date_MAJ";
        public const string name_field_DateValidation = "Date_de_validation";
        public const string name_field_Themes = "Thèmes";
        public const string name_field_Statut = "Statut";
        public const string name_field_Message = "Message";
        public const string name_field_Reponse = "Réponses";
        public const string name_field_Url = "URL";
        public const string name_field_UrlPrive = "URL_privé";
        public const string name_field_Document = "Document";
        public const string name_field_Autorisation = "Autorisation";
        public const string name_field_LienReport = "Lien_signalement";
        public const string name_field_NomCroquis = "Nom";
        public const string name_field_Attributs = "Attributs_croquis";
        public const string name_field_Source = "Source";
        public const string name_field_Longitude = "Lon";
        public const string name_field_Latitude = "Lat";
        public const string name_field_Shape = "Shape";
        public const string xml_UrlHost = "/Parametres_connexion_a_EspaceCollaboratif/Serveur/URLHost";
        public const string xml_Login = "/Parametres_connexion_a_EspaceCollaboratif/Serveur/Login";
        public const string xml_StartDateExtraction = "/Parametres_connexion_a_EspaceCollaboratif/Map/Debut_date_extraction";
        public const string xml_EndDateExtraction = "/Parametres_connexion_a_EspaceCollaboratif/Map/Fin_date_extraction";
        public const string xml_Pagination = "/Parametres_connexion_a_EspaceCollaboratif/Map/Pagination";
        public const string xml_Themes = "/Parametres_connexion_a_EspaceCollaboratif/Map/Themes_preferes/Theme";
        public const string xml_Zone_extraction = "/Parametres_connexion_a_EspaceCollaboratif/Map/Zone_extraction";
        public const string xml_AfficherCroquis = "/Parametres_connexion_a_EspaceCollaboratif/Map/Afficher_Croquis";
        public const string xml_AttributsCroquis = "/Parametres_connexion_a_EspaceCollaboratif/Map/Attributs_croquis";
        public const string xml_BaliseNomCalque = "Calque_Nom";
        public const string xml_BaliseChampCalque = "Calque_Champ";
        public const string xml_Group = "/Parametres_connexion_a_EspaceCollaboratif/Map/Import_pour_groupe";
        public const string xml_Proxy = "/Parametres_connexion_a_EspaceCollaboratif/Serveur/Proxy";
        public const string xml_GroupeActif = "/Parametres_connexion_a_EspaceCollaboratif/Serveur/groupe_actif";
        public const string xml_GroupePrefere = "/Parametres_connexion_a_EspaceCollaboratif/Serveur/groupe_prefere";

        public const string dateDefault = "01/01/2000";
        public const int lengthMaxField = 5000;

        public static readonly string EspaceCollaboratifDirectoryFiles = string.Format("{0}\\Files\\", System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
        public static readonly string EspaceCollaboratifDirectoryImages = string.Format("{0}\\Images\\", System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));

        /// <summary>
        /// Le logger qui permet d'enregistrer des informations sur le processus
        /// </summary>
        public static readonly log4net.ILog logger = LogManager.GetLogger(typeof(Helper));

        // Dictionnaire des attributs de la couche signalements (avec types et contraintes)
        public static readonly Dictionary<string, KeyValuePair<string, string>> reportAttributes = new ()
        {
            { name_field_IdReport, new KeyValuePair<string,string> ("LONG","") },
            { name_field_Auteur, new KeyValuePair<string,string> ("TEXT", "50") },
            { name_field_Commune, new KeyValuePair<string,string> ("TEXT", "") },
            { name_field_Insee, new KeyValuePair<string, string> ("TEXT", "") },
            { name_field_Departement, new KeyValuePair<string,string> ("TEXT", "23") },
            { name_field_IDDepartement, new KeyValuePair<string,string> ("TEXT", "3") },
            { name_field_DateCreation, new KeyValuePair<string,string> ("DATE", "") },
            { name_field_DateMAJ, new KeyValuePair<string,string> ("DATE", "") },
            { name_field_DateValidation, new KeyValuePair<string,string> ("DATE", "") },
            { name_field_Themes, new KeyValuePair<string,string> ("TEXT",  Helper.lengthMaxField.ToString()) },
            { name_field_Statut, new KeyValuePair<string,string> ("LONG", "") },
            { name_field_Message, new KeyValuePair<string,string> ("TEXT", Helper.lengthMaxField.ToString()) },
            { name_field_Reponse, new KeyValuePair<string,string> ("TEXT", Helper.lengthMaxField.ToString()) },
            { name_field_Url, new KeyValuePair<string,string> ("TEXT", "") },
            { name_field_Document, new KeyValuePair<string,string> ("TEXT",  Helper.lengthMaxField.ToString()) },
            { name_field_Autorisation, new KeyValuePair<string,string> ("TEXT", "") },
            { name_field_Source, new KeyValuePair<string,string> ("TEXT", "") }
        };

        // Dictionnaire des attributs des couches croquis (avec types et contraintes)
        public static readonly Dictionary<string, KeyValuePair<string, string>> sketchAttributes = new()
        {
            { name_field_LienReport, new KeyValuePair<string,string> ("LONG", "") },
            { name_field_NomCroquis, new KeyValuePair<string, string>("TEXT", "") },
            { name_field_Attributs, new KeyValuePair<string, string>("TEXT", Helper.lengthMaxField.ToString()) },
//            { name_field_LienBDuni, new KeyValuePair<string, string>("TEXT", "") }
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="editOperation"></param>
        public static void ExecuteEditOperation(EditOperation editOperation)
        {
            try
            {
                bool editResult = editOperation.Execute();
                if (!editResult)
                {
                    throw new Exception (editOperation.ErrorMessage);
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Helper.ExecuteEditOperation : {0}\n", e.Message));
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetFileAboutStatusResponse()
        {
            string about = "";
            string assemblyDir = Helper.EspaceCollaboratifDirectoryFiles;
            string fileTxt = string.Format("{0}{1}", assemblyDir, name_file_about_status);
            using (StreamReader sr = new (fileTxt))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    about += string.Format("{0}\n", line);
                }
            }
            return about;
        }

        /// <summary>
        /// Vérifie si l'extension du fichier joint à un nouveau signalement
        /// est inclu dans le fichier
        /// ~\source\repos\arcgis_pro_espace_collaboratif\AddIn\ArcGisProEspaceCollaboratif\ArcGisProEspaceCollaboratif\Files
        /// </summary>
        /// <param name="extension">L'extension du fichier à vérifier</param>
        /// <returns>false si l'extension n'existe pas dans le fichier formats.txt</returns>
        public static bool IsFileExtensionAuthorised(string extension)
        {
            string assemblyDir = Helper.EspaceCollaboratifDirectoryFiles;
            string fileTxt = string.Format("{0}{1}", assemblyDir, name_file_extensions);
            using (StreamReader sr = new (fileTxt))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line == extension)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Teste si le signalement est compris entre les dates de début et fin d'extraction
        /// </summary>    
        /// <param name="reportDateUpdate">La date de mise à jour du signalement.</param>
        /// <param name="startDate">La date de début d'extraction.</param>
        /// /// <param name="endDate">La date de fin d'extraction.</param>
        /// <returns>True si le signalement est compris entre les dates de début et fin</returns>
        public static bool IsInDates(DateTime reportDateUpdate, DateTime startDate, DateTime endDate)
        {
            int resultStart = DateTime.Compare(reportDateUpdate, startDate);
            int resultEnd = DateTime.Compare(reportDateUpdate, endDate);
            if (resultStart < 0 || resultEnd > 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Teste si le signalement est contenu à l'intérieur d'une des géométrie fournie en entrée.  
        /// </summary>    
        /// <param name="report">le signalement EspaceCollaboratif à tester.</param>
        /// <param name="geometrys">La liste des géométries à tester pour le filtrage spatial.</param>
        /// <returns>True si le signalement à tester est incluse à l'intérieur d'une des géométries fournies en entrée.</returns>
        public static bool IsInGeometry(ArcGisProEspaceCollaboratif.Core.Report report, List<ArcGIS.Core.Geometry.Geometry> geometrys)
        {
            MapPoint reportPoint = Helper.TransformPoint(report.Position);
            // ArcGIS.Core.Geometry.Geometry resultPoint = GeometryEngine.Instance.Project(reportPoint, SpatialReferences.WGS84);
            foreach (ArcGIS.Core.Geometry.Geometry geometry in geometrys)
            {
                ArcGIS.Core.Geometry.Geometry geomResult = GeometryEngine.Instance.Project(geometry, SpatialReferences.WGS84);
                if (!geomResult.IsEmpty)
                {
                    if (GeometryEngine.Instance.Intersects(reportPoint, geomResult))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Limite la longueur d'une string pour ne pas dépasser la taille maximale que ne peut contenir les attributs d'un calque.
        /// La taille maximale est défini par la constante EspaceCollaboratifHelper.longueurMaxChamp.
        /// </summary>
        /// <param name="champ"> L'object string dont on doit éventuellement limiter sa longueur.
        /// <returns>L'object champ raccourcie à ses EspaceCollaboratifHelper.longueurMaxChamp premiers caractères si il dépasse cette taille limite</returns>
        public static string Limite(string champ)
        {
            if (champ.Length > Helper.lengthMaxField)
            {
                return champ.Substring(0, Helper.lengthMaxField);
            }
            else
            {
                return champ;
            }
        }

        /// <summary>
        /// Génère à partir d'un croquis EspaceCollaboratif, son équivalent Geometry pouvant être mise dans une shape.
        /// </summary>
        /// <param name="uneGeometrie">Une Geometry de départ pour définir la nature géométrique de la IGeometry en sortie.</param>
        /// <param name="unCroquis">Le croquis à transformer en IGeometry équivalent.</param>
        /// <returns>Une Geometry équivalente au croquis en entrée.</returns>
        public static List<MapPoint> GetPointCollectionFromSketch(Sketch currSketch)
        {
            List<MapPoint> pointCollection = new ();

            foreach (Core.Point sketchVertex in currSketch.Points)
            {
                pointCollection.Add(Helper.TransformPoint(sketchVertex));
            }

            return pointCollection;
        }

        public static ArcGIS.Core.Geometry.SpatialReference IsDefaultSpatialReference()
        {
            //Par défaut GCS_WGS_1984
            ArcGIS.Core.Geometry.SpatialReference spatialRef = SpatialReferences.WGS84;
            // Si la référence de la carte n'est pas WGS84, il faut prendre celle de la carte
            MapView mapUser = MapView.Active;
            if (mapUser.Map == null)
            {
                string message = "Pas de carte active, impossible de déterminer le SRID de celle-ci";
                logger.Error(string.Format("Helper.IsDefaultSpatialReference : {0}\n", message));
                throw new Exception(message);
            }
            ArcGIS.Core.Geometry.SpatialReference mapUserSpatialReference = mapUser.Map.SpatialReference;
            string nameMapUserSpatialReference = mapUserSpatialReference.Name;
            string nameDefaultSpatialReference = spatialRef.Name;
            if (nameMapUserSpatialReference != nameDefaultSpatialReference)
            {
                spatialRef = mapUserSpatialReference;
            }
            return spatialRef;
        }


        /// <summary>
        /// Convertit un point ArcGisProEspaceCollaboratif.Core.Point en son équivalent ArcGIS.Core.Geometry.MapPoint. 
        /// </summary>
        /// <param name="pointin">Le Point ArcGisProEspaceCollaboratif.Core qu'on veut convertir en MapPoint.</param>
        /// <returns>Le point converti.</returns>
        public static ArcGIS.Core.Geometry.MapPoint TransformPoint(ArcGisProEspaceCollaboratif.Core.Point pointin)
        {
            MapPoint point = null;
            QueuedTask.Run(() =>
            {
                // ArcGIS.Core.Geometry.SpatialReference spatialRef = IsDefaultSpatialReference();
                point = MapPointBuilderEx.CreateMapPoint(pointin.Longitude, pointin.Latitude, SpatialReferences.WGS84);        
            });

            return point;
        }
       
        /// <summary>
        /// Convertit un MapPoint en son équivalent ArcGisProEspaceCollaboratif.Core.Point
        /// </summary>
        /// <param name="pointIn">Le MapPoint qu'on veut convertir en ArcGisProEspaceCollaboratif.Core.Point</param>
        /// <returns>Le point converti.</returns>
        public static ArcGisProEspaceCollaboratif.Core.Point TransformPoint(ArcGIS.Core.Geometry.MapPoint pointIn)
        {
            if (pointIn.IsEmpty)
            {
                return new ArcGisProEspaceCollaboratif.Core.Point();
            }

            // S'il faut arrondir la valeur des coordonnées du point
            return new Core.Point(pointIn.X, pointIn.Y);
        }
        
        /// <summary>
        /// Calcule le barycentre d'une liste de point.
        /// </summary>
        /// <param name="points">La liste des points ArcGIS dont on veut déterminer leur barycentre.</param>
        /// <returns>Le Point ArcGIS positionné sur le barycentre calculé.</returns>
        public static MapPoint Barycentre(List<MapPoint> points)
        {
            if (points.Count == 0)
            {
                string message = "Pas de points, impossible de calculer le barycentre";
                logger.Error(string.Format("Helper.Barycentre : {0}\n", message));
                throw new Exception(message);
            }
            if (points.Count == 1)
            {
                return points.First();
            }

            double barycentreX = 0;
            double barycentreY = 0;

            foreach (MapPoint pointTemp in points)
            {
                barycentreX += pointTemp.X;
                barycentreY += pointTemp.Y;
            }

            barycentreX /= points.Count;
            barycentreY /= points.Count;

            MapPoint pointResult = null;
            ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                MapPointBuilderEx mapPointBuilder = new ();
                mapPointBuilder.SetValues(barycentreX, barycentreY);
                pointResult = mapPointBuilder.ToGeometry();
                
            });

            if (pointResult.IsEmpty)
            {
                string message = "Impossible de déterminer un barycentre avec les coordonnées de la liste de points en entrée";
                logger.Error(string.Format("Helper.Barycentre.MapPointBuilder : {0}\n", message));
                throw new Exception(message);
            }

            return pointResult;
        }

        /// <summary>
        /// Calcule la distance entre deux points.
        /// </summary>
        /// <param name="point1">Le premier point d'extrémité.</param>
        /// <param name="point2">Le second point d'extrémité.</param>
        /// <returns>La distance entre les points <paramref name="point1"/> et <paramref name="point2"/>.</returns>
        public static double Distance(MapPoint point1, MapPoint point2)
        {
            return Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
        }

        /// <summary>
        /// Calcule le centroïde d'un object croquis EspaceCollaboratif.       
        /// C'est le centroïde de l'équivalent ArcGIS Pro du croquis initial.
        /// </summary>
        /// <param name="sketchIn">Le croquis dont il faut calculer son centroïde.</param>
        /// <returns>Le centroïde calculé depuis <paramref name="sketchIn"/>.</returns>  
        public static ArcGIS.Core.Geometry.MapPoint Centroid(ArcGisProEspaceCollaboratif.Core.Sketch sketchIn)
        {
            // Si le croquis n'est pas défini
            if (sketchIn.Type == ArcGisProEspaceCollaboratif.Core.Sketch.SketchType.Vide || sketchIn.Points.Count == 0)
            {
                return null;
            }

            // Si le croquis se résume à un ponctuel
            if (sketchIn.Type == ArcGisProEspaceCollaboratif.Core.Sketch.SketchType.Point)
            {
                return Helper.TransformPoint(sketchIn.FirstCoord());
            }

            return null;
        }

        /// <summary>
        /// Calcule le centroïde de chaque croquis EspaceCollaboratif contenus dans une liste.  
        /// </summary>
        /// <param name="listCroquisEntree">La liste contenant les objects croquis EspaceCollaboratif dont il faut déterminer leur centroïde.</param>
        /// <returns>La liste des centroïdes calculés depuis les croquis contenus dans <paramref name="listCroquisEntree"/>.</returns>  
        public static MapPoint Centroid(List<ArcGisProEspaceCollaboratif.Core.Sketch> listCroquisEntree)
        {
            switch (listCroquisEntree.Count)
            {
                case 0:
                    return null;

                case 1:
                    return Helper.Centroid(listCroquisEntree.First());

                default:

                    List<MapPoint> listCentroid = new ();
                    foreach (ArcGisProEspaceCollaboratif.Core.Sketch croquis in listCroquisEntree)
                    {
                        listCentroid.Add(Helper.Centroid(croquis));
                    }

                    return Helper.Barycentre(listCentroid);
            }
        }

        /// <summary>
        /// Teste si le champ donné d'un calque donné doit être mis ou non en attribut pour un futur croquis.
        /// </summary>
        /// <param name="nomCalque">Le nom du calque.</param>
        /// <param name="nomChamp">Le nom du champ.</param>
        /// <param name="treeLayerAndField">L'arborescence des calques et de leurs champs devant être mis en attribut pour le nouveau croquis.</param>
        /// <returns>True si le calque et le champ donnés sont inclus dans l'arborescence <paramref name="treeLayerAndField"/>.</returns>
        public static bool IsFieldGoodForAttribut(string nomCalque, string nomChamp, System.Windows.Forms.TreeNode treeLayerAndField)
        {  
            for (int i = 0; i < treeLayerAndField.Nodes.Count; i++)
            {
                if (treeLayerAndField.Nodes[i].Text.Equals(nomCalque))
                {
                    for (int j = 0; j < treeLayerAndField.Nodes[i].Nodes.Count; j++)
                    {
                        if (treeLayerAndField.Nodes[i].Nodes[j].Text.Equals(nomChamp))
                        {
                            return true;
                        }
                    }
                }
            }  
            return false;
        }

        /// <summary>
        /// Récupère l'ensemble des attributs d'un objet sous la forme d'un dictionnaire [Nom:valeur]
        /// Supprime pour un champ 'Date' les hh:mm:ss
        /// </summary>
        /// <param name="inspector">Accès aux attributs d'un objet</param>
        /// <param name="fieldDescription">La liste des attributs</param>
        /// <returns>Retourne un dictionnaire avec nom et valeur des attributs</returns>
        public static Dictionary<string, string> GetAttributes(
            ArcGIS.Desktop.Editing.Attributes.Inspector inspector,
            List<FieldDescription> fieldDescription
        )
        {
            Dictionary<string, string> attributes = new ();

            foreach(FieldDescription fieldDesc in fieldDescription)
            {
                string originalValue = inspector[fieldDesc.Name].ToString();
                if (fieldDesc.Type == FieldType.Date && originalValue.Length != 0)
                {
                    if (originalValue.Substring(originalValue.Length - 9).Equals(" 00:00:00"))
                    {
                        originalValue = originalValue.Substring(0, originalValue.Length - 9);
                    }
                }
                attributes.Add(fieldDesc.Name, originalValue);
            }

            return attributes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Core.Point ReplaceSpatialReferenceToPoint(MapPoint point)
        {
            ArcGIS.Core.Geometry.Geometry result = GeometryEngine.Instance.Project(point, SpatialReferences.WGS84);
            MapPoint projectedPt = result as MapPoint;
            return TransformPoint(projectedPt);
        }

        /// <summary>
        /// Génère un croquis pour l'Espace collaboratif à partir d'une géométrie ArcGIS pro.
        /// La géométrie du nouveau croquis est celle issue de la conversion de la géométrie donnée en entrée.
        /// </summary>
        /// <param name="geometry">La géométrie qu'il faut convertir en croquis de l'Espace collaboratif</param>
        /// <returns>Le croquis pour l'Espace collaboratif</returns>
        public static ArcGisProEspaceCollaboratif.Core.Sketch MakeSketch(ArcGIS.Core.Geometry.Geometry geometry)
        {
            if (geometry == null)
            {
                return null;
            }
            ArcGisProEspaceCollaboratif.Core.Sketch newSketch = new ();

            // Selon le type géométrique de la géométrie à traiter.
            switch (geometry.GeometryType)
            {
                case GeometryType.Point:
                    newSketch.SetType(Sketch.SketchType.Point);
                    MapPoint point = geometry as MapPoint;
                    newSketch.AddPoint(ReplaceSpatialReferenceToPoint(point));
                    break;

                case GeometryType.Polyline:
                    newSketch.SetType(Sketch.SketchType.Ligne);
                    ArcGIS.Core.Geometry.Polyline polyline = geometry as ArcGIS.Core.Geometry.Polyline;
                    foreach (MapPoint mapPoint in polyline.Points)
                    {
                        newSketch.AddPoint(ReplaceSpatialReferenceToPoint(mapPoint));
                    }
                    break;
                  
                case GeometryType.Polygon:
                    newSketch.SetType(Sketch.SketchType.Polygone);
                    ArcGIS.Core.Geometry.Polygon polygon = geometry as ArcGIS.Core.Geometry.Polygon;
                    foreach (MapPoint mp in polygon.Points)
                    {
                        newSketch.AddPoint(ReplaceSpatialReferenceToPoint(mp));
                    }
                    break;

                default:
                    string message = "Géométrie non prise en charge pour la transformer en croquis.";
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                        message,
                        Constantes.WARNING
                    );
                    logger.Warn(message);
                    break;
            }

            return newSketch;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static ArcGisProEspaceCollaboratif.Core.Point CalculatePositionReport(List<Core.Point> points)
        {
            Core.Point pointResult = new ();

            double barycentreX = 0;
            double barycentreY = 0;

            foreach (Core.Point point in points)
            {
                barycentreX += point.Longitude;
                barycentreY += point.Latitude;
            }

            barycentreX /= points.Count;
            barycentreY /= points.Count;

            pointResult.Longitude = barycentreX;
            pointResult.Latitude = barycentreY;

            return pointResult;
        }

        /// <summary>
        /// Calcule le point d'application pour un nouveau signalement à partir des croquis associés à ce futur signalement.
        /// On calcule le centroïde de chaque croquis contenu dans <paramref name="listSketchs"/>, puis le barycentre de l'ensemble de ces centroïdes calculés et enfin on retient celui qui est le plus proche du barycentre calculé.
        /// </summary>
        /// <param name="listSketchs">La liste contenant les croquis du futur signalement.</param>        
        /// <returns>Le point sur lequel sera centré le nouveau signalement contenant les croquis de <paramref name="listSketchs"/>.</returns>
        public static ArcGisProEspaceCollaboratif.Core.Point CalculatePointReport(List<ArcGisProEspaceCollaboratif.Core.Sketch> listSketchs)
        {
            switch (listSketchs.Count)
            {
                case 0:
                    string message = "Aucun objet sélectionné.\nIl est donc impossible de déterminer la position du nouveau signalement à créer.";
                    logger.Error(string.Format("Helper.CalculatePointReport : {0}\n", message));
                    throw new Exception(message);

                case 1:
                    ArcGisProEspaceCollaboratif.Core.Sketch sketchOne = listSketchs.First();

                    if (sketchOne.Type == ArcGisProEspaceCollaboratif.Core.Sketch.SketchType.Point)
                    {
                        return sketchOne.FirstCoord();
                    }
                    else
                    {
                        return Helper.TransformPoint(Helper.Centroid(sketchOne));
                    }

                default:
                    
                    ArcGIS.Core.Geometry.MapPoint barycentre = Helper.Centroid(listSketchs);

                    List<double> distanceSketch = new ();
                    List<MapPoint> centroidSketch = new ();

                    foreach (ArcGisProEspaceCollaboratif.Core.Sketch croquis in listSketchs)
                    {               
                        MapPoint ptTemp = Helper.Centroid(croquis);
                        centroidSketch.Add(ptTemp);
                        distanceSketch.Add(Helper.Distance(ptTemp, barycentre));
                    }

                    int rang = 0;

                    // Recherche du centroïde le plus proche du barycentre
                    for (int i = 1; i < distanceSketch.Count; i++)
                    {
                        if (distanceSketch[i] < distanceSketch[rang])
                        {
                            rang = i;
                        }
                    }

                    return Helper.TransformPoint(centroidSketch[rang]);
            }
        }

        /// <summary>
        /// Renvoie le chemin complet d'accès du fichier XML de paramétrage pour le fonctionnement de l'add-in EspaceCollaboratif pour ArcGis Pro.
        /// Il doit être situé dans le même répertoire que celui où se trouve le fichier de la carte ouverte en cours dans ArcGios Pro, et son nom est "espaceco.xml".
        /// </summary>     
        /// <returns>Le chemin complet + nom du fichier du fichier de paramétrage.</returns>
        public static string XML_NameFile()
        {
            Project project = Project.Current;
            string fullPath = System.IO.Path.GetDirectoryName(project.Path);
            return string.Format("{0}\\{1}", fullPath, Helper.name_file_espaceco_xml);
        }

        /// <summary>
        /// Charge en mémoire le fichier XML de paramétrage nécéssaire au fonctionnement de l'add-on EspaceCollaboratif.
        /// Cette opération est nécéssaire avant touts autres manipulations ultérieures (lecture ou écriture).
        /// </summary>
        /// <returns>Un object XmlDocument contenant le chargement du fichier XML de paramétrage EspaceCollaboratif.</returns>
        public static XmlDocument XML_Load()
        {
            string nom_FichierParametre = Helper.XML_NameFile();
            XmlDocument paramsXML = new ();
            paramsXML.Load(nom_FichierParametre);        
            return paramsXML;
        }

        /// <summary>
        /// Teste la présence ou non  d'une balise à l'intérieur du fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap.
        /// </summary>
        /// <param name="baliseTest">Le chemin complet de la balise à rechercher à l'intérieur du fichier de paramétrage.</param>  
        /// <returns>True si le fichier de paramétrage contient la balise <paramref name="baliseTest"/>, False dans le cas contraire.</returns>
        public static bool XML_HasElement(string baliseTest)
        {
            XmlDocument paramsXML = Helper.XML_Load();
            XmlNode noeudTest = paramsXML.SelectSingleNode(baliseTest);

            return (noeudTest != null);
        }

        /// <summary>
        /// Efface à l'intérieur du fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap, tous les éléments inclus dans la balise indiquée en entrée.
        /// Dans le fichier de paramétrage, la balise est conservée, mais elle est vidée de ses éléments qui étaient inclus auparavant.
        /// </summary>
        /// <param name="balise">Le chemin complet de la balise à rechercher à l'intérieur du fichier de paramétrage, à l'intérieur de laquelle il faut effacer tous ses éléments.</param>       
        public static void XML_ClearElements(string balise)
        {
            XmlDocument paramsXML = Helper.XML_Load();
            XmlNode noeud = paramsXML.SelectSingleNode(balise);
            noeud.RemoveAll();
            paramsXML.Save(Helper.XML_NameFile());
        }

        /// <summary>
        /// Écrit à l'intérieur du fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap, la valeur donnée pour la balise indiquée.
        /// Si la balise donnée n'existe pas dans le fichier de paramétrage, elle est alors d'abord créée.
        /// </summary>
        /// <param name="balise">Le chemin complet de la balise à rechercher à l'intérieur du fichier de paramétrage dans laquelle on souhaite écrire sa valeur.</param> 
        /// <param name="valeur">La valeur à écrire pour la balise indiquée.</param> 
        public static void XML_SetElement(string balise, string valeur)
        {
            if (!Helper.XML_HasElement(balise))
            {
                Helper.XML_AddElement(balise);
            }

            XmlDocument paramsXML = Helper.XML_Load();
            XmlNode noeudRoot = paramsXML.SelectSingleNode(balise);
            noeudRoot.InnerText = valeur;
            paramsXML.Save(Helper.XML_NameFile());
        }

        /// <summary>
        /// Ajoute à l'intérieur du fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap, un nouveau noeud à l'emplacement indiqué.
        /// </summary>
        /// <param name="root">L'emplacement à l'intérieur du fichier de paramétrage où il faut ajouter le nouveau noeud.</param> 
        /// <param name="noeudNouveau">Le nom du nouveau noeud à ajouter.</param> 
        public static void XML_AddElement(string root, string noeudNouveau)
        {
            XmlDocument paramsXML = Helper.XML_Load();
            XmlNode noeudRoot = paramsXML.SelectSingleNode(root);
            XmlElement elem = paramsXML.CreateElement(noeudNouveau);
            noeudRoot.AppendChild(elem);
            paramsXML.Save(Helper.XML_NameFile());
        }

        /// <summary>
        /// Ajoute à l'intérieur du fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap, un nouveau noeud à l'emplacement indiqué.
        /// L'argument <paramref name="noeudNouveau"/> contient à la fois l'emplacement et le nom du nouveau noeud, le tout séparé par le dernier "/" contenu dans <paramref name="noeudNouveau"/>.
        /// </summary>        
        /// <param name="noeudNouveau">Le nom complet du nouveau noeud à ajouter.</param> 
        public static void XML_AddElement(string noeudNouveau)
        {
            Helper.XML_AddElement(Helper.XML_Prefixe(noeudNouveau),
                                              Helper.XML_Suffixe(noeudNouveau));
        }

        /// <summary>
        /// Extrait tout le texte précédant le dernier caractère "/" situé à l'intérieur du texte donné en entrée.      
        /// </summary>        
        /// <param name="balise">Le texte dans lequel on cherche l'emplacement du dernier caractère "/".</param> 
        /// <returns>Tout le texte situé avant le dernier caractère "/" contenu dans <paramref name="balise"/>.</returns>
        public static string XML_Prefixe(string balise)
        {
            return balise.Substring(0, balise.LastIndexOf("/"));
        }

        /// <summary>
        /// Extrait tout le texte succédant le dernier caractère "/" situé à l'intérieur du texte donné en entrée.      
        /// </summary>        
        /// <param name="balise">Le text dans lequel on cherche l'emplacement du dernier caractère "/".</param> 
        /// <returns>Tout le texte situé après le dernier caractère "/" contenu dans <paramref name="balise"/>.</returns>
        public static string XML_Suffixe(string balise)
        {
            return balise.Substring(balise.LastIndexOf("/") + 1);
        }

        /// <summary>
        /// Ajoute à l'emplacement indiqué, un nouvel élément et sa valeur associée dans le fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap.
        /// </summary> 
        /// <param name="root">L'emplacement dans le fichier paramètre où on veut ajouter le nouvel élément.</param>
        /// <param name="elementName">Le nom de l'élément à ajouter.</param>
        /// <param name="elementVal">La valeur de l'élément à ajouter.</param>
        public static void XML_InsertElement(string root, string elementName, string elementVal)
        {
            XmlDocument paramsXML = Helper.XML_Load();
            XmlNode noeudRoot = paramsXML.SelectSingleNode(root);

            XmlDocument newDoc = new ();
            newDoc.LoadXml("<" + elementName + ">" + elementVal + "</" + elementName + ">");

            XmlNode noeud = paramsXML.ImportNode(newDoc.FirstChild, true);
            noeudRoot.AppendChild(noeud);
            paramsXML.Save(Helper.XML_NameFile());
        }

        /// <summary>
        /// Renvoie le premier élément indiqué l'intérieur du fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap.
        /// </summary>
        /// <param name="elementName">Le nom de l'élément à rechercher.</param>
        /// <returns>La valeur associée à l'élément <paramref name="element"/> s'il existe (vide en cas contraire).</returns>
        public static string XML_FirstElement(string element)
        {
            System.Xml.XmlDocument paramsEspaceCollaboratif = new ();

            StreamReader streamXML = new (Helper.XML_NameFile());
            paramsEspaceCollaboratif.Load(streamXML);
            System.Xml.XmlNodeList elemlist = paramsEspaceCollaboratif.DocumentElement.SelectNodes(element);
            streamXML.Close();

            if (elemlist.Count == 1)
            {
                return elemlist[0].InnerText;
            }

            return "";
        }

        /// <summary>
        /// Retourne tous les éléments contenant la balise element
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static List<string> XML_AllElement(string element)
        {
            System.Xml.XmlDocument paramsEspaceCollaboratif = new ();
            StreamReader streamXML = new (Helper.XML_NameFile());
            paramsEspaceCollaboratif.Load(streamXML);
            System.Xml.XmlNodeList elemlist = paramsEspaceCollaboratif.DocumentElement.SelectNodes(element);
            streamXML.Close();
            List<string> allElements = new ();
            foreach (XmlNode node in elemlist)
            {
                allElements.Add(node.InnerText);
            }
            return allElements;
        }

        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage de l'add-on, une liste d'éléments à un emplacement donné.
        /// </summary>
        /// <param name="listeElement">La liste contenant les éléments à stocker dans le fichier de paramétrage.</param>
        /// <param name="chemin">Le chemin d'accès au sein du fichier de paramétrage où il faut ajouter les éléments.</param>
        public static void XML_WriteElement(List<string> listeElement, string chemin)
        {
            if (chemin.Length == 0) { return; }
            string pathElement = Helper.XML_Prefixe(chemin);

            if (Helper.XML_HasElement(pathElement))
            {
                Helper.XML_ClearElements(pathElement);
            }
            else
            {
                Helper.XML_AddElement(pathElement);
            }

            foreach (string element in listeElement)
            {
                Helper.XML_InsertElement(pathElement, Helper.XML_Suffixe(chemin), element);
            }
        }        
       
        /// <summary>
        /// Renvoie la liste des thèmes préférés contenus à l'intérieur du fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap.
        /// </summary>
        /// <returns>La liste des noms de thèmes préférés contenus dans le fichier de paramétrage.</returns>
        public static List<string> Load_PreferredThemes()
        {
            return Helper.XML_AllElement(Helper.xml_Themes);
        }
       
        /// <summary>
        /// Sauvegarde les thèmes préférés dans le fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap.
        /// </summary>
        /// <param name="preferedThemes">La liste des thèmes préférés à sauvegarder dans le fichier de paramétrage.</param>
        public static void Save_PreferredThemes(List<string> preferedThemes)
        {
            Helper.XML_WriteElement(preferedThemes, Helper.xml_Themes);
        }
        
        /// <summary>
        /// Sauvegarde les thèmes préférés dans le fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap.
        /// </summary>
        /// <param name="preferedThemes">La liste des thèmes préférés à sauvegarder dans le fichier de paramétrage.</param>
        public static void Save_PreferredThemes(List<ArcGisProEspaceCollaboratif.Core.Theme> preferedThemes)
        {
            List<string> ListThemes = new ();

            foreach (ArcGisProEspaceCollaboratif.Core.Theme theme in preferedThemes)
            {
                ListThemes.Add(theme.Group.Name);
            }

            Helper.Save_PreferredThemes(ListThemes);
        }
        
        /// <summary>
        /// Lit le login par défaut à utiliser pour se connecter au service EspaceCollaboratif contenu dans le fichier XML de paramétrage.
        /// </summary>
        /// <returns>Le login à utiliser par défaut pour se connecter au service EspaceCollaboratif.</returns>
        public static string LoadLogin()
        {
            return Helper.XML_FirstElement(Helper.xml_Login);
        }
        
        /// <summary>
        ///  Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, le login à utiliser pour se connecter au service EspaceCollaboratif.
        /// </summary>
        /// <param name="login">Le login par défaut à sauvegarder dans le fichier de paramétrage.</param>
        public static void SaveLogin(string login)
        {
            if (!Helper.XML_HasElement(Helper.xml_Login))
            {
                Helper.XML_AddElement(Helper.xml_Login);
            }

            Helper.XML_SetElement(Helper.xml_Login, login);
        }

        /// <summary>
        /// Lit la valeur du tag "Import_pour_groupe" du fichier XML de paramétrage.
        /// </summary>
        /// <returns></returns>
        public static string LoadExtractionForGroup()
        {
            return Helper.XML_FirstElement(Helper.xml_Group);
        }
        
        /// <summary>
        /// Lit depuis le fichier de paramétrage XML EspaceCollaboratif, la date de début d'extraction des signalements postérieurs à celle-ci.
        /// </summary>
        /// <returns>La date de début d'extraction stockée dans le fichier de paramétrage.</returns>
        public static System.DateTime LoadStartDateExtraction()
        {
            string dateExtration = Helper.XML_FirstElement(Helper.xml_StartDateExtraction);

            try
            {
                if (dateExtration.Length != 0)
                {
                    return Convert.ToDateTime(dateExtration);
                }
                else
                {
                    return Convert.ToDateTime(Helper.dateDefault);
                }
            }
            catch
            {
                string message = string.Format("La date limite d'extraction contenue dans fichier XML de paramétrage n'est pas de forme valide.\n\nDate limite d'extraction = '{0}'", dateExtration);
                logger.Error(string.Format("Helper.LoadDateExtraction : {0}\n", message));
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                    message,
                    Constantes.ERROR
                );
                return Convert.ToDateTime(Helper.dateDefault);
            }
        }

        /// <summary>
        /// Lit depuis le fichier de paramétrage XML EspaceCollaboratif, la date de fin d'extraction des signalements antérieurs à celle-ci.
        /// </summary>
        /// <returns>La date de fin d'extraction stockée dans le fichier de paramétrage.</returns>
        public static System.DateTime LoadEndDateExtraction()
        {
            string dateExtration = Helper.XML_FirstElement(Helper.xml_EndDateExtraction);

            try
            {
                if (dateExtration.Length != 0)
                {
                    return Convert.ToDateTime(dateExtration);
                }
                else
                {
                    return DateTime.Now;
                }
            }
            catch
            {
                string message = string.Format("La date limite d'extraction contenue dans fichier XML de paramétrage n'est pas de forme valide.\n\nDate limite d'extraction = '{0}'", dateExtration);
                logger.Error(string.Format("Helper.LoadDateExtraction : {0}\n", message));
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                    message,
                    Constantes.ERROR
                );
                return Convert.ToDateTime(Helper.dateDefault);
            }
        }

        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, la date d'extraction pour l'importation des signalements.
        /// </summary>
        /// <param name="date">La date d'extraction à enregistrer dans le fichier de paramétrage.</param>
        public static void SaveStartDateExtraction(string date)
        {
            if (!Helper.XML_HasElement(Helper.xml_StartDateExtraction))
            {
                Helper.XML_AddElement(Helper.xml_StartDateExtraction);
            }

            Helper.XML_SetElement(Helper.xml_StartDateExtraction, date);
        }

        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, la date d'extraction pour l'importation des signalements.
        /// </summary>
        /// <param name="date">La date d'extraction à enregistrer dans le fichier de paramétrage.</param>
        public static void SaveEndDateExtraction(string date)
        {
            if (!Helper.XML_HasElement(Helper.xml_EndDateExtraction))
            {
                Helper.XML_AddElement(Helper.xml_EndDateExtraction);
            }

            Helper.XML_SetElement(Helper.xml_EndDateExtraction, date);
        }

        /// <summary>
        /// Obtient à partir du fichier XML de paramétrage, le nom du calque à utiliser pour le filtrage spatial de l'importation des signalements.
        /// </summary>
        /// <returns>Le nom du calque pour le filtrage spatiale stocké dans le fichier de paramétrage.</returns>
        public static string LoadNameLayerForSpatialFilter()
        {
            return Helper.XML_FirstElement(Helper.xml_Zone_extraction);
        }
        
        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, le nom du calque à utiliser pour le filtrage spatial lors l'importation des signalements.
        /// </summary>
        /// <param name="layer">Le nom du calque à enregistrer dans le fichier de paramétrage pour le filtrage spatiale.</param>
        public static void SaveNameLayerForSpatialFilter(string layer)
        {
            XML_SetElement(Helper.xml_Zone_extraction, layer);       
        }

        /// <summary>
        /// Obtient à partir du XML de paramétrage, l'adresse du service EspaceCollaboratif contenue dans le fichier XML de paramétrage.
        /// </summary>
        /// <returns>L'adresse d'accès au service EspaceCollaboratif stockée dans le fichier de paramétrage EspaceCollaboratif.</returns>
        public static string LoadUrlhost()
        {
            string Urlhost = Helper.XML_FirstElement(Helper.xml_UrlHost);

            if (Urlhost.Equals(""))
            {
                string message = "Impossible de trouver l'adresse du service de l'Espace collaboratif dans le fichier XML de paramétrage.";
                logger.Error(string.Format("Helper.LoadUrlhost : {0}\n", message));
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                    message,
                    Constantes.ERROR
                );
                return "";
            }

            return Urlhost;
        }
 
        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, l'adresse du service EspaceCollaboratif.
        /// </summary>
        /// <param name="UrlHost">L'adresse d'accès au service EspaceCollaboratif à enregistrer dans le fichier de paramétrage.</param>
        public static void SaveUrlhost(string UrlHost)
        {
            XML_SetElement(Helper.xml_UrlHost, UrlHost);
        }

        /// <summary>
        ///Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, l'option d'affichage des croquis EspaceCollaboratif.
        /// </summary>
        /// <param name="afficher">L'option d'affichage des croquis à stocker dans le fichier de paramétrage.</param>
        public static void Save_AfficherCroquis(bool afficher)
        {
            if (afficher)
            {
                XML_SetElement(Helper.xml_AfficherCroquis, "Oui");
            }
            else
            {
                XML_SetElement(Helper.xml_AfficherCroquis, "Non");
            }
        }

        public static string LoadProxy()
        {
            return Helper.XML_FirstElement(Helper.xml_Proxy);
        }

        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, le proxy dans le cas d'une connexion partenariale extérieure au service EspaceCollaboratif.
        /// </summary>
        /// <param name="proxy">L e nom du proxy à enregistrer dans le fichier de paramétrage.</param>
        public static void SaveProxy(string proxy)
        {
            if (!XML_HasElement(Helper.xml_Proxy))
            {
                XML_AddElement(Helper.xml_Proxy);
            }
            XML_SetElement(Helper.xml_Proxy, proxy);
        }

        /// <summary>
        /// Retourne le nom du groupe préféré de l'utilisateur enregistré dans le fichier XML de paramétrage.
        /// </summary>
        public static string Load_PreferredGroup()
        {
            return Helper.XML_FirstElement(Helper.xml_GroupePrefere);
        }

        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage (espaceco.xml), le nom du groupe préféré de l'utilisateur du service de l'Espace collaboratif.
        /// </summary>
        /// <param name="groupePrefere">Le nom du groupe préféré de l'utilisateur à enregistrer dans le fichier de paramétrage.</param>
        public static void Save_PreferredGroup(string groupePrefere)
        {
            if (!XML_HasElement(xml_GroupePrefere))
            {
                XML_AddElement(xml_GroupePrefere);
            }

            XML_SetElement(xml_GroupePrefere, groupePrefere);
        }

        /// <summary>
        /// Retourne le nom du groupe actif de l'utilisateur enregistré dans le fichier XML de paramétrage.
        /// </summary>
        public static string LoadActiveGroup()
        {
            return Helper.XML_FirstElement(Helper.xml_GroupeActif);
        }

        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, le nom du groupe actif de l'utilisateur du service EspaceCollaboratif.
        /// </summary>
        /// <param name="groupeActif">Le nom du groupe actif de l'utilisateur du service EspaceCollaboratif à enregistrer dans le fichier de paramétrage.</param>
        public static void SaveActiveGroup(string groupeActif)
        {
            if (!XML_HasElement(xml_GroupeActif))
            {
                XML_AddElement(xml_GroupeActif);
            }

            XML_SetElement(xml_GroupeActif, groupeActif);
        }

        /// <summary>
        /// Retourne le nom de la zone de travail de l'utilisateur enregistré dans le fichier XML de paramétrage.
        /// </summary>
        public static string LoadWorkZone()
        {
            return Helper.XML_FirstElement(Helper.xml_Zone_extraction);
        }

        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, le nom de la zone de travail de l'utilisateur.
        /// Cela peut être un shapefile
        /// </summary>
        /// <param name="workZone">Le nom de la zone de travail de l'utilisateur.</param>
        public static void SaveWorkZone(string workZone)
        {
            if (!XML_HasElement(xml_Zone_extraction))
            {
                XML_AddElement(xml_Zone_extraction);
            }

            XML_SetElement(xml_Zone_extraction, workZone);
        }

        public static List<Map> GetMaps()
        {
            List<Map> maps = new();
            IEnumerable<MapProjectItem> projectMapItems = Project.Current.GetItems<MapProjectItem>();
            if (projectMapItems == null) return null;
            var projectMaps = QueuedTask.Run(() =>
            {
                foreach (var item in projectMapItems)
                {
                    maps.Add(item.GetMap());
                }
            });
            return maps;
        }

        public static Task<Layer> AddLayer(string uri)
        {
            return QueuedTask.Run(() =>
            {
                Map map = MapView.Active.Map;
                if (map == null)
                {
                    string mess = "Pas de carte active, impossible d'ajouter une couche à celle-ci";
                    logger.Error(string.Format("Helper.AddLayer : {0}\n", mess));
                    throw new Exception(mess);
                }
                Layer layer = LayerFactory.Instance.CreateLayer(new Uri(uri), map);
                ArcGIS.Core.Geometry.SpatialReference layer_projection = layer.GetSpatialReference();
                string projection_name = layer_projection.Name;
                string message = "";
                if (projection_name == "Unknown")
                {
                    message = string.Format("Le système de coordonnées de référence (SCR) n'est pas assigné pour la couche [{0}]. Veuillez le renseigner avec [View/Geoprocessing/Define projection/Parameters]", layer.Name);
                }
                if (layer == null)
                {
                    message = string.Format("Layer {0} failed to load", uri);
                }
                if (!string.IsNullOrWhiteSpace(message))
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.WARNING);
                }          
                return layer;
            });
        }

        //Suppression des couches liées au signalement si elles existent
        public static void RemoveLayersInMap(List<string> layersName)
        {
            Map map = MapView.Active.Map;
            if (map == null)
            {
                string mess = "Pas de carte active, impossible de détruire les couches de celle-ci";
                logger.Error(string.Format("Helper.RemoveLayersInMap : {0}\n", mess));
                throw new Exception(mess);
            }
            IReadOnlyList<Layer> layers = map.GetLayersAsFlattenedList();
            foreach (Layer layer in layers)
            {
                foreach (var item in layersName)
                {
                    if (item != layer.Name) continue;
                    map.RemoveLayer(layer);
                    break;
                }
            }
        }
    }  
}
