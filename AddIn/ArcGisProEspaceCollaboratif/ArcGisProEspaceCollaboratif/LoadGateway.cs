using System;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGisProEspaceCollaboratif.Core;
using log4net;

namespace ArcGisProEspaceCollaboratif
{
    /// <summary>
    /// Chargement des couches du groupe utilisateur de l'Espace collaboratif
    /// </summary>
    internal class LoadGateway : Button
    {
        //private readonly Logger riplogger = Logger.Instance;
        //private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(Connecter));

        protected override void OnClick()
        {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Charger les couches de mon groupe", "Espace collaboratif");
            //logger.Debug("Click sur le bouton de chargement des couches du groupe utilisateur");
            /*try
            {
                Contexte contexte = Contexte.Instance;
                if(contexte == null)
                {
                    Enabled = false;
                    return;
                }

                if (contexte.profil != null)
                {
                    if (contexte.profil.Geogroupes.Count == 1)
                    {
                        if (contexte.profil.Geogroupes[0].Layers.Count == 0)
                        {
                            throw new Exception("Votre groupe n'a pas paramétré sa carte, il n'y a pas de données à charger.");
                        }
                    }
                }
                FormLoadGateway formLoadGateway = new FormLoadGateway(contexte);
                formLoadGateway.ShowDialog();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message,
                 "IGN Espace collaboratif - ERREUR", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                logger.Error(e.Message + "\n" + e.StackTrace);
            }*/
        }
    }
}
