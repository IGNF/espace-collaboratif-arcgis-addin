using System;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGisProEspaceCollaboratif.Core;
using ArcGisProEspaceCollaboratif.ViewModels;
using log4net;

namespace ArcGisProEspaceCollaboratif
{
    internal class HelpConfigure : Button
    {
        private static readonly Logger riplogger = Logger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(HelpConfigure));

        protected override void OnClick()
        {
            logger.Debug("Clic sur le bouton de configuration de l'add-in Espace collaboratif");
            try
            {
                Context context = Context.Instance;

                // Test de la présence du fichier XML de paramétrage
                context.CheckConfigFile();

                HelpConfigureViewModel helpConfigureViewModel = new(context);
                helpConfigureViewModel.helpConfigureView.DataContext = helpConfigureViewModel;
                bool? dialogResult = helpConfigureViewModel.helpConfigureView.ShowDialog();
                // L'utilisateur a cliqué sur la croix pour fermer le dialogue
                if (dialogResult == false)
                {
                    helpConfigureViewModel.helpConfigureView.Close();
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
                return;
            }
        }
    }
}
