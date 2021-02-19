namespace ArcGisProEspaceCollaboratif
{
    partial class FormLoadGateway
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLoadGateway));
            this.labelMonGuichet = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonEnregistrer = new System.Windows.Forms.Button();
            this.buttonAnnuler = new System.Windows.Forms.Button();
            this.labelGroupeActif = new System.Windows.Forms.Label();
            this.listViewMyGateway = new System.Windows.Forms.ListView();
            this.listViewGeoportail = new System.Windows.Forms.ListView();
            this.listViewGeoportailBis = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // labelMonGuichet
            // 
            this.labelMonGuichet.AutoSize = true;
            this.labelMonGuichet.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMonGuichet.Location = new System.Drawing.Point(9, 33);
            this.labelMonGuichet.Name = "labelMonGuichet";
            this.labelMonGuichet.Size = new System.Drawing.Size(100, 18);
            this.labelMonGuichet.TabIndex = 3;
            this.labelMonGuichet.Text = "Mon guichet";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(9, 255);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(139, 18);
            this.label1.TabIndex = 4;
            this.label1.Text = "Fonds Géoportail";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(9, 472);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(564, 68);
            this.label2.TabIndex = 5;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // buttonEnregistrer
            // 
            this.buttonEnregistrer.Location = new System.Drawing.Point(344, 674);
            this.buttonEnregistrer.Name = "buttonEnregistrer";
            this.buttonEnregistrer.Size = new System.Drawing.Size(112, 28);
            this.buttonEnregistrer.TabIndex = 6;
            this.buttonEnregistrer.Text = "Enregistrer";
            this.buttonEnregistrer.UseVisualStyleBackColor = true;
            this.buttonEnregistrer.Click += new System.EventHandler(this.ButtonEnregistrer_Click);
            // 
            // buttonAnnuler
            // 
            this.buttonAnnuler.Location = new System.Drawing.Point(468, 674);
            this.buttonAnnuler.Name = "buttonAnnuler";
            this.buttonAnnuler.Size = new System.Drawing.Size(112, 28);
            this.buttonAnnuler.TabIndex = 7;
            this.buttonAnnuler.Text = "Annuler";
            this.buttonAnnuler.UseVisualStyleBackColor = true;
            this.buttonAnnuler.Click += new System.EventHandler(this.ButtonAnnuler_Click);
            // 
            // labelGroupeActif
            // 
            this.labelGroupeActif.AutoSize = true;
            this.labelGroupeActif.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelGroupeActif.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.labelGroupeActif.Location = new System.Drawing.Point(9, 4);
            this.labelGroupeActif.Name = "labelGroupeActif";
            this.labelGroupeActif.Size = new System.Drawing.Size(36, 20);
            this.labelGroupeActif.TabIndex = 8;
            this.labelGroupeActif.Text = "xxx";
            // 
            // listViewMyGateway
            // 
            this.listViewMyGateway.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewMyGateway.FullRowSelect = true;
            this.listViewMyGateway.GridLines = true;
            this.listViewMyGateway.HideSelection = false;
            this.listViewMyGateway.Location = new System.Drawing.Point(9, 56);
            this.listViewMyGateway.Name = "listViewMyGateway";
            this.listViewMyGateway.Size = new System.Drawing.Size(570, 188);
            this.listViewMyGateway.TabIndex = 9;
            this.listViewMyGateway.UseCompatibleStateImageBehavior = false;
            this.listViewMyGateway.View = System.Windows.Forms.View.Details;
            // 
            // listViewGeoportail
            // 
            this.listViewGeoportail.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewGeoportail.FullRowSelect = true;
            this.listViewGeoportail.GridLines = true;
            this.listViewGeoportail.HideSelection = false;
            this.listViewGeoportail.Location = new System.Drawing.Point(9, 276);
            this.listViewGeoportail.Name = "listViewGeoportail";
            this.listViewGeoportail.Size = new System.Drawing.Size(570, 187);
            this.listViewGeoportail.TabIndex = 10;
            this.listViewGeoportail.UseCompatibleStateImageBehavior = false;
            this.listViewGeoportail.View = System.Windows.Forms.View.Details;
            // 
            // listViewGeoportailBis
            // 
            this.listViewGeoportailBis.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewGeoportailBis.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.listViewGeoportailBis.FullRowSelect = true;
            this.listViewGeoportailBis.GridLines = true;
            this.listViewGeoportailBis.HideSelection = false;
            this.listViewGeoportailBis.Location = new System.Drawing.Point(9, 547);
            this.listViewGeoportailBis.Name = "listViewGeoportailBis";
            this.listViewGeoportailBis.Size = new System.Drawing.Size(570, 120);
            this.listViewGeoportailBis.TabIndex = 11;
            this.listViewGeoportailBis.UseCompatibleStateImageBehavior = false;
            this.listViewGeoportailBis.View = System.Windows.Forms.View.Details;
            // 
            // FormLoadGateway
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(590, 712);
            this.Controls.Add(this.listViewGeoportailBis);
            this.Controls.Add(this.listViewGeoportail);
            this.Controls.Add(this.listViewMyGateway);
            this.Controls.Add(this.labelGroupeActif);
            this.Controls.Add(this.buttonAnnuler);
            this.Controls.Add(this.buttonEnregistrer);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelMonGuichet);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "FormLoadGateway";
            this.Text = "Charger les couches de mon groupe";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelMonGuichet;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonEnregistrer;
        private System.Windows.Forms.Button buttonAnnuler;
        private System.Windows.Forms.Label labelGroupeActif;
        private System.Windows.Forms.ListView listViewMyGateway;
        private System.Windows.Forms.ListView listViewGeoportail;
        private System.Windows.Forms.ListView listViewGeoportailBis;
    }
}