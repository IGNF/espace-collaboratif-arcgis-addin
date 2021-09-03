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
                    string error = "Il faut sélectionner un et un seul signalement";
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
                    FeatureLayer reportLayer = context.GetLayerByName(Helper.name_layer_Signalement);
                    var selectedFeatures = reportLayer.GetSelection();
                    if (selectedFeatures.GetCount() != 1)
                    {
                        throw new Exception(error);
                    }

                    Dictionary<string, string> attributes = new Dictionary<string, string>();

                    List<FieldDescription> fieldDescription = reportLayer.GetFieldDescriptions();
                    IReadOnlyList<long> lOid = selectedFeatures.GetObjectIDs();
                    foreach (long oid in lOid)
                    {
                        {
                            var inspector = reportLayer.Inspect(oid);
                            attributes = Helper.GetAttributes(inspector, fieldDescription);

                            Geometry geometry = inspector.Shape;
                            MapPoint point = geometry as MapPoint;
                            attributes.Add(Helper.name_field_Longitude, Math.Round(point.X, 5).ToString());
                            attributes.Add(Helper.name_field_Latitude, Math.Round(point.Y, 5).ToString());
                        }
                    }
                    
                   
                    // Si le dictionnaire attributes est vide, c'est qu'il n'y a pas de signalement sélectionné
                    if (attributes.Count == 0)
                    {
                        throw new Exception(error);
                    }

                    // Ouverture de la boite "SeeReport" s'il y a un et un seul signalement sélectionné
                    // TODO Eric : faire défiler les fiches de tous les signalements sélectionnés
                    // http://sd-redmine.ign.fr/issues/15041 
                    SeeReportViewModel seeReportViewModel = new SeeReportViewModel(context, attributes);
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
                    if (e.Message == Constantes.OPERATIONANNULEE)
                    {
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                            e.Message,
                            Constantes.INFORMATION
                        );
                    }
                    else
                    {
                        string message = string.Format("Problème dans l'affichage de la fiche d'un signalement : {0}\n{1}", e.Message, e.StackTrace);
                        logger.Error(string.Format("SeeReport.OnClick : {0}\n", message));
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                            e.Message,
                            Constantes.ERROR
                        );
                    }
                }
            });
        }
    }
}
