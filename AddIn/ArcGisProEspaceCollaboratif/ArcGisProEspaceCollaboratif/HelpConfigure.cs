using System;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGisProEspaceCollaboratif.Core;
using log4net;

namespace ArcGisProEspaceCollaboratif
{
    internal class HelpConfigure : Button
    {
        private readonly Logger riplogger = Logger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(Connecter));

        protected override void OnClick()
        {
            logger.Debug("Clic sur le bouton de configuration de l'add-in Espace collaboratif");
            try
            {
                Contexte contexte = Contexte.Instance;

                if (!contexte.CheckConfigFile())
                {
                    System.Windows.Forms.MessageBox.Show(@"Le fichier " + contexte.repertoireTravail +
                                    Helper.nom_Fichier_Parametres_EspaceCollaboratif + @" n'existe pas");
                }

                FormSetUp configurateur = new FormSetUp(contexte);
                configurateur.SetTreeViewAttributs(contexte);
                configurateur.Show();

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
