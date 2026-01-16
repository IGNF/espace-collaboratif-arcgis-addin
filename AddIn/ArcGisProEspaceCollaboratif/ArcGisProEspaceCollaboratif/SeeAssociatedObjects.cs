using System;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGisProEspaceCollaboratif.Core;
using log4net;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace ArcGisProEspaceCollaboratif
{
    internal class SeeAssociatedObjects : Button
    {
        private static readonly Logger riplogger = Logger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(SeeAssociatedObjects));

        protected override async void OnClick()
        {
            logger.Debug("Click sur le bouton Voir les objets associés");
            await QueuedTask.Run(() =>
            {
                try
                {
                    Context context = Context.Instance;

                    // Il faut s'être connecté au service pour créer un signalement
                    if (context.Client == null)
                    {
                        // Établissement de la connexion avec le service Espace collaboratif.
                        ArcGisProEspaceCollaboratif.Core.Client client = null;
                        context.GetConnexionEspaceCollaboratif(ref client);
                        context.Client = client;
                        if (context.Client == null)
                        {
                            return;
                        }
                    }

                    // Est-ce que la couche signalement existe dans la carte ?
                    bool bRes = Context.IsLayerInMap(Helper.name_layer_Signalement);
                    if (!bRes)
                    {
                        string mess = "Pas de couche 'Signalement' dans la carte.\nIl est donc impossible de visualiser les associations signalement/croquis.\nIl faut se connecter à l'Espace collaboratif et télécharger les signalements.";
                        logger.Error(string.Format("CreateReport.OnClick.context.IsLayerInMap : {0}\n", mess));
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                            mess,
                            Constantes.ERROR);
                        return;
                    }

                    AssociatedObjects associatedObjects = new (context);
                    associatedObjects.SelectObjects();
                }
                catch (Exception e)
                {     
                    string message = string.Format("Problème dans la visualisation des objets associés : {0}\n{1}", e.Message, e.StackTrace);
                    logger.Error(string.Format("SeeAssociatedObjects.OnClick : {0}\n", message));
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                        e.Message,
                        Constantes.ERROR
                    );
                }
            });
        }
    }
}
