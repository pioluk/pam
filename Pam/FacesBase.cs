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
                ushort[] miniFace;
                Rectangle modFaceRect = new Rectangle(faceRect.X + faceRect.Width / 4, faceRect.Y, faceRect.Width / 2, faceRect.Height);
                Bitmap miniFaceBmp;
                using (Bitmap faceBitmap = frame.Clone(modFaceRect, frame.PixelFormat))
                {
                    miniFaceBmp = new Bitmap(faceBitmap, new Size(16, 16));
                }
                using (miniFaceBmp)
                {
                    miniFace = blurredImg(miniFaceBmp);
                }

                double bestFactor = 1000;
                Face bestFace = null;
                float bestMse = float.PositiveInfinity;

                foreach (Face face in detectedFaces)
                {
                    if (face.InUse)
                        continue;

                    double dist = distanceFactor(face, faceRect);
                    float mse = MeanSquareError(face.Mini, miniFace);

                    factLog.WriteLine("{0} {1}", dist, mse);

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
                fixed(ushort* bl_ptr = bl)
                {
                    ushort* bl_line = bl_ptr;
                    byte* bmp_line = (byte*)data.Scan0.ToPointer();
                    for(int y = 0; y < iH; ++y)
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

        private float MeanSquareError(ushort[] prev, ushort[] curr)
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

            return ((float)sum) / (curr.Length);
        }

    }
}
