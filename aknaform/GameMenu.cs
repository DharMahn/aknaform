using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace aknaform
{
    public partial class GameMenu : Form
    {
        public GameMenu()
        {
            InitializeComponent();
            ActiveControl = button1;
        }
        public int W;
        public int H;
        public int B;
        private void button1_Click(object sender, EventArgs e)
        {

            W = (int)numericUpDown1.Value;
            H = (int)numericUpDown2.Value;
            B = (int)numericUpDown3.Value;
            if (W * H < B + 9)
            {
                MessageBox.Show("A bombák számának kisebbnek kell lennie, mint a cellák számának mínusz 9!");
                numericUpDown3.Value = W * H - 9;
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void GameMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            numericUpDown1.Value = 9;
            numericUpDown2.Value = 9;
            numericUpDown3.Value = 10;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            numericUpDown1.Value = 16;
            numericUpDown2.Value = 16;
            numericUpDown3.Value = 40;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            numericUpDown1.Value = 30;
            numericUpDown2.Value = 16;
            numericUpDown3.Value = 99;
        }

    }
}
