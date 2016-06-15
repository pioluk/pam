using System.Drawing;

namespace Pam.Artifacts
{
    internal class MoustacheArtifact : IArtifact
    {
        private static readonly Bitmap artifact = new Bitmap("moustache.png");

        public void draw(Graphics g, Rectangle face)
        {
            float ratio = (float)face.Width / artifact.Width;
            Size newSize = new Size((int)(artifact.Width * ratio), (int)(artifact.Height * ratio));
            Point newPosition = new Point(face.X, face.Y + (int)(face.Height * 0.74) - (int)(newSize.Height / 1.8));
            g.DrawImage(artifact, new Rectangle(newPosition, newSize));
        }
    }
}
