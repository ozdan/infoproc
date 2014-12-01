using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
//using System.Drawing.Drawing2D;

namespace Infproc
{
    public interface IGraph
    {
        void Draw(PaintEventArgs ev);
        bool ItI(int X, int Y);
        Color BackColor { set; }
    }

    /// <summary>
    /// Класс вершины графа
    /// </summary>
    public class Node : IGraph
    {
        private const int radius = 20;
        private int nodX;
        private int nodY;
        private Graphics dc;
        private string name;

        public Node(string name, int inX, int inY)
        {
            this.name = name;
            nodX = inX - radius;
            nodY = inY - radius;
        }

        /// <summary>
        /// Отрисовка фигуры
        /// </summary>
        public void Draw(PaintEventArgs ev)
        {
            dc = ev.Graphics;
            dc.FillEllipse(new SolidBrush(color), nodX, nodY, 2 * radius, 2 * radius);
        }

        /// <summary>
        /// Определение принадлежности точки вершине
        /// </summary>      
        public bool ItI(int X, int Y)
        {
            if (Math.Sqrt(Math.Pow((nodX + radius - X), 2) + Math.Pow((nodY + radius - Y), 2)) <= radius)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private Color color;
        /// <summary>
        /// Цвет вершины
        /// </summary>
        public Color BackColor
        {
            set { color = value; }
        }

        /// <summary>
        /// Центр вершины
        /// </summary>
        public Point Coord
        {
            get { return new Point(nodX + radius, nodY + radius); }
        }

        /// <summary>
        /// Радиус вершины
        /// </summary>
        public int Radius
        {
            get { return radius; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        //public override bool Equals(object obj)
        //{
        //    return obj.ToString() == this.ToString();
        //}

        public override string ToString()
        {
            return this.Coord.ToString();
        }

        ///// <summary>
        //////В обеих операциях сравнения второй параметр почему-то приходит нулевым
        ///// </summary>
        ///// <param name="p1"></param>
        ///// <param name="p2"></param>
        ///// <returns></returns>
        //public static bool operator ==(Node p1, Node p2)
        //{
        //    return p1.Equals(p2);
        //}

        //public static bool operator !=(Node p1, Node p2)
        //{
        //    return !p1.Equals(p2);
        //}
    }


    /// <summary>
    /// Класс дуги графа
    /// </summary>
    public class Arc : IGraph
    {
        //       private const int width = 10;
        private Graphics dc;
        private Node[] nodes = new Node[2];
        // параметры прямой
        private float coefK1, coefB1, coefK2;
        // координаты вершин
        private float x1, x2, y1, y2;
        private int width;
        private Color color;
        Point p1, p2, p3, p4;
        // ориентированность графа
        private bool directed;
        // вес дуги
        private byte weight;



        public Arc(Node P1, Node P2)
        {
            nodes[0] = P1;
            nodes[1] = P2;
            y1 = P1.Coord.Y;
            y2 = P2.Coord.Y;
            x1 = P1.Coord.X;
            x2 = P2.Coord.X;
            width = P1.Radius;
            coefK1 = (y1 - y2) / (x1 - x2);
            coefB1 = y1 - coefK1 * x1;
            coefK2 = -1 / coefK1;
        }

        /// <summary>
        /// Отрисовка фигуры
        /// </summary>
        public void Draw(PaintEventArgs ev)
        {
            dc = ev.Graphics;
            int xX = Convert.ToInt32(width * Math.Sin(90 - Math.Atan(coefK2)));
            int yY = Convert.ToInt32(width * Math.Cos(90 - Math.Atan(coefK2)));
            p1 = new Point((int)x1 - xX, (int)y1 - yY);
            p2 = new Point((int)x1 + xX, (int)y1 + yY);            

            if (directed)
            {
                p3 = new Point((int)x2 - xX, (int)y2 - yY);
                p4 = new Point((int)x2 + xX, (int)y2 + yY);
                Point[] pArray = { p1, p2, p3, p4 };
                //            dc.FillPolygon(new SolidBrush(color), pArray);
                dc.DrawPolygon(new Pen(color), pArray);
            }
            else
            {
                p3 = new Point((int)x2, (int)y2);
                Point[] pArray = { p1, p2, p3 };
                //            dc.FillPolygon(new SolidBrush(color), pArray);
                dc.DrawPolygon(new Pen(color), pArray);
            }
        }

        /// <summary>
        ////Определение принадлежности дуге
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public bool ItI(int X, int Y)
        {
            Point tP = new Point(X, Y);
            if (directed)
            {
                int xXx = (int)(nodes[0].Coord.X + nodes[0].Coord.X) / 2;
                int yYy = (int)(nodes[0].Coord.Y + nodes[0].Coord.Y) / 2;
                Point pPp = new Point(xXx, yYy);

                return EqualArea(p1, p2, pPp, tP) || EqualArea(p3, p4, pPp, tP);
            } 
            else
            {
                return EqualArea(p1, p2, p3, tP);
            }            
        }

        /// <summary>
        /// Получение площади треугольника по координатам трех точек
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns>Площадь треугольника</returns>
        private double Area(Point p1, Point p2, Point p3)
        {
            return Math.Abs((p2.X - p1.X) * (p3.Y - p1.Y) - (p3.X - p1.X) * (p2.Y - p1.Y)) / 2;
        }

        /// <summary>
        /// Принадлежность точки треугольнику
        /// </summary>
        /// <param name="p1">Вершина треугольника №1</param>
        /// <param name="p2">Вершина треугольника №2</param>
        /// <param name="p3">Вершина треугольника №3</param>
        /// <param name="pp">Проверяемая точка</param>
        /// <returns>Факт принадлежности</returns>
        private bool EqualArea(Point p1, Point p2, Point p3, Point pp)
        {
            double sS1 = Area(p1, p2, p3);
            double sS2 = Area(p1, p2, pp) + Area(p1, p3, pp) + Area(p2, p3, pp);

            // на случай погрешности
            if ((sS1 == sS2) || (sS1 == sS2 + 1) || (sS1 == sS2 - 1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Точки являются вершинами дуги?
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public bool MyNodes(Node n1, Node n2)
        {
            if (((n1.ToString() == nodes[0].ToString()) ||
                (n1.ToString() == nodes[1].ToString())) &&
                ((n2.ToString() == nodes[0].ToString()) ||
                (n2.ToString() == nodes[1].ToString()) ))
            {
                return true;
            } 
            else
            {
                return false;
            }
        }

        public bool MyNode(Node n)
        {
            if ((n.ToString() == this.nodes[0].ToString()) ||
                (n.ToString() == this.nodes[1].ToString()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Node GetSecondNode(Node n)
        {
            if (MyNode(n))
            {
                if (n.ToString() == this.nodes[0].ToString())
                {
                    return nodes[1];
                }
                else
                {
                    return nodes[0];
                }
            } 
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Цвет дуги
        /// </summary>
        public Color BackColor { set { color = value; } }

        /// <summary>
        /// Ориентированность дуги
        /// если true - неориентированная
        /// если false - ориентированная
        /// </summary>
        public bool Directed
        {
            get { return directed; }
            set { directed = value; }
        }

        /// <summary>
        /// Вес дуги
        /// </summary>
        public byte Weight
        {
            get { return weight; }
            set { weight = value; }
        }
    }
}
