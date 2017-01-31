using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Snake
{
    public enum WindowSize { Small=40, Normal=46, Big=52}
    public enum Direction { Right, Left, Up, Down };

    class Game
    {
        public static readonly Random randy = new Random();
        public static int MaxNameLength { get; private set; }
        public static int MaxScore { get; private set; }
        public static int StartLength { get; private set; }
        public static int WindowHeight { get; private set; }
        public static int WindowWidth { get; private set; }
        public static char BodyChar { get; private set; }
        public static char HeadChar { get; private set; }
        public static bool AskForName { get; private set; }
        public static bool DeleteCorps { get; private set; }
        public static ConsoleColor DefaultForegroundColor { get; private set; }
        public static ConsoleColor DefaultBackgroundColor { get; private set; }
        public static ConsoleColor ReadColor { get; private set; }
        private static ConsoleColor frameForegroundColor;
        private static ConsoleColor frameBackgroundColor;
        private static ConsoleColor player1Color;
        private static ConsoleColor player2Color;
        private static ConsoleColor starColor;
        private static ConsoleColor pauseColor;
        private static ConsoleColor pauseHighlightColor;
        private static ConsoleColor pauseTextColor;
        private static WindowSize windowSize;
        private static int time;
        private static int tickSpeed = 80;
        private static int[][] starCoordinates;
        private static string currentPath;
        private static string configPath;
        private static char frameChar;
        private static char starChar;

        public static int[][] StarCoordinates
        {
            get { return starCoordinates; }
            set { starCoordinates = value; }
        }

        static void Main(string[] args)
        {
            if (Initialize() != 0)
                return;

            Console.Clear();

            starCoordinates[0][0] = randy.Next(1, WindowWidth - 1);

            Player p1 = new Player("Player 1", 1, player1Color);    
            Player p2 = new Player("Player 2", 2, player2Color);

            Console.Clear();

            StarCoordinates[1][0] = randy.Next(1, WindowHeight - 4);

            DrawFrame();
            p1.ScoreLeft = 0;
            p1.ScoreTop = p2.ScoreTop = WindowHeight - 3;
            p2.ScoreLeft = WindowWidth - MaxNameLength - 1;
            p1.PrintScore();
            p2.PrintScore();

            p1.Snake = new Snake(WindowWidth / 4, (WindowHeight - 8) / 2, p1);
            p2.Snake = new Snake(WindowWidth / 2 + WindowWidth / 4, (WindowHeight - 8) / 2, p2);

            PrintStar(p1.Snake, p2.Snake);

            while(true)
            {
                time = Environment.TickCount;
                if (Console.KeyAvailable)
                {
                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.W: if (p1.Snake.Dir != Direction.Down) p1.Snake.Dir = Direction.Up; break;
                        case ConsoleKey.A: if (p1.Snake.Dir != Direction.Right) p1.Snake.Dir = Direction.Left; break;
                        case ConsoleKey.S: if (p1.Snake.Dir != Direction.Up) p1.Snake.Dir = Direction.Down; break;
                        case ConsoleKey.D: if (p1.Snake.Dir != Direction.Left) p1.Snake.Dir = Direction.Right; break;
                        case ConsoleKey.LeftArrow: if (p2.Snake.Dir != Direction.Right) p2.Snake.Dir = Direction.Left; break;
                        case ConsoleKey.RightArrow: if (p2.Snake.Dir != Direction.Left) p2.Snake.Dir = Direction.Right; break;
                        case ConsoleKey.UpArrow: if (p2.Snake.Dir != Direction.Down) p2.Snake.Dir = Direction.Up; break;
                        case ConsoleKey.DownArrow: if (p2.Snake.Dir != Direction.Up) p2.Snake.Dir = Direction.Down; break;
                        case ConsoleKey.Escape: Pause(); break;
                        case ConsoleKey.P: Pause(); break;
                    }
                    starCoordinates[0][0] = randy.Next(1, WindowWidth - 1);
                }
                starCoordinates[1][0] = randy.Next(1, WindowHeight - 4);
                
                if (p1.Snake.IsDead)
                {
                    if (!p1.Snake.StartedRespawnThread)
                    {
                        new Thread(() =>
                        {
                            Thread.Sleep(2500);
                            p1.Snake.CanRespawn = true;
                        }).Start();
                        p1.Snake.StartedRespawnThread = true;
                    }
                    if (p1.Snake.CanRespawn)
                        p1.Snake = new Snake(WindowWidth / 4, (WindowHeight - 8) / 2, p1);
                }
                else
                {
                    p1.Snake.RefreshCoordiantes();
                    Snake.HandleCollision(p1.Snake, p2.Snake);
                    if (!p1.Snake.IsDead)
                    {
                        p1.Snake.Move();
                        Snake.Moved = true;
                    }
                }
                
                if (p2.Snake.IsDead)
                {
                    if (!p2.Snake.StartedRespawnThread)
                    {
                        new Thread(() =>
                        {
                            Thread.Sleep(2500);
                            p2.Snake.CanRespawn = true;
                        }).Start();
                        p2.Snake.StartedRespawnThread = true;
                    }
                    if (p2.Snake.CanRespawn)
                        p2.Snake = new Snake(WindowWidth / 2 + WindowWidth / 4, (WindowHeight - 8) / 2, p2);
                }
                else
                {
                    p2.Snake.RefreshCoordiantes();
                    Snake.HandleCollision(p2.Snake, p1.Snake);
                    Snake.Moved = false;
                    if(!p2.Snake.IsDead)
                        p2.Snake.Move();
                }

                while (time + tickSpeed >= Environment.TickCount)
                {
                    Thread.Sleep(1);
                }
            }
        }



        private static int Initialize()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;
            Console.Title = "Snake - Multiplayer";

            SetVariables();

            Console.ForegroundColor = DefaultForegroundColor;
            Console.BackgroundColor = DefaultBackgroundColor;

            if (!File.Exists(configPath))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Recommended font is Consolas with fontsize 16!");
                Console.ForegroundColor = DefaultForegroundColor;
                Console.WriteLine("Press any key to continue...");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.ReadKey(true);
            }

            AppendConfig();

            Console.ForegroundColor = DefaultForegroundColor;
            Console.BackgroundColor = DefaultBackgroundColor;

            return SetWindowSize();
        }

        private static void SetVariables()
        {
            StarCoordinates = new int[][]
            {
                new int[2],
                new int[2]
            };
            currentPath = Application.StartupPath;
            configPath = currentPath + "\\snake.conf";
            AskForName = false;
            DeleteCorps = true;
            frameChar = '\u2592';
            starChar = '*';
            HeadChar = 'O';
            BodyChar = '\u25E6';
            DefaultForegroundColor = ConsoleColor.Gray;     //Console.ForegroundColor?
            DefaultBackgroundColor = ConsoleColor.Black;    //Console.BackgroundColor?
            starColor = ConsoleColor.Yellow;
            ReadColor = ConsoleColor.White;
            player1Color = ConsoleColor.DarkGreen;
            player2Color = ConsoleColor.DarkCyan;
            frameForegroundColor = ConsoleColor.DarkGray;
            frameBackgroundColor = ConsoleColor.Black;
            pauseColor = ConsoleColor.Red;
            pauseHighlightColor = ConsoleColor.Gray;
            pauseTextColor = ConsoleColor.DarkGray;
            windowSize = WindowSize.Normal;
            MaxNameLength = 20;
            MaxScore = 9999;
            StartLength = 4;
        }

        private static void AppendConfig()
        {
            string[] config = new string[0];
            if (!Utils.TryReadLines(configPath, ref config))
            {   
                config = new string[] { "#Here you can change some settings from the Program.\n","player1Color=" + player1Color, "player2Color=" + player2Color, "frameForegroundcolor=" + frameForegroundColor, "frameBackgroundcolor=" + frameBackgroundColor, "defaultForegroundcolor=" + DefaultForegroundColor, "defaultBackgroundcolor=" + DefaultBackgroundColor, "starColor=" + starColor, "readColor=" + ReadColor, "pauseColor="+pauseColor, "pauseTextColor="+pauseTextColor, "pauseHighlightColor="+pauseHighlightColor, "frameChar=" + frameChar, "starChar="+starChar,"bodyChar="+BodyChar,"headChar="+HeadChar,"askForName=" + AskForName, "deleteCorps=" + DeleteCorps, "preferredWindowSize=" +  windowSize, "maxNameLength="+MaxNameLength,"maxScore="+MaxScore,"startLength="+StartLength,"tickSpeed="+tickSpeed};
                foreach (string item in config)
                {
                    File.AppendAllText(configPath, item + Environment.NewLine);
                }
                return;
            }

            string name = "";
            string realName = "";
            string confValue;

            foreach (string line in config)
            {
                try
                {
                    if (line == "")
                        continue;
                    if (line[0] == '#')
                        continue;
                    realName = line.Substring(0, line.IndexOf('='));
                    name = realName.ToLower();
                    confValue = line.Substring(line.IndexOf('=') + 1);
                    switch (name)
                    {
                        case "player1color":            player1Color = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                        case "player2color":            player2Color = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                        case "frameforegroundcolor":    frameForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                        case "framebackgroundcolor":    frameBackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                        case "defaultforegroundcolor":  DefaultForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                        case "defaultbackgroundcolor":  DefaultBackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                        case "starcolor":               starColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                        case "readcolor":               ReadColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                        case "pausecolor":              pauseColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                        case "pausetextcolor":          pauseTextColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                        case "pausehighlightcolor":     pauseHighlightColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                        case "framechar":               frameChar = confValue[0]; break;
                        case "starchar":                starChar = confValue[0];break;
                        case "bodychar":                BodyChar = confValue[0];break;
                        case "headchar":                HeadChar = confValue[0];break;
                        case "askforname":              AskForName = bool.Parse(confValue); break;
                        case "deletecorps":             DeleteCorps = bool.Parse(confValue); break;
                        case "preferredwindowsize":     windowSize = (WindowSize)Enum.Parse(typeof(WindowSize), confValue); break;
                        case "maxnamelength":           MaxNameLength = int.Parse(confValue);  break;
                        case "maxscore":                MaxScore = int.Parse(confValue); if (MaxScore < 10) { MaxScore = 9999; throw new ArgumentOutOfRangeException(); } break;
                        case "startlength":             StartLength = int.Parse(confValue); if (StartLength < 0) { StartLength = 4; throw new ArgumentOutOfRangeException(); } break;
                        case "tickspeed":               tickSpeed = int.Parse(confValue); if (tickSpeed < 0) { tickSpeed=80; throw new ArgumentOutOfRangeException(); } break;
                        default:                        throw new InvalidDataException();
                    }
                }
                catch (Exception)
                {
                    if (!line.Contains('='))
                        realName = line;
                    PrintError("Error appending " + '"' + realName + '"' + " from the config file (spelling issue?)... Using default value!");
                }
            }

            int[] windowDimesions = { (int)windowSize, (int)windowSize * 2 + 1 };

            while(windowDimesions[0]>Console.LargestWindowHeight||windowDimesions[1]>Console.LargestWindowWidth)
            {
                windowDimesions[0] -= 6;
                windowDimesions[1] = windowDimesions[0] * 2 - 1;
                if (windowDimesions[0] < 40)
                    return;
            }

            if (MaxNameLength > windowDimesions[1] / 4 + 5 || MaxNameLength > windowDimesions[1] / 2 - 15)
            {
                PrintError('"' + "maxNameLength" + '"' + " is too big... Using default value!");
                MaxNameLength = 20;
            }
            if (MaxNameLength < 11 + MaxScore.ToString().Length)
            {
                PrintError('"' + "maxScore" + '"' + " is too small... Using default value!");
                MaxNameLength = 20;
            }

            if (MaxScore.ToString().Length > MaxNameLength - 11)
            {
                PrintError('"' + "maxScore" + '"' + " is too big... Using default value!");
                MaxScore = 9999;
                if (MaxNameLength < 11 + MaxScore.ToString().Length)
                {
                    PrintError('"' + "maxNameLenght" + '"' + " is too big(maxNameLenght and maxScore should suit each other)... Using default value!");
                    MaxNameLength = 20;
                }
            }

            if (StartLength > (windowDimesions[0] - 8) / 2 + 3)
            {
                PrintError('"' + "startLength" + '"' + " is too big... Using default value!");
                StartLength = 4;
            }

        }

        private static int SetWindowSize()
        {
            int originalHeight = Console.WindowHeight;
            int originalWidth = Console.WindowWidth;

            WindowHeight = (int)windowSize;
            WindowWidth = WindowHeight * 2 + 1;

            while (windowSize >= WindowSize.Small)
            {
                try
                {
                    Console.WindowHeight = 10;
                    Console.WindowWidth = 10;
                    Console.BufferHeight = WindowHeight;
                    Console.BufferWidth = WindowWidth;
                    Console.WindowHeight = WindowHeight;
                    Console.WindowWidth = WindowWidth;
                    return 0;
                }
                catch (Exception)
                {
                    windowSize -= 6;
                    WindowHeight = (int)windowSize;
                    WindowWidth = WindowHeight * 2 + 1;

                    Console.WindowHeight = originalHeight;
                    Console.WindowWidth = originalWidth;
                    PrintError("Can not set window size for game (Try smaller font size!)... Trying smaller window...");
                }
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Can't set Console Size for game... Quitting!");
            Console.ForegroundColor = DefaultForegroundColor;
            Thread.Sleep(3000);
            return 1;
        }

        private static void DrawFrame()
        {
            Console.ForegroundColor = frameForegroundColor;
            Console.BackgroundColor = frameBackgroundColor;
            for (int i = 1; i <= WindowHeight - 3; i++)
            {
                Console.SetCursorPosition(0, i - 1);
                Console.Write(frameChar);
                Console.SetCursorPosition(WindowWidth - 1, i - 1);
                Console.Write(frameChar);
            }
            for (int i = 1; i <= WindowWidth - 1; i++)
            {
                Console.SetCursorPosition(i - 1, 0);
                Console.Write(frameChar);
                Console.SetCursorPosition(i - 1, WindowHeight - 4);
                Console.Write(frameChar);
            }
            Console.ForegroundColor = DefaultForegroundColor;
            Console.BackgroundColor = DefaultBackgroundColor;
        }

        public static void PrintStar(Snake snake1, Snake snake2)
        {
            starCoordinates[0][1] = starCoordinates[0][0]; 
            starCoordinates[1][1] = starCoordinates[1][0];
            bool samePosition;
            do
            {
                samePosition = false;
                for (int i = snake1.Lenght; i >= 0; i--)
                {
                    if (snake1.Coordinates[0][i] == starCoordinates[0][1] && snake1.Coordinates[1][i] == starCoordinates[1][1])
                    {
                        starCoordinates[0][1] = randy.Next(1, WindowWidth - 1);
                        starCoordinates[1][1] = randy.Next(1, WindowHeight - 4);
                        samePosition = true;
                    }
                }
                for (int i = snake2.Lenght; i >= 0; i--)
                {
                    if (snake2.Coordinates[0][i] == starCoordinates[0][1] && snake2.Coordinates[1][i] == starCoordinates[1][1])
                    {
                        starCoordinates[0][1] = randy.Next(1, WindowWidth - 1);
                        starCoordinates[1][1] = randy.Next(1, WindowHeight - 4);
                        samePosition = true;
                    }
                }
            }
            while (samePosition);
            Console.ForegroundColor = starColor;
            Console.SetCursorPosition(starCoordinates[0][1], starCoordinates[1][1]);
            Console.WriteLine(starChar);
            Console.ForegroundColor = DefaultForegroundColor;
        }

        public static void Pause()  //have a break, have a KitKat :)
        {
            Console.Title = Console.Title + " [Paused]";

            Console.SetCursorPosition(Console.WindowWidth/2-5, Console.WindowHeight - 3);
            Console.ForegroundColor = pauseColor;
            Console.Write("Game paused");
            Console.SetCursorPosition(Console.WindowWidth / 2 - 12, Console.WindowHeight - 2);
            Console.ForegroundColor = pauseTextColor;
            PrintHighlight("Press ", "[Enter]", "to continue", pauseHighlightColor);

            do
            {
                Thread.Sleep(1);
            }
            while (!(Console.ReadKey(true).Key == ConsoleKey.Enter));

            string space = new string(' ', 25);
            Console.SetCursorPosition(Console.WindowWidth / 2 - 5, Console.WindowHeight - 3);
            Console.Write(space);
            Console.SetCursorPosition(Console.WindowWidth / 2 - 12, Console.WindowHeight - 2);
            Console.Write(space);

            time = Environment.TickCount;
            Console.Title = Console.Title.Remove(Console.Title.Length - 9);
        }

        public static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ForegroundColor = DefaultForegroundColor;
            Thread.Sleep(2500);
        }

        public static void PrintHighlight(string begin, string highlight, string end, ConsoleColor highlightColor)
        {
            ConsoleColor prevColor = Console.ForegroundColor;

            Console.Write(begin);
            Console.ForegroundColor = highlightColor;
            Console.Write(highlight);
            Console.ForegroundColor = prevColor;
            Console.Write(end);
        }
    }
}
