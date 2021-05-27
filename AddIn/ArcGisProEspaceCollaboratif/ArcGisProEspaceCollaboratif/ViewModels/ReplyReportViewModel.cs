using ArcGisProEspaceCollaboratif.Utils;
using ArcGisProEspaceCollaboratif.Views;
using ArcGisProEspaceCollaboratif.Core;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Collections.Generic;
using static ArcGisProEspaceCollaboratif.Core.Status;

namespace ArcGisProEspaceCollaboratif.ViewModels
{
    class ReplyReportViewModel
    {
        #region Parameters

        /// <summary>
        /// L'instance du dialogue "Répondre à un signalement"
        /// </summary>
        public ReplyReportView replyReportView;

        /// <summary>
        /// Le contexte de travail
        /// </summary>
        public Context Context { get; set; }

        /// <summary>
        /// La liste des signalements qui sont autorisés à avoir une réponse
        /// </summary>
        public List<Report> Reports { get; set; }

        /// <summary>
        /// Le message qui contient les signalements qui n'auront pas de réponses
        /// PAs d'autorisation d'écriture sur le serveur ou statut clôturé
        /// </summary>
        string Message { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialisation du dialogue "Répondre à un signalement"
        /// </summary>
        public ReplyReportViewModel(Context context, List<Report> reports, string error)
        {
            this.Context = context;
            this.Reports = reports;
            this.Message = error;
            this.replyReportView = new ReplyReportView();
            this.InitializeCreateReportView();
        }

        #endregion

        #region Initialize Dialog

        /// <summary>
        /// Initialisation du contenu du dialogue "Nouvelle réponse"
        /// </summary>
        private void InitializeCreateReportView()
        {
            // Mise à jour de la ComboBox "Statut" avec ses libellés
            this.StatutItemsSourceComboBox = ListStatutWording;
            int numberReports = this.Reports.Count;
            if (numberReports == 1)
            {
                this.NumberReportLabel = string.Format("Réponse au signalement n°{0}", this.Reports[0].Id);
            }
            else
            {
                this.NumberReportLabel = string.Format("Attention, {0} signalements sélectionnés", numberReports);
            }
            this.NewStatusToolTip = Helper.GetFileAboutStatusResponse();
        }

        #endregion

        #region Bindings

        /// <summary>
        /// Message d'avertissement à l'utilisateur sur le nombre de signalements sélectionnés
        /// </summary>
        public string NumberReportLabel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<string> StatutItemsSourceComboBox { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string StatutSelectedItemComboBox { get; set; } = Status.ListStatutWording[0];

        /// <summary>
        /// 
        /// </summary>
        public string NewResponseTextBox { get; set; } = "";

        public string NewStatusToolTip { get; set; } = "";

        #endregion

        #region Commands

        public ICommand SendResponseButtonCmd { get { return new RelayCommand(OnSend, AlwaysTrue); } }

        /// <summary>
        /// L'utilisateur a cliqué sur le bouton "Envoyer"
        /// Le ou les signalements sont envoyés sur l'espace collaboratif
        /// </summary>
        private void OnSend()
        {
            /*string groupName = this.GroupSelectedItemComboBox;
            Helper.Save_PreferredGroup(groupName);

            List<Theme> themesSelected = GetSelectedThemes();
            Helper.Save_PreferredThemes(themesSelected);

            // Création d'un nouveau signalement temporaire.
            this.VirtualReport = new ArcGisProEspaceCollaboratif.Core.Report()
            {
                Commentary = CommentaryTextBox,
                Author = this.Context.Profil.Author,
                Group = this.GetGroupSelectedItemComboBox(),
                DateCreation = DateTime.Today,
                DateValidation = DateTime.Today,
                Status = Status.Undefined
            };

            this.VirtualReport.AddTheme(themesSelected);
            this.VirtualReport.AddDocument(this.NameFileJoinToReport);

            // Option création d'un signalement unique
            if (this.CreateReportIsChecked)
            {
                CreateReport();
            }

            // Option création de plusieurs signalements
            if (this.CreateReportsIsChecked)
            {
                CreateReports();
            }*/

            if (Message != "")
            {
                System.Windows.Forms.MessageBox.Show(
                    this.Message,
                    Constantes.WARNING,
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning
                    );
            }
        }
        private bool AlwaysTrue() { return true; }

        #endregion

        #region Methods
        
        #endregion
    }
}
