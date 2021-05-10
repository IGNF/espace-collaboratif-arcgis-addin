using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGisProEspaceCollaboratif.Core;
using log4net;

namespace ArcGisProEspaceCollaboratif
{
    internal class DownloadReports : ArcGIS.Desktop.Framework.Contracts.Button
    {
        private readonly Logger riplogger = Logger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(Connect));

        /*public DownloadReports()
        {
        }*/


        protected override async void OnClick()
        {
            FormProgressDownload progressDownload = new FormProgressDownload();

            try
            {
                await QueuedTask.Run(async() =>
                {
                    Context contexte = Context.Instance;

                    // Est-ce que l'utilisateur s'est connecté ?
                    if (contexte.Client == null)
                    {
                        contexte.Client = (Client)contexte.GetConnexionEspaceCollaboratif();
                        if (contexte.Client == null) return;
                    }

                    // Test de la présence du fichier XML de paramétrage
                    if (!System.IO.File.Exists(Helper.name_file_espaceco_xml))
                    {
                        string mess = string.Format("Impossible de poursuivre la procédure en raison de l'absence du fichier XML de paramétrage pour se connecter au service de l'Espace collaboratif.\n\nLe fichier '{0}' doit se situer dans le dossier suivant :\n'{1}'", Helper.name_file_espaceco_xml, contexte.DirectoryWorking);
                        System.Windows.Forms.MessageBox.Show(
                            mess,
                            Constantes.ERROR,
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Error
                        );
                        return;
                    }

                    // Préparation des paramètres à envoyer pour la requête de récupération des signalements
                    Dictionary<string, string> parameters = new Dictionary<string, string>();

                    // Paramètre groupe
                    int groupeId = -1;
                    if (Helper.Load_Group() == "true")
                    {
                        groupeId = Convert.ToInt32(contexte.Profil.Group.Id);
                        parameters.Add("group", groupeId.ToString());
                    }

                    // Paramtère date d'extraction
                    string sdate = string.Format("{0:yyyy-MM-dd HH:mm:ss}", Helper.Load_DateExtraction());
                    parameters.Add("updatingDate", sdate);

                    // Paramètre filtrage spatial

                    string filterLayerName = Helper.Load_FilterLayer();
                    Tuple<bool, bool, Box, List<Geometry>> filterParameters = GetSpatialFilterParameters(filterLayerName);

                    bool hasFilter = filterParameters.Item1;
                    bool overrideFilter = filterParameters.Item2;

                    if (!hasFilter && !overrideFilter)
                        return;

                    if (hasFilter)
                        parameters.Add("box", filterParameters.Item3.BoxToString());
                                    
                    // Envoi de la requête au serveur et création de la liste des signalements
                    progressDownload.Show();
                    progressDownload.SetText("Import des signalements depuis le serveur: \n" + contexte.URLHost);

                    contexte.Client.SetProgressBar(progressDownload.GetProgressBar());
                    progressDownload.Refresh();

                    List<Report> signalements = contexte.Client.GetGeoRems(parameters);

                    // Filtrage spatial affiné des signalements
                    if (hasFilter)
                    {
                        List<Report> signalementAConserver = new List<Report>();

                        foreach (Report signalementTest in signalements)
                        {
                            if (Helper.IsInGeometry(signalementTest, filterParameters.Item4))
                                signalementAConserver.Add(signalementTest);
                        }

                        signalements = signalementAConserver;
                    }

                    // Chargement ou création des couches liées aux signalements
                    await contexte.CreateOrLoadReportLayers();

                    // On vide les couches récupérées au cas où elles contiendraient d'anciens objets
                    contexte.EmptyCollaborativeSpaceLayers();

                    // Barre de progression
                    int countBar = 0;

                    progressDownload.GetProgressBar().Maximum = signalements.Count;
                    progressDownload.GetProgressBar().Step = 1;
                    progressDownload.SetMaxProgressor(signalements.Count);
                    progressDownload.SetBar(1);

                    // Placement des signalements importés et filtrés sur la carte.
                    foreach (Report remarque in signalements)
                    {
                        countBar++;
                        progressDownload.NextProgressor("Placement sur la carte du signalement " + countBar + "/" + signalements.Count);
                        await contexte.CreerPointSignalement(remarque);
                    }

                    progressDownload.Close();

                    //Zoom sur la couche des signalements
                    FeatureLayer reportLayer = contexte.GetLayerByName(Helper.name_layer_Signalement);
                    contexte.MapActiveView.ZoomTo(reportLayer.QueryExtent());

                    // Message de confirmation
                    int newReports = contexte.CountReportsByStatus(Status.Submit);
                    int pendingReports = contexte.CountReportsByStatus(Status.Pending) + contexte.CountReportsByStatus(Status.Pending0) + contexte.CountReportsByStatus(Status.Pending1) + contexte.CountReportsByStatus(Status.Pending2);
                    int rejectedReports = contexte.CountReportsByStatus(Status.Reject) + contexte.CountReportsByStatus(Status.Reject0);
                    int validatedReports = contexte.CountReportsByStatus(Status.Valid) + contexte.CountReportsByStatus(Status.Valid0);

                    string message = "Import de " + countBar + " signalement(s) depuis l'Espace collaboratif :\n";

                    //mess.set_Message(0, message + " !");

                    message += "\n _ " + newReports + " nouveaux signalements.";
                    message += "\n _ " + pendingReports + " signalement(s) en cours de traitement.";
                    message += "\n _ " + validatedReports + " signalement(s) validé(s).";
                    message += "\n _ " + rejectedReports + " signalement(s) rejeté(s).";

                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

                    MessageBox.Show(message, "IGN Espace collaboratif", MessageBoxButtons.OK, MessageBoxIcon.Information);

                });
            }

            catch (Exception e)
            {
                logger.Error(e.Message + "\n" + e.StackTrace);
                progressDownload.Close();
                MessageBox.Show(e.Message, "IGN Espace collaboratif - ERREUR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

            // Initialisation boîte de dialogue
            DialogResult resultDialog;

            // Cas nom de la couche non rempli
            if (filterLayerName.Length == 0)
            {
                resultDialog = MessageBox.Show("Impossible de déterminer dans le fichier de paramétrage de l'Espace collaboratif le nom de la couche à utiliser pour le filtrage spatial.\n\nSouhaitez-vous poursuivre l'import des signalements sur la France entière ? (Cela risque de prendre un temps long.)", "IGN Espace collaboratif - QUESTION", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question);

                if (resultDialog == DialogResult.Yes)
                    return overrideFilterTuple;
                else
                    return noFilterTuple;
            }

            Context contexte = Context.Instance;
            Layer filterLayer = contexte.GetLayerByName(filterLayerName);

            if (filterLayer == null)
            {
                resultDialog = MessageBox.Show("La carte en cours ne contient pas la couche '" + filterLayerName + "' définie pour le filtrage spatial des signalements.\n\nSouhaitez-vous poursuivre l'import des signalements sur la France entière ? (Cela risque de prendre un temps long.)", "IGN Espace collaboratif", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (resultDialog == DialogResult.Yes)
                    return overrideFilterTuple;
                else
                    return noFilterTuple;
            }

            spatialFilterGeometry = contexte.GetSpatialFilterGeometry(filterLayerName);

            if (spatialFilterGeometry.Count == 0)
            {
                resultDialog = MessageBox.Show("La couche '" + filterLayerName + "' ne contient aucun object utilisable pour le filtrage spatial.\n\nSouhaitez-vous poursuivre l'import des signalements sur la France entière ? (Cela risque de prendre un temps long.)", "IGN Espace collaboratif", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (resultDialog != DialogResult.Yes)
                    return noFilterTuple;
            }


            // On ajoute la BBOX comme paramètre de la requête
            bboxFiltrageSpatial = contexte.GetBBox(spatialFilterGeometry);
            
            return Tuple.Create(true, false, bboxFiltrageSpatial, spatialFilterGeometry);
        }


        protected override void OnUpdate()
        {
            // Enabled = true;
            //this.Enabled = ArcMap.Application != null;
            // Pas trop sure...
            this.Enabled = Project.Current != null;
        }

    }
}
