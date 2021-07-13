using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGisProEspaceCollaboratif.Core;
using ArcGisProEspaceCollaboratif.Utils;
using ArcGisProEspaceCollaboratif.Views;
using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using static ArcGisProEspaceCollaboratif.Core.Status;

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
        public Context Context { get; set; }

        /// <summary>
        /// Le nombre d'objets sélectionnés lié au signalement
        /// </summary>
        public int SketchNumber { get; set; } = 0;

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
        /// Le nom du fichier qui est joint au signalement
        /// </summary>
        public string NameFileJoinToReport { get; set; } = "";

        /// <summary>
        /// Le control "StackPanel" général contenant
        /// l'ensemble des items de l'onglet "Thèmes"
        /// </summary>
        public StackPanel StackPanelGlobal { get; set; }

        /// <summary>
        /// La classe représentant un signalement
        /// </summary>
        public Report VirtualReport { get; set; }

        /// <summary>
        /// Contient pour chaque "CheckBox thème" la liste des contrôles créés automatiquement.
        /// Il s'agit par ce biais de récupérer le (ou les) thème(s) et les valeurs d'attributs
        /// sélectionnés et entrés par l'utilisateur lors de la création d'un (ou plusieurs) signalement(s)
        /// </summary>
        public Dictionary<string, List<string>> ControlsCreate { get; set; }

        /// <summary>
        /// La liste des croquis sélectionnés par l'utilisateur
        /// qui entrent dans la création d'un signalement
        /// </summary>
        private List<ArcGisProEspaceCollaboratif.Core.Sketch> Sketches { get; set; }

        /// <summary>
        /// Le logger qui permet d'enregistrer des informations sur le processus
        /// </summary>
        private static readonly Logger riplogger = Logger.Instance;
        private static readonly ILog logger = LogManager.GetLogger(typeof(CreateReportViewModel));

        #endregion

        #region Constructors

        /// <summary>
        /// Initialisation du dialogue "Créer un nouveau signalement"
        /// </summary>
        public CreateReportViewModel(Context context, List<ArcGisProEspaceCollaboratif.Core.Sketch> sketches)
        {
            this.Context = context;
            this.ListPreferredThemes = Helper.Load_PreferredThemes();
            this.PreferredGroup = Helper.Load_PreferredGroup();
            this.SketchNumber = sketches.Count;
            this.Sketches = sketches;
            this.createReportView = new CreateReportView();
            this.InitializeCreateReportView();
        }
        #endregion

        #region Initialize Dialog

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
            // Onglet "Options"
            this.SetJoinSketchIsChecked();
            this.SetJoinDocumentLabel();
            this.SetCreateReportRadioButton();
            // Zone d'édition pour un commentaire
            this.SetCommentaireTextBox();
        }

        #region Initialize Entête
        /// <summary>
        /// Mise à jour de l'entête de la GroupBox
        /// avec le nom de l'utilisateur et son groupe actif
        /// </summary>
        /// <returns>Le nom de l'auteur et son profil</returns>
        private void SetUserProfileHeaderGroupBox()
        {
            string textGroupBox = "";
            if (string.IsNullOrEmpty(this.Context.Profil.Group.Name))
            {
                textGroupBox = string.Format("{0} (Profil par défaut)", this.Context.Profil.Author.Name);
            }
            else
            {
                textGroupBox = string.Format("{0} ({1})", this.Context.Profil.Author.Name, this.Context.Profil.Group.Name);
            }
            this.UserProfileHeaderGroupBox = textGroupBox;
        }

        #endregion

        #region Initialize TabControl "Thèmes"

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
            foreach (GeoGroup geogroupe in this.Context.Profil.Geogroupes)
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

        #endregion

        #region Initialize TabControl "Options"

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

        #region Intialize Commentaire

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
            this.CommentaryTextBox = message;
        }
        #endregion

        #endregion

        #region Update Theme 
        /// <summary>
        /// L'utilisateur a sélectionné un nom de groupe dans la ComboBox "Groupe"
        /// Il faut modifier le contenu du TabControl "Thèmes"
        /// </summary>
        private void OnPropertyChanged()
        {
            this.ControlsCreate = new Dictionary<string, List<string>>();

            if (this.StackPanelGlobal != null)
            {
                this.createReportView.GridGeneral.Children.Clear();
                this.StackPanelGlobal = null;
            }

            this.SetStackPanelGlobal();

            // L'utilisateur a selectionné un groupe autre que celui qui est actif
            Group newGroupActive = GetGroupSelectedItemComboBox();

            // Il faut récupérer les thèmes du nouveau groupe
            List<Theme> listThemesGroup = new List<Theme>();
            List<string> filteredThemes = new List<string>();
            foreach (GeoGroup geoGroupe in this.Context.Profil.Geogroupes)
            {
                if (newGroupActive.Name == geoGroupe.Name)
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
                    string check = "0";
                    if (this.ListPreferredThemes.Contains(thName))
                    {
                        check = "1";
                    }
                    StackPanel stackPanelExpander = SetStackPanel(thName);
                    this.OnRegisterName(stackPanelExpander.Name, stackPanelExpander);

                    CheckBox checkBox = SetCheckBox(thName, check);
                    stackPanelExpander.Children.Add(checkBox);
                    this.OnRegisterName(checkBox.Name, checkBox);

                    Expander expander = SetExpander();
                    List<string> controls = new List<string>();
                    expander.Content = DisplayTypeAttributes(thGroup, ref controls);
                    stackPanelExpander.Children.Add(expander);
                    this.StackPanelGlobal.Children.Add(stackPanelExpander);
                    this.ControlsCreate.Add(checkBox.Name, controls);
                }
            }
            ScrollViewer scrollViewer = SetScrollViewer();
            scrollViewer.Content = this.StackPanelGlobal;
            this.createReportView.GridGeneral.Children.Add(scrollViewer);
        }

        /// <summary>
        /// Création d'un control "Label" qui est le nom de l'attribut
        /// Certains attributs sont de la forme Info@Nom
        /// Il faut remplacer le @ par un _ sinon la création du nom de label est impossible
        /// </summary>
        /// <param name="content">Le nom de l'attribut</param>
        /// <param name="bold">A true si l'attribut est obligatoire</param>
        /// <returns>Le Label mis à jour</returns>
        private Label SetLabel(string content, bool bold)
        {
            string[] toReplaces = new string[] { "@" };
            foreach (string toReplace in toReplaces)
            {
                content = content.Replace(toReplace, "___");
            }

            Label label = new Label
            {
                Content = content,
                Name = this.RemoveSpecialCharacter(string.Format("Label_{0}", content))
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
        private TextBox SetTextBox(string text, string attributeName)
        {
            TextBox textBox = new TextBox
            {
                Text = text,
                Name = this.RemoveSpecialCharacter(string.Format("TextBox_{0}", attributeName))
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
                Content = content,
                Name = this.RemoveSpecialCharacter(string.Format("CheckBox_{0}", content))
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
        private ComboBox SetComboBox(Dictionary<string, string> values, string defaultValue, string attributeName)
        {
            List<string> lvalues = new List<string>();
            foreach (KeyValuePair<string, string> kvp in values)
            {
                lvalues.Add(kvp.Value);
            }

            ComboBox comboBox = new ComboBox()
            {
                ItemsSource = lvalues,
                SelectedItem = values[defaultValue],
                Name = this.RemoveSpecialCharacter(string.Format("ComboBox_{0}", attributeName))
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
        private DatePicker SetDatePicker(string value, string attributeName)
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
                SelectedDate = dateTime,
                Name = this.RemoveSpecialCharacter(string.Format("DatePicker_{0}", attributeName))
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
        private DatePickerTextBox SetDatePickerTextBox(string value, string attributeName)
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
                    Int32.Parse(tabDate[0]),
                    Int32.Parse(tabDate[1]),
                    Int32.Parse(tabDate[2]),
                    Int32.Parse(tabTime[0]),
                    Int32.Parse(tabTime[1]),
                    Int32.Parse(tabTime[2]));
            }
            //ArcGIS.Desktop.Internal.Framework.Controls.DateTimePicker dateTimePicker = new ArcGIS.Desktop.Internal.Framework.Controls.DateTimePicker();
            DatePickerTextBox datePickerTextBox = new DatePickerTextBox()
            {
                SelectedText = dateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                Name = this.RemoveSpecialCharacter(string.Format("DatePickerTextBox_{0}", attributeName))
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
        private Expander SetExpander()
        {
            Expander expander = new Expander();
            return expander;
        }

        /// <summary>
        /// Création d'un control "StackPanel"
        /// </summary>
        /// <returns>Le StackPanel mis à jour</returns>
        private StackPanel SetStackPanel(string themeName)
        {
            StackPanel stackPanel = new StackPanel()
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                Name = this.RemoveSpecialCharacter(string.Format("StackPanel_{0}", themeName))
            };
            return stackPanel;
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
                HorizontalAlignment = HorizontalAlignment.Left,
                Name = "StackPanelGlobal"
            };

            this.OnRegisterName(StackPanelGlobal.Name, StackPanelGlobal);
        }

        /// <summary>
        /// Dispose les attributs en créant un Control Windows par type
        /// sur une ligne horizontale les uns au-dessous des autres
        /// </summary>
        /// <param name="themeGroup">Le theme sélectionné par l'utilisateur contenant les attributs à afficher</param>
        /// <returns></returns>
        private StackPanel DisplayTypeAttributes(Theme themeGroup, ref List<string> controls)
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
                if (!string.IsNullOrEmpty(att.DefaultValue))
                {
                    value = att.DefaultValue;
                }

                // Types d'attributs
                if (att.Type == "checkbox")
                {
                    CheckBox checkBox = SetCheckBox(att.TagDisplay, value);
                    stackPanel.Children.Add(checkBox);
                    this.OnRegisterName(checkBox.Name, checkBox);
                    controls.Add(checkBox.Name);
                }
                else if (att.Type == "list")
                {
                    Label label = SetLabel(att.TagDisplay, bold);
                    stackPanel.Children.Add(label);
                    this.OnRegisterName(label.Name, label);
                    controls.Add(label.Name);

                    ComboBox comboBox = SetComboBox(att.Values, value, label.Name);
                    stackPanel.Children.Add(comboBox);
                    this.OnRegisterName(comboBox.Name, comboBox);
                    controls.Add(comboBox.Name);
                }
                else if (att.Type == "date")
                {
                    Label label = SetLabel(att.TagDisplay, bold);
                    stackPanel.Children.Add(label);
                    this.OnRegisterName(label.Name, label);
                    controls.Add(label.Name);

                    DatePicker datePicker = SetDatePicker(value, label.Name);
                    stackPanel.Children.Add(datePicker);
                    this.OnRegisterName(datePicker.Name, datePicker);
                    controls.Add(datePicker.Name);
                }
                else if (att.Type == "datetime")
                {
                    Label label = SetLabel(att.TagDisplay, bold);
                    stackPanel.Children.Add(label);
                    this.OnRegisterName(label.Name, label);
                    controls.Add(label.Name);

                    DatePickerTextBox datePickerTextBox = SetDatePickerTextBox(value, label.Name);
                    stackPanel.Children.Add(datePickerTextBox);
                    this.OnRegisterName(datePickerTextBox.Name, datePickerTextBox);
                    controls.Add(datePickerTextBox.Name);
                }
                else
                {
                    // textBox pour tous les autres types d'attributs
                    Label label = SetLabel(att.TagDisplay, bold);
                    stackPanel.Children.Add(label);
                    this.OnRegisterName(label.Name, label);
                    controls.Add(label.Name);

                    TextBox textBox = SetTextBox(value, label.Name);
                    stackPanel.Children.Add(textBox);
                    this.OnRegisterName(textBox.Name, textBox);
                    controls.Add(textBox.Name);
                }
            }
            return stackPanel;
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
        private string groupSelectedItem;

        /// <summary>
        /// Mise à jour de la sélection par défaut de la ComboBox "Groupe"
        /// </summary>
        public string GroupSelectedItemComboBox
        {
            get { return groupSelectedItem; }

            set
            {
                groupSelectedItem = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Mise à jour du texte du Label qui donne le nom
        /// du fichier et son répertoire qui sera joint au signalement
        /// </summary>
        public string JoinDocumentLabel { get; set; } = "";

        /// <summary>
        /// La CheckBox "Joindre un croquis" est cochée/décochée
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
        public string CommentaryTextBox { get; set; }
        #endregion

        #region Commands

        public ICommand SendNewReportButtonCmd { get { return new RelayCommand(OnSend, AlwaysTrue); } }

        /// <summary>
        /// L'utilisateur a cliqué sur le bouton "Envoyer"
        /// Le ou les signalements sont envoyés sur l'espace collaboratif
        /// </summary>
        private void OnSend()
        {
            string groupName = this.GroupSelectedItemComboBox;
            Helper.Save_PreferredGroup(groupName);
            
            List<Theme> themesSelected = GetSelectedThemes();
            Helper.Save_PreferredThemes(themesSelected);

            // Création d'un nouveau signalement temporaire.
            this.VirtualReport = new ArcGisProEspaceCollaboratif.Core.Report()
            {
                Commentary = CommentaryTextBox,
                Author = this.Context.Profil.Author,
                Group = this.GetGroupSelectedItemComboBox(),
                DateCreation = DateTime.Today,
                DateValidation = DateTime.Today,
                Status = EnumStatus.undefined
            };

            this.VirtualReport.AddTheme(themesSelected);
            this.VirtualReport.AddDocument(this.NameFileJoinToReport);

            // Option création d'un signalement unique
            if (this.CreateReportIsChecked)
            {
                CreateReport();
            }

            // Option création de plusieurs signalements
            if (this.CreateReportsIsChecked)
            {
                CreateReports();
            }
        }

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
                Filter = string.Format("{0}|{1}|{2}|{3}|{4}|{5}", Constantes.ALLFILE, Constantes.IMAGEFILE, Constantes.TRACKFILE, Constantes.TXTFILE, Constantes.SHEETFILE, Constantes.ZIPFILE)
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
                        logger.Error(string.Format("CreateReportViewModel.OnJoinDocument : {0}\n", message));
                        throw new Exception(message);
                    }

                    // Est-ce que la taille du fichier dépasse 16 Mo (MAX_TAILLE_UPLOAD_FILE)
                    long fileSize = new FileInfo(openFileDialog.FileName).Length;
                    if (fileSize > Constantes.MAX_TAILLE_UPLOAD_FILE)
                    {
                        message = string.Format("Le fichier {0} ne peut être envoyé car sa taille ({1} Ko) dépasse celle maximale autorisée ({2} Ko)", openFileDialog.FileName, fileSize / 1000, Constantes.MAX_TAILLE_UPLOAD_FILE / 1000);
                        logger.Error(string.Format("CreateReportViewModel.OnJoinDocument : {0}\n", message));
                        throw new Exception(message);
                    }

                    this.createReportView.JoinDocumentCheckBox.IsChecked = true;
                    this.createReportView.JoinDocumentLabel.Content = openFileDialog.FileName;
                    this.NameFileJoinToReport = openFileDialog.FileName;
                }
                catch
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, Constantes.ERROR);
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

        private bool AlwaysTrue() { return true; }

        #endregion

        #region Methods

        /// <summary>
        /// Affiche les informations de création d'un ou plusieurs signalements
        /// </summary>
        /// <param name="reports">La liste des identifiants des nouveaux signalements</param>
        private void ShowFeedbackInformation(List<ulong> reports)
        {
            string message = "";
            if (reports.Count == 1)
            {
                message = string.Format("Succès : création d'un nouveau signalement n°{0}", reports[0]);
            }
            else
            {
                message += string.Format("Succès de la création de {0} nouveaux signalements pour l'espace collaboratif.\n", reports.Count);
                message += string.Format("Les identifiants vont de {0} à {1}.", reports.FirstOrDefault(), reports.LastOrDefault());
            }
            var connectInfoViewModel = new FeedbackInformationViewModel();
            connectInfoViewModel.feedbackInformationView.DataContext = connectInfoViewModel;
            if (!string.IsNullOrEmpty(this.Context.Client.GetProfile().Logo))
            {
                connectInfoViewModel.Logo = string.Format("{0}{1}", this.Context.URLHost, this.Context.Client.GetProfile().Logo);
            }
            connectInfoViewModel.MessageFeedback = message;
            connectInfoViewModel.feedbackInformationView.ShowDialog();
        }

        /// <summary>
        /// Création d'un signalement unique
        /// </summary>
        private async void CreateReport()
        {
            await QueuedTask.Run(async () =>
            {
                // Calcul du positionnement du signalement par rapport à l'ensemble des croquis.
                List<Core.Point> points = new List<Core.Point>();
                foreach (Sketch sketch in this.Sketches)
                {
                    foreach (Core.Point point in sketch.Points)
                    {
                        points.Add(point);
                    }
                }
                this.VirtualReport.SetPosition(Helper.CalculatePositionReport(points));

                // Si option de joindre un croquis au nouveau signalement.
                if (this.JoinSketchIsChecked)
                {
                    this.VirtualReport.AddSketches(this.Sketches);
                }

                // Création du nouveau signalement
                Report newReport = this.Context.Client.CreateReport(this.VirtualReport);
                bool result = await this.Context.InsertReports(new List<Report> { newReport });

                List<ulong> listIdNouveauxSignalements = new List<ulong>
                {
                    newReport.Id
                };
                ShowFeedbackInformation(listIdNouveauxSignalements);
            });
        }

        /// <summary>
        /// Création de plusieurs signalements
        /// </summary>
        private async void CreateReports()
        {
            await QueuedTask.Run(async () =>
            {
                // Parcours des croquis un par un
                List<ulong> listIdNouveauxSignalements = new List<ulong>();
                List<Report> listNewReports = new List<Report>();

                foreach (ArcGisProEspaceCollaboratif.Core.Sketch sketch in this.Sketches)
                {
                    // Positionnement du signalement par rapport au croquis un par un.
                    this.VirtualReport.SetPosition(Helper.CalculatePositionReport(sketch.Points));
                    this.VirtualReport.ClearCroquis();

                    // Si option de joindre un croquis au nouveau signalement
                    if (this.JoinSketchIsChecked)
                    {
                        this.VirtualReport.AddSketch(sketch);
                    }

                    // Création du nouveau signalement
                    ArcGisProEspaceCollaboratif.Core.Report newReport = this.Context.Client.CreateReport(this.VirtualReport);
                    listNewReports.Add(newReport);

                    listIdNouveauxSignalements.Add(newReport.Id);
                }

                bool result = await this.Context.InsertReports(listNewReports);
                ShowFeedbackInformation(listIdNouveauxSignalements);
            });
        }

        /// <summary>
        /// Récupère le (ou les) thème(s) coché(s) par l'utilisateur
        /// ainsi que tous les attributs/valeurs liés au thème
        /// en fonction du dictionnaire des contrôles créés automatiquement
        /// </summary>
        /// <returns>La liste des thèmes/attributs/valeurs sélectionnés</returns>
        private List<Theme> GetSelectedThemes()
        {
            List<Theme> themesSelected = new List<Theme>();
            foreach (KeyValuePair<string, List<string>> kvp in this.ControlsCreate)
            {
                CheckBox cb = (CheckBox)this.createReportView.FindName(kvp.Key);
                if (cb.IsChecked == false)
                {
                    continue;
                }
                string themeName = cb.Content.ToString();
                Group group = new Group
                {
                    Name = cb.Content.ToString(),
                    Id = GetCorrespondenceIdNameTheme(themeName)
                };

                Theme tmpTheme = new Theme()
                {
                    Attributes = new List<ThemeAttributes>(),
                    Group = group
                };

                ThemeAttributes tmpThemeAttributes = null;
                foreach (string str in kvp.Value)
                {
                    object obj = this.createReportView.FindName(str);
                    Type type = obj.GetType();

                    if (type == typeof(CheckBox))
                    {
                        CheckBox checkBox = (CheckBox)this.createReportView.FindName(str);
                        string val = "1";
                        if (checkBox.IsChecked == false)
                        {
                            val = "0";
                        }

                        // Il faut chercher la correspondance entre le nom de l'attribut affiché
                        // et le nom de la colonne dans la table
                        string tagName = GetCorrespondenceAttributeColumn(checkBox.Content.ToString(), themeName);

                        tmpThemeAttributes = new ThemeAttributes
                        {
                            UserSelectedValue = val,
                            TagDisplay = checkBox.Content.ToString(),
                            TagName = tagName,
                            ThemeName = cb.Content.ToString()
                        };
                        tmpTheme.Attributes.Add(tmpThemeAttributes);
                        tmpThemeAttributes = null;
                    }

                    if (type == typeof(Label))
                    {
                        Label label = (Label)this.createReportView.FindName(str);

                        // Il faut chercher la correspondance entre le nom de l'attribut affiché
                        // et le nom de la colonne dans la table
                        string tagName = GetCorrespondenceAttributeColumn(label.Content.ToString(), themeName);

                        tmpThemeAttributes = new ThemeAttributes
                        {
                            TagDisplay = label.Content.ToString(),
                            TagName = tagName,
                            ThemeName = themeName
                        };
                    }

                    if (type == typeof(TextBox))
                    {
                        TextBox textBox = (TextBox)this.createReportView.FindName(str);
                        tmpThemeAttributes.UserSelectedValue = textBox.Text;
                        tmpTheme.Attributes.Add(tmpThemeAttributes);
                        tmpThemeAttributes = null;
                    }

                    if (type == typeof(ComboBox))
                    {
                        ComboBox comboBox = (ComboBox)this.createReportView.FindName(str);
                        string value = GetCorrespondenceValueAttributeColumn(comboBox.Text, tmpThemeAttributes.TagName, themeName);
                        tmpThemeAttributes.UserSelectedValue = value;
                        tmpTheme.Attributes.Add(tmpThemeAttributes);
                        tmpThemeAttributes = null;
                    }

                    if (type == typeof(DatePicker))
                    {
                        DatePicker datePicker = (DatePicker)this.createReportView.FindName(str);
                        string tmpDate = datePicker.Text;
                        // Si la date récupérée est de la forme jeudi 29 avril 2021
                        if (tmpDate.Length > 11)
                        {
                            string[] tmp = tmpDate.Split(' ');
                            string mois="";
                            switch (tmp[1])
                            {
                                case "janvier":
                                    mois = "01";
                                    break;
                                case "février":
                                    mois = "02";
                                    break;
                                case "mars":
                                    mois = "03";
                                    break;
                                case "avril":
                                    mois = "04";
                                    break;
                                case "mai":
                                    mois = "05";
                                    break;
                                case "juin":
                                    mois = "06";
                                    break;
                                case "juillet":
                                    mois = "07";
                                    break;
                                case "août":
                                    mois = "08";
                                    break;
                                case "septembre":
                                    mois = "09";
                                    break;
                                case "octobre":
                                    mois = "10";
                                    break;
                                case "novembre":
                                    mois = "11";
                                    break;
                                case "décembre":
                                    mois = "12";
                                    break;
                            }
                            // Si la transformation du mois a échoué
                            if (mois == "")
                            {
                                // On prend la date du jour
                                string nowDate = DateTime.Now.ToString("yyyy-MM-dd");
                                tmpThemeAttributes.UserSelectedValue = nowDate;
                            }
                            else
                            {
                                // Le mois a bien été codé en chiffre
                                tmpThemeAttributes.UserSelectedValue = string.Format("{0}-{1}-{2}", tmp[3], mois, tmp[1]);
                            }
                        }
                        // sinon la date récupérée est de la forme 29/04/2021
                        // il faut la transformer en 'yyyy-MM-dd'
                        else
                        {
                            string[] tmp = tmpDate.Split('/');
                            tmpThemeAttributes.UserSelectedValue = string.Format("{0}-{1}-{2}", tmp[2], tmp[1], tmp[0]);
                        }
                        tmpTheme.Attributes.Add(tmpThemeAttributes);
                        tmpThemeAttributes = null;
                    }

                    if (type == typeof(DatePickerTextBox))
                    {
                        DatePickerTextBox datePickerTextBox = (DatePickerTextBox)this.createReportView.FindName(str);
                        // La date récupérée est de la forme 2020-08-15 12:23:48 et correspond à la forme 'yyyy-MM-dd hh:mm:ss'
                        tmpThemeAttributes.UserSelectedValue = datePickerTextBox.Text;
                        tmpTheme.Attributes.Add(tmpThemeAttributes);
                        tmpThemeAttributes = null;
                    }
                }
                themesSelected.Add(tmpTheme);
            }
            return themesSelected;
        }

        /// <summary>
        /// Retrouve pour un thème donné son identifiant
        /// </summary>
        /// <param name="theme">Le nom du thème</param>
        /// <returns>L'identifiant du thème</returns>
        private string GetCorrespondenceIdNameTheme( string theme)
        {
            string id = "";
            foreach (GeoGroup geoGroup in this.Context.Profil.Geogroupes)
            {
                foreach (Theme th in geoGroup.Themes)
                {
                    if (th.Group.Name != theme)
                    {
                        continue;
                    }
                    return th.Group.Id;
                }
            }
            return id;
        }

        /// <summary>
        /// Retrouve pour un attribut d'un thème, la correspondance
        /// entre son nom d'affichage et son nom de colonne dans la table
        /// </summary>
        /// <param name="display">le nom affiché à l'utilisateur</param>
        /// <param name="theme">le thème de l'attribut</param>
        /// <returns>Le nom de la colonne dans la table</returns>
        private string GetCorrespondenceAttributeColumn(string display, string theme)
        {
            display = display.Replace("___", "@");
            string tmp = "";
            foreach (GeoGroup geoGroup in this.Context.Profil.Geogroupes)
            {
                foreach (Theme th in geoGroup.Themes)
                {
                    if (th.Group.Name != theme)
                    {
                        continue;
                    }

                    if (th.Attributes == null)
                    {
                        continue;
                    }

                    foreach (ThemeAttributes thAtt in th.Attributes)
                    {
                        if (thAtt.TagDisplay == display)
                        {
                            return thAtt.TagName;
                        }
                    }
                }
            }
            return tmp;
        }

        private string GetCorrespondenceValueAttributeColumn(string display, string attributeName, string theme)
        {
            string tmp = "";
            foreach (GeoGroup geoGroup in this.Context.Profil.Geogroupes)
            {
                foreach (Theme th in geoGroup.Themes)
                {
                    if (th.Group.Name != theme)
                    {
                        continue;
                    }

                    if (th.Attributes == null)
                    {
                        continue;
                    }

                    foreach (ThemeAttributes thAtt in th.Attributes)
                    {
                        if (thAtt.TagName != attributeName)
                        {
                            continue;
                        }

                        if (thAtt.Values == null)
                        {
                            continue;
                        }

                        foreach (KeyValuePair<string, string> kvp in thAtt.Values)
                        {
                            if (kvp.Value == display)
                            {
                                return kvp.Key;
                            }
                        }        
                    }
                }
            }
            return tmp;
        }

        /// <summary>
        /// Enregistre le nom du contrôle et son type
        /// qui peut-être : Label, TextBox, CheckBox, ComboBox, DatePicker, DatePickerTextBox
        /// </summary>
        /// <param name="nameScopedElement">Nom du contrôle</param>
        /// <param name="scopedElement">Type de contrôle</param>
        private void OnRegisterName(string nameScopedElement, object scopedElement)
        {
            object obj = this.createReportView.FindName(nameScopedElement);
            if (obj != null)
            {
                this.createReportView.UnregisterName(nameScopedElement);
            }
            this.createReportView.RegisterName(nameScopedElement, scopedElement);
        }

        /// <summary>
        /// Pour la création du nom de contrôle, il faut supprimer
        ///  - les caractères accentués
        ///  - les caractères spéciaux
        /// </summary>
        /// <param name="stIn">La chaine de caractères à modifier</param>
        /// <returns>La chaine modifiée</returns>
        private string RemoveSpecialCharacter(string stIn)
        {
            // Les caractères accentués en premier
            string stFormD = stIn.Normalize(System.Text.NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }
            string strNormalize = sb.ToString().Normalize(NormalizationForm.FormC);

            // Les caractères spéciaux ensuite
            List<string> tmp = new List<string>
            {
                "/",
                " ",
                "'",
                ",",
                "*",
                "-",
                "#",
                ":"
            };
            foreach (string item in tmp)
            {
                strNormalize = strNormalize.Replace(item, "_");
            }

            return strNormalize;
        }

        /// <summary>
        /// Récupère dans la ComboBox 'Groupe' le nom du groupe sélectionné par l'utilisateur
        /// et rempli les attributs Name et Id de la classe Group
        /// </summary>
        /// <returns>Retourne le groupe avec ses atrrbuts remplis</returns>
        private Group GetGroupSelectedItemComboBox()
        {
            Group group = null;

            // L'utilisateur a selectionné un groupe autre que celui qui est actif
            string groupName = this.GroupSelectedItemComboBox;

            if (groupName == Constantes.AUCUN)
            {
                this.StackPanelGlobal.Children.Add(SetLabel("Pas de groupe, pas d'attributs", true));
                this.createReportView.GridGeneral.Children.Add(this.StackPanelGlobal);
                return group;
            }

            foreach (GeoGroup geoGroup in this.Context.Profil.Geogroupes)
            {
                if (groupName == geoGroup.Name)
                {
                    group = new Group()
                    {
                        Name = geoGroup.Name,
                        Id = geoGroup.Id
                    };
                    break;
                }
            }
            return group;
        }

        #endregion
    }
}
