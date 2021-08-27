using ArcGisProEspaceCollaboratif.Utils;
using ArcGisProEspaceCollaboratif.Views;
using ArcGisProEspaceCollaboratif.Core;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Collections.Generic;
using static ArcGisProEspaceCollaboratif.Core.Status;
using System;
using log4net;

namespace ArcGisProEspaceCollaboratif.ViewModels
{
    class ReplyReportViewModel : ViewModelBase
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
        /// Pas d'autorisation d'écriture sur le serveur ou statut clôturé
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
            this.InitializeReplyReportView();
        }

        #endregion

        #region Initialize Dialog

        /// <summary>
        /// Initialisation du contenu du dialogue "Nouvelle réponse"
        /// </summary>
        private void InitializeReplyReportView()
        {
            // Mise à jour de la ComboBox "Statut" avec ses libellés
            this.StatutItemsSourceComboBox = ListWordings;
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
        public string StatutSelectedItemComboBox { get; set; } = Status.ListWordings[0];

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
            try
            {
                foreach (Report report in this.Reports)
                {
                    report.Status = Status.CorrespondenceStatusWording[this.StatutSelectedItemComboBox];
                    Report reportUpdating = this.Context.Client.AddReponse(report, this.NewResponseTextBox);
                    this.Context.UpdateGeodatabase(reportUpdating);   
                }

                // Message de confirmation après l'envoi d'une réponse
                string information = "Votre réponse ";
                if (this.Reports.Count == 1)
                {
                    information += string.Format("au signalement {0} a bien été envoyée.", this.Reports[0].Id);
                }
                else
                {
                    information += string.Format("aux {0} signalements a bien été envoyée.", this.Reports.Count);
                }               
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                    information,
                    Constantes.INFORMATION
                );
            }
            catch(Exception e)
            {
                this.Message += e.Message.ToString();
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                    this.Message,
                    Constantes.WARNING
                );
                logger.Warn(this.Message);
            }
        }
        private bool AlwaysTrue() { return true; }

        #endregion

        #region Methods
        
        #endregion
    }
}
