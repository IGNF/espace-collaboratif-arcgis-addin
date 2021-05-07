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
                    List<ArcGisProEspaceCollaboratif.Core.Sketch> futursSketch = context.MakeSketchFromSelection();
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
                    var createReportViewModel = new CreateReportViewModel(context, futursSketch);
                    createReportViewModel.createReportView.DataContext = createReportViewModel;
                    bool? dialogResult = createReportViewModel.createReportView.ShowDialog();
                    // Si l'utilisateur a cliqué sur le bouton "Annuler"
                    // dans son choix du groupe, on sort
                    if (dialogResult == false)
                    {
                        return;
                    }
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
    }
}
