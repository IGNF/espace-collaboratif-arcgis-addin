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
            this.tableLayoutPanelMyGateway = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanelLayersGeoportail = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanelLayersGeoportailBis = new System.Windows.Forms.TableLayoutPanel();
            this.labelMonGuichet = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonEnregistrer = new System.Windows.Forms.Button();
            this.buttonAnnuler = new System.Windows.Forms.Button();
            this.labelGroupeActif = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tableLayoutPanelMyGateway
            // 
            this.tableLayoutPanelMyGateway.ColumnCount = 1;
            this.tableLayoutPanelMyGateway.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelMyGateway.Location = new System.Drawing.Point(7, 55);
            this.tableLayoutPanelMyGateway.Name = "tableLayoutPanelMyGateway";
            this.tableLayoutPanelMyGateway.RowCount = 1;
            this.tableLayoutPanelMyGateway.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelMyGateway.Size = new System.Drawing.Size(720, 191);
            this.tableLayoutPanelMyGateway.TabIndex = 0;
            // 
            // tableLayoutPanelLayersGeoportail
            // 
            this.tableLayoutPanelLayersGeoportail.ColumnCount = 1;
            this.tableLayoutPanelLayersGeoportail.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelLayersGeoportail.Location = new System.Drawing.Point(7, 281);
            this.tableLayoutPanelLayersGeoportail.Name = "tableLayoutPanelLayersGeoportail";
            this.tableLayoutPanelLayersGeoportail.RowCount = 1;
            this.tableLayoutPanelLayersGeoportail.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelLayersGeoportail.Size = new System.Drawing.Size(720, 181);
            this.tableLayoutPanelLayersGeoportail.TabIndex = 1;
            // 
            // tableLayoutPanelLayersGeoportailBis
            // 
            this.tableLayoutPanelLayersGeoportailBis.ColumnCount = 1;
            this.tableLayoutPanelLayersGeoportailBis.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelLayersGeoportailBis.Location = new System.Drawing.Point(7, 526);
            this.tableLayoutPanelLayersGeoportailBis.Name = "tableLayoutPanelLayersGeoportailBis";
            this.tableLayoutPanelLayersGeoportailBis.RowCount = 1;
            this.tableLayoutPanelLayersGeoportailBis.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelLayersGeoportailBis.Size = new System.Drawing.Size(720, 121);
            this.tableLayoutPanelLayersGeoportailBis.TabIndex = 2;
            // 
            // labelMonGuichet
            // 
            this.labelMonGuichet.AutoSize = true;
            this.labelMonGuichet.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMonGuichet.Location = new System.Drawing.Point(5, 33);
            this.labelMonGuichet.Name = "labelMonGuichet";
            this.labelMonGuichet.Size = new System.Drawing.Size(100, 18);
            this.labelMonGuichet.TabIndex = 3;
            this.labelMonGuichet.Text = "Mon guichet";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(5, 257);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(139, 18);
            this.label1.TabIndex = 4;
            this.label1.Text = "Fonds Géoportail";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(5, 469);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(704, 51);
            this.label2.TabIndex = 5;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // buttonEnregistrer
            // 
            this.buttonEnregistrer.Location = new System.Drawing.Point(515, 655);
            this.buttonEnregistrer.Name = "buttonEnregistrer";
            this.buttonEnregistrer.Size = new System.Drawing.Size(100, 28);
            this.buttonEnregistrer.TabIndex = 6;
            this.buttonEnregistrer.Text = "Enregistrer";
            this.buttonEnregistrer.UseVisualStyleBackColor = true;
            this.buttonEnregistrer.Click += new System.EventHandler(this.ButtonEnregistrer_Click);
            // 
            // buttonAnnuler
            // 
            this.buttonAnnuler.Location = new System.Drawing.Point(625, 655);
            this.buttonAnnuler.Name = "buttonAnnuler";
            this.buttonAnnuler.Size = new System.Drawing.Size(100, 28);
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
            this.labelGroupeActif.Location = new System.Drawing.Point(5, 4);
            this.labelGroupeActif.Name = "labelGroupeActif";
            this.labelGroupeActif.Size = new System.Drawing.Size(36, 20);
            this.labelGroupeActif.TabIndex = 8;
            this.labelGroupeActif.Text = "xxx";
            // 
            // FormLoadGateway
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(733, 690);
            this.Controls.Add(this.labelGroupeActif);
            this.Controls.Add(this.buttonAnnuler);
            this.Controls.Add(this.buttonEnregistrer);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelMonGuichet);
            this.Controls.Add(this.tableLayoutPanelLayersGeoportailBis);
            this.Controls.Add(this.tableLayoutPanelLayersGeoportail);
            this.Controls.Add(this.tableLayoutPanelMyGateway);
            this.Name = "FormLoadGateway";
            this.Text = "Charger les couches de mon groupe";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMyGateway;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelLayersGeoportail;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelLayersGeoportailBis;
        private System.Windows.Forms.Label labelMonGuichet;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonEnregistrer;
        private System.Windows.Forms.Button buttonAnnuler;
        private System.Windows.Forms.Label labelGroupeActif;
    }
}