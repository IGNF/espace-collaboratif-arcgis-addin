using System;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGisProEspaceCollaboratif.Core;
using log4net;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace ArcGisProEspaceCollaboratif
{
    internal class DeleteReport : Button
    {
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(CreateReport));

        protected override async void OnClick()
        {
            logger.Debug("Click sur le bouton de suppression de tous les objets des couches de signalement");
            await QueuedTask.Run(async () =>
            {
                try
                {
                    Context context = Context.Instance;

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
                    // Vérification si les couches "Signalement", "Croquis_EC_Polygone", "Croquis_EC_Ligne", "Croquis_EC_Point" existent
                    await context.CreateOrLoadReportLayers();
                    // Suppression
                    context.RemoveAllObjectsFromLayers();
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(
                        e.Message,
                        Constantes.ERROR,
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error
                    );
                    logger.Error(string.Format("Problème dans la suppression des objets des couches signalements : {0}\n{1}", e.Message, e.StackTrace));
                }
            });
        }
    }
}
