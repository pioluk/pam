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

        private int nextFaceId = 1;

        public void Clear()
        {
            List<Face> oldList = detectedFaces;
            detectedFaces = new List<Face>();
            foreach(Face face in oldList)
                face.Dispose();
        }

        private IArtifact RandomArtifact()
        {
            int index = rng.Next(0, availableArtifacts.Length);
            return availableArtifacts[index];
        }

        public void DrawArtifacts(Graphics g)
        {
            Font font = new Font("Comic Sans MS", 48);
            foreach (Face face in detectedFaces)
            {
                if (face.InUse)
                {
                    //face.Artifact.draw(g, face.RectFilter.Rectangle);
                    g.DrawString(String.Format("#{0}", face.Id), font, Brushes.Blue, face.RectFilter.Rectangle.X, face.RectFilter.Rectangle.Y);
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
                using (Bitmap faceBitmap = frame.Clone(faceRect, frame.PixelFormat))
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

        private static unsafe float[] calcHistogram(Bitmap image)
        {
            BitmapData data = image.LockBits(new Rectangle(Point.Empty, image.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int[] hist = new int[Face.HIST_LEN];

            byte* pixels = (byte*)data.Scan0.ToPointer();

            for(int y = 0; y < data.Height; ++y)
            {
                byte* pix = pixels;
                for(int x = 0; x < data.Width; ++x)
                {
                    uint r = (uint)pix[0] >> (8 - Face.HIST_CH_BITS);
                    uint g = (uint)pix[1] >> (8 - Face.HIST_CH_BITS);
                    uint b = (uint)pix[2] >> (8 - Face.HIST_CH_BITS);

                    uint v = r | (g << Face.HIST_CH_BITS) | (b << (Face.HIST_CH_BITS * 2));

                    hist[v]++;

                    pix += 3;
                }
                pixels += data.Stride;
            }

            float imgSize = data.Width * data.Height;

            image.UnlockBits(data);

            float[] norm_hist = new float[Face.HIST_LEN];
            for(int i = 0; i < hist.Length; ++i)
            {
                norm_hist[i] = hist[i] / imgSize;
            }
            return norm_hist;
        }

        private static double compareHistograms(float[] h1, float[] h2)
        {
            double mse = 0f;

            for(int i = 0; i < Face.HIST_LEN; ++i)
            {
                float diff = h1[i] - h2[i];
                mse += diff * diff;
            }

            return mse;
        }

    }
}
