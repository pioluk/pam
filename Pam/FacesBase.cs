using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

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

        StreamWriter factLog = new StreamWriter("factLog.txt");

        public void Dispose()
        {
            StreamWriter fl = factLog;
            factLog = null;
            fl.Flush();
            fl.Dispose();
            font.Dispose();
            Clear();
        }

        public void Clear()
        {
            List<Face> oldList = detectedFaces;
            detectedFaces = new List<Face>();
            foreach (Face face in oldList)
                face.Dispose();
        }

        public void DrawArtifacts(Graphics g)
        {
            foreach (Face face in detectedFaces)
            {
                if (face.InUse)
                {
                    if (drawId)
                        g.DrawString(String.Format("#{0}", face.Id), font, Brushes.Blue, face.RectFilter.Rectangle.X, face.RectFilter.Rectangle.Y);
                    else
                        face.Artifact.draw(g, face.RectFilter.Rectangle);
                }
            }
        }

        public void UpdateFacesInfo(Bitmap frame, Rectangle[] faceRects)
        {
            detectedFaces.ForEach((Face face) =>
            {
                face.InUse = false;
                ++face.TimesUnused;
            });

            detectedFaces.RemoveAll((Face face) =>
            {
                if (face.TimesUnused > 100)
                {
                    face.Dispose();
                    return true;
                }
                return false;
            });

            if (faceRects == null || faceRects.Length == 0)
                return;

            bool[] rectUsed = new bool[faceRects.Length];

            foreach (Face face in detectedFaces)
            {

                int bestRectIdx = -1;
                double bestFactor = 1000;
                Bitmap miniFace = null;

                for (int ri = 0; ri < faceRects.Length; ++ri)
                {
                    Rectangle faceRect = faceRects[ri];
                    Rectangle modFaceRect = new Rectangle(faceRect.X + faceRect.Width / 4, faceRect.Y, faceRect.Width / 2, faceRect.Height);

                    using (Bitmap faceBitmap = frame.Clone(modFaceRect, frame.PixelFormat))
                    {
                        if (miniFace != null)
                            miniFace.Dispose();
                        miniFace = new Bitmap(faceBitmap, new Size(16, 16));
                    }

                    double dist = distanceFactor(face, faceRect);
                    float mse = MeanSquareError(face.Mini, miniFace);

                    factLog.WriteLine("{0} {1}", dist, mse);

                    double factor = dist;

                    if (factor < bestFactor)
                    {
                        bestFactor = factor;
                        bestRectIdx = ri;
                    }
                }

                if (bestRectIdx != -1)
                {
                    rectUsed[bestRectIdx] = true;
                    face.InUse = true;
                    face.Mini.Dispose();
                    face.Mini = miniFace;
                }

            }

            for (int ri = 0; ri < faceRects.Length; ++ri)
            {
                if (rectUsed[ri])
                    continue;

                Rectangle faceRect = faceRects[ri];
                Rectangle modFaceRect = new Rectangle(faceRect.X + faceRect.Width / 4, faceRect.Y, faceRect.Width / 2, faceRect.Height);
                Bitmap miniFace;
                using (Bitmap faceBitmap = frame.Clone(modFaceRect, frame.PixelFormat))
                {
                    miniFace = new Bitmap(faceBitmap, new Size(16, 16));
                }

                IArtifact artifact = RandomArtifact();
                Face newFace = new Face { Id = nextFaceId++, TimesUnused = 0, Artifact = artifact, Mini = miniFace };
                newFace.RectFilter.add(faceRect);
                detectedFaces.Add(newFace);
            }

        }

        private IArtifact RandomArtifact()
        {
            int index = rng.Next(0, availableArtifacts.Length);
            return availableArtifacts[index];
        }

        private static double distanceFactor(Face face, Rectangle rect)
        {
            return RectUtils.distanceFactor(face.RectFilter.Rectangle, rect);
        }

        private static unsafe ushort[] blurredImg(Bitmap bmp)
        {
            const int R = 2;
            const int R2 = 2 * R;
            const int D = R2 + 1;
            const int CH = 3;
            int iH = bmp.Height - R2;
            int iW = bmp.Width - R2;
            int iWC = iW * CH;
            ushort[] bl = new ushort[iH * iWC];
            BitmapData data = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                fixed (ushort* bl_ptr = bl)
                {
                    ushort* bl_line = bl_ptr;
                    byte* bmp_line = (byte*)data.Scan0.ToPointer();
                    for (int y = 0; y < iH; ++y)
                    {
                        for (int dy = 0; dy < D; ++dy)
                        {
                            ushort* bl_pix = bl_line;
                            byte* bmp_pix = bmp_line;
                            for (int x = 0; x < iW; ++x)
                            {
                                for (int dx = 0; dx < D; ++dx)
                                {
                                    for (int ch = 0; ch < CH; ++ch)
                                    {
                                        bl_pix[ch] += bmp_pix[ch];
                                    }
                                    bmp_pix += CH;
                                }
                                bmp_pix -= CH * R2;
                                bl_pix += CH;
                            }
                            bmp_line += data.Stride;
                        }
                        bl_line += iWC;
                        bmp_line -= data.Stride * R2;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(data);
            }
            return bl;
        }

        private float MeanSquareError(Bitmap previousFrame, Bitmap frame)
        {
            ulong sum = 0;

            ushort[] pb = blurredImg(previousFrame);
            ushort[] b = blurredImg(frame);

            for (int i = 0; i < b.Length; ++i)
            {
                int x = pb[i];
                int y = b[i];
                int d = x - y;
                ulong dd = (ulong)(d * d);
                sum += dd;
            }

            return ((float)sum) / (b.Length);
        }

        private static unsafe float[] calcHistogram(Bitmap image)
        {
            BitmapData data = image.LockBits(new Rectangle(Point.Empty, image.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            const int HIST_LEN = 72;

            int[] hist = new int[HIST_LEN];

            byte* pixels = (byte*)data.Scan0.ToPointer();

            for (int y = 0; y < data.Height; ++y)
            {
                byte* pix = pixels;
                for (int x = 0; x < data.Width; ++x)
                {
                    int r = (int)(uint)pix[0];
                    int g = (int)(uint)pix[1];
                    int b = (int)(uint)pix[2];

                    int h = rgb2hsv_hue(r, g, b);
                    if (h >= 0)
                        hist[h]++;

                    pix += 3;
                }
                pixels += data.Stride;
            }

            float imgSize = data.Width * data.Height;

            image.UnlockBits(data);

            float[] norm_hist = new float[HIST_LEN];
            for (int i = 0; i < hist.Length; ++i)
            {
                norm_hist[i] = hist[i] / imgSize;
            }
            return norm_hist;
        }

        private static double compareHistograms(float[] h1, float[] h2)
        {
            double mse = 0f;

            for (int i = 0; i < h1.Length - 1; ++i)
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

    }
}
