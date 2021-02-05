using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGisProEspaceCollaboratif.Core;
using log4net;

namespace ArcGisProEspaceCollaboratif
{
    internal class DownloadReports : ArcGIS.Desktop.Framework.Contracts.Button
    {
        private readonly EspaceCollaboratifLogger riplogger = EspaceCollaboratifLogger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(Connecter));

        public DownloadReports()
        {
        }


        protected override async void OnClick()
        {
            StatusBar mess;
            FormProgressDownload attenteChargement = new FormProgressDownload();

            try
            {
                await QueuedTask.Run(async() =>
                {
                    Contexte contexte = Contexte.Instance;

                    // Test de la présence du fichier XML de paramétrage
                    if (!System.IO.File.Exists(EspaceCollaboratifHelper.nom_Fichier_Parametres_EspaceCollaboratif))
                    {
                        System.Windows.Forms.MessageBox.Show("Impossible de poursuivre la procédure en raison de l'absence du fichier XML de paramétrage pour se connecter au service Ripart.\n\nLe fichier '" + EspaceCollaboratifHelper.nom_Fichier_Parametres_EspaceCollaboratif + "' doit se situer dans le dossier suivant:\n'" + contexte.repertoireTravail + "'", "IGN Ripart", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return;
                    }

                    DialogResult result1;
                    DialogResult result2;
                    bool courtcircuite = false;
                    string calqueFiltrage = EspaceCollaboratifHelper.Load_CalqueFiltrage();
                    if (calqueFiltrage.Length == 0)
                    {
                        result1 = MessageBox.Show("Impossible de déterminer dans le fichier de paramétrage Ripart, le nom du calque à utiliser pour le filtrage spatial.\n\nSouhaitez-vous poursuivre l'importation des remarques Ripart sur la France entière ? (Cela risque de prendre un temps long.)", "IGN Ripart", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question);
                        if (result1 != DialogResult.Yes)
                        { return; }

                        courtcircuite = true;
                    }

                    Layer layerFiltrage = contexte.GetLayerByName(calqueFiltrage);
                    /*
                                    if (layerFiltrage == null && courtcircuite == false)
                                    {
                                        result2 = MessageBox.Show("La carte en cours ne contient pas le calque '" + calqueFiltrage + "' définit pour être le filtrage spatial.\n\nSouhaitez-vous poursuivre l'importation des remarques Ripart sur la France entière ? (Cela risque de prendre un temps long.)", "IGN Ripart", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question);
                                        if (result2 != DialogResult.Yes)
                                        { return; }

                                        courtcircuite = true;
                                    }

                                    List<Geometry> geometryFiltreSpatial = contexte.GetGeometryFiltreSpatial(calqueFiltrage);

                                    if (geometryFiltreSpatial.Count == 0 && courtcircuite == false)
                                    {
                                        if (MessageBox.Show("Le calque '" + calqueFiltrage + "' ne contient aucun object utilisable pour le filtrage spatial.\n\nSouhaitez-vous poursuivre l'importation des remarques Ripart sur la France entière ? (Cela risque de prendre un temps long.)", "IGN Ripart", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) != DialogResult.Yes)
                                        { return; }
                                    }


                                    ArcGisProEspaceCollaboratif.Core.Box BBoxFiltrageSpatial = contexte.GetBBox(geometryFiltreSpatial);
                    */
                    if (contexte.ripClient == null)
                    {
                        contexte.ripClient = (Client)contexte.GetConnexionEspaceCollaboratif();
                        if (contexte.ripClient == null) return;
                    }
                    /*
                                    ESRI.ArcGIS.Framework.IApplication application = ArcMap.Application;
                                    mess = application.StatusBar;
                                    mess.set_Message(0, "Requête auprès du service Ripart ...");
                                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                                    attenteChargement.Show();
                                    attenteChargement.setText("Téléchargement des remarques depuis le serveur: \n" + contexte.URLHostEspaceCollaboratif);

                                    contexte.ripClient.setProgressBar(attenteChargement.getProgressBar());
                                    attenteChargement.Refresh();
                    */
                    contexte.EmptyCollaborativeSpaceLayers();

                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    int groupeId = -1;
                    if (EspaceCollaboratifHelper.Load_Group() == "true")
                    {
                        groupeId = Convert.ToInt32(contexte.profil.Geogroupe.Id);
                        parameters.Add("group", groupeId.ToString());
                    }


                    /*
                                    if (!courtcircuite)
                                    {
                                        parameters.Add("box", BBoxFiltrageSpatial.boxToString());
                                    }
                    */
                    String sdate = String.Format("{0:yyyy-MM-dd HH:mm:ss}", EspaceCollaboratifHelper.Load_DateExtraction());
                    parameters.Add("updatingDate", sdate);
                    parameters.Add("territory", contexte.profil.Zone.ToString());

                    // création de la liste des remarques.  
                    List<ArcGisProEspaceCollaboratif.Core.Signalement> remarques = contexte.ripClient.GetGeoRems(parameters);

                    attenteChargement.getProgressBar().Maximum = remarques.Count;
                    attenteChargement.getProgressBar().Step = 1;

                    // Filtrage spatial affiné des remarques.
                    //                if (!BBoxFiltrageSpatial.IsEmpty())
                    //                {
                    List<ArcGisProEspaceCollaboratif.Core.Signalement> remarqueAConserver = new List<ArcGisProEspaceCollaboratif.Core.Signalement>();

                    foreach (ArcGisProEspaceCollaboratif.Core.Signalement remarqueTest in remarques)
                    {
                        //                        if (EspaceCollaboratifHelper.IsInGeometry(remarqueTest, geometryFiltreSpatial))
                        //                        {
                        remarqueAConserver.Add(remarqueTest);
                        //                        }
                    }

                    remarques = remarqueAConserver;
                    //                }

                    //                contexte.Zoom(remarques);
                    /*
                                    int countBar = 0;
                                    attenteChargement.setMaxProgressor(remarques.Count);

                                    attenteChargement.setBar(1);

                                    mess.ProgressBar.Position = 0;
                                    mess.ShowProgressBar("Implantation des remarques Ripart", 0, remarques.Count, 1, false);

                                    mess.Visible = true;
                    */

                    // Implantation des remarques importées et filtrées sur la carte.
                    foreach (ArcGisProEspaceCollaboratif.Core.Signalement remarque in remarques)
                    {

                        //                    countBar++;
                        //                    attenteChargement.nextProgressor("Placement sur la carte en cours de la remarque: \n#" + countBar + "/" + remarques.Count + ".");
                        await contexte.CreerPointSignalement(remarque);
                        //                    contexte.activeView.Refresh();
                        //                    mess.StepProgressBar();
                    }

                    //                mess.ProgressBar.Hide();

                    attenteChargement.Close();
                    /*
                                    int remarquesNouvelles = contexte.Count_Remarque_by_Statut(Ripart.Core.Statut.Submit);
                                    int remarquesEnCours = contexte.Count_Remarque_by_Statut(Ripart.Core.Statut.Pending) + contexte.Count_Remarque_by_Statut(Ripart.Core.Statut.Pending0) + contexte.Count_Remarque_by_Statut(Ripart.Core.Statut.Pending1) + contexte.Count_Remarque_by_Statut(Ripart.Core.Statut.Pending2);
                                    int remarquesRejetees = contexte.Count_Remarque_by_Statut(Ripart.Core.Statut.Reject) + contexte.Count_Remarque_by_Statut(Ripart.Core.Statut.Reject0);
                                    int remarquesValidees = contexte.Count_Remarque_by_Statut(Ripart.Core.Statut.Valid) + contexte.Count_Remarque_by_Statut(Ripart.Core.Statut.Valid0);

                                    string message = "Extraction réussie avec succès de " + countBar + " remarque(s) Ripart depuis le serveur";

                                    mess.set_Message(0, message + " !");

                                    message += " avec la répartition suivante:";
                                    message += "\n _ " + remarquesNouvelles + " remarque(s) nouvelle(s).";
                                    message += "\n _ " + remarquesEnCours + " remarque(s) en cours de traitement.";
                                    message += "\n _ " + remarquesValidees + " remarque(s) validée(s).";
                                    message += "\n _ " + remarquesRejetees + " remarque(s) rejetée(s).";

                                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

                                    System.Windows.Forms.MessageBox.Show(message, "IGN Ripart", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    */
                    System.Windows.Forms.MessageBox.Show("Import terminé", "IGN Espace collaboratif", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                });
            }

            catch (Exception e)
            {
                logger.Error(e.Message + "\n" + e.StackTrace);
                mess = null;
                attenteChargement.Close();
                MessageBox.Show(e.Message, "IGN Espace collaboratif", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

            }
        }


        // INUTILE - on utilise la fonction présente dans Contexte

        protected void creerPointRemarqueRipart(ArcGisProEspaceCollaboratif.Core.Signalement report, List<FeatureLayer> mapLayers)
        {

            // on cast en featureLayer
            FeatureLayer reportLayer = Contexte.Instance.GetLayerByName(EspaceCollaboratifHelper.nom_Calque_Signalement) as FeatureLayer;
            //FeatureClass featureClass = featureLayer.GetFeatureClass();

            //Feature featureRemarque = featureClass.CreateRow();

            // Placement géographique du point d'application de la remarque Ripart
            //featureRemarque.SetShape(EspaceCollaboratifHelper.TransformPoint(report.Position));

            var createFeatures = new EditOperation();

            // Remplissage des attributs de la remarque Ripart
            var newReportAttributes = new Dictionary<string, object>
            {
                { "SHAPE", EspaceCollaboratifHelper.TransformPoint(report.Position) },
                { EspaceCollaboratifHelper.nom_Champ_IdRemarque, report.Id },
                { EspaceCollaboratifHelper.nom_Champ_Auteur, report.Auteur.Nom },
                { EspaceCollaboratifHelper.nom_Champ_Departement, report.Departement.Nom },
                { EspaceCollaboratifHelper.nom_Champ_IDDepartement, report.Departement.Id },
                { EspaceCollaboratifHelper.nom_Champ_Commune, report.Commune },
                { EspaceCollaboratifHelper.nom_Champ_DateCreation, report.DateCreation },
                { EspaceCollaboratifHelper.nom_Champ_DateMAJ, report.DateMiseAJour },
                { EspaceCollaboratifHelper.nom_Champ_DateValidation, report.DateValidation },
                { EspaceCollaboratifHelper.nom_Champ_Statut, (int)report.Statut },
                { EspaceCollaboratifHelper.nom_Champ_Themes, report.ConcatenateThemes() },
                { EspaceCollaboratifHelper.nom_Champ_Url, report.Lien },
                { EspaceCollaboratifHelper.nom_Champ_UrlPrive, report.LienPrive }
            };
            /*
                        EspaceCollaboratifHelper.CompleteChampRipart(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_IdRemarque, report.Id);
                        EspaceCollaboratifHelper.CompleteChampRipart(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_Auteur, report.Auteur.Nom);
                        EspaceCollaboratifHelper.CompleteChampRipart(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_Departement, report.Departement.Nom);
                        EspaceCollaboratifHelper.CompleteChampRipart(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_IDDepartement, report.Departement.Id);
                        EspaceCollaboratifHelper.CompleteChampRipart(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_Commune, report.Commune);
                        EspaceCollaboratifHelper.CompleteChampRipart(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_DateCreation, report.DateCreation);
                        EspaceCollaboratifHelper.CompleteChampRipart(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_DateMAJ, report.DateMiseAJour);
                        EspaceCollaboratifHelper.CompleteChampRipart(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_DateValidation, report.DateValidation);
                        EspaceCollaboratifHelper.CompleteChampRipart(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_Statut, (int)report.Statut);
                        EspaceCollaboratifHelper.CompleteChampRipart(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_Themes, report.ConcatenateThemes());
                        EspaceCollaboratifHelper.CompleteChampRipart(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_Url, report.Lien);
                        EspaceCollaboratifHelper.CompleteChampRipart(featureClass, featureRemarque, EspaceCollaboratifHelper.nom_Champ_UrlPrive, report.LienPrive);
            */

            createFeatures.Create(reportLayer, newReportAttributes);

            /* TO-DO : croquis
            featureRemarque.Store();


            //  Traitement du ou des croquis associé(s) à la remarque            
            if (!report.IsCroquisEmpty())
            {

                foreach (ArcGisProEspaceCollaboratif.Core.Sketch unCroquis in report.Sketch)
                {
                    if (unCroquis.Points.Count == 0)
                    {
                        //  this.debugForm.WriteLine("Croquis sans coordonnées dans la remarque n°" + uneRemarque.Id);
                    }
                    else
                    {
                        // on cast le featureLayer en fonction du type du croquis pour utiliser le bon calque associé
                        FeatureLayer featureLayerCroquis = mapLayers[(int)unCroquis.Type] as FeatureLayer;
                        FeatureClass featureClassCroquis = featureLayerCroquis.GetFeatureClass();

                        IFeature featureCroquis = featureClassCroquis.CreateFeature();

                        Polyline polylineCroquis = new Polyline() as IPolyline;
                        Polygon polygonCroquis = new Polygon() as IPolygon;
                        Point pointCroquis = EspaceCollaboratifHelper.TransformPoint(unCroquis.Points.First());

                        // Construction géométrique du croquis en fonction de son type et à partir du vecteur de vertex du croquis.
                        switch (unCroquis.Type)
                        {
                            case ArcGisProEspaceCollaboratif.Core.Sketch.SketchType.Point:
                                featureCroquis.Shape = pointCroquis;
                                break;

                            case ArcGisProEspaceCollaboratif.Core.Sketch.SketchType.Ligne:
                                featureCroquis.Shape = EspaceCollaboratifHelper.GeometryFromCroquis(polylineCroquis, unCroquis);
                                break;

                            case ArcGisProEspaceCollaboratif.Core.Sketch.SketchType.Polygone:
                                featureCroquis.Shape = EspaceCollaboratifHelper.GeometryFromCroquis(polygonCroquis, unCroquis);
                                break;

                        }

                        EspaceCollaboratifHelper.CompleteChampRipart(featureClassCroquis, featureCroquis, EspaceCollaboratifHelper.nom_Champ_LienRemarque, uneRemarque.Id);
                        EspaceCollaboratifHelper.CompleteChampRipart(featureClassCroquis, featureCroquis, EspaceCollaboratifHelper.nom_Champ_NomCroquis, unCroquis.Nom);

                        featureCroquis.Store();
                    }
                }
                
            }

    */
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
