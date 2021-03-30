using ArcGisProEspaceCollaboratif.Core;
using ArcGisProEspaceCollaboratif.Utils;
using ArcGisProEspaceCollaboratif.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace ArcGisProEspaceCollaboratif.ViewModels
{
    class CreateReportViewModel : ViewModelBase
    {
        #region Parameters
        /// <summary>
        /// L'instance du dialogue "Créer un nouveau signalement"
        /// </summary>
        public CreateReportView createReportView;

        /// <summary>
        /// Le contexte de travail qui contient le résultat de la requete
        /// vers l'espace collaboratif Profil/GeoGroupes/Groupes/Thèmes/Attributs
        /// </summary>
        public Contexte Context { get; set; }

        /// <summary>
        /// Le nombre d'objets sélectionnés lié au signalement
        /// </summary>
        public int SketchNumber { get; set; } = 0;

        /// <summary>
        /// Liste des fichiers en pièce-jointe
        /// </summary>
        public List<string> ListAttachedFiles { get; set; }

        /// <summary>
        /// Taille maximale des fichiers en pièce-jointe
        /// </summary>
        public int MaxSizeAttachedFile { get; set; }

        /// <summary>
        /// Le groupe préféré de l'utilisateur car il peut changer
        /// en cours de saisie des signalements
        /// </summary>
        public string PreferredGroup { get; set; } = "";

        /// <summary>
        /// Liste des thèmes préférés de l'utilisateur cochés
        /// lors de la saisie d'un nouveau signalement
        /// et présents dans le fichier de configuration espace_co.xml
        /// </summary>
        public List<string> ListPreferredThemes { get; set; } = new List<string>();

        /// <summary>
        /// Le nom du ficher qui est joint au signalement
        /// </summary>
        public string NameFileJoinToReport { get; set; } = "";

        /// <summary>
        /// Le control "StackPanel" général contenant
        /// l'ensemble des items de l'onglet "Thèmes"
        /// </summary>
        public StackPanel StackPanelGlobal { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initialisation du dialogue "Créer un nouveau signalement"
        /// </summary>
        public CreateReportViewModel(Contexte context, int sketchNumber)
        {
            this.Context = context;
            this.ListPreferredThemes = Helper.Load_PreferredThemes();
            this.PreferredGroup = Helper.Load_PreferredGroup();
            this.SketchNumber = sketchNumber;
            this.createReportView = new CreateReportView();
            this.InitializeCreateReportView();

        }
        #endregion

        #region Bindings
        /// <summary>
        /// Mise à jour de l'entête de la GroupBox avec
        /// "Nom de l'utilisateur (nom du groupe actif)"
        /// </summary>
        public string UserProfileHeaderGroupBox { get; set; }

        /// <summary>
        /// Mise à jour de la liste des groupes dans la ComboBox "Groupe"
        /// </summary>
        public ObservableCollection<string> GroupItemsSourceComboBox { get; set; }

        /// <summary>
        /// L'item de groupe qui sélectionné dans la ComboBox "Groupe"
        /// </summary>
        private string selectedItem;

        /// <summary>
        /// Mise à jour de la sélection par défaut de la ComboBox "Groupe"
        /// </summary>
        public string GroupSelectedItemComboBox
        {
            get { return selectedItem; }

            set
            {
                selectedItem = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Mise à jour du texte du Label qui donne le nom
        /// du fichier et son répertoire qui sera joint au signalement
        /// </summary>
        public string JoinDocumentLabel { get; set; } = "";

        /// <summary>
        /// La CheckBox "Joindre un croquis" est coché/décoché
        /// </summary>
        public bool JoinSketchIsChecked { get; set; }

        /// <summary>
        /// Le RadioButton "Créer un signalement unique" est coché/décoché
        /// </summary>
        public bool CreateReportIsChecked { get; set; } = true;

        /// <summary>
        /// Mise à jour du nom du RadioButton "Créer xx signalements"
        /// </summary>
        public string CreateReportsContent { get; set; } = "Créer xx signalements";

        /// <summary>
        /// Le RadioButton "Créer xx signalements" est coché/décoché
        /// </summary>
        public bool CreateReportsIsChecked { get; set; }

        /// <summary>
        /// Mise à jour de la zone d'édition "Commentaire"
        /// </summary>
        public string CommentaireTextBox { get; set; }
        #endregion

        #region Commands
        public ICommand JoinDocumentCmd { get { return new RelayCommand(OnJoinDocument, AlwaysTrue); } }

        /// <summary>
        /// L'utilsateur a cliqué sur la CheckBox "Joindre un document".
        /// Il faut lui présenter une boite "Parcourir" pour qu'il joigne
        /// un fichier à son signalement et afficher le nom du fichier
        /// et son répertoire dans le label dédié
        /// </summary>
        private void OnJoinDocument()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Document à joindre au signalement",
                Multiselect = false,
                InitialDirectory = this.Context.DirectoryWorking,
                Filter = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", Constantes.ALLFILE, Constantes.IMAGEFILE, Constantes.TRACKFILE, Constantes.TXTFILE, Constantes.SHEETFILE, Constantes.DBFILE, Constantes.SIGFILE)
            };
            bool? dialogResult = openFileDialog.ShowDialog();
            if (dialogResult == true)
            {
                string message = "";
                try
                {
                    // Est-ce que le format de fichier joint est autorisé ?
                    string extension = openFileDialog.FileName.Split('.')[1];
                    if (!Helper.IsFileExtensionAuthorised(extension))
                    {
                        message = string.Format("Les fichiers de type '.{0}' ne sont pas autorisés comme pièce-jointe", extension);
                        throw new Exception(message);
                    }

                    // Est-ce que la taille du fichier dépasse 16 Mo (MAX_TAILLE_UPLOAD_FILE)
                    long fileSize = new FileInfo(openFileDialog.FileName).Length;
                    if (fileSize > Constantes.MAX_TAILLE_UPLOAD_FILE)
                    {
                        message = string.Format("Le fichier {0} ne peut être envoyé car sa taille ({1} Ko) dépasse celle maximale autorisée ({2} Ko)", openFileDialog.FileName, fileSize / 1000, Constantes.MAX_TAILLE_UPLOAD_FILE / 1000);
                        throw new Exception(message);
                    }

                    this.createReportView.JoinDocumentCheckBox.IsChecked = true;
                    this.createReportView.JoinDocumentLabel.Content = openFileDialog.FileName;
                    this.NameFileJoinToReport = openFileDialog.FileName;
                }
                catch
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, "Espace collaboratif");
                    this.createReportView.JoinDocumentLabel.Content = Constantes.NOFILE;
                    this.createReportView.JoinDocumentCheckBox.IsChecked = false;
                }
            }
            else
            {
                this.createReportView.JoinDocumentLabel.Content = Constantes.NOFILE;
                this.createReportView.JoinDocumentCheckBox.IsChecked = false;
            }
        }

        public ICommand SendButtonCmd { get { return new RelayCommand(OnSend, AlwaysTrue); } }

        /// <summary>
        /// L'utilisateur a cliqué sur le bouton "Envoyer"
        /// </summary>
        private void OnSend()
        {

        }

        private bool AlwaysTrue() { return true; }
        #endregion

        #region Methods
        /// <summary>
        /// Initialisation du contenu du dialogue "Créer un nouveau signalement"
        /// </summary>
        private void InitializeCreateReportView()
        {
            // Entête (UserProfileGroupBox) avec le nom de l'utilisateur et son profil actif
            this.SetUserProfileHeaderGroupBox();
            // Onglet "Thèmes"
            this.SetGroupItemsSourceComboBox();
            this.SetGroupSelectedItemComboBox();
            this.SetStackPanelGlobal();
            // Onglet "Options"
            this.SetJoinSketchIsChecked();
            this.SetJoinDocumentLabel();
            this.SetCreateReportRadioButton();
            // Zone d'édition pour un commentaire
            this.SetCommentaireTextBox();
            
        }

        /// <summary>
        /// Mise à jour de l'entête de la GroupBox avec le nom de l'utilisateur
        /// et son groupe actif
        /// </summary>
        /// <returns></returns>
        private void SetUserProfileHeaderGroupBox()
        {
            string textGroupBox = "";
            if (string.IsNullOrEmpty(this.Context.Profil.Group.Name))
            {
                textGroupBox = string.Format("{0} (Profil par défaut)", this.Context.Profil.Author.Nom);
            }
            else
            {
                textGroupBox = string.Format("{0} ({1})", this.Context.Profil.Author.Nom, this.Context.Profil.Group.Name);
            }
            this.UserProfileHeaderGroupBox = textGroupBox;
        }

        #region Initialize TabControl "Thèmes"
        /// <summary>
        /// Création d'un control "Label" qui est le nom de l'attribut
        /// </summary>
        /// <param name="content">Le nom de l'attribut</param>
        /// <param name="bold">A true si l'attribut est obligatoire</param>
        /// <returns>Le Label mis à jour</returns>
        private Label SetLabel(string content, bool bold)
        {
            Label label = new Label
            {
                Content = content
            };
            if (bold)
            {
                label.FontWeight = FontWeights.Bold;
            }
            return label;
        }

        /// <summary>
        /// Création d'un control "TextBox" qui représente
        /// un des types d'attribut possible
        /// </summary>
        /// <param name="text">Le texte à afficher</param>
        /// <returns>La TextBox mise à jour</returns>
        private TextBox SetTextBox(string text)
        {
           TextBox textBox = new TextBox
            {
               Text = text
            };
            return textBox;
        }

        /// <summary>
        /// Création d'un control "CheckBox" qui représente
        /// un des types d'attribut possible
        /// </summary>
        /// <param name="content">Le nom de l'attribut</param>
        /// <param name="check">Valeur par défaut de la coche</param>
        /// <returns>La CheckBox mise à jour</returns>
        private CheckBox SetCheckBox(string content, string check)
        {
            CheckBox checkBox = new CheckBox()
            {
                IsChecked = false,
                Content = content
            };
            if (check == "1" ||
                check == "True" ||
                check == "TRUE" ||
                check == "true" ||
                check == "Vrai" ||
                check == "VRAI" ||
                check == "vrai")
            {
                checkBox.IsChecked = true;
            }
            return checkBox;
        }

        /// <summary>
        /// Création d'un control "ComboBox" qui représente
        /// un des types d'attribut possible
        /// </summary>
        /// <param name="values">La liste des valeurs possibles</param>
        /// <param name="defaultValue">La valeur sélectionnée par défaut </param>
        /// <returns>La ComboBox mise à jour</returns>
        private ComboBox SetComboBox(List<string> values, string defaultValue)
        {
            ComboBox comboBox = new ComboBox()
            {
                ItemsSource = values,
                SelectedItem = defaultValue
            };
            return comboBox;
        }

        /// <summary>
        /// Création d'un control "DatePicker" qui représente
        /// un des types d'attribut possible
        /// Le format de la date est ""yyyy-MM-dd""
        /// </summary>
        /// <param name="value">La date par défaut à afficher</param>
        /// <returns>Le "DatePicker" mis à jour</returns>
        private DatePicker SetDatePicker(string value)
        {
            DateTime dateTime = new DateTime();
            if (string.IsNullOrEmpty(value))
            {
                dateTime = DateTime.Today;
            }
            else
            {
                // 2020-10-28
                string[] tmp = value.Split('-');
                dateTime = new DateTime(Int32.Parse(tmp[0]), Int32.Parse(tmp[1]), Int32.Parse(tmp[2]));
            }

            DatePicker datePicker = new DatePicker
            {
                DisplayDateStart = new DateTime(1900, 1, 1),
                DisplayDateEnd = new DateTime(3000, 12, 31),
                SelectedDate = dateTime
            };

            return datePicker;
        }

        /// <summary>
        /// Création d'un control "DatePicker" qui représente
        /// un des types d'attribut possible
        /// Le format de la date est "yyyy-MM-dd HH:mm:ss"
        /// </summary>
        /// <param name="value">La date par défaut à afficher</param>
        /// <returns>Le "DatePicker" mis à jour</returns>
        private DatePickerTextBox SetDatePickerTextBox(string value)
        {
            DateTime dateTime = new DateTime();
            if (string.IsNullOrEmpty(value))
            {
                dateTime = DateTime.Now;
            }
            else
            {
                // 2020-10-28
                string[] tabTmp = value.Split(' ');
                string[] tabDate = tabTmp[0].Split('-');
                string[] tabTime = tabTmp[1].Split(':');
                dateTime = new DateTime(
                    Int32.Parse(tabTmp[0]),
                    Int32.Parse(tabTmp[1]),
                    Int32.Parse(tabTmp[2]),
                    Int32.Parse(tabTime[0]),
                    Int32.Parse(tabTime[1]),
                    Int32.Parse(tabTime[2]));
            }
            //ArcGIS.Desktop.Internal.Framework.Controls.DateTimePicker dateTimePicker = new ArcGIS.Desktop.Internal.Framework.Controls.DateTimePicker();
            DatePickerTextBox datePickerTextBox = new DatePickerTextBox()
            {
                SelectedText = dateTime.ToString("yyyy-MM-dd HH:mm:ss"),
            };

            return datePickerTextBox;
        }

        /// <summary>
        /// Création d'un control ScrollViewer pour l'onglet 'Thèmes'
        /// qui permet de faire défiler les attributs de l'ensemble
        ///  des thèmes pour le groupe de l'utilisateur 
        /// </summary>
        /// <returns>Le ScrollViewer mis à jour</returns>
        private ScrollViewer SetScrollViewer()
        {
            ScrollViewer scrollViewer = new ScrollViewer()
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            return scrollViewer;
        }

        /// <summary>
        /// Création d'un control "Expander"
        /// dépliant les attributs d'un thème
        /// </summary>
        /// <param name="header">Le nom du thème</param>
        /// <returns>L'expander mis à jour</returns>
        private Expander SetExpander(string header)
        {
            Expander expander = new Expander()
            {
                Header = header,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left
            };
            return expander;
        }

        /// <summary>
        /// Création et mise à jour d'un control "StackPanel"
        /// contenant tous les thèmes/attributs du groupe utilisateur
        /// disposés sur une seule ligne les uns en dessous des autres
        /// </summary>
        private void SetStackPanelGlobal()
        {
            this.StackPanelGlobal = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Left
            };
        }

        /// <summary>
        /// L'utilisateur a sélectionné un nom de groupe dans la ComboBox "Groupe"
        /// Il faut modifier le contenu du TabControl "Thèmes"
        /// </summary>
        private void OnPropertyChanged()
        {     
            if (this.StackPanelGlobal != null)
            {
                this.createReportView.GridGeneral.Children.Clear();
                this.StackPanelGlobal = null;
            }

            this.SetStackPanelGlobal();

            // L'utilisateur a selectionné un groupe autre que celui qui est actif
            string theme = this.GroupSelectedItemComboBox;

            if (theme == Constantes.AUCUN)
            {
                this.StackPanelGlobal.Children.Add(SetLabel("Pas de groupe, pas d'attributs", true));
                this.createReportView.GridGeneral.Children.Add(this.StackPanelGlobal);
                return;
            }

            // Il faut récupérer les thèmes du nouveau groupe
            List<Theme> listThemesGroup = new List<Theme>();
            List<string> filteredThemes = new List<string>();
            foreach (GeoGroupe geoGroupe in this.Context.Profil.Geogroupes)
            {
                if (theme == geoGroupe.Name)
                {
                    listThemesGroup = geoGroupe.Themes;
                    filteredThemes = geoGroupe.FilteredThemes;
                    break;
                }
            }

            // Il faut afficher uniquement les thèmes filtrés   
            foreach (string thName in filteredThemes)
            {
                foreach (Theme thGroup in listThemesGroup)
                {
                    if (thName != thGroup.Group.Name)
                    {
                        continue;
                    }
                    // Si le thème n'est pas dans le filtre du profil, on ne l'affiche pas
                    if (!thGroup.Filtered)
                    {
                        continue;
                    }
                    // Un thème peut ne pas avoir d'attributs
                    if (thGroup.Attributes == null)
                    {
                        this.StackPanelGlobal.Children.Add(SetLabel("Pas d'attributs", true));
                        continue;
                    }
                    // Un expander par theme qui contient tous les attributs initialisés par type
                    Expander expander = SetExpander(thName);
                    expander.Content = DisplayTypeAttributes(thGroup);
                    // Ajout de l'expander au StackPanel global
                    this.StackPanelGlobal.Children.Add(expander);
                }
            }
            ScrollViewer scrollViewer = SetScrollViewer();
            scrollViewer.Content = this.StackPanelGlobal;
            this.createReportView.GridGeneral.Children.Add(scrollViewer);
        }

        /// <summary>
        /// Dispose les attributs en créant un Control Windows par type
        /// sur une ligne horizontale les uns au-dessous des autres
        /// </summary>
        /// <param name="themeGroup">Le theme sélectionné par l'utilisateur contenant les attributs à afficher</param>
        /// <returns></returns>
        private StackPanel DisplayTypeAttributes(Theme themeGroup)
        {
            StackPanel stackPanel = new StackPanel();
            foreach (ThemeAttributes att in themeGroup.Attributes)
            {
                // Nom d'attribut obligatoire en gras
                bool bold = false;
                if (att.Required)
                {
                    bold = true;
                }
                
                // Valeur de l'attribut remplie ? sinon valeur par défaut
                string value = "";
                if (!string.IsNullOrEmpty(att.Value))
                {
                    value = att.Value;
                }
                if (!string.IsNullOrEmpty(att.DefaultValue))
                {
                    value = att.DefaultValue;
                }

                // Types d'attributs
                if (att.Type == "checkbox")
                {
                    stackPanel.Children.Add(SetCheckBox(att.Name, value));
                }
                else if (att.Type == "list")
                {
                    stackPanel.Children.Add(SetLabel(att.Name, bold));
                    stackPanel.Children.Add(SetComboBox(att.Values, value));
                }
                else if (att.Type == "date")
                {
                    stackPanel.Children.Add(SetLabel(att.Name, bold));
                    stackPanel.Children.Add(SetDatePicker(value));
                }
                else if (att.Type == "datetime")
                {
                    stackPanel.Children.Add(SetLabel(att.Name, bold));
                    stackPanel.Children.Add(SetDatePickerTextBox(value));
                }
                else
                {
                    // textBox pour tous les autres types d'attributs
                    stackPanel.Children.Add(SetLabel(att.Name, bold));
                    stackPanel.Children.Add(SetTextBox(value));
                }
            }
            return stackPanel;
        }
        #endregion

        #region Initialize TabControl "Options"
        /// <summary>
        /// Mise à jour de la ComboBox "Groupe" avec les noms
        /// des groupes auquels l'utilisateur appartient
        /// </summary>
        private void SetGroupItemsSourceComboBox()
        {
            ObservableCollection<string> listGroup = new ObservableCollection<string>
            {
                Constantes.AUCUN
            };
            foreach (GeoGroupe geogroupe in this.Context.Profil.Geogroupes)
            {
                listGroup.Add(geogroupe.Name);
            }
            this.GroupItemsSourceComboBox = listGroup;
        }

        /// <summary>
        /// Sélection par défaut du dernier groupe utilisateur dans la ComboBox "Groupe" 
        /// </summary>
        private void SetGroupSelectedItemComboBox()
        {
            // Par défaut la liste des groupes de la combobox est positionnée sur "Aucun"
            string item = Constantes.AUCUN;

            // ou sur le groupe préféré
            if (!string.IsNullOrEmpty(this.PreferredGroup))
            {
                // Si le groupe préféré fait parti de la liste des groupes utilisateur
                if (this.Context.Profil.Geogroupes.FindIndex(x => x.Name.Equals(PreferredGroup)) != -1)
                {
                    item = PreferredGroup;
                }
            }
            else
            {
                // ou sur le groupe actif qui fait parti de la liste des groupes utilisateur
                if (!string.IsNullOrEmpty(this.Context.Groupeactif))
                {
                    item = this.Context.Groupeactif;
                }
            }
            this.GroupSelectedItemComboBox = item;
        }

        /// <summary>
        /// La CheckBox "Joindre un croquis" est cochée
        /// si le nombre d'objets sélectionnés est au moins égal à 1
        /// </summary>
        private void SetJoinSketchIsChecked()
        {
            this.JoinSketchIsChecked = false;
            if (this.SketchNumber >= 1)
            {
                this.JoinSketchIsChecked = true;
            }
        }

        /// <summary>
        /// Mise à jour du Label qui affiche le répertoire
        /// et le nom du fichier qui sera joint au signalement
        /// </summary>
        private void SetJoinDocumentLabel()
        {
            this.JoinDocumentLabel = Constantes.NOFILE;
        }

        /// <summary>
        /// Mise à jour des RadioButton de création d'un ou plusieurs signalements
        /// en fonction du nombre d'objets sélectionnés
        /// </summary>
        private void SetCreateReportRadioButton()
        {
            if (this.SketchNumber >= 2)
            {
                this.CreateReportsContent = string.Format("Créer {0} signalements", this.SketchNumber);
                this.CreateReportIsChecked = false;
                this.CreateReportsIsChecked = true;
            }
        }
        #endregion

        /// <summary>
        /// Mise à jour de la zone d'édition "Commentaire" avec la valeur de l'attribut
        /// avec la valeur de l'attribut "COMMENTAIRE_GEOREM"
        /// </summary>
        private void SetCommentaireTextBox()
        {
            string message = "";
            if (!string.IsNullOrEmpty(this.PreferredGroup))
            {
                message = this.Context.Profil.Geogroupes.Find(x => x.Name.Equals(PreferredGroup)).CommentaryGeorem;
            }
            else
            {
                message = this.Context.Profil.Geogroupes.Find(x => x.Name.Equals(this.Context.Groupeactif)).CommentaryGeorem;
            }
            this.CommentaireTextBox = message;
        }
        #endregion
    }
}
