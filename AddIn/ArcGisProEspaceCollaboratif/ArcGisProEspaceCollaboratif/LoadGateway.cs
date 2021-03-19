using System;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGisProEspaceCollaboratif.Core;
using ArcGisProEspaceCollaboratif.ViewModels;
using log4net;

namespace ArcGisProEspaceCollaboratif
{
    /// <summary>
    /// Chargement des couches du groupe utilisateur de l'Espace collaboratif
    /// </summary>
    internal class LoadGateway : Button
    {
        private readonly Logger riplogger = Logger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(Connect));

        protected override void OnClick()
        {
            //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Charger les couches de mon groupe", "Espace collaboratif");
            logger.Debug("Click sur le bouton de chargement des couches du groupe utilisateur");
            try
            {
                Contexte context = Contexte.Instance;
                if (context == null)
                {
                    Enabled = false;
                    return;
                }

                if (context.Profil == null)
                {
                    Client client = context.GetConnexionEspaceCollaboratif();
                    if (client == null)
                    {
                        throw new Exception("Un problème de connexion avec le service Espace collaboratif est survenu. Veuillez ré-éssayer.");
                    }
                }
                
                if (string.IsNullOrEmpty(context.Profil.Group.Name))
                {
                    throw new Exception("Vous n'êtes pas autorisé à effectuer cette opération. Vous n'avez pas de profil actif.");
                }

                if (context.Profil.Geogroupes.Count == 1)
                {
                    if (context.Profil.Geogroupes[0].Layers.Count == 0)
                    {
                        throw new Exception("Votre groupe n'a pas paramétré sa carte, il n'y a pas de données à charger.");
                    }
                }

                // Chargement du dialogue "Charger les couches de mon groupe"
                var loadGatewayViewModel = new LoadGatewayViewModel()
                {
                    Context = context
                };
                loadGatewayViewModel.loadGatewayView.DataContext = loadGatewayViewModel;
                loadGatewayViewModel.loadGatewayView.ShowDialog();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(
                    e.Message,
                    Constantes.ERROR,
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error
                );
                logger.Error(e.Message + "\n" + e.StackTrace);
            }
        }
    }
}
