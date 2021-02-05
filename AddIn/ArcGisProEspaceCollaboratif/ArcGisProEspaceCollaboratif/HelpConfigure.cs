using System;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGisProEspaceCollaboratif.Core;
using log4net;

namespace ArcGisProEspaceCollaboratif
{
    internal class HelpConfigure : Button
    {
        private readonly EspaceCollaboratifLogger riplogger = EspaceCollaboratifLogger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(Connecter));

        protected override void OnClick()
        {
            //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Se connecter à l'espace collaboratif", "Espace collaboratif");
            logger.Debug("Clic sur le bouton de configuration de l'add-in Espace collaboratif");
            try
            {
                Contexte contexte = Contexte.Instance;

                if (!contexte.CheckConfigFile())
                {
                    System.Windows.Forms.MessageBox.Show(@"Le fichier " + contexte.repertoireTravail +
                                    Helper.nom_Fichier_Parametres_EspaceCollaboratif + @" n'existe pas");

                }

                /* [NG] Partie issue du code existant mais qui ne me semble pas appropriée ici :
                 * on peut très bien ouvrir les paramètres avant de se connecter à l'Espace co.
                 * TO-DO : à confirmer et supprimer
                if (contexte.ripClient == null)
                {
                    contexte.ripClient = (Client)contexte.GetConnexionEspaceCollaboratif();
                    if (contexte.ripClient == null) return;
                }
                */

                FormConfigurer configurateur = new FormConfigurer(contexte);
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
