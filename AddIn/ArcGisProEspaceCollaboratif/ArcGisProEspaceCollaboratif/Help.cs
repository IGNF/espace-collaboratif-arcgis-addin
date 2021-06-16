using ArcGIS.Desktop.Framework.Contracts;
using ArcGisProEspaceCollaboratif.ViewModels;
using log4net;
using System.Diagnostics;
using System.Reflection;

namespace ArcGisProEspaceCollaboratif
{
    internal class HelpManual : Button
    {
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(HelpManual));

        protected override void OnClick()
        {
            logger.Debug("Clic sur le bouton d'ouverture du manuel de l'add-in Espace collaboratif");

            string file = string.Format("{0}{1}", Helper.EspaceCollaboratifDirectoryFiles, Helper.name_file_manuel);
            Process fileopener = new Process();
            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = string.Format("\"{0}\"", file);
            fileopener.Start();
        }
    }

    internal class HelpOpenFileLog : Button
    {
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(HelpOpenFileLog));

        protected override void OnClick()
        {
            logger.Debug("Clic sur le bouton d'ouverture du fichier de log de l'add-in Espace collaboratif");
            string name = logger.Logger.Name.ToString();

            /*string accesLog = Ripart.Core.RipartLogger.getLogPath() + "\\";

            var directory = new DirectoryInfo(accesLog);

            //cherche le fichier le plus récent
            try
            {
                var logFile = (from f in directory.GetFiles("*ripart.log")
                               orderby f.LastWriteTime descending
                               select f).First();

                System.Diagnostics.Process.Start(logFile.FullName);

            }
            catch
            {
                MessageBox.Show("Fichier log inexistant !", "IGN Ripart", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);

            }*/

        }
    }

    internal class HelpAbout : Button
    {
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(HelpAbout));

        protected override void OnClick()
        {
            logger.Debug("Clic sur le bouton d'ouverture de la fenêtre d'informations de l'add-in Espace collaboratif");

            Assembly assembly = Assembly.GetExecutingAssembly();
            string title = ((AssemblyTitleAttribute)assembly.GetCustomAttribute(typeof(AssemblyTitleAttribute))).Title;
            string description = ((AssemblyDescriptionAttribute)assembly.GetCustomAttribute(typeof(AssemblyDescriptionAttribute))).Description;
            string version = ((AssemblyFileVersionAttribute)assembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute))).Version;
            string product = ((AssemblyProductAttribute)assembly.GetCustomAttribute(typeof(AssemblyProductAttribute))).Product;
            string copyright = ((AssemblyCopyrightAttribute)assembly.GetCustomAttribute(typeof(AssemblyCopyrightAttribute))).Copyright;

            string message = string.Format("{0}\n\n", title);
            message += string.Format("{0}\n", description);
            message += string.Format("Version : {0}, '{1}'\n", version, product);
            message += copyright;
            var connectInfoViewModel = new FeedbackInformationViewModel()
            {
                MessageFeedback = message,
                Logo = string.Format("{0}logo_IGN.png", Helper.EspaceCollaboratifDirectoryImages)
            };
            connectInfoViewModel.feedbackInformationView.DataContext = connectInfoViewModel;
            connectInfoViewModel.feedbackInformationView.ShowDialog();
        }
    }

}
