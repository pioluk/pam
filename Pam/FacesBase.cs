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
                    float[] hist = calcHistogram(faceBitmap);

                    double bestFactor = 1e3f;
                    Face bestFace = null;

                    foreach (Face face in detectedFaces)
                    {
                        if (face.InUse)
                            continue;

                        double factor = compareHistograms(hist, face.histogram);

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
                    }
                    else
                    {
                        IArtifact artifact = RandomArtifact();
                        Face newFace = new Face { Id = nextFaceId++, TimesUnused = 0, Artifact = artifact, histogram = hist };
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

        private static unsafe float[] calcHistogram(Bitmap image)
        {
            BitmapData data = image.LockBits(new Rectangle(Point.Empty, image.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            const int HIST_LEN = 36;

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

            for(int i = 0; i < h1.Length; ++i)
            {
                float diff = h1[i] - h2[i];
                mse += diff * diff;
            }

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
                hue = 0 + (g - b) * 6 / span;
            else if (g == max)
                hue = 12 + (b - g) * 6 / span;
            else
                hue = 24 + (r - g) * 6 / span;

            while (hue < 0)
                hue += 36;
            while (hue >= 36)
                hue -= 36;

            return hue;

        }

    }
}
