using System;
using System.Windows.Forms;
using log4net;


namespace ArcGisProEspaceCollaboratif
{
    public partial class FormConnecterEspaceCollaboratif : Form
    {

        private System.Drawing.Size formSize; 

        ArcGisProEspaceCollaboratif.Core.EspaceCollaboratifLogger riplogger = ArcGisProEspaceCollaboratif.Core.EspaceCollaboratifLogger.Instance;
        private static readonly ILog logger = LogManager.GetLogger(typeof(FormConnecterEspaceCollaboratif));
       

        public FormConnecterEspaceCollaboratif()
        {
            InitializeComponent();

            buttonConnect.DialogResult = DialogResult.OK;
            buttonAbort.DialogResult = DialogResult.Abort;

            this.toolTip.SetToolTip(this.buttonConnect, "Pour se connecter au service de l'espace collaboratif.");
            this.toolTip.SetToolTip(this.buttonAbort, "Pour abandonner la tentative de connexion au service de l'espace collaboratif.");
            this.toolTip.SetToolTip(this.InputLoginEspaceCollaboratif, "Saisissez ici votre login de votre compte de l'espace collaboratif.");
            this.toolTip.SetToolTip(this.InputPasswordEspaceCollaboratif, "Saisissez ici votre mot de passe de votre compte de l'espace collaboratif.");

            formSize = this.Size;

            logger.Debug("FormConnecterEspaceCollaboratif constructor");            
        }
        
        /// <summary>
        /// Renvoie le login saisi dans le formulaire pour se connecter au service de l'espace collaboratif.
        /// </summary>     
        /// <returns>Le login pour se connecter au service de l'espace collaboratif.</returns>
        public String GetLogin() { return this.InputLoginEspaceCollaboratif.Text; }

        /// <summary>
        /// Renvoie le mot de passe saisi dans le formulaire pour se connecter au service de l'espace collaboratif.
        /// </summary>     
        /// <returns>Le mot de passe pour se connecter au service de l'espace collaboratif.</returns>
        public String GetPassword() { return this.InputPasswordEspaceCollaboratif.Text; }              

        /// <summary>
        /// Pré-remplit d'un login par défaut le formulaire pour se connecter au service de l'espace collaboratif.
        /// </summary>     
        /// <param name="defaultLogin">Le login par défaut qu'il faut mettre dans le formulaire de connexion.</param>
        public void SetLogin(String defaultLogin)
        {
            this.InputLoginEspaceCollaboratif.Text = defaultLogin;
        }   

        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            this.Size = new System.Drawing.Size(formSize.Width, formSize.Height + 25);
            ResetLblErreur();
            this.DialogResult = DialogResult.OK;
        }

        private void ButtonAbort_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Action après changement du login saisi dans le formulaire de connection au service de l'espace collaboratif.
        /// </summary>   
        private void InputLoginEspaceCollaboratif_TextChanged(object sender, EventArgs e)
        {
           
            this.buttonConnect.Enabled = this.InputLoginEspaceCollaboratif.TextLength != 0 && this.InputPasswordEspaceCollaboratif.TextLength != 0;
            
        }

        /// <summary>
        /// Action après changement du mot de passe saisi dans le formulaire de connection au service de l'espace collaboratif.
        /// </summary>   
        private void InputPasswordEspaceCollaboratif_TextChanged(object sender, EventArgs e)
        {
          
            if (InputLoginEspaceCollaboratif.TextLength != 0 && InputPasswordEspaceCollaboratif.TextLength != 0)
            {
                this.buttonConnect.Enabled = true;
            }
            else
            {
                this.buttonConnect.Enabled = false;
            }
        }

        private void LogEspaceCollaboratif_Load(object sender, EventArgs e)
        {
            InputPasswordEspaceCollaboratif.Text = "";    
        }

        private void LogEspaceCollaboratif_Close(object sender, EventArgs e)
        {
            
            this.lblErreur.Visible = false;
        }

        /// <summary>
        /// Affiche un message d'erreur
        /// </summary>   
        /// <param name="message">Le texte du message d'erreur.</param>   
        public void Notifier(string message)
        {
            this.Size = new System.Drawing.Size(formSize.Width, formSize.Height + 22);
            this.lblErreur.Text = message;
            this.lblErreur.Visible = true;        
        }

        private void ResetLblErreur()
        {
            this.lblErreur.Text = "";
            this.lblErreur.Visible = false;
        }
    }
}