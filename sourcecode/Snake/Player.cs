using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    class Player
    {
        public int ScoreLeft { get; set; }
        public int ScoreTop { get; set; }
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
            Console.SetCursorPosition(ScoreLeft, ScoreTop);
            Console.ForegroundColor = color;
            Console.Write(name + ":");
            Console.ForegroundColor = Game.DefaultForegroundColor;
            Console.SetCursorPosition(ScoreLeft+7, ScoreTop + 1);
            Console.Write(space);
            Console.SetCursorPosition(ScoreLeft, ScoreTop + 1);
            Console.Write("Score: {0}", score);
            Console.SetCursorPosition(ScoreLeft + 11, ScoreTop + 2);
            Console.Write(space);
            Console.SetCursorPosition(ScoreLeft, ScoreTop + 2);
            Console.Write("Highscore: {0}", highscore);
        }
    }
}
