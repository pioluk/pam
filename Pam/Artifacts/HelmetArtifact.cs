using System.Drawing;

namespace Pam.Artifacts
{
    internal class HelmetArtifact : IArtifact
    {
        private static readonly Bitmap artifact = new Bitmap("helmet.png");

        public void draw(Graphics g, Rectangle face)
        {
            float ratio = (float)face.Width / artifact.Width;
            Size newSize = new Size((int)(artifact.Width * ratio * 1.5), (int)(artifact.Height * ratio));
            Point newPosition = new Point((int)(face.X - newSize.Width / 5.8), (int)(face.Y - newSize.Height * 0.95));
            g.DrawImage(artifact, new Rectangle(newPosition, newSize));
        }
    }
}
