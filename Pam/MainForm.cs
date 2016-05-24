using AForge.Video;
using AForge.Video.DirectShow;
using NatLib;
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

namespace Pam
{
    public partial class MainForm : Form
    {
        private delegate void Proc();

        private Nat nat = new Nat();
        private VSourceDlg vSourceDlg = new VSourceDlg();

        private bool playing = false;

        private bool mirror = false;

        private IArtifact sombreroArtifact = new SombreroArtifact();
        private IArtifact sunglassesArtifact = new SunglassesArtifact();
        private IArtifact moustacheArtifact = new MoustacheArtifact();

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
                Invoke((Proc)delegate
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
                Rectangle[] faces = DetectFaces(image);
                if (faces != null && faces.Length > 0)
                {
                    using (Graphics g = Graphics.FromImage(image))
                    {
                        foreach (Rectangle face in faces)
                        {
                            sombreroArtifact.draw(g, face);
                            moustacheArtifact.draw(g, face);
                            sunglassesArtifact.draw(g, face);
                        }
                    }
                }
            }
            catch (Exception) { }

            if (mirror)
            {
                image.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }
        }

        private unsafe Rectangle[] DetectFaces(Bitmap frame)
        {
            BitmapData data = frame.LockBits(new Rectangle(Point.Empty, frame.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                return nat.detectFaces(data.Scan0.ToPointer(), data.Width, data.Height, data.Stride);
            }
            finally
            {
                frame.UnlockBits(data);
            }
        }
    }
}