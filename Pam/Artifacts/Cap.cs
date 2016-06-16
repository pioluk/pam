using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Pam.Artifacts
{
    internal class Cap : IArtifact
    {
        private static readonly Bitmap capBase = new Bitmap("capBase.png");

        public void draw(Graphics g, Rectangle face)
        {
            drawBase(g, face);
        }

        private void drawBase(Graphics g, Rectangle face)
        {
            float ratio = (float)face.Width / capBase.Width * 1.1f;
            Size newSize = new Size((int)(capBase.Width * ratio), (int)(capBase.Height * ratio));
            Point newPosition = new Point(face.X + (face.Width / 2) - (newSize.Width / 2), (int)(face.Y - newSize.Height * 0.7));
            g.DrawImage(capBase, new Rectangle(newPosition, newSize));
        }
    }
}
