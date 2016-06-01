using AForge.Video;
using NatLib;
using System;
using System.Collections.Generic;
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
        private bool mirror = true;

        private readonly IArtifact[] availableArtifacts =
        {
            new SombreroArtifact(),
            new SunglassesArtifact(),
            new MoustacheArtifact()
        };

        private volatile int frameCount = 0;

        private List<Face> detectedFaces = new List<Face>();

        private Timer timer = new Timer();

        private Random rng = new Random();

        public MainForm()
        {
            InitializeComponent();
            videoPlayer.NewFrame += VideoPlayer_NewFrame;
            videoPlayer.PlayingFinished += VideoPlayer_PlayingFinished;

            timer.Interval = 1000;
            timer.Tick += FPSCount;
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

        private IArtifact RandomArtifact()
        {
            int index = rng.Next(0, availableArtifacts.Length);
            return availableArtifacts[index];
        }

        private float MeanSquareError(Bitmap previousFrame, Bitmap frame)
        {
            Bitmap scaledPreviousFrame = previousFrame;
            bool clonedPrevFrame = false;

            if (previousFrame.Size != frame.Size)
            {
                scaledPreviousFrame = new Bitmap(previousFrame, frame.Size);
                clonedPrevFrame = true;
            }

            ulong sum = 0;

            BitmapData previousFrameData = scaledPreviousFrame.LockBits(new Rectangle(Point.Empty, scaledPreviousFrame.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData frameData = frame.LockBits(new Rectangle(Point.Empty, frame.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int width3 = frameData.Width * 3;
            int height = frameData.Height;

            unsafe
            {
                byte* previousPixels = (byte*)previousFrameData.Scan0.ToPointer();
                byte* pixels = (byte*)frameData.Scan0.ToPointer();

                for (int y = 0; y < height; ++y)
                {
                    byte* pp = previousPixels;
                    byte* p = pixels;

                    for (int x = 0; x < width3; ++x)
                    {
                        byte pVal = *pp;
                        byte val = *p;

                        long diff = pVal - val;
                        long sd = diff * diff;

                        sum += (ulong)sd;

                        ++pp;
                        ++p;
                    }

                    previousPixels += previousFrameData.Stride;
                    pixels += frameData.Stride;
                }
            }

            scaledPreviousFrame.UnlockBits(previousFrameData);
            frame.UnlockBits(frameData);

            if(clonedPrevFrame)
            {
                scaledPreviousFrame.Dispose();
            }

            return ((float)sum) / (width3 * height);
        }

        private void VideoPlayer_NewFrame(object sender, ref Bitmap frame)
        {
            try
            {
                Rectangle[] faces = DetectFaces(frame);
                UpdateFacesInfo(frame, faces);
            }
            catch (Exception) { }

            using (Graphics g = Graphics.FromImage(frame))
            {
                DrawArtifacts(g);
            }

            if (mirror)
            {
                frame.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }

            ++frameCount;
        }

        private void DrawArtifacts(Graphics g)
        {
            foreach(Face face in detectedFaces)
            {
                if(face.InUse)
                {
                    face.Artifact.draw(g, face.RectFilter.Rectangle);
                }
            }
        }

        private void UpdateFacesInfo(Bitmap frame, Rectangle[] faceRects)
        {
            detectedFaces.ForEach(f => f.InUse = false);

            if (faceRects == null || faceRects.Length == 0)
                return;

            foreach (Rectangle faceRect in faceRects)
            {
                Bitmap faceBitmap = frame.Clone(new Rectangle(faceRect.Location, faceRect.Size), frame.PixelFormat);

                float bestFactor = 1e3f;
                Face bestFace = null;

                foreach (Face face in detectedFaces)
                {
                    if (face.InUse)
                        continue;

                    if (face.TimesUnused > 100)
                    {
                        detectedFaces.Remove(face);
                        face.Dispose();
                        continue;
                    }

                    ++face.TimesUnused;

                    float mse = MeanSquareError(face.Bitmap, faceBitmap);
                    float factor = mse;

                    if (factor < bestFactor)
                    {
                        bestFactor = factor;
                        bestFace = face;
                    }
                }

                if (bestFace != null)
                {
                    bestFace.InUse = true;
                    bestFace.TimesUnused = 0;
                    bestFace.RectFilter.add(faceRect);
                    bestFace.Bitmap.Dispose();
                    bestFace.Bitmap = faceBitmap;
                }
                else
                {
                    IArtifact artifact = RandomArtifact();
                    Face newFace = new Face { TimesUnused = 0, Bitmap = faceBitmap, Artifact = artifact };
                    newFace.RectFilter.add(faceRect);
                    detectedFaces.Add(newFace);
                }
            }
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
            detectedFaces.Clear();
        }
    }
}