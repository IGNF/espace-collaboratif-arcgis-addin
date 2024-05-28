using System;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGisProEspaceCollaboratif.Core;
using log4net;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGisProEspaceCollaboratif.ViewModels;
using System.Collections.Generic;
using ArcGIS.Desktop.Internal.Framework;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;
//using System.Windows.Forms;

namespace ArcGisProEspaceCollaboratif
{
    internal class CreateReport : Button
    {
        private static readonly Logger riplogger = Logger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(CreateReport));
        private Context context = Context.Instance;

        protected override async void OnClick()
        {
            logger.Debug("Click sur le bouton de création d'un nouveau signalement");
            await QueuedTask.Run(() =>
            {
                ArcGIS.Desktop.Framework.Threading.Tasks.ProgressDialog progressDialog = new ProgressDialog("Création des signalements et croquis dans la carte...");
                progressDialog.Show();
                try
                {
                    //Context context = Context.Instance;

                    // Il faut s'être connecté au service pour créer un signalement
                    if (context.Client == null)
                    {
                        // Établissement de la connexion avec le service Espace collaboratif.
                        ArcGisProEspaceCollaboratif.Core.Client client = null;
                        context.GetConnexionEspaceCollaboratif(ref client);
                        context.Client = client;
                        if (context.Client == null)
                        {
                            return;
                        }
                    }

                    // Est-ce que la couche signalement existe dans la carte ?
                    bool bRes = context.IsLayerInMap(Helper.name_layer_Signalement);
                    if (!bRes)
                    {
                        string mess = "Pas de couche 'Signalement' dans la carte.\nIl est donc impossible de créer un nouveau signalement.\nIl faut se connecter à l'Espace collaboratif et télécharger les signalements.";
                        logger.Error(string.Format("CreateReport.OnClick.context.IsLayerInMap : {0}\n", mess));
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                            mess,
                            Constantes.ERROR);
                        return;
                    }

                    // Transformation des objets sélectionnés en croquis.
                    string message = "";
                    List<ArcGisProEspaceCollaboratif.Core.Sketch> futursSketch = MakeSketchFromSelection();
                    if (futursSketch == null)
                    {
                        message = "Arrêt demandé par l'utilisateur, certains croquis n'ont pu être créés";
                        progressDialog.Hide();
                        return;
                    }
                    logger.Debug(futursSketch.Count + " croquis générés.");
                    if (futursSketch.Count == 0)
                    {
                        message = "Aucun objet sélectionné.\nIl est donc impossible de déterminer le point d'application du nouveau signalement à créer.";
                        progressDialog.Hide();
                        return;
                    }
                    if (!string.IsNullOrEmpty(message))
                    {
                        logger.Error(string.Format("CreateReport.OnClick.context.MakeSketchFromSelection : {0}\n", message));
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                            message,
                            Constantes.WARNING
                        );
                        progressDialog.Hide();
                        return;
                    }

                    // Lancement du formulaire pour créer un nouveau signalement
                    CreateReportViewModel createReportViewModel = new (context, futursSketch);
                    createReportViewModel.createReportView.DataContext = createReportViewModel;
                    bool? dialogResult = createReportViewModel.createReportView.ShowDialog();
                    // Si l'utilisateur a cliqué sur le bouton "Annuler"
                    // dans son choix du groupe, on sort
                    if (dialogResult == false)
                    {
                        createReportViewModel.createReportView.Close();
                        progressDialog.Hide();
                        return;
                    }
                    progressDialog.Hide();
                }
                catch (Exception e)
                {  
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                        e.Message,
                        Constantes.ERROR
                    );
                    string message = string.Format("Problème dans la création des signalements : {0}\n{1}", e.Message, e.StackTrace);
                    logger.Error(string.Format("CreateReport.OnClick : {0}\n", message));
                    progressDialog.Hide();
                }
            });
        }

        /// <summary>
        /// Transforme en croquis les objets sélectionnés dans la carte en cours.
        /// </summary>
        /// <returns>Liste de croquis créés à partir des objects sélectionnés.</returns>
        public List<ArcGisProEspaceCollaboratif.Core.Sketch> MakeSketchFromSelection()
        {
            List<ArcGisProEspaceCollaboratif.Core.Sketch> sketches = new();

            if (context.MapActiveView == null)
            {
                return sketches;
            }

            string message = "";
            // Get the currently selected features in the map
            QueuedTask.Run(() =>
            {
                SelectionSet selectedFeatures = context.GetMap().GetSelection();
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
                            // Initialisation de croquis avec la géométrie de l'objet
                            var inspector = featureLayer.Inspect(oid);
                            ArcGIS.Core.Geometry.Geometry geometryFeature = inspector.Shape;
                            List<ArcGisProEspaceCollaboratif.Core.Sketch> tmpSketches = MakeSketch(geometryFeature);
                            foreach (Sketch tmpSketch in tmpSketches)
                            {
                                if (tmpSketch.Points.Count == 0)
                                {
                                    message += string.Format("Le croquis pour le signalement {0} n'a pu être créé.\n", oid);
                                }
                                else
                                {
                                    // Ajout des attributs au nouveau croquis
                                    Dictionary<string, string> attributes = Helper.GetAttributes(inspector, fieldDescription);
                                    foreach (KeyValuePair<string, string> kv in attributes)
                                    {
                                        tmpSketch.AddAttribute(kv.Key, kv.Value);
                                    }
                                    sketches.Add(tmpSketch);
                                }
                            }
                        });
                    }
                }
            });
            // Si le message n'est pas vide, c'est qu'il y a des problèmes dans la création des croquis
            // Il faut demander à l'utilisateur s'il veut continuer
            if (!string.IsNullOrEmpty(message))
            {
                message += "Voulez-vous continuer ?";
                System.Windows.MessageBoxResult result = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.QUESTION, System.Windows.MessageBoxButton.YesNo);
                if (result == System.Windows.MessageBoxResult.Cancel ||
                    result == System.Windows.MessageBoxResult.No ||
                    result == System.Windows.MessageBoxResult.None)
                {
                    return null;
                }
            }
            return sketches;
        }

        /// <summary>
        /// Génère un croquis pour l'Espace collaboratif à partir d'une géométrie ArcGIS pro.
        /// La géométrie du nouveau croquis est celle issue de la conversion de la géométrie donnée en entrée.
        /// </summary>
        /// <param name="geometry">La géométrie qu'il faut convertir en croquis de l'Espace collaboratif</param>
        /// <returns>Le croquis pour l'Espace collaboratif</returns>
        private static List<ArcGisProEspaceCollaboratif.Core.Sketch> MakeSketch(ArcGIS.Core.Geometry.Geometry geometry)
        {
            if (geometry == null)
            {
                return null;
            }
            List<ArcGisProEspaceCollaboratif.Core.Sketch> sketches = new();
            //

            // Selon le type géométrique de la géométrie à traiter.
            switch (geometry.GeometryType)
            {
                case GeometryType.Point:
                    ArcGisProEspaceCollaboratif.Core.Sketch newSketchPt = new();
                    newSketchPt.SetType(Sketch.SketchType.Point);
                    MapPoint point = geometry as MapPoint;
                    newSketchPt.AddPoint(Helper.ReplaceSpatialReferenceToPoint(point));
                    sketches.Add(newSketchPt);
                    break;

                case GeometryType.Multipoint:
                    var multiPoint = geometry as ArcGIS.Core.Geometry.Multipoint;
                    foreach (MapPoint pt in multiPoint.Points)
                    {
                        ArcGisProEspaceCollaboratif.Core.Sketch newSketchMultiPt = new();
                        newSketchMultiPt.SetType(Sketch.SketchType.Point);
                        newSketchMultiPt.AddPoint(Helper.ReplaceSpatialReferenceToPoint(pt));
                        sketches.Add(newSketchMultiPt);
                    }
                    break;

                case GeometryType.Polyline:
                    ArcGIS.Core.Geometry.Polyline polyline = geometry as ArcGIS.Core.Geometry.Polyline;
                    int nbParts = polyline.PartCount;
                    if (nbParts > 1)
                    {
                        foreach (ReadOnlySegmentCollection part in polyline.Parts)
                        {
                            ArcGisProEspaceCollaboratif.Core.Sketch newSketchMultiPolyline = new();
                            foreach (Segment segment in part)
                            {   
                                newSketchMultiPolyline.SetType(Sketch.SketchType.Ligne);
                                newSketchMultiPolyline.AddPoint(Helper.ReplaceSpatialReferenceToPoint(segment.StartPoint));
                            }
                            sketches.Add(newSketchMultiPolyline);
                        }
                    }
                    else
                    {
                        ArcGisProEspaceCollaboratif.Core.Sketch newSketchPolyline = new();
                        newSketchPolyline.SetType(Sketch.SketchType.Ligne);
                        foreach (MapPoint mapPoint in polyline.Points)
                        {
                            newSketchPolyline.AddPoint(Helper.ReplaceSpatialReferenceToPoint(mapPoint));
                        }
                        sketches.Add(newSketchPolyline);
                    } 
                    break;

                case GeometryType.Polygon:
                    ArcGIS.Core.Geometry.Polygon polygon = geometry as ArcGIS.Core.Geometry.Polygon;
                    int partCount = polygon.PartCount;
                    if (partCount > 1){
                        for (int idx = 0; idx < partCount; idx++)
                        {
                            bool isExteriorRing = polygon.IsExteriorRing(idx);
                            if (!isExteriorRing)
                            {
                                continue;
                            }
                            Polygon extRing = polygon.GetExteriorRing(idx);
                            ArcGisProEspaceCollaboratif.Core.Sketch newSketchExtRing = new();
                            newSketchExtRing.SetType(Sketch.SketchType.Polygone);
                            foreach (MapPoint mp in extRing.Points)
                            {
                                newSketchExtRing.AddPoint(Helper.ReplaceSpatialReferenceToPoint(mp));
                            }
                            sketches.Add(newSketchExtRing);
                        }
                    }
                    else
                    {
                        ArcGisProEspaceCollaboratif.Core.Sketch newSketchPolygon = new();
                        newSketchPolygon.SetType(Sketch.SketchType.Polygone);
                        foreach (MapPoint mp in polygon.Points)
                        {
                            newSketchPolygon.AddPoint(Helper.ReplaceSpatialReferenceToPoint(mp));
                        }
                        sketches.Add(newSketchPolygon);
                    }
                    break;

                default:
                    string message = string.Format("Géométrie ({0}) non prise en charge pour la transformer en croquis. Les types de géométrie acceptés sont : Point, Multipoint, Polyligne et Polygone.", geometry.GeometryType.ToString());
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                        message,
                        Constantes.WARNING
                    );
                    logger.Warn(message);
                    break;
            }

            return sketches;
        }
    }
}
