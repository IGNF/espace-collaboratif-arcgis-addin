namespace ArcGisProEspaceCollaboratif
{
    partial class FormGroupChoice
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
            this.comboBoxGroupes = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.radioButtonOui = new System.Windows.Forms.RadioButton();
            this.radioButtonNon = new System.Windows.Forms.RadioButton();
            this.textBoxCleGeoportailUser = new System.Windows.Forms.TextBox();
            this.Enregistrer = new System.Windows.Forms.Button();
            this.Annuler = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboBoxGroupes
            // 
            this.comboBoxGroupes.AllowDrop = true;
            this.comboBoxGroupes.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.comboBoxGroupes.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxGroupes.FormattingEnabled = true;
            this.comboBoxGroupes.Location = new System.Drawing.Point(4, 20);
            this.comboBoxGroupes.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.comboBoxGroupes.MaxDropDownItems = 10;
            this.comboBoxGroupes.Name = "comboBoxGroupes";
            this.comboBoxGroupes.Size = new System.Drawing.Size(189, 21);
            this.comboBoxGroupes.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(4, 3);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(219, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Dans quel groupe souhaitez-vous travailler ? ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(5, 47);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(319, 52);
            this.label2.TabIndex = 2;
            this.label2.Text = "Vous pouvez utiliser une clé Géoportail pour afficher les fonds IGN\r\nde votre groupe " +
            "directement dans QGIS.\r\n\r\nDisposez-vous de votre propre clé Géoportail ?";
            // 
            // radioButtonOui
            // 
            this.radioButtonOui.AutoSize = true;
            this.radioButtonOui.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonOui.Location = new System.Drawing.Point(7, 103);
            this.radioButtonOui.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.radioButtonOui.Name = "radioButtonOui";
            this.radioButtonOui.Size = new System.Drawing.Size(41, 17);
            this.radioButtonOui.TabIndex = 3;
            this.radioButtonOui.TabStop = true;
            this.radioButtonOui.Text = "Oui";
            this.radioButtonOui.UseVisualStyleBackColor = true;
            // 
            // radioButtonNon
            // 
            this.radioButtonNon.AutoSize = true;
            this.radioButtonNon.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonNon.Location = new System.Drawing.Point(8, 126);
            this.radioButtonNon.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.radioButtonNon.Name = "radioButtonNon";
            this.radioButtonNon.Size = new System.Drawing.Size(248, 17);
            this.radioButtonNon.TabIndex = 4;
            this.radioButtonNon.TabStop = true;
            this.radioButtonNon.Text = "Non - Utiliser la clé Géoportail de démonstration";
            this.radioButtonNon.UseVisualStyleBackColor = true;
            // 
            // textBoxCleGeoportailUser
            // 
            this.textBoxCleGeoportailUser.Location = new System.Drawing.Point(51, 103);
            this.textBoxCleGeoportailUser.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBoxCleGeoportailUser.Name = "textBoxCleGeoportailUser";
            this.textBoxCleGeoportailUser.Size = new System.Drawing.Size(269, 20);
            this.textBoxCleGeoportailUser.TabIndex = 5;
            // 
            // Enregistrer
            // 
            this.Enregistrer.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Enregistrer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Enregistrer.Location = new System.Drawing.Point(166, 151);
            this.Enregistrer.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Enregistrer.Name = "Enregistrer";
            this.Enregistrer.Size = new System.Drawing.Size(75, 25);
            this.Enregistrer.TabIndex = 6;
            this.Enregistrer.Text = "Enregistrer";
            this.Enregistrer.UseVisualStyleBackColor = true;
            this.Enregistrer.Click += new System.EventHandler(this.Enregistrer_Click);
            // 
            // Annuler
            // 
            this.Annuler.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Annuler.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Annuler.Location = new System.Drawing.Point(245, 151);
            this.Annuler.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Annuler.Name = "Annuler";
            this.Annuler.Size = new System.Drawing.Size(75, 25);
            this.Annuler.TabIndex = 7;
            this.Annuler.Text = "Annuler";
            this.Annuler.UseVisualStyleBackColor = true;
            this.Annuler.Click += new System.EventHandler(this.Annuler_Click);
            // 
            // FormGroupChoice
            // 
            this.AcceptButton = this.Enregistrer;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Annuler;
            this.ClientSize = new System.Drawing.Size(342, 187);
            this.Controls.Add(this.Annuler);
            this.Controls.Add(this.Enregistrer);
            this.Controls.Add(this.textBoxCleGeoportailUser);
            this.Controls.Add(this.radioButtonNon);
            this.Controls.Add(this.radioButtonOui);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxGroupes);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "FormGroupChoice";
            this.Text = "Paramétrage";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxGroupes;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton radioButtonOui;
        private System.Windows.Forms.RadioButton radioButtonNon;
        private System.Windows.Forms.TextBox textBoxCleGeoportailUser;
        private System.Windows.Forms.Button Enregistrer;
        private System.Windows.Forms.Button Annuler;
    }
}