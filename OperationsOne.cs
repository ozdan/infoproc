/// <summary>
/// Добавление графических элементов
/// Удаление графических элементов
/// Выбор графического элемента
/// Очистка выбранных элементов
/// Отрисовка всех элементов
/// Создание таблицы на форме
/// Установка "правильного" имени для вершины
/// Возврат незанятого номера для вершины
/// ColorElement - пояснения ниже
/// Отметка двух вершин
/// </summary>

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Infproc
{
    public partial class InfoForm
    {
        /// <summary>
        /// Добавление вершины
        /// </summary>
        /// <param name="e"></param>
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
                    break;
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
                //SetValidName();
                int frn = FreeNum();
                drawNodes[frn] = true;

                Node newNode = new Node(e.X, e.Y, maxXValue, maxYValue);
                string name = "x" + (frn + 1).ToString();
                newNode.Name = name;
                newNode.BackColor = Color.Blue;
                grList.Add(newNode);
                propertyGrid1.SelectedObject = newNode;
                propertyGrid1.Visible = true;
                numericUpDown1.UpButton();
                canva.Refresh();
            }
            else
            {
                MessageBox.Show(String.Format("Максимально возможное число вершин равно {0}", maxCountNodes));
            }
        }

        /// <summary>
        /// Удаление графического элемента
        /// </summary>
        /// <param name="e"></param>
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
                        //drawNodes[int.Parse(nd.Name.Replace("x", "")) - 1] = false;
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
            SetValidName();
            canva.Refresh();
        }

        /// <summary>
        /// Добавление дуги
        /// </summary>
        /// <param name="e"></param>
        private void AddArc(MouseEventArgs e)
        {
            CheckTwoNodes(e);
            if (checkedNodes.Count == 2)
            {
                // Если существует дуга с такими вершинами, сделать ее неориентированной
                for (int i = 0; i < grList.Count; i++)
                {
                    if (grList[i] is Arc)
                    {
                        Arc myArc = grList[i] as Arc;
                        if (myArc.MyNodes(checkedNodes[0], checkedNodes[1]))
                        {
                            myArc.Directed = true;
                            checkedNodes[0].BackColor = Color.Blue;
                            checkedNodes[1].BackColor = Color.Blue;
                            checkedNodes.Clear();
                            canva.Refresh();
                            return;
                        }

                    }
                }

                if (checkedNodes[0] != checkedNodes[1])
                {
                    Arc newArc = new Arc(checkedNodes[0], checkedNodes[1]);
                    newArc.BackColor = Color.Blue;
                    grList.Insert(0, newArc);
                    propertyGrid1.SelectedObject = newArc;
                    propertyGrid1.Visible = true;
                }
                ClearSelected();
            }
            canva.Refresh();
        }

        /// <summary>
        /// Выбор графического элемента
        /// </summary>
        /// <param name="e"></param>
        private void SelectGraph(MouseEventArgs e)
        {
            ClearSelected();
            int i;
            for (i = 0; i < grList.Count; i++)
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
                    propertyGrid1.Focus();
                    break;
                }
            }
            if (i == grList.Count)
            {
                propertyGrid1.Visible = false;
            }
            canva.Refresh();
        }

        private void ClearSelected()
        {
            checkedNodes.Clear();
            foreach (IGraph gr in grList)
            {
                gr.BackColor = Color.Blue;
            }
        }

        /// <summary>
        /// Создание таблицы на форме
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

        /// <summary>
        /// Установка "правильного" имени для вершины
        /// </summary>
        private void SetValidName()
        {
            int i = 0;
            foreach (IGraph gr in grList)
            {
                Node nd = gr as Node;
                if (nd != null)
                {
                    drawNodes[i] = true;
                    nd.Name = "x" + (++i).ToString();
                }
            }
            for (int j = i; j < maxCountNodes; j++)
            {
                drawNodes[j] = false;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flag">
        /// Флаг:
        /// если true - функция устанавливает новые координаты
        /// если false - функция проверяет координаты
        /// </param>
        /// <param name="color"></param>
        /// <param name="e"></param>
        private void ColorElement(bool flag, Color color, MouseEventArgs e)
        {
            foreach (var gr in grList)
            {
                Node nd = gr as Node;
                if (nd != null)
                {
                    if ((flag) && (captureNode != null))
                    {
                        captureNode.Coord = new Point(e.X, e.Y);
                        captureNode.BackColor = color;
                        foreach (var gr2 in grList)
                        {
                            Arc myArc = gr2 as Arc;
                            if (myArc != null)
                            {
                                if (myArc.MyNode(captureNode))
                                {
                                    myArc.BackColor = color;
                                }
                            }
                        }
                        break;
                    }
                    else
                    {
                        if (nd.ItI(e.X, e.Y))
                        {
                            captureNode = nd;
                            nd.BackColor = color;
                            foreach (var gr2 in grList)
                            {
                                Arc myArc = gr2 as Arc;
                                if (myArc != null)
                                {
                                    if (myArc.MyNode(nd))
                                    {
                                        myArc.BackColor = color;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
            canva.Refresh();
        }

        /// <summary>
        /// Отмечает 2 вершины
        /// </summary>
        /// <param name="e"></param>
        private void CheckTwoNodes(MouseEventArgs e)
        {
            if (checkedNodes.Count == 0)
            {
                ClearSelected();
            }

            for (int i = 0; i < grList.Count; i++)
            {
                if (grList[i].ItI(e.X, e.Y))
                {
                    Node nd = grList[i] as Node;
                    if (nd != null)
                    {
                        grList[i].BackColor = Color.Red;
                        if (checkedNodes.Count < 2)
                        {
                            checkedNodes.Add(nd);
                        }
                    }
                    else
                    {
                        continue;
                    }
                    canva.Refresh();
                    break;
                }
            }
        }

        /// <summary>
        /// Запуск алгоритма Дейкстры
        /// </summary>
        /// <param name="e"></param>
        private void RunAlgoD(MouseEventArgs e)
        {
            CheckTwoNodes(e);
            if (checkedNodes.Count == 2)
            {
                int s = int.Parse(checkedNodes[0].Name.Replace("x", "")) - 1;
                int t = int.Parse(checkedNodes[1].Name.Replace("x", "")) - 1;
                ClearSelected();
                Dijkstra algo = new Dijkstra(ref matrixIncident, s, t);
                RunAlgo(e, algo);
            }

        }

        /// <summary>
        /// Запуск алгоритма Флойда
        /// </summary>
        /// <param name="e"></param>
        private void RunAlgoF(MouseEventArgs e)
        {
            CheckTwoNodes(e);
            if (checkedNodes.Count == 2)
            {
                int s = int.Parse(checkedNodes[0].Name.Replace("x", "")) - 1;
                int t = int.Parse(checkedNodes[1].Name.Replace("x", "")) - 1;
                ClearSelected();
                Floyd algo = new Floyd(ref matrixIncident, s, t);
                RunAlgo(e, algo);
            }
        }

        /// <summary>
        /// Запуск алгоритма
        /// </summary>
        /// <param name="e"></param>
        /// <param name="algo">Конкретный алгоритм поиска кратчайшего пути</param>
        private void RunAlgo(MouseEventArgs e, IAlgo algo )
        {               
                string len = algo.Run1().ToString();
                textBox1.Text = len;
                if (len != "0")
                {
                    int[] mas = algo.GetPath();
                    List<Node> myN = new List<Node>();

                    foreach (var el in mas)
                    {
                        for (int i = 0; i < grList.Count; i++)
                        {
                            Node nd = grList[i] as Node;
                            if (nd != null)
                            {
                                int num = int.Parse(nd.Name.Replace("x", "")) - 1;
                                if (num == el)
                                {
                                    myN.Add(nd);
                                    grList[i].BackColor = Color.SeaGreen;
                                    break;
                                }
                            }
                        }
                    }

                    foreach (var gr in grList)
                    {
                        Arc arc = gr as Arc;
                        if (arc != null)
                        {
                            for (int i = 0; i < myN.Count - 1; i++)
                            {
                                if (arc.MyNodes(myN[i], myN[i + 1]))
                                {
                                    gr.BackColor = Color.SeaGreen;
                                    break;
                                }
                            }
                        }
                    }
                    myN.Clear();
                }
                else
                {
                    MessageBox.Show("Путь между заданными вершинами отсутствует");
                }

                canva.Refresh();
                //

        }

    }
}