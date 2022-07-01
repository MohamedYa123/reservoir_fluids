using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace App
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            chart1.Series.Clear();
            chart1.Series.Add("Chart");
            chart1.Series["Chart"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            chart2.Series.Clear();
            chart2.Series.Add("Chart");
            chart1.Series.Add("line1");
            chart2.Series.Add("Line");
            chart2.Series["Chart"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.IsMarginVisible = false;
            chart1.ChartAreas[0].AxisX.Interval = 2;
            comboBox1.SelectedIndex = 0;
            chart2.Series.Add("Y-function");
            chart1.Series.Add("line2");
        }
        List<double[]> inputs;
        flashtest fh;
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "Comma delimated Files|*.csv;*.txt";
            of.ShowDialog();
            if (of.FileName != "")
            {
                guna2TextBox1.Text = of.FileName;
                try
                {
                    inputs = readcsv(of.FileName);
                    fh = new flashtest(inputs);
                    refresh();
                    //  fill();
                }
                catch (Exception er)
                {
                    MessageBox.Show(er.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // refreshrecommended();

            }
        }
        void fill()
        {

            chart1.Series["Chart"].Points.Clear();
            foreach (var i in inputs)
            {
                chart1.Series["Chart"].Points.AddXY(i[vi], i[pi]);
            }
        }
        public void refresh()
        {
            foreach (var g in columns)
            {
                pressure.Items.Add(g);
                volume.Items.Add(g);
            }
            pressure.SelectedIndex = 0;
            volume.SelectedIndex = 1;
            // pressure.Text = columns[0];
            // volume.Text = columns[1];
        }
        string[] columns;
        List<double[]> readcsv(string read)
        {
            List<double[]> listA = new List<double[]>();

            using (var reader = new StreamReader(read))
            {
                int ii = 0;
                while (!reader.EndOfStream)
                {

                    var line = reader.ReadLine();
                    line = line.Replace(",", ";");
                    var values = line.Split(';');
                    if (ii == 0)
                    {
                        columns = values;
                    }
                    if (ii != 0)
                    {
                        double[] rg = new double[values.Length];
                        for (int i = 0; i < values.Length; i++)
                        {
                            var s = Convert.ToDouble(values[i]);
                            if (s < 0)
                            {
                                //MessageBox.Show("Negative values in input are forbiden ! ");
                                //  throw new Exception("Negative values in input are forbiden ! ");
                            }
                            else if (s == 0)
                            {
                                //  s = 0.00001;
                            }
                            rg[i] = s;
                        }
                        listA.Add(rg);
                    }
                    ii++;
                }
            }
            return listA;
        }
        double d;
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            d = Convert.ToDouble(numericUpDown1.Value);
            fh.report = "";
            fh.solve();
            listView1.Items.Clear();
            listView2.Items.Clear();
            listView1.Columns.Clear();
            listView2.Columns.Clear();
            guna2TextBox2.Text = fh.report;
            guna2TextBox2.Text += "\r\nYfunction line :" + fh.yfline.ToString();
            
            chart1.Series["line1"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            foreach (var l in fh.inputsline1)
            {
                chart1.Series["line1"].Points.AddXY(l[vi], fh.line1.calc(l[vi]));
                //   chart1.Series["line1"].Points[chart1.Series["line1"].Points.Count - 1].MarkerSize = 100;
            }
            
            chart2.Series["Y-function"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            int y = -1;
            foreach (var l in fh.yfunc)
            {
                y++;
                if (l == 0)
                {
                    continue;

                }
                chart2.Series["Y-function"].Points.AddXY(inputs[y][pi], l);

                //   chart1.Series["line1"].Points[chart1.Series["line1"].Points.Count - 1].MarkerSize = 100;
            }
            
            chart2.Series["Line"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            y = -1;
            foreach (var l in fh.yfunc)
            {
                y++;
                if (l == 0)
                {
                    continue;

                }
                chart2.Series["Line"].Points.AddXY(inputs[y][pi], fh.yfline.calc(inputs[y][pi]));

                //   chart1.Series["line1"].Points[chart1.Series["line1"].Points.Count - 1].MarkerSize = 100;
            }
            chart1.Series["line1"].Points.AddXY(fh.intercept[0], fh.line1.calc(fh.intercept[0]));
            
            chart1.Series["line2"].MarkerSize = 500;
            chart1.Series["line2"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            foreach (var l in fh.inputsline2)
            {
                chart1.Series["line2"].Points.AddXY(l[vi], fh.line2.calc(l[vi]));
                //  chart1.Series["line2"].Points[chart1.Series["line2"].Points.Count - 1].MarkerSize = 100;
            }
            chart1.Series["line2"].Points.AddXY(fh.intercept[0], fh.line2.calc(fh.intercept[0]));
            foreach (var c in columns)
            {
                listView1.Columns.Add(c);
            }
            listView1.Columns.Add("Rel volume");
            listView1.Columns.Add("y func");
            listView1.Columns.Add("density");
            int i = 0;
            foreach (var u in inputs)
            {
                ListViewItem lv = new ListViewItem();
                lv.Text = u[0] + "";

                for (var g = 1; g < u.Length; g++)
                {
                    lv.SubItems.Add(u[g] + "");
                }

                if (fh.relvols[i] <= 1)
                {
                    lv.SubItems.Add(fh.relvols[i] + "");
                }
                else
                {
                    var newrelv = 1 + (fh.intercept[1] - u[pi]) / (u[pi] * fh.yfline.calc(u[pi]));
                    lv.SubItems.Add(newrelv + "");
                }
                lv.SubItems.Add(fh.yfunc[i] + "");
                if (fh.relvols[i] <= 1)
                {
                    var dens = d / fh.relvols[i];
                    lv.SubItems.Add(dens + "");
                }
                else
                {
                    lv.SubItems.Add("-");
                }
                i++;
                listView1.Items.Add(lv);
            }
        }

        int pi;
        int vi;
        private void pressure_SelectedIndexChanged(object sender, EventArgs e)
        {
            fh.pressurecolumn = pressure.SelectedIndex;
            fh.volumecolumn = volume.SelectedIndex;
            pi = pressure.SelectedIndex;
            vi = volume.SelectedIndex;
            try
            {
                fill();
            }
            catch { }
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "Comma delimated Files|*.csv;*.txt";
            of.ShowDialog();
            if (of.FileName != "")
            {
                guna2TextBox4.Text = of.FileName;
                try
                {
                    inputs = readcsv(of.FileName);
                    fh = new flashtest(inputs);
                    refresh2();
                    //  fill();
                }
                catch (Exception er)
                {
                    MessageBox.Show(er.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // refreshrecommended();

            }
        }
        void refresh2()
        {
            foreach (Control c in panel4.Controls)
            {
                if (c.GetType() == comboBox1.GetType())
                {
                    try
                    {
                        var g = (ComboBox)c;
                        int indx = Convert.ToInt32(g.Name.Substring(1));
                        foreach (var cs in columns)
                        {
                            g.Items.Add(cs);
                        }
                        g.SelectedIndex = indx;
                    }
                    catch
                    {

                    }
                }
            }
        }
        private void guna2Button4_Click(object sender, EventArgs e)
        {
            double t = Convert.ToDouble(temp.Value);
            double vosc = Convert.ToDouble(voscc.Value);
            listView1.Items.Clear();
            listView2.Items.Clear();
            listView1.Columns.Clear();
            listView2.Columns.Clear();
            diffrentialtest dft = new diffrentialtest(inputs, p0.SelectedIndex, g1.SelectedIndex, g2.SelectedIndex, o3.SelectedIndex, i4.SelectedIndex, t, vosc);
            dft.solve();
            foreach (var c in columns)
            {
                listView2.Columns.Add(c);
            }
            listView2.Columns.Add("Bg");
            listView2.Columns.Add("Z");
            listView2.Columns.Add("Rsd");
            listView2.Columns.Add("BOD");
            listView2.Columns.Add("BtD");
            listView2.FullRowSelect = true;
            for (int i = 0; i < inputs.Count; i++)
            {
                ListViewItem lv = new ListViewItem();
                lv.Text = inputs[i][0] + "";
                for (int i2 = 1; i2 < inputs[i].Length; i2++)
                {
                    lv.SubItems.Add(inputs[i][i2] + "");
                }
                lv.SubItems.Add(dft.bg[i] + "");
                lv.SubItems.Add(dft.z[i] + "");
                lv.SubItems.Add(dft.Rsd[i] + "");
                lv.SubItems.Add(dft.BOD[i] + "");
                lv.SubItems.Add(dft.BtD[i] + "");
                listView2.Items.Add(lv);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedItem.ToString())
            {
                case "Diffrential test":
                    panel4.Show();
                    panel3.Hide();
                    panel6.Hide();
                    break;
                case "Flash test":
                    panel4.Hide();
                    panel3.Show();
                    panel6.Hide();
                    break;
                case "Seprator test":
                    panel6.Show();
                    panel3.Hide();
                    panel4.Hide();
                    break;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
          //  string c = AppDomain.CurrentDomain.BaseDirectory  + AppDomain.CurrentDomain.FriendlyName;
            Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
        }
        void refresh3()
        {

        }
        private void guna2Button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "Comma delimated Files|*.csv;*.txt";
            of.ShowDialog();
            if (of.FileName != "")
            {
                guna2TextBox4.Text = of.FileName;
                try
                {
                    inputs = readcsv(of.FileName);
                    fh = new flashtest(inputs);
                    refresh3();
                    //  fill();
                }
                catch (Exception er)
                {
                    MessageBox.Show(er.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void panel7_Paint(object sender, PaintEventArgs e)
        {

        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {

        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {
            decimal[] vs = { v1.Value, v2.Value, v3.Value, v4.Value, v5.Value, v6.Value, v7.Value, v8.Value, v9.Value };
            sepratortest sp = new sepratortest(vs);
            sp.solve();
            reportsp.Text = sp.report;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            label15.Text = "Volume of seperator liquid at "+ numericUpDown4.Value + " psig & "+ numericUpDown7 .Value+ " F";
            if (numericUpDown5.Value == 0)
            {
                label19.Text = "Volume of Stock tank liquid at "+ numericUpDown5.Value + " psig & "+ numericUpDown8.Value + " F";
            }
            else
            {
                label19.Text = "Volume of Separator liquid at " + numericUpDown5.Value + " psig & " + numericUpDown8.Value + " F";
            }
            
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 f = new Form2();
            f.ShowDialog();
        }
    }
}
