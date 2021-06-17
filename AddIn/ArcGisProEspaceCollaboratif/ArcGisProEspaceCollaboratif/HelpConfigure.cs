using System;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGisProEspaceCollaboratif.Core;
using ArcGisProEspaceCollaboratif.ViewModels;
using log4net;

namespace ArcGisProEspaceCollaboratif
{
    internal class HelpConfigure : Button
    {
        private static readonly Logger riplogger = Logger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(HelpConfigure));

        protected override async void OnClick()
        {
            logger.Debug("Clic sur le bouton de configuration de l'add-in Espace collaboratif");
            await QueuedTask.Run(() =>
            {
                try
                {
                    Context context = Context.Instance;

                    // Il faut s'être connecté au service pour configurer
                    if (context.Client == null)
                    {
                        // Établissement de la connexion avec le service Espace collaboratif.
                        context.Client = context.GetConnexionEspaceCollaboratif();
                        if (context.Client == null)
                        {
                            return;
                        }
                    }

                    if (!context.CheckConfigFile())
                    {
                        string message = string.Format("Le fichier '{0}{1}' n'existe pas", context.DirectoryWorking, Helper.name_file_espaceco_xml);
                        logger.Error(string.Format("HelpConfigure.OnClick.context.CheckConfigFile : {0}\n", message));
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                            message,
                            Constantes.STOP
                        );
                        return;
                    }

                    var helpConfigureViewModel = new HelpConfigureViewModel(context);
                    helpConfigureViewModel.helpConfigureView.DataContext = helpConfigureViewModel;
                    bool? dialogResult = helpConfigureViewModel.helpConfigureView.ShowDialog();
                    // L'utilisateur a cliqué sur la croix pour fermer le dialogue
                    if (dialogResult == false)
                    {
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
                    logger.Error(string.Format("HelpConfigure.OnClick : {0}\n", message));
                }
            });
        }
    }
}
