using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ArcGisProEspaceCollaboratif.Core;

namespace ArcGisProEspaceCollaboratif
{
    
    public partial class FormGroupChoice : Form
    {
        public Profil _profil { get; set; }

        public FormGroupChoice(string cleGeoportail, string groupeActif, Profil profil)
        {
            InitializeComponent();

            _profil = profil;

            // Ajout des noms de groupes trouvés pour l'utilisateur
            foreach (GeoGroupe geogroupe in _profil.Geogroupes)
            {
                this.comboBoxGroupes.Items.Add(geogroupe.Nom);
            } 
            if (!string.IsNullOrEmpty(groupeActif))
            {
                this.comboBoxGroupes.SelectedIndex = _profil.Geogroupes.FindIndex(x => x.Nom.Equals(groupeActif));
            }
            
            // Quelle clé GeoPortail pour l'utilisateur ?
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
            int index = comboBoxGroupes.SelectedIndex;
            string igGroup = _profil.Geogroupes[index].Id;
            string nomGroup = _profil.Geogroupes[index].Nom;
            string cleGeoportail = "";
            if (radioButtonOui.Checked)
            {
                cleGeoportail = textBoxCleGeoportailUser.SelectedText;
            }
            if (radioButtonNon.Checked)
            {
                cleGeoportail = Constantes.DEMO;
            }
            this.Close();
            _profil.IdNomGroupeCleGeoPortail = (igGroup, nomGroup, cleGeoportail);
        }

        private void Annuler_Click(object sender, EventArgs e)
        {
            this.Close();
            return;
        }
    }
}
