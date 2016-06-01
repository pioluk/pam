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
            Rectangle = rect;
        }
    }
}
