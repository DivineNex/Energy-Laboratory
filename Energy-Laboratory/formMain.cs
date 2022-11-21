using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Energy_Laboratory
{
    public partial class formMain : Form
    {
        public formMain()
        {
            InitializeComponent();
        }

        private void formMain_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ReadCSV(@"..\..\Data\GA03_1003.csv");
        }

        private void ReadCSV(string fileName)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                string currentLine;
                int index = 0;

                while ((currentLine = sr.ReadLine()) != null)
                {
                    //textBox1.Text += $"{currentLine}\r\n";
                    //textBox1.Update();
                    //Text = index++.ToString();
                }
            }
        }
    }
}
