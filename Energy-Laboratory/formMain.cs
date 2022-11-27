using ScottPlot;
using ScottPlot.Drawing.Colormaps;
using ScottPlot.Plottable;
using ScottPlot.Renderable;
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
        public Dictionary<string, string> paramsFileNames = new Dictionary<string, string>()
        {
            { "Уровень нижнего бьефа", "ASU03_0.csv" },
            { "Уровень верхнего бьефа", "ASU03_1.csv" },
            { "Открытие направляющего аппарата", "ASU03_8.csv" },
            { "Напор", "ASU03_10.csv" },
            { "Активная мощность генератора", "ASU03_17.csv" },
            { "Реактивная мощность генератора", "ASU03_18.csv" },
            { "Абсолютная вибрация корпуса генераторного подшипника ЛБ рад..2А", "GA03_1003.csv" },
            { "Абсолютная вибрация корпуса генераторного подшипника ЛБ рад..СКЗ", "GA03_1005.csv" },
            { "Абсолютная вибрация корпуса генераторного подшипника НБ рад..2А", "GA03_1013.csv" },
            { "Абсолютная вибрация корпуса генераторного подшипника НБ рад..СКЗ", "GA03_1015.csv" },
            { "Абсолютная вибрация опоры подпятника ЛБ верт..2А", "GA03_1023.csv" },
            { "Абсолютная вибрация опоры подпятника ЛБ верт..СКЗ", "GA03_1025.csv" },
            { "Абсолютная вибрация опоры подпятника НБ верт..2А", "GA03_1033.csv" },
            { "Абсолютная вибрация опоры подпятника НБ верт..СКЗ", "GA03_1035.csv" },
            { "Абсолютная вибрация корпуса турбинного подшипника ЛБ рад..2А", "GA03_1043.csv" },
            { "Абсолютная вибрация корпуса турбинного подшипника ЛБ рад..СКЗ", "GA03_1045.csv" },
            { "Абсолютная вибрация корпуса турбинного подшипника НБ рад..2А", "GA03_1053.csv" },
            { "Абсолютная вибрация корпуса турбинного подшипника НБ рад..СКЗ", "GA03_1055.csv" },
            { "Биение вала в зоне генераторного подшипника ЛБ.2А", "GA03_1123.csv" },
            { "Биение вала в зоне генераторного подшипника НБ.2А", "GA03_1133.csv" },
            { "Биение зеркальной поверхности диска подпятника ЛБ.2А", "GA03_1143.csv" },
            { "Биение зеркальной поверхности диска подпятника НБ.2А", "GA03_1153.csv" },
            { "Биение вала в зоне турбинного подшипника ЛБ.2А", "GA03_1163.csv" },
            { "Биение вала в зоне турбинного подшипника НБ.2А", "GA03_1173.csv" }
        };

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

                sr.ReadLine(); //skip first line

                while ((currentLine = sr.ReadLine()) != null)
                {
                    index++;
                    parsedLine = currentLine.Split(',');

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

                deltasHandler?.Invoke(criticalDeltas);
            }
        }

        private void PlotATimeRangeChart(ScottPlot.Plot plot, string fileName, string chartLabel, DateTime startDateTime, DateTime endDateTime, int startIndex = 0, bool single = true)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                string currentLine;
                string[] parsedLine;
                DateTime dateTime;
                List<double> xData = new List<double>(10000);
                List<double> yData = new List<double>(10000);
                bool result;

                formsPlot1.Plot.XAxis.DateTimeFormat(true);

                sr.ReadLine(); //skip first line
                if (startIndex > 0)
                    sr.ReadLine().Skip(startIndex-1);

                while ((currentLine = sr.ReadLine()) != null)
                {
                    parsedLine = currentLine.Split(',');

                    parsedLine[0] = parsedLine[0].Substring(1, parsedLine[0].Length - 2);
                    dateTime = DateTime.Parse(parsedLine[0]);

                    if (dateTime >= startDateTime)
                    {
                        if (dateTime <= endDateTime)
                        {
                            result = Double.TryParse(parsedLine[1], out double value);

                            if (result)
                            {
                                xData.Add(dateTime.ToOADate());
                                yData.Add(value);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                try
                {
                    if (single)
                    {
                        var signalPlot = formsPlot1.Plot.AddSignalXY(xData.ToArray(), yData.ToArray(), color: Color.FromArgb(255, 31, 119, 180), label: chartLabel);

                        plot.Legend();
                        //plot.YAxis.Label(fileName);
                        plot.YAxis.Color(signalPlot.Color);
                        plot.XAxis.Color(signalPlot.Color);
                    }
                    else
                    {
                        //Заместо лейбла на оси Y лучше создавать легенду

                        var signalPlot = formsPlot1.Plot.AddSignalXY(xData.ToArray(), yData.ToArray(), label: chartLabel);
                        var yAxis = formsPlot1.Plot.AddAxis(ScottPlot.Renderable.Edge.Left);
                        signalPlot.YAxisIndex = yAxis.AxisIndex;
                        //yAxis.Label(fileName);
                        yAxis.Color(signalPlot.Color);
                        plot.Legend();
                    }
                }
                catch (Exception) {}
                

                formsPlot1.Refresh();

            }
        }

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string dateStr = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();
            DateTime date = DateTime.Parse(dateStr);
            DateTime startDateTime = date.AddMinutes(-(double)numericUpDown1.Value*60);
            DateTime endDateTime = date.AddMinutes((double)numericUpDown1.Value*60);

            bool result = Int32.TryParse(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString(), out int startIndex);
            
            if (result)
            {
                //Пока что хардкод, потому что только по одному параметру строим dgv
                string label = "Абсолютная вибрация корпуса генераторного подшипника ЛБ рад. 2А";
                PlotATimeRangeChart(formsPlot1.Plot, @"..\..\Data\GA03_1003.csv", label, startDateTime, endDateTime, startIndex, true);
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

        private void button2_Click(object sender, EventArgs e)
        {
            string[] collection = checkedListBox1.CheckedItems.Cast<string>().ToArray();
            if (collection.Length == 0) return;

            formsPlot1.Reset();

            if (collection.Length == 1 ) 
            {
                string fileName = paramsFileNames[collection[0]];
                string label = paramsFileNames.FirstOrDefault(x => x.Value == fileName).Key;
                PlotATimeRangeChart(formsPlot1.Plot, $"../../Data/{fileName}", label, dateTimePicker2.Value, dateTimePicker1.Value, single: true);
            }
            else
            {
                string fileName = paramsFileNames[collection[0]];
                string label = paramsFileNames.FirstOrDefault(x => x.Value == fileName).Key;

                PlotATimeRangeChart(formsPlot1.Plot, $"../../Data/{fileName}", label, dateTimePicker2.Value, dateTimePicker1.Value, single: true);

                for (int i = 1; i < collection.Length; i++)
                {
                    fileName = paramsFileNames[collection[i]];
                    label = paramsFileNames.FirstOrDefault(x => x.Value == fileName).Key;

                    PlotATimeRangeChart(formsPlot1.Plot, $"../../Data/{fileName}", label, dateTimePicker2.Value, dateTimePicker1.Value, single: false);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            formsPlot1.Reset();
            formsPlot1.Refresh();
        }
    }
}
