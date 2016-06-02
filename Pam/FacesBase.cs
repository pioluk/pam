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
                using (Bitmap faceBitmap = frame.Clone(new Rectangle(faceRect.Location, faceRect.Size), frame.PixelFormat))
                {
                    float bestFactor = 1e3f;
                    Face bestFace = null;

                    foreach (Face face in detectedFaces)
                    {
                        if (face.InUse)
                            continue;

                        float factor = 0;

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
                        Face newFace = new Face { TimesUnused = 0, Artifact = artifact };
                        newFace.RectFilter.add(faceRect);
                        detectedFaces.Add(newFace);
                    }
                }
            }
        }

    }
}
