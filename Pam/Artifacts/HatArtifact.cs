using System;
using System.Drawing;

namespace Pam
{
    internal class HatArtifact : IArtifact
    {
        private static readonly Bitmap artifact = new Bitmap("hat.png");

        public void draw(Graphics g, Rectangle face)
        {
            float ratio = (float)face.Width / artifact.Width;
            Size newSize = new Size((int)(artifact.Width * ratio * 1.05), (int)(artifact.Height * ratio));
            Point newPosition = new Point(face.X, face.Y - newSize.Height);
            g.DrawImage(artifact, new Rectangle(newPosition, newSize));
        }
    }
}
