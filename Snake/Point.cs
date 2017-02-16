namespace Snake
{
    public struct Point
    {
        private int x;
        private int y;
        private bool isFilled;
        
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
            isFilled = true;
        }

        public bool IsEmpty
        {
            get { return !isFilled; }
        }

        public int X
        {
            get { return x; }
            set { x = value; isFilled = true; }
        }

        public int Y
        {
            get { return y; }
            set { y = value; isFilled = true; }
        }

        public static bool operator ==(Point left, Point right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Point)) return false;
            Point comp = (Point)obj;
            return comp.X == this.X && comp.Y == this.Y;
        }

        public override int GetHashCode()
        {
            return unchecked(x ^ y);
        }

        public void Offset(int dx, int dy)
        {
            x += dx;
            y += dy;
            isFilled = true;
        }

        public void Offset(Point p)
        {
            Offset(p.X, p.Y);
        }

        public override string ToString()
        {
            return "{X=" + X.ToString() + ",Y=" + Y.ToString() + "}";
        }
    }
}
