using System.Drawing;

namespace Pam
{
    internal interface IArtifact
    {
        void draw(Graphics g, Rectangle rect);
    }
}
