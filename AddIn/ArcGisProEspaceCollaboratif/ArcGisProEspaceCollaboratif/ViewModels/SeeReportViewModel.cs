using ArcGisProEspaceCollaboratif.Core;
using ArcGisProEspaceCollaboratif.Views;
using log4net;
using System;
using System.Collections.Generic;
using static ArcGisProEspaceCollaboratif.Core.Status;

namespace ArcGisProEspaceCollaboratif.ViewModels
{
    class SeeReportViewModel
    {
        #region Parameters

        private static readonly Logger riplogger = Logger.Instance;
        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(ReplyReport));

        /// <summary>
        /// L'instance du dialogue "Voir un signalement"
        /// </summary>
        public SeeReportView seeReportView;

        /// <summary>
        /// Le contexte de travail
        /// </summary>
        public Context Context { get; set; }

        /// <summary>
        /// Le signalement sélectionné par l'utilisateur
        /// </summary>
        public Dictionary<string, string> ReportAttributes { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialisation du dialogue "Répondre à un signalement"
        /// </summary>
        public SeeReportViewModel(Context context, Dictionary<string, string> attributes)
        {
            this.Context = context;
            this.ReportAttributes = attributes;
            this.seeReportView = new SeeReportView();
            this.InitializeSeeReportView();
        }

        #endregion

        #region Initialize Dialog

        /// <summary>
        /// Initialisation et affichage des attributs du signalement
        /// </summary>
        private void InitializeSeeReportView()
        {
            this.DisplayNumberReport();
            this.DisplayGeneralInformation();
            this.DisplayGroupDescription();
            this.DisplayFilesAttached();
            this.DisplayResponses();
            this.DisplayThemes();
        }

        #endregion

        #region Binding

        /// <summary>
        /// Le numéro du signalement
        /// </summary>
        public string SeeNumberReport { get; set; }

        /// <summary>
        /// Les informations générales du signalement
        /// </summary>
        public string SeeGeneralInformation { get; set; }

        /// <summary>
        /// Le nom du groupe actif
        /// </summary>
        public string SeeGroupDescription { get; set; }

        /// <summary>
        /// Le lien vers le(s) document(s) attaché(s) au signalement
        /// </summary>
        public string SeeFilesAttached { get; set; }

        /// <summary>
        /// La ou les réponses attachées au signalement
        /// </summary>
        public string SeeResponses { get; set; }

        /// <summary>
        /// Les thèmes utilisés lors de la création du signalement
        /// </summary>
        public string SeeThemes { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Affichage du numéro du signalement
        /// </summary>
        private void DisplayNumberReport()
        {
            this.SeeNumberReport = string.Format("Signalement n°{0}", this.ReportAttributes[Constantes.N_REPORT_IN_GDB]);
        }

        private void DisplayGeneralInformation()
        {
            string generalInformation = string.Format("Groupe : {0}\n", this.Context.Groupeactif);
            generalInformation += string.Format("Auteur : {0}\n", this.ReportAttributes[Helper.name_field_Auteur]);
            generalInformation += string.Format("Commune : {0}\n", this.GetTown(this.ReportAttributes[Helper.name_field_Commune], this.ReportAttributes[Helper.name_field_Insee]));
            generalInformation += string.Format("Posté le : {0}\n", this.ReportAttributes[Helper.name_field_DateCreation]);
            generalInformation += string.Format("Statut : {0}\n", Status.GetDisplayStatus((EnumStatus)Enum.Parse(typeof(EnumStatus), this.ReportAttributes[Helper.name_field_Statut], true)));
            generalInformation += string.Format("Source : {0}\n", this.GetDisplaySource(this.ReportAttributes[Helper.name_field_Source]));
            generalInformation += string.Format("Localisation : {0}\n", this.GetDisplayLocation(this.ReportAttributes[Helper.name_field_Longitude], this.ReportAttributes[Helper.name_field_Latitude]));
            this.SeeGeneralInformation = generalInformation;
        }

        /// <summary>
        /// Affichage du nom du groupe actif
        /// </summary>
        private void DisplayGroupDescription()
        {
            this.SeeGroupDescription = this.ReportAttributes[Helper.name_field_Message];
        }

        /// <summary>
        /// Affichage du lien vers le(s) document(s) attaché(s) au signalement
        /// </summary>
        private void DisplayFilesAttached()
        {
            string document = this.ReportAttributes[Helper.name_field_Document];
            if (string.IsNullOrEmpty(document))
            {
                this.SeeFilesAttached = "Pas de document(s) attaché(s) au signalement";
            }
            else
            {
                this.SeeFilesAttached = document;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void DisplayResponses()
        {
            string responses = this.ReportAttributes[Helper.name_field_Reponse];
            if (string.IsNullOrEmpty(responses))
            {
                this.SeeResponses = "Ce signalement n'a pas reçu de réponses.";
            }
            else
            {
                this.SeeResponses = responses;
            } 
        }

        /// <summary>
        /// 
        /// </summary>
        private void DisplayThemes()
        {
            string themes = this.ReportAttributes[Helper.name_field_Themes];
            char[] separator = new char[] { '|', '(', ',', ')' };
            foreach (char ch in separator)
            {
                themes = themes.Replace(ch, '\n');
            }
            this.SeeThemes = themes.Replace("=", " : ");
        }

        private string GetTown(string town, string number)
        {
            return string.Format("{0} ({1})", town, number);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetDisplaySource(string stringToSeach)
        {
            Dictionary<string, string> sources = new Dictionary<string, string>
            {
                { "UNKNOWN", "Soumise via l\'API" },
                { "www", "Saisie sur le site web" },
                { "SIG-GC", "Saisie depuis GeoConcept" },
                { "SIG-AG", "Saisie depuis ArcGIS" },
                { "SIG-QGIS", "Saisie depuis QGIS" },
                { "PHONE", "Saisie depuis un smartphone" },
                { "SPOTIT", "Saisie sur SPOTIT" }
            };
            return sources[stringToSeach];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        /// <returns></returns>
        private string GetDisplayLocation(string longitude, string latitude)
        {
            double lon = Convert.ToDouble(longitude);
            double lat = Convert.ToDouble(latitude);
            string dirLon = "E";
            string dirLat = "N";
            if (lon < 0)
            {
                dirLon = "O";
            }
            if (lat < 0)
            {
                dirLat = "S";
            }
            return string.Format("{0}°{1}, {2}°{3}", longitude.Replace("-", ""), dirLon, latitude.Replace("-", ""), dirLat);
        }


        #endregion
    }
}
