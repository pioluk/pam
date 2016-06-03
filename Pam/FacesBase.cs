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

        public bool drawId = false;

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
                using (Bitmap faceBitmap = frame.Clone(faceRect, frame.PixelFormat))
                {
                    double bestFactor = 1000;
                    Face bestFace = null;

                    foreach (Face face in detectedFaces)
                    {
                        if (face.InUse)
                            continue;

                        double factor = distanceFactor(face, faceRect);

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
                    }
                    else
                    {
                        IArtifact artifact = RandomArtifact();
                        Face newFace = new Face { Id = nextFaceId++, TimesUnused = 0, Artifact = artifact };
                        newFace.RectFilter.add(faceRect);
                        detectedFaces.Add(newFace);
                    }
                }
            }
        }

        private static double distanceFactor(Face face, Rectangle rect)
        {
            return rectsDistance(face.RectFilter.Rectangle, rect) / Math.Sqrt(face.RectFilter.Rectangle.Width * face.RectFilter.Rectangle.Height);
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

    }
}
