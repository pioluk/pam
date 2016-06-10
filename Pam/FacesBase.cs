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

    }
}
