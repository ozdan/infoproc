/// <summary>
/// Копирование весов дуг из матрицы на форму
/// Копирование весов дуг из графа в матрицу
/// Копирование весов дуг с формы в матрицу
/// Копирование весов дуг с матрицы на граф
/// </summary>

using System;
using System.Collections.Generic;

namespace Infproc
{
    public partial class InfoForm
    {
        /// <summary>
        /// Копирование весов дуг из матрицы на форму
        /// </summary>
        private void MatrixToForm()
        {
            for (int i = 0; i < numColumns; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    dataGridView1[i, j].Value = matrixIncident[j, i];
                }
            }
        }

        /// <summary>
        /// Копирование весов дуг из графа в матрицу
        /// </summary>        
        private void GraphToMatrix()
        {
            for (int i = 0; i < numColumns; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    string nodesArc = string.Format("x{0}; x{1}", i + 1, j + 1);
                    foreach (var gr in grList)
                    {
                        Arc myArc = gr as Arc;
                        if (myArc != null)
                        {
                            if (nodesArc == myArc.Nodes)
                            {
                                if (myArc.Directed)
                                {
                                    matrixIncident[i, j] = myArc.Weight;
                                    matrixIncident[j, i] = myArc.Weight;
                                } 
                                else
                                {
                                    matrixIncident[i, j] = myArc.Weight;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Копирование весов дуг с формы в матрицу
        /// </summary>          
        private void FormToMatrix()
        {
            for (int i = 0; i < numColumns; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    int hpg = Convert.ToInt32(dataGridView1[j, i].Value);
                    if (hpg > 0)
                    {
                        matrixIncident[i, j] = hpg;

                    }
                }
            }
        }

        /// <summary>
        /// Копирование весов дуг с матрицы на граф
        /// </summary>                
        private void MatrixToGraph()
        {
            for (int i = 0; i < numColumns; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    string nodesArc = string.Format("x{0}; x{1}", i + 1, j + 1);
                    foreach (var gr in grList)
                    {
                        Arc myArc = gr as Arc;
                        if (myArc != null)
                        {
                            if (nodesArc == myArc.Nodes)
                            {
                                myArc.Weight = matrixIncident[i, j];
                            }
                        }
                    }
                }
            }
        }

        public static void GetMatrixIncident(ref int[,] matrix)
        {
            matrix = new int[numColumns, numColumns];
            for (int i = 0; i < numColumns; i++ )
            {
                for (int j = 0; j < numColumns; j++ )
                {
                    matrix[i, j] = matrixIncident[i, j];
                }
            }
        }

        public static int GetCount()
        {
            return count;
        }

    }
}