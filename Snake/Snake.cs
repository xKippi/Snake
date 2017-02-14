using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Snake
{
    class Snake
    {
        public bool CanRespawn { get; set; } = false;
        private static char bodyChar = Game.BodyChar;
        private static char headChar = Game.HeadChar;
        private CollisionObject collisionSnake;
        private Direction dir;
        private Player player;
        private int lenght = 0;
        private bool isDead = false;
        private List<Point> coords;
        public int Lenght
        {
            get { return lenght; }
        }
        public bool IsDead
        {
            get { return isDead; }
        }
        public Direction Dir
        {
            get { return dir; }
            set { dir = value; }
        }

        public Snake(int left, int top, Player player)
        {
            int index = (Game.WindowWidth - 2) * (Game.WindowHeight - 5);
            coords = new List<Point>();
            this.player = player;

            if (player.Score > player.Highscore)
                player.Highscore = player.Score;
            player.Score = 0;
            player.PrintScore();

            Console.ForegroundColor = player.Color;
            dir = Direction.Up;
            collisionSnake = (CollisionObject)player.PlayerNumber;
            lenght = Game.StartLength;
            isDead = false;

            coords.Add(new Point(left,top));
            Game.Coordinates[left, top] = CollisionObject.SnakeHead;
            Console.SetCursorPosition(left, top);
            Console.Write(headChar);
            for (int i = top + 1; i <= top + lenght; i++)
            {
                coords.Add(new Point(left,i));
                Game.Coordinates[left, i] = collisionSnake;
                Console.SetCursorPosition(left, i);
                Console.Write(bodyChar);
            }
            Console.ForegroundColor = Game.DefaultForegroundColor;
        }

        public void Move()
        {
            if (coords.Count - 1 >= lenght)
            {
                int index = coords.Count - 1;
                if (Game.Coordinates[coords[index].X, coords[index].Y] == CollisionObject.SnakeHead)
                    coords.RemoveAt(index);
                else
                {
                    Console.SetCursorPosition(coords[index].X, coords[index].Y);
                    Game.Coordinates[coords[index].X, coords[index].Y] = CollisionObject.Nothing;
                    coords.RemoveAt(index);
                    Console.Write(" ");
                }
            }
            
            /*for (int i = lenght - 1; i >= 0; i--) //c[3]=c[2] c[2]=c[1] c[1]=c[0] 
            {
                coordinates[0][i + 1] = coordinates[0][i];
                coordinates[1][i + 1] = coordinates[1][i];
            }*/

            int leftAdder = 0;
            int topAdder = 0;

            switch (dir)
            {
                case Direction.Right: leftAdder = 1; break;
                case Direction.Left: leftAdder = -1; ; break;
                case Direction.Up: topAdder = -1; break;
                case Direction.Down: topAdder = 1; break;
            }

            Console.ForegroundColor = player.Color;
            Console.SetCursorPosition(coords[0].X, coords[0].Y);
            Game.Coordinates[coords[0].X, coords[0].Y] = collisionSnake;
            Console.Write(bodyChar);

            coords.Insert(0, new Point(coords[0].X + leftAdder, coords[0].Y + topAdder));
            Console.SetCursorPosition(coords[0].X, coords[0].Y);
            Game.Coordinates[coords[0].X, coords[0].Y] = CollisionObject.SnakeHead;
            Console.Write(headChar);
            Console.ForegroundColor = Game.DefaultForegroundColor;
        }

        public void Kill()
        {
            if (Game.DeleteCorpse)
                Delete();

            CollisionObject replaceObject = Game.EatingCorpseGivesPoints ? CollisionObject.Corpse : CollisionObject.Nothing;
            for (int i = coords.Count - 1; i >= 0; i--)
                Game.Coordinates[coords[i].X, coords[i].Y] = replaceObject;

            new Thread(() =>
            {
                Thread.Sleep(2500);
                CanRespawn = true;
            }).Start();

            isDead = true;
        }

        public void Delete()
        {
            for (int i = coords.Count-1; i >= 0; i--)
            {
                Console.SetCursorPosition(coords[i].X, coords[i].Y);
                Game.Coordinates[coords[i].X, coords[i].Y] = collisionSnake;
                Console.Write(" ");
            }
        }

        public static void HandleCollision(Snake snake1, Snake snake2)
        {
            int leftAdder = 0;
            int topAdder = 0;
            switch (snake1.dir)
            {
                case Direction.Right: leftAdder = 1; break;
                case Direction.Left: leftAdder = -1; break;
                case Direction.Up: topAdder = -1; break;
                case Direction.Down: topAdder = 1; break;
            }

            switch (Game.Coordinates[snake1.coords[0].X + leftAdder, snake1.coords[0].Y + topAdder])
            {
                case CollisionObject.Wall: snake1.Kill(); return;
                case CollisionObject.SnakeHead: snake1.Kill(); snake2.Kill(); return;
                case CollisionObject.Star:
                    {
                        snake1.lenght++;
                        snake1.player.Score++;
                        Game.Coordinates[snake1.coords[0].X, snake1.coords[0].Y] = CollisionObject.Nothing;
                        Game.PrintStar();
                        snake1.player.PrintScore();
                        return;
                    }
                case CollisionObject.Corpse: snake1.player.Score++; snake1.player.PrintScore(); return;
            }

            if(Game.Coordinates[snake1.coords[0].X + leftAdder, snake1.coords[0].Y + topAdder] == snake1.collisionSnake)
            {
                snake1.Kill();
                return;
            }

            if (Game.Coordinates[snake1.coords[0].X + leftAdder, snake1.coords[0].Y + topAdder] == snake2.collisionSnake)
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