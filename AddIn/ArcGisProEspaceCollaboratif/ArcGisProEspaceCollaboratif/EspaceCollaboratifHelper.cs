using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using ArcGIS.Core.Geometry;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
//using ESRI.ArcGIS.Carto;
//using ESRI.ArcGIS.esriSystem;
//using ESRI.ArcGIS.Geodatabase;
using log4net;
using ArcGisProEspaceCollaboratif.Core;

namespace ArcGisProEspaceCollaboratif
{


    /// <summary>
    /// Classe contenant des utilitaires pour le plugin
    /// </summary>
    public class EspaceCollaboratifHelper
    {
        public const String nom_Fichier_EspaceCollaboratif = "EspaceCollaboratif.gdb";
        public const String nom_Fichier_Parametres_EspaceCollaboratif = "EspaceCollaboratif.xml";

        public const String nom_Calque_Remarque = "Remarque_EspaceCollaboratif";
        public const String nom_Calque_Croquis_Fleche = "Croquis_EspaceCollaboratif_Fleche";
        public const String nom_Calque_Croquis_Texte = "Croquis_EspaceCollaboratif_Texte";
        public const String nom_Calque_Croquis_Polygone = "Croquis_EspaceCollaboratif_Polygone";
        public const String nom_Calque_Croquis_Ligne = "Croquis_EspaceCollaboratif_Ligne";
        public const String nom_Calque_Croquis_Point = "Croquis_EspaceCollaboratif_Point";

        public const String calque_Remarque_Lyr = "Remarque_EspaceCollaboratif.lyr";

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
        public const String xml_DateExtraction = "/Paramètres_connexion_à_EspaceCollaboratif/ArcMap/Date_extraction";
        public const String xml_Pagination = "/Paramètres_connexion_à_EspaceCollaboratif/ArcMap/Pagination";
        public const String xml_Themes = "/Paramètres_connexion_à_EspaceCollaboratif/ArcMap/Thèmes_préférés/Thème";
        public const String xml_Zone_extraction = "/Paramètres_connexion_à_EspaceCollaboratif/ArcMap/Zone_extraction";
        public const String xml_AfficherCroquis = "/Paramètres_connexion_à_EspaceCollaboratif/ArcMap/Afficher_Croquis";
        public const String xml_AttributsCroquis = "/Paramètres_connexion_à_EspaceCollaboratif/ArcMap/Attributs_croquis";
        public const String xml_BaliseNomCalque = "Calque_Nom";
        public const String xml_BaliseChampCalque = "Calque_Champ";
        public const String xml_Group = "/Paramètres_connexion_à_EspaceCollaboratif/ArcMap/Import_pour_groupe";

        public const String dateDefaut = "01/01/1900";
        public const int longueurMaxChamp = 5000;

        public static String EspaceCollaboratifAssemblyDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";

        private readonly ArcGisProEspaceCollaboratif.Core.EspaceCollaboratifLogger riplogger = ArcGisProEspaceCollaboratif.Core.EspaceCollaboratifLogger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(EspaceCollaboratifHelper));

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
        /// Définit toutes les caractéristiques pour le futur calque dédié à contenir les remarques EspaceCollaboratif.
        /// </summary>
        /// <param name="featureWorkspace">L'espace de travail de la carte en cours sur laquelle on veut créer le calque supplémentaire.</param>
        /// <param name="spatialReferenceCalque">Le système de référence spatial à attribuer au calque nouvellement crée.</param>
        /// <returns>FeatureLayer pouvant être ajouté dans la carte en cours.</returns>
        /*        public static IFeatureLayer CreerCalqueRemarqueEspaceCollaboratif(IFeatureWorkspace featureWorkspace, SpatialReference spatialReferenceCalque)
                {
                    // Instantiate a feature class description to get the required fields.
                    IFeatureClassDescription fcDescription = new FeatureClassDescription() as IFeatureClassDescription;
                    IObjectClassDescription ocDescription = (IObjectClassDescription)fcDescription;
                    IFields fields = ocDescription.RequiredFields;
                    IFieldsEdit fieldsEdit = (IFieldsEdit)fields;
                    // -- on complete la définition de la géométrie
                    {
                        int shapeFieldIndex = fields.FindField(fcDescription.ShapeFieldName);
                        IField shapeField = fields.get_Field(shapeFieldIndex);

                        IGeometryDef geometryDef = shapeField.GeometryDef;
                        IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
                        geometryDefEdit.GeometryType_2 = GeometryType.Point; // Ici on choisit une géométrie de point pour le nouveau calque.

                        SpatialReference spatialReference = spatialReferenceCalque;
                        ISpatialReferenceResolution spatialReferenceResolution = (ISpatialReferenceResolution)spatialReference;
                        spatialReferenceResolution.ConstructFromHorizon();
                        spatialReferenceResolution.SetDefaultXYResolution();
                        ISpatialReferenceTolerance spatialReferenceTolerance = (ISpatialReferenceTolerance)spatialReference;
                        spatialReferenceTolerance.SetDefaultXYTolerance();
                        geometryDefEdit.SpatialReference_2 = spatialReference;

                    } // -- fin def géométrie


                    // Ajoute le champ: numéro de la remarque
                    fieldsEdit.AddField(DefinirChamp(nom_Champ_IdRemarque, FieldType.Integer));
                    // Ajoute le champ: nom de l'auteur de la remarque
                    fieldsEdit.AddField(DefinirChamp(nom_Champ_Auteur, FieldType.String, 50));
                    // Ajoute le champ: nom de la commune où est située la remarque
                    fieldsEdit.AddField(DefinirChamp(nom_Champ_Commune, FieldType.String));
                    // Ajoute le champ: nom du département où est située la remarque
                    fieldsEdit.AddField(DefinirChamp(nom_Champ_Departement, FieldType.String, 23));
                    // Ajoute le champ: indicatif département où est située la remarque
                    fieldsEdit.AddField(DefinirChamp(nom_Champ_IDDepartement, FieldType.String, 3));
                    // Ajoute le champ: date de création
                    fieldsEdit.AddField(DefinirChamp(nom_Champ_DateCreation, FieldType.Date));
                    // Ajoute le champ: date de mise à jour
                    fieldsEdit.AddField(DefinirChamp(nom_Champ_DateMAJ, FieldType.Date));
                    // Ajoute le champ: date de validation
                    fieldsEdit.AddField(DefinirChamp(nom_Champ_DateValidation, FieldType.Date));
                    // Ajoute le champ: thèmes
                    fieldsEdit.AddField(DefinirChamp(nom_Champ_Themes, FieldType.String));
                    // Ajoute le champ: statut
                    fieldsEdit.AddField(DefinirChamp(nom_Champ_Statut, FieldType.Integer));

                    // Ajoute le champ: message            
                    fieldsEdit.AddField(DefinirChamp(nom_Champ_Message, FieldType.String, EspaceCollaboratifHelper.longueurMaxChamp));

                    // Ajoute le champ: réponse
                    fieldsEdit.AddField(DefinirChamp(nom_Champ_Reponse, FieldType.String, EspaceCollaboratifHelper.longueurMaxChamp));

                    // Ajoute le champ: lien public.
                    fieldsEdit.AddField(DefinirChamp(nom_Champ_Url, FieldType.String));
                    // Ajoute le champ: lien privé.
                    fieldsEdit.AddField(DefinirChamp(nom_Champ_UrlPrive, FieldType.String));

                    // Ajoute le champ: document.
                    fieldsEdit.AddField(DefinirChamp(nom_Champ_Document, FieldType.String));
                    // Ajoute le champ: autorisation
                    fieldsEdit.AddField(DefinirChamp(nom_Champ_Autorisation, FieldType.String));

                    // Use IFieldChecker to create a validated fields collection.
                    IFieldChecker fieldChecker = new FieldChecker();
                    IEnumFieldError enumFieldError = null;
                    IFields validatedFields = null;
                    fieldChecker.ValidateWorkspace = (IWorkspace)featureWorkspace;
                    fieldChecker.Validate(fields, out enumFieldError, out validatedFields);

                    //   this.debugForm.WriteLine("fcDescription.ShapeFieldName : " + fcDescription.ShapeFieldName);
                    IFeatureClass featureClass = featureWorkspace.CreateFeatureClass(
                        EspaceCollaboratifHelper.nom_Calque_Remarque, //featureClassName
                        validatedFields, // validatedFields
                        ocDescription.InstanceCLSID, // ocDescription.InstanceCLSID
                        ocDescription.ClassExtensionCLSID, // ocDescription.ClassExtensionCLSID
                        esriFeatureType.esriFTSimple,
                        fcDescription.ShapeFieldName, // fcDescription.ShapeFieldName Attention, c'est dangereux!
                        "" //configKeyword
                    );

                    FeatureLayer featureLayer = new FeatureLayer
                    {
                        FeatureClass = featureClass,
                        Name = featureClass.AliasName
                    };

                    ILayer layer = featureLayer;

                    return featureLayer;
                }
        */

        /// <summary>
        /// Définit toutes les caractéristiques pour le futur calque devant contenir un type géométrique donné de croquis EspaceCollaboratif.
        /// </summary>
        /// <param name="nomCoucheCroquis">Le nom du calque à créer.</param>
        /// <param name="featureWorkspace">L'espace de travail de la carte en cours sur laquelle on veut créer le calque supplémentaire.</param>
        /// <param name="spatialReferenceCalque">Le système de référence spatial à attribuer au calque nouvellement crée.</param>
        /// <param name="type_CoucheCroquis">Le type géométrique du calque nouvellement créé.</param>
        /// <returns>FeatureLayer pouvant être ajouté dans la carte en cours.</returns>
        /*        public static IFeatureLayer CreerCalqueCroquisEspaceCollaboratif(String nomCoucheCroquis, IFeatureWorkspace workspaceTemp, ISpatialReference spatialReferenceCalque, GeometryType type_CoucheCroquis)
                {
                    // Instantiate a feature class description to get the required fields.
                    IFeatureClassDescription fcDescription_CoucheCroquis = new FeatureClassDescription() as IFeatureClassDescription;
                    IObjectClassDescription ocDescription_CoucheCroquis = (IObjectClassDescription)fcDescription_CoucheCroquis;
                    IFields fields_CoucheCroquis = ocDescription_CoucheCroquis.RequiredFields;
                    IFieldsEdit fieldsEdit_CoucheCroquis = (IFieldsEdit)fields_CoucheCroquis;

                    // -- on complete la définition de la géométrie
                    {

                        int shapeFieldIndex = fields_CoucheCroquis.FindField(fcDescription_CoucheCroquis.ShapeFieldName);
                        IField shapeField_CoucheCroquis = fields_CoucheCroquis.get_Field(shapeFieldIndex);

                        IGeometryDef geometryDef_CoucheCroquis = shapeField_CoucheCroquis.GeometryDef;
                        IGeometryDefEdit geometryDefEdit_CoucheCroquis = (IGeometryDefEdit)geometryDef_CoucheCroquis;
                        geometryDefEdit_CoucheCroquis.GeometryType_2 = type_CoucheCroquis;

                        SpatialReference spatialReference_CoucheCroquis = spatialReferenceCalque;

                        ISpatialReferenceResolution spatialReferenceResolution_CoucheCroquis = (ISpatialReferenceResolution)spatialReference_CoucheCroquis;
                        spatialReferenceResolution_CoucheCroquis.ConstructFromHorizon();
                        spatialReferenceResolution_CoucheCroquis.SetDefaultXYResolution();
                        ISpatialReferenceTolerance spatialReferenceTolerance_CoucheCroquis = (ISpatialReferenceTolerance)spatialReference_CoucheCroquis;
                        spatialReferenceTolerance_CoucheCroquis.SetDefaultXYTolerance();
                        geometryDefEdit_CoucheCroquis.SpatialReference_2 = spatialReference_CoucheCroquis;

                    } // -- fin def géométrie


                    // Ajoute le champ: Lien_remarque
                    fieldsEdit_CoucheCroquis.AddField(DefinirChamp(nom_Champ_LienRemarque, FieldType.Integer));
                    fieldsEdit_CoucheCroquis.AddField(DefinirChamp(nom_Champ_NomCroquis, FieldType.String));
                    fieldsEdit_CoucheCroquis.AddField(DefinirChamp(nom_Champ_Attributs, FieldType.String, EspaceCollaboratifHelper.longueurMaxChamp));
                    fieldsEdit_CoucheCroquis.AddField(DefinirChamp(nom_Champ_LienBDuni, FieldType.String));




                    // Use IFieldChecker to create a validated fields collection.
                    IFieldChecker fieldChecker_CoucheCroquis = new FieldChecker();
                    IEnumFieldError enumFieldError_CoucheCroquis = null;
                    IFields validatedFields_CoucheCroquis = null;
                    fieldChecker_CoucheCroquis.ValidateWorkspace = (IWorkspace)workspaceTemp;
                    fieldChecker_CoucheCroquis.Validate(fields_CoucheCroquis, out enumFieldError_CoucheCroquis, out validatedFields_CoucheCroquis);

                    //   this.debugForm.WriteLine("fcDescription.ShapeFieldName : " + fcDescription.ShapeFieldName);
                    IFeatureClass featureClass_Croquis = workspaceTemp.CreateFeatureClass(
                        nomCoucheCroquis, //featureClassName
                        validatedFields_CoucheCroquis, // validatedFields
                        ocDescription_CoucheCroquis.InstanceCLSID, // ocDescription.InstanceCLSID
                        ocDescription_CoucheCroquis.ClassExtensionCLSID, // ocDescription.ClassExtensionCLSID
                        esriFeatureType.esriFTSimple,
                        fcDescription_CoucheCroquis.ShapeFieldName, // fcDescription.ShapeFieldName Attention, c'est dangereux!
                        "" //configKeyword
                    );

                    IFeatureLayer featureLayer_Croquis = new FeatureLayer
                    {
                        FeatureClass = featureClass_Croquis,
                        Name = featureClass_Croquis.AliasName
                    };

                    return featureLayer_Croquis;
                }
        */

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
        /// Limite la longueur d'une string pour ne pas dépasser la taille maximalle que ne peut contenir les attributs d'un calque.
        /// La taille maximalle est définit par la constante EspaceCollaboratifHelper.longueurMaxChamp.
        /// </summary>
        /// <param name="champ"> L'object string dont on doit éventuellement limiter sa longueur.
        /// <returns>L'object champ raccourcie à ses EspaceCollaboratifHelper.longueurMaxChamp premiers caractères si il dépasse cette taille limite</returns>
        /*       public static string Limite(string champ)
               {
                   if (champ.Length > EspaceCollaboratifHelper.longueurMaxChamp)
                   {
                       return champ.Substring(0, EspaceCollaboratifHelper.longueurMaxChamp);
                   }
                   else
                   {
                       return champ;
                   }
               }
       */

        /// <summary>
        /// Génère à partir d'un croquis EspaceCollaboratif, son équivalent Geometry pouvant être mise dans une shape.
        /// </summary>
        /// <param name="uneGeometrie">Une Geometry de départ pour définir la nature géométrique de la IGeometry en sortie.</param>
        /// <param name="unCroquis">Le croquis à transformer en IGeometry équivalent.</param>
        /// <returns>Une Geometry équivalente au croquis en entrée.</returns>
        /*        public static Geometry GeometryFromCroquis(Geometry uneGeometrie, ArcGisProEspaceCollaboratif.Core.Croquis unCroquis)
                {
                    IPointCollection pointCollection = null;
                    pointCollection = uneGeometrie as IPointCollection;

                    foreach (ArcGisProEspaceCollaboratif.Core.Point unVertexCroquis in unCroquis.Points)
                    {
                        pointCollection.AddPoint(EspaceCollaboratifHelper.TransformPoint(unVertexCroquis));
                    }

                    return uneGeometrie;
                }
        */

        /// <summary>
        /// Remplit un attribut d'un object EspaceCollaboratif sur la carte en cours.
        /// </summary>
        /// <param name="featureClassEspaceCollaboratif">La FeatureClass du calque contenant l'object EspaceCollaboratif sur lequel on veut compléter un attribut.</param>
        /// <param name="featureEspaceCollaboratif">La Feature de l'object EspaceCollaboratif sur lequel on veut compléter un attribut.</param>
        /// <param name="nomChamp">Le nom de l'attribut dans lequel on veut écrire.</param>
        /// <param name="valChamp">La valeur à écrire dans l'attribut de l'object EspaceCollaboratif. </param>
        /*        public static void CompleteChampEspaceCollaboratif(IFeatureClass featureClassEspaceCollaboratif, IFeature featureEspaceCollaboratif, string nomChamp, object valChamp)
                {
                    if (valChamp == null || nomChamp.Equals("")) { return; }

                    int iFiedName = featureClassEspaceCollaboratif.FindField(nomChamp);
                    if (iFiedName >= 0)
                    {
                        featureEspaceCollaboratif.set_Value(iFiedName, valChamp);
                    }
                }
        */

        /// <summary>
        /// Renvoie la longueur du plus long message contenu dans une liste de remarque EspaceCollaboratif.
        /// </summary>
        /// <param name="remarques">La liste des remarques EspaceCollaboratif dans laquelle on veut chercher celle qui a le message le plus long.</param>
        /// <returns>La longueur du plus long message.</returns>
        /*        public static int Max_Length_Message(List<ArcGisProEspaceCollaboratif.Core.Signalement> remarques)
                {
                    int maxLength = 0;

                    foreach (ArcGisProEspaceCollaboratif.Core.Signalement remarque in remarques)
                    {
                        if (remarque.Commentaire.Length > maxLength)
                        {
                            maxLength = remarque.Commentaire.Length;
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
        /// Convertit un point ArcGisProEspaceCollaboratif.Core en son équivalent point MapPoint. 
        /// </summary>
        /// <param name="pointEntree">Le Point ArcGisProEspaceCollaboratif.Core qu'on veut convertir en MapPoint.</param>
        /// <returns>Le point converti.</returns>
        /*       public static ArcGIS.Core.Geometry.MapPoint TransformPoint(ArcGisProEspaceCollaboratif.Core.Point pointEntree)
               {
                   MapPoint point = null;
                   ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                   {
                       using (MapPointBuilder mapPointBuilder = new MapPointBuilder())
                       {
                           mapPointBuilder.SetValues(pointEntree.Longitude, pointEntree.Latitude);
                           point = mapPointBuilder.ToGeometry();
                       }
                   });

                   if (point.IsEmpty)
                   {
                       return null;
                   }

                   return point;
               }
       */
        /// <summary>
        /// Convertit un point MapPoint en son équivalent point ArcGisProEspaceCollaboratif.Core 
        /// </summary>
        /// <param name="pointEntree">Le Point MapPoint qu'on veut convertir en point ArcGisProEspaceCollaboratif.Core</param>
        /// <returns>Le point converti.</returns>
        /*        public static ArcGisProEspaceCollaboratif.Core.Point TransformPoint(ArcGIS.Core.Geometry.MapPoint pointEntree)
                {
                    if (pointEntree.IsEmpty)
                    {
                        return new ArcGisProEspaceCollaboratif.Core.Point();
                    }
                    else
                    { 
                        // S'il faut arrondir la valeur des coordonnées du point
                        return new Point(pointEntree.X, pointEntree.Y);
                    }
                }
        */
        /// <summary>
        /// Calcule le barycentre d'une liste de point.
        /// </summary>
        /// <param name="points">La liste des points Point Esri dont on veut déterminer leur barycentre.</param>
        /// <returns>Le Point Esri positionné sur le barycentre calculé.</returns>
        /*        public static Point Barycentre(List<Point> points)
                {
                    if (points.Count == 0) { return null; }
                    if (points.Count == 1) { return points.First(); }

                    double barycentreX = 0;
                    double barycentreY = 0;

                    foreach (Point pointTemp in points)
                    {
                        barycentreX += pointTemp.X;
                        barycentreY += pointTemp.Y;
                    }

                    barycentreX /= points.Count;
                    barycentreY /= points.Count;

                    Point pointResult = new Point();
                    pointResult.PutCoords(barycentreX, barycentreY);
                    return pointResult;
                }
        */
        /// <summary>
        /// Calcule le barycentre d'une liste de point.
        /// </summary>
        /// <param name="points">La liste des points Point EspaceCollaboratif dont on veut déterminer leur barycentre.</param>
        /// <returns>Le Point EspaceCollaboratif positionné sur le barycentre calculé.</returns>
        /*       public static ArcGisProEspaceCollaboratif.Core.Point Barycentre(List<ArcGisProEspaceCollaboratif.Core.Point> points)
               {
                   if (points.Count == 0) { return null; }
                   if (points.Count == 1) { return points.First(); }

                   double barycentreX = 0;
                   double barycentreY = 0;

                   foreach (Point pointTemp in points)
                   {
                       barycentreX += pointTemp.X;
                       barycentreY += pointTemp.Y;
                   }

                   barycentreX /= points.Count;
                   barycentreY /= points.Count;

                   return new ArcGisProEspaceCollaboratif.Core.Point(barycentreX, barycentreY);
               }
       */

        /// <summary>
        /// Calcule la distance entre deux points.
        /// </summary>
        /// <param name="point1">Le premier point d'extrémité.</param>
        /// <param name="point2">Le second point d'extrémité.</param>
        /// <returns>La distance entre les points <paramref name="point1"/> et <paramref name="point2"/>.</returns>
        /*        public static double Distance(MapPoint point1, MapPoint point2)
                {
                    return Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
                }
        */
        /// <summary>
        /// Calcule la distance entre deux points.
        /// </summary>
        /// <param name="point1">Le premier point d'extrémité.</param>
        /// <param name="point2">Le second point d'extrémité.</param>
        /// <returns>La distance entre les points <paramref name="point1"/> et <paramref name="point2"/>.</returns>
        /*       public static double Distance(ArcGisProEspaceCollaboratif.Core.Point point1, ArcGisProEspaceCollaboratif.Core.Point point2)
               {
                   return Math.Sqrt(Math.Pow(point2.Longitude - point1.Longitude, 2) + Math.Pow(point2.Latitude - point1.Latitude, 2));
               }
       */

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
        /// <param name="pointEntree">Le point dont il faut calculer son centroïde.</param>
        /// <returns>Le centroïde calculé depuis <paramref name="pointEntree"/>.</returns>
        /*       public static MapPoint Centroid(MapPoint pointEntree)
               {
                   if (pointEntree.IsEmpty)
                   {
                       return null;
                   }
                   return pointEntree;
               }
       */
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
        /// <param name="croquisEntree">Le croquis dont il faut calculer son centroïde.</param>
        /// <returns>Le centroïde calculé depuis <paramref name="croquisEntree"/>.</returns>  
 /*       public static MapPoint Centroid(ArcGisProEspaceCollaboratif.Core.Croquis croquisEntree)
        {
            // Si le croquis n'est pas défini
            if (croquisEntree.Type == ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Vide || croquisEntree.Points.Count == 0)
            { return null; }

            // Si le croquis se résume à un ponctuel
            if (croquisEntree.Type == ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Point || croquisEntree.Type == ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Texte)
            { return EspaceCollaboratifHelper.TransformPoint(croquisEntree.FirstCoord()); }

            return EspaceCollaboratifHelper.Centroid(EspaceCollaboratifHelper.CroquisToPolyline(croquisEntree));
        }
*/
        /// <summary>
        /// Calcule le centroïde de chaque croquis EspaceCollaboratif contenus dans une liste.  
        /// </summary>
        /// <param name="listCroquisEntree">La liste contenant les objects croquis EspaceCollaboratif dont il faut déterminer leur centroïde.</param>
        /// <returns>La liste des centroïdes calculés depuis les croquis contenus dans <paramref name="listCroquisEntree"/>.</returns>  
 /*       public static MapPoint Centroid(List<ArcGisProEspaceCollaboratif.Core.Croquis> listCroquisEntree)
        {
            switch (listCroquisEntree.Count)
            {
                case 0:
                    return null;

                case 1:
                    return EspaceCollaboratifHelper.Centroid(listCroquisEntree.First());

                default:

                    List<Point> listCentroid = new List<Point>();
                    foreach (ArcGisProEspaceCollaboratif.Core.Croquis croquis in listCroquisEntree)
                    {
                        listCentroid.Add(EspaceCollaboratifHelper.Centroid(croquis));
                    }

                    return EspaceCollaboratifHelper.Barycentre(listCentroid);
            }
        }
*/
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
 /*       public static bool IsFieldGoodForAttribut(string nomCalque, string nomChamp, System.Windows.Forms.TreeNode treeLayerAndField)
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
*/

        /// <summary>
        /// Ajoute des attributs à un croquis en fonction des propriétés de la feature donnée en entrée.
        /// </summary>
        /// <param name="croquis">Le croquis auquel on veut ajouter des attributs.</param>
        /// <param name="feature">La feature initiale contenant les champs qu'on veut mettre en attribut dans le croquis.</param>
        /// <param name="treeLayerAndField">L'arborescence des calques et de leurs champs devant être mis en attribut pour le nouveau croquis.</param>
        /// <returns>Le <paramref name="croquis"/> complété d'attributs supplémentaires issus des propriétés de la <paramref name="feature"/>.</returns>
/*        public static void AddAttributs(ref ArcGisProEspaceCollaboratif.Core.Croquis croquis, Feature feature, System.Windows.Forms.TreeNode treeLayerAndField)
        {    
            // Parcours des champs de la feature.
            for (int i = 0; i < feature.Fields.FieldCount; i++)
            {
                // Teste pour savoir si le champ de la feature est à mettre en croquis.
                if (EspaceCollaboratifHelper.IsFieldGoodForAttribut(feature.Class.AliasName, feature.Fields.Field[i].Name, treeLayerAndField))
                {                    
                   string  sAtt = System.Convert.ToString(feature.get_Value(i));

                   // Suppression de la partie heure 00:00:00 si le champ est de type date
                   if (feature.Fields.Field[i].Type == FieldType.Date && sAtt.Length != 0)
                   {
                       if (sAtt.Substring(sAtt.Length - 9).Equals(" 00:00:00"))
                       {
                           sAtt = sAtt.Substring(0, sAtt.Length - 9);
                       }
                   }

                   croquis.AddAttribut(feature.Fields.Field[i].Name, sAtt);
                }
            }
             
        }
*/

        /// <summary>
        /// Ajoute à un croquis l'attribut composé du nom et de la valeur donnés en entrée.
        /// </summary>
        /// <param name="croquis">Le croquis auquel on veut ajouter un attribut.</param>
        /// <param name="nom">Le nom de l'attribut à ajouter au croquis.</param>
        /// <param name="val">La valeur de l'attribut à ajouter au croquis.</param>
        /// <returns>Le <paramref name="croquis"/> complété de l'attribut supplémentaire issu de la paire <paramref name="nom"/> et <paramref name="val"/>.</returns>
/*        public static void AddAttributs(ref ArcGisProEspaceCollaboratif.Core.Croquis croquis, string nom, string val)
        {
            croquis.AddAttribut(new ArcGisProEspaceCollaboratif.Core.Attribut(nom, val));
        }
*/

        /// <summary>
        /// Génère un croquis EspaceCollaboratif à partir d'une géométrie Esri.
        /// La géométrie du nouveau croquis est celle issue de la conversion de la géométrie donnée en entrée.
        /// </summary>
        /// <param name="geometryEntree">La géométrie dont il faut convertir en croquis EspaceCollaboratif.</param>        
        /// <returns>Le croquis EspaceCollaboratif issu depuis <paramref name="geometryEntree"/>.</returns>
/*        public static ArcGisProEspaceCollaboratif.Core.Croquis MakeCroquis(Geometry geometryEntree)
        {
            ArcGisProEspaceCollaboratif.Core.Croquis newCroquis = new ArcGisProEspaceCollaboratif.Core.Croquis();

            // Selon le type géométrique de la géométrie à traiter.
            switch (geometryEntree.GeometryType)
            {
                case esriGeometryType.esriGeometryRing:
                    IRing ring = geometryEntree as IRing;
                    IPointCollection vertexRing = ring as IPointCollection;
                    newCroquis = EspaceCollaboratifHelper.PointCollectionToCroquis(vertexRing, ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Polygone);
                    break;

                case esriGeometryType.esriGeometryPath:
                    IPath path = geometryEntree as IPath;
                    IPointCollection vertexPath = path as IPointCollection;
                    newCroquis = EspaceCollaboratifHelper.PointCollectionToCroquis(vertexPath, ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Ligne);
                    break;

                case esriGeometryType.esriGeometryLine:
                    newCroquis.SetType(ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Ligne);
                    LineSegment ligne = geometryEntree as LineSegment;
                    newCroquis.AddPoint(EspaceCollaboratifHelper.TransformPoint(ligne.FromPoint));
                    newCroquis.AddPoint(EspaceCollaboratifHelper.TransformPoint(ligne.ToPoint));
                    break;

                case GeometryType.Point:
                    newCroquis.SetType(ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Point);
                    Point point = geometryEntree as Point;
                    newCroquis.AddPoint(EspaceCollaboratifHelper.TransformPoint(point));
                    break;

                case GeometryType.Polygon:
                    Polygon polygon = geometryEntree as Polygon;
                    IPointCollection vertexPolygon = polygon as IPointCollection;
                    newCroquis = EspaceCollaboratifHelper.PointCollectionToCroquis(vertexPolygon, ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Polygone);
                    break;

                case GeometryType.Polyline:
                    Polyline polyligne = geometryEntree as Polyline;
                    IPointCollection vertexPolyligne = polyligne as IPointCollection;
                    newCroquis = EspaceCollaboratifHelper.PointCollectionToCroquis(vertexPolyligne, ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Ligne);
                    break;
            }

            if (!newCroquis.IsValid()) { return new ArcGisProEspaceCollaboratif.Core.Croquis(); }
            return newCroquis;
        }
*/

        /// <summary>
        /// Calcule le point d'application pour une nouvelle remarque EspaceCollaboratif à partir des croquis associées à cette future remarque.
        /// On calcule le centroïde de chaque croquis EspaceCollaboratif contenu dans <paramref name="listCroquisEntree"/>, puis le barycentre de l'ensemble de ces centroïdes calculés et enfin on retient celui qui est le plus proche du barycentre calculé.
        /// </summary>
        /// <param name="listCroquisEntree">La liste contenant les croquis EspaceCollaboratif de la future remarque EspaceCollaboratifs.</param>        
        /// <returns>Le point sur lequel sera centrée la nouvelle remarque EspaceCollaboratif contenant les croquis de <paramref name="listCroquisEntree"/>.</returns>
/*        public static ArcGisProEspaceCollaboratif.Core.Point PointApplicationEspaceCollaboratif(List<ArcGisProEspaceCollaboratif.Core.Croquis> listCroquisEntree)
        {
            switch (listCroquisEntree.Count)
            {
                case 0:
                    return null;

                case 1:

                    ArcGisProEspaceCollaboratif.Core.Croquis croquisUn = listCroquisEntree.First();

                    if (croquisUn.Type == ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Point ||
                        croquisUn.Type == ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Texte)
                    {
                        return croquisUn.FirstCoord();
                    }
                    else
                    {
                        return EspaceCollaboratifHelper.TransformPoint(EspaceCollaboratifHelper.Centroid(croquisUn));
                    }

                default:
                    
                    ArcGIS.Core.Geometry.MapPoint barycentre = EspaceCollaboratifHelper.Centroid(listCroquisEntree);

                    List<double> distanceCroquis = new List<double>();
                    List<Point> centroidCroquis = new List<Point>();

                    foreach (ArcGisProEspaceCollaboratif.Core.Croquis croquis in listCroquisEntree)
                    {
                        Point ptTemp = new Point();

                        if (croquis.Type == ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Fleche || croquis.Type == ArcGisProEspaceCollaboratif.Core.Croquis.CroquisType.Ligne)
                        {
                            double dist1 = 0;
                            double dist2 = 0;
                            bool cote = false;
                            Polyline polyligne = EspaceCollaboratifHelper.CroquisToPolyline(croquis);
                            // On recherche le point situé sur la polyligne le plus proche du barycentre
                            polyligne.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, barycentre, true, ptTemp, dist1, dist2, cote);
                        }
                        else
                        {
                            ptTemp = EspaceCollaboratifHelper.Centroid(croquis);
                        }

                        centroidCroquis.Add(ptTemp);
                        distanceCroquis.Add(EspaceCollaboratifHelper.Distance(ptTemp, barycentre));
                    }

                    int rang = 0;

                    // Recherche du centroïde le plus proche du barycentre
                    for (int i = 1; i < distanceCroquis.Count; i++)
                    {
                        if (distanceCroquis[i] < distanceCroquis[rang])
                        {
                            rang = i;
                        }
                    }

                    return EspaceCollaboratifHelper.TransformPoint(centroidCroquis[rang]);
            }
        }
*/
        /// <summary>
        /// Calcule le point d'application pour une nouvelle remarque EspaceCollaboratif à partir d'un unique croquis EspaceCollaboratif associé à cette future remarque.
        /// Il s'agit du centroïde du croquis EspaceCollaboratif <paramref name="croquisEntree"/>.
        /// </summary>
        /// <param name="croquisEntree">Le croquis EspaceCollaboratif dont son centroïde sera le point d'application de la nouvelle remarque EspaceCollaboratif.</param>        
        /// <returns>Le point sur lequel sera centrée la nouvelle remarque EspaceCollaboratif contenant le croquis <paramref name="croquisEntree"/>.</returns>
/*        public static ArcGisProEspaceCollaboratif.Core.Point PointApplicationEspaceCollaboratif(ArcGisProEspaceCollaboratif.Core.Croquis croquisEntree)
        {
            List<ArcGisProEspaceCollaboratif.Core.Croquis> listCroquis = new List<ArcGisProEspaceCollaboratif.Core.Croquis>
            {
                croquisEntree
            };
            return EspaceCollaboratifHelper.PointApplicationEspaceCollaboratif(listCroquis);
        }
*/

        /// <summary>
        /// Affiche un message dans la barre d'état d'ArcMap.
        /// 25/01/2021 Impossible de changer la status bar dans ArcGIS pro
        /// https://community.esri.com/t5/arcgis-pro-sdk-questions/status-bar-customization/m-p/771445
        /// </summary>
        /// <param name="message">Le message à afficher dans la barre d'état d'ArcMap (en bas à gauche de l'écran.).</param>       
/*        public static void MessageBar(string message)
        {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, "Espace collaboratif");

            /*ArcGIS.Desktop.Framework.FrameworkApplication frameworkApplication;
            frameworkApplication.
            ESRI.ArcGIS.Framework.IApplication application = ArcMap.Application;

            IStatusBar mess = application.StatusBar;
            mess.set_Message(0, message);
            mess.Visible = true;

            return;
        }
*/

        /// <summary>
        /// Renvoie le chemin complet d'accès du fichier XML de paramétrage pour le fonctionnement de l'add-on EspaceCollaboratif pour ArcMap.
        /// Il doit être situé dans le même répertoire que celui où se trouve le fichier de la carte ouverte en cours dans ArcMap, et son nom est EspaceCollaboratif.xml.
        /// </summary>     
        /// <returns>Le chemin complet + nom du fichier du fichier de paramétrage.</returns>
        public static string EspaceCollaboratifXML_NameFile()
        {
            //IMapDocument mapDocument = ArcMap.Application.Document as IMapDocument;
            string workDir = Contexte.Instance.repertoireTravail;

            //return System.IO.Path.GetDirectoryName(mapDocument.DocumentFilename) + "\\" + EspaceCollaboratifHelper.nom_Fichier_Parametres_EspaceCollaboratif;
            return System.IO.Path.GetFullPath(workDir) + "\\" + EspaceCollaboratifHelper.nom_Fichier_Parametres_EspaceCollaboratif;
        }


        /// <summary>
        /// Teste la présence ou non du fichier XML de paramétrage nécéssaire au fonctionnement de l'add-on EspaceCollaboratif pour ArcMap.
        /// </summary>
        /// <returns>True si le fichier de paramétrage est présent, False dans le cas contraire.</returns>
        public static bool EspaceCollaboratifXML_TestFile()
        {
            return System.IO.File.Exists(EspaceCollaboratifHelper.EspaceCollaboratifXML_NameFile());
        }


        /// <summary>
        /// Charge en mémoire le fichier XML de paramétrage nécéssaire au fonctionnement de l'add-on EspaceCollaboratif.
        /// Cette opération est nécéssaire avant touts autres manipulations ultérieures (lecture ou écriture).
        /// </summary>
        /// <returns>Un object XmlDocument contenant le chargement du fichier XML de paramétrage EspaceCollaboratif.</returns>
        public static XmlDocument EspaceCollaboratifXML_Load()
        {
            string nom_FichierParametre = EspaceCollaboratifHelper.EspaceCollaboratifXML_NameFile();
            XmlDocument paramsXML = new XmlDocument();
            paramsXML.Load(nom_FichierParametre);        
            return paramsXML;
        }


        /// <summary>
        /// Teste la présence ou non  d'une balise à l'intérieur du fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap.
        /// </summary>
        /// <param name="baliseTest">Le chemin complet de la balise à rechercher à l'intérieur du fichier de paramétrage.</param>  
        /// <returns>True si le fichier de paramétrage contient la balise <paramref name="baliseTest"/>, False dans le cas contraire.</returns>
        public static bool EspaceCollaboratifXML_HasElement(string baliseTest)
        {
            XmlDocument paramsXML = EspaceCollaboratifHelper.EspaceCollaboratifXML_Load();
            XmlNode noeudTest = paramsXML.SelectSingleNode(baliseTest);

            return (noeudTest != null);
        }


        /// <summary>
        /// Efface à l'intérieur du fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap, tous les éléments inclus dans la balise indiquée en entrée.
        /// Dans le fichier de paramétrage, la balise est conservée, mais elle est vidée de ses éléments qui étaient inclus auparavant.
        /// </summary>
        /// <param name="balise">Le chemin complet de la balise à rechercher à l'intérieur du fichier de paramétrage, à l'intérieur de laquelle il faut effacer tous ses éléments.</param>       
        public static void EspaceCollaboratifXML_ClearElements(string balise)
        {
            XmlDocument paramsXML = EspaceCollaboratifHelper.EspaceCollaboratifXML_Load();
            XmlNode noeud = paramsXML.SelectSingleNode(balise);
            noeud.RemoveAll();
            paramsXML.Save(EspaceCollaboratifHelper.EspaceCollaboratifXML_NameFile());
        }


        /// <summary>
        /// Supprime à l'intérieur du fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap, la balise indiquée en entrée.
        /// La balise et ses éléments qui y sont invclus sont totalement supprimés du fichier de paramétrage. 
        /// </summary>
        /// <param name="balise">Le chemin complet de la balise à rechercher à l'intérieur du fichier de paramétrage pour la supprimer.</param>  
        public static void EspaceCollaboratifXML_RemoveElement(string balise)
        {
            XmlDocument paramsXML = EspaceCollaboratifHelper.EspaceCollaboratifXML_Load();
            XmlNode nodeToDelete = paramsXML.SelectSingleNode(balise);
            nodeToDelete.ParentNode.RemoveChild(nodeToDelete);
            paramsXML.Save(EspaceCollaboratifHelper.EspaceCollaboratifXML_NameFile());
        }


        /// <summary>
        /// Écrit à l'intérieur du fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap, la valeur donnée pour la balise indiquée.
        /// Si la balise donnée n'existe pas dans le fichier de paramétrage, elle est alors d'abord créée.
        /// </summary>
        /// <param name="balise">Le chemin complet de la balise à rechercher à l'intérieur du fichier de paramétrage dans laquelle on souhaite écrire sa valeur.</param> 
        /// <param name="valeur">La valeur à écrire pour la balise indiquée.</param> 
        public static void EspaceCollaboratifXML_SetElement(string balise, string valeur)
        {
            if (!EspaceCollaboratifHelper.EspaceCollaboratifXML_HasElement(balise))
            {
                EspaceCollaboratifHelper.EspaceCollaboratifXML_AddElement(balise);
            }

            XmlDocument paramsXML = EspaceCollaboratifHelper.EspaceCollaboratifXML_Load();
            XmlNode noeudRoot = paramsXML.SelectSingleNode(balise);
            noeudRoot.InnerText = valeur;
            paramsXML.Save(EspaceCollaboratifHelper.EspaceCollaboratifXML_NameFile());
        }


        /// <summary>
        /// Ajoute à l'intérieur du fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap, un nouveau noeud à l'emplacement indiqué.
        /// </summary>
        /// <param name="root">L'emplacement à l'intérieur du fichier de paramétrage où il faut ajouter le nouveau noeud.</param> 
        /// <param name="noeudNouveau">Le nom du nouveau noeud à ajouter.</param> 
        public static void EspaceCollaboratifXML_AddElement(string root, string noeudNouveau)
        {
            XmlDocument paramsXML = EspaceCollaboratifHelper.EspaceCollaboratifXML_Load();
            XmlNode noeudRoot = paramsXML.SelectSingleNode(root);
            XmlElement elem = paramsXML.CreateElement(noeudNouveau);
            noeudRoot.AppendChild(elem);
            paramsXML.Save(EspaceCollaboratifHelper.EspaceCollaboratifXML_NameFile());
        }
        /// <summary>
        /// Ajoute à l'intérieur du fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap, un nouveau noeud à l'emplacement indiqué.
        /// L'argument <paramref name="noeudNouveau"/> contient à la fois l'emplacement et le nom du nouveau noeud, le tout séparé par le dernier "/" contenu dans <paramref name="noeudNouveau"/>.
        /// </summary>        
        /// <param name="noeudNouveau">Le nom complet du nouveau noeud à ajouter.</param> 
        public static void EspaceCollaboratifXML_AddElement(string noeudNouveau)
        {
            EspaceCollaboratifHelper.EspaceCollaboratifXML_AddElement(EspaceCollaboratifHelper.EspaceCollaboratifXML_Prefixe(noeudNouveau),
                                              EspaceCollaboratifHelper.EspaceCollaboratifXML_Suffixe(noeudNouveau));
        }


        /// <summary>
        /// Extrait tout le texte précédant le dernier caractère "/" situé à l'intérieur du texte donné en entrée.      
        /// </summary>        
        /// <param name="balise">Le texte dans lequel on cherche l'emplacement du dernier caractère "/".</param> 
        /// <returns>Tout le texte situé avant le dernier caractère "/" contenu dans <paramref name="balise"/>.</returns>
        public static string EspaceCollaboratifXML_Prefixe(string balise)
        {
            return balise.Substring(0, balise.LastIndexOf("/"));
        }


        /// <summary>
        /// Extrait tout le texte succédant le dernier caractère "/" situé à l'intérieur du texte donné en entrée.      
        /// </summary>        
        /// <param name="balise">Le text dans lequel on cherche l'emplacement du dernier caractère "/".</param> 
        /// <returns>Tout le texte situé après le dernier caractère "/" contenu dans <paramref name="balise"/>.</returns>
        public static string EspaceCollaboratifXML_Suffixe(string balise)
        {
            return balise.Substring(balise.LastIndexOf("/") + 1);
        }


        /// <summary>
        /// Ajoute à l'emplacement indiqué, un nouvel élément et sa valeur associée dans le fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap.
        /// </summary> 
        /// <param name="root">L'emplacement dans le fichier paramètre où on veut ajouter le nouvel élément.</param>
        /// <param name="elementName">Le nom de l'élément à ajouter.</param>
        /// <param name="elementVal">La valeur de l'élément à ajouter.</param>
        public static void EspaceCollaboratifXML_InsertElement(string root, string elementName, string elementVal)
        {
            XmlDocument paramsXML = EspaceCollaboratifHelper.EspaceCollaboratifXML_Load();
            XmlNode noeudRoot = paramsXML.SelectSingleNode(root);

            XmlDocument newDoc = new XmlDocument();
            newDoc.LoadXml("<" + elementName + ">" + elementVal + "</" + elementName + ">");

            XmlNode noeud = paramsXML.ImportNode(newDoc.FirstChild, true);
            noeudRoot.AppendChild(noeud);
            paramsXML.Save(EspaceCollaboratifHelper.EspaceCollaboratifXML_NameFile());
        }


        /// <summary>
        /// Renvoie le premier élément indiqué l'intérieur du fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap.
        /// </summary>
        /// <param name="elementName">Le nom de l'élément à rechercher.</param>
        /// <returns>La valeur associée à l'élément <paramref name="element"/> s'il existe (vide en cas contraire).</returns>
        public static string EspaceCollaboratifXML_FirstElement(string element)
        {
            System.Xml.XmlDocument paramsEspaceCollaboratif = new System.Xml.XmlDocument();

            StreamReader streamXML = new StreamReader(EspaceCollaboratifHelper.EspaceCollaboratifXML_NameFile());
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
        /// Renvoie tous les éléments indiqués à l'intérieur du fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap.
        /// </summary>
        /// <param name="elementName">Le nom des éléments à rechercher.</param>
        /// <returns>La valeur associée de chaque élément ayant comme nom celui de <paramref name="element"/>.</returns>
        public static List<string> EspaceCollaboratifXML_ReadElement(string element)
        {
            List<string> liste = new List<string>();

            System.Xml.XmlDocument paramsEspaceCollaboratif = new System.Xml.XmlDocument();
            StreamReader streamXML = new StreamReader(EspaceCollaboratifHelper.EspaceCollaboratifXML_NameFile());
            paramsEspaceCollaboratif.Load(streamXML);
            System.Xml.XmlNodeList elemlist = paramsEspaceCollaboratif.DocumentElement.SelectNodes(element);
            streamXML.Close();

            for (int i = 0; i < elemlist.Count; i++)
            {
                liste.Add(elemlist[i].InnerText);
            }

            return liste;
        }


        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage de l'add-on, une liste d'éléments à un emplacement donné.
        /// </summary>
        /// <param name="listeElement">La liste contenant les éléments à stocker dans le fichier de paramétrage.</param>
        /// <param name="chemin">Le chemin d'accès au sein du fichier de paramétrage où il faut ajouter les éléments.</param>
        public static void EspaceCollaboratifXML_WriteElement(List<string> listeElement, string chemin)
        {
            if (chemin.Length == 0) { return; }
            string pathElement = EspaceCollaboratifHelper.EspaceCollaboratifXML_Prefixe(chemin);

            if (EspaceCollaboratifHelper.EspaceCollaboratifXML_HasElement(pathElement))
            {
                EspaceCollaboratifHelper.EspaceCollaboratifXML_ClearElements(pathElement);
            }
            else
            {
                EspaceCollaboratifHelper.EspaceCollaboratifXML_AddElement(pathElement);
            }

            foreach (string element in listeElement)
            {
                EspaceCollaboratifHelper.EspaceCollaboratifXML_InsertElement(pathElement,
                                                      EspaceCollaboratifHelper.EspaceCollaboratifXML_Suffixe(chemin),
                                                      element);
            }
        }        


        /// <summary>
        /// Renvoie la liste des thèmes préférés contenus à l'intérieur du fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap.
        /// </summary>
        /// <returns>La liste des noms de thèmes préférés contenus dans le fichier de paramétrage.</returns>
        public static List<string> Load_PreferedThemes()
        {
            return EspaceCollaboratifHelper.EspaceCollaboratifXML_ReadElement(EspaceCollaboratifHelper.xml_Themes);
        }
        /// <summary>
        /// Sauvegarde les thèmes préférés dans le fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap.
        /// </summary>
        /// <param name="preferedThemes">La liste des thèmes préférés à sauvegarder dans le fichier de paramétrage.</param>
        public static void Save_PreferedThemes(List<String> preferedThemes)
        {
            EspaceCollaboratifHelper.EspaceCollaboratifXML_WriteElement(preferedThemes, EspaceCollaboratifHelper.xml_Themes);
        }
        /// <summary>
        /// Sauvegarde les thèmes préférés dans le fichier XML de paramétrage de l'add-on EspaceCollaboratif pour ArcMap.
        /// </summary>
        /// <param name="preferedThemes">La liste des thèmes préférés à sauvegarder dans le fichier de paramétrage.</param>
        public static void Save_PreferedThemes(List<ArcGisProEspaceCollaboratif.Core.Theme> preferedThemes)
        {
            List<String> ListThemes = new List<string>();

            foreach (ArcGisProEspaceCollaboratif.Core.Theme theme in preferedThemes)
            {
                ListThemes.Add(theme.Groupe.Nom);
            }

            EspaceCollaboratifHelper.Save_PreferedThemes(ListThemes);
        }


        /// <summary>
        /// Lit le login par défaut à utiliser pour se connecter au service EspaceCollaboratif contenu dans le fichier XML de paramétrage.
        /// </summary>
        /// <returns>Le login à utiliser par défaut pour se connecter au service EspaceCollaboratif.</returns>
        public static string Load_Login()
        {
            return EspaceCollaboratifHelper.EspaceCollaboratifXML_FirstElement(EspaceCollaboratifHelper.xml_Login);
        }
        /// <summary>
        ///  Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, le login à utiliser pour se connecter au service EspaceCollaboratif.
        /// </summary>
        /// <param name="login">Le login par défaut à sauvegarder dans le fichier de paramétrage.</param>
        public static void Save_Login(string login)
        {
            if (!EspaceCollaboratifHelper.EspaceCollaboratifXML_HasElement(EspaceCollaboratifHelper.xml_Login))
            {
                EspaceCollaboratifHelper.EspaceCollaboratifXML_AddElement(EspaceCollaboratifHelper.xml_Login);
            }

            EspaceCollaboratifHelper.EspaceCollaboratifXML_SetElement(EspaceCollaboratifHelper.xml_Login, login);
        }


        /// <summary>
        /// Lit depuis le fichier de paramétrage XML EspaceCollaboratif, la taille de la pagination pour l'importation des remarques.
        /// </summary>
        /// <returns>La taille de pagination contenue dans le fichier de paramétrage. Renvoie 0 si cette valeur est absente.</returns>
        public static int Load_Pagination()
        {
            string pagination = EspaceCollaboratifHelper.EspaceCollaboratifXML_FirstElement(EspaceCollaboratifHelper.xml_Pagination);

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
            if (!EspaceCollaboratifHelper.EspaceCollaboratifXML_HasElement(EspaceCollaboratifHelper.xml_Pagination))
            {
                EspaceCollaboratifHelper.EspaceCollaboratifXML_AddElement(EspaceCollaboratifHelper.xml_Pagination);
            }

            EspaceCollaboratifHelper.EspaceCollaboratifXML_SetElement(EspaceCollaboratifHelper.xml_Pagination, pagination.ToString());
        }
        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, la taille par défaut de la pagination pour l'importation des remarques.
        /// </summary>
        public static void Save_Pagination()
        {
            EspaceCollaboratifHelper.Save_Pagination(0);
        }




        /// <summary>
        /// Lit la valeur du tag "Import_pour_groupe" du fichier XML de paramétrage.
        /// </summary>
        /// <returns></returns>
        public static string Load_Group()
        {
            return EspaceCollaboratifHelper.EspaceCollaboratifXML_FirstElement(EspaceCollaboratifHelper.xml_Group);
        }
        /// <summary>
        ///  Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, "Import_pour_groupe"
        /// </summary>
        /// <param name="group"> true ou false</param>
        public static void Save_Group(string group)
        {
            if (!EspaceCollaboratifHelper.EspaceCollaboratifXML_HasElement(EspaceCollaboratifHelper.xml_Group))
            {
                EspaceCollaboratifHelper.EspaceCollaboratifXML_AddElement(EspaceCollaboratifHelper.xml_Group);
            }

            EspaceCollaboratifHelper.EspaceCollaboratifXML_SetElement(EspaceCollaboratifHelper.xml_Group, group);
        }
        

        /// <summary>
        /// Lit depuis le fichier de paramétrage XML EspaceCollaboratif, la date pour laquelle on extrait que les remaques postérieures à celle-ci.
        /// </summary>
        /// <returns>La date d'extration stockée dans le fichier de paramétrage.</returns>
        public static System.DateTime Load_DateExtraction()
        {
            string dateExtration = EspaceCollaboratifHelper.EspaceCollaboratifXML_FirstElement(EspaceCollaboratifHelper.xml_DateExtraction);

            try
            {
                if (dateExtration.Length != 0)
                {
                    return Convert.ToDateTime(dateExtration);
                }
                else
                {
                    return Convert.ToDateTime(EspaceCollaboratifHelper.dateDefaut);
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("La date limite d'extraction contenue dans fichier XML de paramétrage n'est pas de forme valide.\n\nDate limite d'extraction = ''" + dateExtration + "''.", "IGN EspaceCollaboratif", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return Convert.ToDateTime(EspaceCollaboratifHelper.dateDefaut);
            }
        }
        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, la date d'extraction pour l'importation des remarques.
        /// </summary>
        /// <param name="date">La date d'extraction à enregistrer dans le fichier de paramétrage.</param>
        public static void Save_DateExtraction(System.DateTime date)
        {
            if (!EspaceCollaboratifHelper.EspaceCollaboratifXML_HasElement(EspaceCollaboratifHelper.xml_DateExtraction))
            {
                EspaceCollaboratifHelper.EspaceCollaboratifXML_AddElement(EspaceCollaboratifHelper.xml_DateExtraction);
            }

            EspaceCollaboratifHelper.EspaceCollaboratifXML_SetElement(EspaceCollaboratifHelper.xml_DateExtraction, date.ToString());
        }
        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, la date du jour comme date d'extraction.
        /// </summary>
        public static void Save_DateExtraction()
        {
            EspaceCollaboratifHelper.Save_DateExtraction(System.DateTime.Now);
        }


        /// <summary>
        /// Obtient à partir du fichier XML de paramétrage, le nom du calque à utiliser pour le filtrage spatial de l'importation des remarques.
        /// </summary>
        /// <returns>Le nom du calque pour le filtrage spatiale stocké dans le fichier de paramétrage.</returns>
        public static string Load_CalqueFiltrage()
        {
            return EspaceCollaboratifHelper.EspaceCollaboratifXML_FirstElement(EspaceCollaboratifHelper.xml_Zone_extraction);
        }
        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, le nom du calque à utiliser pour le filtrage spatial lors l'importation des remarques.
        /// </summary>
        /// <param name="layer">Le nom du calque à enregistrer da   ns le fichier de paramétrage pour le filtrage spatiale.</param>
        public static void Save_CalqueFiltrage(string layer)
        {
            EspaceCollaboratifXML_SetElement(EspaceCollaboratifHelper.xml_Zone_extraction, layer);
        }
        /// <summary>
        /// Sauvegarde dans le fichier XML de paramétrage EspaceCollaboratif, le nom du calque à utiliser pour le filtrage spatial lors l'importation des remarques.
        /// </summary>
        /// <param name="layer">Le calque dont on enregistre son nom dans le fichier de paramétrage pour le filtrage spatiale.</param>
/*        public static void Save_CalqueFiltrage(ILayer layer)
        {
            EspaceCollaboratifXML_SetElement(EspaceCollaboratifHelper.xml_Zone_extraction, layer.Name);
        }
*/

        /// <summary>
        /// Obtient à partir du XML de paramétrage, l'adresse du service EspaceCollaboratif contenue dans le fichier XML de paramétrage.
        /// </summary>
        /// <returns>L'adresse d'accès au service EspaceCollaboratif stockée dans le fichier de paramétrage EspaceCollaboratif.</returns>
        public static string Load_Urlhost()
        {
            string Urlhost = EspaceCollaboratifHelper.EspaceCollaboratifXML_FirstElement(EspaceCollaboratifHelper.xml_UrlHost);

            if (Urlhost.Equals(""))
            {
                System.Windows.Forms.MessageBox.Show("Impossible de trouver l'adresse du service EspaceCollaboratif dans le fichier XML de paramétrage.", "IGN EspaceCollaboratif", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
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
            EspaceCollaboratifXML_SetElement(EspaceCollaboratifHelper.xml_UrlHost, UrlHost);
        }


        /// <summary>
        /// Lit l'option d'affichage des calques croquis EspaceCollaboratif dans le fichier XML de paramétrage EspaceCollaboratif.
        /// </summary>
        /// <returns>True si le fichier de paramétrage contient explicitement qu'il faut afficher les croquis.</returns>
        public static bool Load_AfficherCroquis()
        {
            if (!EspaceCollaboratifHelper.EspaceCollaboratifXML_HasElement(EspaceCollaboratifHelper.xml_AfficherCroquis))
            {
                EspaceCollaboratifHelper.Save_AfficherCroquis();
                return true;
            }

            string option = EspaceCollaboratifHelper.EspaceCollaboratifXML_FirstElement(EspaceCollaboratifHelper.xml_AfficherCroquis);
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
                EspaceCollaboratifXML_SetElement(EspaceCollaboratifHelper.xml_AfficherCroquis, "Oui");
            }
            else
            {
                EspaceCollaboratifXML_SetElement(EspaceCollaboratifHelper.xml_AfficherCroquis, "Non");
            }
        }


        /// <summary>
        ///Force dans le fichier XML de paramétrage EspaceCollaboratif, la valeur True pour afficher les croquis EspaceCollaboratif.
        /// </summary>
        public static void Save_AfficherCroquis()
        {
            EspaceCollaboratifHelper.Save_AfficherCroquis(true);
        }


        /// <summary>
        /// Renvoie depuis le fichier XML de paramétrage, la liste des calques et de leurs champs qu'il faut mettre en attribut dans la génération des croquis EspaceCollaboratif.
        /// </summary>
        /// <returns> Les calques et leurs champs sous forme d'un System.Windows.Forms.TreeView </returns>
        public static System.Windows.Forms.TreeNode Load_AttributsCroquis()
        {
            System.Windows.Forms.TreeNode attributs = new System.Windows.Forms.TreeNode();
            XmlDocument paramsEspaceCollaboratif = EspaceCollaboratifHelper.EspaceCollaboratifXML_Load();              

            // Recherche des nœuds pour les calques. 
            System.Xml.XmlNodeList elemlist = paramsEspaceCollaboratif.DocumentElement.SelectNodes(EspaceCollaboratifHelper.xml_AttributsCroquis);
            if (elemlist.Count == 0) { return attributs; }

            for (int numElemList = 0; numElemList < elemlist.Count; numElemList++)
            {
                System.Xml.XmlNode nodeCalque = elemlist.Item(numElemList).SelectSingleNode(EspaceCollaboratifHelper.xml_BaliseNomCalque);

                // S'il y a bien la balise pour le calque.
                if (nodeCalque != null)
                {
                    System.Xml.XmlNodeList listNodeChamps = elemlist.Item(numElemList).SelectNodes(EspaceCollaboratifHelper.xml_BaliseChampCalque);
                    
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
            XmlDocument paramsXML = EspaceCollaboratifHelper.EspaceCollaboratifXML_Load();
            XmlNode nodeToDelete = paramsXML.SelectSingleNode(  EspaceCollaboratifHelper.xml_AttributsCroquis );

            // Suppresion préalable des éventuels anciens attributs stockés dans le fichier de paramétrage.
            while ( nodeToDelete != null )
            { 
                nodeToDelete.ParentNode.RemoveChild(nodeToDelete);
                nodeToDelete = paramsXML.SelectSingleNode(EspaceCollaboratifHelper.xml_AttributsCroquis);
            }

            // Parcours des nœuds de l'object attributs.
            foreach (System.Windows.Forms.TreeNode attributCalque in attributs.Nodes)
            {   // Si le nœuds contient des sous-éléments
                if (attributCalque.Nodes.Count != 0 && attributCalque.Text.Length != 0)
                {
                    XmlNode noeudRoot = paramsXML.SelectSingleNode(EspaceCollaboratifHelper.EspaceCollaboratifXML_Prefixe(EspaceCollaboratifHelper.xml_AttributsCroquis));
                    XmlElement elemCalque = paramsXML.CreateElement(EspaceCollaboratifHelper.EspaceCollaboratifXML_Suffixe(EspaceCollaboratifHelper.xml_AttributsCroquis));
                    
                    // Stockage du nom du calque à retenir.
                    XmlElement nomCalque = paramsXML.CreateElement(EspaceCollaboratifHelper.xml_BaliseNomCalque);
                    nomCalque.InnerText = attributCalque.Text;
                    elemCalque.AppendChild(nomCalque);

                    // Parcours des sous-éléments qui contiennent les champs à stocker.
                    foreach (System.Windows.Forms.TreeNode attribut in attributCalque.Nodes)
                    {
                        XmlElement elementChamp = paramsXML.CreateElement(EspaceCollaboratifHelper.xml_BaliseChampCalque);
                        elementChamp.InnerText = attribut.Text;
                        elemCalque.AppendChild(elementChamp);
                    }
                    
                    // Enregistrement dans le fichier de paramétrage du nom et des champs du calque à mettre en attribut.
                    noeudRoot.AppendChild(elemCalque);
                }
            }

            // Fermeture du fichier XML.
            paramsXML.Save(EspaceCollaboratifHelper.EspaceCollaboratifXML_NameFile()); 
        }


    }
    
}
