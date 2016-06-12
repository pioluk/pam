using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

namespace Pam
{
    public partial class VSourceDlg : Form
    {

        public IVideoSource videoSource = null;

        private class FI
        {
            public FilterInfo fi;
            public FI(FilterInfo f)
            {
                fi = f;
            }
            public override string ToString()
            {
                return fi.Name;
            }
        }

        public VSourceDlg()
        {
            InitializeComponent();
            ttBtnRfrshLst.SetToolTip(btnRefreshList, "Odśwież listę");
        }

        public void refreshDevicesList()
        {
            lstDev.Items.Clear();
            FilterInfoCollection fic = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo fi in fic)
                lstDev.Items.Add(new FI(fi));
        }

        private void btnRefreshList_Click(object sender, EventArgs e)
        {
            refreshDevicesList();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            FI fi = (FI)lstDev.SelectedItem;
            if (fi == null)
                return;
            videoSource = new VideoCaptureDevice(fi.fi.MonikerString);
            DialogResult = DialogResult.OK;
        }

        private void btnFile_Click(object sender, EventArgs e)
        {
            string formats = "All Videos Files |*.dat; *.wmv; *.3g2; *.3gp; *.3gp2; *.3gpp; *.amv; *.asf;  *.avi; *.bin; *.cue; *.divx; *.dv; *.flv; *.gxf; *.iso; *.m1v; *.m2v; *.m2t; *.m2ts; *.m4v; " +
                  " *.mkv; *.mov; *.mp2; *.mp2v; *.mp4; *.mp4v; *.mpa; *.mpe; *.mpeg; *.mpeg1; *.mpeg2; *.mpeg4; *.mpg; *.mpv2; *.mts; *.nsv; *.nuv; *.ogg; *.ogm; *.ogv; *.ogx; *.ps; *.rec; *.rm; *.rmvb; *.tod; *.ts; *.tts; *.vob; *.vro; *.webm";

            dlgOpenFile.Filter = formats;
            if (dlgOpenFile.ShowDialog() == DialogResult.OK)
            {
                
                videoSource = new FileVideoSource(dlgOpenFile.FileName);
                DialogResult = DialogResult.OK;
            }
        }
    }
}
