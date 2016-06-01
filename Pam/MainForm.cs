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

            float sumR = 0f, sumG = 0f, sumB = 0f;

            BitmapData previousFrameData = scaledPreviousFrame.LockBits(new Rectangle(Point.Empty, scaledPreviousFrame.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData frameData = frame.LockBits(new Rectangle(Point.Empty, frame.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int bitmapSize = frame.Width * frame.Height;

            unsafe
            {
                byte* previousPixels = (byte*)previousFrameData.Scan0.ToPointer();
                byte* pixels = (byte*)frameData.Scan0.ToPointer();

                for (int i = 0; i < bitmapSize; ++i)
                {
                    byte b1 = *(previousPixels);
                    byte g1 = *(previousPixels + 1);
                    byte r1 = *(previousPixels + 2);

                    byte b2 = *(pixels);
                    byte g2 = *(pixels + 1);
                    byte r2 = *(pixels + 2);

                    sumB += (b1 - b2) * (b1 - b2);
                    sumG += (g1 - g2) * (g1 - g2);
                    sumR += (r1 - r2) * (r1 - r2);

                    previousPixels += 3;
                    pixels += 3;
                }
            }

            scaledPreviousFrame.UnlockBits(previousFrameData);
            frame.UnlockBits(frameData);

            if(clonedPrevFrame)
            {
                scaledPreviousFrame.Dispose();
            }

            sumB /= bitmapSize;
            sumG /= bitmapSize;
            sumR /= bitmapSize;

            return (sumR + sumG + sumB) / 3f;
        }

        private void VideoPlayer_NewFrame(object sender, ref Bitmap frame)
        {
            try
            {
                Rectangle[] faces = DetectFaces(frame);
                if (faces != null && faces.Length > 0)
                {
                    UpdateFacesInfo(frame, faces);
                    using (Graphics g = Graphics.FromImage(frame))
                    {
                        DrawArtifacts(g);
                    }
                }
            }
            catch (Exception) { }

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
                    float factor = mse / (faceBitmap.Width * faceBitmap.Height) * 100f;

                    if (factor < bestFactor && factor < 15f)
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