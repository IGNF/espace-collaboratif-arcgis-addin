namespace ArcGisProEspaceCollaboratif
{
    partial class FormCreerSignalement
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCreerSignalement));
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.groupImageProfil = new System.Windows.Forms.GroupBox();
            this.pictureProfil = new System.Windows.Forms.PictureBox();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabThemes = new System.Windows.Forms.TabPage();
            this.checkedListBoxThemes = new System.Windows.Forms.CheckedListBox();
            this.tabOptions = new System.Windows.Forms.TabPage();
            this.tableLayoutPanelOption = new System.Windows.Forms.TableLayoutPanel();
            this.checkBoxDocument = new System.Windows.Forms.CheckBox();
            this.radioButtonRemarqueMultiple = new System.Windows.Forms.RadioButton();
            this.radioButtonRemarqueUnique = new System.Windows.Forms.RadioButton();
            this.checkBoxCroquis = new System.Windows.Forms.CheckBox();
            this.listViewPJ = new System.Windows.Forms.ListView();
            this.buttonCreer = new System.Windows.Forms.Button();
            this.labelMessage = new System.Windows.Forms.Label();
            this.richTextBoxMessage = new System.Windows.Forms.RichTextBox();
            this.OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel.SuspendLayout();
            this.groupImageProfil.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureProfil)).BeginInit();
            this.tabControl.SuspendLayout();
            this.tabThemes.SuspendLayout();
            this.tabOptions.SuspendLayout();
            this.tableLayoutPanelOption.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.AutoSize = true;
            this.tableLayoutPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(184)))), ((int)(((byte)(184)))));
            this.tableLayoutPanel.ColumnCount = 4;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 133F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.Controls.Add(this.groupImageProfil, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.tabControl, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.buttonCreer, 2, 3);
            this.tableLayoutPanel.Controls.Add(this.labelMessage, 0, 2);
            this.tableLayoutPanel.Controls.Add(this.richTextBoxMessage, 0, 3);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel.MinimumSize = new System.Drawing.Size(396, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 5;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 123F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(929, 553);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // groupImageProfil
            // 
            this.groupImageProfil.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(this.groupImageProfil, 4);
            this.groupImageProfil.Controls.Add(this.pictureProfil);
            this.groupImageProfil.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupImageProfil.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupImageProfil.Location = new System.Drawing.Point(4, 4);
            this.groupImageProfil.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupImageProfil.Name = "groupImageProfil";
            this.groupImageProfil.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupImageProfil.Size = new System.Drawing.Size(921, 115);
            this.groupImageProfil.TabIndex = 0;
            this.groupImageProfil.TabStop = false;
            this.groupImageProfil.Text = "Profil:";
            // 
            // pictureProfil
            // 
            this.pictureProfil.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.pictureProfil.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureProfil.Location = new System.Drawing.Point(4, 19);
            this.pictureProfil.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureProfil.Name = "pictureProfil";
            this.pictureProfil.Size = new System.Drawing.Size(136, 92);
            this.pictureProfil.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureProfil.TabIndex = 0;
            this.pictureProfil.TabStop = false;
            // 
            // tabControl
            // 
            this.tableLayoutPanel.SetColumnSpan(this.tabControl, 4);
            this.tabControl.Controls.Add(this.tabThemes);
            this.tabControl.Controls.Add(this.tabOptions);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl.Location = new System.Drawing.Point(4, 127);
            this.tabControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabControl.MinimumSize = new System.Drawing.Size(481, 92);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(921, 172);
            this.tabControl.TabIndex = 12;
            // 
            // tabThemes
            // 
            this.tabThemes.AutoScroll = true;
            this.tabThemes.Controls.Add(this.checkedListBoxThemes);
            this.tabThemes.Location = new System.Drawing.Point(4, 26);
            this.tabThemes.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabThemes.Name = "tabThemes";
            this.tabThemes.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabThemes.Size = new System.Drawing.Size(913, 142);
            this.tabThemes.TabIndex = 0;
            this.tabThemes.Text = "Thèmes";
            this.tabThemes.UseVisualStyleBackColor = true;
            // 
            // checkedListBoxThemes
            // 
            this.checkedListBoxThemes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.checkedListBoxThemes.CheckOnClick = true;
            this.checkedListBoxThemes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkedListBoxThemes.FormattingEnabled = true;
            this.checkedListBoxThemes.Location = new System.Drawing.Point(4, 4);
            this.checkedListBoxThemes.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkedListBoxThemes.Name = "checkedListBoxThemes";
            this.checkedListBoxThemes.Size = new System.Drawing.Size(905, 134);
            this.checkedListBoxThemes.TabIndex = 0;
            this.checkedListBoxThemes.MouseUp += new System.Windows.Forms.MouseEventHandler(this.checkedListBoxThemes_MouseUp);
            // 
            // tabOptions
            // 
            this.tabOptions.Controls.Add(this.tableLayoutPanelOption);
            this.tabOptions.Location = new System.Drawing.Point(4, 26);
            this.tabOptions.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabOptions.Name = "tabOptions";
            this.tabOptions.Size = new System.Drawing.Size(789, 142);
            this.tabOptions.TabIndex = 2;
            this.tabOptions.Text = "Options";
            this.tabOptions.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanelOption
            // 
            this.tableLayoutPanelOption.ColumnCount = 2;
            this.tableLayoutPanelOption.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelOption.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelOption.Controls.Add(this.checkBoxDocument, 0, 1);
            this.tableLayoutPanelOption.Controls.Add(this.radioButtonRemarqueMultiple, 0, 3);
            this.tableLayoutPanelOption.Controls.Add(this.radioButtonRemarqueUnique, 0, 2);
            this.tableLayoutPanelOption.Controls.Add(this.checkBoxCroquis, 0, 0);
            this.tableLayoutPanelOption.Controls.Add(this.listViewPJ, 1, 1);
            this.tableLayoutPanelOption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelOption.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelOption.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanelOption.Name = "tableLayoutPanelOption";
            this.tableLayoutPanelOption.RowCount = 4;
            this.tableLayoutPanelOption.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelOption.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelOption.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelOption.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelOption.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelOption.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelOption.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelOption.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelOption.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelOption.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelOption.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelOption.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelOption.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelOption.Size = new System.Drawing.Size(789, 142);
            this.tableLayoutPanelOption.TabIndex = 5;
            // 
            // checkBoxDocument
            // 
            this.checkBoxDocument.AutoSize = true;
            this.checkBoxDocument.Location = new System.Drawing.Point(4, 33);
            this.checkBoxDocument.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBoxDocument.Name = "checkBoxDocument";
            this.checkBoxDocument.Size = new System.Drawing.Size(187, 21);
            this.checkBoxDocument.TabIndex = 3;
            this.checkBoxDocument.Text = "Joindre un document.";
            this.checkBoxDocument.UseVisualStyleBackColor = true;
            this.checkBoxDocument.CheckedChanged += new System.EventHandler(this.checkBoxDocument_CheckedChanged);
            // 
            // radioButtonRemarqueMultiple
            // 
            this.radioButtonRemarqueMultiple.AutoSize = true;
            this.radioButtonRemarqueMultiple.Location = new System.Drawing.Point(246, 92);
            this.radioButtonRemarqueMultiple.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.radioButtonRemarqueMultiple.Name = "radioButtonRemarqueMultiple";
            this.radioButtonRemarqueMultiple.Size = new System.Drawing.Size(271, 21);
            this.radioButtonRemarqueMultiple.TabIndex = 2;
            this.radioButtonRemarqueMultiple.Text = "Créer 1000 remarques distinctes.";
            this.radioButtonRemarqueMultiple.UseVisualStyleBackColor = true;
            // 
            // radioButtonRemarqueUnique
            // 
            this.radioButtonRemarqueUnique.AutoSize = true;
            this.radioButtonRemarqueUnique.Checked = true;
            this.radioButtonRemarqueUnique.Location = new System.Drawing.Point(4, 92);
            this.radioButtonRemarqueUnique.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.radioButtonRemarqueUnique.Name = "radioButtonRemarqueUnique";
            this.radioButtonRemarqueUnique.Size = new System.Drawing.Size(234, 21);
            this.radioButtonRemarqueUnique.TabIndex = 1;
            this.radioButtonRemarqueUnique.TabStop = true;
            this.radioButtonRemarqueUnique.Text = "Créer une remarque unique.";
            this.radioButtonRemarqueUnique.UseVisualStyleBackColor = true;
            // 
            // checkBoxCroquis
            // 
            this.checkBoxCroquis.AutoSize = true;
            this.checkBoxCroquis.Checked = true;
            this.checkBoxCroquis.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCroquis.Location = new System.Drawing.Point(4, 4);
            this.checkBoxCroquis.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBoxCroquis.Name = "checkBoxCroquis";
            this.checkBoxCroquis.Size = new System.Drawing.Size(170, 21);
            this.checkBoxCroquis.TabIndex = 0;
            this.checkBoxCroquis.Text = "Joindre un croquis.";
            this.checkBoxCroquis.UseVisualStyleBackColor = true;
            // 
            // listViewPJ
            // 
            this.listViewPJ.Alignment = System.Windows.Forms.ListViewAlignment.Default;
            this.listViewPJ.BackColor = System.Drawing.SystemColors.Window;
            this.listViewPJ.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tableLayoutPanelOption.SetColumnSpan(this.listViewPJ, 2);
            this.listViewPJ.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewPJ.Enabled = false;
            this.listViewPJ.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewPJ.HideSelection = false;
            this.listViewPJ.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.listViewPJ.Location = new System.Drawing.Point(27, 58);
            this.listViewPJ.Margin = new System.Windows.Forms.Padding(27, 0, 0, 0);
            this.listViewPJ.MultiSelect = false;
            this.listViewPJ.Name = "listViewPJ";
            this.listViewPJ.Scrollable = false;
            this.listViewPJ.Size = new System.Drawing.Size(762, 30);
            this.listViewPJ.TabIndex = 2;
            this.listViewPJ.UseCompatibleStateImageBehavior = false;
            this.listViewPJ.View = System.Windows.Forms.View.SmallIcon;
            this.listViewPJ.Visible = false;
            this.listViewPJ.DoubleClick += new System.EventHandler(this.listViewPJ_DoubleClick);
            // 
            // buttonCreer
            // 
            this.buttonCreer.AutoSize = true;
            this.buttonCreer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel.SetColumnSpan(this.buttonCreer, 4);
            this.buttonCreer.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonCreer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonCreer.Enabled = false;
            this.buttonCreer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCreer.Location = new System.Drawing.Point(4, 519);
            this.buttonCreer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonCreer.MinimumSize = new System.Drawing.Size(481, 0);
            this.buttonCreer.Name = "buttonCreer";
            this.buttonCreer.Size = new System.Drawing.Size(921, 30);
            this.buttonCreer.TabIndex = 11;
            this.buttonCreer.Text = "Envoyer";
            this.buttonCreer.UseVisualStyleBackColor = true;
            // 
            // labelMessage
            // 
            this.labelMessage.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(this.labelMessage, 4);
            this.labelMessage.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.labelMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.labelMessage.Location = new System.Drawing.Point(4, 316);
            this.labelMessage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(921, 25);
            this.labelMessage.TabIndex = 13;
            this.labelMessage.Text = "Message";
            // 
            // richTextBoxMessage
            // 
            this.tableLayoutPanel.SetColumnSpan(this.richTextBoxMessage, 4);
            this.richTextBoxMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxMessage.Location = new System.Drawing.Point(4, 345);
            this.richTextBoxMessage.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.richTextBoxMessage.MinimumSize = new System.Drawing.Size(480, 91);
            this.richTextBoxMessage.Name = "richTextBoxMessage";
            this.richTextBoxMessage.Size = new System.Drawing.Size(921, 166);
            this.richTextBoxMessage.TabIndex = 14;
            this.richTextBoxMessage.Text = "";
            this.richTextBoxMessage.TextChanged += new System.EventHandler(this.richTextBoxMessage_TextChanged);
            // 
            // OpenFileDialog
            // 
            this.OpenFileDialog.FileName = "OpenFileDialog";
            this.OpenFileDialog.ReadOnlyChecked = true;
            this.OpenFileDialog.Title = "Document à joindre pour la nouvelle remarque de l\'Espace collaboratif.";
            // 
            // toolTip
            // 
            this.toolTip.Popup += new System.Windows.Forms.PopupEventHandler(this.toolTip_Popup);
            // 
            // FormCreerEspaceCollaboratif
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(929, 553);
            this.Controls.Add(this.tableLayoutPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(575, 507);
            this.Name = "FormCreerEspaceCollaboratif";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Espace collaboratif : créer un nouveau signalement";
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.groupImageProfil.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureProfil)).EndInit();
            this.tabControl.ResumeLayout(false);
            this.tabThemes.ResumeLayout(false);
            this.tabOptions.ResumeLayout(false);
            this.tableLayoutPanelOption.ResumeLayout(false);
            this.tableLayoutPanelOption.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.GroupBox groupImageProfil;
        private System.Windows.Forms.PictureBox pictureProfil;
        public System.Windows.Forms.Button buttonCreer;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.RichTextBox richTextBoxMessage;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ListView listViewPJ;
        private System.Windows.Forms.TabPage tabOptions;
        private System.Windows.Forms.CheckBox checkBoxCroquis;
        private System.Windows.Forms.RadioButton radioButtonRemarqueMultiple;
        private System.Windows.Forms.RadioButton radioButtonRemarqueUnique;
        private System.Windows.Forms.TabPage tabThemes;
        private System.Windows.Forms.CheckedListBox checkedListBoxThemes;
        private System.Windows.Forms.CheckBox checkBoxDocument;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelOption;

    }
}