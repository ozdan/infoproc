using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infproc
{
    public interface IAlgo
    {
        int Run1();
        int[] GetPath();
    }

    public class Dijkstra : IAlgo
    {
        private int[,] matrix;
        private int s;
        private int t;
        private int P;
        private Marker[] l;
        // искать путь от одной вершины к другой?
        private bool ptop;
        private List<int> path = new List<int>();

        private struct Marker
        {
            public int Mark { get; set; }
            public bool Temporary { get; set; }
        }

        public Dijkstra(ref int[,] matrix, int s, int t)
        {
            this.matrix = matrix;
            this.s = s;
            this.t = t;
            l = new Marker[matrix.GetLength(0)];
            ptop = true;
        }

        public Dijkstra(ref int[,] matrix, int s)
            : this (ref matrix, s, 0) 
        {
            ptop = false;
        }

        /// <summary>
        /// Превращает массив меток в массив чисел
        /// </summary>
        /// <returns></returns>
        private int[] ArrMarkToArrInt()
        {
            int[] res = new int[l.Count()];
            int i = 0;
            foreach (var el in l)
            {
                if (el.Mark == int.MaxValue)
                {
                    res[i++] = 0;
                } 
                else
                {
                    res[i++] = el.Mark;
                }
            }
            return res;
        }

        /// <summary>
        /// Если ptop == true
        /// функция ведет поиск пути между 2мя вершинами
        /// Если ptop == false
        /// функция ведет поиск пути между 1 и остальными вершинами
        /// </summary>
        private void Run()
        {
            int n = l.Count();
            for (int i = 0; i < n; i++)
            {
                l[i].Mark = int.MaxValue;
                l[i].Temporary = true;
            }
            P = s;
            l[P].Mark = 0;

            if (ptop)
            {
                while (P != t)
                {
                    if (!Block())
                    {
                        break;
                    }
                }
            } 
            else
            {
                for (int k = 0; k < n; k++)
                {
                    if (!Block())
                    {
                        break;
                    }
                }
            }
            
        }

        /// <summary>
        /// Блок кода, одинаковый для поиска пути
        /// между 2мя вершинами
        /// и между 1 и остальными вершинами
        /// </summary>
        /// <returns></returns>
        private bool Block()
        {
            int n = l.Count();
            l[P].Temporary = false;
            int minw = int.MaxValue;
            int minn = -1;
            for (int i = 0; i < n; i++)
            {
                if (l[i].Temporary)
                {
                    int wnext = matrix[P, i];
                    if (matrix[P, i] != int.MaxValue)
                    {
                        wnext = wnext + l[P].Mark;
                    }
                    if (l[i].Mark > wnext)
                    {
                        l[i].Mark = wnext;
                    }
                    if (l[i].Mark < minw)
                    {
                        minw = l[i].Mark;
                        minn = i;
                    }
                }
            }
            if (minn < 0) return false;
            P = minn;
            return true;
        }

        /// <summary>
        /// Возвращает кратчайший путь от одной вершины к другой
        /// </summary>
        /// <returns></returns>
        public int Run1()
        {
            Run();
            if (t == P)
            {
                return l[P].Mark;
            } 
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Возвращает кратчайший путь от одной вершины к остальным
        /// </summary>
        /// <returns></returns>
        public int[] Run2()
        {
            Run();
            return ArrMarkToArrInt();
        }

        public int[] GetPath()
        {
            path.Add(t);
            PrevNode(t);
            return path.ToArray<int>();
        }

        private void PrevNode(int j)
        {
            if (s == j)
            {
                return;
            }
            else
            {
                for (int i = 0; i < l.Count(); i++)
                {
                    if ((l[i].Mark + matrix[i, j] == l[j].Mark) && (i != j) )
                    {
                        path.Add(i);
                        PrevNode(i);
                    }
                }
            }
        }

    }


    public class Floyd : IAlgo
    {
        private int[,] matrix;
        private int s;
        private int t;
        private int[,] who;
        private List<int> path = new List<int>();

        private int len;

        public Floyd(ref int[,] matrix, int s, int t)
        {            
            this.s = s;
            this.t = t;
            this.len = (int)Math.Sqrt(matrix.Length);
            this.matrix = new int[len, len];
            for (int i = 0; i < len; i++)
            {
                for (int j = 0; j < len; j++)
                {
                    this.matrix[i, j] = matrix[i, j];
                }
            }
            for (int i = 0; i < len; i++)
            {
                for (int j = 0; j < len; j++)
                {
                    if ((matrix[i, j] == 0) && (i != j))
                    {
                        this.matrix[i, j] = int.MaxValue;
                    }
                }
            }
            this.who = new int[len, len];
            for (int i = 0; i < len; i++)
            {
                for (int j = 0; j < len; j++)
                {
                    who[i, j] = i;
                }
            }
        }

        public Floyd(ref int[,] matrix, int s)
            : this (ref matrix, s, 0) { }


        private int k;

        public int Run1()
        {
            int result = Run();
            if (result == -1)
            {
                return 0;
            }
            else
            {
                return LenPath();
            }            
        }

        private int Run()
        {
            int result = -2;
            k = 0;
            do
            {
                TwoStep();
                ThreeStep();
                result = FourStep();
            } while (result == 0);
            return result;
        }

        public int[,] RunFull()
        {
            int result = Run();
            if (result == -1)
            {
                throw new Exception("Кривой граф");
            }
            else
            {
                for (int i = 0; i < len; i++)
                {
                    for (int j = 0; j < len; j++)
                    {
                        if (matrix[i, j] == int.MaxValue)
                        {
                            matrix[i, j] = 0;
                        }
                    }
                }
                return matrix;
            }  
        }

        private void TwoStep()
        {
            k = k + 1;
        }

        private void ThreeStep()
        {
            for (int i = 0; i < len; i++)
            {
                for (int j = 0; j < len; j++)
                {
                    if (((i != k) && (matrix[i, k] != int.MaxValue))
                        && ((j != k) && (matrix[k, j] != int.MaxValue)))
                    {
                        int sum = matrix[i, k] + matrix[k, j];
                        if (sum < matrix[i, j])
                        {
                            matrix[i, j] = Min(matrix[i, j], sum);
                            who[i, j] = who[k, j];
                        }
                    }
                }
            }
        }

        private int FourStep()
        {
            if (FF())
            {
                if (k < len - 1)
                {
                    return 0;
                }
                else
                {
                    if (k == len - 1)
                    {
                        return 1;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            else
            {
                return -1;
            }

        }

        private int LenPath()
        {
            if (matrix[s, t] != int.MaxValue)
            {
                return matrix[s, t];
            } 
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// В графе нет циклов отрицательного веса, содержащих вершину Хi?
        /// </summary>
        /// <returns>
        /// </returns>
        private bool FF()
        {
            for (int i = 0; i < len; i++)
            {
                if (matrix[i, i] < 0)
                {
                    return false;
                }
            }
            return true;
        }

        public int[] GetPath()
        {
            path.Add(t);
            PrevNode(t);
            return path.ToArray<int>();
        }

        private void PrevNode(int j)
        {
            if (s == j)
            {
                return;
            }
            else
            {
                path.Add(who[s, j]);
                PrevNode(who[s, j]);
            }
        }

        /// <summary>
        /// Определение минимального из двух чисел
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Min(int a, int b)
        {
            if (a > b)
            {
                return b;
            }
            else
            {
                return a;
            }
        }
    }
}
