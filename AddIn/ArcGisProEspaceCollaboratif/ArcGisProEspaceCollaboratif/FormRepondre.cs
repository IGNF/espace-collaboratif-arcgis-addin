using System;
using System.Windows.Forms;
using System.Net;

namespace ArcGisProEspaceCollaboratif
{
    public partial class FormRepondre : Form
    {
        public FormRepondre()
        {
            InitializeComponent();

            this.comboBoxStatut.Items.Add("Reçue dans nos services" ); // 0
            this.comboBoxStatut.Items.Add("En cours de traitement"  ); // 1           
            this.comboBoxStatut.Items.Add("Demande de qualification"); // 2
            this.comboBoxStatut.Items.Add("En attente de saisie"    ); // 3
            this.comboBoxStatut.Items.Add("En attente de validation"); // 4
            this.comboBoxStatut.Items.Add("Prise en compte"         ); // 5
            this.comboBoxStatut.Items.Add("Déjà prise en compte"    ); // 6
            this.comboBoxStatut.Items.Add("Rejetée (hors spec.)"    ); // 7
            this.comboBoxStatut.Items.Add("Rejetée (hors propos)"   ); // 8

            this.richTextBoxReponse.Focus();
        }

        private void FormReponse_Load(object sender, EventArgs e)
        {
        }

        public void SetStatut( uint statut )
        {
            this.comboBoxStatut.SelectedIndex = (int) statut;
        }


        public void GetStatut(ref ArcGisProEspaceCollaboratif.Core.Report signalement)
        {
            signalement.Statut = (ArcGisProEspaceCollaboratif.Core.Statut) this.comboBoxStatut.SelectedIndex;
        }


        public void SetIdRemarque(string id)
        {
            this.labelIdSignalement.Text = "Message du signalement n°" + id;
        }

        public void SetMessage(string message)
        {
            this.webBrowserMessage.DocumentText = message;
        }

        public void SetAnciennesReponses(string reponses)
        {
            this.webBrowserReponseAncienne.DocumentText = reponses;
        }       

        public void ButtonRepondre_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        public string GetReponse()
        {
            return this.richTextBoxReponse.Text;
        }

        public string GetTitre()
        {
            return this.txtRespTitre.Text;
        }

        private void RichTextBoxReponse_TextChanged(object sender, EventArgs e)
        {
            this.buttonRepondre.Enabled = ( richTextBoxReponse.TextLength != 0 );
        }

        public void SetFormulaire(ArcGisProEspaceCollaboratif.Core.Report signalement)
        {
            this.SetIdRemarque(signalement.Id.ToString() );
            this.SetStatut((uint)signalement.Statut);
            this.SetMessage(signalement.GetUrlDecodedComment());
            this.SetAnciennesReponses(signalement.ConcatenateReponseHTML() ); 
        }

        private void TableLayoutPanel_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
