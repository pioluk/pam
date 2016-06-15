using System.Drawing;

namespace Pam.Artifacts
{
    internal class BearArtifact : IArtifact
    {
        private static readonly Bitmap artifact = new Bitmap("broda.png");

        public void draw(Graphics g, Rectangle face)
        {
            float ratio = (float)face.Width / artifact.Width;
            Size newSize = new Size((int)((artifact.Width * ratio)-0.5), (int)((artifact.Height * ratio)-0.5));
            Point newPosition = new Point(face.X, face.Y + (int)(face.Height -0.38) - (int)(newSize.Height / 1.6));
            g.DrawImage(artifact, new Rectangle(newPosition, newSize));
        }
       
    }
}
