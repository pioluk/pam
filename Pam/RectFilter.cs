using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Pam
{
    internal struct RectFilter
    {

        public Rectangle Rectangle { get; private set; }

        public void add(Rectangle rect)
        {
            if (tooFar(rect))
                Rectangle = rect;
            else
            {
                const int factNum = 3;
                const int factDen = 5;
                const int oneMinFactNum = factDen - factNum;
                int left = (Rectangle.Left * factNum + rect.Left * oneMinFactNum) / factDen;
                int rigth = (Rectangle.Right * factNum + rect.Right * oneMinFactNum) / factDen;
                int top = (Rectangle.Top * factNum + rect.Top * oneMinFactNum) / factDen;
                int bottom = (Rectangle.Bottom * factNum + rect.Bottom * oneMinFactNum) / factDen;
                Rectangle = new Rectangle(left, top, rigth - left, bottom - top);
            }
        }

        private bool tooFar(Rectangle rect)
        {
            const int clNum = 1;
            const int clDen = 10;
            int mx_dx = Rectangle.Width * clNum / clDen;
            int mx_dy = Rectangle.Height * clNum / clDen;
            int dleft = Math.Abs(Rectangle.Left - rect.Left);
            int drigth = Math.Abs(Rectangle.Right - rect.Right);
            int dtop = Math.Abs(Rectangle.Top - rect.Top);
            int dbottom = Math.Abs(Rectangle.Bottom - rect.Bottom);
            return (dleft > mx_dx) || (drigth > mx_dx) || (dtop > mx_dy) || (dbottom > mx_dy);
        }

    }
}
