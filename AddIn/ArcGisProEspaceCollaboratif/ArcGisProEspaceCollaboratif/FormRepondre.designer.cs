namespace ArcGisProEspaceCollaboratif
{
    partial class FormRepondre
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRepondre));
            this.labelstatut = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.webBrowserReponseAncienne = new System.Windows.Forms.WebBrowser();
            this.label2 = new System.Windows.Forms.Label();
            this.webBrowserMessage = new System.Windows.Forms.WebBrowser();
            this.labelIdSignalement = new System.Windows.Forms.Label();
            this.comboBoxStatut = new System.Windows.Forms.ComboBox();
            this.richTextBoxReponse = new System.Windows.Forms.RichTextBox();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonRepondre = new System.Windows.Forms.Button();
            this.txtRespTitre = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelstatut
            // 
            this.labelstatut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelstatut.AutoSize = true;
            this.labelstatut.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelstatut.Location = new System.Drawing.Point(4, 11);
            this.labelstatut.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelstatut.Name = "labelstatut";
            this.labelstatut.Size = new System.Drawing.Size(76, 25);
            this.labelstatut.TabIndex = 0;
            this.labelstatut.Text = "Statut:";
            this.labelstatut.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(this.label4, 2);
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(4, 349);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(187, 25);
            this.label4.TabIndex = 8;
            this.label4.Text = "Nouvelle réponse:";
            // 
            // webBrowserReponseAncienne
            // 
            this.webBrowserReponseAncienne.CausesValidation = false;
            this.tableLayoutPanel.SetColumnSpan(this.webBrowserReponseAncienne, 2);
            this.webBrowserReponseAncienne.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowserReponseAncienne.Location = new System.Drawing.Point(4, 198);
            this.webBrowserReponseAncienne.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowserReponseAncienne.MinimumSize = new System.Drawing.Size(388, 92);
            this.webBrowserReponseAncienne.Name = "webBrowserReponseAncienne";
            this.webBrowserReponseAncienne.Size = new System.Drawing.Size(589, 136);
            this.webBrowserReponseAncienne.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(this.label2, 2);
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(4, 169);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(261, 25);
            this.label2.TabIndex = 6;
            this.label2.Text = "Réponse(s) antérieure(s):";
            // 
            // webBrowserMessage
            // 
            this.webBrowserMessage.CausesValidation = false;
            this.tableLayoutPanel.SetColumnSpan(this.webBrowserMessage, 2);
            this.webBrowserMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowserMessage.Location = new System.Drawing.Point(4, 76);
            this.webBrowserMessage.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowserMessage.MinimumSize = new System.Drawing.Size(388, 92);
            this.webBrowserMessage.Name = "webBrowserMessage";
            this.webBrowserMessage.Size = new System.Drawing.Size(589, 92);
            this.webBrowserMessage.TabIndex = 5;
            // 
            // labelIdSignalement
            // 
            this.labelIdSignalement.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelIdSignalement.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(this.labelIdSignalement, 2);
            this.labelIdSignalement.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelIdSignalement.Location = new System.Drawing.Point(4, 47);
            this.labelIdSignalement.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelIdSignalement.Name = "labelIdSignalement";
            this.labelIdSignalement.Size = new System.Drawing.Size(333, 25);
            this.labelIdSignalement.TabIndex = 3;
            this.labelIdSignalement.Text = "Message du signalement n° 0000";
            // 
            // comboBoxStatut
            // 
            this.comboBoxStatut.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxStatut.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxStatut.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxStatut.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxStatut.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxStatut.FormattingEnabled = true;
            this.comboBoxStatut.Location = new System.Drawing.Point(100, 4);
            this.comboBoxStatut.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxStatut.Name = "comboBoxStatut";
            this.comboBoxStatut.Size = new System.Drawing.Size(493, 28);
            this.comboBoxStatut.TabIndex = 1;
            // 
            // richTextBoxReponse
            // 
            this.richTextBoxReponse.AcceptsTab = true;
            this.richTextBoxReponse.BackColor = System.Drawing.SystemColors.Window;
            this.tableLayoutPanel.SetColumnSpan(this.richTextBoxReponse, 2);
            this.richTextBoxReponse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxReponse.EnableAutoDragDrop = true;
            this.richTextBoxReponse.Location = new System.Drawing.Point(4, 414);
            this.richTextBoxReponse.Margin = new System.Windows.Forms.Padding(4);
            this.richTextBoxReponse.MinimumSize = new System.Drawing.Size(387, 91);
            this.richTextBoxReponse.Name = "richTextBoxReponse";
            this.richTextBoxReponse.ShowSelectionMargin = true;
            this.richTextBoxReponse.Size = new System.Drawing.Size(589, 122);
            this.richTextBoxReponse.TabIndex = 2;
            this.richTextBoxReponse.Text = "";
            this.richTextBoxReponse.TextChanged += new System.EventHandler(this.RichTextBoxReponse_TextChanged);
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.AutoSize = true;
            this.tableLayoutPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(184)))), ((int)(((byte)(184)))));
            this.tableLayoutPanel.ColumnCount = 2;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Controls.Add(this.label3, 0, 6);
            this.tableLayoutPanel.Controls.Add(this.richTextBoxReponse, 0, 7);
            this.tableLayoutPanel.Controls.Add(this.comboBoxStatut, 1, 0);
            this.tableLayoutPanel.Controls.Add(this.labelIdSignalement, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.webBrowserMessage, 0, 2);
            this.tableLayoutPanel.Controls.Add(this.label2, 0, 3);
            this.tableLayoutPanel.Controls.Add(this.webBrowserReponseAncienne, 0, 4);
            this.tableLayoutPanel.Controls.Add(this.label4, 0, 5);
            this.tableLayoutPanel.Controls.Add(this.labelstatut, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.buttonRepondre, 1, 7);
            this.tableLayoutPanel.Controls.Add(this.txtRespTitre, 1, 6);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 5;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 24F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 36F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(597, 578);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(4, 390);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 20);
            this.label3.TabIndex = 11;
            this.label3.Text = "Titre";
            // 
            // buttonRepondre
            // 
            this.buttonRepondre.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRepondre.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(this.buttonRepondre, 2);
            this.buttonRepondre.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonRepondre.Enabled = false;
            this.buttonRepondre.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRepondre.Location = new System.Drawing.Point(4, 544);
            this.buttonRepondre.Margin = new System.Windows.Forms.Padding(4);
            this.buttonRepondre.Name = "buttonRepondre";
            this.buttonRepondre.Size = new System.Drawing.Size(589, 30);
            this.buttonRepondre.TabIndex = 3;
            this.buttonRepondre.Text = "Envoyer";
            this.buttonRepondre.UseVisualStyleBackColor = true;
            // 
            // txtRespTitre
            // 
            this.txtRespTitre.Location = new System.Drawing.Point(100, 378);
            this.txtRespTitre.Margin = new System.Windows.Forms.Padding(4);
            this.txtRespTitre.Name = "txtRespTitre";
            this.txtRespTitre.Size = new System.Drawing.Size(492, 22);
            this.txtRespTitre.TabIndex = 9;
            // 
            // FormRepondre
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(597, 578);
            this.Controls.Add(this.tableLayoutPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(411, 507);
            this.Name = "FormRepondre";
            this.Opacity = 0.95D;
            this.Text = "Espace collaboratif : ajout d\'une nouvelle réponse";
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelstatut;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.RichTextBox richTextBoxReponse;
        public System.Windows.Forms.ComboBox comboBoxStatut;
        private System.Windows.Forms.Label labelIdSignalement;
        private System.Windows.Forms.WebBrowser webBrowserMessage;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.WebBrowser webBrowserReponseAncienne;
        private System.Windows.Forms.TextBox txtRespTitre;
        public System.Windows.Forms.Button buttonRepondre;

    }
}