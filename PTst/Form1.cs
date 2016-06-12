using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PTst
{
    public partial class Form1 : Form
    {

        public Rectangle rect1 = new Rectangle(256, 128, 128, 128);
        public Rectangle rect2 = new Rectangle(0, 0, 128, 128);
        public Bitmap bmp;

        public Form1()
        {
            InitializeComponent();
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = bmp;
            redraw();
        }

        public void redraw()
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.DrawRectangle(Pens.Blue, rect1);
                g.DrawRectangle(Pens.Green, rect2);
            }
            pictureBox1.Image = bmp;
        }

        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Console.Out.WriteLine("X = {0}  Y = {1}", e.X, e.Y);
            Point c = RectUtils.rectCenter(rect2);
            rect2.Offset(e.X - c.X, e.Y - c.Y);
            redraw();
            Console.Out.WriteLine("F = {0}", RectUtils.distanceFactor(rect1, rect2));
        }
    }
}
