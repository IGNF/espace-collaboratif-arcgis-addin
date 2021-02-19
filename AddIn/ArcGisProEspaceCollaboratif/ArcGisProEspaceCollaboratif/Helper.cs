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
using System.Windows.Forms;
using ArcGIS.Desktop.Core;
using ArcGIS.Core.CIM;

namespace ArcGisProEspaceCollaboratif
{
    /// <summary>
    /// Classe contenant des utilitaires pour le plugin
    /// </summary>
    public class Helper
    {
        public const String nom_Fichier_EspaceCollaboratif = "espaceco.gdb";
        public const String nom_Fichier_Parametres_EspaceCollaboratif = "espaceco.xml";

        public const String nom_Calque_Signalement = "Signalement";
        public const String nom_Calque_Croquis_Polygone = "Croquis_EC_Polygone";
        public const String nom_Calque_Croquis_Ligne = "Croquis_EC_Ligne";
        public const String nom_Calque_Croquis_Point = "Croquis_EC_Point";

        public const String nom_Champ_IdRemarque = "N°remarque";
        public const String nom_Champ_Auteur = "Auteur";
        public const String nom_Champ_Commune = "Commune";
        public const String nom_Champ_Departement = "Département";
        public const String nom_Champ_IDDepartement = "Département_ID";
        public const String nom_Champ_DateCreation = "Date_de_création";
        public const String nom_Champ_DateMAJ = "Date_MAJ";
        public const String nom_Champ_DateValidation = "Date_de_validation";
        public const String nom_Champ_Themes = "Thèmes";
        public const String nom_Champ_Statut = "Statut";
        public const String nom_Champ_Message = "Message";
        public const String nom_Champ_Reponse = "Réponses";
        public const String nom_Champ_Url = "URL";
        public const String nom_Champ_UrlPrive = "URL_privé";
        public const String nom_Champ_Document = "Document";
        public const String nom_Champ_Autorisation = "Autorisation";
        public const String nom_Champ_LienRemarque = "Lien_remarque";
        public const String nom_Champ_NomCroquis = "Nom";
        public const String nom_Champ_Attributs = "Attributs_croquis";
        public const String nom_Champ_LienBDuni = "Lien_object_BDUni";

        public const String xml_UrlHost = "/Paramètres_connexion_à_EspaceCollaboratif/Serveur/URLHost";
        public const String xml_Login = "/Paramètres_connexion_à_EspaceCollaboratif/Serveur/Login";
        public const String xml_DateExtraction = "/Paramètres_connexion_à_EspaceCollaboratif/Map/Date_extraction";
        public const String xml_Pagination = "/Paramètres_connexion_à_EspaceCollaboratif/Map/Pagination";
        public const String xml_Themes = "/Paramètres_connexion_à_EspaceCollaboratif/Map/Thèmes_préférés/Thème";
        public const String xml_Zone_extraction = "/Paramètres_connexion_à_EspaceCollaboratif/Map/Zone_extraction";
        public const String xml_AfficherCroquis = "/Paramètres_connexion_à_EspaceCollaboratif/Map/Afficher_Croquis";
        public const String xml_AttributsCroquis = "/Paramètres_connexion_à_EspaceCollaboratif/Map/Attributs_croquis";
        public const String xml_BaliseNomCalque = "Calque_Nom";
        public const String xml_BaliseChampCalque = "Calque_Champ";
        public const String xml_Group = "/Paramètres_connexion_à_EspaceCollaboratif/Map/Import_pour_groupe";
        public const String xml_Proxy = "/Paramètres_connexion_à_EspaceCollaboratif/Serveur/Proxy";
        public const String xml_CleGeoPortail = "/Paramètres_connexion_à_EspaceCollaboratif/Serveur/cle_geoportail";
        public const String xml_GroupeActif = "/Paramètres_connexion_à_EspaceCollaboratif/Serveur/groupe_actif";
        public const String xml_GroupePrefere = "/Paramètres_connexion_à_EspaceCollaboratif/Serveur/groupe_prefere";

        public const String dateDefaut = "01/01/1900";
        public const int longueurMaxChamp = 5000;

        public static String EspaceCollaboratifAssemblyDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";

        private readonly ArcGisProEspaceCollaboratif.Core.Logger riplogger = ArcGisProEspaceCollaboratif.Core.Logger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(Helper));

        // Dictionnaire des attributs de la couche signalements (avec types et contraintes)
        public static Dictionary<string, KeyValuePair<string, string>> reportAttributes = new Dictionary<string, KeyValuePair<string, string>>
        {
            { nom_Champ_IdRemarque,     new KeyValuePair<string,string> ("LONG","")      },
            { nom_Champ_Auteur,         new KeyValuePair<string,string> ("TEXT", "50")   },
            { nom_Champ_Commune,        new KeyValuePair<string,string> ("TEXT", "")     },
            { nom_Champ_Departement,    new KeyValuePair<string,string> ("TEXT", "23")   },
            { nom_Champ_IDDepartement,  new KeyValuePair<string,string> ("TEXT", "3")    },
            { nom_Champ_DateCreation,   new KeyValuePair<string,string> ("DATE", "")     },
            { nom_Champ_DateMAJ,        new KeyValuePair<string,string> ("DATE", "")     },
            { nom_Champ_DateValidation, new KeyValuePair<string,string> ("DATE", "")     },
            { nom_Champ_Themes,         new KeyValuePair<string,string> ("TEXT", "")     },
            { nom_Champ_Statut,         new KeyValuePair<string,string> ("LONG", "")     },
            { nom_Champ_Message,        new KeyValuePair<string,string> ("TEXT", Helper.longueurMaxChamp.ToString()) },
            { nom_Champ_Reponse,        new KeyValuePair<string,string> ("TEXT", Helper.longueurMaxChamp.ToString()) },
            { nom_Champ_Url,            new KeyValuePair<string,string> ("TEXT", "")     },
            { nom_Champ_UrlPrive,       new KeyValuePair<string,string> ("TEXT", "")     },
            { nom_Champ_Document,       new KeyValuePair<string,string> ("TEXT", "")     },
            { nom_Champ_Autorisation,   new KeyValuePair<string,string> ("TEXT", "")     }

        };

        // Dictionnaire des attributs des couches croquis (avec types et contraintes)
        public static Dictionary<string, KeyValuePair<string, string>> sketchAttributes = new Dictionary<string, KeyValuePair<string, string>>
        {
            { nom_Champ_LienRemarque, new KeyValuePair<string,string> ("LONG", "") },
            { nom_Champ_NomCroquis, new KeyValuePair<string, string>("TEXT", "") },
            { nom_Champ_Attributs, new KeyValuePair<string, string>("TEXT", Helper.longueurMaxChamp.ToString()) },
            { nom_Champ_LienBDuni, new KeyValuePair<string, string>("TEXT", "") }
        };

        /// <summary>
        /// Teste si la remarque donnée est contenue à l'intérieur d'une des géométrie fournie en entrée.  
        /// </summary>    
        /// <param name="remarqueTest">La remarque EspaceCollaboratif à tester.</param>
        /// <param name="geometrys">La liste des géométries à tester pour le filtrage spatial.</param>
        /// <returns>True si la remarque à tester est incluse à l'intérieur d'une des géométries fournies en entrée.</returns>
        /*        public static bool IsInGeometry(ArcGisProEspaceCollaboratif.Core.Signalement remarqueTest, List<Geometry> geometrys)
                {
                    foreach (Geometry shape in geometrys)
                    {
                        if (!shape.IsEmpty)
                        {
                            Polygon polygon = shape as Polygon;
                            // Création de l'opérateur de filtrage
                            IRelationalOperator2 filtrageSpatial = (IRelationalOperator2)polygon;

                            if (filtrageSpatial.Contains(EspaceCollaboratifHelper.TransformPoint(remarqueTest.Position) as Point))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
        */
        /// <summary>
        /// Définit toutes les caractéristiques pour le futur calque dédié à contenir les signalements pour l'Espace collaboratif.
        /// </summary>
        /// <param name="featureWorkspace">L'espace de travail de la carte en cours sur laquelle on veut créer le calque supplémentaire.</param>
        /// <param name="spatialReferenceCalque">Le système de référence spatial à attribuer au calque nouvellement crée.</param>
        /// <returns>FeatureLayer pouvant être ajouté dans la carte en cours.</returns>
        public static async Task LoadOrCreateCollaborativeSpaceLayer(string fcName, string fcType, Dictionary<string, KeyValuePair<string, string>> fcAttributesDict, int layerPosition, string symbolName = "")
        {
            try
            {
                await QueuedTask.Run(() =>
                {
                    Contexte contexte = Contexte.Instance;

                    // On vérifie si la feature class existe déjà dans la geodatabase du projet
                    string gdbPath = CoreModule.CurrentProject.DefaultGeodatabasePath;
                    Uri gdbUri = new Uri(uriString: gdbPath);
                    Geodatabase gdbCollaborativeSpace = new Geodatabase(new FileGeodatabaseConnectionPath(gdbUri));
                    bool bFcExists = ExistsFcInGdb(fcName, gdbCollaborativeSpace);
        
                    // Si la feature class existe et est déjà chargée dans la carte, on sort
                    if (bFcExists && Contexte.Instance.IsLayerInMap(fcName))
                    {
                        return;
                    }

                    // Si la feature class n'existe pas dans la geodatabase du projet, on la crée
                    FeatureLayer collabSpaceLayer;
                    if (!bFcExists)
                    {
                        List<object> arguments = new List<object>
                        {
                            gdbPath, // Chemin de la geodatabase
                            fcName, // Nom de la feature class à créer                   
                            fcType, // type de géométrie                    
                            "", // no template                    
                            "DISABLED", // no z values                    
                            "DISABLED" // no m values
                        };

                        // Ajout de la référence spatiale
                        arguments.Add(contexte.spatialReference);

                        // Création de la feature class
                        var result = Geoprocessing.ExecuteToolAsync("CreateFeatureclass_management", Geoprocessing.MakeValueArray(arguments.ToArray()));

                        // Ajout des champs à la nouvelle feature class
                        string fcPath = CoreModule.CurrentProject.DefaultGeodatabasePath + "\\" + fcName;
                        AddFieldsToFc(fcPath, fcAttributesDict);

                        // La nouvelle feature class est chargée automatiquement dans la carte.
                        // On récupère le FeatureLayer correspondant.
                        collabSpaceLayer = contexte.GetLayerByName(fcName);
                    }

                    // Si la feature class existe déjà, on l'ouvre et on l'ajoute comme couche (FeatureLayer) à la carte
                    else
                    {
                        // Ouverture de la feature class
                        FeatureClass collabSpaceFc = gdbCollaborativeSpace.OpenDataset<FeatureClass>(fcName);

                        // Ajout en tant que FeatureLayer à la carte
                        collabSpaceLayer = LayerFactory.Instance.CreateFeatureLayer(
                            collabSpaceFc,
                            contexte.mapActiveView.Map,
                            layerPosition,
                            fcName
                        );
                    }

                    // Application d'une symbologie - Ne traite actuellement que les signalements
                    if (symbolName != "")
                    {
                        SetLayerStyle(collabSpaceLayer, symbolName);
                    }
            
                });
            }
            
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
        }

        /// <summary>
        /// Ajoute les champs indiqués dans le dictionnaire à la feature class en entrée.
        /// </summary>
        /// <param name="fcPath">Chemin de la feature class dans laquelle les champs doivent être ajoutés.</param>
        /// <param name="fcAttributesDict">Dictionnaire contenant le nom, le type et la longueur éventuelle des champs à traiter.</param>
        /// <returns></returns>
        public static void AddFieldsToFc(string fcPath, Dictionary<string, KeyValuePair<string, string>> fcAttributesDict)
        {
            foreach (KeyValuePair<string, KeyValuePair<string, string>> kvp in fcAttributesDict)
            {
                string fieldName = kvp.Key;
                string fieldType = kvp.Value.Key;
                string fieldLength = kvp.Value.Value;

                Geoprocessing.ExecuteToolAsync("AddField_management", Geoprocessing.MakeValueArray(fcPath, fieldName, fieldType, fieldLength));
            }
        }

        /// <summary>
        /// Applique un symbole à une couche ponctuelle.
        /// </summary>
        /// <param name="fcLayer">FeatureLayer à laquelle le symbole doit être appliqué.</param>
        /// <param name="symbolName">Nom du symbole à appliquer.</param>
        /// <returns></returns>
        public static void SetLayerStyle(FeatureLayer fcLayer, string symbolName)
        {
            // Get all styles in the project
            var styles = Project.Current.GetItems<StyleProjectItem>();

            // Get a specific style in the project
            StyleProjectItem style = styles.First(s => s.Name == "ArcGIS 2D");

            // Get the Push Pin 1 symbol
            var pt_ssi = style.SearchSymbols(StyleItemType.PointSymbol, symbolName).FirstOrDefault();

            // Create a new renderer definition and reference the symbol
            SimpleRendererDefinition srDef = new SimpleRendererDefinition
            {
                SymbolTemplate = pt_ssi.Symbol.MakeSymbolReference()
            };

            // Create the renderer and apply the definition
            CIMSimpleRenderer ssRenderer = (CIMSimpleRenderer)fcLayer.CreateRenderer(srDef);

            // Update the feature layer renderer
            fcLayer.SetRenderer(ssRenderer);
        }

        /// <summary>
        /// Crée un nouveau champ aux caractéristiques souhaitées.
        /// A utiliser pour lors de la créations des calques dédiés à contenir les objects EspaceCollaboratif.
        /// </summary>
        /// <param name="nomChamp">Le nom à attribuer au nouveau champ.</param>
        /// <param name="typeChamp">Le type de donnée contenue dans le nouveau champ.</param>
        /// <param name="tailleLimite">La longueur maximale du nouveau champ.</param>
        /// <returns>Field à utliser lors de la définition d'un nouvel calque.</returns>
        /*        public static Field DefinirChamp(string nomChamp, FieldType typeChamp, int tailleLimite = 0)
                {
                    IField fieldSupp = new IField();
                    IFieldEdit fieldEdit = (IFieldEdit)fieldSupp;
                    fieldEdit.Name_2 = nomChamp;
                    fieldEdit.Type_2 = typeChamp;

                    if (tailleLimite != 0)
                    {
                        fieldEdit.Length_2 = tailleLimite;
                    }

                    return fieldSupp;
                }
        */

        /// <summary>
        /// Vérifie si une feature class existe dans une geodatabase.
        /// </summary>
        /// <param name="fcName"> Nom de la feature class à chercher.
        /// <param name="gdbPath"> Chemin de la geodatabase.
        /// <returns>Vrai si la feature class existe dans la geodatabase, Faux sinon.</returns>
        public static bool ExistsFcInGdb(string fcName, Geodatabase gdbCollaborativeSpace)
        {
            IReadOnlyList<FeatureClassDefinition> fcdList = gdbCollaborativeSpace.GetDefinitions<FeatureClassDefinition>();

            bool bExists = false;
            foreach (FeatureClassDefinition fcd in fcdList)
            {
                if (fcd.GetName() == fcName)
                {
                    bExists = true;
                    break;
                }
            }
            return bExists;
        }

        /// <summary>
        /// Limite la longueur d'une string pour ne pas dépasser la taille maximale que ne peut contenir les attributs d'un calque.
        /// La taille maximale est défini par la constante EspaceCollaboratifHelper.longueurMaxChamp.
        /// </summary>
        /// <param name="champ"> L'object string dont on doit éventuellement limiter sa longueur.
        /// <returns>L'object champ raccourcie à ses EspaceCollaboratifHelper.longueurMaxChamp premiers caractères si il dépasse cette taille limite</returns>
        public static string Limite(string champ)
        {
            if (champ.Length > Helper.longueurMaxChamp)
            {
                return champ.Substring(0, Helper.longueurMaxChamp);
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
            List<MapPoint> pointCollection = new List<MapPoint>();

            foreach (Point sketchVertex in currSketch.Points)
            {
                pointCollection.Add(Helper.TransformPoint(sketchVertex));
            }

            return pointCollection;
        }
          
        /// <summary>
        /// Remplit un attribut d'un objet Espace collaboratif sur la carte en cours.
        /// </summary>
        /// <param name="featureClass">La FeatureClass du calque contenant l'objet Espace collaboratif sur lequel on veut compléter un attribut.</param>
        /// <param name="feature">La Feature de l'objet Espace collaboratif sur lequel on veut compléter un attribut.</param>
        /// <param name="fieldName">Le nom de l'attribut dans lequel on veut écrire.</param>
        /// <param name="fieldValue">La valeur à écrire dans l'attribut de l'objet Espace collaboratif. </param>
        public static void UpdateField(
            FeatureClass featureClass,
            Feature feature,
            string fieldName,
            object fieldValue)
        {
            if (fieldValue == null || fieldName.Equals(""))
            {
                return;
            }

            int indexField = feature.FindField(fieldName);
            if (indexField == -1)
            {
                return;
            }

            // Mise à jour de la valeur du champ de l'objet
            //feature.set_Value(iFiedName, fieldValue.ToString());
        }

        public static RowBuffer UpdateReportFields(Dictionary<string,string> dico)
        {
            RowBuffer rowBuffer = null;
            foreach (KeyValuePair<string, string> kvp in dico)
            {
                rowBuffer[kvp.Key] = kvp.Value;
            }
            return rowBuffer;
        }

        /// <summary>
        /// Renvoie la longueur du plus long message contenu dans une liste de remarque EspaceCollaboratif.
        /// </summary>
        /// <param name="signalements">La liste des signalements de l'Espace collaboratif dans laquelle on veut chercher celle qui a le message le plus long.</param>
        /// <returns>La longueur du plus long message.</returns>
        /*        public static int Max_Length_Message(List<ArcGisProEspaceCollaboratif.Core.Signalement> signalements)
                {
                    int maxLength = 0;

                    foreach (ArcGisProEspaceCollaboratif.Core.Signalement signalement in signalements)
                    {
                        if (signalement.Commentaire.Length > maxLength)
                        {
                            maxLength = signalement.Commentaire.Length;
                        }
                    }

                    return maxLength;
                }
        */
        /// <summary>
        /// Renvoie la longueur de la plus longue des concaténations de réponses des remarques EspaceCollaboratif, en tenant compte du formatge HTML.
        /// </summary>
        /// <param name="remarques">La liste des remarques EspaceCollaboratif dans laquelle on veut chercher celle qui a la plus longue concaténation de réponse.</param>
        /// <returns>La longueur de la plus longue concaténation de réponse.</returns>
        /*        public static int Max_Length_concatenateReponseHTML(List<ArcGisProEspaceCollaboratif.Core.Signalement> remarques)
                {
                    int maxLength = 0;

                    foreach (ArcGisProEspaceCollaboratif.Core.Signalement remarque in remarques)
                    {
                        if (remarque.ConcatenateReponse().Length > maxLength)
                        {
                            maxLength = remarque.ConcatenateReponseHTML().Length;
                        }
                    }

                    return maxLength;
               }
        */
        /// <summary>
        /// Renvoie la longueur de la plus longue des concaténations de réponses des remarques EspaceCollaboratif, sans tenir compte du formatge HTML.
        /// </summary>
        /// <param name="remarques">La liste des remarques EspaceCollaboratif dans laquelle on veut chercher celle qui a la plus longue concaténation de réponse.</param>
        /// <returns>La longueur de la plus longue concaténation de réponse.</returns>
        /*       public static int Max_Length_concatenateReponse(List<ArcGisProEspaceCollaboratif.Core.Signalement> remarques)
               {
                   int maxLength = 0;

                   foreach (ArcGisProEspaceCollaboratif.Core.Signalement remarque in remarques)
                   {
                       if (remarque.ConcatenateReponse().Length > maxLength)
                       {
                           maxLength = remarque.ConcatenateReponse().Length;
                       }
                   }

                   return maxLength;
               }
       */
        /*
                /// <summary>
                /// Teste si la géometrie d'une Feature est utilisable ou non pour le filtrage spatial lors de l'importation des remarques EspaceCollaboratif.
                /// </summary>
                /// <param name="featureTest">La Feature dont veut tester sa géométrie.</param>
                /// <returns>True si <paramref name="featureTest"/> est de type Polygon, Envelope ou est une ligne fermée.</returns>
                public static bool TestGeometrieFiltrageSpatial(Feature featureTest)
                {
                    Geometry contourFiltrageSpatial = featureTest.GetShape();
                    ICurve contour = featureTest as Curve;

                    if (contourFiltrageSpatial.GeometryType == GeometryType.Polygon
                          || contourFiltrageSpatial.GeometryType == GeometryType.Envelope
                           )
                    {
                        return true;
                    }
                    else
                    {

                        if (contourFiltrageSpatial.GeometryType == GeometryType.Polygon
                          || contourFiltrageSpatial.GeometryType == esriGeometryType.esriGeometryCircularArc
                          || contourFiltrageSpatial.GeometryType == esriGeometryType.esriGeometryEllipticArc
                          || contourFiltrageSpatial.GeometryType == esriGeometryType.esriGeometryRing
                           )
                        {
                            if (contour.IsClosed)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
        */
        /// <summary>
        /// Convertit un point ArcGisProEspaceCollaboratif.Core.Point en son équivalent ArcGIS.Core.Geometry.MapPoint. 
        /// </summary>
        /// <param name="pointin">Le Point ArcGisProEspaceCollaboratif.Core qu'on veut convertir en MapPoint.</param>
        /// <returns>Le point converti.</returns>
        public static ArcGIS.Core.Geometry.MapPoint TransformPoint(ArcGisProEspaceCollaboratif.Core.Point pointin)
        {
            MapPoint point = null;
            ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                using (MapPointBuilder mapPointBuilder = new MapPointBuilder())
                {
                    mapPointBuilder.SetValues(pointin.Longitude, pointin.Latitude);
                    point = mapPointBuilder.ToGeometry();
                }
            });

            if (point.IsEmpty)
            {
                return null;
            }

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
            return new Point(pointIn.X, pointIn.Y);
        }
        
        /// <summary>
        /// Calcule le barycentre d'une liste de point.
        /// </summary>
        /// <param name="points">La liste des points ArcGIS dont on veut déterminer leur barycentre.</param>
        /// <returns>Le Point ArcGIS positionné sur le barycentre calculé.</returns>
        public static MapPoint Barycentre(List<MapPoint> points)
        {
            if (points.Count == 0) { return null; }
            if (points.Count == 1) { return points.First(); }

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
                using (MapPointBuilder mapPointBuilder = new MapPointBuilder())
                {
                    mapPointBuilder.SetValues(barycentreX, barycentreY);
                    pointResult = mapPointBuilder.ToGeometry();
                }
            });

            if (pointResult.IsEmpty)
            {
                return null;
            }

            return pointResult;
        }
        
        /// <summary>
        /// Calcule le barycentre d'une liste de point.
        /// </summary>
        /// <param name="points">La liste des points Point EspaceCollaboratif dont on veut déterminer leur barycentre.</param>
        /// <returns>Le Point EspaceCollaboratif positionné sur le barycentre calculé.</returns>
        public static ArcGisProEspaceCollaboratif.Core.Point Barycentre(List<ArcGisProEspaceCollaboratif.Core.Point> points)
        {
            if (points.Count == 0) { return null; }
            if (points.Count == 1) { return points.First(); }

            double barycentreX = 0;
            double barycentreY = 0;

            foreach (ArcGisProEspaceCollaboratif.Core.Point pointTemp in points)
            {
                barycentreX += pointTemp.Longitude;
                barycentreY += pointTemp.Latitude;
            }

            barycentreX /= points.Count;
            barycentreY /= points.Count;

            return new ArcGisProEspaceCollaboratif.Core.Point(barycentreX, barycentreY);
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
        /// Calcule la distance entre deux points.
        /// </summary>
        /// <param name="point1">Le premier point d'extrémité.</param>
        /// <param name="point2">Le second point d'extrémité.</param>
        /// <returns>La distance entre les points <paramref name="point1"/> et <paramref name="point2"/>.</returns>
        public static double Distance(ArcGisProEspaceCollaboratif.Core.Point point1, ArcGisProEspaceCollaboratif.Core.Point point2)
        {
            return Math.Sqrt(Math.Pow(point2.Longitude - point1.Longitude, 2) + Math.Pow(point2.Latitude - point1.Latitude, 2));
        }
       

        /// <summary>
        /// Convertit une polyligne (object IPolyline) en son équivalent polygone (object IPolygon), délimité par le contour formé par la polyligne.
        /// </summary>
        /// <param name="polyligneEntree">La polyligne à convertir en polygone</param>
        /// <returns>Le Polygon délimitée par <paramref name="polyligneEntree"/>.</returns>
        /*       public static Polygon PolylineToPolygon(Polyline polyligneEntree)
               {
                   Polygon polygonSortie = new Polygon() as Polygon;

                   IPointCollection vertexPolyligne = polyligneEntree as IPointCollection;
                   IPointCollection vertexPolygon = polygonSortie as IPointCollection;

                   for (int index = 0; index < vertexPolyligne.PointCount; index++)
                   {
                       vertexPolygon.AddPoint(vertexPolyligne.Point[index]);
                   }

                   if (!polyligneEntree.IsClosed)
                   {
                       vertexPolygon.AddPoint(vertexPolyligne.Point[0]);
                   }

                   return polygonSortie;
               }
       */

        /// <summary>
        /// Convertit un polygone (object IPolygon) en son équivalent surface (object IArea), délimité par le contour du polygone.
        /// </summary>
        /// <param name="polygonEntree">Le polygone à convertir en surface.</param>
        /// <returns>L'Area délimitée par <paramref name="polygonEntree"/>.</returns>
        /*     public static IArea PolygonToArea(Polygon polygonEntree)
               {
                   IArea aire = polygonEntree as IArea;
                   return aire;
               }
        */


        /// <summary>
        /// Convertit une polyligne (object IPolyline) en son équivalent surface (object IArea).
        /// </summary>
        /// <param name="polyligneEntree">La polyligne à convertir en surface.</param>
        /// <returns>L'Area délimitée par <paramref name="polygonEntree"/>.</returns>
        /*public static IArea PolylineToArea(Polyline polyligneEntree)
        {
            return EspaceCollaboratifHelper.PolygonToArea(EspaceCollaboratifHelper.PolylineToPolygon(polyligneEntree));
        }*/


        /// <summary>
        /// Convertit un chemin (object IPath) en son équivalent polyligne (object IPolyline).
        /// </summary>
        /// <param name="pathEntree">Le chemin à convertir en polyligne.</param>
        /// <returns>La Polyline issue depuis <paramref name="pathEntree"/>.</returns>
        /*       public static Polyline PathToPolyline(IPath pathEntree)
               {
                   Polyline polyligne = new Polyline() as Polyline;

                   IPointCollection vertexPolyligne = polyligne as IPointCollection;
                   IPointCollection vertexPath = pathEntree as IPointCollection;

                   for (int i = 0; i < vertexPath.PointCount; i++)
                   {
                       vertexPolyligne.AddPoint(vertexPath.Point[i]);
                   }

                   return polyligne;
               }
       */

        /// <summary>
        /// Calcule le centroïde d'un point.
        /// Le centroïde d'un point est trivialement le point lui-même.
        /// </summary>
        /// <param name="pointIn">Le point dont il faut calculer son centroïde.</param>
        /// <returns>Le centroïde calculé depuis <paramref name="pointIn"/>.</returns>
        public static MapPoint Centroid(MapPoint pointIn)
        {
            if (pointIn.IsEmpty)
            {
                return null;
            }
            return pointIn;
        }
       
        /// <summary>
        /// Calcule le centroïde d'un polygone.
        /// </summary>
        /// <param name="polygonEntree">Le polygone dont il faut calculer son centroïde.</param>
        /// <returns>Le centroïde calculé depuis <paramref name="polygonEntree"/>.</returns>
        /*       public static MapPoint Centroid(Polygon polygonEntree)
               {

                   return null;

                   /*IEnumerator<MapPoint> enumPts = polygonEntree.Points.GetEnumerator();

                   ICollection<Segment> collection = new List<Segment>();
                   polygonEntree.GetAllSegments(ref collection);
                   Envelope envelope = EnvelopeBuilder.();
                   Coordinate2D envelopeCenter = envelope.CenterCoordinate;
                   return envelopeCenter.ToMapPoint();

                   polygonEntree.GetAllSegments
                   if (polygonEntree.IsEmpty)
                   {
                       return null;
                   }
                   return EspaceCollaboratifHelper.PolygonToArea(polygonEntree).Centroid;
                }

*/
        /// <summary>
        /// Calcule le centroïde d'une polyligne.
        /// Si la polyligne est fermée, le centroïde calculé correspond au centroïde de la surface délimitée par cette polyligne.
        /// Sinon il s'agit du milieu de la polyligne.
        /// </summary>
        /// <param name="polylineEntree">La polyligne dont il faut calculer son centroïde.</param>
        /// <returns>Le centroïde calculé depuis <paramref name="polylineEntree"/>.</returns>
/*        public static MapPoint Centroid(Polyline polylineEntree)
        {
            if (polylineEntree.IsEmpty) { return null; }
            if (polylineEntree.IsClosed)
            {   // Si la polyligne est fermée, on prend le centröide de la surface délimitée par la polyligne.
                return EspaceCollaboratifHelper.PolylineToArea(polylineEntree).Centroid;
            }
            else
            {   // Si la polyligne est ouverte, on prend le milieu de l'abscisse curviligne de la polyligne.
                return EspaceCollaboratifHelper.Milieu(polylineEntree);
            }
        }
*/
        /// <summary>
        /// Calcule le centroïde d'arc elliptique.
        /// Si l'arc est fermé, le centroïde calculé correspond au centroïde de la surface délimitée par cet arc.
        /// Sinon il s'agit du milieu de l'arc.
        /// </summary>
        /// <param name="arcEntree">L'arc elliptique dont il faut calculer son centroïde.</param>
        /// <returns>Le centroïde calculé depuis <paramref name="arcEntree"/>.</returns>        
        /*public static MapPoint Centroid(EllipticArcSegment arcEntree)
        {
            if (arcEntree.IsEmpty) { return null; }
            if (arcEntree.IsClosed)
            {
                return arcEntree.CenterPoint;
            }
            else
            {
                return EspaceCollaboratifHelper.Milieu(arcEntree);
            }
        }*/


        /// <summary>
        /// Calcule le centroïde d'arc circulaire.
        /// Si l'arc est fermé, le centroïde calculé correspond au centroïde de la surface délimitée par cet arc.
        /// Sinon il s'agit du milieu de l'arc.
        /// </summary>
        /// <param name="arcEntree">L'arc circulaire dont il faut calculer son centroïde.</param>
        /// <returns>Le centroïde calculé depuis <paramref name="arcEntree"/>.</returns>        
        /*public static MapPoint Centroid(ICircularArc arcEntree)
        {
            if (arcEntree.IsEmpty) { return null; }
            IEllipticArc arc = arcEntree as IEllipticArc;
            return Centroid(arc);
        }*/



        /// <summary>
        /// Calcule le centroïde d'une ligne.       
        /// Il s'agit du milieu de cette ligne.
        /// </summary>
        /// <param name="ligneEntree">La ligne dont il faut calculer son centroïde.</param>
        /// <returns>Le centroïde calculé depuis <paramref name="ligneEntree"/>.</returns>   
  /*      public static MapPoint Centroid(Segment ligneEntree)
        {
            if (ligneEntree.Length == 0)
            {
                return null;
            }

            MapPoint milieu = new MapPointBuilder();
            ligneEntree.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, milieu);
            return milieu;
        }
*/
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

            //TODO return EspaceCollaboratifHelper.Centroid(EspaceCollaboratifHelper.CroquisToPolyline(sketchIn));
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

                    List<MapPoint> listCentroid = new List<MapPoint>();
                    foreach (ArcGisProEspaceCollaboratif.Core.Sketch croquis in listCroquisEntree)
                    {
                        listCentroid.Add(Helper.Centroid(croquis));
                    }

                    return Helper.Barycentre(listCentroid);
            }
        }

        /// <summary>
        /// Calcule le milieu d'un segment droit (object LineSegment).
        /// Les coordonnées du milieu d'un segment sont les demi-sommes de chacune des coordonnées des extrémités du segment
        /// </summary>
        /// <param name="lineSegment">Le segment dont il faut calculer le milieu</param>
        /// <returns>Le milieu de <paramref name="lineSegment"/>.</returns>  
        /*public static MapPoint Milieu(Segment segment)
        {
            MapPoint startPoint = segment.StartPoint;
            MapPoint endPoint = segment.EndPoint;
            double XcenterPoint = (startPoint.X + endPoint.X) / 2;
            double YcenterPoint = (startPoint.X + endPoint.X) / 2;
            return MapPointBuilder.CreateMapPoint(XcenterPoint, YcenterPoint);
        }*/

        /// <summary>
        /// Calcule le milieu d'une polyligne (object IPolyline).
        /// Il s'agit du point situé sur la polyligne ayant comme abscisse curviligne la moitié de la longueur de la polyligne.
        /// </summary>
        /// <param name="polylineEntree">La polyligne dont il faut calculer son milieu</param>
        /// <returns>Le milieu de <paramref name="polylineEntree"/>.</returns>  
        /*public static MapPoint Milieu(Polyline polylineEntree)
        {
            MapPoint milieu = new MapPoint();
            polylineEntree.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, milieu);
            return milieu;
        }*/

        /// <summary>
        /// Calcule le milieu d'un arc elliptique (object EllipticArcSegment).
        /// Il s'agit du point situé sur l'arc elliptique ayant comme abscisse curviligne la moitié de la longueur de l'arc.
        /// </summary>
        /// <param name="ellipticArcEntree">L'arc elliptique dont il faut calculer son milieu</param>
        /// <returns>Le milieu de <paramref name="arcEntree"/>.</returns>  
       /* public static MapPoint Milieu(EllipticArcSegment ellipticArcEntree)
        {
            Coordinate2D centerCoordinate = ellipticArcEntree.CenterPoint;
            return centerCoordinate.ToMapPoint();
        }*/

        /// <summary>
        /// Calcule le milieu d'un arc circulaire (object ICircularArc).
        /// Il s'agit du point situé sur l'arc circulaire ayant comme abscisse curviligne la moitié de la longueur de l'arc.
        /// </summary>
        /// <param name="arcEntree">L'arc circulaire dont il faut calculer son milieu</param>
        /// <returns>Le milieu de <paramref name="arcEntree"/>.</returns>  
        /*public static MapPoint Milieu(ICircularArc arcEntree)
        {
            IEllipticArc arc = arcEntree as IEllipticArc;
            return EspaceCollaboratifHelper.Milieu(arc);
        }*/

        /// <summary>
        /// Calcule le milieu d'une courbe (object EllipticArcSegment).
        /// Il s'agit du point situé sur la courbe ayant comme abscisse curviligne la moitié de la longueur de la courbe.
        /// </summary>
        /// <param name="curveEntree">L'arc circulaire dont il faut calculer son milieu</param>
        /// <returns>Le milieu de <paramref name="curveEntree"/>.</returns>  
        /*public static MapPoint Milieu(ICurve curveEntree)
        {
            IPoint milieu = new Point();
            curveEntree.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, milieu);
            return milieu;
        }*/

        /// <summary>
        /// Calcule le milieu d'un chemin(object IPath).
        /// Il s'agit du point situé sur le chemin ayant comme abscisse curviligne la moitié de la longueur du chemin.
        /// </summary>
        /// <param name="pathEntree">Le chemin dont il faut calculer son milieu</param>
        /// <returns>Le milieu de <paramref name="pathEntree"/>.</returns>
        /*public static MapPoint Milieu(IPath pathEntree)
        {
            Point milieu = new Point();
            pathEntree.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, milieu);
            return milieu;
        }*/


        /// <summary>
        /// Convertit un croquis EspaceCollaboratif en son équivalent polyligne (object IPolyline). 
        /// </summary>
        /// <param name="croquisEntree">Le croquis dont on veut convertir en polyligne.</param>
        /// <returns>La polyligne générée à partir de <paramref name="croquisEntree"/>.</returns>
 /*       public static Polyline CroquisToPolyline(ArcGisProEspaceCollaboratif.Core.Croquis croquisEntree)
        {
            if (croquisEntree.Points.Count == 0) { return null; }

            Polyline polyligne = new Polyline() as Polyline;
            IPointCollection vertexPolyligne = polyligne as IPointCollection;

            foreach (ArcGisProEspaceCollaboratif.Core.Point vertex in croquisEntree.Points)
            {
                vertexPolyligne.AddPoint(EspaceCollaboratifHelper.TransformPoint(vertex));
            }

            return polyligne;
        }
*/

        /// <summary>
        /// Créer un croquis EspaceCollaboratif à partir d'une collection de points composant sa future géométrie.
        /// </summary>
        /// <param name="pointCollectionEntree">La collection de points du croquis à générer.</param>
        /// <param name="typeCroquis">Le type géométrique du croquis à générer.</param>
        /// <returns>Le croquis généré.</returns>
/*        public static ArcGisProEspaceCollaboratif.Core.Croquis PointCollectionToCroquis(IPointCollection pointCollectionEntree, ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType typeCroquis)
        {
            ArcGisProEspaceCollaboratif.Core.Croquis newCroquis = new ArcGisProEspaceCollaboratif.Core.Croquis();
            if (pointCollectionEntree.PointCount == 0) { return newCroquis; }
            newCroquis.SetType(typeCroquis);

            for (int i = 0; i < pointCollectionEntree.PointCount; i++)
            {
                newCroquis.AddPoint(EspaceCollaboratifHelper.TransformPoint(pointCollectionEntree.Point[i]));
            }
            return newCroquis;
        }
*/

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
        /// Ajoute des attributs à un croquis en fonction des propriétés de la feature donnée en entrée.
        /// </summary>
        /// <param name="croquis">Le croquis auquel on veut ajouter des attributs.</param>
        /// <param name="feature">La feature initiale contenant les champs qu'on veut mettre en attribut dans le croquis.</param>
        /// <param name="treeLayerAndField">L'arborescence des calques et de leurs champs devant être mis en attribut pour le nouveau croquis.</param>
        /// <returns>Le <paramref name="croquis"/> complété d'attributs supplémentaires issus des propriétés de la <paramref name="feature"/>.</returns>
        public static void AddAttributs(ArcGisProEspaceCollaboratif.Core.Sketch croquis, Feature feature, System.Windows.Forms.TreeNode treeLayerAndField)
        {
            // Parcours des champs du feature.
            IReadOnlyList<Field> fields = feature.GetFields();
            foreach (Field field in fields)
            {
                // Test pour savoir si le champ du feature est à mettre en croquis.
                if (!Helper.IsFieldGoodForAttribut(feature.GetTable().GetName(), field.Name, treeLayerAndField))
                {
                    continue;
                }

                int assetNameIndex = feature.FindField(field.Name);
                QueuedTask.Run(() =>
                {
                    string originalValue = feature.GetOriginalValue(assetNameIndex) as String;
                    // Suppression de la partie heure 00:00:00 si le champ est de type date
                    if (field.FieldType == FieldType.Date && originalValue.Length != 0)
                    {
                        if (originalValue.Substring(originalValue.Length - 9).Equals(" 00:00:00"))
                        {
                            originalValue = originalValue.Substring(0, originalValue.Length - 9);
                        }
                    }
                    croquis.AddAttribute(field.Name, originalValue);
                });
            }
        }

        /// <summary>
        /// Ajoute à un croquis l'attribut composé du nom et de la valeur donnés en entrée.
        /// </summary>
        /// <param name="croquis">Le croquis auquel on veut ajouter un attribut.</param>
        /// <param name="nom">Le nom de l'attribut à ajouter au croquis.</param>
        /// <param name="val">La valeur de l'attribut à ajouter au croquis.</param>
        /// <returns>Le <paramref name="croquis"/> complété de l'attribut supplémentaire issu de la paire <paramref name="nom"/> et <paramref name="val"/>.</returns>
        public static void AddAttributs(ref ArcGisProEspaceCollaboratif.Core.Sketch croquis, string nom, string val)
        {
            croquis.AddAttribute(new ArcGisProEspaceCollaboratif.Core.Attribut(nom, val));
        }

        /// <summary>
        /// Génère un croquis pour l'Espace collaboratif à partir d'une géométrie ArcGIS pro.
        /// La géométrie du nouveau croquis est celle issue de la conversion de la géométrie donnée en entrée.
        /// </summary>
        /// <param name="geometry">La géométrie qu'il faut convertir en croquis de l'Espace collaboratif.</param>        
        /// <returns>Le croquis de l'Espace collaboratif issu depuis <paramref name="geometryEntree"/>.</returns>
        public static ArcGisProEspaceCollaboratif.Core.Sketch MakeSketch(Geometry geometry)
        {
            ArcGisProEspaceCollaboratif.Core.Sketch newSketch = new ArcGisProEspaceCollaboratif.Core.Sketch();

            // Selon le type géométrique de la géométrie à traiter.
            switch (geometry.GeometryType)
            {
                /*case esriGeometryType.esriGeometryRing:
                    IRing ring = geometryEntree as IRing;
                    IPointCollection vertexRing = ring as IPointCollection;
                    newCroquis = EspaceCollaboratifHelper.PointCollectionToCroquis(vertexRing, ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Polygone);
                    break;*/

                /*case esriGeometryType.esriGeometryPath:
                    IPath path = geometryEntree as IPath;
                    IPointCollection vertexPath = path as IPointCollection;
                    newCroquis = EspaceCollaboratifHelper.PointCollectionToCroquis(vertexPath, ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Ligne);
                    break;*/

                /*case esriGeometryType.esriGeometryLine:
                    newCroquis.SetType(ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Ligne);
                    LineSegment ligne = geometryEntree as LineSegment;
                    newCroquis.AddPoint(EspaceCollaboratifHelper.TransformPoint(ligne.FromPoint));
                    newCroquis.AddPoint(EspaceCollaboratifHelper.TransformPoint(ligne.ToPoint));
                    break;*/

                case GeometryType.Point:
                    newSketch.SetType(ArcGisProEspaceCollaboratif.Core.Sketch.SketchType.Point);
                    MapPoint point = geometry as MapPoint;
                    newSketch.AddPoint(Helper.TransformPoint(point));
                    break;

/*                case GeometryType.Polygon:
                    newSketch.SetType(ArcGisProEspaceCollaboratif.Core.Sketch.SketchType.Polygon);
                    Polygon polygon = geometry as Polygon;
                    ReadOnlyPointCollection vertexPolygon = geometry as ReadOnlyPointCollection;
                    newSketch = EspaceCollaboratifHelper.PointCollectionToSketch(vertexPolygon, ArcGisProEspaceCollaboratif.Core.Sketch.SketchType.Polygon);
                    break;
*/
                /*case GeometryType.Polyline:
                    Polyline polyligne = geometryEntree as Polyline;
                    IPointCollection vertexPolyligne = polyligne as IPointCollection;
                    newCroquis = EspaceCollaboratifHelper.PointCollectionToCroquis(vertexPolyligne, ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Ligne);
                    break;*/
            }

            if (!newSketch.IsValid()) { return new ArcGisProEspaceCollaboratif.Core.Sketch(); }
            return newSketch;
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
                    return null;

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

                    List<double> distanceSketch = new List<double>();
                    List<MapPoint> centroidSketch = new List<MapPoint>();

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
        /// Calcule le point d'application pour un nouveau signalement de l'Espace collaboratif à partir d'un unique croquis EspaceCollaboratif associé à cette future remarque.
        /// Il s'agit du centroïde du croquis<paramref name="croquisEntree"/>.
        /// </summary>
        /// <param name="croquisEntree">Le croquis dont son centroïde sera le point d'application du nouveau signalement.</param>        
        /// <returns>Le point sur lequel sera centrée le nouveau signalement contenant le croquis <paramref name="croquisEntree"/>.</returns>
        public static ArcGisProEspaceCollaboratif.Core.Point CalculatePointReport(ArcGisProEspaceCollaboratif.Core.Sketch croquisEntree)
        {
            List<ArcGisProEspaceCollaboratif.Core.Sketch> listCroquis = new List<ArcGisProEspaceCollaboratif.Core.Sketch>
            {
                croquisEntree
            };
            return Helper.CalculatePointReport(listCroquis);
        }


        /// <summary>
        /// Affiche un message dans la barre d'état d'ArcMap.
        /// 25/01/2021 Impossible de changer la status bar dans ArcGIS pro
        /// https://community.esri.com/t5/arcgis-pro-sdk-questions/status-bar-customization/m-p/771445
        /// J'ai donc mis une MessageBox...
        /// </summary>
        /// <param name="message">Le message à afficher dans la barre d'état d'ArcMap (en bas à gauche de l'écran.).</param>       
        public static void MessageBar(string message)
        {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, "Espace collaboratif");

            /*ArcGIS.Desktop.Framework.FrameworkApplication frameworkApplication;
            frameworkApplication.
            ESRI.ArcGIS.Framework.IApplication application = ArcMap.Application;

            IStatusBar mess = application.StatusBar;
            mess.set_Message(0, message);
            mess.Visible = true;

            return;*/
        }


        /// <summary>
        /// Renvoie le chemin complet d'accès du fichier XML de paramétrage pour le fonctionnement de l'add-on EspaceCollaboratif pour ArcMap.
        /// Il doit être situé dans le même répertoire que celui où se trouve le fichier de la carte ouverte en cours dans ArcMap, et son nom est EspaceCollaboratif.xml.
        /// </summary>     
        /// <returns>Le chemin complet + nom du fichier du fichier de paramétrage.</returns>
        public static string XML_NameFile()
        {
            string workDir = Contexte.Instance.repertoireTravail;
            return System.IO.Path.GetFullPath(workDir) + "\\" + Helper.nom_Fichier_Parametres_EspaceCollaboratif;
        }


        /// <summary>
        /// Teste la présence ou non du fichier XML de paramétrage nécéssaire au fonctionnement de l'add-on EspaceCollaboratif pour ArcMap.
        /// </summary>
        /// <returns>True si le fichier de paramétrage est présent, False dans le cas contraire.</returns>
 /*       public static bool XML_TestFile()
        {
            return System.IO.File.Exists(EspaceCollaboratifHelper.XML_NameFile());
        }
*/

        /// <summary>
        /// Charge en mémoire le fichier XML de paramétrage nécéssaire au fonctionnement de l'add-on EspaceCollaboratif.
        /// Cette opération est nécéssaire avant touts autres manipulations ultérieures (lecture ou écriture).
        /// </summary>
        /// <returns>Un object XmlDocument contenant le chargement du fichier XML de paramétrage EspaceCollaboratif.</returns>
        public static XmlDocument XML_Load()
        {
            string nom_FichierParametre = Helper.XML_NameFile();
            XmlDocument paramsXML = new XmlDocument();
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
        /// Supprime à l'intérieur du fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap, la balise indiquée en entrée.
        /// La balise et ses éléments qui y sont invclus sont totalement supprimés du fichier de paramétrage. 
        /// </summary>
        /// <param name="balise">Le chemin complet de la balise à rechercher à l'intérieur du fichier de paramétrage pour la supprimer.</param>  
 /*       public static void XML_RemoveElement(string balise)
        {
            XmlDocument paramsXML = EspaceCollaboratifHelper.XML_Load();
            XmlNode nodeToDelete = paramsXML.SelectSingleNode(balise);
            nodeToDelete.ParentNode.RemoveChild(nodeToDelete);
            paramsXML.Save(EspaceCollaboratifHelper.XML_NameFile());
        }
*/

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

            XmlDocument newDoc = new XmlDocument();
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
            System.Xml.XmlDocument paramsEspaceCollaboratif = new System.Xml.XmlDocument();

            StreamReader streamXML = new StreamReader(Helper.XML_NameFile());
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
            System.Xml.XmlDocument paramsEspaceCollaboratif = new System.Xml.XmlDocument();
            StreamReader streamXML = new StreamReader(Helper.XML_NameFile());
            paramsEspaceCollaboratif.Load(streamXML);
            System.Xml.XmlNodeList elemlist = paramsEspaceCollaboratif.DocumentElement.SelectNodes(element);
            streamXML.Close();
            List<string> allElements = new List<string>();
            foreach (XmlNode node in elemlist)
            {
                allElements.Add(node.InnerText);
            }
            return allElements;
        }

        /// <summary>
        /// Renvoie tous les éléments indiqués à l'intérieur du fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap.
        /// </summary>
        /// <param name="elementName">Le nom des éléments à rechercher.</param>
        /// <returns>La valeur associée de chaque élément ayant comme nom celui de <paramref name="element"/>.</returns>
        /*       public static List<string> XML_ReadElement(string element)
               {
                   List<string> liste = new List<string>();

                   System.Xml.XmlDocument paramsEspaceCollaboratif = new System.Xml.XmlDocument();
                   StreamReader streamXML = new StreamReader(EspaceCollaboratifHelper.XML_NameFile());
                   paramsEspaceCollaboratif.Load(streamXML);
                   System.Xml.XmlNodeList elemlist = paramsEspaceCollaboratif.DocumentElement.SelectNodes(element);
                   streamXML.Close();

                   for (int i = 0; i < elemlist.Count; i++)
                   {
                       liste.Add(elemlist[i].InnerText);
                   }

                   return liste;
               }
       */

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
                Helper.XML_InsertElement(pathElement,
                                                        Helper.XML_Suffixe(chemin),
                                                        element);
            }
        }        
       

        /// <summary>
        /// Renvoie la liste des thèmes préférés contenus à l'intérieur du fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap.
        /// </summary>
        /// <returns>La liste des noms de thèmes préférés contenus dans le fichier de paramétrage.</returns>
        public static List<string> Load_PreferredThemes(string element)
        {
            return Helper.XML_AllElement(element);
        }
       
        /// <summary>
        /// Sauvegarde les thèmes préférés dans le fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap.
        /// </summary>
        /// <param name="preferedThemes">La liste des thèmes préférés à sauvegarder dans le fichier de paramétrage.</param>
        public static void Save_PreferredThemes(List<String> preferedThemes)
        {
            Helper.XML_WriteElement(preferedThemes, Helper.xml_Themes);
        }
        
        /// <summary>
        /// Sauvegarde les thèmes préférés dans le fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap.
        /// </summary>
        /// <param name="preferedThemes">La liste des thèmes préférés à sauvegarder dans le fichier de paramétrage.</param>
        public static void Save_PreferredThemes(List<ArcGisProEspaceCollaboratif.Core.Theme> preferedThemes)
        {
            List<String> ListThemes = new List<string>();

            foreach (ArcGisProEspaceCollaboratif.Core.Theme theme in preferedThemes)
            {
                ListThemes.Add(theme.Groupe.Nom);
            }

            Helper.Save_PreferredThemes(ListThemes);
        }
        

        /// <summary>
        /// Lit le login par défaut à utiliser pour se connecter au service EspaceCollaboratif contenu dans le fichier XML de paramétrage.
        /// </summary>
        /// <returns>Le login à utiliser par défaut pour se connecter au service EspaceCollaboratif.</returns>
        public static string Load_Login()
        {
            return Helper.XML_FirstElement(Helper.xml_Login);
        }
        
        /// <summary>
        ///  Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, le login à utiliser pour se connecter au service EspaceCollaboratif.
        /// </summary>
        /// <param name="login">Le login par défaut à sauvegarder dans le fichier de paramétrage.</param>
        public static void Save_Login(string login)
        {
            if (!Helper.XML_HasElement(Helper.xml_Login))
            {
                Helper.XML_AddElement(Helper.xml_Login);
            }

            Helper.XML_SetElement(Helper.xml_Login, login);
        }

        /// <summary>
        /// Lit depuis le fichier de paramétrage XML EspaceCollaboratif, la taille de la pagination pour l'importation des remarques.
        /// </summary>
        /// <returns>La taille de pagination contenue dans le fichier de paramétrage. Renvoie 0 si cette valeur est absente.</returns>
        public static int Load_Pagination()
        {
            string pagination = Helper.XML_FirstElement(Helper.xml_Pagination);

            if (pagination.Equals(""))
            {
                return 0;
            }

            try
            {
                return int.Parse(pagination);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return 0;
            }
        }
        
        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, la taille de la pagination pour l'importation des remarques.
        /// </summary>
        /// <param name="pagination">La taille de pagination à sauvegarder dans le fichier de paramétrage.</param>
        public static void Save_Pagination(uint pagination)
        {
            if (!Helper.XML_HasElement(Helper.xml_Pagination))
            {
                Helper.XML_AddElement(Helper.xml_Pagination);
            }

            Helper.XML_SetElement(Helper.xml_Pagination, pagination.ToString());
        }
        
        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, la taille par défaut de la pagination pour l'importation des remarques.
        /// </summary>
        /*        public static void Save_Pagination()
                {
                    EspaceCollaboratifHelper.Save_Pagination(0);
                }
        */



        /// <summary>
        /// Lit la valeur du tag "Import_pour_groupe" du fichier XML de paramétrage.
        /// </summary>
        /// <returns></returns>
        public static string Load_Group()
        {
            return Helper.XML_FirstElement(Helper.xml_Group);
        }
        
        /// <summary>
        ///  Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, "Import_pour_groupe"
        /// </summary>
        /// <param name="group"> true ou false</param>
        public static void Save_Group(string group)
        {
            if (!Helper.XML_HasElement(Helper.xml_Group))
            {
                Helper.XML_AddElement(Helper.xml_Group);
            }

            Helper.XML_SetElement(Helper.xml_Group, group);
        }
        

        /// <summary>
        /// Lit depuis le fichier de paramétrage XML EspaceCollaboratif, la date pour laquelle on extrait que les remaques postérieures à celle-ci.
        /// </summary>
        /// <returns>La date d'extration stockée dans le fichier de paramétrage.</returns>
        public static System.DateTime Load_DateExtraction()
        {
            string dateExtration = Helper.XML_FirstElement(Helper.xml_DateExtraction);

            try
            {
                if (dateExtration.Length != 0)
                {
                    return Convert.ToDateTime(dateExtration);
                }
                else
                {
                    return Convert.ToDateTime(Helper.dateDefaut);
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("La date limite d'extraction contenue dans fichier XML de paramétrage n'est pas de forme valide.\n\nDate limite d'extraction = ''" + dateExtration + "''.", "IGN EspaceCollaboratif", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return Convert.ToDateTime(Helper.dateDefaut);
            }
        }
       
        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, la date d'extraction pour l'importation des remarques.
        /// </summary>
        /// <param name="date">La date d'extraction à enregistrer dans le fichier de paramétrage.</param>
        public static void Save_DateExtraction(System.DateTime date)
        {
            if (!Helper.XML_HasElement(Helper.xml_DateExtraction))
            {
                Helper.XML_AddElement(Helper.xml_DateExtraction);
            }

            Helper.XML_SetElement(Helper.xml_DateExtraction, date.ToString());
        }
        
        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, la date du jour comme date d'extraction.
        /// </summary>
        /*        public static void Save_DateExtraction()
                {
                    EspaceCollaboratifHelper.Save_DateExtraction(System.DateTime.Now);
                }
        */

        /// <summary>
        /// Obtient à partir du fichier XML de paramétrage, le nom du calque à utiliser pour le filtrage spatial de l'importation des remarques.
        /// </summary>
        /// <returns>Le nom du calque pour le filtrage spatiale stocké dans le fichier de paramétrage.</returns>
        public static string Load_CalqueFiltrage()
        {
            return Helper.XML_FirstElement(Helper.xml_Zone_extraction);
        }
        
        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, le nom du calque à utiliser pour le filtrage spatial lors l'importation des remarques.
        /// </summary>
        /// <param name="layer">Le nom du calque à enregistrer dans le fichier de paramétrage pour le filtrage spatiale.</param>
        public static void Save_CalqueFiltrage(string layer)
        {
            XML_SetElement(Helper.xml_Zone_extraction, layer);
        
        }

        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, le nom du calque à utiliser pour le filtrage spatial lors l'importation des remarques.
        /// </summary>
        /// <param name="layer">Le calque dont on enregistre son nom dans le fichier de paramétrage pour le filtrage spatiale.</param>
        public static void Save_CalqueFiltrage(Layer layer)
        {
            XML_SetElement(Helper.xml_Zone_extraction, layer.Name);
        }
    

        /// <summary>
        /// Obtient à partir du XML de paramétrage, l'adresse du service EspaceCollaboratif contenue dans le fichier XML de paramétrage.
        /// </summary>
        /// <returns>L'adresse d'accès au service EspaceCollaboratif stockée dans le fichier de paramétrage EspaceCollaboratif.</returns>
        public static string Load_Urlhost()
        {
            string Urlhost = Helper.XML_FirstElement(Helper.xml_UrlHost);

            if (Urlhost.Equals(""))
            {
                System.Windows.Forms.MessageBox.Show("Impossible de trouver l'adresse du service de l'Espace collaboratif dans le fichier XML de paramétrage.", "IGN EspaceCollaboratif", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return "";
            }

            return Urlhost;
        }
 

        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, l'adresse du service EspaceCollaboratif.
        /// </summary>
        /// <param name="UrlHost">L'adresse d'accès au service EspaceCollaboratif à enregistrer dans le fichier de paramétrage.</param>
        public static void Save_Urlhost(string UrlHost)
        {
            XML_SetElement(Helper.xml_UrlHost, UrlHost);
        }


        /// <summary>
        /// Lit l'option d'affichage des calques croquis EspaceCollaboratif dans le fichier XML de paramétrage EspaceCollaboratif.
        /// </summary>
        /// <returns>True si le fichier de paramétrage contient explicitement qu'il faut afficher les croquis.</returns>
        public static bool Load_AfficherCroquis()
        {
            if (!Helper.XML_HasElement(Helper.xml_AfficherCroquis))
            {
                Helper.Save_AfficherCroquis();
                return true;
            }

            string option = Helper.XML_FirstElement(Helper.xml_AfficherCroquis);
            return option.Equals("Oui") || option.Equals("Vrai") || option.Equals("oui") || option.Equals("vrai") || option.Equals("True") || option.Equals("true");
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


        /// <summary>
        ///Force dans le fichier XML de paramétrage EspaceCollaboratif, la valeur True pour afficher les croquis EspaceCollaboratif.
        /// </summary>
        public static void Save_AfficherCroquis()
        {
            Helper.Save_AfficherCroquis(true);
        }

        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, le proxy dans le cas d'une connexion partenariale extérieure au service EspaceCollaboratif.
        /// </summary>
        /// <param name="proxy">L e nom du proxy à enregistrer dans le fichier de paramétrage.</param>
        public static void Save_Proxy(string proxy)
        {
            if (!XML_HasElement(xml_Proxy))
            {
                XML_AddElement(xml_Proxy);
            }
            XML_SetElement(xml_Proxy, proxy);
        }

        /// <summary>
        /// Lit le login par défaut à utiliser pour se connecter au service EspaceCollaboratif contenu dans le fichier XML de paramétrage.
        /// </summary>
        /// <returns>Le login à utiliser par défaut pour se connecter au service EspaceCollaboratif.</returns>
        public static string Load_CleGeoportail()
        {
            return Helper.XML_FirstElement(Helper.xml_CleGeoPortail); ;
        }

        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, la clé Géoportail de l'utilisateur.
        /// </summary>
        /// <param name="cle">La clé Géoportail à enregistrer dans le fichier de paramétrage.</param>
        public static void Save_CleGeoportail(string cle)
        {
            if (!XML_HasElement(xml_CleGeoPortail))
            {
                XML_AddElement(xml_CleGeoPortail);
            }

            XML_SetElement(xml_CleGeoPortail, cle);
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
        public static string Load_GroupeActif()
        {
            return Helper.XML_FirstElement(Helper.xml_GroupeActif);
        }

        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, le nom du groupe actif de l'utilisateur du service EspaceCollaboratif.
        /// </summary>
        /// <param name="groupeActif">Le nom du groupe actif de l'utilisateur du service EspaceCollaboratif à enregistrer dans le fichier de paramétrage.</param>
        public static void Save_GroupeActif(string groupeActif)
        {
            if (!XML_HasElement(xml_GroupeActif))
            {
                XML_AddElement(xml_GroupeActif);
            }

            XML_SetElement(xml_GroupeActif, groupeActif);
        }

        /// <summary>
        /// Renvoie depuis le fichier XML de paramétrage, la liste des calques et de leurs champs qu'il faut mettre en attribut dans la génération des croquis EspaceCollaboratif.
        /// </summary>
        /// <returns> Les calques et leurs champs sous forme d'un System.Windows.Forms.TreeView </returns>
        public static System.Windows.Forms.TreeNode Load_AttributsCroquis()
        {
            System.Windows.Forms.TreeNode attributs = new System.Windows.Forms.TreeNode();
            XmlDocument paramsEspaceCollaboratif = Helper.XML_Load();              

            // Recherche des nœuds pour les calques. 
            System.Xml.XmlNodeList elemlist = paramsEspaceCollaboratif.DocumentElement.SelectNodes(Helper.xml_AttributsCroquis);
            if (elemlist.Count == 0) { return attributs; }

            for (int numElemList = 0; numElemList < elemlist.Count; numElemList++)
            {
                System.Xml.XmlNode nodeCalque = elemlist.Item(numElemList).SelectSingleNode(Helper.xml_BaliseNomCalque);

                // S'il y a bien la balise pour le calque.
                if (nodeCalque != null)
                {
                    System.Xml.XmlNodeList listNodeChamps = elemlist.Item(numElemList).SelectNodes(Helper.xml_BaliseChampCalque);
                    
                    // Et s'il y a des balises pour les attributs.
                    if (listNodeChamps.Count != 0)
                    {
                        attributs.Nodes.Add(nodeCalque.InnerText);

                        for (int i = 0; i < listNodeChamps.Count; i++)
                        {
                            attributs.Nodes[attributs.Nodes.Count-1].Nodes.Add(listNodeChamps.Item(i).InnerText);
                        }
                    }
                }
            }   
            return attributs;
        }
                
        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage les calques et leurs attributs qu'il faut mettre en attribut dans la génération des croquis EspaceCollaboratif.
        /// </summary>
        /// <param name="attributs">Le nom des calques et leurs champs à stocker mis dans la structure d'un System.Windows.Forms.TreeNode.</param>
        public static void Save_AttributsCroquis(System.Windows.Forms.TreeNode attributs)
        {
            XmlDocument paramsXML = Helper.XML_Load();
            XmlNode nodeToDelete = paramsXML.SelectSingleNode(  Helper.xml_AttributsCroquis );

            // Suppresion préalable des éventuels anciens attributs stockés dans le fichier de paramétrage.
            while ( nodeToDelete != null )
            { 
                nodeToDelete.ParentNode.RemoveChild(nodeToDelete);
                nodeToDelete = paramsXML.SelectSingleNode(Helper.xml_AttributsCroquis);
            }

            // Parcours des nœuds de l'object attributs.
            foreach (System.Windows.Forms.TreeNode attributCalque in attributs.Nodes)
            {   // Si le nœuds contient des sous-éléments
                if (attributCalque.Nodes.Count != 0 && attributCalque.Text.Length != 0)
                {
                    XmlNode noeudRoot = paramsXML.SelectSingleNode(Helper.XML_Prefixe(Helper.xml_AttributsCroquis));
                    XmlElement elemCalque = paramsXML.CreateElement(Helper.XML_Suffixe(Helper.xml_AttributsCroquis));
                    
                    // Stockage du nom du calque à retenir.
                    XmlElement nomCalque = paramsXML.CreateElement(Helper.xml_BaliseNomCalque);
                    nomCalque.InnerText = attributCalque.Text;
                    elemCalque.AppendChild(nomCalque);

                    // Parcours des sous-éléments qui contiennent les champs à stocker.
                    foreach (System.Windows.Forms.TreeNode attribut in attributCalque.Nodes)
                    {
                        XmlElement elementChamp = paramsXML.CreateElement(Helper.xml_BaliseChampCalque);
                        elementChamp.InnerText = attribut.Text;
                        elemCalque.AppendChild(elementChamp);
                    }
                    
                    // Enregistrement dans le fichier de paramétrage du nom et des champs du calque à mettre en attribut.
                    noeudRoot.AppendChild(elemCalque);
                }
            }
            // Fermeture du fichier XML.
            paramsXML.Save(Helper.XML_NameFile()); 
        }
    }  
}
