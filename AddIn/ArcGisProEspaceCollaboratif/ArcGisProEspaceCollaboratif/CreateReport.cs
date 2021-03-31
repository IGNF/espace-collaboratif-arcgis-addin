using System;
using System.Collections.Generic;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGisProEspaceCollaboratif.Core;
using log4net;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGisProEspaceCollaboratif.ViewModels;

namespace ArcGisProEspaceCollaboratif
{
    internal class CreateReport : Button
    {
        private readonly Logger riplogger = Logger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(CreateReport));

        protected override async void OnClick()
        {
            logger.Debug("Click sur le bouton de création d'un nouveau signalement");
            //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Créer un signalement sur la carte en cours", "Espace collaboratif");
            await QueuedTask.Run(async () =>
            {
                try
                {
                    Contexte context = Contexte.Instance;

                    // Il faut s'être connecté au service pour la créer un signalement
                    if (context.Client == null)
                    {
                        // Établissement de la connexion avec le service Espace collaboratif.
                        context.Client = context.GetConnexionEspaceCollaboratif();
                        if (context.Client == null)
                        {
                            return;
                        }
                    }

                    // Est-ce que la couche signalement existe dans la carte ?
                    bool bRes = context.IsLayerInMap(Helper.name_layer_Signalement);
                    if (!bRes)
                    {
                        string message = "Pas de couche 'Signalement' dans la carte.\nIl est donc impossible de créer un nouveau signalement.\nIl faut se connecter à l'Espace collaboratif et et télécharger les signalements.";
                        System.Windows.Forms.MessageBox.Show(
                            message,
                            Constantes.ERROR,
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Error);
                        logger.Debug(message);
                        return;
                    } 

                    // Transformation des objets sélectionnés en croquis.
                    List<ArcGisProEspaceCollaboratif.Core.Sketch> futursSketch = context.MakeCroquis_from_Selection();
                    logger.Debug(futursSketch.Count + " croquis générés.");
                    if (futursSketch.Count == 0)
                    {
                        string message = "Aucun objet sélectionné.\nIl est donc impossible de déterminer le point d'application du nouveau signalement à créer.";
                        System.Windows.Forms.MessageBox.Show(
                            message,
                            Constantes.WARNING,
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Warning
                        );
                        logger.Debug(message);
                        return;
                    }

                    // Lancement du formulaire pour créer un nouveau signalement
                    var createReportViewModel = new CreateReportViewModel(context, futursSketch.Count);
                    createReportViewModel.createReportView.DataContext = createReportViewModel;
                    bool? dialogResult = createReportViewModel.createReportView.ShowDialog();
                    // Si l'utilisateur a cliqué sur le bouton "Annuler"
                    // dans son choix du groupe, on sort
                    if (dialogResult == false)
                    {
                        return;
                    }

                    // Création d'un nouveau signalement temporaire.
                    ArcGisProEspaceCollaboratif.Core.Report virtualReport = new ArcGisProEspaceCollaboratif.Core.Report()
                    {
                        // Affectation du commentaire du nouveau signalement.
                        Commentaire = createReportViewModel.createReportView.CommentaireTextBox.Text
                    };


                    /*
                        // Création d'un nouveau signalement temporaire.
                        ArcGisProEspaceCollaboratif.Core.Signalement signalementVirtuel = new ArcGisProEspaceCollaboratif.Core.Signalement();

                        // Affectation du message de la nouvelle remarque.
                        signalementVirtuel.SetCommentaire(formCreateReport.GetMessage());

                        // Récupération des thèmes sélectionnés qui sont ensuite enregistrés dans le fichier XML de paramétrage Ripart.               
                        //signalementVirtuel.AddTheme(formCreateReport.GetSelectedThemes());

                        // Ajout du fichier en pièce-jointe.
                        signalementVirtuel.AddDocument(formCreateReport.GetFichierPJ());

                        // Selon l'option création d'un signalement unique ou un par croquis.
                        if (formCreateReport.OptionSingleSignalement())
                        {
                            // Positionnement du signalement unique par rapport à l'ensemble des croquis.
                            signalementVirtuel.SetPosition(Helper.CalculatePointReport(futursSketch));

                            // Si option de joindre un croquis au nouveau signalement.
                            if (formCreateReport.OptionWithCroquis())
                            {
                                signalementVirtuel.AddCroquis(futursSketch);
                            }

                            // Création du nouveau signalement
                            ArcGisProEspaceCollaboratif.Core.Signalement signalementNouveau = context.Client.CreateSignalement(signalementVirtuel);
                            await context.CreerPointSignalement(signalementNouveau);

                            var connectInfoViewModel = new FeedbackInformationViewModel();
                            connectInfoViewModel.feedbackInformationView.DataContext = connectInfoViewModel;
                            connectInfoViewModel.Logo = context.Client.GetProfil().Logo;
                            string message = string.Format("Succès : création d'un nouveau signalement n°{0}", signalementNouveau.Id);
                            connectInfoViewModel.MessageFeedback = message;
                            connectInfoViewModel.feedbackInformationView.ShowDialog();

                            logger.Info(message);
                        }
                        else
                        {
                            // Parcours des croquis un par un
                            List<ulong> listIdNouveauxSignalements = new List<ulong>();

                            foreach (ArcGisProEspaceCollaboratif.Core.Sketch croquis in futursSketch)
                            {
                                // Positionnement de la remarque par rapport au croquis un par un.
                                signalementVirtuel.SetPosition(Helper.CalculatePointReport(croquis));
                                signalementVirtuel.ClearCroquis();

                                // Si option de joindre un croquis à la nouvelle remarque.
                                if (formCreateReport.OptionWithCroquis())
                                {
                                    signalementVirtuel.AddCroquis(croquis);
                                }

                                // Création de la nouvelle remarque.
                                ArcGisProEspaceCollaboratif.Core.Signalement signalementNouveau = context.Client.CreateSignalement(signalementVirtuel);
                                await context.CreerPointSignalement(signalementNouveau);

                                listIdNouveauxSignalements.Add(signalementNouveau.Id);
                            }

                            var connectInfoViewModel = new FeedbackInformationViewModel();
                            connectInfoViewModel.feedbackInformationView.DataContext = connectInfoViewModel;
                            connectInfoViewModel.Logo = context.Client.GetProfil().Logo;
                            string message = string.Format("Succès de la création de {0} nouveaux signalements pour l'espace collaboratif.\n", listIdNouveauxSignalements.Count);
                            message += string.Format("Les identifiants de ces nouvelles remontées vont de {0} à {1}.", listIdNouveauxSignalements.First(), listIdNouveauxSignalements.Last());
                            connectInfoViewModel.MessageFeedback = message;
                            connectInfoViewModel.feedbackInformationView.ShowDialog();
                            logger.Info(message);
                        }*/

                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(
                        e.Message,
                        Constantes.ERROR,
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error
                    );
                    logger.Error(string.Format("Problème dans la création des signalements : {0}\n{1}", e.Message,e.StackTrace));
                }
            });
        }

        private void CreateNewReport()
        {

        }
    }
}
