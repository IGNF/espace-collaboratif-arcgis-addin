using ArcGisProEspaceCollaboratif.Core;
using ArcGisProEspaceCollaboratif.Utils;
using ArcGisProEspaceCollaboratif.Views;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

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
        /// 
        /// </summary>
        public Contexte Context { get; set; }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        public string UserProfileHeaderGroupBox { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<string> GroupItemsSourceComboBox { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string GroupSelectedItemComboBox { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string JoinDocumentLabel { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        public bool JoinSketchIsChecked { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool CreateReportIsChecked { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        public string CreateReportsContent { get; set; } = "Créer x signalements";

        /// <summary>
        /// 
        /// </summary>
        public bool CreateReportsIsChecked { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CommentaireTextBox { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<ItemTheme> lvThemesItemsSource { get; set; }

        public List<ItemTest> ItemsSourceTestListView { get; set; }
        #endregion

        #region Class

        public class ItemTheme
        {
            /// <summary>
            /// 
            /// </summary>
            public string Attribute { get; set; } = "";

            /// <summary>
            /// 
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string ThemeName { get; set; }
        }

        public class ItemTest
        {
            /// <summary>
            /// 
            /// </summary>
            public string Attribute { get; set; } = "";

            /// <summary>
            /// 
            /// </summary>
            //public string Value { get; set; }

            public DataTemplate CellTemplate { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string ThemeName { get; set; }
        }
        #endregion

        #region Commands
        public ICommand JoinDocumentCmd { get { return new RelayCommand(OnJoinDocument, AlwaysTrue); } }

        /// <summary>
        /// 
        /// </summary>
        private void OnJoinDocument()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                InitialDirectory = this.Context.DirectoryWorking,
                Filter = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", Constantes.ALLFILE, Constantes.IMAGEFILE, Constantes.TRACKFILE, Constantes.TXTFILE, Constantes.SHEETFILE, Constantes.DBFILE, Constantes.SIGFILE)
            };
            bool? dialogResult = openFileDialog.ShowDialog();
            if (dialogResult == true)
            {
                this.createReportView.JoinDocumentCheckBox.IsChecked = true;
                this.createReportView.JoinDocumentLabel.Content = openFileDialog.FileName;
                this.NameFileJoinToReport = openFileDialog.FileName;
            }
            else
            {
                this.createReportView.JoinDocumentLabel.Content = Constantes.NOFILE;
                this.createReportView.JoinDocumentCheckBox.IsChecked = false;
            }
        }

        public ICommand SendButtonCmd { get { return new RelayCommand(OnSend, AlwaysTrue); } }

        /// <summary>
        /// Import des couches sélectionnées par l'utilisateur dans ArcGIS
        /// </summary>
        private void OnSend()
        {

        }

        private bool AlwaysTrue() { return true; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        private void InitializeCreateReportView()
        {
            
            this.SetUserProfileHeaderGroupBox();
            this.SetGroupItemsSourceComboBox();
            this.SetGroupSelectedItemComboBox();
            this.SetJoinSketchIsChecked();
            this.SetJoinDocumentLabel();
            this.SetCreateReportRadioButton();
            this.SetCommentaireTextBox();
            this.SetItemsSourceTestListView();
            this.SetThemes();
        }

        /// <summary>
        /// Affiche les thèmes dans le formulaire en fonction du groupe de l'utilisateur
        /// </summary>
        private void SetThemes()
        {
            if (this.Context.Profil.Themes.Count == 0)
            {
                // Pas de thèmes à afficher, on sort de la fonction
                return;
            }

            List<ItemTheme> items = new List<ItemTheme>();
            foreach (string thName in this.Context.Profil.FilteredThemes)
            {
                Theme th = null;
                bool foundTheme = false;
                foreach (Theme tmpth in this.Context.Profil.Themes)
                {
                    if (thName == tmpth.Group.Name)
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

                foreach (ThemeAttributes att in th.Attributes)
                {
                    string value = "";
                    if (!string.IsNullOrEmpty(att.Value))
                    {
                        if (!string.IsNullOrEmpty(att.DefaultValue))
                        {
                            value = att.DefaultValue;
                        }
                            
                    }
                    items.Add(new ItemTheme() { Attribute = att.Name, Value = value, ThemeName = thName });
                }
            }

            //this.createReportView.lvThemes.ItemsSource = items;
            this.lvThemesItemsSource = items;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.lvThemesItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("ThemeName");
            view.GroupDescriptions.Add(groupDescription);
        }

        private void SetItemsSourceTestListView()
        {
            if (this.Context.Profil.Themes.Count == 0)
            {
                // Pas de thèmes à afficher, on sort de la fonction
                return;
            }

            // Filtrage des thèmes utilisateur en fonction du contenu des thèmes de son profil
            List<ItemTest> items = new List<ItemTest>();
            foreach (string thName in this.Context.Profil.FilteredThemes)
            {
                Theme th = null;
                bool foundTheme = false;
                foreach (Theme tmpth in this.Context.Profil.Themes)
                {
                    if (thName == tmpth.Group.Name)
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

                foreach (ThemeAttributes att in th.Attributes)
                {
                    string value = "";
                    if (!string.IsNullOrEmpty(att.Value))
                    {
                        value = att.Value;
                    }

                    FrameworkElementFactory frameworkElementFactory = new FrameworkElementFactory(typeof(ComboBox));
                    //frameworkElementFactory.SetBinding(TextBox.PaddingProperty, new Binding("Value"));
                    DataTemplate dataTemplate = new DataTemplate
                    {
                        VisualTree = frameworkElementFactory
                    };
                    /*{
                        DataType = typeof(TextBox),
                        VisualTree = frameworkElementFactory
                    };*/

                    items.Add(new ItemTest() { Attribute = att.Name, CellTemplate = dataTemplate, ThemeName = thName });

                }
            }
            ItemsSourceTestListView = items;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.ItemsSourceTestListView);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("ThemeName");
            view.GroupDescriptions.Add(groupDescription);
        }

        /// <summary>
        /// 
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

        /// <summary>
        /// 
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
        /// 
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

        private void SetJoinSketchIsChecked()
        {
            this.JoinSketchIsChecked = false;
            if (this.SketchNumber >= 1)
            {
                this.JoinSketchIsChecked = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetJoinDocumentLabel()
        {
            this.JoinDocumentLabel = Constantes.NOFILE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        private void DisplayAttributsInTreeView(Theme th, int index)
        {
            foreach (ThemeAttributes att in th.Attributes)
            {
                //this.treeViewThemesAttributs.Nodes[index].Nodes.Add(att.Nom);
                if (att.Required)
                {
                    //this.treeViewThemesAttributs.Nodes[index].NodeFont = new Font(Font, FontStyle.Bold);
                }

                if (att.Type == "checkbox")
                {

                }
                else if (att.Type == "date")
                {

                }
                else if (att.Type == "datetime")
                {
                    //this.treeViewThemesAttributs.Nodes[index].Nodes.Add(att.Nom);
                }
                else if (att.Type == "list")
                {
                    /*ComboBoxTreeNode cb = new ComboBoxTreeNode();
                    foreach (string val in att.Valeurs)
                    {
                        cb.ComboBox.Items.Add(val);
                    }

                    TreeNode listNode = new TreeNode(att.Nom);
                    listNode.Nodes.Add(cb);
                    this.treeViewThemesAttributs.Nodes[index].Nodes.Add(listNode);*/
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// 
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
