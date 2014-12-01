using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Infproc
{
    public interface IGraph
    {
        void Draw(PaintEventArgs ev);
        bool ItI(int X, int Y);
        Color BackColor { get; set; }
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
        private int maxXValue;
        private int maxYValue;
        private Color color2;

        public Node(int inX, int inY, int maxX, int maxY)
        {
            nodX = inX - radius;
            nodY = inY - radius;
            maxXValue = maxX;
            maxYValue = maxY;
        }

        /// <summary>
        /// Отрисовка фигуры
        /// </summary>
        public void Draw(PaintEventArgs ev)
        {
            dc = ev.Graphics;
            dc.FillEllipse(new SolidBrush(color), nodX, nodY , 2 * radius, 2 * radius);
            Font font = new Font("Arial", radius - 4, FontStyle.Regular);
            dc.DrawString(name, font, new SolidBrush(color2), nodX, nodY);
        }

        /// <summary>
        /// Определение принадлежности точки вершине
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>

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
            set
            {
                color = value;
                if (value != Color.White)
                {
                    color2 = Color.Brown;
                }
                else
                {
                    color2 = value;
                }
            }
            get { return color; }
        }

        /// <summary>
        /// Центр вершины
        /// </summary>
        public Point Coord
        {
            get { return new Point(nodX + radius, nodY + radius); }
            set
            {
                bool edition = false;
                int cX = nodX + radius;
                int cY = nodY + radius;
                if ((value.X - radius < maxXValue) && (value.Y - radius > 0))
                {
                    nodX = value.X - radius;
                    edition = true;
                }
                if ((value.Y - radius < maxYValue) && (value.Y - radius > 0))
                {
                    nodY = value.Y - radius;
                    edition = true;
                }

                if ((CoordEdition != null) && (edition))
                {
                    CoordEdition(this, new NodeEventArgs("Установлены новые координаты"));
                }
            }
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

        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                return obj.ToString() == this.ToString();
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override string ToString()
        {
            return this.Coord.ToString();
        }

        /// <summary>
        ////В обеих операциях сравнения второй параметр почему-то приходит нулевым
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static bool operator ==(Node p1, Node p2)
        {
            try
            {
                return p1.Equals(p2);
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }

        public static bool operator !=(Node p1, Node p2)
        {
            try
            {
                return !p1.Equals(p2);
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }

        public delegate void NodeEventHandler(object sender, NodeEventArgs e);

        public event NodeEventHandler CoordEdition;


    }

    public class NodeEventArgs : EventArgs
    {
        public readonly string msg;
        public NodeEventArgs(string message)
        {
            msg = message;
        }
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
        private double coefK1, coefB1, coefK2, coefB21, coefB22;
        // координаты вершин
        private double x1, x2, y1, y2;
        private int width;
        private Color color, color2;
        // ориентированность графа
        private bool directed;
        // вес дуги
        private int weight;



        public Arc(Node n1, Node n2)
        {
            nodes[0] = n1;
            nodes[1] = n2;
            SetCoordAndParam();
            nodes[0].CoordEdition += new Node.NodeEventHandler(Arc_CoordEditionFirst);
            nodes[1].CoordEdition += new Node.NodeEventHandler(Arc_CoordEditionTwo);
        }

        void Arc_CoordEditionFirst(object sender, NodeEventArgs e)
        {
            Node nd = sender as Node;
            if (nd != null)
            {
                nodes[0] = nd;
                SetCoordAndParam();
            }
        }

        void Arc_CoordEditionTwo(object sender, NodeEventArgs e)
        {
            Node nd = sender as Node;
            if (nd != null)
            {
                nodes[1] = nd;
                SetCoordAndParam();
            }
        }

        private void SetCoordAndParam()
        {
            y1 = nodes[0].Coord.Y;
            y2 = nodes[1].Coord.Y;
            x1 = nodes[0].Coord.X;
            x2 = nodes[1].Coord.X;
            width = (int)nodes[0].Radius;
            coefK1 = (y1 - y2) / (x1 - x2);
            coefB1 = y1 - coefK1 * x1;
            coefK2 = -1 / coefK1;
            coefB21 = y1 - coefK2 * x1;
            coefB22 = y2 - coefK2 * x2;
        }

        /// <summary>
        /// Отрисовка фигуры
        /// </summary>
        public void Draw(PaintEventArgs ev)
        {
            dc = ev.Graphics;
            int dx, dy, mu = 5, ddx, ddy;
            int tx, ty;
            int x_x = (int)Math.Abs(x1-x2)/2;
            int y_y = (int)Math.Abs(y1-y2)/2;
            int rad = -nodes[0].Radius / 2;
            if (y2 < y1)
            {
                dy = -mu;
                ty = (int)y2;
                ddy = rad;
            }
            else
            {
                dy = 0;
                ty = (int)y1;
                ddy = -rad;
            }
            if (x2 < x1)
            {
                dx = -mu;
                tx = (int)x2;
                ddx = rad;
            }
            else
            {
                dx = 0;
                tx = (int)x1;
                ddx = -rad;
            }        
               
           
            if (!directed)
            {
                Pen myPen = new Pen(color, mu);
                System.Drawing.Drawing2D.GraphicsPath hPath = new System.Drawing.Drawing2D.GraphicsPath();
                
                hPath.AddLine(new Point(3, rad), new Point(0, -5));   //магические числа
                hPath.AddLine(new Point(-3, rad), new Point(0, -5));
                System.Drawing.Drawing2D.CustomLineCap hCap = new System.Drawing.Drawing2D.CustomLineCap(null, hPath);
                myPen.CustomEndCap = hCap;
                dc.DrawLine(myPen, (int)x1 + dx + ddx, (int)y1 + dy + ddy, (int)x2 + dx - ddx, (int)y2 + dy - ddy);
            }
            else
            {
                dc.DrawLine(new Pen(color, mu), (int)x1 + dx + ddx, (int)y1 + dy + ddy, (int)x2 + dx - ddx, (int)y2 + dy - ddy);
            }
            Font font = new Font("Arial", 16, FontStyle.Regular);
            dc.DrawString(weight.ToString(), font, new SolidBrush(color2), (int)(tx+x_x), (int)(ty+y_y));
        }

        /// <summary>
        ////Определение принадлежности дуге
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public bool ItI(int X, int Y)
        {
            int delta = 10;
            double yyy = coefK1 * X + coefB1;
            if ((yyy <= Y + delta) && (yyy >= Y - delta))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Пересечение функций
        /// </summary>
        /// <param name="k1"></param>
        /// <param name="b1"></param>
        /// <param name="k2"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        private Point Cross(double k1, double b1, double k2, double b2)
        {
            int x = (int)((b2 - b1) / (k1 - k2));
            int y = (int)(k2 * x + b2);

            return (new Point(x, y));
        }

        /// <summary>
        /// Точки являются вершинами дуги?
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="n2"></param>
        /// <returns></returns>
        public bool MyNodes(Node n1, Node n2)
        {
            if (((n1 == nodes[0]) || (n1 == nodes[1])) &&
                ((n2 == nodes[0]) || (n2 == nodes[1])))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Точка является одной из вершин?
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool MyNode(Node n)
        {
            if ((n == this.nodes[0]) || (n == this.nodes[1]))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Возвращает вторую вершину дуги
        /// </summary>
        /// <param name="n">Первая вершина дуги</param>
        /// <returns>
        /// Возвращает конечную вершину, если граф ориентированный
        /// Возвращает любую вторую вершину, если граф неориентированный
        /// Возвращает нулл, если граф ориентированный и входной параметр равен конечной вершине
        /// Генерирует исключение, если вершина не принадлежит графу
        /// </returns>
        public Node GetSecondNode(Node n)
        {
            if (MyNode(n))
            {
                if (!directed)
                {
                    if (n == this.nodes[0])
                    {
                        return nodes[1];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    if (n == this.nodes[0])
                    {
                        return nodes[1];
                    }
                    else
                    {
                        return nodes[0];
                    }
                }
            }
            else
            {
                throw new Exception("Вершина не принадлежит графу");
            }

        }

        /// <summary>
        /// Цвет дуги
        /// </summary>
        public Color BackColor
        {
            set
            {
                color = value;
                if (value != Color.White)
                {
                    color2 = Color.Brown;
                }
                else
                {
                    color2 = value;
                }
            }
            get { return color; }
        }

        /// <summary>
        /// Ориентированность дуги
        /// если true - ориентированная
        /// если false - неориентированная
        /// </summary>
        public bool Directed
        {
            get { return directed; }
            set { directed = value; }
        }

        /// <summary>
        /// Вес дуги
        /// </summary>
        public int Weight
        {
            get { return weight; }
            set
            {
                if ((value > -1) && (value < int.MaxValue))
                {
                    weight = value;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0}; {1}", nodes[0].Name, nodes[1].Name);
        }

        /// <summary>
        /// Строковая запись вершин графа
        /// </summary>
        public string Nodes
        {
            get { return this.ToString(); }
        }
    }

    /// <summary>
    /// Класс моего курсора
    /// </summary>
    public class MyCursor
    {
        private const int radius = 20;
        private int nodX;
        private int nodY;
        private Graphics dc;

        public MyCursor()
        {
            nodX = -20;
            nodY = -20;
        }

        public void Draw(PaintEventArgs ev)
        {
            dc = ev.Graphics;
            dc.DrawEllipse(new Pen(Color.Black), nodX, nodY, 2 * radius, 2 * radius);
        }

        public Point Coord
        {
            set
            {
                nodX = value.X - radius;
                nodY = value.Y - radius;
            }
        }

    }
}
