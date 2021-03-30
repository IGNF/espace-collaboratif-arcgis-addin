using System;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGisProEspaceCollaboratif.Core;
using log4net;

namespace ArcGisProEspaceCollaboratif
{
    internal class HelpConfigure : Button
    {
        private readonly Logger riplogger = Logger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(Connect));

        protected override void OnClick()
        {
            logger.Debug("Clic sur le bouton de configuration de l'add-in Espace collaboratif");
            try
            {
                Contexte contexte = Contexte.Instance;

                if (!contexte.CheckConfigFile())
                {
                    string message = string.Format("Le fichier '{0}{1}' n'existe pas", contexte.DirectoryWorking, Helper.name_file_espaceco_xml);
                    System.Windows.Forms.MessageBox.Show(
                        message,
                        Constantes.STOP,
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Stop
                    );
                }

                FormSetUp configurateur = new FormSetUp(contexte);
                configurateur.SetTreeViewAttributs(contexte);
                configurateur.Show();

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
