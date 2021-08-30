using System;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGisProEspaceCollaboratif.Core;
using log4net;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace ArcGisProEspaceCollaboratif
{
    internal class SeeAssociatedObjects : Button
    {
        private static readonly Logger riplogger = Logger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(SeeAssociatedObjects));

        protected override async void OnClick()
        {
            logger.Debug("Click sur le bouton Voir les objets associés");
            await QueuedTask.Run(() =>
            {
                try
                {
                    Context context = Context.Instance;

                    // Il faut s'être connecté au service pour créer un signalement
                    if (context.Client == null)
                    {
                        // Établissement de la connexion avec le service Espace collaboratif.
                        context.Client = context.GetConnexionEspaceCollaboratif();
                        if (context.Client == null)
                        {
                            return;
                        }
                    }
                    AssociatedObjects associatedObjects = new AssociatedObjects(context);
                    associatedObjects.SelectObjects();
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
                        string message = string.Format("Problème dans la visualistion des objets associés : {0}\n{1}", e.Message, e.StackTrace);
                        logger.Error(string.Format("SeeAssociatedObjects.OnClick : {0}\n", message));
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                            e.Message,
                            Constantes.ERROR
                        );
                    }
                }
            });
        }
    }
}
