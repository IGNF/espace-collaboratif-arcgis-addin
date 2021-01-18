using System;
using System.Windows.Forms;



namespace ArcGisProEspaceCollaboratif
{
    public partial class FormInfoEspaceCollaboratif : Form
    {
        int tempo;


        public FormInfoEspaceCollaboratif()
        {
            InitializeComponent();
            this.timer.Stop();
        }

        /// <summary>
        /// Affiche un message dans l'espace réservé de la fenêtre d'informations de l'espace collaboratif.
        /// </summary>     
        /// <param name="message">Le message à afficher dans la fenêtre d'informations de l'espace collaboratif.</param>
        public void setMessage(string message)
        {
            this.richTextBox.Text = message;
        }

        /// <summary>
        /// Ajoute un message au message déjà affiché dans la fenêtre d'informations de l'espace collaboratif.
        /// </summary>     
        /// <param name="message">Le message à ajouter.</param>
        public void addMessage(string message)
        {
            this.richTextBox.Text += "\n" + message;
        }

        /// <summary>
        /// Affiche une image (logo) dans l'espace réservé de la fenêtre d'informations de l'espace collaboratif.
        /// </summary>     
        /// <param name="image">L'image à afficher dans la fenêtre d'informations de l'espace collaboratif.</param>
        public void setLogo(string image)
        {
            this.LogoBox.ImageLocation = image;
        }

        private void InfoEspaceCollaboratif_Load(object sender, EventArgs e)
        {          
            this.buttonOK.Text = "OK";            
        }

        /// <summary>
        /// Régle et déclenche le compte-à-rebours dans le fenêtre d'informations de l'espace collaboratif.
        /// </summary>     
        /// <param name="time">La durée en seconde du compte-à-rebours.</param>
        public void StartCountDown(int time)
        {
            this.tempo = time;
            this.buttonOK.Text = "OK (" + time + ")";
            this.timer.Start(); 
        }


        /// <summary>
        /// Événement lorsque le compte-à-rebours arrive à échéance.
        /// </summary>             
        private void timer_Tick(object sender, EventArgs e)
        {
            tempo--;

            if (tempo == 0)
            {
                this.Close();
            }
            else
            {               
                this.buttonOK.Text = "OK (" + this.tempo + ")";
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
