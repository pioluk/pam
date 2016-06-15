using System.Drawing;

namespace Pam.Artifacts
{
    internal class Sombrero : IArtifact
    {
        private static readonly Bitmap artifact = new Bitmap("sombrero.png");

        public void draw(Graphics g, Rectangle face)
        {
            float ratio = (float)face.Width / artifact.Width;
            Size newSize = new Size((int)(artifact.Width * ratio), (int)(artifact.Height * ratio));
            Point newPosition = new Point(face.X, face.Y - newSize.Height);
            g.DrawImage(artifact, new Rectangle(newPosition, newSize));
        }
    }
}
