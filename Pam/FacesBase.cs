using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace Pam
{
    class FacesBase
    {

        private static readonly IArtifact[] availableArtifacts =
        {
            new SombreroArtifact(),
            new SunglassesArtifact(),
            new MoustacheArtifact()
        };

        private Random rng = new Random();

        private List<Face> detectedFaces = new List<Face>();

        public void Clear()
        {
            detectedFaces.Clear();
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

            if (clonedPrevFrame)
            {
                scaledPreviousFrame.Dispose();
            }

            return ((float)sum) / (width3 * height);
        }

        public void DrawArtifacts(Graphics g)
        {
            foreach (Face face in detectedFaces)
            {
                if (face.InUse)
                {
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
                Bitmap faceBitmap = frame.Clone(new Rectangle(faceRect.Location, faceRect.Size), frame.PixelFormat);

                float bestFactor = 1e3f;
                Face bestFace = null;

                foreach (Face face in detectedFaces)
                {
                    if (face.InUse)
                        continue;

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

    }
}
