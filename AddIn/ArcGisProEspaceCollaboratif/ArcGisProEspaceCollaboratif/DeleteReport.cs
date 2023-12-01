using System;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGisProEspaceCollaboratif.Core;
using log4net;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Threading.Tasks;
using System.Collections.Generic;
using ArcGIS.Desktop.Mapping;
using System.Linq;

namespace ArcGisProEspaceCollaboratif
{
    internal class DeleteReport : Button
    {
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(DeleteReport));

        protected override async void OnClick()
        {
            logger.Debug("Click sur le bouton de suppression de tous les objets des couches de signalement");
            await QueuedTask.Run(() =>
            {
                try
                {
                    Context context = Context.Instance;

                    // Il faut s'être connecté au service pour la créer un signalement
                    if (context.Client == null)
                    {
                        // Établissement de la connexion avec le service Espace collaboratif.
                        context.Client = context.GetConnexionEspaceCollaboratif();
                        if (context.Client == null)
                        {
                            return;
                        }
                    }

                    // Demande de confirmation
                    string message = string.Format("Vous allez supprimer les signalements et croquis de votre projet ArcGIS. Ceux-ci seront toutefois toujours accessibles sur l'Espace collaboratif. \nSouhaitez-vous poursuivre la suppression ?");
                    System.Windows.MessageBoxResult messageBoxResult = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.QUESTION, System.Windows.MessageBoxButton.YesNo);
                    if (messageBoxResult == System.Windows.MessageBoxResult.Cancel ||
                        messageBoxResult == System.Windows.MessageBoxResult.No)
                    {
                        return;
                    }

                    // Suppression
                    context.EmptyCollabFeatureClasses();

                    IEnumerable<GroupLayer> groupLayers = context.MapActiveView.Map.GetLayersAsFlattenedList().OfType<GroupLayer>();
                    foreach (GroupLayer grLayer in groupLayers)
                    {
                        if (grLayer.Name == Helper.name_group_layer)
                        {
                            context.MapActiveView.Map.RemoveLayer(grLayer);
                        }
                    }
                }
                catch (Exception e)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                        e.Message,
                        Constantes.ERROR
                    );
                    string message = string.Format("Problème dans la suppression des objets des couches signalements : {0}\n{1}", e.Message, e.StackTrace);
                    logger.Error(string.Format("DeleteReport.OnClick : {0}\n", message));
                }
            });
        }
    }
}
