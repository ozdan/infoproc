using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Infproc
{
    public partial class Form3 : Form
    {
        int count;
        // матрица инцидентности
        private int[,] matrixIncident;

        public Form3()
        {
            InitializeComponent();
            InfoForm.GetMatrixIncident(ref matrixIncident);
            count = InfoForm.GetCount();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            int n = matrixIncident.GetLength(0);

            for (int i = 0; i < n; i++)
            {
                string head = "x" + Convert.ToString(i + 1);
                dataGridView1.Columns.Add(head, head);
                dataGridView1.Columns[i].Width = 30;
                dataGridView2.Columns.Add(head, head);
                dataGridView2.Columns[i].Width = 30;
            }
            if (n > 0)
            {
                dataGridView1.Rows.Add(n);
                dataGridView2.Rows.Add(n);
            }

            for (int i = 0; i < n; i++)
            {
                string head = "x" + Convert.ToString(i + 1);
                dataGridView1.Rows[i].HeaderCell.Value = head;
                dataGridView2.Rows[i].HeaderCell.Value = head;
            }
            Process p = Process.GetCurrentProcess();
            TimeSpan ts;

            ts = p.TotalProcessorTime;
            for (int g = 0; g < count; g++)
            {
                for (int i = 0; i < n; i++)
                {
                    Dijkstra algo = new Dijkstra(ref matrixIncident, i);
                    int[] len = algo.Run2();
                    for (int j = 0; j < n; j++ )
                    {
                        dataGridView1[j, i].Value = len[j];
                    }                    
                }
            }
            ts = p.TotalProcessorTime - ts;
            label3.Text += (ts.Seconds.ToString() + " с " + ts.Milliseconds + " мс");


            ts = p.TotalProcessorTime;
            for (int g = 0; g < count; g++)
            {
                Floyd f1 = new Floyd(ref matrixIncident, 0);
                int[,] result = f1.RunFull();
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        dataGridView2[j, i].Value = result[i, j];
                    }
                }
            }
            ts = p.TotalProcessorTime - ts;
            label4.Text += (ts.Seconds.ToString() + " с " + ts.Milliseconds + " мс");

        }
    }
}
