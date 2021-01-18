using System;
using System.IO;
using log4net.Config;

namespace EspaceCollaboratif.Core
{
    /// <summary>
    /// Classe qui initialise le logger
    /// </summary>
    public  class EspaceCollaboratifLogger 
    {
       //instance unique de EspaceCollaboratifLogger
       private static EspaceCollaboratifLogger instance;
       private static readonly object padlock = new object();

       //le fichier de configuration du log
       private readonly String configFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EspaceCollaboratif\\log4net.config";
       //le chemin vers le répertoire du fichier de log
       private static String logPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EspaceCollaboratif\\logs";

       //niveau de log
       private readonly String level = "ALL";


        /// <summary>
        /// retourne l'instance du EspaceCollaboratifLogger
        /// </summary>
        public static EspaceCollaboratifLogger Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new EspaceCollaboratifLogger();

                        }
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Constructeur
        /// Crée le fichier de configuration s'il n'existe pas et configure le chemin du XNLConfigurateur de log4Net
        /// </summary>
        private EspaceCollaboratifLogger()
        {
         
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            StartLog();
           
            XmlConfigurator.Configure(new FileInfo(configFile));

           
        }


        /// <summary>
        /// Création du fichier de configuration du log (log4net.config), s'il n'existe pas
        /// </summary>
        private void StartLog()
        {                    
            if (!File.Exists(configFile))

            {
                String logConfig = "<log4net>\n" +
                                  "<appender name=\"RollingFile\" type=\"log4net.Appender.RollingFileAppender\">\n" +
                                   "<file value=\"${APPDATA}\\EspaceCollaboratif\\logs\\\"/>\n" +
                                    "<appendToFile value=\"true\" />\n" +
                                    "<maximumFileSize value=\"100KB\" />\n" +
                                    "<maxSizeRollBackups value=\"2\" />\n" +
                                    "<staticLogFileName value=\"false\" />\n" +
                                    "<rollingStyle value=\"Composite\"/>\n" +
                                    "<datePattern value=\"yyyy.MM.dd&quot;_EspaceCollaboratif.log&quot;\"/>\n" +
                                    "<layout type=\"log4net.Layout.PatternLayout\">\n" +
                                     " <conversionPattern value=\"%date [%thread] %-5level %logger  - %message%newline\" />\n" +
                                    "</layout>\n" +
                                  "</appender>\n" +
                                  "<root>\n" +
                                   " <level value=\"" + level + "\" />   \n" +
                                    "<appender-ref ref=\"RollingFile\" />\n" +
                                  "</root>\n" +
                                "</log4net>";


                System.IO.File.WriteAllText(configFile, logConfig);
            }

  
            //on ne garde que le fichier de log du jour courant
            DirectoryInfo d = new DirectoryInfo(logPath);
            FileInfo[] Files = d.GetFiles("*.log");
            String date = DateTime.Now.Year + "." + DateTime.Now.Month.ToString().PadLeft(2, '0') + "." + (DateTime.Now.Day).ToString().PadLeft(2, '0');

            foreach (FileInfo file in Files)
            {
                if (!file.Name.StartsWith(date))
                {
                    File.Delete(logPath + "\\" + file);
                }
            }

        }

        /// <summary>
        /// Getteur public pour retourner le chemin d'accès où est situé le fichier log
        /// </summary>
        public static string GetLogPath()
        {
            return logPath;
        }

    }
}