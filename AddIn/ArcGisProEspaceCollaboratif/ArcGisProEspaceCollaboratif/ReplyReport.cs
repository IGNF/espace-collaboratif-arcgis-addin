using System;
using System.Collections.Generic;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGisProEspaceCollaboratif.Core;
using ArcGisProEspaceCollaboratif.ViewModels;
using log4net;

namespace ArcGisProEspaceCollaboratif
{
    internal class ReplyReport : Button
    {
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(ReplyReport));
        protected override async void OnClick()
        {
            logger.Debug("Click sur le bouton de réponse à un signalement");
            await QueuedTask.Run(() =>
            {
                try
                {
                    Context context = Context.Instance;

                    // Il faut s'être connecté au service pour répondre à un signalement
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
                    if (!context.IsLayerInMap(Helper.name_layer_Signalement))
                    {
                        string mess = "Pas de couche 'Signalement' dans la carte.\nIl est donc impossible de répondre à un signalement.\nIl faut se connecter à l'Espace collaboratif et télécharger les signalements.";
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                            mess,
                            Constantes.ERROR
                        );
                        logger.Debug(mess);
                        return;
                    }

                    // Liste des signalements valides auxquels l'utilisateur peut répondre
                    List<Report> replyReports = new ();
                    string messageReportNoValid = "";
                    // Récupération des objets sélectionnés
                    SelectionSet selectedFeatures = context.GetMap().GetSelection();
                    foreach (KeyValuePair<MapMember, List<long>> kvp in selectedFeatures.ToDictionary())
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
                                Dictionary<string, string> attributes = Helper.GetAttributes(inspector, fieldDescription);
                                string IdReport = attributes[Constantes.N_REPORT_IN_GDB];
                                Report report = context.Client.GetGeoRem(IdReport);

                                // Le statut du signalement est-il cloturé ?
                                int pos = Array.IndexOf(Status.OpenStatut, report.Status);
                                if (pos == -1)
                                {
                                    messageReportNoValid += string.Format("Impossible de répondre au signalement n°{0}, car il est clôturé depuis le {1}\n", IdReport, report.DateValidation);
                                }
                                
                                // Les autorisations sont-elles suffisantes pour modifier le signalement par une réponse
                                bool bAuthorisation = Constantes.listAuthorisation.Contains(report.Authorisation);
                                if (!bAuthorisation)
                                {
                                    messageReportNoValid += string.Format("Vous n'êtes pas autorisé à modifier le signalement n°{0}\n", IdReport);
                                }
                                if (pos != -1 && bAuthorisation)
                                {
                                    replyReports.Add(report);
                                }
                            });
                        }
                    }
                    if (replyReports.Count == 0)
                    {
                        string mess = "Pas de signalements sélectionnés. Veuillez sélectionner un ou plusieurs signalements.";
                        if (messageReportNoValid != "")
                        {
                            mess = string.Format("Les signalements sélectionnés ne sont pas valides. Opération terminée.\n{0}", messageReportNoValid);
                        }
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                            mess,
                            Constantes.ERROR);
                        logger.Debug(mess);
                        return;
                    }

                    // Ouverture de la boite "ReplyReport" s'il y a au moins un signalement valide
                    ReplyReportViewModel replyReportViewModel = new (context, replyReports, messageReportNoValid);
                    replyReportViewModel.replyReportView.DataContext = replyReportViewModel;
                    bool? dialogResult = replyReportViewModel.replyReportView.ShowDialog();
                    // L'utilisateur a cliqué sur la croix pour fermer le dialogue
                    if (dialogResult == false)
                    {
                        replyReportViewModel.replyReportView.Close();
                        return;
                    }

                }
                catch (Exception e)
                {   
                    string message = string.Format("Problème dans la réponse faite au(x) signalement(s) : {0}\n{1}", e.Message, e.StackTrace);
                    logger.Error(string.Format("ReplyReport.OnClick : {0}\n", message));
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                        e.Message,
                        Constantes.ERROR
                    );
                }
            });
        }
    }
}
