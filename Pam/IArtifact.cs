using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pam
{
    internal interface IArtifact
    {
        void draw(Graphics g, Rectangle rect);
    }
}
