using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using NatLib;

namespace Pam
{
    public partial class MainForm : Form
    {

        private delegate void Proc();
        private Nat nat = new Nat();
        private VSourceDlg vSourceDlg = new VSourceDlg();
        private Bitmap img = new Bitmap("sombrero.png");
        private bool playing = false;
        private bool mirror = false;

        public MainForm()
        {
            InitializeComponent();
            videoPlayer.NewFrame += VideoPlayer_NewFrame;
            videoPlayer.PlayingFinished += VideoPlayer_PlayingFinished;
        }

        private void AtDispose()
        {
            Stop();
        }

        private void Stop()
        {
            videoPlayer.SignalToStop();
        }

        private void Start()
        {
            if (playing)
                return;
            vSourceDlg.refreshDevicesList();
            if (vSourceDlg.ShowDialog() == DialogResult.OK)
            {
                videoPlayer.VideoSource = vSourceDlg.videoSource;
                videoPlayer.Start();
                playing = true;
                btnStartStop.Text = "Stop";
                vSourceDlg.videoSource = null;
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            Start();
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (playing)
                Stop();
            else
                Start();
        }

        private void VideoPlayer_PlayingFinished(object sender, ReasonToFinishPlaying reason)
        {
            playing = false;
            if (!IsDisposed)
                Invoke((Proc) delegate
                {
                    btnStartStop.Text = "Start";
                    videoPlayer.VideoSource = null;
                });
        }

        private void checkMirror_CheckedChanged(object sender, EventArgs e)
        {
            mirror = checkMirror.Checked;
        }

        private void VideoPlayer_NewFrame(object sender, ref Bitmap image)
        {
            try
            {
                Rectangle[] eyes, faceEyes;
                Rectangle[] faces = DetectFaces(image, out eyes, out faceEyes);
                using (Graphics g = Graphics.FromImage(image))
                {
                    if (faces != null && faces.Length > 0)
                        g.DrawRectangles(Pens.AliceBlue, faces);
                    if (eyes != null && eyes.Length > 0)
                        g.DrawRectangles(Pens.BlueViolet, eyes);
                    if (faceEyes != null && faceEyes.Length > 0)
                        g.DrawRectangles(Pens.DarkRed, faceEyes);
                }
            }
            catch(Exception) { }
            if(mirror)
                image.RotateFlip(RotateFlipType.RotateNoneFlipX);
        }

        private unsafe Rectangle[] DetectFaces(Bitmap frame, out Rectangle[] retEyes, out Rectangle[] retFaceEyes)
        {
            BitmapData data = frame.LockBits(new Rectangle(Point.Empty, frame.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                return nat.detectFaces(data.Scan0.ToPointer(), data.Width, data.Height, data.Stride, out retEyes, out retFaceEyes);
            }
            finally
            {
                frame.UnlockBits(data);
            }
        }

    }
}
