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
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.radioButtonOui = new System.Windows.Forms.RadioButton();
            this.radioButtonNon = new System.Windows.Forms.RadioButton();
            this.textBoxCleGeoportailUser = new System.Windows.Forms.TextBox();
            this.Enregistrer = new System.Windows.Forms.Button();
            this.Annuler = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(5, 25);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(251, 24);
            this.comboBox1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(252, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Dans quel groupe souhaitez-vous travailler ? ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(7, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(365, 60);
            this.label2.TabIndex = 2;
            this.label2.Text = "Vous pouvez utiliser une clé Géoportail pour afficher les fonds IGN\r\nde votre gro" +
    "upe directement dans QGIS.\r\n\r\nDisposez-vous de votre propre clé Géoportail ?";
            // 
            // radioButtonOui
            // 
            this.radioButtonOui.AutoSize = true;
            this.radioButtonOui.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonOui.Location = new System.Drawing.Point(9, 127);
            this.radioButtonOui.Name = "radioButtonOui";
            this.radioButtonOui.Size = new System.Drawing.Size(47, 19);
            this.radioButtonOui.TabIndex = 3;
            this.radioButtonOui.TabStop = true;
            this.radioButtonOui.Text = "Oui";
            this.radioButtonOui.UseVisualStyleBackColor = true;
            // 
            // radioButtonNon
            // 
            this.radioButtonNon.AutoSize = true;
            this.radioButtonNon.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonNon.Location = new System.Drawing.Point(10, 155);
            this.radioButtonNon.Name = "radioButtonNon";
            this.radioButtonNon.Size = new System.Drawing.Size(290, 19);
            this.radioButtonNon.TabIndex = 4;
            this.radioButtonNon.TabStop = true;
            this.radioButtonNon.Text = "Non - Utiliser la clé Géoportail de démonstration";
            this.radioButtonNon.UseVisualStyleBackColor = true;
            // 
            // textBoxCleGeoportailUser
            // 
            this.textBoxCleGeoportailUser.Location = new System.Drawing.Point(63, 127);
            this.textBoxCleGeoportailUser.Name = "textBoxCleGeoportailUser";
            this.textBoxCleGeoportailUser.Size = new System.Drawing.Size(309, 22);
            this.textBoxCleGeoportailUser.TabIndex = 5;
            // 
            // Enregistrer
            // 
            this.Enregistrer.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Enregistrer.Location = new System.Drawing.Point(202, 177);
            this.Enregistrer.Name = "Enregistrer";
            this.Enregistrer.Size = new System.Drawing.Size(77, 24);
            this.Enregistrer.TabIndex = 6;
            this.Enregistrer.Text = "Enregistrer";
            this.Enregistrer.UseVisualStyleBackColor = true;
            this.Enregistrer.Click += new System.EventHandler(this.Enregistrer_Click);
            // 
            // Annuler
            // 
            this.Annuler.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Annuler.Location = new System.Drawing.Point(296, 177);
            this.Annuler.Name = "Annuler";
            this.Annuler.Size = new System.Drawing.Size(77, 24);
            this.Annuler.TabIndex = 7;
            this.Annuler.Text = "Annuler";
            this.Annuler.UseVisualStyleBackColor = true;
            this.Annuler.Click += new System.EventHandler(this.Annuler_Click);
            // 
            // FormGroupChoice
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(381, 212);
            this.Controls.Add(this.Annuler);
            this.Controls.Add(this.Enregistrer);
            this.Controls.Add(this.textBoxCleGeoportailUser);
            this.Controls.Add(this.radioButtonNon);
            this.Controls.Add(this.radioButtonOui);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBox1);
            this.Name = "FormGroupChoice";
            this.Text = "Paramétrage";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton radioButtonOui;
        private System.Windows.Forms.RadioButton radioButtonNon;
        private System.Windows.Forms.TextBox textBoxCleGeoportailUser;
        private System.Windows.Forms.Button Enregistrer;
        private System.Windows.Forms.Button Annuler;
    }
}