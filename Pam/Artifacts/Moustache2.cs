using System.Drawing;

namespace Pam.Artifacts
{
    internal class Moustache2 : IArtifact
    {
        private static readonly Bitmap artifact = new Bitmap("moustache2.png");

        public void draw(Graphics g, Rectangle face)
        {
            float ratio = (float)face.Width / artifact.Width;
            Size newSize = new Size((int)(artifact.Width * ratio), (int)(artifact.Height * ratio));
            Point newPosition = new Point(face.X + (int)(face.Width) - (int)(newSize.Width), face.Y + (int)(face.Height * 0.9) - (int)(newSize.Height / 1.5)); 
            g.DrawImage(artifact, new Rectangle(newPosition, newSize));
        }
    }
}
