﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace Snake
{
    class Snake
    {
        public Direction Dir { get; set; }
        public int Lenght { get; private set; }
        public bool CanRespawn { get; set; }
        public bool IsDead { get; private set; }
        private static char bodyChar = Game.BodyChar;
        private static char headChar = Game.HeadChar;
        private CollisionObject collisionSnake;
        private Player player;
        private List<Point> coords;
        private double scoreTmp;

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
            Dir = Direction.Up;
            collisionSnake = (CollisionObject)player.PlayerNumber;
            Lenght = Game.StartLength;
            IsDead = false;

            coords.Add(new Point(left,top));
            Game.Coordinates[left, top] = CollisionObject.SnakeHead;
            Console.SetCursorPosition(left, top);
            Console.Write(headChar);
            for (int i = top + 1; i <= top + Lenght; i++)
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
            if (coords.Count - 1 >= Lenght)
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
            
            int leftAdder = 0;
            int topAdder = 0;

            switch (Dir)
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

            CollisionObject replaceObject = Game.DeleteCorpse? CollisionObject.Nothing : CollisionObject.Corpse;
            for (int i = coords.Count - 1; i >= 0; i--)
                Game.Coordinates[coords[i].X, coords[i].Y] = replaceObject;

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
                Game.Coordinates[coords[i].X, coords[i].Y] = collisionSnake;
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

            switch (Game.Coordinates[snake1.coords[0].X + leftAdder, snake1.coords[0].Y + topAdder])
            {
                case CollisionObject.Wall: snake1.Kill(); return;
                case CollisionObject.SnakeHead: snake1.Kill(); snake2.Kill(); return;
                case CollisionObject.Star:
                    {
                        snake1.Lenght++;
                        snake1.player.Score++;
                        Game.Coordinates[snake1.coords[0].X, snake1.coords[0].Y] = CollisionObject.Nothing;
                        Game.PrintStar();
                        snake1.player.PrintScore();
                        return;
                    }
                case CollisionObject.Corpse: snake1.scoreTmp += Game.PointsPerDeadBodyPart; if (snake1.scoreTmp >= 1) { snake1.scoreTmp--; snake1.player.Score++; snake1.player.PrintScore(); } return;
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