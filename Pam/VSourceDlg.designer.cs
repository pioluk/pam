namespace Pam
{
    partial class VSourceDlg
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
            System.Windows.Forms.Label labCam;
            this.lstDev = new System.Windows.Forms.ComboBox();
            this.btnRefreshList = new System.Windows.Forms.Button();
            this.ttBtnRfrshLst = new System.Windows.Forms.ToolTip(this.components);
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnFile = new System.Windows.Forms.Button();
            this.dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
            labCam = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lstDev
            // 
            this.lstDev.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstDev.FormattingEnabled = true;
            this.lstDev.Location = new System.Drawing.Point(64, 19);
            this.lstDev.Name = "lstDev";
            this.lstDev.Size = new System.Drawing.Size(302, 21);
            this.lstDev.TabIndex = 1;
            // 
            // btnRefreshList
            // 
            this.btnRefreshList.AccessibleName = "Odśwież listę";
            this.btnRefreshList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefreshList.BackgroundImage = global::Pam.Properties.Resources.RefreshIcon;
            this.btnRefreshList.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnRefreshList.Location = new System.Drawing.Point(372, 12);
            this.btnRefreshList.Name = "btnRefreshList";
            this.btnRefreshList.Size = new System.Drawing.Size(32, 32);
            this.btnRefreshList.TabIndex = 2;
            this.btnRefreshList.UseVisualStyleBackColor = true;
            this.btnRefreshList.Click += new System.EventHandler(this.btnRefreshList_Click);
            // 
            // labCam
            // 
            labCam.AutoSize = true;
            labCam.Location = new System.Drawing.Point(12, 22);
            labCam.Name = "labCam";
            labCam.Size = new System.Drawing.Size(46, 13);
            labCam.TabIndex = 0;
            labCam.Text = "Kamera:";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(248, 56);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(329, 56);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Anuluj";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnFile
            // 
            this.btnFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnFile.Location = new System.Drawing.Point(12, 56);
            this.btnFile.Name = "btnFile";
            this.btnFile.Size = new System.Drawing.Size(91, 23);
            this.btnFile.TabIndex = 3;
            this.btnFile.Text = "Z pliku wideo";
            this.btnFile.UseVisualStyleBackColor = true;
            this.btnFile.Click += new System.EventHandler(this.btnFile_Click);
            // 
            // VSourceDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(416, 91);
            this.Controls.Add(this.btnFile);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(labCam);
            this.Controls.Add(this.btnRefreshList);
            this.Controls.Add(this.lstDev);
            this.Name = "VSourceDlg";
            this.Text = "Źródło obrazu";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox lstDev;
        private System.Windows.Forms.Button btnRefreshList;
        private System.Windows.Forms.ToolTip ttBtnRfrshLst;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnFile;
        private System.Windows.Forms.OpenFileDialog dlgOpenFile;
    }
}