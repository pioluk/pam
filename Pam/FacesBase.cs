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
        }

        public void Clear()
        {
            detectedFaces.Clear();
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
                ++face.TimesUndetected;
                ++face.Age;
            });

            detectedFaces.Sort((Face a, Face b) => { return a.TimesUnused - b.TimesUnused; });

            detectedFaces.RemoveAll((Face face) => { return (face.TimesUnused > 100); });

            if (faceRects == null || faceRects.Length == 0)
                return;

            bool[] rectUsed = new bool[faceRects.Length];
            ushort[][] miniFaces = new ushort[faceRects.Length][];

            for (int ri = 0; ri < faceRects.Length; ++ri)
            {
                miniFaces[ri] = faceImg(frame, faceRects[ri]);
            }

            foreach (Face face in detectedFaces)
            {
                int bestRectIdx = -1;
                double bestDist = 2;
                double bestMSE = 3000000;

                for (int ri = 0; ri < faceRects.Length; ++ri)
                {
                    Rectangle faceRect = faceRects[ri];
                    ushort[] miniFace = miniFaces[ri];

                    double dist = distanceFactor(face, faceRect);
                    double mse = MeanSquareError(face.Mini, miniFace);

                    factLog.WriteLine("{0} {1}", dist, mse);

                    if (dist < bestDist && mse < bestMSE)
                    {
                        bestDist = dist;
                        bestMSE = mse;
                        bestRectIdx = ri;
                    }
                }

                if (bestRectIdx != -1)
                {
                    rectUsed[bestRectIdx] = true;
                    face.InUse = true;
                    face.TimesUnused = 0;
                    face.TimesUndetected = 0;
                    face.RectFilter.add(faceRects[bestRectIdx]);
                    face.Mini = miniFaces[bestRectIdx];
                }

            }

            foreach (Face face in detectedFaces)
            {
                if (face.InUse)
                    continue;

                if (face.TimesUndetected > 10)
                    continue;

                ushort[] img = faceImg(frame, face.RectFilter.Rectangle);
                double mse = MeanSquareError(face.Mini, img);

                if(mse < 1000000)
                {
                    face.InUse = true;
                    face.TimesUnused = 0;
                    face.Mini = img;
                }
            }

            for (int ri = 0; ri < faceRects.Length; ++ri)
            {
                if (rectUsed[ri])
                    continue;

                IArtifact artifact = RandomArtifact();
                Face newFace = new Face { Id = nextFaceId++, TimesUnused = 0, Artifact = artifact, Mini = miniFaces[ri] };
                newFace.RectFilter.add(faceRects[ri]);
                detectedFaces.Add(newFace);
            }

        }

        private static ushort[] faceImg(Bitmap frame, Rectangle faceRect)
        {
            Rectangle modFaceRect = new Rectangle(faceRect.X + faceRect.Width / 4, faceRect.Y, faceRect.Width / 2, faceRect.Height);
            Bitmap miniFaceBmp;
            using (Bitmap faceBitmap = frame.Clone(modFaceRect, frame.PixelFormat))
            {
                miniFaceBmp = new Bitmap(faceBitmap, new Size(16, 16));
            }
            using (miniFaceBmp)
            {
                return blurredImg(miniFaceBmp);
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

        private static double MeanSquareError(ushort[] prev, ushort[] curr)
        {
            ulong sum = 0;

            for(int i = 0; i < curr.Length; ++i)
            {
                int p = prev[i];
                int c = curr[i];
                int d = p - c;
                ulong dd = (ulong)(d * d);
                sum += dd;
            }

            return ((double)sum) / curr.Length;
        }

    }
}
