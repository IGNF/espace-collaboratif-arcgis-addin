using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ArcGisProEspaceCollaboratif.Core;

namespace ArcGisProEspaceCollaboratif
{
    public partial class FormCreateReport : Form
    {
        /// <summary>
        /// 
        /// </summary>
        public Contexte Contexte { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int SketchNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Profil Profil { get; set; }

        /// <summary>
        /// Liste des fichiers en pièce-jointe
        /// </summary>
        public List<string> ListFilesPJ { get; set; }

        /// <summary>
        /// Taille maximale des fichiers en pièce-jointe
        /// </summary>
        public int MaxSizeFilePJ { get; set; }

        /// <summary>
        /// Le groupe préféré de l'utilisateur car il peut changer
        /// en cours de saisie des signalements
        /// </summary>
        public string PreferredGroup { get; set; } = "";

        /// <summary>
        /// Liste de l'ensemble des thèmes du profil utilisateur
        /// </summary>
        public List<Theme> ListThemes { get; set; } = new List<Theme>();

        /// <summary>
        /// Liste des thèmes filtrés par l'utilisateur en fonction de son groupe
        /// </summary>
        public List<string> ListFilteredThemes { get; set; } = new List<string>();

        /// <summary>
        /// Liste des thèmes préférés de l'utilisateur cochés
        /// lors de la saisie d'un nouveau signalement
        /// et présents dans le fichier de configuration espace_co.xml
        /// </summary>
        public List<string> ListPreferredThemes { get; set; } = new List<string>();

        /// <summary>
        /// Construction du dialogue de création des signalements
        /// </summary>
        public FormCreateReport(Contexte contexte)
        {
            this.Contexte = contexte;
            InitializeComponent();
            SetForm();
        }

        /// <summary>
        /// Remplit le formulaire FormCreerSignalement avant son utilisation.
        /// </summary>         
        /// <param name="nbrSignalement">Le nombre de remarques Ripart à créer si l'option de création d'une unique remarque n'est pas choisie.</param>
        /// <param name="contexte">Le contexte de la carte en cours ouverte dans ArcMap.</param>
        /// <param name="iclient">La connexion avec le service en-ligne Ripart.</param>
        public void SetForm()
        {
            this.toolTip.SetToolTip(this.treeViewThemesAttributs, "Sélectionnez les thèmes auxquels vous souhaitez associer le nouveau signalement.");
            this.toolTip.SetToolTip(this.richTextBoxMessage, "Rédigez ici le message pour le nouveau signalement.");
            this.toolTip.SetToolTip(this.buttonCreer, "Créer le nouveau signalement.\nNécessite au moins qu'un thème soit associé et qu'un message ait été rédigé.");
            
            //self.lblDoc.setProperty("visible", False)
            if (SketchNumber < 2)
            {
                this.radioButtonSignalementUnique.Visible = false;
                this.radioButtonSignalementMultiple.Visible = false;
            }
            else
            {
                this.radioButtonSignalementMultiple.Text = string.Format("Créer {0} signalements distincts", SketchNumber.ToString());
            }

            // Récupération du profil utilisateur
            Profil = this.Contexte.Client.GetProfil();
            if (string.IsNullOrEmpty(Profil.Groupe.Nom))
            {
                this.groupImageProfil.Text = string.Format("{0} (Profil par défaut)", Profil.Auteur.Nom);
            }
            else
            {
                this.groupImageProfil.Text = string.Format("{0} ({1})", Profil.Auteur.Nom, Profil.Groupe.Nom);
            }

            // Les thèmes du profil
            ListThemes = Profil.Themes;
            ListFilteredThemes = Profil.FilteredThemes;
            ListPreferredThemes = Helper.Load_PreferredThemes();
            PreferredGroup = Helper.Load_PreferredGroup();

            // Ajout des noms de groupes trouvés pour l'utilisateur
            this.comboBoxGroupe.Items.Add(Constantes.AUCUN);
            foreach (GeoGroupe geogroupe in this.Profil.Geogroupes)
            {
                this.comboBoxGroupe.Items.Add(geogroupe.Nom);
            }

            // Par défaut la liste des groupes de la combobox est positionnée sur "Aucun"
            this.comboBoxGroupe.SelectedIndex = 0;

            // ou sur le groupe préféré
            if (!string.IsNullOrEmpty(PreferredGroup))
            {
                // Si le groupe préféré fait parti de la liste des groupes utilisateur
                if (this.Profil.Geogroupes.FindIndex(x => x.Nom.Equals(PreferredGroup)) != -1)
                {
                    this.comboBoxGroupe.SelectedItem = PreferredGroup;
                }
            }
            else
            {
                // ou sur le groupe actif qui fait parti de la liste des groupes utilisateur
                if(!string.IsNullOrEmpty(this.Contexte.Groupeactif))
                {
                    this.comboBoxGroupe.SelectedItem = this.Contexte.Groupeactif;
                }
            }

            if (!string.IsNullOrEmpty(PreferredGroup))
            {
                this.richTextBoxMessage.Text = this.Profil.Geogroupes.Find(x => x.Nom.Equals(PreferredGroup)).CommentaireGeorem;
            }
            else
            {
                this.richTextBoxMessage.Text = this.Profil.Geogroupes.Find(x => x.Nom.Equals(this.Contexte.Groupeactif)).CommentaireGeorem;
            }

            DisplayThemesInTreeView();




            /*this.maxSizePJ = iclient.Get_MAX_TAILLE_UPLOAD_FILE();
            this.OpenFileDialog.InitialDirectory = contexte.repertoireTravail;
            this.OpenFileDialog.FileName = "";

            if (profil.Themes.Count == 0)
            {
                this.checkedListBoxThemes.Enabled = false;
                this.checkedListBoxThemes.Visible = false;
                this.listeTheme.Clear();
            }
            else
            {
                foreach (ArcGisProEspaceCollaboratif.Core.Theme theme in profil.Themes)
                {
                    this.listeTheme.Add(theme);
                    this.checkedListBoxThemes.Items.Add(theme.Groupe.Nom);
                }
            }

            this.SetPreferredThemes();
            this.tabThemes.Text = "Thèmes (" + this.CountSelectedThemes() + ")";

            if (nbrSignalement == 1)
            {
                this.radioButtonSignalementMultiple.Enabled = false;
                this.radioButtonSignalementMultiple.Visible = false;
                this.radioButtonSignalementMultiple.Visible = false;
            }
            else
            {
                this.radioButtonSignalementMultiple.Enabled = true;
                this.radioButtonSignalementMultiple.Visible = true;
                this.radioButtonSignalementMultiple.Text = "Créer " + nbrSignalement + " signalements distincts.";
            }

            this.richTextBoxMessage.Text = contexte.Profil*/
        }

        /// <summary>
        /// Affiche les thèmes dans le formulaire en fonction du groupe de l'utilisateur.
        /// </summary>
        private void DisplayThemesInTreeView()
        {
            if (Profil.Themes.Count == 0)
            {
                // Pas de thèmes à afficher, on sort de la fonction
                return;
            }

            // Filtrage des thèmes utilisateur en fonction du contenu des thèmes de son profil
            int index = 0;
            foreach(string thName in ListFilteredThemes)
            {
                Theme th = null;
                bool foundTheme = false;
                foreach (Theme tmpth in ListThemes)
                {
                    if (thName == tmpth.Groupe.Nom)
                    {
                        foundTheme = true;
                        th = tmpth;
                        break;
                    }
                }
                if (!foundTheme)
                {
                    continue;
                }
                // Si le thème n'est pas dans le filtre du profil, on ne l'affiche pas
                if (!th.Filtered)
                {
                    continue;
                }

                // Ajout du thème dans le treeview
                //this.treeViewThemesAttributs.CheckBoxes = true;
                this.treeViewThemesAttributs.BeginUpdate();
                this.treeViewThemesAttributs.Nodes.Add(thName);
                if (this.ListPreferredThemes.Contains(thName))
                {
                    this.treeViewThemesAttributs.Nodes[index].Checked = true;
                }

                // Ajout des attributs du thème
                DisplayAttributsInTreeView(th, index);
                this.treeViewThemesAttributs.EndUpdate();
                index++;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        private void DisplayAttributsInTreeView(Theme th, int index)
        {
            foreach (ThemeAttributes att in th.Attributs)
            {
                this.treeViewThemesAttributs.Nodes[index].Nodes.Add(att.Nom);
                if (att.Obligatoire)
                {
                    this.treeViewThemesAttributs.Nodes[index].NodeFont = new Font(Font, FontStyle.Bold);
                }

                if (att.Type == "checkbox")
                {

                }
                else if (att.Type == "date")
                {

                }
                else if (att.Type == "datetime")
                {
                    this.treeViewThemesAttributs.Nodes[index].Nodes.Add(att.Nom);
                }
                else if (att.Type == "list")
                {
                    ComboBoxTreeNode cb = new ComboBoxTreeNode();
                    foreach (string val in att.Valeurs)
                    {
                        cb.ComboBox.Items.Add(val);
                    }
                    
                    TreeNode listNode = new TreeNode(att.Nom);
                    listNode.Nodes.Add(cb);
                    this.treeViewThemesAttributs.Nodes[index].Nodes.Add(listNode);
                }
                else
                {

                }
            } 
        }


               /* # 
                thItem = QTreeWidgetItem(self.treeWidget)
                thItem.setText(0, th.groupe.nom)
                thItem.setText(1, th.groupe.id)
                self.treeWidget.addTopLevelItem(thItem)

# Pour masquer la 2ème colonne (qui contient le groupe id)
                thItem.setForeground(1, QtGui.QBrush(Qt.white))

                if ClientHelper.notNoneValue(th.groupe.nom) in preferredThemes:
                    thItem.setCheckState(0, Qt.Checked)
                    thItem.setExpanded(True)
                else:
                    thItem.setCheckState(0, Qt.Unchecked)

                # ajout des attributs du thème
                for att in th.attributs:
                    attLabel = att.nom
                    attType = att.type
                    attDefaultval = att.defaultval

                    label = QtWidgets.QLabel(attLabel, self.treeWidget)
                    # Les attributs obligatoires sont en gras
                    if att.obligatoire is True:
                        myFont = QtGui.QFont()
                        myFont.setBold(True)
                        label.setFont(myFont)

                    if attType == "checkbox":
                        valeur = QtWidgets.QCheckBox(self.treeWidget)
                        valeur.setChecked(False)
                        if attDefaultval == '1' \
                                or attDefaultval == 'True' \
                                or attDefaultval == 'TRUE' \
                                or attDefaultval == 'true' \
                                or attDefaultval == 'Vrai' \
                                or attDefaultval == 'VRAI' \
                                or attDefaultval == 'vrai':
                            valeur.setChecked(True)

                        attItem = QtWidgets.QTreeWidgetItem()
                        thItem.addChild(attItem)
                        self.treeWidget.setItemWidget(attItem, 0, label)
                        self.treeWidget.setItemWidget(attItem, 1, valeur)

                    elif attType == 'date':
                        dateEdit = QDateEdit()

                        if attDefaultval is not None and attDefaultval != '':
                            #'2020-10-28'
                            date = attDefaultval.split('-')
                            dateEdit.setDate(QDate(int(date[0]), int (date[1]), int (date[2])))

                        dateEdit.setMinimumDate(QDate(1900, 1, 1))
                        dateEdit.setMaximumDate(QDate(3000, 1, 1))
                        dateEdit.setDisplayFormat("yyyy-MM-dd")
                        attItem = QtWidgets.QTreeWidgetItem()
                        thItem.addChild(attItem)
                        self.treeWidget.setItemWidget(attItem, 0, label)
                        self.treeWidget.setItemWidget(attItem, 1, dateEdit)

                    elif attType == 'datetime':
                        dateTimeEdit = QDateTimeEdit()

                        if attDefaultval is not None and attDefaultval != '':
                            #'2020-08-15 12:23:48'
                            dateTime = attDefaultval.split(' ')
                            date = dateTime[0].split('-')
                            time = dateTime[1].split(':')
                            dateTimeEdit.setDateTime(QDateTime(QDate(int(date[0]), int (date[1]), int (date[2])),
                                                               QTime(int(time[0]), int(time[1]), int(time[2]))))

                        dateTimeEdit.setMinimumDateTime(QDateTime(QDate(1900, 1, 1), QTime(0, 0, 0)))
                        dateTimeEdit.setMaximumDateTime(QDateTime(QDate(3000, 1, 1), QTime(0, 0, 0)))
                        dateTimeEdit.setDisplayFormat("yyyy-MM-dd HH:mm:ss")
                        attItem = QtWidgets.QTreeWidgetItem()
                        thItem.addChild(attItem)
                        self.treeWidget.setItemWidget(attItem, 0, label)
                        self.treeWidget.setItemWidget(attItem, 1, dateTimeEdit)

                    elif attType == 'list':
                        listAtt = QtWidgets.QComboBox(self.treeWidget)
                        listAtt.insertItems(0, att.valeurs)
                        attItem = QtWidgets.QTreeWidgetItem()
                        thItem.addChild(attItem)
                        self.treeWidget.setItemWidget(attItem, 0, label)
                        self.treeWidget.setItemWidget(attItem, 1, listAtt)

                    else:
                        valeur = QtWidgets.QLineEdit(self.treeWidget)
                        valeur.setText(attDefaultval)
                        attItem = QtWidgets.QTreeWidgetItem()
                        thItem.addChild(attItem)
                        self.treeWidget.setItemWidget(attItem, 0, label)
                        self.treeWidget.setItemWidget(attItem, 1, valeur)
                        */





        /// <summary>
        ///  Événement après double-click sur un fichier mis en pièce-jointe pour le retirer.
        /// </summary>  
        private void ListViewPJ_DoubleClick(object sender, EventArgs e)
        {
            this.ListFilesPJ.Clear();
            this.ViewListPJ();
            this.Refresh();
            this.listViewFilePJ.Visible = false;
            this.listViewFilePJ.Enabled = false;

            this.checkBoxDocument.Checked = false;
        }

        /// <summary>
        ///  Affiche dans le formulaire FormCreerSignalement le fichier sélectionné en pièce-jointe avec son icône associé.
        /// </summary>  
        public void ViewListPJ()
        {
            this.listViewFilePJ.Clear();
            ImageList imageList = new ImageList();

            foreach (String file in this.ListFilesPJ)
            {
                System.IO.FileInfo fichierPJ = new System.IO.FileInfo(file);

                Icon iconForFile = SystemIcons.WinLogo;
                iconForFile = Icon.ExtractAssociatedIcon(fichierPJ.FullName);
                ListViewItem item = new ListViewItem(fichierPJ.FullName, 1);

                if (!imageList.Images.ContainsKey(fichierPJ.Extension))
                {
                    iconForFile = System.Drawing.Icon.ExtractAssociatedIcon(fichierPJ.FullName);
                    imageList.Images.Add(fichierPJ.Extension, iconForFile);
                }

                item.ImageKey = fichierPJ.Extension;
                this.listViewFilePJ.Items.Add(item);
            }

            this.listViewFilePJ.SmallImageList = imageList;
            this.listViewFilePJ.View = View.SmallIcon;        
            this.listViewFilePJ.Refresh();

            bool hasPJ = (this.ListFilesPJ.Count != 0);
            this.checkBoxDocument.Checked = hasPJ;
            this.listViewFilePJ.Visible = hasPJ;
            this.listViewFilePJ.Enabled = hasPJ;

            if (hasPJ)
            {
                this.checkBoxDocument.Text = "Joindre un document :";               
            }
            else
            {
                this.checkBoxDocument.Text = "Joindre un document.";               
            }
        }           
      
        /// <summary>
        /// Présélectionné les thèmes dans le formulaire FormCreerSignalement à partir des thèmes préférés indiqués dans le fichier XML de paramétrage.
        /// </summary>        
        private void SetPreferredThemes()
        {
/*            List<String> themesPreferes = EspaceCollaboratifHelper.Load_PreferedThemes();
            if (themesPreferes.Count == 0) { return; }

            foreach (String themeRecherche in themesPreferes)
            {
                for (int i = 0; i < this.checkedListBoxThemes.Items.Count; i++)
                {
                    if (this.checkedListBoxThemes.Items[i].Equals(themeRecherche))
                    {
                        this.checkedListBoxThemes.SetItemChecked(i, true);
                        continue;
                    }
                }
            }*/
        }

        /// <summary>
        /// Indique si l'option de création d'un signalement unique a été choisie dans le formulaire FormCreerSignalement.
        /// </summary>  
        /// <returns>True si l'option de création d'un signalement unique est activée.</returns>
        public bool OptionSingleSignalement()
        {
            return this.radioButtonSignalementUnique.Checked;
        }

        /// <summary>
        /// Le message saisi dans le formulaire FormCreerSignalement et à affecter dans le nouveau signalement de l'Espace collaboratif.
        /// </summary>  
        /// <returns>Le message saisi pour le nouveau signalement.</returns>
        public String GetMessage()
        {
            return this.richTextBoxMessage.Text;
        }

        /// <summary>
        /// Les thèmes sélectionnés dans le formulaire FormCreerSignalement et à affecter au nouveau signalement.
        /// </summary>  
        /// <param name="profil">Le profil du rédacteur du nouveau signalement et qui contient les thèmes pour lesquels il peut y contribuer.</param>
        /// <returns>La liste des thèmes à affecter au nouveau signalement.</returns>
        public List<ArcGisProEspaceCollaboratif.Core.Theme> GetSelectedThemes(ArcGisProEspaceCollaboratif.Core.Profil profil)
        {
            /*            List<ArcGisProEspaceCollaboratif.Core.Theme> selectedTheme = new List<ArcGisProEspaceCollaboratif.Core.Theme>();

                        for (int i = 0; i < this.checkedListBoxThemes.Items.Count; i++)
                        {
                            if (this.checkedListBoxThemes.GetItemChecked(i))
                            {
                                selectedTheme.Add(profil.Themes[i]);
                            }
                        }

                        EspaceCollaboratifHelper.Save_PreferedThemes(selectedTheme);
                        return selectedTheme;*/
            return null;
        }

        /// <summary>
        /// Les thèmes sélectionnés dans le formulaire FormCreerSignalement et à affecter au nouveau signalement.
        /// </summary>         
        /// <returns>La liste des thèmes à affecter au nouveau signalement.</returns>
        /*public List<ArcGisProEspaceCollaboratif.Core.Theme> GetSelectedThemes()
        {
            List<ArcGisProEspaceCollaboratif.Core.Theme> selectedTheme = new List<ArcGisProEspaceCollaboratif.Core.Theme>();  

            for (int i = 0; i < this.checkedListBoxThemes.Items.Count; i++)
            {
                if (this.checkedListBoxThemes.GetItemChecked(i))
                {
                    selectedTheme.Add(  this.ListThemes[ i ] );
                }
            }

            Helper.Save_PreferredThemes(selectedTheme);
            return selectedTheme;

        }*/

        /// <summary>
        /// Décompte les thèmes qui sont sélectionnés dans le formulaire FormCreerSignalement.
        /// </summary>         
        /// <returns>Le nombre de thèmes sélectionnés dans le formulaire.</returns>
        /*public int CountSelectedThemes()
        {
            return this.checkedListBoxThemes.CheckedItems.Count;
        }*/

        /// <summary>
        /// Indique si des thèmes sont sélectionnés dans le formulaire FormCreerSignalement.
        /// </summary>         
        /// <returns>True s'il y a au moins un thème sélectionné dans le formulaire.</returns>
        /*public bool HasSelectedTheme()
        {
            return this.CountSelectedThemes() != 0;
        }*/

        /// <summary>
        /// Donne la liste des fichiers sélectionnés dans le formulaire FormCreerSignalement et à mettre en pièce-jointe dans le nouveau signalement à créer.
        /// </summary>         
        /// <returns>Liste du chemin d'accès complet de chaque fichier choisi comme pièce-jonte.</returns>
        public List<String> GetFichierPJ()
        {
            if (this.checkBoxDocument.Checked == true)
            {
                return this.ListFilesPJ;
            }
            else
            {
                return new List<string>();
            }
        }
        
        /// <summary>
        ///  Indique si l'option de joindre un croquis au nouveau signalement qu'on veut créer, a été choisi dans le formulaire FormCreerSignalement.
        /// </summary>         
        /// <returns>True si l'option de joindre un croquis est activée.</returns>
        public bool OptionWithCroquis()
        {
            return this.checkBoxCroquis.Checked;
        }

        /// <summary>
        ///  Action après click-souris sur la liste des thèmes dans le formulaire FormCreerSignalement. 
        /// </summary>  
        /*private void CheckedListBoxThemes_MouseUp(object sender, MouseEventArgs e)
        {
            this.tabThemes.Text = "Thèmes (" + this.CountSelectedThemes() + ")";
            this.buttonCreer.Enabled = ((this.richTextBoxMessage.TextLength != 0) && (this.HasSelectedTheme()));
        }*/

        /// <summary>
        ///  Action après changement dans l'option de joindre un fichier dans le formulaire FormCreerSignalement. 
        /// </summary>  
        private void CheckBoxDocument_CheckedChanged(object sender, EventArgs e)
        {
            this.listViewFilePJ.Enabled = this.checkBoxDocument.Checked;
            this.listViewFilePJ.Visible = this.checkBoxDocument.Checked;

            // Procédure de sélection du document à joindre au nouveau signalement.
            if (this.checkBoxDocument.Checked)
            {
                this.OpenFileDialog.Filter = "Images (*.BMP;*.JPG;*.GIF;*.JPG2000;*.TIFF;*.ECW;*.PSD)|*.BMP;*.JPG;*.GIF;*.JPG2000;*.TIFF,*.ECW;*.PSD|"
                                      + "Tracés (*.KML;*.GPX;*.SWG;*.WMF;*.AI)|*.KML;*.GPX;*.SWG;*.WMF;*.AI|"
                                      + "Textes (*.TXT;*.PDF;*.RTF;*.DOC;*.DOCX;*.ODT)|*.TXT;*.PDF;*.RTF;*.DOC;*.DOCX;*.ODT|"
                                      + "Tableurs (*.XML;*.CSV;*XLS;*.XLSX;*.ODS)|*.XML;*.CSV;*XLS;*.XLSX;*.ODS|"
                                      + "Bases de données (*.MDB;*.MDBX;*.ODB;*.DBF)|*.MDB;*.MDBX;*.ODB;*.DBF|"
                                      + "SIG (*.SHP;*.LYR;*.GDB;*.MXD;*GCM;*.GCR;*.DXF;*.DWG;*.QGS;*.MIF;*MID)|*.SHP;*.LYR;*.GDB;*.MXD;*GCM;*.GCR;*.DXF;*.DWG;*.QGS;*.MIF;*MID|"
                                      + "All files (*.*)|*.*";

                this.OpenFileDialog.FilterIndex = 7;

                DialogResult dr = this.OpenFileDialog.ShowDialog();
                if (dr != System.Windows.Forms.DialogResult.OK)
                { 
                    // Si abandon dans la sélection du document.
                    this.listViewFilePJ.Items.Clear();
                    this.listViewFilePJ.Enabled = false;
                    this.listViewFilePJ.Visible = false;
                    this.checkBoxDocument.Checked = false;
                }
                else
                {
                    // Si un fichier est bien choisi.
                    System.IO.FileInfo fichierPJ = new System.IO.FileInfo(this.OpenFileDialog.FileName);
                    string extension = fichierPJ.Extension;

                    // Vérification du format du fichier (exclusion des formats potentiellement malveillants)
                    if (extension.Equals(".php") || extension.Equals(".exe") || extension.Equals(".dll"))
                    {
                        string message = "Les fichiers de type ''" +
                                         fichierPJ.Extension +
                                         "'' ne sont pas autorisés comme pièce-jointe par le service Espace collaboratif.";
                        System.Windows.Forms.MessageBox.Show(message, "IGN Espace collaboratif - STOP", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Stop);
                       
                        this.ViewListPJ();
                        this.listViewFilePJ.Refresh();                        
                        
                        return;
                    }
                    else
                    {
                        // Vérification si le fichier a une taille acceptable pour le service Espace collaboratif.
                        if (fichierPJ.Length > this.MaxSizeFilePJ)
                        {
                            string message = "Le fichier ''" +
                                             fichierPJ.FullName +
                                             "'' ne peut être envoyé au service de l'Espace collaboratif, car sa taille (" +
                                             fichierPJ.Length / 1024 +
                                             " Ko) dépasse celle maximale autorisée. (" +
                                             this.MaxSizeFilePJ / 1024 +
                                             " Ko)";
                            System.Windows.Forms.MessageBox.Show(message, "IGN Espace collaboratif - STOP", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Stop);
                            this.ViewListPJ();
                            this.listViewFilePJ.Refresh();
                            
                            return;
                        }
                        else
                        {
                            this.ListFilesPJ.Clear();
                            this.ListFilesPJ.Add(fichierPJ.FullName);
                            this.ViewListPJ();
                            this.ViewListPJ();
                            this.listViewFilePJ.Refresh();
                        }
                    }
                }
            }
            this.listViewFilePJ.Refresh();
        }

        /// <summary>
        ///  Action après modification du message dans le formulaire FormCreerSignalement. 
        /// </summary>  
        /*private void RichTextBoxMessage_TextChanged(object sender, EventArgs e)
        {
            this.buttonCreer.Enabled = ((this.richTextBoxMessage.TextLength != 0) && (this.HasSelectedTheme()));
        }*/
    }
}
