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
        private delegate void CriticalDeltasHandler(List<string> strings);

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
            FindCriticalDeltaRecords(@"..\..\Data\GA03_1003.csv", FillCriticalDeltasToDGV);
            formsPlot1.Refresh();
        }

        private void FindCriticalDeltaRecords(string fileName, CriticalDeltasHandler deltasHandler = null)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                List<string> criticalDeltas = new List<string>(1000);
                string currentLine;
                string[] parsedLine = new string[2];
                int index = 0;
                double? prevValue = null;
                double? delta = 0;
                bool result = false;

                while ((currentLine = sr.ReadLine()) != null)
                {
                    index++;
                    parsedLine = currentLine.Split(',');

                    if (parsedLine.Length == 2)
                    {
                        if (prevValue != null)
                        {
                            delta = Double.Parse(parsedLine[1]) - prevValue;

                            if (delta >= _criticalDelta)
                            {
                                parsedLine[0] = parsedLine[0].Substring(1, parsedLine[0].Length - 2);
                                criticalDeltas.Add($"{index} {parsedLine[1]} {delta} {parsedLine[0]}");
                            }
                        }

                        result = Double.TryParse(parsedLine[1], out double value);
                        if (result)
                            prevValue = value;
                    }
                }

                deltasHandler?.Invoke(criticalDeltas);
            }
        }

        private void PlotATimeRangeChart(ScottPlot.Plot plot, string fileName,  DateTime startDateTime, DateTime endDateTime, int startIndex = 0)
        {
            plot.Clear();
            using (StreamReader sr = new StreamReader(fileName))
            {
                string currentLine;
                string[] parsedLine;
                ScatterPlotList<double> valuesList = plot.AddScatterList();
                bool result = false;
                DateTime dateTime;
                int counter = 0; //временно

                if (startIndex > 0)
                    sr.ReadLine().Skip(startIndex);

                while ((currentLine = sr.ReadLine()) != null)
                {
                    parsedLine = currentLine.Split(',');

                    if (parsedLine.Length == 2)
                    {
                        parsedLine[0] = parsedLine[0].Substring(1, parsedLine[0].Length - 2);
                        dateTime = DateTime.Parse(parsedLine[0]);

                        if (dateTime >= startDateTime)
                        {
                            if (dateTime <= endDateTime)
                            {
                                result = Double.TryParse(parsedLine[1], out double value);
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
            }            
        }

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string dateStr = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();
            DateTime date = DateTime.Parse(dateStr);
            DateTime startDateTime = date.AddMinutes(-2);
            DateTime endDateTime = date.AddMinutes(2);

            bool result = Int32.TryParse(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString(), out int startIndex);
            
            if (result)
            {
                PlotATimeRangeChart(formsPlot1.Plot, @"..\..\Data\GA03_1003.csv", startDateTime, endDateTime, startIndex);
                formsPlot1.Refresh();
            }
            else
            {
                MessageBox.Show("Не удалось преобразовать стартовый индекс строки");
            }
        }

        private void FillCriticalDeltasToDGV(List<string> strings)
        {
            for (int i = 0; i < strings.Count; i++)
            {
                var parsedItem = strings[i].Split(' ');
                dataGridView1.Rows.Add(parsedItem[0], parsedItem[1], parsedItem[2],
                                       parsedItem[3] + " " + parsedItem[4]);
            }
        }
    }
}
