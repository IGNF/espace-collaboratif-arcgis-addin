using System;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
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
        /// <summary>
        /// Le logger qui permet d'enregistrer des informations sur le processus
        /// </summary>
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(LoadGateway));

        protected override async void OnClick()
        {
            logger.Debug("Click sur le bouton de chargement des couches du groupe utilisateur");
            await QueuedTask.Run(() =>
            {
                try
                {
                    Context context = Context.Instance;
                    if (context == null)
                    {
                        Enabled = false;
                        return;
                    }

                    if (context.Profil == null)
                    {
                        ArcGisProEspaceCollaboratif.Core.Client client = null;
                        context.GetConnexionEspaceCollaboratif(ref client);
                        context.Client = client;
                        if (context.Client == null)
                        {
                            string message = "Un problème de connexion avec le service Espace collaboratif est survenu. Veuillez ré-essayer.";
                            logger.Error(string.Format("LoadGateway.OnClick.GetConnexionEspaceCollaboratif : {0}\n", message));
                            throw new Exception(message);
                        }
                    }
                
                    if (string.IsNullOrEmpty(context.Profil.Group.Name))
                    {
                        string message = "Vous n'êtes pas autorisé à effectuer cette opération. Vous n'avez pas de profil actif.";
                        logger.Error(string.Format("LoadGateway.OnClick.context.Profil.Group.Name : {0}\n", message));
                        throw new Exception(message);
                    }

                    if (context.Profil.Geogroupes.Count == 1)
                    {
                        if (context.Profil.Geogroupes[0].Layers.Count == 0)
                        {
                            string message = "Votre groupe n'a pas paramétré sa carte, il n'y a pas de données à charger.";
                            logger.Error(string.Format("LoadGateway.OnClick.context.Profil.Geogroupes[0].Layers.Count : {0}\n", message));
                            throw new Exception();
                        }
                    }

                    // Chargement du dialogue "Charger les couches de mon groupe"
                    LoadGatewayViewModel loadGatewayViewModel = new (context);
                    loadGatewayViewModel.loadGatewayView.DataContext = loadGatewayViewModel;
                    bool? dialogResult = loadGatewayViewModel.loadGatewayView.ShowDialog();
                    if (dialogResult == false)
                    {
                        loadGatewayViewModel.loadGatewayView.Close();
                        return;
                    }
                }
                catch (Exception e)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                        e.Message,
                        Constantes.ERROR
                    );
                    string message = string.Format("{0}\n{1}", e.Message, e.StackTrace);
                    logger.Error(message);
                }
            });
        }
    }
}
