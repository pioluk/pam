using System;
using System.Drawing;

namespace Pam
{
    class RectUtils
    {
        public static double distanceFactor(Rectangle ro, Rectangle rn)
        {
            return (rectsDistance(ro, rn) + squaredCentersDistance(ro, rn)) / Math.Sqrt(ro.Width * ro.Height);
        }

        public static ulong rectsDistance(Rectangle a, Rectangle b)
        {
            int dl = a.Left - b.Left;
            int dr = a.Right - b.Right;
            int dt = a.Top - b.Top;
            int db = a.Bottom - b.Bottom;
            ulong ll = (ulong)(dl * dl);
            ulong rr = (ulong)(dr * dr);
            ulong tt = (ulong)(dt * dt);
            ulong bb = (ulong)(db * db);
            return ll + rr + tt + bb;
        }

        public static ulong squaredCentersDistance(Rectangle a, Rectangle b)
        {
            return squaredDistance(rectCenter(a), rectCenter(b));
        }

        public static Point rectCenter(Rectangle rect)
        {
            return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }

        public static ulong squaredDistance(Point a, Point b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            ulong xx = (ulong)(dx * dx);
            ulong yy = (ulong)(dy * dy);
            return xx + yy;
        }
    }
}
