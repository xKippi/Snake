using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    class Snake
    {
        public static bool Moved { get; set; }
        public bool CanRespawn { get; set; } = false;
        public bool StartedRespawnThread { get; set; } = false;
        private static char bodyChar = Game.BodyChar;
        private static char headChar = Game.HeadChar;
        private Direction dir;
        private Player player;
        private int lenght = 0;
        private bool isDead = false;
        private bool refreshedCoordiantes = false;
        private List<int>[] coordinates;
        public int Lenght
        {
            get { return lenght; }
            set { lenght = value; }
        }
        public bool IsDead
        {
            get { return isDead; }
            set { isDead = value; }
        }
        public List<int>[] Coordinates
        {
            get { return coordinates; }
            set { coordinates = value; }
        }
        public Direction Dir
        {
            get { return dir; }
            set { dir = value; }
        }

        public Snake(int left, int top, Player player)
        {
            int index = (Game.WindowWidth - 2) * (Game.WindowHeight - 5);
            coordinates = new List<int>[]
            {
                new List<int>(),
                new List<int>()
            };
            this.player = player;

            if (player.Score > player.Highscore)
                player.Highscore = player.Score;
            player.Score = 0;
            player.PrintScore();

            Console.ForegroundColor = player.Color;
            dir = Direction.Up;
            lenght = Game.StartLength;
            isDead = false;
            coordinates[0].Add(left);
            coordinates[1].Add(top);
            Console.SetCursorPosition(coordinates[0][0], coordinates[1][0]);
            Console.Write(headChar);
            for (int i = top + 1; i <= top + lenght; i++)
            {
                coordinates[0].Add(left);
                coordinates[1].Add(i);
                Console.SetCursorPosition(coordinates[0][i - top], i);
                Console.Write(bodyChar);
            }
            Console.ForegroundColor = Game.DefaultForegroundColor;
        }

        public void Move()
        {
            if (!refreshedCoordiantes)
            {
                Console.SetCursorPosition(coordinates[0][lenght], coordinates[1][lenght]);
                Console.Write(" ");
            }
            else
                refreshedCoordiantes = false;


            for (int i = lenght - 1; i >= 0; i--)
            {
                coordinates[0][i + 1] = coordinates[0][i];
                coordinates[1][i + 1] = coordinates[1][i];
            }

            int leftAdder = 0;
            int topAdder = 0;

            switch (dir)
            {
                //case Direction.Right: leftAdder = 0; topAdder = 0; break;
                case Direction.Left: leftAdder = -2; ; break;
                case Direction.Up: leftAdder = -1; topAdder = -1; break;
                case Direction.Down: leftAdder = -1; topAdder = 1; break;
            }

            Console.ForegroundColor = player.Color;
            Console.SetCursorPosition(coordinates[0][0], coordinates[1][0]);
            Console.Write(bodyChar);
            coordinates[0][0] = Console.CursorLeft + leftAdder;
            coordinates[1][0] = Console.CursorTop + topAdder;
            Console.SetCursorPosition(coordinates[0][0], coordinates[1][0]);
            Console.Write(headChar);
            Console.ForegroundColor = Game.DefaultForegroundColor;
        }

        public void Kill()
        {
            if (Game.DeleteCorps)
                Delete();
            isDead = true;
        }

        public void Delete()
        {
            for (int i = lenght; i >= 0; i--)
            {
                Console.SetCursorPosition(coordinates[0][i], coordinates[1][i]);
                Console.Write(" ");
            }
        }

        public void RefreshCoordiantes()
        {
            if (coordinates[0].Count - 1 < lenght)
            {
                for (int i = lenght - coordinates[0].Count; i >= 0; i--)
                {
                    coordinates[0].Add(0);
                    coordinates[1].Add(0);
                }
                refreshedCoordiantes = true;
            }
            else
                refreshedCoordiantes = false;
        }

        public static void HandleCollision(Snake snake1, Snake snake2)
        {
            if (snake1.coordinates[0][0] == Game.StarCoordinates[0][1] && snake1.coordinates[1][0] == Game.StarCoordinates[1][1])
            {
                snake1.lenght++;
                snake1.player.Score++;
                snake1.RefreshCoordiantes();
                Game.PrintStar(snake1, snake2);
                snake1.player.PrintScore();
            }

            int index = 0;
            int adder = 0;
            int leftAdder = 0;
            int topAdder = 0;
            int factor = 1;
            switch (snake1.dir)
            {
                case Direction.Right: index = 0; adder = Game.WindowWidth - 2; factor = -1; leftAdder = 1; break;
                case Direction.Left: index = 0; adder = -1; factor = 1; leftAdder = -1; break;
                case Direction.Up: index = 1; adder = -1; factor = 1; topAdder = -1; break;
                case Direction.Down: index = 1; adder = Game.WindowHeight - 5; factor = -1; topAdder = 1; break;
            }

            if (factor * snake1.coordinates[index][0] + adder < 1)
            {
                snake1.Kill();
            }

            for (int i = snake1.lenght - 1; i >= 1; i--)
            {
                if ((snake1.coordinates[0][0] + leftAdder == snake1.coordinates[0][i]) && (snake1.coordinates[1][0] + topAdder == snake1.coordinates[1][i]))
                {
                    snake1.Kill();
                }
            }
            if (!(snake1.isDead || snake2.isDead))
            {
                for (int i = Moved ? snake2.lenght : snake2.lenght - 1; i >= 1; i--)
                {
                    if ((snake1.coordinates[0][0] + leftAdder == snake2.coordinates[0][i]) && (snake1.coordinates[1][0] + topAdder == snake2.coordinates[1][i]))
                    {
                        snake2.player.Score += 2;
                        snake1.Kill();
                        snake2.player.PrintScore();
                    }
                }
                if (snake1.coordinates[0][0] + leftAdder == snake2.coordinates[0][0] && snake1.coordinates[1][0] + topAdder == snake2.coordinates[1][0])
                {
                    snake1.Kill();
                    snake2.Kill();
                }
            }
        }
    }
}