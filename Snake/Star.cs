using System;

namespace Snake
{
    public static class Star
    {
        private static bool firstSetColor = true;
        private static bool firstSetChar = true;

        private static char starChar='*';
        public static char Char
        {
            get { return starChar; }
            set { if (!firstSetChar) throw new InvalidOperationException("Star char can be only set once"); starChar = value; firstSetChar = false; }
        }

        private static ConsoleColor color = ConsoleColor.Yellow;
        public static ConsoleColor Color
        {
            get { return color; }
            set { if (!firstSetColor) throw new InvalidOperationException("Color can be only set once"); color = value; firstSetColor = false; }
        }

        private static Point coords;
        public static Point Coordinates
        {
            get { return coords; }
            set { coords = value; }
        }
        
        public static void Print()
        {
            Random randy = new Random(1337);
            ConsoleColor colorBefore = Console.ForegroundColor;

            while (Game.CoordinateSystem[coords.X, coords.Y] != CollisionObject.Nothing)
            {
                coords = new Point(randy.Next(1, Game.WindowWidth - 1), randy.Next(1, Game.WindowHeight - 4));
            }
            Console.ForegroundColor = color;
            Console.SetCursorPosition(coords.X, coords.Y);
            Game.CoordinateSystem[coords.X, coords.Y] = CollisionObject.Star;
            Console.Write(starChar);
            Console.ForegroundColor = colorBefore;
        }
    }
}
