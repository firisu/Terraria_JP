using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace Terraria_JP
{
    public partial class Waiting : Form
    {
        public int time;

        public Waiting()
        {
            InitializeComponent();

            var asm = Assembly.GetExecutingAssembly();
            var stream = asm.GetManifestResourceStream("Terraria_JP.ajax-loader.gif");
            pictureBox1.Image = new Bitmap(stream);
            time = 0;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            time++;
            label2.Text = String.Format("{0} 秒経過", time);
        }
    }
}
