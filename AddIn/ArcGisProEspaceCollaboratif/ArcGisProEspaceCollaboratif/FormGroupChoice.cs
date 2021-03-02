using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ArcGisProEspaceCollaboratif.Core;

namespace ArcGisProEspaceCollaboratif
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FormGroupChoice : Form
    {
        /// <summary>
        /// Le profil mis à jour avec les choix de l'utilisateur
        /// </summary>
        public Profil _profil { get; set; }

        /// <summary>
        /// Initialisation du dialogue avec les anciens choix de l'utilisateur.
        /// Cela permet à l'utilisateur de choisir éventuellement un nouveau groupe
        /// sur lequel il désire travailler et la clé Géoportail.
        /// </summary>
        /// <param name="cleGeoportail">la clé Géoportail de l'utilsateur enregistrée lors d'une session de travail précédente</param>
        /// <param name="groupeActif">le groupe actif enregistré lors d'une session de travail précédente</param>
        /// <param name="profil"> les données du profil utilisateur</param>
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

        /// <summary>
        /// Bouton Enregistrer
        /// Permet de sauvegarder le choix de l'utilisateur
        /// sur un groupe et la clé Géoportail
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Bouton Annuler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Annuler_Click(object sender, EventArgs e)
        {
            this.Close();
            return;
        }
    }
}
