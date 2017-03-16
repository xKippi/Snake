using System;
using System.Collections.Generic;
using System.Threading;

namespace Snake
{
    public class Snake
    {
        public Direction Dir { get; set; }
        public int Lenght { get; private set; }
        public bool CanRespawn { get; set; }
        public bool IsDead { get; private set; }
        private static char bodyChar = '\u25E6';
        private static char headChar = 'O';
        public static bool DeleteCorpse { get; set; } = true;
        public static double PointsPerDeadBodyPart { get; set; } = 0;
        private readonly CollisionObject collisionSnake;
        private Player player;
        private List<Point> coords;
        private double scoreTmp;
        private static bool firstSetHeadChar=true;
        private static bool firstSetBodyChar=true;

        public static char HeadChar
        {
            get { return headChar; }
            set { if (!firstSetHeadChar) throw new InvalidOperationException("HeadChar can be only set once"); headChar = value; firstSetHeadChar = false; }
        }
        public static char BodyChar
        {
            get { return bodyChar; }
            set { if (!firstSetBodyChar) throw new InvalidOperationException("BodyChar can be only set once"); bodyChar = value; firstSetBodyChar = false; }
        }

        public Snake(int left, int top, int length, Player player)
        {
            int index = (Game.WindowWidth - 2) * (Game.WindowHeight - 5);
            coords = new List<Point>();
            this.player = player;

            player.Score = 0;
            player.PrintScore();

            ConsoleColor colorBefore = Console.ForegroundColor;
            Console.ForegroundColor = player.Color;
            Dir = Direction.Up;
            collisionSnake = (CollisionObject)player.PlayerNumber;
            Lenght = length;
            IsDead = false;

            while (Game.CoordinateSystem[left, top] == CollisionObject.Star)
                Star.Print();
            coords.Add(new Point(left, top));
            Game.CoordinateSystem[left, top] = CollisionObject.SnakeHead;
            Console.SetCursorPosition(left, top);
            Console.Write(HeadChar);
            for (int i = top + 1; i <= top + Lenght; i++)
            {
                while (Game.CoordinateSystem[left, i] == CollisionObject.Star)
                    Star.Print();

                coords.Add(new Point(left, i));
                Game.CoordinateSystem[left, i] = collisionSnake;
                Console.SetCursorPosition(left, i);
                Console.Write(BodyChar);
            }
            Console.ForegroundColor = colorBefore;
        }

        public void Move()
        {
            if (coords.Count - 1 >= Lenght)
            {
                int index = coords.Count - 1;
                if (Game.CoordinateSystem[coords[index].X, coords[index].Y] == CollisionObject.SnakeHead)
                    coords.RemoveAt(index);
                else
                {
                    Console.SetCursorPosition(coords[index].X, coords[index].Y);
                    Game.CoordinateSystem[coords[index].X, coords[index].Y] = CollisionObject.Nothing;
                    coords.RemoveAt(index);
                    Console.Write(" ");
                }
            }
            
            int leftAdder = 0;
            int topAdder = 0;

            switch (Dir)
            {
                case Direction.Right: leftAdder = 1; break;
                case Direction.Left: leftAdder = -1; ; break;
                case Direction.Up: topAdder = -1; break;
                case Direction.Down: topAdder = 1; break;
            }

            ConsoleColor colorBefore = Console.ForegroundColor;
            Console.ForegroundColor = player.Color;
            Console.SetCursorPosition(coords[0].X, coords[0].Y);
            Game.CoordinateSystem[coords[0].X, coords[0].Y] = collisionSnake;
            Console.Write(BodyChar);

            coords.Insert(0, new Point(coords[0].X + leftAdder, coords[0].Y + topAdder));
            Console.SetCursorPosition(coords[0].X, coords[0].Y);
            Game.CoordinateSystem[coords[0].X, coords[0].Y] = CollisionObject.SnakeHead;
            Console.Write(HeadChar);
            Console.ForegroundColor = colorBefore;
        }

        public void Kill()
        {
            if (DeleteCorpse)
                Delete();

            CollisionObject replaceObject = DeleteCorpse? CollisionObject.Nothing : CollisionObject.Corpse;
            for (int i = coords.Count - 1; i >= 0; i--)
                Game.CoordinateSystem[coords[i].X, coords[i].Y] = replaceObject;

            new Thread(() =>
            {
                Thread.Sleep(2500);
                CanRespawn = true;
            }).Start();

            IsDead = true;
        }

        public void Delete()
        {
            for (int i = coords.Count-1; i >= 0; i--)
            {
                Console.SetCursorPosition(coords[i].X, coords[i].Y);
                Game.CoordinateSystem[coords[i].X, coords[i].Y] = collisionSnake;
                Console.Write(" ");
            }
        }

        public static void HandleCollision(Snake snake1, Snake snake2)
        {
            int leftAdder = 0;
            int topAdder = 0;
            switch (snake1.Dir)
            {
                case Direction.Right: leftAdder = 1; break;
                case Direction.Left: leftAdder = -1; break;
                case Direction.Up: topAdder = -1; break;
                case Direction.Down: topAdder = 1; break;
            }

            switch (Game.CoordinateSystem[snake1.coords[0].X + leftAdder, snake1.coords[0].Y + topAdder])
            {
                case CollisionObject.Wall: snake1.Kill(); return;
                case CollisionObject.SnakeHead: snake1.Kill(); snake2.Kill(); return;
                case CollisionObject.Star:
                    {
                        snake1.Lenght++;
                        snake1.player.Score++;
                        Game.CoordinateSystem[snake1.coords[0].X, snake1.coords[0].Y] = CollisionObject.Nothing;
                        Star.Print();
                        snake1.player.PrintScore();
                        return;
                    }
                case CollisionObject.Corpse: snake1.scoreTmp += PointsPerDeadBodyPart; if (snake1.scoreTmp >= 1) { snake1.scoreTmp--; snake1.player.Score++; snake1.player.PrintScore(); } return;
            }

            if(Game.CoordinateSystem[snake1.coords[0].X + leftAdder, snake1.coords[0].Y + topAdder] == snake1.collisionSnake)
            {
                snake1.Kill();
                return;
            }

            if (Game.CoordinateSystem[snake1.coords[0].X + leftAdder, snake1.coords[0].Y + topAdder] == snake2.collisionSnake)
            {
                if(!(snake1.player.PlayerNumber==1 && snake1.coords[0].X + leftAdder==snake2.coords[snake2.coords.Count-1].X && snake1.coords[0].Y + topAdder==snake2.coords[snake2.coords.Count-1].Y))
                {
                    snake1.Kill();
                    snake2.player.Score += 2;
                    snake2.player.PrintScore();
                }
            }
        }
    }
}