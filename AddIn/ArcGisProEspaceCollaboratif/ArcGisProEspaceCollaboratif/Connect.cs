using System;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGisProEspaceCollaboratif.Core;
using log4net;

namespace ArcGisProEspaceCollaboratif
{
    internal class Connect : Button
    {
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(Connect));

        protected override async void OnClick()
        {
            logger.Debug("Click sur le bouton de connexion au service de l'Espace collaboratif");
            await QueuedTask.Run(async () =>
            {
                try
                {
                    Context context = Context.Instance;
                    context.Client = context.GetConnexionEspaceCollaboratif();
                }
                catch (Exception e)
                {
                    string message = string.Format("{0}\n{1}", e.Message, e.StackTrace);
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                        message,
                        Constantes.ERROR
                    );
                    logger.Error(message);
                }
            });
        }
    }
}
