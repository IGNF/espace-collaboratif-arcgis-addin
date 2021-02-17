using ArcGisProEspaceCollaboratif.Core;
using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace ArcGisProEspaceCollaboratif
{
    public partial class FormLoadGateway : Form
    {
        public Contexte Contexte { get; set; }
        public List<LayerGateway> ListLayers { get; set; }
        public Profil ProfilUser { get; set; }

        public FormLoadGateway(Contexte contexte)
        {
            InitializeComponent();
            this.Contexte = contexte;
            this.ListLayers.Clear();

            // le tuple contient Rejected/Accepted pour la connexion au service et la liste des layers du groupe utilisateur
            (string, List<LayerGateway>) connexionLayers = this.GetInfosLayers();

            if (connexionLayers.Item1 == "Rejected")
            {
                throw new Exception("Vous n'appartenez à aucun groupe, il n'y a pas de données à charger.");
            }

            if (connexionLayers.Item1 == "Accepted")
            {
                this.ListLayers = connexionLayers.Item2;
                // Les couches sont chargées dans l'ordre renvoyé dans geoaut_get.
                // Il faut donc inverser l'ordre pour retrouver le paramétrage de la carte du groupe
                this.ListLayers.Reverse();
            }

            // Remplissage des différentes tables de couches
            //this.SetTableWidgetMonGuichet();
            //this.SetTableWidgetFondsGeoportail();
            //this.SetTableWidgetFondsGeoportailBis();
            //this.SetTableWidgetAutresGeoservices();

            this.labelGroupeActif.Text = string.Format("Groupe actif : {0}", this.ProfilUser.Groupe.Nom);
        }

        private void ButtonEnregistrer_Click(object sender, EventArgs e)
        {

        }

        private void ButtonAnnuler_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Récupérations des couches de l'Espace collaboratif pour le groupe choisi par l'utilisateur
        /// </summary>
        /// <returns></returns> retourne un tuple contenant Rejected/Accepted pour la connexion à l'Espace collaboratif et la liste des layers du groupe utilisateur
        private (string, List<LayerGateway>) GetInfosLayers()
        {
            List<LayerGateway> layerGateways = new List<LayerGateway>();
            if (this.Contexte.ripClient == null)
            {
                IClient connResult = this.Contexte.GetConnexionEspaceCollaboratif();
                if (connResult == null)
                {
                    // la connexion a échoué ou l'utilisateur a cliqué sur Annuler
                    return ("Rejected", layerGateways);
                }

                if (this.Contexte.ripClient == null)
                {
                    return ("Rejected", layerGateways);
                }

                this.ProfilUser = this.Contexte.ripClient.GetProfil();
                if (this.ProfilUser.Geogroupes.Count == 0)
                {
                    return ("Rejected", layerGateways);
                }

                foreach (GeoGroupe groupe in this.ProfilUser.Geogroupes)
                {
                    if (groupe.Id != this.ProfilUser.Groupe.Id)
                    {
                        continue;
                    }
                    foreach (LayerGateway layer in groupe.Layers)
                    {
                        layerGateways.Add(layer);
                    }
                }
            }
            return ("Accepted", layerGateways);
        }
    }
}
