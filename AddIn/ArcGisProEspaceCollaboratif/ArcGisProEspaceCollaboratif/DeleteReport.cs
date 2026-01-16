using System;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGisProEspaceCollaboratif.Core;
using log4net;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Framework;

namespace ArcGisProEspaceCollaboratif
{
    internal class DeleteReport : Button
    {
        private static readonly Logger riplogger = Logger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(DeleteReport));

        protected override async void OnClick()
        {
            logger.Debug("Click sur le bouton de suppression de tous les objets des couches de signalement");
            await QueuedTask.Run(() =>
            {
                ArcGIS.Desktop.Framework.Threading.Tasks.ProgressDialog progressDialog = new ProgressDialog("Suppression des signalements et croquis dans la carte...");
                progressDialog.Show();
                try
                {
                    Context context = Context.Instance;
                    // Il faut s'être connecté au service pour la créer un signalement
                    if (context.Client == null)
                    {
                        // Établissement de la connexion avec le service Espace collaboratif.
                        ArcGisProEspaceCollaboratif.Core.Client client = null;
                        context.GetConnexionEspaceCollaboratif(ref client);
                        context.Client = client;
                        if (context.Client == null)
                        {
                            progressDialog.Hide();
                            return;
                        }
                    }

                    bool bRes = Context.IsLayerInMap(Helper.name_layer_Signalement);
                    if (!bRes)
                    {
                        string mess = "Pas de couche 'Signalement' dans la carte.\nIl est donc impossible de supprimer le contenu des couches de signalements.\nIl faut se connecter à l'Espace collaboratif et télécharger les signalements.";
                        logger.Error(string.Format("CreateReport.OnClick.context.IsLayerInMap : {0}\n", mess));
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                            mess,
                            Constantes.ERROR);
                        progressDialog.Hide();
                        return;
                    }

                    // Demande de confirmation
                    string message = string.Format("Vous allez supprimer les signalements et croquis de votre projet ArcGIS. Ceux-ci seront toutefois toujours accessibles sur l'Espace collaboratif. \nSouhaitez-vous poursuivre la suppression ?");
                    System.Windows.MessageBoxResult messageBoxResult = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.QUESTION, System.Windows.MessageBoxButton.YesNo);
                    if (messageBoxResult == System.Windows.MessageBoxResult.Cancel ||
                        messageBoxResult == System.Windows.MessageBoxResult.No ||
                        messageBoxResult == System.Windows.MessageBoxResult.None)
                    {
                        progressDialog.Hide();
                        return;
                    }

                    // Vide la Geodatabase pour les couches "Signalement", "Croquis_EC_Polygone", "Croquis_EC_Ligne", "Croquis_EC_Point" 
                    foreach (string layerName in Helper.CollaborativeSpaceLayers)
                    {
                        context.EmptyCollabFeatureClasses(layerName);
                    }
                    progressDialog.Hide();

                }
                catch (Exception e)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                        e.Message,
                        Constantes.ERROR
                    );
                    string message = string.Format("Problème dans la suppression des objets des couches signalements : {0}\n{1}", e.Message, e.StackTrace);
                    logger.Error(string.Format("DeleteReport.OnClick : {0}\n", message));
                    progressDialog.Hide();
                }
            });
        }
    }
}
