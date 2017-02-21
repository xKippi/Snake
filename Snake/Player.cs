using System;

namespace Snake
{
    public class Player
    {
        private static int maxScore = 9999;

        public Point ScoreCoords { get; set; }
        public Snake Snake { get; set; }
        private readonly ConsoleKey[] controlKeys;
        private readonly ConsoleColor color;
        private readonly string name;
        private readonly int playerNumber;
        private int score;
        private int highscore;

        public static int MaxScore
        {
            get { return maxScore; }
            set { if (value < 10) throw new ArgumentException(); maxScore = value; }
        }
        public int PlayerNumber
        {
            get { return playerNumber; }
        }
        public int Score
        {
            get { return score; }
            set { if (score <= maxScore) score = value; }
        }
        public int Highscore
        {
            get { return highscore; }
            set { if(highscore <= maxScore) highscore = value; }
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
        }


        public Player(string name, int playerNumber, ConsoleColor color, ConsoleKey[] controlKeys)
        {
            this.color = color;
            this.controlKeys = controlKeys;
            this.name = name;
            this.playerNumber = playerNumber;
        }

        public void PrintScore()
        {
            if (ScoreCoords.IsEmpty)
                throw new ArgumentException("Cannot print score at empty score coordinates");
            if (score > highscore)
                highscore = score;
            ConsoleColor colorBefore = Console.ForegroundColor;
            string space = new string(' ', maxScore.ToString().Length);
            Console.SetCursorPosition(ScoreCoords.X, ScoreCoords.Y);
            Console.ForegroundColor = color;
            Console.Write(name + ":");
            Console.ForegroundColor = colorBefore;
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