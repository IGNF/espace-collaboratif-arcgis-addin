namespace ArcGisProEspaceCollaboratif
{
    partial class FormConnecterEspaceCollaboratif
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormConnecterEspaceCollaboratif));
            this.InputLoginEspaceCollaboratif = new System.Windows.Forms.TextBox();
            this.labelInputLoginEspaceCollaboratif = new System.Windows.Forms.Label();
            this.labelInputPasswordEspaceCollaboratif = new System.Windows.Forms.Label();
            this.InputPasswordEspaceCollaboratif = new System.Windows.Forms.TextBox();
            this.buttonAbort = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.buttonConnect = new System.Windows.Forms.Button();
            this.LogoBox = new System.Windows.Forms.PictureBox();
            this.lblErreur = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.LogoBox)).BeginInit();
            this.SuspendLayout();
            // 
            // InputLoginEspaceCollaboratif
            // 
            resources.ApplyResources(this.InputLoginEspaceCollaboratif, "InputLoginEspaceCollaboratif");
            this.InputLoginEspaceCollaboratif.Name = "InputLoginEspaceCollaboratif";
            this.InputLoginEspaceCollaboratif.TextChanged += new System.EventHandler(this.InputLoginEspaceCollaboratif_TextChanged);
            // 
            // labelInputLoginEspaceCollaboratif
            // 
            resources.ApplyResources(this.labelInputLoginEspaceCollaboratif, "labelInputLoginEspaceCollaboratif");
            this.labelInputLoginEspaceCollaboratif.Name = "labelInputLoginEspaceCollaboratif";
            // 
            // labelInputPasswordEspaceCollaboratif
            // 
            resources.ApplyResources(this.labelInputPasswordEspaceCollaboratif, "labelInputPasswordEspaceCollaboratif");
            this.labelInputPasswordEspaceCollaboratif.Name = "labelInputPasswordEspaceCollaboratif";
            // 
            // InputPasswordEspaceCollaboratif
            // 
            resources.ApplyResources(this.InputPasswordEspaceCollaboratif, "InputPasswordEspaceCollaboratif");
            this.InputPasswordEspaceCollaboratif.Name = "InputPasswordEspaceCollaboratif";
            this.InputPasswordEspaceCollaboratif.TextChanged += new System.EventHandler(this.InputPasswordEspaceCollaboratif_TextChanged);
            // 
            // buttonAbort
            // 
            this.buttonAbort.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.buttonAbort, "buttonAbort");
            this.buttonAbort.Name = "buttonAbort";
            this.buttonAbort.UseVisualStyleBackColor = true;
            this.buttonAbort.Click += new System.EventHandler(this.ButtonAbort_Click);
            // 
            // buttonConnect
            // 
            this.buttonConnect.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this.buttonConnect, "buttonConnect");
            this.buttonConnect.Image = global::ArcGisProEspaceCollaboratif.Properties.Resources.Fatcow_Farm_Fresh_Connect;
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.ButtonConnect_Click);
            // 
            // LogoBox
            // 
            this.LogoBox.Image = global::ArcGisProEspaceCollaboratif.Properties.Resources.LogoIGN;
            resources.ApplyResources(this.LogoBox, "LogoBox");
            this.LogoBox.Name = "LogoBox";
            this.LogoBox.TabStop = false;
            // 
            // lblErreur
            // 
            resources.ApplyResources(this.lblErreur, "lblErreur");
            this.lblErreur.ForeColor = System.Drawing.Color.Red;
            this.lblErreur.Name = "lblErreur";
            // 
            // FormConnecterEspaceCollaboratif
            // 
            this.AcceptButton = this.buttonConnect;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonAbort;
            this.Controls.Add(this.lblErreur);
            this.Controls.Add(this.buttonAbort);
            this.Controls.Add(this.LogoBox);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.labelInputPasswordEspaceCollaboratif);
            this.Controls.Add(this.InputPasswordEspaceCollaboratif);
            this.Controls.Add(this.labelInputLoginEspaceCollaboratif);
            this.Controls.Add(this.InputLoginEspaceCollaboratif);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormConnecterEspaceCollaboratif";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.TopMost = true;
            this.Load += new System.EventHandler(this.LogEspaceCollaboratif_Load);
            ((System.ComponentModel.ISupportInitialize)(this.LogoBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox InputLoginEspaceCollaboratif;
        private System.Windows.Forms.Label labelInputLoginEspaceCollaboratif;
        private System.Windows.Forms.Label labelInputPasswordEspaceCollaboratif;
        private System.Windows.Forms.TextBox InputPasswordEspaceCollaboratif;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.PictureBox LogoBox;
        private System.Windows.Forms.Button buttonAbort;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Label lblErreur;

        

        

    }
}