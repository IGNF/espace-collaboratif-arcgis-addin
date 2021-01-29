using System;
using System.Windows.Forms;

namespace ArcGisProEspaceCollaboratif
{
    public partial class FormProgressDownload : Form
    {
         
        public FormProgressDownload()
        {
            InitializeComponent();
          //  Color vertIGN = System.Drawing.Color.FromArgb(148, 192, 26);
          //  this.progressBar.ForeColor = vertIGN;
        }

        public void setBar(int position)
        {
            if (position > 100) { position = 100; }

            if (position == 0)
            {
                this.progressBar.Style = ProgressBarStyle.Marquee;

            }
            else
            {
                this.progressBar.Style = ProgressBarStyle.Continuous;
                this.progressBar.Increment(position);

            }

        }

        public ProgressBar getProgressBar()
        {
            return this.progressBar;
        }

        public void setText(string message)
        {

            this.label.Text = message;
            this.label.Visible = !message.Equals("");

        }

        public void setMaxProgressor(int max)
        {
            this.progressBar.Maximum = max;
        }

        public void nextProgressor(string optionstring = "")
        {
            
            this.progressBar.PerformStep();
            if (!optionstring.Equals(""))
            {
                this.setText(optionstring);
            }

            this.Refresh();

        }



        private void AttenteChargement_Load(object sender, EventArgs e)
        {
           // Color vertIGN = System.Drawing.Color.FromArgb(148, 192, 26);
           // this.progressBar.ForeColor = vertIGN;
        }

       
        
    }
}
