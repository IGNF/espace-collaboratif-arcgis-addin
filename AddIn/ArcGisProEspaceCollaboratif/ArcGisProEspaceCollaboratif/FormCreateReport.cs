using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ArcGisProEspaceCollaboratif
{
    public partial class FormCreateReport : Form
    {

        public List<String> listePJ = new List<String>(); // Liste des fichiers en pièce-jointe
        public int maxSizePJ; // Taille maximalle des fichiers en pièce-jointe
        public List<ArcGisProEspaceCollaboratif.Core.Theme> listeTheme = new List<ArcGisProEspaceCollaboratif.Core.Theme>(); // Liste de thèmes sélectionnables.

        public FormCreateReport()
        {
            InitializeComponent();
            listePJ = new List<String>();          
            this.toolTip.SetToolTip(this.checkedListBoxThemes, "Sélectionnez les thèmes auxquels vous souhaitez associer le nouveau signalement.");
            this.toolTip.SetToolTip(this.richTextBoxMessage, "Rédigez ici le message pour le nouveau signalement.");
            this.toolTip.SetToolTip(this.buttonCreer, "Créer le nouveau signalement.\n Nécessite au moins qu'un thème soit associé et qu'un message ait été rédigé.");
        }

        /// <summary>
        /// Remplit le formulaire FormCreerSignalement avant son utilisation.
        /// </summary>         
        /// <param name="nbrSignalement">Le nombre de remarques Ripart à créer si l'option de création d'une unique remarque n'est pas choisie.</param>
        /// <param name="contexte">Le contexte de la carte en cours ouverte dans ArcMap.</param>
        /// <param name="iclient">La connexion avec le service en-ligne Ripart.</param>
        public void SetFormulaire(int nbrSignalement, Contexte contexte, ArcGisProEspaceCollaboratif.Core.IClient iclient)
        {
            ArcGisProEspaceCollaboratif.Core.Profil profil = iclient.GetProfil();

            this.groupImageProfil.Text = "Profil: " + profil.Geogroupe.Nom;
            this.pictureProfil.ImageLocation = profil.Logo;
            this.maxSizePJ = iclient.Get_MAX_TAILLE_UPLOAD_FILE();
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
        }

        /// <summary>
        ///  Événement après double-click sur un fichier mis en pièce-jointe pour le retirer.
        /// </summary>  
        private void ListViewPJ_DoubleClick(object sender, EventArgs e)
        {
            this.listePJ.Clear();
            this.ViewListPJ();
            this.Refresh();
            this.listViewPJ.Visible = false;
            this.listViewPJ.Enabled = false;

            this.checkBoxDocument.Checked = false;
        }

        /// <summary>
        ///  Affiche dans le formulaire FormCreerSignalement le fichier sélectionné en pièce-jointe avec son icône associé.
        /// </summary>  
        public void ViewListPJ()
        {
            this.listViewPJ.Clear();
            ImageList imageList = new ImageList();

            foreach (String file in this.listePJ)
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
                this.listViewPJ.Items.Add(item);
            }

            this.listViewPJ.SmallImageList = imageList;
            this.listViewPJ.View = View.SmallIcon;        
            this.listViewPJ.Refresh();

            bool hasPJ = (this.listePJ.Count != 0);
            this.checkBoxDocument.Checked = hasPJ;
            this.listViewPJ.Visible = hasPJ;
            this.listViewPJ.Enabled = hasPJ;

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
        public List<ArcGisProEspaceCollaboratif.Core.Theme> GetSelectedThemes()
        {
                        List<ArcGisProEspaceCollaboratif.Core.Theme> selectedTheme = new List<ArcGisProEspaceCollaboratif.Core.Theme>();  

                        for (int i = 0; i < this.checkedListBoxThemes.Items.Count; i++)
                        {
                            if (this.checkedListBoxThemes.GetItemChecked(i))
                            {
                                selectedTheme.Add(  this.listeTheme[ i ] );
                            }
                        }

                        EspaceCollaboratifHelper.Save_PreferedThemes(selectedTheme);
                        return selectedTheme;
            return null;
        }

        /// <summary>
        /// Décompte les thèmes qui sont sélectionnés dans le formulaire FormCreerSignalement.
        /// </summary>         
        /// <returns>Le nombre de thèmes sélectionnés dans le formulaire.</returns>
        public int CountSelectedThemes()
        {
            return this.checkedListBoxThemes.CheckedItems.Count;
        }

        /// <summary>
        /// Indique si des thèmes sont sélectionnés dans le formulaire FormCreerSignalement.
        /// </summary>         
        /// <returns>True s'il y a au moins un thème sélectionné dans le formulaire.</returns>
        public bool HasSelectedTheme()
        {
            return this.CountSelectedThemes() != 0;
        }

        /// <summary>
        /// Donne la liste des fichiers sélectionnés dans le formulaire FormCreerSignalement et à mettre en pièce-jointe dans le nouveau signalement à créer.
        /// </summary>         
        /// <returns>Liste du chemin d'accès complet de chaque fichier choisi comme pièce-jonte.</returns>
        public List<String> GetFichierPJ()
        {
            if (this.checkBoxDocument.Checked == true)
            {
                return this.listePJ;
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
        private void CheckedListBoxThemes_MouseUp(object sender, MouseEventArgs e)
        {
            this.tabThemes.Text = "Thèmes (" + this.CountSelectedThemes() + ")";
            this.buttonCreer.Enabled = ((this.richTextBoxMessage.TextLength != 0) && (this.HasSelectedTheme()));
        }

        /// <summary>
        ///  Action après changement dans l'option de joindre un fichier dans le formulaire FormCreerSignalement. 
        /// </summary>  
        private void CheckBoxDocument_CheckedChanged(object sender, EventArgs e)
        {
            this.listViewPJ.Enabled = this.checkBoxDocument.Checked;
            this.listViewPJ.Visible = this.checkBoxDocument.Checked;

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
                    this.listViewPJ.Items.Clear();
                    this.listViewPJ.Enabled = false;
                    this.listViewPJ.Visible = false;
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
                        this.listViewPJ.Refresh();                        
                        
                        return;
                    }
                    else
                    {
                        // Vérification si le fichier a une taille acceptable pour le service Espace collaboratif.
                        if (fichierPJ.Length > this.maxSizePJ)
                        {
                            string message = "Le fichier ''" +
                                             fichierPJ.FullName +
                                             "'' ne peut être envoyé au service de l'Espace collaboratif, car sa taille (" +
                                             fichierPJ.Length / 1024 +
                                             " Ko) dépasse celle maximale autorisée. (" +
                                             this.maxSizePJ / 1024 +
                                             " Ko)";
                            System.Windows.Forms.MessageBox.Show(message, "IGN Espace collaboratif - STOP", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Stop);
                            this.ViewListPJ();
                            this.listViewPJ.Refresh();
                            
                            return;
                        }
                        else
                        {
                            this.listePJ.Clear();
                            this.listePJ.Add(fichierPJ.FullName);
                            this.ViewListPJ();
                            this.ViewListPJ();
                            this.listViewPJ.Refresh();
                        }
                    }
                }
            }
            this.listViewPJ.Refresh();
        }

        /// <summary>
        ///  Action après modification du message dans le formulaire FormCreerSignalement. 
        /// </summary>  
        private void RichTextBoxMessage_TextChanged(object sender, EventArgs e)
        {
            this.buttonCreer.Enabled = ((this.richTextBoxMessage.TextLength != 0) && (this.HasSelectedTheme()));
        }

        private void ToolTip_Popup(object sender, PopupEventArgs e)
        {
        }
    }
}
