using System;

namespace Snake
{
    class Player
    {
        public Point ScoreCoords { get; set; }
        public int PlayerNumber { get; private set; }
        private int score;
        private int highscore;
        private readonly string name;
        private readonly ConsoleColor color;
        private Snake snake;

        public int Score
        {
            get { return score; }
            set { score = value; }
        }
        public int Highscore
        {
            get { return highscore; }
            set { highscore = value; }
        }
        public string Name
        {
            get { return name; }
        }
        public ConsoleColor Color
        {
            get { return color; }
        }
        public Snake Snake
        {
            get { return snake; }
            set { snake = value; }
        }


        public Player(string name, int playerNumber, ConsoleColor color)
        {
            this.color = color;

            if (playerNumber < 1 || playerNumber > Game.PlayerCount)
                throw new ArgumentOutOfRangeException("playerNumber");

            PlayerNumber = playerNumber;

            if (Game.AskForName)
            {
                Console.ForegroundColor = color;
                Console.Write("Player "+playerNumber);
                Console.ForegroundColor = Game.DefaultForegroundColor;
                Console.Write(" Name: ");
                this.name = Utils.Read(Game.MaxNameLength);
                Console.WriteLine();
                if (this.name.Length == 0) this.name = name;
            }
            else this.name = name;
        }

        public void PrintScore()
        {
            if (score > highscore)
                highscore = score;
            if (highscore > Game.MaxScore)
                return;
            string space = new string(' ', Game.MaxScore.ToString().Length);
            Console.SetCursorPosition(ScoreCoords.X, ScoreCoords.Y);
            Console.ForegroundColor = color;
            Console.Write(name + ":");
            Console.ForegroundColor = Game.DefaultForegroundColor;
            Console.SetCursorPosition(ScoreCoords.X+7, ScoreCoords.Y + 1);
            Console.Write(space);
            Console.SetCursorPosition(ScoreCoords.X, ScoreCoords.Y + 1);
            Console.Write("Score: {0}", score);
            Console.SetCursorPosition(ScoreCoords.X + 11, ScoreCoords.Y + 2);
            Console.Write(space);
            Console.SetCursorPosition(ScoreCoords.X, ScoreCoords.Y + 2);
            Console.Write("Highscore: {0}", highscore);
        }
    }
}