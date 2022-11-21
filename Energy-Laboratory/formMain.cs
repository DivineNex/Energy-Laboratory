using ScottPlot;
using ScottPlot.Drawing.Colormaps;
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
        private int _criticalDelta = 400;

        public formMain()
        {
            InitializeComponent();
        }

        private void formMain_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBox1.Text, out int value))
                _criticalDelta = value;
            ReadCSV(@"..\..\Data\GA03_1003.csv");
        }

        private void ReadCSV(string fileName)
        {
            List<string> criticalDeltas = new List<string>();

            using (StreamReader sr = new StreamReader(fileName))
            {
                string currentLine;
                string[] parsedLine;
                int index = 0;
                double? prevValue = null;

                while ((currentLine = sr.ReadLine()) != null)
                {
                    index++;
                    parsedLine = currentLine.Split(',');

                    if (parsedLine.Length == 2)
                    {
                        if (prevValue != null)
                        {
                            var delta = Double.Parse(parsedLine[1]) - prevValue;

                            if (delta >= _criticalDelta)
                                criticalDeltas.Add($"{index} {parsedLine[1]} {delta} {parsedLine[0]}");
                        }

                        bool result = Double.TryParse(parsedLine[1], out double value);
                        if (result)
                            prevValue = value;
                    }
                }

                dataGridView1.Rows.Clear();
                for (int i = 0; i < criticalDeltas.Count; i++)
                {
                    var parsedItem = criticalDeltas[i].Split(' ');
                    dataGridView1.Rows.Add(parsedItem[0], 
                        parsedItem[1], parsedItem[2], 
                        parsedItem[3] + parsedItem[4]);
                }
            }
        }
    }
}
