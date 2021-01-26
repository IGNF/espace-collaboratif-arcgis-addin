using System;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGisProEspaceCollaboratif.Core;
using log4net;

namespace ArcGisProEspaceCollaboratif
{
    internal class CreerSignalement : Button
    {
        private readonly EspaceCollaboratifLogger riplogger = EspaceCollaboratifLogger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(CreerSignalement));

        protected override void OnClick()
        {
            logger.Debug("Click sur le bouton de création d'un nouveau signalement");

            /*try
            {
                Contexte contexte = Contexte.Instance;

                // Transformation des objets sélectionnés en croquis.
                List<ArcGisProEspaceCollaboratif.Core.Croquis> futursCroquis = contexte.MakeCroquis_from_Selection();
                  logger.Debug(futursCroquis.Count + " croquis générés.");

                if (futursCroquis.Count == 0)
                {
                    string message = "Aucun objet sélectionné.\nIl est donc impossible de déterminer le point d'application du nouveau signalement à créer.";
                    System.Windows.Forms.MessageBox.Show(message,
                        "IGN Espace collaboratif - WARNING", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    logger.Debug(message);
                    return;
                }

                if (contexte.ripClient == null)
                {   
                    // Établissement de la connexion avec le service Espace collaboratif.
                    contexte.ripClient = (Client)contexte.GetConnexionRipart();
                    if (contexte.ripClient == null)
                    {
                        return;
                    }
                }

                EspaceCollaboratifHelper.MessageBar("Création d'un nouveau signalement");

                // Lancement du formulaire pour créer une nouvelle remarque Ripart.
                FormCreerSignalement formulaireCreation = new FormCreerSignalement();
                formulaireCreation.SetFormulaire(futursCroquis.Count, contexte, contexte.ripClient);
                if (formulaireCreation.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                // Création d'un nouveau signalement temporaire.
                ArcGisProEspaceCollaboratif.Core.Signalement signalementVirtuel = new ArcGisProEspaceCollaboratif.Core.Signalement();

                // Affectation du message de la nouvelle remarque.
                signalementVirtuel.SetCommentaire(formulaireCreation.GetMessage());

                // Récupération des thèmes sélectionnés qui sont ensuite enregistrés dans le fichier XML de paramétrage Ripart.               
                signalementVirtuel.AddTheme(formulaireCreation.GetSelectedThemes());

                // Ajout du fichier en pièce-jointe.
                signalementVirtuel.AddDocument(formulaireCreation.GetFichierPJ());

                // Selon l'option création d'un signalement unique ou un par croquis.
                if (formulaireCreation.OptionSingleSignalement())
                {
                    // Positionnement du signalement unique par rapport à l'ensemble des croquis.
                    signalementVirtuel.SetPosition(EspaceCollaboratifHelper.PointApplicationEspaceCollaboratif(futursCroquis));

                    // Si option de joindre un croquis au nouveau signalement.
                    if (formulaireCreation.OptionWithCroquis())
                    {
                        signalementVirtuel.AddCroquis(futursCroquis);
                    }

                    // Création du nouveau signalement
                    ArcGisProEspaceCollaboratif.Core.Signalement signalementNouveau = contexte.ripClient.CreateSignalement(signalementVirtuel);
                    contexte.CreerPointSignalement(signalementNouveau);

                    FormInfo popupInfo = new FormInfo();
                    popupInfo.SetLogo(contexte.ripClient.GetProfil().Logo);
                    string message = "Succès de la création du nouveau signalement n° " + signalementNouveau.Id;
                    popupInfo.SetMessage(message);
                    popupInfo.ShowDialog();

                    logger.Info(message);
                }
                else
                {
                    // Parcours des croquis un par un
                    List<ulong> listIdNouveauxSignalements = new List<ulong>();

                    foreach (ArcGisProEspaceCollaboratif.Core.Croquis croquis in futursCroquis)
                    {
                        // Positionnement de la remarque par rapport au croquis un par un.
                        signalementVirtuel.SetPosition(EspaceCollaboratifHelper.PointApplicationEspaceCollaboratif(croquis));
                        signalementVirtuel.ClearCroquis();

                        // Si option de joindre un croquis à la nouvelle remarque.
                        if (formulaireCreation.OptionWithCroquis())
                        {
                            signalementVirtuel.AddCroquis(croquis);
                        }

                        // Création de la nouvelle remarque.
                        ArcGisProEspaceCollaboratif.Core.Signalement signalementNouveau = contexte.ripClient.CreateSignalement(signalementVirtuel);
                        contexte.CreerPointSignalement(signalementNouveau);

                        listIdNouveauxSignalements.Add(signalementNouveau.Id);
                    }

                    FormInfo popupRipart = new FormInfo();
                    popupRipart.SetLogo(contexte.ripClient.GetProfil().Logo);
                    string message = "Succès de la création de " + listIdNouveauxSignalements.Count + " nouveaux signalements pour l'espace collaboratif.";
                    popupRipart.SetMessage(message);
                    popupRipart.AddMessage("");
                    popupRipart.AddMessage("Les identifiants de ces nouvelles remarques vont de " + listIdNouveauxSignalements.First() + " à " + listIdNouveauxSignalements.Last() + ".");
                    popupRipart.ShowDialog();

                    logger.Info(message);
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message,
                 "IGN Espace collaboratif - ERREUR", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                logger.Error("Problème dans la création des signalements " + e.Message + "\n" + e.StackTrace);
            }*/
        }
    }
}
