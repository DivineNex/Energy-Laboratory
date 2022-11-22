using ScottPlot;
using ScottPlot.Drawing.Colormaps;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
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
            FindCriticalDeltaRecords(@"..\..\Data\GA03_1003.csv");
            //PlotATimeRangeChart(formsPlot1.Plot, @"..\..\Data\GA03_1003.csv", 
            //    new DateTime(2020, 03, 18, 03, 20, 0), 
            //    new DateTime(2020, 03, 18, 03, 30, 0));
            formsPlot1.Refresh();
        }

        private void FindCriticalDeltaRecords(string fileName)
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
                        parsedItem[3] + " " + parsedItem[4]);
                }
            }
        }

        private int PlotATimeRangeChart(ScottPlot.Plot plot, string fileName,  DateTime startDateTime, DateTime endDateTime, int startIndex = 0)
        {
            plot.Clear();
            using (StreamReader sr = new StreamReader(fileName))
            {
                string currentLine;
                string[] parsedLine;
                ScatterPlotList<double> valuesList = plot.AddScatterList();

                if (startIndex > 0)
                    sr.ReadLine().Skip(startIndex);

                int counter = 0; //временно

                while ((currentLine = sr.ReadLine()) != null)
                {
                    parsedLine = currentLine.Split(',');

                    if (parsedLine.Length == 2)
                    {
                        parsedLine[0] = parsedLine[0].Remove(0, 1);
                        parsedLine[0] = parsedLine[0].Remove(parsedLine[0].Length-1, 1);
                        var dateTime = DateTime.Parse(parsedLine[0]);

                        if (dateTime >= startDateTime)
                        {
                            if (dateTime <= endDateTime)
                            {
                                bool result = Double.TryParse(parsedLine[1], out double value);
                                if (result)
                                    valuesList.Add(counter, value);
                                counter++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                return valuesList.Count;
            }            
        }

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string dateStr = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();
            dateStr = dateStr.Remove(0, 1);
            dateStr = dateStr.Remove(dateStr.Length - 1, 1);
            DateTime date = DateTime.Parse(dateStr);
            DateTime startDateTime = date.AddMinutes(-60);
            DateTime endDateTime = date.AddMinutes(60);

            int startIndex = 0;
            bool result = Int32.TryParse(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString(), out startIndex);
            
            if (result)
            {
                Text = PlotATimeRangeChart(formsPlot1.Plot, @"..\..\Data\GA03_1003.csv", startDateTime, endDateTime, startIndex).ToString();
                formsPlot1.Refresh();
            }
            else
            {
                MessageBox.Show("Не удалось преобразовать стартовый индекс строки");
            }
        }
    }
}
