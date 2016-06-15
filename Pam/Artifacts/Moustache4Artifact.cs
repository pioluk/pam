using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pam
{
    class Moustache4Artifact : IArtifact
    {
        public void draw(Graphics g, Rectangle face)
        {
            int width = face.Width / 8;
            int height = face.Height / 8;
            int x = face.X + (face.Width - width) / 2;
            int y = face.Y + face.Height * 2 / 3;
            g.FillRectangle(Brushes.Black, new Rectangle(x, y, width, height));
        }
    }
}
