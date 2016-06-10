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
                Bitmap miniFace;
                Rectangle modFaceRect = new Rectangle(faceRect.X + faceRect.Width / 4, faceRect.Y, faceRect.Width / 2, faceRect.Height);

                using (Bitmap faceBitmap = frame.Clone(modFaceRect, frame.PixelFormat))
                {
                    miniFace = new Bitmap(faceBitmap, new Size(16, 16));
                }

                double bestFactor = 1;
                Face bestFace = null;
                float bestMse = float.PositiveInfinity;

                foreach (Face face in detectedFaces)
                {
                    if (face.InUse)
                        continue;

                    double dist = distanceFactor(face, faceRect);
                    float mse = MeanSquareError(face.Mini, miniFace);

                    double factor = dist;

                    if (factor < bestFactor)
                    {
                        bestFactor = factor;
                        bestFace = face;
                        bestMse = mse;
                    }
                }

                if (bestFace != null)
                {
                    bestFace.InUse = true;
                    bestFace.TimesUnused = 0;
                    bestFace.RectFilter.add(faceRect);
                    bestFace.Mini.Dispose();
                    bestFace.Mini = miniFace;
                }
                else
                {
                    IArtifact artifact = RandomArtifact();
                    Face newFace = new Face { Id = nextFaceId++, TimesUnused = 0, Artifact = artifact, Mini = miniFace };
                    newFace.RectFilter.add(faceRect);
                    detectedFaces.Add(newFace);
                }

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
            int iH = bmp.Height - 2;
            int iW = bmp.Width - 2;
            int iW3 = iW * 3;
            ushort[] bl = new ushort[iH * iW3];
            BitmapData data = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                fixed(ushort* bl_ptr = bl)
                {
                    ushort* bl_line = bl_ptr;
                    byte* bmp_line = (byte*)data.Scan0.ToPointer();
                    for(int y = 0; y < iH; ++y)
                    {
                        for (int dy = 0; dy < 3; ++dy)
                        {
                            ushort* bl_pix = bl_line;
                            byte* bmp_pix = bmp_line;
                            for (int x = 0; x < iW; ++x)
                            {
                                for (int dx = 0; dx < 3; ++dx)
                                {
                                    for (int ch = 0; ch < 3; ++ch)
                                    {
                                        bl_pix[ch] += bmp_pix[ch];
                                    }
                                    bmp_pix += 3;
                                }
                                bmp_pix -= 6;
                                bl_pix += 3;
                            }
                            bmp_line += data.Stride;
                        }
                        bl_line += iW3;
                        bmp_line -= data.Stride * 2;
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

            for(int i = 0; i < b.Length; ++i)
            {
                int x = pb[i];
                int y = b[i];
                int d = x - y;
                int dd = d * d;
                sum += (uint)dd;
            }

            return ((float)sum) / (b.Length);
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

    }
}
