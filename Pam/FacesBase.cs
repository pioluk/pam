using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace Pam
{
    class FacesBase : IDisposable
    {

        private static readonly IArtifact[] availableArtifacts =
        {
            new SombreroArtifact(),
            new SunglassesArtifact(),
            new MoustacheArtifact()
        };

        private Random rng = new Random();

        private List<Face> detectedFaces = new List<Face>();

        private int nextFaceId = 1;

        public bool drawId = false;

        Font font = new Font("Comic Sans MS", 48);

        public void Dispose()
        {
            font.Dispose();
            Clear();
        }

        public void Clear()
        {
            List<Face> oldList = detectedFaces;
            detectedFaces = new List<Face>();
            foreach(Face face in oldList)
                face.Dispose();
        }

        public void DrawArtifacts(Graphics g)
        {
            foreach (Face face in detectedFaces)
            {
                if (face.InUse)
                {
                    if(drawId)
                        g.DrawString(String.Format("#{0}", face.Id), font, Brushes.Blue, face.RectFilter.Rectangle.X, face.RectFilter.Rectangle.Y);
                    else
                        face.Artifact.draw(g, face.RectFilter.Rectangle);
                }
            }
        }

        public void UpdateFacesInfo(Bitmap frame, Rectangle[] faceRects)
        {
            detectedFaces.ForEach((Face face) => {
                face.InUse = false;
                ++face.TimesUnused;
            });

            detectedFaces.RemoveAll((Face face) =>
            {
                if(face.TimesUnused > 100)
                {
                    face.Dispose();
                    return true;
                }
                return false;
            });

            if (faceRects == null || faceRects.Length == 0)
                return;

            foreach (Rectangle faceRect in faceRects)
            {
                Rectangle modFaceRect = new Rectangle(faceRect.X + faceRect.Width / 4, faceRect.Y, faceRect.Width / 2, faceRect.Height);
                using (Bitmap faceBitmap = frame.Clone(modFaceRect, frame.PixelFormat))
                {
                    Bitmap miniFace = new Bitmap(faceBitmap, new Size(16, 16));

                    float[] hist = calcHistogram(faceBitmap);

                    double bestFactor = 1e3f;

                    Face bestFace = null;

                    foreach (Face face in detectedFaces)
                    {
                        if (face.InUse)
                            continue;

                        float mini_mse = MeanSquareError(face.Mini, miniFace);
                        double hist_cmp = compareHistograms(hist, face.histogram) * 1000;
                        double dist = distanceFactor(face, faceRect) * 25;

                        double factor = mini_mse + hist_cmp + dist;

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
                        bestFace.histogram = hist;
                        bestFace.RectFilter.add(faceRect);
                        bestFace.Mini.Dispose();
                        bestFace.Mini = miniFace;
                    }
                    else
                    {
                        IArtifact artifact = RandomArtifact();
                        Face newFace = new Face { Id = nextFaceId++, TimesUnused = 0, Artifact = artifact, Mini = miniFace, histogram = hist };
                        newFace.RectFilter.add(faceRect);
                        detectedFaces.Add(newFace);
                    }
                }
            }
        }

        private IArtifact RandomArtifact()
        {
            int index = rng.Next(0, availableArtifacts.Length);
            return availableArtifacts[index];
        }

        private float MeanSquareError(Bitmap previousFrame, Bitmap frame)
        {
            ulong sum = 0;

            BitmapData previousFrameData = previousFrame.LockBits(new Rectangle(Point.Empty, previousFrame.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
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

            previousFrame.UnlockBits(previousFrameData);
            frame.UnlockBits(frameData);

            return ((float)sum) / (width3 * height);
        }

        private static unsafe float[] calcHistogram(Bitmap image)
        {
            BitmapData data = image.LockBits(new Rectangle(Point.Empty, image.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            const int HIST_LEN = 72;

            int[] hist = new int[HIST_LEN];

            byte* pixels = (byte*)data.Scan0.ToPointer();

            for(int y = 0; y < data.Height; ++y)
            {
                byte* pix = pixels;
                for(int x = 0; x < data.Width; ++x)
                {
                    int r = (int)(uint)pix[0];
                    int g = (int)(uint)pix[1];
                    int b = (int)(uint)pix[2];

                    int h = rgb2hsv_hue(r, g, b);
                    if(h >= 0)
                        hist[h]++;

                    pix += 3;
                }
                pixels += data.Stride;
            }

            float imgSize = data.Width * data.Height;

            image.UnlockBits(data);

            float[] norm_hist = new float[HIST_LEN];
            for(int i = 0; i < hist.Length; ++i)
            {
                norm_hist[i] = hist[i] / imgSize;
            }
            return norm_hist;
        }

        private static double compareHistograms(float[] h1, float[] h2)
        {
            double mse = 0f;

            for(int i = 0; i < h1.Length - 1; ++i)
            {
                float s1 = h1[i] + h1[i + 1];
                float s2 = h2[i] + h2[i + 1];
                float diff = s1 - s2;
                mse += diff * diff;
            }

            float ls1 = h1[h1.Length - 1] + h1[0];
            float ls2 = h2[h2.Length - 1] + h2[0];
            float ldiff = ls1 - ls2;
            mse += ldiff * ldiff;

            return mse;
        }

        private static int rgb2hsv_hue(int r, int g, int b)
        {
            int min = Math.Min(r, Math.Min(g, b));
            int max = Math.Max(r, Math.Max(g, b));
            int span = max - min;

            int hue;

            if (min == max)
                return -1;

            if (r == max)
                hue = 0 + (g - b) * 12 / span;
            else if (g == max)
                hue = 24 + (b - g) * 12 / span;
            else
                hue = 48 + (r - g) * 12 / span;

            while (hue < 0)
                hue += 72;
            while (hue >= 72)
                hue -= 72;

            return hue;
        }

        private static double distanceFactor(Face face, Rectangle rect)
        {
            return (rectsDistance(face.RectFilter.Rectangle, rect) + squaredCentersDistance(face.RectFilter.Rectangle, rect)) / Math.Sqrt(face.RectFilter.Rectangle.Width * face.RectFilter.Rectangle.Height);
        }

        private static ulong rectsDistance(Rectangle a, Rectangle b)
        {
            int dl = a.Left - b.Left;
            int dr = a.Right - b.Right;
            int dt = a.Top - b.Top;
            int db = a.Bottom - b.Bottom;
            ulong ll = (ulong)(dl * dl);
            ulong rr = (ulong)(dr * dr);
            ulong tt = (ulong)(dt * dt);
            ulong bb = (ulong)(db * db);
            return ll + rr + tt + bb;
        }

        private static ulong squaredCentersDistance(Rectangle a, Rectangle b)
        {
            return squaredDistance(rectCenter(a), rectCenter(b));
        }

        private static Point rectCenter(Rectangle rect)
        {
            return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }

        private static ulong squaredDistance(Point a, Point b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            ulong xx = (ulong)(dx * dx);
            ulong yy = (ulong)(dy * dy);
            return xx + yy;
        }

    }
}
