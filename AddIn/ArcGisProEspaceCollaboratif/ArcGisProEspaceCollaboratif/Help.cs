using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGisProEspaceCollaboratif.Core;
using ArcGisProEspaceCollaboratif.ViewModels;
using log4net;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ArcGisProEspaceCollaboratif
{
    internal class HelpManual : Button
    {
        private static readonly Logger riplogger = Logger.Instance;
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

    internal class HelpFileLog : Button
    {
        private static readonly Logger riplogger = Logger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(HelpFileLog));

        protected override void OnClick()
        {
            logger.Debug("Clic sur le bouton d'ouverture du fichier de log de l'add-in Espace collaboratif");

            string logPath = Logger.GetLogPath();
            DirectoryInfo directory = new DirectoryInfo(logPath);
            string fileLog = string.Format("*{0}", Constantes.NAMELOGFILE);
            try
            {
                var logFile = (from f in directory.GetFiles(fileLog)
                               orderby f.LastWriteTime descending
                               select f).First();

                System.Diagnostics.Process.Start(logFile.FullName);
            }
            catch
            {
                string message = string.Format("Le fichier yyyy.MM.dd{0} n'existe pas le répertoire {1}", Constantes.NAMELOGFILE, logPath);
                logger.Error(string.Format("HelpFileLog.OnClick : {0}\n", message));
                MessageBox.Show(message,Constantes.ERROR);
            }
        }
    }

    internal class HelpAbout : Button
    {
        private static readonly Logger riplogger = Logger.Instance;
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
            logger.Info(message);
            FeedbackInformationViewModel feedbackInformationViewModel = new FeedbackInformationViewModel()
            {
                MessageFeedback = message,
                Logo = string.Format("{0}logo_IGN.png", Helper.EspaceCollaboratifDirectoryImages)
            };
            feedbackInformationViewModel.feedbackInformationView.DataContext = feedbackInformationViewModel;
            bool? dialogResult = feedbackInformationViewModel.feedbackInformationView.ShowDialog();
            if (dialogResult == false)
            {
                feedbackInformationViewModel.feedbackInformationView.Close();
            }
        }
    }

}
