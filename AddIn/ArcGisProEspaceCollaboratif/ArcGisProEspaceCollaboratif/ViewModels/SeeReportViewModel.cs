using ArcGisProEspaceCollaboratif.Core;
using ArcGisProEspaceCollaboratif.Views;
using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;
using static ArcGisProEspaceCollaboratif.Core.Status;

namespace ArcGisProEspaceCollaboratif.ViewModels
{
    class SeeReportViewModel : ViewModelBase
    {
        #region Parameters

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
        /// La description du signalement contenu dans l'item message
        /// </summary>
        public string SeeDescription { get; set; }

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
            this.seeReportView.ContentNumberReport.Foreground = this.SetArcGisColor();
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
        /// Affichage du contenu du champ message : la description du signalement  
        /// </summary>
        private void DisplayGroupDescription()
        {
            this.SeeDescription = this.ReportAttributes[Helper.name_field_Message];
        }

        /// <summary>
        /// Affichage du lien vers le(s) document(s) attaché(s) au signalement
        /// </summary>
        private void DisplayFilesAttached()
        {
            string document = this.ReportAttributes[Helper.name_field_Document];
            if (string.IsNullOrEmpty(document))
            {
                this.seeReportView.TextBlockHyperlink.Text = "Pas de document(s) attaché(s) au signalement";
            }
            else
            {
                SimpleGetHyperLink(document);
            }
        }

        private void SimpleGetHyperLink(string document)
        {
            this.seeReportView.TextBlockHyperlink.Inlines.Add(document);
        }

        /// <summary>
        /// Création du lien vers le document
        /// Le lien complet est de type : https://espacecollaboratif.ign.fr/document/download/62644
        /// Le numéro du document contient le lien complet
        /// </summary>
        /// <param name="document">La chaine de caractères à transformer en hyperlink</param>
        private void GetHyperLink(string document)
        { 
            int loc = document.LastIndexOf("/", document.Length - 1);
            if (loc == -1)
            {
                Run run = new Run(document);
                
                Hyperlink hyperlink = new Hyperlink(run)
                {
                    NavigateUri = new Uri(document),
                    FontWeight = System.Windows.FontWeights.Bold,
                    Foreground = SetArcGisColor()
                };
                // Ajout du lien au dialogue
                this.seeReportView.TextBlockHyperlink.Inlines.Add(hyperlink);
            }
            else
            {
                string link = document.Substring(loc + 1);
                string TextPrecedingTheHyperlink = document.Substring(0, loc + 1);
                Run run1 = new Run(TextPrecedingTheHyperlink);
                Run run2 = new Run(link);
                Hyperlink hyperlink = new Hyperlink(run2)
                {
                    NavigateUri = new Uri(document),
                    FontWeight = System.Windows.FontWeights.Bold,
                    Foreground = SetArcGisColor()
                };
                // Ouvrir le navigateur qui est bloqué par défaut
                hyperlink.RequestNavigate += LinkOnRequestNavigate;
                // Ajout du lien au dialogue
                this.seeReportView.TextBlockHyperlink.Inlines.Add(run1);
                this.seeReportView.TextBlockHyperlink.Inlines.Add(hyperlink);
            }
        }

        /// <summary>
        /// Pour indiquer à Windows d'ouvrir le navigateur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LinkOnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        /// <summary>
        /// Définition de la couleur bleu ArcGIS pro
        /// </summary>
        /// <returns></returns>
        private SolidColorBrush SetArcGisColor()
        {
            return new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#007AC2"));
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
            string[] firstSeparator = new string[] {"|", ")"};
            foreach (string ch in firstSeparator)
            {
                themes = themes.Replace(ch, "\n");
            }

            string[] secondSeparator = new string[] {"(", ","};
            foreach (string ch in secondSeparator)
            {
                themes = themes.Replace(ch, "\n    ");
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
        private string GetDisplaySource(string stringToSearch)
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
            return sources[stringToSearch];
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
