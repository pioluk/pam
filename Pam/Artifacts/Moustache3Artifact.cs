using System.Drawing;

namespace Pam.Artifacts
{
    internal class Moustache3Artifact : IArtifact
    {
        private static readonly Bitmap artifact = new Bitmap("moustache3.png");

        public void draw(Graphics g, Rectangle face)
        {
            float ratio = (float)face.Width / artifact.Width;
            Size newSize = new Size((int)(artifact.Width * ratio * 1.6), (int)(artifact.Height * ratio * 1.6));
            Point newPosition = new Point(face.X + (int)(face.Width * 0.97) - (int)(newSize.Width * 0.8), face.Y + (int)(face.Height * 0.9) - (int)(newSize.Height / 1.6));
            g.DrawImage(artifact, new Rectangle(newPosition, newSize));
        }
    }
}
