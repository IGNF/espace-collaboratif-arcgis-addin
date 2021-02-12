using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArcGisProEspaceCollaboratif.Core;

namespace ArcGisProEspaceCollaboratif
{
    public partial class FormGroupChoice : Form
    {
        public FormGroupChoice(string cleGeoportail)
        {
            InitializeComponent();
            if (cleGeoportail == Constantes.DEMO || cleGeoportail == "")
            {
                this.radioButtonNon.Select();
            }
            if (cleGeoportail != Constantes.DEMO && cleGeoportail != "")
            {
                this.radioButtonOui.Select();
                this.textBoxCleGeoportailUser.AppendText(cleGeoportail);
            }
        }

        private void Enregistrer_Click(object sender, EventArgs e)
        {

        }

        private void Annuler_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
