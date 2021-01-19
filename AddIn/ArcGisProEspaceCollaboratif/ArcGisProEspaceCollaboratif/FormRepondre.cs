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

        public void setStatut( uint statut )
        {
            this.comboBoxStatut.SelectedIndex = (int) statut;
        }


        public void getStatut(ref ArcGisProEspaceCollaboratif.Core.Remarque remarque)
        {
            remarque.Statut = (ArcGisProEspaceCollaboratif.Core.Statut) this.comboBoxStatut.SelectedIndex;
        }


        public void setIdRemarque(string id)
        {
            this.labelIdRemarque.Text = "Message de la remarque n°" + id;
        }

        public void setMessage(string message)
        {
            this.webBrowserMessage.DocumentText = message;
        }

        public void setAnciennesReponses(string reponses)
        {
            this.webBrowserReponseAncienne.DocumentText = reponses;
        }       

        public void buttonRepondre_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        public string getReponse()
        {
            return this.richTextBoxReponse.Text;
        }

        public string getTitre()
        {
            return this.txtRespTitre.Text;
        }

        private void richTextBoxReponse_TextChanged(object sender, EventArgs e)
        {
            this.buttonRepondre.Enabled = ( richTextBoxReponse.TextLength != 0 );
        }

        public void setFormulaire(ArcGisProEspaceCollaboratif.Core.Remarque remarque)
        {
            this.setIdRemarque( remarque.Id.ToString() );
            this.setStatut((uint)remarque.Statut);
            this.setMessage(remarque.getUrlDecodedComment());
            this.setAnciennesReponses( remarque.concatenateReponseHTML() ); 
        }

        private void tableLayoutPanel_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
