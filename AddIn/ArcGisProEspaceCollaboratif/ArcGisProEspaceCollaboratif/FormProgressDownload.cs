using System;
using System.Windows.Forms;

namespace ArcGisProEspaceCollaboratif
{
    public partial class FormProgressDownload : Form
    {
         
        public FormProgressDownload()
        {
            InitializeComponent();
        }

        public void SetBar(int position)
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

        public ProgressBar GetProgressBar()
        {
            return this.progressBar;
        }

        public void SetText(string message)
        {
            this.label.Text = message;
            this.label.Visible = !message.Equals("");
        }

        public void SetMaxProgressor(int max)
        {
            this.progressBar.Maximum = max;
        }

        public void NextProgressor(string optionstring = "")
        {         
            this.progressBar.PerformStep();
            if (!optionstring.Equals(""))
            {
                this.SetText(optionstring);
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
