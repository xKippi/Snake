using System;

namespace Snake
{
    class Player
    {
        public Point ScoreCoords { get; set; }
        public Snake Snake { get; set; }
        private ConsoleKey[] controlKeys;
        private readonly ConsoleColor color;
        private readonly string name;
        private readonly int playerNumber;
        private int score;
        private int highscore;

        public int PlayerNumber
        {
            get { return playerNumber; }
        }
        public int Score
        {
            get { return score; }
            set { if (score <= Game.MaxScore) score = value; }
        }
        public int Highscore
        {
            get { return highscore; }
            set { if(highscore <= Game.MaxScore) highscore = value; }
        }
        public string Name
        {
            get { return name; }
        }
        public ConsoleColor Color
        {
            get { return color; }
        }
        public ConsoleKey[] ControlKeys
        {
            get { return controlKeys; }
            set { controlKeys = value; }
        }


        public Player(string name, int playerNumber, ConsoleColor color)
        {
            this.color = color;

            if (playerNumber < 1 || playerNumber > Game.PlayerCount)
                throw new ArgumentOutOfRangeException("playerNumber");

            this.playerNumber = playerNumber;

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
            if (ScoreCoords.IsEmpty)
                throw new ArgumentException("Cannot print score with empty score coordinates");
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

        public void PrintScore(int left,int top)
        {
            ScoreCoords = new Point(left, top);
            PrintScore();
        }
    }
}