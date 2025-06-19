using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Media;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Layouts.DockPanes;
using ArcGIS.Desktop.Internal.Mapping.Object3DRenderingFilter;
using ArcGIS.Desktop.Internal.Mapping.TOC;
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
                    logger.Info("DownloadReports : début du traitement.");
                    Context context = Context.Instance;

                    // Est-ce que l'utilisateur s'est connecté ou a changé de projet ?
                    Project project = Project.Current;
                    if (project.HomeFolderPath.Length == 0)
                    {
                        string mess = "Votre projet doit être enregistré avant de pouvoir utiliser l'add-in Espace collaboratif";
                        logger.Error(string.Format("Context.Init : {0}\n", mess));
                        throw new ArgumentNullException(mess);
                    }

                    if (context.Client == null || (context.DirectoryWorking != project.HomeFolderPath))
                    {
                        ArcGisProEspaceCollaboratif.Core.Client client = null;
                        context.GetConnexionEspaceCollaboratif(ref client);
                        context.Client = client;
                        if (context.Client == null) return;
                    }

                    // Test de la présence du fichier XML de paramétrage
                    context.CheckConfigFile();

                    // Est-ce la bonne carte active qui contient les couches signalement et croquis ?
                    // si ce n'est pas la bonne carte active, il faut arrêter l'import des signalements et demander la bonne carte active
                    // si c'est la bonne carte active mais que les couches signalement et croquis n'existent pas, il faut demander à créer ces couches
                    await context.CheckMapActiveWithCollaborativeLayersAsync();

                    // Si c'est la bonne carte active, il faut vérifier que les couches sont connectées à la source de données
                    Context.CheckConnectionStatus();

                    // Enfin, il faut supprimer les données dans la Geodatabase pour les couches "Signalement", "Croquis_EC_Polygone", "Croquis_EC_Ligne", "Croquis_EC_Point" 
                    foreach (string layerName in Helper.CollaborativeSpaceLayers)
                    {
                        context.EmptyCollabFeatureClasses(layerName);
                    }

                    // Préparation des paramètres à envoyer pour la requête de récupération des signalements
                    Dictionary<string, string> parameters = new ();

                    // Paramètre groupe
                    int groupeId = -1;
                    if (Helper.LoadExtractionForGroup() == "true")
                    {
                        groupeId = Convert.ToInt32(context.Profil.Group.Id);
                        parameters.Add("group", groupeId.ToString());
                    }

                    // Paramètres date d'extraction
                    string sStartDate = string.Format("{0:yyyy-MM-dd HH:mm:ss}", Helper.LoadStartDateExtraction());
                    parameters.Add("updatingDate", sStartDate);
                    string sEndDate = string.Format("{0:yyyy-MM-dd HH:mm:ss}", Helper.LoadEndDateExtraction());

                    // Paramètre filtrage spatial
                    string filterLayerName = Helper.LoadNameLayerForSpatialFilter();
                    Tuple<bool, bool, Box, List<ArcGIS.Core.Geometry.Geometry>> filterParameters = GetSpatialFilterParameters(context, filterLayerName);
                    bool hasFilter = filterParameters.Item1;
                    bool overrideFilter = filterParameters.Item2;

                    if (!hasFilter && !overrideFilter)
                        return;

                    if (hasFilter)
                        parameters.Add("box", filterParameters.Item3.BoxToString());
                    
                    logger.Info("DownloadReports : fin check files.");

                    ArcGIS.Desktop.Framework.Threading.Tasks.ProgressDialog progressDialog = new ("Récupération des signalements sur le serveur...");
                    progressDialog.Show();
                    List<Report> reports = context.Client.GetGeoRems(parameters);
                    // Délai pour télécharger les objets du serveur 10mn
                    // await DownloadReports.StopDownloadReports(progressDialog, 600);
                    progressDialog.Hide();

                    if (reports.Count == 0)
                    {
                        string mess = "Pas de signalements extraits depuis l'Espace collaboratif";
                        logger.Info(mess);
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(mess, Constantes.INFORMATION);
                        return;
                    }
                    logger.Info("DownloadReports : fin requête spatiale.");
                    // Filtrage par dates
                    // le cas sStartDate != Constantes.DEFAULT_DATE_EXTRACTION && sEndDate == Constantes.DEFAULT_DATE_EXTRACTION
                    // est celui des paramètres d'extraction ligne 47 car on extrait les signalements avec la date de début
                    if (sStartDate != Constantes.DEFAULT_DATE_EXTRACTION && sEndDate != Constantes.DEFAULT_DATE_EXTRACTION ||
                        sStartDate == Constantes.DEFAULT_DATE_EXTRACTION && sEndDate != Constantes.DEFAULT_DATE_EXTRACTION)
                    {
                        DateTime dtStart = Convert.ToDateTime(sStartDate);
                        DateTime dtEnd = Convert.ToDateTime(sEndDate);
                        int result = DateTime.Compare(dtStart, dtEnd);
                        if (sStartDate != sEndDate && result < 0)
                        {
                            List<Report> reportToKeep = new ();

                            foreach (Report report in reports)
                            {
                                DateTime reportDate = report.DateUpdate;
                                if (Helper.IsInDates(reportDate, dtStart, dtEnd))
                                {
                                    reportToKeep.Add(report);
                                }
                            }
                            reports = reportToKeep;
                        }
                    }
                    logger.Info("DownloadReports : fin filtrage par date.");

                    // Filtrage spatial des signalements
                    if (hasFilter)
                    {
                        List<Report> reportToKeep = new ();

                        foreach (Report report in reports)
                        {
                            if (Helper.IsInGeometry(report, filterParameters.Item4))
                            {
                                reportToKeep.Add(report);
                            }                         
                        }
                        reports = reportToKeep;
                    }
                    logger.Info("DownloadReports : fin filtrage spatial.");
                    progressDialog = new ProgressDialog("Import des signalements dans la carte...");
                    progressDialog.Show();
                    
                    // Chargement ou création des couches liées aux signalements
                    await context.CreateOrLoadReportLayers();

                    // Délai pour importer les objets dans la carte 10mn
                    //await DownloadReports.StopDownloadReports(progressDialog, 600);

                    // On vide les couches récupérées au cas où elles contiendraient d'anciens objets
                    //                    context.RemoveAllObjectsFromLayers();

                    int countReports = reports.Count;
                    bool res = await context.InsertReports(reports);
                    progressDialog.Hide();
                    if (!res)
                    {
                        string mess = "Context.InsertReports a retourné une erreur, fin du traitement. Veuillez consulter le fichier de log.";
                        logger.Error(mess);
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(mess, Constantes.ERROR);
                        return;
                    }
                    logger.Info("DownloadReports : fin import des signalements.");
                    //Zoom sur la couche des signalements (supprimé à la demande du SVRP)
                    //FeatureLayer reportLayer = context.GetLayerByName(Helper.name_layer_Signalement);
                    //context.MapActiveView.ZoomTo(reportLayer.QueryExtent());

                    // Message de confirmation
                    long newReports = context.CountReportsByStatus(EnumStatus.submit);
                    long pendingReports = context.CountReportsByStatus(EnumStatus.pending) + context.CountReportsByStatus(EnumStatus.pending0) + context.CountReportsByStatus(EnumStatus.pending1) + context.CountReportsByStatus(EnumStatus.pending2);
                    long rejectedReports = context.CountReportsByStatus(EnumStatus.reject) + context.CountReportsByStatus(EnumStatus.reject0);
                    long validatedReports = context.CountReportsByStatus(EnumStatus.valid) + context.CountReportsByStatus(EnumStatus.valid0);

                    string message = "Import de " + countReports + " signalement(s) depuis l'Espace collaboratif :\n";
                    message += "\n _ " + newReports + " nouveaux signalements.";
                    message += "\n _ " + pendingReports + " signalement(s) en cours de traitement.";
                    message += "\n _ " + validatedReports + " signalement(s) validé(s).";
                    message += "\n _ " + rejectedReports + " signalement(s) rejeté(s).";
                    logger.Info(message);
                    logger.Info("DownloadReports : fin du traitement.");
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.INFORMATION);
                }
                catch (Exception e)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(e.Message, Constantes.ERROR);
                    string message = string.Format("{0}\n{1}", e.Message, e.StackTrace);
                    logger.Error(string.Format("DownloadReports.OnClick : {0}\n", message));
                    return;
                }
            });
        }

        private static Task StopDownloadReports(ArcGIS.Desktop.Framework.Threading.Tasks.ProgressDialog progDialog, uint timeToStop)
        {
            return QueuedTask.Run(async () =>
            {
                for (uint iSeconds = 0; iSeconds < timeToStop; iSeconds++)
                {
                    await Task.Delay(1000);
                }
                string msg = "Le délai imparti pour télécharger les signalements est écoulé, voulez-vous continuer le processus ?";
                System.Windows.MessageBoxResult result = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(msg, Constantes.QUESTION, System.Windows.MessageBoxButton.YesNo);
                if (result == System.Windows.MessageBoxResult.Cancel ||
                    result == System.Windows.MessageBoxResult.No ||
                    result == System.Windows.MessageBoxResult.None)
                {
                    progDialog.Hide();
                    throw new Exception("Téléchargement des signalements interrompu par l'utilisateur.");
                }
            });
        }

        /// <summary>
        /// Renvoie les paramètres de filtrage spatial des signalements sous forme de tuple.
        /// 1. Pas de sélection ni de nom de couche indiqué dans le fichier de paramètres -> extraction France entière
        /// 2. Pas de sélection ni de couche associée au nom stocké dans le fichier de paramètres -> extraction France entière
        /// 3. Pas de sélection ni d'objets dans la couche servant au filtrage spatial -> extraction France entière
        /// 4. Pas de sélection mais un nom de couche indiqué dans le fichier de paramètres -> extraction à partir de cette couche
        /// 5. Une sélection et un nom de couche indiqué dans le fichier de paramètres -> extraction à partir de la sélection
        /// 6. Par défaut -> extraction France entière
        /// </summary>
        /// <param name="filterLayerName">Nom de la couche définie comme filtre spatial.</param>
        /// <returns>Un tuple :
        /// Item1 : booléen indiquant si un filtre est défini,
        /// Item2 : booléen indiquant si on continue même en l'absence de filtre,
        /// Item3 : bbox des géométries contenues dans la couche,
        /// Item4 : liste des géométries contenues dans la couche.
        /// </returns>
        public static Tuple<bool, bool, Box, List<ArcGIS.Core.Geometry.Geometry>> GetSpatialFilterParameters(Context cxt, string filterLayerName)
        {
            //Initialisation
            Box bboxFiltrageSpatial = new();
            List<ArcGIS.Core.Geometry.Geometry> spatialFilterGeometry = new();

            Tuple<bool, bool, Box, List<ArcGIS.Core.Geometry.Geometry>> noFilterTuple = Tuple.Create(false, false, bboxFiltrageSpatial, spatialFilterGeometry);
            Tuple<bool, bool, Box, List<ArcGIS.Core.Geometry.Geometry>> overrideFilterTuple = Tuple.Create(false, true, bboxFiltrageSpatial, spatialFilterGeometry);

            List<ArcGIS.Core.Geometry.Geometry> geomFromSelection = CheckSelectedObjects(cxt);

            // Cas 1 : le nom de la couche et la sélection sont vides -> extraction France entière
            if (string.IsNullOrEmpty(filterLayerName) && geomFromSelection.Count == 0)
            {
                string message = "Impossible de déterminer dans le fichier de paramétrage de l'Espace collaboratif le nom de la couche à utiliser pour le filtrage spatial.\n\nSouhaitez-vous poursuivre l'import des signalements sur la France entière ? (Cela risque de prendre du temps.)";
                System.Windows.MessageBoxResult messageBoxResult = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.QUESTION, System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == System.Windows.MessageBoxResult.OK ||
                    messageBoxResult == System.Windows.MessageBoxResult.Yes ||
                    messageBoxResult == System.Windows.MessageBoxResult.None)
                {
                    return overrideFilterTuple;
                }
                else
                {
                    return noFilterTuple;
                }
            }

            // Cas 2 : le nom de la couche est indiqué mais elle n'existe pas dans la carte et la sélection est vide -> extraction France entière
            Context context = Context.Instance;
            Layer filterLayer = context.GetLayerByName(filterLayerName);
            if (filterLayer == null && geomFromSelection.Count == 0)
            {
                string message = string.Format("La carte en cours ne contient pas la couche '{0}' définie pour le filtrage spatial des signalements.\n\nSouhaitez-vous poursuivre l'import des signalements sur la France entière ? (Cela risque de prendre du temps.)", filterLayerName);
                System.Windows.MessageBoxResult messageBoxResult = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.QUESTION, System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == System.Windows.MessageBoxResult.OK ||
                    messageBoxResult == System.Windows.MessageBoxResult.Yes ||
                    messageBoxResult == System.Windows.MessageBoxResult.None)
                {
                    return overrideFilterTuple;
                }
                else
                {
                    return noFilterTuple;
                }
            }

            // Cas 3 : la couche existe mais ne contient pas d'objets et la sélection est vide -> extraction France entière
            spatialFilterGeometry = context.GetSpatialFilterGeometry(filterLayerName);
            if (spatialFilterGeometry.Count == 0 && geomFromSelection.Count == 0)
            {
                string message = string.Format("La couche '{0}' ne contient aucun objet utilisable pour le filtrage spatial.\n\nSouhaitez-vous poursuivre l'import des signalements sur la France entière ? (Cela risque de prendre du temps.)", filterLayerName);
                System.Windows.MessageBoxResult messageBoxResult = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.QUESTION, System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == System.Windows.MessageBoxResult.OK ||
                    messageBoxResult == System.Windows.MessageBoxResult.Yes ||
                    messageBoxResult == System.Windows.MessageBoxResult.None)
                {
                    return overrideFilterTuple;
                }
                else
                {
                    return noFilterTuple;
                }
            }

            // Cas 4 : la sélection est vide mais la couche pour le filtrage spatial contient un ou plusieurs objets -> extraction à partir de cette couche
            if (spatialFilterGeometry.Count >= 1 && geomFromSelection.Count == 0)
            {
                // On ajoute la BBOX comme paramètre de la requête
                bboxFiltrageSpatial = Context.GetBBoxBis(spatialFilterGeometry);
                return Tuple.Create(true, false, bboxFiltrageSpatial, spatialFilterGeometry);
            }

            // Cas 5 : la sélection est remplie et la couche pour le filtrage spatial contient un ou plusieurs objets -> extraction à partir de la sélection
            if (spatialFilterGeometry.Count >= 1 && geomFromSelection.Count >= 1)
            {
                // On ajoute la BBOX comme paramètre de la requête
                bboxFiltrageSpatial = Context.GetBBoxBis(geomFromSelection);
                return Tuple.Create(true, false, bboxFiltrageSpatial, geomFromSelection);
            }

            // Cas par défaut : extraction France entière
            return noFilterTuple;
        }

        /// <summary>
        /// Contrôle si un des cas suivants est vrai:
        ///    1) un ou plusieurs croquis sélectionnés
        ///    2) une ou plusieurs signalements sélectionnés
        /// </summary>
        /// <returns>
        ///     - Une chaine de caractères vide si pas de sélection ou sélection d'objets de type différents
        ///     - "sketch" si des croquis sont sélectionnés, 
        ///     - "report" si des signalements sont sélectionnés
        /// </returns>
        private static List<ArcGIS.Core.Geometry.Geometry> CheckSelectedObjects(Context context)
        {
            List<ArcGIS.Core.Geometry.Geometry> spatialFilterGeometry = new();
            SelectionSet selectedFeatures = context.GetMap().GetSelection();
            foreach (KeyValuePair<MapMember, List<long>> kvp in selectedFeatures.ToDictionary())
            {
                if (kvp.Value.Count == 0)
                {
                    continue;
                }
                if (kvp.Key is not FeatureLayer featureLayer)
                {
                    continue;
                }
                if (featureLayer.ShapeType is not esriGeometryType.esriGeometryPolygon)
                {
                    continue;
                }
                List<long> lOid = kvp.Value;
                foreach (long oid in lOid)
                {
                    QueuedTask.Run(() =>
                    {
                        ArcGIS.Desktop.Editing.Attributes.Inspector inspector = featureLayer.Inspect(oid);
                        ArcGIS.Core.Geometry.Geometry geometryFeature = inspector.Shape;
                        spatialFilterGeometry.Add(geometryFeature);
                    });
                }
            }
            return spatialFilterGeometry;
        }

        protected override void OnUpdate()
        {
            // Pas trop sure...
            this.Enabled = Project.Current != null;
        }
    }
}
