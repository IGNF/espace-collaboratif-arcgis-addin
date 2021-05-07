using System;
using System.Collections.Generic;
using System.Windows.Forms;
//using ESRI.ArcGIS.Geodatabase;
//using ESRI.ArcGIS.Carto;
//using ESRI.ArcGIS.ArcMapUI;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;

namespace ArcGisProEspaceCollaboratif
{
    public partial class FormSetUp : Form
    {
        bool majAttributs = false;
        Contexte contexte = null;

        public FormSetUp(Contexte contexte)
        {
            InitializeComponent();

            this.toolTip.SetToolTip(this.textBoxUrl, "L'adresse en ligne pour accéder au service de l'espace collaboratif.");
            this.toolTip.SetToolTip(this.textBoxLogin, "Le login du compte utilisateur à utiliser par défaut pour se connecter au service de l'espace collaboratif.");
            this.toolTip.SetToolTip(this.dateTimePicker, "La date limite à partir de laquelle seront extraites que les remarques plus récentes que cette date.");
            this.toolTip.SetToolTip(this.numericUpDownDate, "Nombre de jours par rapport aujourd'hui limitant les remarques importées.");
            this.toolTip.SetToolTip(this.comboBoxCalque, "Le calque à utiliser pour le filtrage spatial lors de l'importation des remarques de l'espace collaboratif.");
            this.toolTip.SetToolTip(this.treeViewAttributs, "Les calques et leurs champs qu'il faut mettre en attribut lors de la génération des croquis de l'espace collaboratif.");
            this.toolTip.SetToolTip(this.numericUpDownPagination, "Le nombre de remarques de l'espace collaboratif contenues dans un bloc de communication entre le serveur de l'espace collaboratif et l'add-in.");
            this.toolTip.SetToolTip(this.buttonOK, "Pour sauvegarder la configuration dans le fichier EspaceCollaboratif.xml.");

            this.contexte = contexte;
        }


        private void ConfigEspaceCollaboratif_Load(object sender, EventArgs e)
        {
           this.textBoxUrl.Text = Helper.Load_Urlhost();

            if (Helper.Load_Login().Length == 0)
            {
                this.checkBoxLogin.Checked = false;
                this.textBoxLogin.Enabled = false;
            }
            else
            {
                this.checkBoxLogin.Checked = true;
                this.textBoxLogin.Text = Helper.Load_Login();
            }

            this.numericUpDownPagination.Value = Helper.Load_Pagination();
            if (Helper.Load_Pagination() == 0)
            {
                this.checkBoxPagination.Checked = false;
                this.numericUpDownPagination.Enabled = false;
            }

            this.dateTimePicker.Value = System.DateTime.Now;
            this.dateTimePicker.MinDate = Convert.ToDateTime(Helper.dateDefault).Date;
            System.DateTime dateDefaut = Helper.Load_DateExtraction();
            if (dateDefaut.Date == Convert.ToDateTime(Helper.dateDefault).Date)
            {
                this.dateTimePicker.Enabled = false;
                this.numericUpDownDate.Enabled = false;
                this.checkBoxDate.Checked = false;
            }
            else
            {
                this.dateTimePicker.MaxDate = System.DateTime.Now.AddDays(1);
                this.dateTimePicker.Text = dateDefaut.ToShortDateString();
            }

            string calqueFiltrageDefaut = Helper.Load_FilterLayer();

            // Récupération des couches et attributs
            IReadOnlyList<Layer> mapLayers = contexte.MapActiveView.Map.GetLayersAsFlattenedList();
            List<string> collabSpaceLayers = new List<string>
            {
                Helper.name_layer_Signalement,
                Helper.name_layer_Croquis_Polygone,
                Helper.name_layer_Croquis_Ligne,
                Helper.name_layer_Croquis_Point
            };

            foreach (var layer in mapLayers)
            {
                if (!collabSpaceLayers.Contains(layer.Name))
                    this.comboBoxCalque.Items.Add(layer.Name);
            }

            if (calqueFiltrageDefaut.Length == 0)
            {
                this.checkBoxCalque.Checked = false;
                this.comboBoxCalque.Enabled = false;
            }
            else
            {
                this.comboBoxCalque.SelectedIndex = this.comboBoxCalque.FindStringExact(calqueFiltrageDefaut);
            }

            if (Helper.Load_AttributesSketch().Nodes.Count >0)
            {
                this.checkBoxCroquis.Checked = true;
            }
            this.checkBoxCroquis.Text = "Calques sources et champs à mettre en\nattribut pour les nouveaux croquis de l'espace collaboratif :";

            if (Helper.Load_Group() =="true")
            {
                this.checkBoxGroup.Checked = true;
            }
            else {
                this.checkBoxGroup.Checked = false;
            }

            // A récupérer dans le fichier de paramètres et non dans ripClient
            //this.lblGroup.Text = this.contexte.ripClient.GetProfil().Geogroupe.Nom;

            
        }

        /// <summary>
        /// Complète treeViewAttributs avec les calques et leurs champs présents sur la carte en cours. 
        /// </summary>
        /// <param name="contexte">Le contexte de la carte en cours</param>        
       public void SetTreeViewAttributs(Contexte contexte)
        {
/*            for (int numLayer = 0; numLayer < contexte.Map.LayerCount; numLayer++)
            {
                ILayer calque = contexte.Map.get_Layer(numLayer);

                if (calque is IGroupLayer && ((ICompositeLayer)calque).Count>0)
                {           
                    this.GetLayersInGroupLayer((ICompositeLayer)calque);                
                }
                else if ( calque is IFeatureLayer) // On ne prend que les calques vectoriels           
                {
                    this.AddCalque(calque);
                }

            }          


            // Préselection des attributs pour génération de croquis.
            this.majAttributs = false;
            System.Windows.Forms.TreeNode attributsPreferred = EspaceCollaboratifHelper.Load_AttributsCroquis();
            
            foreach (System.Windows.Forms.TreeNode calqueAttribut in attributsPreferred.Nodes)
            {  
              this.SearchAndCheckElement( this.SeachNodeByName(calqueAttribut.Text)
                                        , calqueAttribut);                
            }

            for (int numCalque = 0; numCalque < this.treeViewAttributs.Nodes.Count; numCalque++)
            {
                this.UpdateCheckNode(numCalque);
            }

            this.majAttributs = true;
*/
        }


        /// <summary>
        /// Recherche récursive de tous les calques dans un groupLayer
        /// </summary>
        /// <param name="group">le groupe </param>
  /*      private void GetLayersInGroupLayer(ICompositeLayer group)
        {
            for (int i = 0; i < group.Count; i++)
            {
                if (group.get_Layer(i) is IFeatureLayer)
                {
                    this.AddCalque(group.get_Layer(i));
                }

                else if (group.get_Layer(i) is IGroupLayer && ((ICompositeLayer)group.get_Layer(i)).Count > 0)
                {
                    this.GetLayersInGroupLayer((ICompositeLayer)group.get_Layer(i));
                }

            }
        }
*/

        /// <summary>
        /// Ajoute le calque et ses attributs à la liste des calques dans le configurateur
        /// </summary>
        /// <param name="calque"></param>
 /*       private void AddCalque(Layer calque)
        {
            ILayerFields fieldsCalque = calque as ILayerFields;
            this.treeViewAttributs.Nodes.Add(calque.Name);

            for (int numField = 0; numField < fieldsCalque.FieldCount; numField++)
            {   // Exclusion si le champ est type géométrique, raster ou binaire
                if ((fieldsCalque.Field[numField].Type != FieldType.Geometry)
                    && (fieldsCalque.Field[numField].Type != FieldType.Raster)
                    && (fieldsCalque.Field[numField].Type != FieldType.Blob))
                {
                    this.treeViewAttributs.Nodes[treeViewAttributs.Nodes.Count - 1].Nodes.Add(fieldsCalque.Field[numField].Name);
                }
            }
        }
*/

        /// <summary>
        /// Parcourt l'arbre des attributs pour retourner le numéro du noeud ayant le nom donné.
        /// </summary>
        /// <param name="nameCalque">Le nom du calque sdont on cherche le noeud dans l'arbre des attributs.</param>
        /// <returns>Le numero du noeud trouvé, ou -1 en cas contraire.</returns>
        private int SeachNodeByName(string nameCalque)
        {
            for (int i = 0; i < this.treeViewAttributs.Nodes.Count; i++)
            {
                if (this.treeViewAttributs.Nodes[i].Text == nameCalque)
                {
                    return i;
                }
            }          

            return -1;
        }

        /// <summary>
        /// Recherche dans la liste des champs d'un calque donné, celui dont le nom correspond à un des noms en entrée et le coche s'il existe. 
        /// </summary>
        /// <param name="numCalque">Le n° du calque dans lequel il faut rechercher le champ voulu.</param>
        /// <param name="nameField">Les noms des champs à rechercher.</param>
        private void SearchAndCheckElement(int numNode, System.Windows.Forms.TreeNode treeNodeElements)
        {
            foreach (System.Windows.Forms.TreeNode element in treeNodeElements.Nodes)
            {
                this.SearchAndCheckElement(numNode, element.Text);
            }
        }

        /// <summary>
        /// Recherche dans la liste des champs d'un calque donné, celui dont le nom correspond au nom en entrée et le coche s'il existe. 
        /// </summary>
        /// <param name="numCalque">Le n° du calque dans lequel il faut rechercher le champ voulu.</param>
        /// <param name="nameField">Le nom du champ à rechercher.</param>
        private void SearchAndCheckElement(int numCalque, string nameField)
        {
            if (nameField.Length == 0 || (numCalque == -1) || (this.treeViewAttributs.Nodes.Count < numCalque - 1) ) { return; }
            
            for (int i = 0; i < this.treeViewAttributs.Nodes[numCalque].Nodes.Count; i++)
            {
                if (this.treeViewAttributs.Nodes[numCalque].Nodes[i].Text == nameField)
                {                   
                    this.treeViewAttributs.Nodes[numCalque].Nodes[i].Checked = true;
                    return;
                }
            }
           
        }


        /// <summary>
        /// Événement à faire lors d'une sélection dans le calendrier. 
        /// </summary>
        private void DateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (this.dateTimePicker.Value.ToShortDateString() == System.DateTime.Now.ToShortDateString())
            {
                this.numericUpDownDate.Value = 0;
            }
            else
            {
                System.TimeSpan diffDate = System.DateTime.Now - this.dateTimePicker.Value;
                this.numericUpDownDate.Value = diffDate.Days;
            }
        }

        /// <summary>
        /// Événement à faire lors d'une sélection du nombre du jour restant.
        /// </summary>
        private void NumericUpDownDate_ValueChanged(object sender, EventArgs e)
        {
            int diff = (int)this.numericUpDownDate.Value;

            if (diff == 00)
            {
                this.dateTimePicker.Value = System.DateTime.Now;
            }
            else
            {
                System.DateTime newDate = System.DateTime.Now.AddDays(-diff);

                if (newDate > this.dateTimePicker.MinDate)
                {
                    this.dateTimePicker.Value = newDate;
                }
                else
                {
                    this.dateTimePicker.Value = this.dateTimePicker.MinDate;
                }
            }
        }

        /// <summary>
        /// Événement à faire en cas de validation du formulaire.
        /// </summary>
        private void ButtonOK_Click(object sender, EventArgs e)
        {
            Helper.Save_Urlhost(this.textBoxUrl.Text);
            
            if (!this.checkBoxLogin.Checked)
            {
                this.textBoxLogin.Text = "";
            }
            Helper.Save_Login(this.textBoxLogin.Text);

            if (this.checkBoxPagination.Checked)
            {
                Helper.Save_Pagination((uint)this.numericUpDownPagination.Value);
            }
            else
            {
                Helper.Save_Pagination(0);
            }

            if (this.checkBoxCalque.Checked && this.comboBoxCalque.SelectedIndex >= 0)
            {
                Helper.Save_CalqueFiltrage(this.comboBoxCalque.SelectedItem.ToString());
            }
            else
            {
                Helper.Save_CalqueFiltrage("");
            }

            if (this.checkBoxDate.Checked)
            {
                Helper.Save_DateExtraction(this.dateTimePicker.Value.Date);
            }
            else
            {
                Helper.Save_DateExtraction(Convert.ToDateTime(Helper.dateDefault));
            }

            if (this.checkBoxGroup.Checked)
            {
                Helper.Save_Group("true");
            }
            else
            {
                Helper.Save_Group("false");
            }

            this.majAttributs = false;
            if (this.checkBoxCroquis.Checked)
            {
                this.SaveAttributs();
            }
            else
            {
                Helper.Save_AttributsCroquis(new TreeNode());
            }

            if (this.checkBoxProxy.Checked)
            {
                Helper.Save_Proxy(this.checkBoxProxy.Text);
            }

            Helper.Save_CleGeoportail(this.textBoxCleGeoportail.Text);
            Helper.Save_GroupeActif(this.textBoxGroupeActif.Text);

            this.Close();
        }

        /// <summary>
        /// Événement à faire en cas de changement de la case à cocher pour l'entrée du login.
        /// </summary>
        private void CheckBoxLogin_CheckedChanged(object sender, EventArgs e)
        {
            this.textBoxLogin.Enabled = this.checkBoxLogin.Checked;
        }

        /// <summary>
        /// Événement à faire en cas de changement de la case à cocher pour l'entrée de la pagination.
        /// </summary>
        private void CheckBoxPagination_CheckedChanged(object sender, EventArgs e)
        {
            this.numericUpDownPagination.Enabled = this.checkBoxPagination.Checked;
        }

        /// <summary>
        /// Événement à faire en cas de changement de la case à cocher pour l'entrée de la date de filtrage.
        /// </summary>
        private void CheckBoxDate_CheckedChanged(object sender, EventArgs e)
        {
            this.dateTimePicker.Enabled = this.checkBoxDate.Checked;
            this.numericUpDownDate.Enabled = this.checkBoxDate.Checked;
        }

        /// <summary>
        /// Événement à faire en cas de changement de la case à cocher pour l'entrée du calque de filtrage.
        /// </summary>
        private void CheckBoxCalque_CheckedChanged(object sender, EventArgs e)
        {
            this.comboBoxCalque.Enabled = this.checkBoxCalque.Checked;   
        }    
      

        /// <summary>
        /// Met une coche sur le nœud  parent de treeViewAttributs si au moins un des ses sous-éléments est coché. 
        /// </summary>
        /// <param name="numNode">L'index du nœud  à tester.</param>
        public void UpdateCheckNode(int numNode)
        {
            if (this.treeViewAttributs.Nodes.Count < numNode) { return; }

            this.treeViewAttributs.Nodes[numNode].Checked = this.HasCheckedElement(numNode);
        }


        /// <summary>
        /// Coche ou décoche tous les sous-éléments d'un nœud  donné de treeViewAttributs.
        /// </summary>
        /// <param name="numNode">L'index du nœud  à parcourrir.</param>
        /// <param name="status">Option pour cocher ou non les sous-éléments.</param>
        public void SetCheckNode(int numNode, bool status)
        {
            if (this.treeViewAttributs.Nodes.Count < numNode) { return; }

            for (int i = 0; i < this.treeViewAttributs.Nodes[numNode].Nodes.Count; i++)
            {
                this.treeViewAttributs.Nodes[numNode].Nodes[i].Checked = status;
            }

            this.treeViewAttributs.Nodes[numNode].Checked = status;
        }


        /// <summary>
        /// Indique si au moins un des sous-élements du nœud est coché
        /// </summary>
        /// <param name="numNode">L'index du nœud  à parcourrir.</param>
        /// <returns>False si aucun sous-élément du nœud n'est coché.</returns>
        public bool HasCheckedElement(int numNode)
        {
            if (this.treeViewAttributs.Nodes.Count < numNode) { return false; }

            foreach (TreeNode childNode in this.treeViewAttributs.Nodes[numNode].Nodes)
            {
                if (childNode.Checked)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Sauvegarde dans le fichier de configuration les champs sélectionnés.
        /// </summary>
        public void SaveAttributs()
        {
            System.Windows.Forms.TreeNode attributs = new TreeNode();

            for (int numCalque = 0; numCalque < this.treeViewAttributs.Nodes.Count; numCalque++)
            {
                this.UpdateCheckNode(numCalque);

                if (this.treeViewAttributs.Nodes[numCalque].Checked)
                {
                    attributs.Nodes.Add(this.treeViewAttributs.Nodes[numCalque].Text);

                    foreach (System.Windows.Forms.TreeNode attribut in this.treeViewAttributs.Nodes[numCalque].Nodes)
                    {
                        if (attribut.Checked)
                        {
                            attributs.Nodes[attributs.Nodes.Count - 1].Nodes.Add(attribut.Text);
                        }
                    }
                }
            }

            Helper.Save_AttributsCroquis(attributs);
        }


        /// <summary>
        /// Met-à-jour les noeuds de treeViewAttributs selon leurs états de sélection et de ceux de leurs enfants. 
        /// </summary>
        private void TreeViewAttributs_AfterCheck(object sender, TreeViewEventArgs e)
        {
            
            if (!this.majAttributs) { return; }

            this.majAttributs = false;
            if (e.Node.Level == 0)
            {
                int index = e.Node.Index;
                bool select = this.treeViewAttributs.Nodes[index].Checked;               
                this.SetCheckNode(index, select);                
            }
            else
            {
                int level = e.Node.Parent.Index;
                this.UpdateCheckNode(level);
            }
            this.majAttributs = true;
        }
    }
}
