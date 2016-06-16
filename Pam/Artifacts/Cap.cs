using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Pam.Artifacts
{
    internal class Cap : IArtifact
    {
        private static readonly Bitmap capBase = new Bitmap("capBase.png");

        public void draw(Graphics g, Rectangle face)
        {
            Rectangle drawRect = calcDrawRect(face);
            g.DrawImage(capBase, drawRect);
            drawAirscrew(g, drawRect);
        }

        private Rectangle calcDrawRect(Rectangle face)
        {
            float ratio = (float)face.Width / capBase.Width * 1.1f;
            Size newSize = new Size((int)(capBase.Width * ratio), (int)(capBase.Height * ratio));
            Point newPosition = new Point(face.X + (face.Width / 2) - (newSize.Width / 2), (int)(face.Y - newSize.Height * 0.7));
            return new Rectangle(newPosition, newSize);
        }

        private void drawAirscrew(Graphics g, Rectangle drawRect)
        {
            int xc = drawRect.Width / 2;
            int r1 = xc;
            int r0 = r1 / 10;

            float pos;
            bool flip = calcPosition(out pos);
            float apos = Math.Abs(pos);

            int x1 = (int)((1.0f - apos) * -r0);
            int x2 = (int)(r1 * apos);

            if(pos < 0.0f)
            {
                int t = x1;
                x1 = -x2;
                x2 = -t;
            }

            int bx1 = -x2;
            int bx2 = -x1;

            Size asSize = new Size(x2 - x1, drawRect.Height / 10);
            int yy = drawRect.Y + drawRect.Height / 10;
            Point aasp = new Point(drawRect.X + xc + x1, yy);
            Point basp = new Point(drawRect.X + xc + bx1, yy);
            Rectangle aasr = new Rectangle(aasp, asSize);
            Rectangle basr = new Rectangle(basp, asSize);

            if(flip)
                g.FillEllipse(Brushes.GreenYellow, basr);
            g.FillEllipse(Brushes.IndianRed, aasr);
            if(!flip)
                g.FillEllipse(Brushes.GreenYellow, basr);
        }

        private bool calcPosition(out float pos)
        {
            const uint S = 2048;
            uint time = GetTickCount() % S;
            double phase = time * Math.PI * 2 / S;
            pos = (float)Math.Cos(phase);
            return time >= (S / 2);
        }

        [DllImport("Kernel32.dll", EntryPoint = "GetTickCount", ExactSpelling = true, SetLastError = false)]
        private static extern uint GetTickCount();

    }
}
