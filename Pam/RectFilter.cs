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
}
