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

        // 
        /// <summary>
        /// Balises <ROLE>
        /// Role de la couche dans le cadre d'un guichet
        /// - visu = couche servant de fond uniquement
        /// - ref = couche servant de référence pour la saisie (snapping ou routing)
        /// - edit, ref-edit = couche éditable sur le guichet
        /// "clé xml":"valeur affichage boite"
        /// </summary>
        Dictionary<string, string> roleCleVal = new Dictionary<string, string>
        {
            { "edit" , "Edition" },
            { "ref-edit" , "Edition" },
            { "visu" , "Visualisation" },
            { "ref" , "Visualisation" }
        };

        public FormLoadGateway(Contexte contexte)
        {
            InitializeComponent();
            this.Contexte = contexte;
            this.ListLayers = new List<LayerGateway>();

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
            this.SetListViewMyGateway();
            this.SetListViewFondsGeoportail();
            this.SetListViewFondsGeoportailBis();
            
            this.labelGroupeActif.Text = string.Format("Groupe actif : {0}", this.ProfilUser.Groupe.Nom);
        }

        /// <summary>
        /// Ajouter une ligne dans la ListView passée en entrée
        /// </summary>
        /// <param name="listView">La ListView à compléter</param>
        /// <param name="att1">Le nom de la couche</param>
        /// <param name="att2">Le role de la couche</param>
        /// <param name="bCheckBox">à true s'il faut une case à cocher</param>
        private void AddItem(ListView listView, string att1, string att2, bool bCheckBox)
        {
            // Display check boxes.
            listView.CheckBoxes = true;     
            if (!bCheckBox)
            {
                listView.CheckBoxes = false;
            }
 
            string[] arr = new string[4];
            ListViewItem itm;
            arr[0] = att1;
            arr[1] = this.roleCleVal[att2];
            itm = new ListViewItem(arr);
            listView.Items.Add(itm);
        }

        /// <summary>
        /// Ajouter des colonnes à une ListView
        /// </summary>
        /// <param name="listView">La ListView à compléter</param>
        /// <param name="allColums">true pour ajouter toutes les colonnes (3 max)</param>
        private void AddColumns(ListView listView, bool allColums)
        {
            listView.Columns.Add("Nom de la couche", -2);
            if (allColums)
            {
                listView.Columns.Add("Rôle", -2);
            }
        }

        /// <summary>
        /// Mise à jour de la ListView "Mon guichet" contenant les couches du groupe
        /// auquel l'utilisateur appartient
        /// </summary>
        private void SetListViewMyGateway()
        {
            this.AddColumns(this.listViewMyGateway, true);

            foreach (LayerGateway layer in this.ListLayers)
            {
                if(layer.Type != Constantes.WFS)
                {
                    continue;
                }

                if (!layer.Url.Contains(Constantes.COLLABORATIF))
                {
                    continue;
                }

                this.AddItem(this.listViewMyGateway, layer.Nom, layer.Role, true);
            }     
        }

        /// <summary>
        /// Mise à jour de la ListView "FondsGéoportail" contenant les couches visibles
        /// avec la clé Géoportail de l'utilisateur
        /// </summary>
        private void SetListViewFondsGeoportail()
        {
            this.AddColumns(this.listViewGeoportail, true);
            foreach (LayerGateway layer in this.ListLayers)
            {
                if (layer.Type != Constantes.GEOPORTAIL)
                {
                    continue;
                }

                if (!this.ProfilUser.LayersCleGeoportail.ContainsKey(layer.Nom))
                {
                    continue;
                }

                string nomLayer = string.Format("{0} ({1})", layer.Description, layer.Nom);
                this.AddItem(this.listViewGeoportail, nomLayer, layer.Role, true);
            }
        }

        /// <summary>
        /// Mise à jour de la ListView "FondsGéoportail" contenant les couches visibles
        /// avec la clé Géoportail de l'utilisateur
        /// </summary>
        private void SetListViewFondsGeoportailBis()
        {
            this.AddColumns(this.listViewGeoportailBis, false);
            foreach (LayerGateway layer in this.ListLayers)
            {
                if (layer.Type != Constantes.GEOPORTAIL)
                {
                    continue;
                }

                if (this.ProfilUser.LayersCleGeoportail.ContainsKey(layer.Nom))
                {
                    continue;
                }
                string nomLayer = string.Format("{0} ({1})", layer.Description, layer.Nom);
                this.AddItem(this.listViewGeoportailBis, nomLayer, layer.Role, false);
            }
        }

        private void ButtonEnregistrer_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ButtonAnnuler_Click(object sender, EventArgs e)
        {
            this.Close();
            return;
        }

        /// <summary>
        /// Récupération des couches de l'Espace collaboratif pour le groupe choisi par l'utilisateur
        /// </summary>
        /// <returns>Retourne un tuple contenant Rejected/Accepted pour la connexion à l'Espace collaboratif et la liste des layers du groupe utilisateur</returns>
        private (string, List<LayerGateway>) GetInfosLayers()
        {
            List<LayerGateway> layerGateways = new List<LayerGateway>();
            if (this.Contexte.RipClient == null)
            {
                IClient connResult = this.Contexte.GetConnexionEspaceCollaboratif();
                if (connResult == null)
                {
                    // la connexion a échoué ou l'utilisateur a cliqué sur Annuler
                    return ("Rejected", layerGateways);
                }

                this.ProfilUser = connResult.GetProfil();
                if (this.ProfilUser.Geogroupes.Count == 0)
                {
                    return ("Rejected", layerGateways);
                }
            }
            else
            {
                this.ProfilUser = this.Contexte.RipClient.GetProfil();
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
 
            return ("Accepted", layerGateways);
        }
    }
}
