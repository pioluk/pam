using System.Drawing;

namespace Pam.Artifacts
{
    internal class FunnyGlasses : IArtifact
    {
        private static readonly Bitmap artifact = new Bitmap("FunnyGlasses.png");

        public void draw(Graphics g, Rectangle face)
        {
            float ratio = (float)face.Width / artifact.Width;
            Size newSize = new Size((int)(artifact.Width * ratio), (int)(artifact.Height * ratio));
            Point newPosition = new Point(face.X, face.Y + face.Height / 2 - (int)(newSize.Height / 1.9)); 
            g.DrawImage(artifact, new Rectangle(newPosition, newSize));
        }
    }
}
