using System;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGisProEspaceCollaboratif.Core;
using log4net;

namespace ArcGisProEspaceCollaboratif
{
    internal class Connect : Button
    {
        private readonly Logger riplogger = Logger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(Connect));

        protected override void OnClick()
        {
            logger.Debug("Click sur le bouton de connexion au service de l'Espace collaboratif");
            try
            {
                Contexte context = Contexte.Instance;
                context.Client = context.GetConnexionEspaceCollaboratif();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(
                    e.Message,
                    Constantes.ERROR,
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error
                );
                logger.Error(string.Format("{0}\n{1}", e.Message, e.StackTrace));
            }
        }
    }
}
