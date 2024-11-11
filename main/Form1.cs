using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace main
{
    public partial class Form1 : Form
    {
        int methodCircuitNum = 0;
        int methodPaintingNum = 1;
        bool backInfo = true;
        public Form1()
        {
            InitializeComponent();
            string[] methodCircuit = { "A", "B", "C" };
            string[] methodPainting = { "A", "B", "C" };
            listBox1.Items.AddRange(methodCircuit);
            listBox2.Items.AddRange(methodPainting);
            listBox1.SelectedIndex = methodCircuitNum;
            listBox2.SelectedIndex = methodPaintingNum;
            checkBox1.Checked = backInfo;
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            methodCircuitNum = listBox1.SelectedIndex;
        }

        private void ListBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            methodPaintingNum = listBox2.SelectedIndex;
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            backInfo = checkBox1.Checked;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            var myForm = new Form2(methodCircuitNum, methodPaintingNum, backInfo);
            myForm.Show();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            var myForm2 = new Form3();
            myForm2.Show();
        }
    }
}
