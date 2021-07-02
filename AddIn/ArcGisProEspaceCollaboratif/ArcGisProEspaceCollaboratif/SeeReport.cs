using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGisProEspaceCollaboratif.Core;
using ArcGisProEspaceCollaboratif.ViewModels;
using log4net;
using System;
using System.Collections.Generic;

namespace ArcGisProEspaceCollaboratif
{
    internal class SeeReport : Button
    {
        private static readonly Logger riplogger = Logger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(SeeReport));

        protected override async void OnClick()
        {
            logger.Debug("Click sur le bouton d'affichage de la fiche d'un signalement");
            await QueuedTask.Run(() =>
            {
                try
                {
                    Context context = Context.Instance;

                    // Il faut s'être connecté au service pour répondre à un signalement
                    if (context.Client == null)
                    {
                        // Établissement de la connexion avec le service Espace collaboratif.
                        context.Client = context.GetConnexionEspaceCollaboratif();
                        if (context.Client == null)
                        {
                            return;
                        }
                    }

                    // L'utilisateur a t'il sélectionné un et un seul signalement ?
                    // TODO voir avec Noémie : attention quand on ajoute des objets à la sélection celle-ci ne se rafraichit, il garde en mémoire la 1ère
                    var selectedFeatures = context.MapActiveView.Map.GetSelection();
                    if (selectedFeatures.Count != 1)
                    {
                        throw new Exception("Il faut sélectionner un et un seul signalement");
                    }
                    Dictionary<string, string> attributes = new Dictionary<string, string>();
                    foreach (KeyValuePair<MapMember, List<long>> kvp in selectedFeatures)
                    {
                        if (kvp.Key.Name != Helper.name_layer_Signalement)
                        {
                            continue;
                        }

                        var featureLayer = kvp.Key as FeatureLayer;
                        List<FieldDescription> fieldDescription = featureLayer.GetFieldDescriptions();
                        List<long> lOid = kvp.Value;
                        foreach (long oid in lOid)
                        {
                            QueuedTask.Run(() =>
                            {
                                var inspector = featureLayer.Inspect(oid);
                                attributes = Helper.GetAttributes(inspector, fieldDescription);

                                Geometry geometry = inspector.Shape;
                                MapPoint point = geometry as MapPoint;
                                attributes.Add(Helper.name_field_Longitude, Math.Round(point.X, 5).ToString());
                                attributes.Add(Helper.name_field_Latitude, Math.Round(point.Y, 5).ToString());
                            });
                        }
                    }
                    
                    // Ouverture de la boite "SeeReport" s'il y a un et un seul signalement sélectionné
                    var seeReportViewModel = new SeeReportViewModel(context, attributes);
                    seeReportViewModel.seeReportView.DataContext = seeReportViewModel;
                    bool? dialogResult = seeReportViewModel.seeReportView.ShowDialog();
                    // L'utilisateur a cliqué sur la croix pour fermer le dialogue
                    if (dialogResult == false)
                    {
                        seeReportViewModel.seeReportView.Close();
                        return;
                    }
                }
                catch (Exception e)
                {
                    string message = string.Format("Problème dans l'affichage de la fiche d'un signalement : {0}\n{1}", e.Message, e.StackTrace);
                    logger.Error(string.Format("SeeReport.OnClick : {0}\n", message));
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                        e.Message,
                        Constantes.ERROR
                    );
                }
            });
        }
    }
}
