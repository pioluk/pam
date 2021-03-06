﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace Pam
{
    class FacesBase : IDisposable
    {

        private static readonly IArtifact[] availableArtifacts =
        {
            new Artifacts.Sombrero(),
            new Artifacts.Sunglasses(),
            new Artifacts.Moustache(),
            new Artifacts.Helmet(),
            new Artifacts.Moustache2(),
            new Artifacts.Moustache3(),
            new Artifacts.FunnyGlasses(),
            new Artifacts.Hat(),
            new Artifacts.Beard(),
            new Artifacts.Moustache4(),
            new Artifacts.Cap(),
        };

        private int[] artifactUseCounts = new int[availableArtifacts.Length];

        private Random rng = new Random();

        private List<Face> detectedFaces = new List<Face>();

        private int nextFaceId = 1;

        public bool drawId = false;

        private static readonly Font font = new Font("Comic Sans MS", 48);

        private const int Blur_R = 2;

        public void Dispose()
        {
        }

        public void Clear()
        {
            detectedFaces.Clear();
        }

        public void refreshArtifacts()
        {
            foreach (Face face in detectedFaces)
                face.Artifact = null;
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
                    {
                        if (face.Artifact == null)
                            face.Artifact = RandomArtifact();
                        face.Artifact.draw(g, face.RectFilter.Rectangle);
                    }
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

            mergeDuplicatedFaces();

            detectedFaces.Sort((Face a, Face b) =>
            {
                int p = a.TimesUnused - b.TimesUnused;
                if (p == 0)
                    p = a.TimesUndetected - b.TimesUndetected;
                if (p == 0)
                    p = b.Age - a.Age;
                return p;
            });

            detectedFaces.RemoveAll((Face face) => { return (face.TimesUnused > 1000); });

            int faceRectCount = (faceRects == null ? 0 : faceRects.Length);

            bool[] rectUsed = null;
            ushort[][] miniFaces = null;

            if (faceRectCount > 0)
            {
                rectUsed = new bool[faceRectCount];
                miniFaces = new ushort[faceRectCount][];
                for (int ri = 0; ri < faceRectCount; ++ri)
                    miniFaces[ri] = faceImg(frame, faceRects[ri]);
            }

            foreach (Face face in detectedFaces)
            {
                if (face.TimesUnused > 100)
                    continue;

                double timeFactor = face.TimesUnused * 0.02;
                timeFactor *= timeFactor;

                int bestRectIdx = -1;
                double bestDist = 2;
                double bestMSE = 4000;

                for (int ri = 0; ri < faceRectCount; ++ri)
                {
                    Rectangle faceRect = faceRects[ri];
                    ushort[] miniFace = miniFaces[ri];

                    double dist = distanceFactor(face, faceRect) + timeFactor;
                    double mse = MeanSquareError(face.Mini, miniFace);

                    if (dist < bestDist && mse < bestMSE)
                    {
                        bestDist = dist;
                        bestMSE = mse;
                        bestRectIdx = ri;
                    }
                }

                if(bestRectIdx == -1)
                {
                    bestMSE = 100;

                    for (int ri = 0; ri < faceRectCount; ++ri)
                    {
                        if (rectUsed[ri])
                            continue;

                        Rectangle faceRect = faceRects[ri];
                        ushort[] miniFace = miniFaces[ri];

                        double mse = MeanSquareError(face.Mini, miniFace);

                        if (mse < bestMSE)
                        {
                            bestMSE = mse;
                            bestRectIdx = ri;
                        }
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

                if(mse < 1600)
                {
                    face.InUse = true;
                    face.TimesUnused = 0;
                    face.Mini = img;
                }
            }

            for (int ri = 0; ri < faceRectCount; ++ri)
            {
                if (rectUsed[ri])
                    continue;

                Face newFace = new Face { Id = nextFaceId++, Mini = miniFaces[ri] };
                newFace.RectFilter.add(faceRects[ri]);
                detectedFaces.Add(newFace);
            }

        }

        private static ushort[] faceImg(Bitmap frame, Rectangle faceRect)
        {
            Rectangle modFaceRect = new Rectangle(faceRect.X + faceRect.Width / 4, faceRect.Y, faceRect.Width / 2, faceRect.Height);
            Bitmap miniFaceBmp;
            using (Bitmap faceBitmap = frame.Clone(modFaceRect, PixelFormat.Format24bppRgb))
            {
                miniFaceBmp = new Bitmap(faceBitmap, new Size(24, 32));
            }
            using (miniFaceBmp)
            {
                return blurredImg(miniFaceBmp);
            }
        }

        private IArtifact RandomArtifact()
        {
            int min = int.MaxValue;
            int minCnt = 0;
            for(int i = 0; i < artifactUseCounts.Length; ++i)
            {
                int x = artifactUseCounts[i];
                if (x < min)
                {
                    min = x;
                    minCnt = 1;
                }
                else if (x == min)
                    ++minCnt;
            }
            int index = 0;
            int r = rng.Next(minCnt);
            for(int i = 0, j = 0; i < artifactUseCounts.Length; ++i)
            {
                if (min != artifactUseCounts[i])
                    continue;
                if(j == r)
                {
                    index = i;
                    break;
                }
                ++j;
            }
            artifactUseCounts[index]++;
            return availableArtifacts[index];
        }

        private static double distanceFactor(Face face, Rectangle rect)
        {
            return RectUtils.distanceFactor(face.RectFilter.Rectangle, rect);
        }

        private static unsafe ushort[] blurredImg(Bitmap bmp)
        {
            const int R = Blur_R;
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
            const int BA = Blur_R * 2 + 1;
            const int BF = BA * BA * BA * BA;

            ulong sum = 0;

            for(int i = 0; i < curr.Length; ++i)
            {
                int p = prev[i];
                int c = curr[i];
                int d = p - c;
                ulong dd = (ulong)(d * d);
                sum += dd;
            }

            return ((double)sum) / (curr.Length * BF);
        }

        private void mergeDuplicatedFaces()
        {
            List<Face> toRemove = new List<Face>();
            for(int i = 0; i < detectedFaces.Count; ++i)
            {
                Face a = detectedFaces[i];
                if (a.TimesUnused > 20)
                    continue;

                for(int j = i + 1; j < detectedFaces.Count; ++j)
                {
                    Face b = detectedFaces[j];
                    if (b.TimesUnused > 20)
                        continue;

                    double dist = distanceFactor(a, b.RectFilter.Rectangle);
                    if(dist < 0.1)
                    {
                        double mse = MeanSquareError(a.Mini, b.Mini);
                        if(mse < 100)
                        {
                            if (a.Age < b.Age)
                            {
                                Face t = a;
                                a = b;
                                b = t;
                            }
                            if(b.TimesUndetected < a.TimesUndetected)
                            {
                                a.RectFilter.add(b.RectFilter.Rectangle);
                                a.TimesUndetected = b.TimesUndetected;
                            }
                            if(b.TimesUnused < a.TimesUnused)
                            {
                                a.TimesUnused = b.TimesUnused;
                            }
                            toRemove.Add(b);
                        }
                    }
                }
            }
            detectedFaces = detectedFaces.Except(toRemove).ToList();
        }

    }
}
