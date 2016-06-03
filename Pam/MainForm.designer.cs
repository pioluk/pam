namespace Pam
{
    partial class MainForm
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
            AtDispose();
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
            this.videoPlayer = new AForge.Controls.VideoSourcePlayer();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.checkMirror = new System.Windows.Forms.CheckBox();
            this.btnClearFaces = new System.Windows.Forms.Button();
            this.checkID = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // videoPlayer
            // 
            this.videoPlayer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.videoPlayer.Location = new System.Drawing.Point(0, 0);
            this.videoPlayer.Name = "videoPlayer";
            this.videoPlayer.Size = new System.Drawing.Size(568, 409);
            this.videoPlayer.TabIndex = 0;
            this.videoPlayer.VideoSource = null;
            // 
            // btnStartStop
            // 
            this.btnStartStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnStartStop.Location = new System.Drawing.Point(12, 415);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(75, 23);
            this.btnStartStop.TabIndex = 0;
            this.btnStartStop.Text = "Start";
            this.btnStartStop.UseVisualStyleBackColor = true;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // checkMirror
            // 
            this.checkMirror.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkMirror.AutoSize = true;
            this.checkMirror.Location = new System.Drawing.Point(93, 419);
            this.checkMirror.Name = "checkMirror";
            this.checkMirror.Size = new System.Drawing.Size(55, 17);
            this.checkMirror.TabIndex = 1;
            this.checkMirror.Text = "Lustro";
            this.checkMirror.UseVisualStyleBackColor = true;
            this.checkMirror.CheckedChanged += new System.EventHandler(this.checkMirror_CheckedChanged);
            // 
            // btnClearFaces
            // 
            this.btnClearFaces.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearFaces.Location = new System.Drawing.Point(481, 415);
            this.btnClearFaces.Name = "btnClearFaces";
            this.btnClearFaces.Size = new System.Drawing.Size(75, 23);
            this.btnClearFaces.TabIndex = 2;
            this.btnClearFaces.Text = "Clear faces";
            this.btnClearFaces.UseVisualStyleBackColor = true;
            this.btnClearFaces.Click += new System.EventHandler(this.btnClearFaces_Click);
            // 
            // checkID
            // 
            this.checkID.AutoSize = true;
            this.checkID.Location = new System.Drawing.Point(154, 419);
            this.checkID.Name = "checkID";
            this.checkID.Size = new System.Drawing.Size(37, 17);
            this.checkID.TabIndex = 3;
            this.checkID.Text = "ID";
            this.checkID.UseVisualStyleBackColor = true;
            this.checkID.CheckedChanged += new System.EventHandler(this.checkID_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 450);
            this.Controls.Add(this.checkID);
            this.Controls.Add(this.btnClearFaces);
            this.Controls.Add(this.btnStartStop);
            this.Controls.Add(this.checkMirror);
            this.Controls.Add(this.videoPlayer);
            this.Name = "MainForm";
            this.Text = "PAM";
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AForge.Controls.VideoSourcePlayer videoPlayer;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.CheckBox checkMirror;
        private System.Windows.Forms.Button btnClearFaces;
        private System.Windows.Forms.CheckBox checkID;
    }
}

