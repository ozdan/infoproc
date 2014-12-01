using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
using System.Linq;

namespace Infproc
{
    public partial class InfoForm : Form
    {
        // список вершин и дуг
        private List<IGraph> grList = new List<IGraph>();
        // выделенные вершины
        private List<Node> checkedNodes = new List<Node>();
        // максимальное число вершин
        private const int maxCountNodes = 10;
        // номер отмеченного радиофлажка
        private int checkedRadio;
        // первый запуск
        bool PrimaryState;
        // текущее количество колонок 
        private static int numColumns;
        // нарисованные вершины
        private bool[] drawNodes = new bool[maxCountNodes];
        // матрица инцидентности
        private static int[,] matrixIncident;
        // мой курсор (:
        private MyCursor myCur = new MyCursor();
        // захваченная вершина
        private Node captureNode;
        // максимальные значения координат канваса
        private int maxXValue;
        private int maxYValue;
        // счетчик для сравнений
        private static int count = 1000;

        public InfoForm()
        {
            InitializeComponent();
            PrimaryState = !PrimaryState;
            CreateMatrix();
            maxXValue = canva.Width;
            maxYValue = canva.Height;
        }

        private void canva_MouseClick(object sender, MouseEventArgs e)
        {
            switch (checkedRadio)
            {
                case 0:
                    AddNode(e);
                    break;
                case 1:
                    AddArc(e);
                    break;
                case 2:
                    RemoveGraph(e);
                    break;
                case 3:
                    SelectGraph(e);
                    break;
                case 4:
                    RunAlgoD(e);
                    break;
                case 5:
                    RunAlgoF(e);
                    break;
            }
        }

        /// <summary>
        /// Void function
        /// </summary>
        private void VoidFunction()
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            button3.BackColor = Color.Transparent;
            button7.BackColor = Color.Transparent;
            checkedRadio = 0;
            ClearSelected();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            button3.BackColor = Color.Transparent;
            button7.BackColor = Color.Transparent;
            checkedRadio = 1;
            ClearSelected();
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            button3.BackColor = Color.Transparent;
            button7.BackColor = Color.Transparent;
            checkedRadio = 2;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            button3.BackColor = Color.Transparent;
            button7.BackColor = Color.Transparent;
            checkedRadio = 3;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Value > 1)
            {
                CreateMatrix();
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            GraphToMatrix();
            MatrixToForm();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < numColumns; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    if ((string)dataGridView1[j, i].Value == string.Empty)
                    {
                        dataGridView1[j, i].Value = "0";
                    }
                }
            }
            FormToMatrix();
            MatrixToGraph();
        }


        private void canva_Paint(object sender, PaintEventArgs e)
        {
            foreach (var gr in grList)
            {
                gr.Draw(e);
            }
            if (captureNode != null)
            {
                myCur.Draw(e);
            }
        }

        private void canva_MouseDown(object sender, MouseEventArgs e)
        {
            if (checkedRadio == 3)
            {
                ColorElement(false, Color.White, e);
                myCur.Coord = new Point(e.X, e.Y);
                canva.Refresh();
            }
        }

        private void canva_MouseUp(object sender, MouseEventArgs e)
        {
            if (checkedRadio == 3)
            {
                ColorElement(true, Color.Red, e);
            }
            captureNode = null;
        }

        private void canva_MouseMove(object sender, MouseEventArgs e)
        {
            if (captureNode != null)
            {
                myCur.Coord = new Point(e.X, e.Y);
                canva.Refresh();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;
            radioButton4.Checked = false;
            button7.BackColor = Color.Transparent;
            button3.BackColor = Color.Green;

            ClearSelected();
            for (int g = 0; g < numColumns; g++)
            {
                for (int k = 0; k < numColumns; k++)
                {
                    matrixIncident[g, k] = int.MaxValue;
                }
            }
            GraphToMatrix();
            MessageBox.Show("Задайте вершины для поиска");
            checkedRadio = 4;

        }

        private void button4_Click(object sender, EventArgs e)
        {
            FileInfo f5 = new FileInfo("matrix.txt");
            using (StreamReader sr = f5.OpenText())
            {
                List<int> ls = new List<int>();
                string input = null;
                while ((input = sr.ReadLine()) != null)
                {
                    int ind = 0, len = 0;
                    bool ok = true;
                    while (ok)
                    {
                        len = input.IndexOf(" ", ind + 1);
                        if (len == -1)
                        {
                            ok = false;
                        }
                        else
                        {
                            string str = input.Substring(ind, len - ind);
                            ind = len;
                            int ccc = int.Parse(str);
                            ls.Add(ccc);
                        }

                    }
                }
                sr.Close();
                int count = (int)Math.Sqrt(ls.Count);
                numericUpDown1.Value = count;
                CreateMatrix();
                matrixIncident = new int[count, count];

                int i = 0, j = 0;
                foreach (var el in ls)
                {
                    matrixIncident[i, j++] = el;
                    if (j == count)
                    {
                        i++;
                        j = 0;
                    }
                }
                MatrixToForm();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            List<Node> nList = new List<Node>();
            List<Arc> aList = new List<Arc>();
            foreach (var gr in grList)
            {
                Node nd = gr as Node;
                if (nd != null)
                {
                    nList.Add(nd);
                    continue;
                }
                Arc ar = gr as Arc;
                if (ar != null)
                {
                    aList.Add(ar);
                    continue;
                }
            }
            if (nList.Count != 0)
            {
                XDocument graphDoc =
                new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XComment("Построенный ранее граф"),
                        new XElement("Graph",
                            from c in nList
                            select new XElement("Node",
                                new XElement("Name", c.Name),
                                new XElement("CoordX", c.Coord.X),
                                new XElement("CoordY", c.Coord.Y)
                            ),
                            from d in aList
                            select new XElement("Arc",
                                new XElement("Weight", d.Weight),
                                new XElement("Directed", d.Directed),
                                new XElement("Nodes", d.Nodes)
                        )
                        )
                );

                saveFileDialog1.Filter = "Файлы XML|*.xml|Все файлы|*.*";
                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.InitialDirectory = @".\";

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    graphDoc.Save(saveFileDialog1.FileName);
                }
            }
            else
            {
                MessageBox.Show("Граф пуст!");
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (grList.Count > 0)
            {
                Form2 f = new Form2();
                f.ShowDialog();
                if (f.DialogResult == DialogResult.Yes)
                {
                    button5_Click(sender, e);
                }
                if (f.DialogResult == DialogResult.Cancel)
                {
                    return;
                }
            }
            grList.Clear();
            numericUpDown1.Value = 0;
            for (int i = 0; i < drawNodes.Count(); i++)
            {
                drawNodes[i] = false;
            }
            openFileDialog1.Filter = "Файлы XML|*.xml";
            openFileDialog1.FileName = "";
            openFileDialog1.InitialDirectory = @".\";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //говнокод
                XDocument graphDoc = XDocument.Load(openFileDialog1.FileName);

                // загрузка характеристик узлов
                var coordX = from pl in graphDoc.Descendants("Node")
                             select pl.Element("CoordX").Value;
                var coordY = from pl in graphDoc.Descendants("Node")
                             select pl.Element("CoordY").Value;
                var names = from pl in graphDoc.Descendants("Node")
                            select pl.Element("Name").Value;
                //список x-координат
                List<int> xS = new List<int>();
                foreach (var c in coordX)
                {
                    xS.Add(int.Parse(c));
                }
                //список y-координат
                List<int> yS = new List<int>();
                foreach (var c in coordY)
                {
                    yS.Add(int.Parse(c));
                }
                //список имен
                List<string> nS = new List<string>();
                foreach (var c in names)
                {
                    nS.Add(c);
                }
                //создание узлов
                for (int i = 0; i < xS.Count(); i++)
                {
                    Node nd = new Node(xS[i], yS[i], maxXValue, maxYValue);
                    nd.Name = nS[i];
                    nd.BackColor = Color.Blue;
                    grList.Add(nd);
                    numericUpDown1.UpButton();
                    drawNodes[i] = true;
                }
                //загрузка характеристик дуг
                var weights = from pl in graphDoc.Descendants("Arc")
                              select pl.Element("Weight").Value;
                var directions = from pl in graphDoc.Descendants("Arc")
                                 select pl.Element("Directed").Value;
                var nodes = from pl in graphDoc.Descendants("Arc")
                            select pl.Element("Nodes").Value;
                //веса
                List<int> wS = new List<int>();
                foreach (var c in weights)
                {
                    wS.Add(int.Parse(c));
                }
                //ориентированность
                List<bool> dS = new List<bool>();
                foreach (var c in directions)
                {
                    dS.Add(bool.Parse(c));
                }
                //узлы
                List<string> nsS = new List<string>();
                foreach (var c in nodes)
                {
                    nsS.Add(c);
                }
                //создание дуг
                for (int i = 0; i < wS.Count(); i++)
                {
                    Node n1 = null, n2 = null;
                    bool counter = false;
                    for (int j = i; j < xS.Count + i; j++)
                    {
                        Node nd = grList[j] as Node;
                        if (!counter)
                        {
                            if (nsS[i].Substring(0, 2) == nd.Name)
                            {
                                n1 = nd;
                                counter = true;
                                // грязный хак: если нашли одну вершину, поиск второй начинается с начала 
                                //(происходит сбивание счетчика цикла for в самом цикле)
                                j = i - 1;
                                continue;
                            }
                        }
                        else
                        {
                            if (nsS[i].Substring(4, 2) == nd.Name)
                            {
                                n2 = nd;
                                break;
                            }
                        }
                    }
                    Arc arc = new Arc(n1, n2);
                    arc.Weight = wS[i];
                    arc.Directed = dS[i];
                    arc.BackColor = Color.Blue;
                    grList.Insert(0, arc);
                }
                canva.Refresh();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;
            radioButton4.Checked = false;
            button3.BackColor = Color.Transparent;
            button7.BackColor = Color.Green;

            ClearSelected();
            GraphToMatrix();
            MessageBox.Show("Задайте вершины для поиска");
            checkedRadio = 5;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            for (int g = 0; g < numColumns; g++)
            {
                for (int k = 0; k < numColumns; k++)
                {
                    matrixIncident[g, k] = int.MaxValue;
                }
            }
            GraphToMatrix();

            Form3 f = new Form3();
            f.ShowDialog();

        }

        private void nud_ValueChanged(object sender, EventArgs e)
        {
            count = Convert.ToInt32(nud.Value);
        }
    }
}
