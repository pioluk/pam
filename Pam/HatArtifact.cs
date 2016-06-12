using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pam
{
    internal class HatArtifact : IArtifact
    {
        private static readonly Bitmap artifact = new Bitmap("kapelusz.png");
    
        public void draw(Graphics g, Rectangle face)
        {
            float ratio = (float)face.Width / artifact.Width;
            Size newSize = new Size((int)(artifact.Width * 1.4 ), (int)(artifact.Height * 1.4 ));
            //Point newPosition = new Point(face.X, face.Y + face.Height / 2 - (int)(newSize.Height / 1.8));
            Point newPosition = new Point(face.X, face.Y - newSize.Height);
            g.DrawImage(artifact, new Rectangle(newPosition, newSize));
          
        }
    }
}
