using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Infproc
{
    public partial class InfoForm : Form
    {
        // список вершин и дуг
        private List<IGraph> grList = new List<IGraph>();

        // выделенные вершины
        private List<IGraph> checkedNodes = new List<IGraph>();
        // максимальное число вершин
        private const int maxCountNodes = 10;
        // номер отмеченного радиофлажка
        private int checkedRadio;
        // первый запуск
        bool PrimaryState;
        // текущее количество колонок 
        private int numColumns;
        // нарисованные вершины
        private bool[] drawNodes = new bool[maxCountNodes];
        // матрица инцидентности
        private int[,] matrixIncident;


        public InfoForm()
        {
            InitializeComponent();
            PrimaryState = !PrimaryState;
            CreateMatrix();
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
            }
        }

        private void AddNode(MouseEventArgs e)
        {
            ClearSelected();
            bool OK = false;
            for (int i = 0; i < grList.Count; i++)
            {
                if (grList[i].ItI(e.X, e.Y))
                {
                    grList[i].BackColor = Color.Red;
                    OK = true;
                    continue;
                }
                else
                {
                    grList[i].BackColor = Color.Blue;
                }
            }
            if (OK)
            {
                canva.Refresh();
                return;
            }


            if (FreeNum() != -1)
            {
                SetValidName();
                int frn = FreeNum();
                drawNodes[frn] = true;

                Node newNode = new Node("x" + (frn + 1).ToString(), e.X, e.Y);
                newNode.BackColor = Color.Blue;
                grList.Add(newNode);
                numericUpDown1.UpButton();
                canva.Refresh();
            }
            else
            {
                MessageBox.Show(String.Format("Максимально возможное число вершин равно {0}", maxCountNodes));
            }
        }

        private void SetValidName()
        {
            int i = 0;
            foreach (IGraph gr in grList)
            {
                Node nd = gr as Node;
                if (nd != null)
                {
                    nd.Name = "x" + (++i).ToString();
                }
            }
        }

        /// <summary>
        /// Возвращает незанятый номер для вершины
        /// </summary>
        /// <returns></returns>
        private int FreeNum()
        {
            for (int i = 0; i < maxCountNodes; i++)
            {
                if (!drawNodes[i])
                {
                    return i;
                }
            }
            return -1;
        }

        private void RemoveGraph(MouseEventArgs e)
        {
            ClearSelected();
            Node nd = null;
            for (int i = 0; i < grList.Count; i++)
            {
                if (grList[i].ItI(e.X, e.Y))
                {
                    grList[i].BackColor = Color.White;
                    nd = grList[i] as Node;
                    if (nd != null)
                    {
                        drawNodes[int.Parse(nd.Name.Replace("x", ""))] = false;
                        numericUpDown1.DownButton();
                    }
                    grList.Remove(grList[i]);
                    break;
                }
            }

            // удаляем дуги у вершины
            if (nd != null)
            {
                for (int i = 0; i < grList.Count; i++)
                {
                    Arc arc = grList[i] as Arc;
                    if (arc != null)
                    {
                        if (arc.MyNode(nd))
                        {
                            arc.BackColor = Color.White;
                            grList.Remove(grList[i--]);
                        }
                    }
                }
            }

            canva.Refresh();
        }

        private void AddArc(MouseEventArgs e)
        {
            for (int i = 0; i < grList.Count; i++)
            {
                if (grList[i].ItI(e.X, e.Y))
                {
                    grList[i].BackColor = Color.Red;
                    if (checkedNodes.Count < 2)
                    {
                        checkedNodes.Add(grList[i]);
                        break;
                    }
                }
            }

            if (checkedNodes.Count == 2)
            {

                Node nd1 = checkedNodes[0] as Node;
                Node nd2 = checkedNodes[1] as Node;

                // Если существует дуга с такими вершинами, сделать ее неориентированной
                for (int i = 0; i < grList.Count; i++)
                {
                    if (grList[i] is Arc)
                    {
                        Arc myArc = grList[i] as Arc;
                        if (myArc.MyNodes(nd1, nd2))
                        {
                            myArc.Directed = true;
                            nd1.BackColor = Color.Blue;
                            nd2.BackColor = Color.Blue;
                            checkedNodes.Clear();
                            canva.Refresh();
                            return;
                        }

                    }
                }

                if ((nd1 != null) && (nd2 != null) && (nd1.ToString() != nd2.ToString()))
                {
                    Arc newArc = new Arc(nd1, nd2);
                    newArc.BackColor = Color.Blue;
                    grList.Add(newArc);
                }
                ClearSelected();
            }
            canva.Refresh();
        }

        private void SelectGraph(MouseEventArgs e)
        {
            ClearSelected();
            for (int i = 0; i < grList.Count; i++)
            {
                if (grList[i].ItI(e.X, e.Y))
                {
                    grList[i].BackColor = Color.Red;
                    Node nd = grList[i] as Node;
                    Arc arc = grList[i] as Arc;
                    if (nd != null)
                    {
                        propertyGrid1.SelectedObject = nd;
                        propertyGrid1.Visible = true;
                    }
                    if (arc != null)
                    {
                        propertyGrid1.SelectedObject = arc;
                        propertyGrid1.Visible = true;
                    }
                    break;
                }
            }
            canva.Refresh();
        }

        private void ClearSelected()
        {
            checkedNodes.Clear();
            propertyGrid1.Visible = false;
            foreach (IGraph gr in grList)
            {
                gr.BackColor = Color.Blue;
            }
        }

        private void VoidFunction()
        {

        }

        private void AddNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // nList.Add(new Node())
        }

        private void canva_Paint(object sender, PaintEventArgs e)
        {
            foreach (var gr in grList)
            {
                gr.Draw(e);
            }
        }


        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            checkedRadio = 0;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            checkedRadio = 1;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            checkedRadio = 2;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            checkedRadio = 3;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Value > 1)
            {
                CreateMatrix();
            }
        }

        /// <summary>
        /// Создание матрицы инцидентности
        /// </summary>
        private void CreateMatrix()
        {
            int n = (int)numericUpDown1.Value;
            matrixIncident = new int[n, n];
            if (!PrimaryState)
            {
                for (int i = 0; i < numColumns; i++)
                {
                    dataGridView1.Columns.RemoveAt(0);
                }
            }
            else
            {
                PrimaryState = !PrimaryState;
            }

            numColumns = n;

            for (int i = 0; i < n; i++)
            {
                string head = "x" + Convert.ToString(i + 1);
                dataGridView1.Columns.Add(head, head);
                dataGridView1.Columns[i].Width = 30;
            }
            if (n > 0) { dataGridView1.Rows.Add(n); }

            for (int i = 0; i < n; i++)
            {
                string head = "x" + Convert.ToString(i + 1);
                dataGridView1.Rows[i].HeaderCell.Value = head;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GraphToMatrix();
            MatrixToForm();
        }

        // запись полученных значений в форму
        private void MatrixToForm()
        {
            for (int i = 0; i < numColumns; i++ )
            {
                for (int j = 0; j < numColumns; j++ )
                {
                    dataGridView1[i, j].Value = matrixIncident[j, i];
                }
            }
        }

        // получение данных с графа
        private void GraphToMatrix()
        {
            List<Node> arrNodes = new List<Node>();
            List<Arc> arrArces = new List<Arc>();
            foreach (IGraph gr in grList)
            {
                Node nd = gr as Node;
                if (nd != null)
                {
                    arrNodes.Add(nd);
                    continue;
                }
                Arc arc = gr as Arc;
                if (arc != null)
                {
                    arrArces.Add(arc);
                }
            }
            int i = 0;
            foreach (Node nd in arrNodes)
            {
                i++;
                foreach (Arc arc in arrArces)
                {
                    if (arc.MyNode(nd))
                    {
                        for (int j = 0; j < arrNodes.Count; j++)
                        {
                            if (arrNodes[j].ToString() == arc.GetSecondNode(nd).ToString())
                            {
                                matrixIncident[i, j] = arc.Weight;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void FormToMatrix()
        {
            for (int i = 0; i < numColumns; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    matrixIncident[j, i] = Convert.ToInt32(dataGridView1[i, j].Value);
                }
            }
        }

        private void MatrixToGraph()
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            FormToMatrix();
            MatrixToGraph();
        }



    }
}
