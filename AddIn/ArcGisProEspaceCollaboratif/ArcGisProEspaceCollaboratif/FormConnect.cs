using System;
using System.Windows.Forms;
using log4net;


namespace ArcGisProEspaceCollaboratif
{
    public partial class FormConnect : Form
    {

        private System.Drawing.Size formSize; 

        ArcGisProEspaceCollaboratif.Core.Logger riplogger = ArcGisProEspaceCollaboratif.Core.Logger.Instance;
        private static readonly ILog logger = LogManager.GetLogger(typeof(FormConnect));
       

        public FormConnect()
        {
            InitializeComponent();

            buttonConnect.DialogResult = DialogResult.OK;
            buttonAbort.DialogResult = DialogResult.Abort;

            this.toolTip.SetToolTip(this.buttonConnect, "Pour se connecter au service de l'espace collaboratif.");
            this.toolTip.SetToolTip(this.buttonAbort, "Pour abandonner la tentative de connexion au service de l'espace collaboratif.");
            this.toolTip.SetToolTip(this.InputLogin, "Saisissez ici votre login de votre compte de l'espace collaboratif.");
            this.toolTip.SetToolTip(this.InputPassword, "Saisissez ici votre mot de passe de votre compte de l'espace collaboratif.");

            formSize = this.Size;

            logger.Debug("FormConnecterEspaceCollaboratif constructor");            
        }
        
        /// <summary>
        /// Renvoie le login saisi dans le formulaire pour se connecter au service de l'espace collaboratif.
        /// </summary>     
        /// <returns>Le login pour se connecter au service de l'espace collaboratif.</returns>
        public string GetLogin() {
            return this.InputLogin.Text;
        }

        /// <summary>
        /// Renvoie le mot de passe saisi dans le formulaire pour se connecter au service de l'espace collaboratif.
        /// </summary>     
        /// <returns>Le mot de passe pour se connecter au service de l'espace collaboratif.</returns>
        public string GetPassword() {
            return this.InputPassword.Text;
        }

        /// <summary>
        /// Pré-remplit d'un login par défaut le formulaire pour se connecter au service de l'espace collaboratif.
        /// </summary>     
        /// <param name="defaultLogin">Le login par défaut qu'il faut mettre dans le formulaire de connexion.</param>
        public void SetLogin(string defaultLogin)
        {
            this.InputPassword.Focus();
            this.InputPassword.ScrollToCaret();
            this.InputLogin.Text = defaultLogin;
            this.InputLogin.DeselectAll();
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
            return;
        }

        /// <summary>
        /// Action après changement du login saisi dans le formulaire de connection au service de l'espace collaboratif.
        /// </summary>   
        private void InputLogin_TextChanged(object sender, EventArgs e)
        {
            this.buttonConnect.Enabled = this.InputLogin.TextLength != 0 && this.InputPassword.TextLength != 0;
        }

        /// <summary>
        /// Action après changement du mot de passe saisi dans le formulaire de connection au service de l'espace collaboratif.
        /// </summary>   
        private void InputPassword_TextChanged(object sender, EventArgs e)
        {
            if (InputLogin.TextLength != 0 && InputPassword.TextLength != 0)
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
            InputPassword.Text = "";    
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
