using System;
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
            if(dlgOpenFile.ShowDialog() == DialogResult.OK)
            {
                videoSource = new FileVideoSource(dlgOpenFile.FileName);
                DialogResult = DialogResult.OK;
            }
        }
    }
}
