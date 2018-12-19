using System;
using System.Windows.Forms;

namespace Backtrace.WinFoms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            throw new Exception("Exception occurs after button click");
        }
    }
}
