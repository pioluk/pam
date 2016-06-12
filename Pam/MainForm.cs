using AForge.Video;
using NatLib;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Pam
{
    public partial class MainForm : Form
    {
        private static readonly string TITLE = "PAM";

        private delegate void Proc();

        private Nat nat = new Nat();
        private VSourceDlg vSourceDlg = new VSourceDlg();

        private bool playing = false;
        private bool closing = false;
        private bool mirror = false;

        private volatile int frameCount = 0;

        private Timer timer = new Timer();

        private FacesBase facesBase = new FacesBase();

        public MainForm()
        {
            InitializeComponent();
            videoPlayer.NewFrame += VideoPlayer_NewFrame;
            videoPlayer.PlayingFinished += VideoPlayer_PlayingFinished;

            timer.Interval = 1000;
            timer.Tick += FPSCount;
        }

        private void AtDispose1()
        {
            closing = true;
            Stop();
            timer.Dispose();
        }

        private void AtDispose2()
        {
            //nat.Dispose();
            facesBase.Dispose();
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

            timer.Start();
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
            {
                try
                {
                    Invoke((Proc)delegate
                    {
                        btnStartStop.Text = "Start";
                        videoPlayer.VideoSource = null;
                    });
                }
                catch(Exception) { }
            }
        }

        private void checkMirror_CheckedChanged(object sender, EventArgs e)
        {
            mirror = checkMirror.Checked;
        }

        private void checkID_CheckedChanged(object sender, EventArgs e)
        {
            facesBase.drawId = checkID.Checked;
        }

        private void VideoPlayer_NewFrame(object sender, ref Bitmap frame)
        {
            if (closing)
                return;

            try
            {
                Rectangle[] faces = DetectFaces(frame);
                facesBase.UpdateFacesInfo(frame, faces);
                using (Graphics g = Graphics.FromImage(frame))
                {
                    facesBase.DrawArtifacts(g);
                }
            }
            catch (Exception) { }

            if (mirror)
            {
                frame.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }

            ++frameCount;
        }

        private void FPSCount(object sender, EventArgs args)
        {
            Text = string.Format("{0} ({1} FPS)", TITLE, frameCount.ToString());
            frameCount = 0;
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

        private void btnClearFaces_Click(object sender, EventArgs e)
        {
            facesBase.Clear();
        }

    }
}
