using System;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGisProEspaceCollaboratif.Core;
using log4net;

namespace ArcGisProEspaceCollaboratif
{
    internal class Connecter : Button
    {
        private readonly Logger riplogger = Logger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(Connecter));

        protected override void OnClick()
        {
            //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Se connecter à l'espace collaboratif", "Espace collaboratif");
            logger.Debug("Click sur le bouton de connexion au service de l'Espace collaboratif");
            try
            {
                Contexte contexte = Contexte.Instance;
                ArcGisProEspaceCollaboratif.Core.Client connexionTemp = contexte.GetConnexionEspaceCollaboratif();
                contexte.Client = (Client)connexionTemp;
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message,
                 "IGN Espace collaboratif - ERREUR", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                logger.Error(e.Message + "\n" + e.StackTrace);
            }
        }
    }
}
