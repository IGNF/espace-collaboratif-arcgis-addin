using System;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGisProEspaceCollaboratif.Core;
using log4net;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGisProEspaceCollaboratif.ViewModels;

namespace ArcGisProEspaceCollaboratif
{
    internal class CreerSignalement : Button
    {
        private readonly Logger riplogger = Logger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(CreerSignalement));

        protected override async void OnClick()
        {
            logger.Debug("Click sur le bouton de création d'un nouveau signalement");
    
            await QueuedTask.Run(async () =>
            {
                try
                {
                    Contexte contexte = Contexte.Instance;

                    // Il faut s'être connecté au service pour la créer un signalement
                    if (contexte.Client == null)
                    {
                        // Établissement de la connexion avec le service Espace collaboratif.
                        contexte.Client = contexte.GetConnexionEspaceCollaboratif();
                        if (contexte.Client == null)
                        {
                            return;
                        }
                    }

                    // Est-ce que la couche signalement existe dans la carte ?
                    bool bRes = contexte.IsLayerInMap(Helper.nom_Calque_Signalement);
                    if (!bRes)
                    {
                        string message = "Pas de couche Signalement dans la carte.\nIl est donc impossible de créer un nouveau signalement.\nIl faut se connecter à l'Espace collaboratif et et télécharger les signalements.";
                        System.Windows.Forms.MessageBox.Show(message,
                            "IGN Espace collaboratif - ERREUR", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        logger.Debug(message);
                        return;
                    } 

                    // Transformation des objets sélectionnés en croquis.
                    List<ArcGisProEspaceCollaboratif.Core.Sketch> futursSketch = contexte.MakeCroquis_from_Selection();
                    logger.Debug(futursSketch.Count + " croquis générés.");
                    if (futursSketch.Count == 0)
                    {
                        string message = "Aucun objet sélectionné.\nIl est donc impossible de déterminer le point d'application du nouveau signalement à créer.";
                        System.Windows.Forms.MessageBox.Show(message,
                            "IGN Espace collaboratif - WARNING", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        logger.Debug(message);
                        return;
                    }

                    // Lancement du formulaire pour créer une nouvelle remarque
                    FormCreateReport formCreateReport = new FormCreateReport(contexte)
                    {
                        SketchNumber = futursSketch.Count,
                        ListFilesPJ = new List<string>()
                    };
                    if (formCreateReport.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        return;
                    }

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
                        ArcGisProEspaceCollaboratif.Core.Signalement signalementNouveau = contexte.Client.CreateSignalement(signalementVirtuel);
                        await contexte.CreerPointSignalement(signalementNouveau);

                        var connectInfoViewModel = new ConnectFeedbackInformationViewModel();
                        connectInfoViewModel.connectFeedbackInformationView.DataContext = connectInfoViewModel;
                        connectInfoViewModel.Logo = contexte.Client.GetProfil().Logo;
                        string message = string.Format("Succès : création d'un nouveau signalement n°{0}", signalementNouveau.Id);
                        connectInfoViewModel.MessageFeedback = message;
                        connectInfoViewModel.connectFeedbackInformationView.ShowDialog();

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
                            ArcGisProEspaceCollaboratif.Core.Signalement signalementNouveau = contexte.Client.CreateSignalement(signalementVirtuel);
                            await contexte.CreerPointSignalement(signalementNouveau);

                            listIdNouveauxSignalements.Add(signalementNouveau.Id);
                        }

                        var connectInfoViewModel = new ConnectFeedbackInformationViewModel();
                        connectInfoViewModel.connectFeedbackInformationView.DataContext = connectInfoViewModel;
                        connectInfoViewModel.Logo = contexte.Client.GetProfil().Logo;
                        string message = string.Format("Succès de la création de {0} nouveaux signalements pour l'espace collaboratif.\n", listIdNouveauxSignalements.Count);
                        message += string.Format("Les identifiants de ces nouvelles remontées vont de {0} à {1}.", listIdNouveauxSignalements.First(), listIdNouveauxSignalements.Last());
                        connectInfoViewModel.MessageFeedback = message;
                        connectInfoViewModel.connectFeedbackInformationView.ShowDialog();
                        logger.Info(message);
                    }
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message,
                     "IGN Espace collaboratif - ERREUR", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    logger.Error("Problème dans la création des signalements " + e.Message + "\n" + e.StackTrace);
                }
            });
        }
    }
}
