using System;
using System.Collections.Generic;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGisProEspaceCollaboratif.Core;
using log4net;
using static ArcGisProEspaceCollaboratif.Core.Status;

namespace ArcGisProEspaceCollaboratif
{
    internal class DownloadReports : ArcGIS.Desktop.Framework.Contracts.Button
    {
        private static readonly Logger riplogger = Logger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(DownloadReports));

        protected override async void OnClick()
        {
            await QueuedTask.Run(async () =>
            {
                try
                {                    
                    Context context = Context.Instance;

                    // Est-ce que l'utilisateur s'est connecté ?
                    if (context.Client == null)
                    {
                        context.Client = (Client)context.GetConnexionEspaceCollaboratif();
                        if (context.Client == null) return;
                    }

                    // Test de la présence du fichier XML de paramétrage
                    context.CheckConfigFile();

                    // Préparation des paramètres à envoyer pour la requête de récupération des signalements
                    Dictionary<string, string> parameters = new Dictionary<string, string>();

                    // Paramètre groupe
                    int groupeId = -1;
                    if (Helper.LoadExtractionForGroup() == "true")
                    {
                        groupeId = Convert.ToInt32(context.Profil.Group.Id);
                        parameters.Add("group", groupeId.ToString());
                    }

                    // Paramtère date d'extraction
                    string sdate = string.Format("{0:yyyy-MM-dd HH:mm:ss}", Helper.LoadDateExtraction());
                    parameters.Add("updatingDate", sdate);

                    // Paramètre filtrage spatial
                    string filterLayerName = Helper.LoadNameLayerForSpatialFilter();
                    Tuple<bool, bool, Box, List<Geometry>> filterParameters = GetSpatialFilterParameters(filterLayerName);

                    bool hasFilter = filterParameters.Item1;
                    bool overrideFilter = filterParameters.Item2;

                    if (!hasFilter && !overrideFilter)
                        return;

                    if (hasFilter)
                        parameters.Add("box", filterParameters.Item3.BoxToString());

                    // TODO Noémie : à valider
                    ArcGIS.Desktop.Framework.Threading.Tasks.ProgressDialog progDialog = new ArcGIS.Desktop.Framework.Threading.Tasks.ProgressDialog("Récupération des signalements sur le serveur...");
                    progDialog.Show();
                    List<Report> reports = context.Client.GetGeoRems(parameters);
                    progDialog.Hide();

                    // Filtrage spatial des signalements
                    if (hasFilter)
                    {
                        List<Report> reportToKeep = new List<Report>();

                        foreach (Report testReport in reports)
                        {
                            if (Helper.IsInGeometry(testReport, filterParameters.Item4))
                                reportToKeep.Add(testReport);
                        }
                        reports = reportToKeep;
                    }

                    // TODO Noémie : à valider
                    progDialog = new ArcGIS.Desktop.Framework.Threading.Tasks.ProgressDialog("Import des signalements dans la carte...");
                    progDialog.Show();
                    // Chargement ou création des couches liées aux signalements
                    await context.CreateOrLoadReportLayers();
                    progDialog.Hide();

                    // On vide les couches récupérées au cas où elles contiendraient d'anciens objets
                    context.RemoveAllObjectsFromLayers();

                    int countReports = reports.Count;
                    await context.InsertReports(reports);
                    
                    //Zoom sur la couche des signalements
                    FeatureLayer reportLayer = context.GetLayerByName(Helper.name_layer_Signalement);
                    context.MapActiveView.ZoomTo(reportLayer.QueryExtent());

                    // Message de confirmation
                    int newReports = context.CountReportsByStatus(EnumStatus.submit);
                    int pendingReports = context.CountReportsByStatus(EnumStatus.pending) + context.CountReportsByStatus(EnumStatus.pending0) + context.CountReportsByStatus(EnumStatus.pending1) + context.CountReportsByStatus(EnumStatus.pending2);
                    int rejectedReports = context.CountReportsByStatus(EnumStatus.reject) + context.CountReportsByStatus(EnumStatus.reject0);
                    int validatedReports = context.CountReportsByStatus(EnumStatus.valid) + context.CountReportsByStatus(EnumStatus.valid0);

                    string message = "Import de " + countReports + " signalement(s) depuis l'Espace collaboratif :\n";
                    message += "\n _ " + newReports + " nouveaux signalements.";
                    message += "\n _ " + pendingReports + " signalement(s) en cours de traitement.";
                    message += "\n _ " + validatedReports + " signalement(s) validé(s).";
                    message += "\n _ " + rejectedReports + " signalement(s) rejeté(s).";

                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.WARNING);
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
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(e.Message, Constantes.ERROR);
                        string message = string.Format("{0}\n{1}", e.Message, e.StackTrace);
                        logger.Error(string.Format("DownloadReports.OnClick : {0}\n", message));
                        return;
                    }
                }
            });
        }

        /// <summary>
        /// Renvoie les paramètres de filtrage spatial des signalements sous forme de tuple.
        /// </summary>
        /// <param name="filterLayerName">Nom de la couche définie comme filtre spatial.</param>
        /// <returns>Un tuple :
        /// Item1 : booléen indiquant si un filtre est défini,
        /// Item2 : booléen indiquant si on continue même en l'absence de filtre,
        /// Item3 : bbox des géométries contenues dans la couche,
        /// Item4 : liste des géométries contenues dans la couche.
        /// </returns>
        public Tuple<bool, bool, Box, List<Geometry>> GetSpatialFilterParameters(string filterLayerName)
        {
            //Initialisation
            Box bboxFiltrageSpatial = new Box();
            List<Geometry> spatialFilterGeometry = new List<Geometry>();

            Tuple< bool, bool, Box, List<Geometry>> noFilterTuple = Tuple.Create(false, false, bboxFiltrageSpatial, spatialFilterGeometry);
            Tuple< bool, bool, Box, List<Geometry>> overrideFilterTuple = Tuple.Create(false, true, bboxFiltrageSpatial, spatialFilterGeometry);

            // Cas nom de la couche non rempli
            if (filterLayerName.Length == 0)
            {
                string message = "Impossible de déterminer dans le fichier de paramétrage de l'Espace collaboratif le nom de la couche à utiliser pour le filtrage spatial.\n\nSouhaitez-vous poursuivre l'import des signalements sur la France entière ? (Cela risque de prendre du temps.)";
                System.Windows.MessageBoxResult messageBoxResult = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.QUESTION);
                if (messageBoxResult == System.Windows.MessageBoxResult.OK ||
                    messageBoxResult == System.Windows.MessageBoxResult.Yes )
                {
                    return overrideFilterTuple;
                }
                else
                {
                    return noFilterTuple;
                }
            }

            Context context = Context.Instance;
            Layer filterLayer = context.GetLayerByName(filterLayerName);

            if (filterLayer == null)
            {
                string message = string.Format("La carte en cours ne contient pas la couche '{0}' définie pour le filtrage spatial des signalements.\n\nSouhaitez-vous poursuivre l'import des signalements sur la France entière ? (Cela risque de prendre du temps.)", filterLayerName);
                System.Windows.MessageBoxResult messageBoxResult = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.QUESTION);
                if (messageBoxResult == System.Windows.MessageBoxResult.OK ||
                    messageBoxResult == System.Windows.MessageBoxResult.Yes)
                {
                    return overrideFilterTuple;
                }
                else
                {
                    return noFilterTuple;
                }
            }

            spatialFilterGeometry = context.GetSpatialFilterGeometry(filterLayerName);
            if (spatialFilterGeometry.Count == 0)
            {
                string message = string.Format("La couche '{0}' ne contient aucun object utilisable pour le filtrage spatial.\n\nSouhaitez-vous poursuivre l'import des signalements sur la France entière ? (Cela risque de prendre du temps.)", filterLayerName);
                System.Windows.MessageBoxResult messageBoxResult = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.QUESTION);
                if (messageBoxResult != System.Windows.MessageBoxResult.OK ||
                    messageBoxResult != System.Windows.MessageBoxResult.Yes)
                {
                    return noFilterTuple;
                }
            }

            // On ajoute la BBOX comme paramètre de la requête
            bboxFiltrageSpatial = context.GetBBox(spatialFilterGeometry);
            
            return Tuple.Create(true, false, bboxFiltrageSpatial, spatialFilterGeometry);
        }

        protected override void OnUpdate()
        {
            // Pas trop sure...
            this.Enabled = Project.Current != null;
        }
    }
}
