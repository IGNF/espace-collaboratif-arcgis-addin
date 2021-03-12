using ArcGisProEspaceCollaboratif.Core;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace ArcGisProEspaceCollaboratif
{
    /// <summary>
    /// La  classe du dialogue qui permet à l'utilisateur de choisir les différentes couches
    /// qu'il veut afficher dans sa carte ArcGIS pro
    /// </summary>
    public partial class FormLoadGateway : Form
    {
        /// <summary>
        /// Toutes les données contextuelles liées à la carte et à l'Espace collaboratif
        /// </summary>
        public Contexte Contexte { get; set; }

        /// <summary>
        /// La liste de toutes les couches disponibles pour le groupe de l'utilisateur 
        /// </summary>
        public List<LayerGateway> ListLayers { get; set; }

        /// <summary>
        /// Le profil de l'utilisateur
        /// </summary>
        public Profil ProfilUser { get; set; }

        // 
        /// <summary>
        /// Balises <ROLE>
        /// Role de la couche dans le cadre d'un guichet
        /// - visu = couche servant de fond uniquement
        /// - ref = couche servant de référence pour la saisie (snapping ou routing)
        /// - edit, ref-edit = couche éditable sur le guichet
        ///
        /// Le dictionnaire est de la forme "clé xml":"valeur affichage boite"
        /// </summary>
        Dictionary<string, string> roleCleVal = new Dictionary<string, string>
        {
            { "edit" , "Edition" },
            { "ref-edit" , "Edition" },
            { "visu" , "Visualisation" },
            { "ref" , "Visualisation" }
        };

        /// <summary>
        /// Initialisation du dialogue avec les couches de l'utilisateur et sa clé Géoportail
        /// </summary>
        /// <param name="contexte"></param>
        public FormLoadGateway(Contexte contexte)
        {
            InitializeComponent();
            this.Contexte = contexte;
            this.ListLayers = new List<LayerGateway>();

            // le tuple contient Rejected/Accepted pour la connexion au service et la liste des layers du groupe utilisateur
            string resultat = this.GetInfosLayers();

            if (resultat == "Rejected")
            {
                throw new Exception("Vous n'appartenez à aucun groupe, il n'y a pas de données à charger.");
            }

            if (resultat == "Accepted")
            {                
                // Les couches sont chargées dans l'ordre renvoyé dans geoaut_get.
                // Il faut donc inverser l'ordre pour retrouver le paramétrage de la carte du groupe
                this.ListLayers.Reverse();
            }

            // Remplissage des différentes lIstView avec les couches disponibles
            this.SetListViewMyGateway();
            this.SetListViewFondsGeoportail();
            this.SetListViewFondsGeoportailBis();
            
            this.labelGroupeActif.Text = string.Format("Groupe actif : {0}", this.ProfilUser.Group.Name);
        }

        /// <summary>
        /// Ajoute une ligne dans la ListView passée en entrée
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
        /// Ajoute des colonnes par défaut à une ListView
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
        /// Mise à jour de la ListView "Mon guichet" contenant
        /// les couches du groupe utilisateur
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
        /// Mise à jour de la ListView "FondsGeoportail" contenant
        /// les couches visibles avec la clé Géoportail de l'utilisateur
        /// </summary>
        public void SetListViewFondsGeoportail()
        {
            this.AddColumns(this.listViewGeoportail, true);
            foreach (LayerGateway layer in this.ListLayers)
            {
                if (layer.Type != Constantes.GEOPORTAIL)
                {
                    continue;
                }
                int index = this.ProfilUser.LayersKeyGeoportail.FindIndex(x => x.Name.Equals(layer.Nom));
                if (index == -1)
                {
                    continue;
                }

                string nomLayer = string.Format("{0} ({1})", this.ProfilUser.LayersKeyGeoportail[index].Title, layer.Nom);
                this.AddItem(this.listViewGeoportail, nomLayer, layer.Role, true);
            }
        }

        /// <summary>
        /// Mise à jour de la ListView "FondsGeoportailBis" contenant
        /// les autres couches visibles avec la clé Géoportail de l'utilisateur
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

                if (this.ProfilUser.LayersKeyGeoportail.FindIndex(x => x.Name.Equals(layer.Nom)) != -1)
                {
                    continue;
                }
                string nomLayer = string.Format("{0} ({1})", layer.Description, layer.Nom);
                this.AddItem(this.listViewGeoportailBis, nomLayer, layer.Role, false);
            }
        }

        /// <summary>
        /// Recherche pour une ListView les couches qui sont cochées
        /// </summary>
        /// <param name="listView">La ListView contenant les couches sélectionnées</param>
        /// <returns></returns>
        private List<string> GetLayersSelected(ListView listView)
        {
            List<string> checked_list = new List<string>();
            ListView.CheckedListViewItemCollection checkedItems = listView.CheckedItems;
            foreach (ListViewItem item in checkedItems)
            {
                checked_list.Add(item.Text);
            }
            return checked_list;
        }

        /// <summary>
        /// Bouton "Enregistrer" qui lance l'import
        /// des couches sélectionnées par l'utilisateur dans ArcGIS 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonEnregistrer_Click(object sender, EventArgs e)
        {
            //Liste des couches à afficher après la sélection de l'utilisateur
            List<string>[] layersChecked = new List<string>[]
            {
                GetLayersSelected(this.listViewMyGateway),
                GetLayersSelected(this.listViewGeoportail),
                GetLayersSelected(this.listViewGeoportailBis)
            };

            // Fermeture de la boite
            this.Close();

            List<LayerGateway> layersToAppend = new List<LayerGateway>();
            foreach (List<string> layerChecked in layersChecked)
            {
                foreach (string layerCheck in layerChecked)
                {
                    // layerCheck est sous la forme 'troncon_de_voie_ferree' ou 'Cartes IGN (GEOGRAPHICALGRIDSYSTEMS.MAPS)'
                    string name;
                    if (layerCheck.Contains("("))
                    {
                        string[] layerCheckName = layerCheck.Split('(');
                        name = layerCheckName[1].Replace(")", "");
                    }
                    else
                    {
                        name = layerCheck;
                    }

                    int index = this.ListLayers.FindIndex(x => x.Nom.Equals(name));
                    if (index == -1)
                    {
                        continue;
                    }
                    layersToAppend.Add(this.ListLayers[index]);
                }
            }

            // Import des couches WFS et WMTS dans ArcGIS
            LoadLayersAsync(layersToAppend);
        }

        /// <summary>
        /// Import des couches WFS et WMTS sélectionnées par l'utilisateur dans ArcGIS
        /// </summary>
        /// <param name="layersToLoad">La liste de toutes les couches sélectionnées à importer avec leurs caractéristiques</param>
        private async void LoadLayersAsync(List<LayerGateway> layersToLoad)
        {
            // Quelles sont les couches existantes dans la carte ?
            List<string> layersInMap = LayersInMap;

            // Les couches WFS de l'Espace collaboratif
            WebFeatureService wfs = new WebFeatureService()
            {
                Layers = layersToLoad,
                LayersInMap = layersInMap,
                Login = this.Contexte.Login,
                Password = this.Contexte.Password,
                ActiveGroup = this.Contexte.Groupeactif
            };
            await wfs.AddLayersAsync();

            // Les couches WMTS du Géoportail
            WebMapTileService wmts = new WebMapTileService()
            {
                Layers = layersToLoad,
                LayersInMap = layersInMap,
                KeyGeoportail = this.Contexte.CleGeoportail
            };
            await wmts.AddLayersAsync();
        }

        /// <summary>
        /// Récupère dans une liste les noms des couches existantes de la carte active
        /// et change le nom des couches Geoportail car dans certains cas les valeurs des balises DESCRIPTION et Title sont les mêmes
        /// dans d'autres elles sont différentes, il faut donc récupérer la valeur de la balise Name
        ///
        /// Espace collaboratif versus Geoportail
        /// <NOM>CADASTRALPARCELS.PARCELLAIRE_EXPRESS</NOM> == <Name>CADASTRALPARCELS.PARCELLAIRE_EXPRESS</Name>
        /// <DESCRIPTION>Plan cadastral informatisé vecteur de la DGFIP.</DESCRIPTION> != <Title>PCI vecteur</Title>
        /// autre exemple
        /// <NOM>ORTHOIMAGERY.ORTHOPHOTOS</NOM> == <Name>ORTHOIMAGERY.ORTHOPHOTOS</Name>
        /// <DESCRIPTION>Photographies aériennes</DESCRIPTION> == <Title>Photographies aériennes</Title>
        /// </summary>
        private List<string> LayersInMap
        {
            get
            {
                System.Collections.ObjectModel.ReadOnlyObservableCollection<Layer> observableLayers = Contexte.MapActiveView.Map.Layers;
                List<string> layersInMap = new List<string>();
                foreach (Layer observableLayer in observableLayers)
                {
                    int index = this.ProfilUser.LayersKeyGeoportail.FindIndex(x => x.Title.Equals(observableLayer.Name));
                    if (index != -1)
                    {
                        layersInMap.Add(this.ProfilUser.LayersKeyGeoportail[index].Name);
                    }
                    else
                    {
                        layersInMap.Add(observableLayer.Name);
                    }
                }
                return layersInMap;
            }
        }

        /// <summary>
        /// Bouton "Annuler"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonAnnuler_Click(object sender, EventArgs e)
        {
            this.Close();
            return;
        }

        /// <summary>
        /// Récupération des couches de l'Espace collaboratif pour le groupe choisi par l'utilisateur
        /// </summary>
        /// <returns>Retourne un tuple contenant Rejected/Accepted pour la connexion à l'Espace collaboratif et la liste des layers du groupe utilisateur</returns>
        private string GetInfosLayers()
        {
            if (this.Contexte.Client == null)
            {
                Client connResult = this.Contexte.GetConnexionEspaceCollaboratif();
                if (connResult == null)
                {
                    // la connexion a échoué ou l'utilisateur a cliqué sur Annuler
                    return "Rejected";
                }
            }

            this.ProfilUser = this.Contexte.Profil;
            if (this.ProfilUser.Geogroupes.Count == 0)
            {
                return "Rejected";
            }
            
            foreach (GeoGroupe groupe in this.ProfilUser.Geogroupes)
            {
                if (groupe.Id != this.ProfilUser.Group.Id)
                {
                    continue;
                }
                foreach (LayerGateway layer in groupe.Layers)
                {
                    this.ListLayers.Add(layer);
                }
            }
 
            return "Accepted";
        }
    }
}
