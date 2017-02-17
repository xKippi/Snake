﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Snake
{
    public enum WindowSize { Small=40, Normal=46, Big=52}
    public enum Direction { Up, Left, Down, Right };
    public enum CollisionObject { Nothing, Snake1, Snake2, Wall, SnakeHead, Star, Corpse };

    class Game
    {
        public static int MaxNameLength { get; private set; }
        public static int StartLength { get; private set; }
        public static ConsoleColor DefaultForegroundColor { get; private set; }
        public static ConsoleColor DefaultBackgroundColor { get; private set; }
        public static ConsoleColor ReadColor { get; private set; }
        private static readonly Random randy = new Random();
        private static List<ConsoleKeyInfo> keyBuffer = new List<ConsoleKeyInfo>();
        private static ConsoleColor frameForegroundColor;
        private static ConsoleColor frameBackgroundColor;
        private static ConsoleColor starColor;
        private static ConsoleColor pauseColor;
        private static ConsoleColor pauseHighlightColor;
        private static ConsoleColor pauseTextColor;
        private static ConsoleColor[] playerColor;
        private static ConsoleKey[] p1ControlKeys;
        private static ConsoleKey[] p2ControlKeys;
        private static CollisionObject[,] coords;
        private static WindowSize windowSize;
        private static Point starCoords;
        private static int tickSpeed = 80;
        private static bool askForName;
        private static string currentPath;
        private static string configPath;
        private static char frameChar;
        private static char starChar;

        static void Main(string[] args)
        {
            if (Initialize() != 0)
                return;

            Snake.Coordinates = coords = new CollisionObject[Console.WindowWidth, Console.WindowHeight - 3];
            starCoords = new Point(randy.Next(1, Console.WindowWidth - 1), randy.Next(1, Console.WindowHeight - 4));

            Console.Clear();

            string[] playerNames = { "Player 1", "Player 2" };
            if (askForName)
            {
                for (int i = 0; i < playerNames.Length; i++)
                {
                    Utils.PrintHighlight("", "Player "+(i+1), " Name: ", playerColor[i]);
                    playerNames[i] = Utils.Read(MaxNameLength);
                    Console.WriteLine();
                    if (playerNames[i].Length == 0) playerNames[i] = "Player "+(i+1);
                }
            }

            Player p1 = new Player(playerNames[0], 1, playerColor[0], p1ControlKeys);
            Player p2 = new Player(playerNames[1], 2, playerColor[1], p2ControlKeys);

            Console.Clear();

            //Console.WriteLine("Press any key to start...");
            //Console.ReadKey(true);

            DrawFrame();
            p1.PrintScore(0, Console.WindowHeight - 3);
            p2.PrintScore(Console.WindowWidth - MaxNameLength - 1, Console.WindowHeight - 3);

            p1.Snake = new Snake(Console.WindowWidth / 4, (Console.WindowHeight - 8) / 2, StartLength, p1);
            p2.Snake = new Snake(Console.WindowWidth / 2 + Console.WindowWidth / 4, (Console.WindowHeight - 8) / 2, StartLength, p2);

            PrintStar();

            new Thread(() =>
            {
               while(true)
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        if (keyBuffer.Count > 0)
                        {
                            if (keyBuffer[keyBuffer.Count - 1] != key)
                                keyBuffer.Add(key);
                        }
                        else
                            keyBuffer.Add(key);
                    }
                    Thread.Sleep(1);
                }
            }).Start();

            while (true)
            {
                int time = Environment.TickCount;
                if (keyBuffer.Count>0)
                {
                    if (keyBuffer[0].Key == ConsoleKey.Escape || keyBuffer[0].Key == ConsoleKey.P)
                    {
                        Pause();
                        time = Environment.TickCount;
                    }
                    for(int i=0;i<4;i++)
                    {
                        int oppositeDir = (i < 2) ? i+2 : i-2;
                        if (keyBuffer[0].Key == p1.ControlKeys[i])
                            if (p1.Snake.Dir != (Direction)oppositeDir)
                            {
                                p1.Snake.Dir = (Direction)i;
                            }
                        if (keyBuffer[0].Key == p2.ControlKeys[i])
                            if (p2.Snake.Dir != (Direction)oppositeDir)
                            {
                                p2.Snake.Dir = (Direction)i;
                            }
                    }

                    keyBuffer.RemoveAt(0);
                    starCoords.X = randy.Next(1, Console.WindowWidth - 1);
                }
                starCoords.Y = randy.Next(1, Console.WindowHeight - 4);

                if (p1.Snake.CanRespawn)
                {
                    p1.Snake = new Snake(Console.WindowWidth / 4, (Console.WindowHeight - 8) / 2, StartLength, p1);
                }
                else if(!p1.Snake.IsDead)
                {
                    Snake.HandleCollision(p1.Snake, p2.Snake);
                    if (!p1.Snake.IsDead)
                    {
                        p1.Snake.Move();
                    }
                }

                if (p2.Snake.CanRespawn)
                {
                    p2.Snake = new Snake(Console.WindowWidth / 2 + Console.WindowWidth / 4, (Console.WindowHeight - 8) / 2, StartLength, p2);
                }
                else if(!p2.Snake.IsDead)
                {
                    Snake.HandleCollision(p2.Snake, p1.Snake);
                    if (!p2.Snake.IsDead)
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
                Utils.PrintHighlight("", "Recommended font is Consolas with fontsize 16!", "\nPress any key to continue...", ConsoleColor.Cyan);
                Console.ReadKey(true);
            }

            AppendConfig();

            Console.ForegroundColor = DefaultForegroundColor;
            Console.BackgroundColor = DefaultBackgroundColor;

            return SetWindowSize();
        }

        private static void SetVariables()
        {
            currentPath = Application.StartupPath;
            configPath = currentPath + "\\snake.conf";
            askForName = false;
            frameChar = '\u2592';
            starChar = '*';
            DefaultForegroundColor = ConsoleColor.Gray;     //Console.ForegroundColor?
            DefaultBackgroundColor = ConsoleColor.Black;    //Console.BackgroundColor?
            starColor = ConsoleColor.Yellow;
            ReadColor = ConsoleColor.White;
            playerColor = new ConsoleColor[] { ConsoleColor.DarkGreen, ConsoleColor.DarkCyan };
            frameForegroundColor = ConsoleColor.DarkGray;
            frameBackgroundColor = ConsoleColor.Black;
            pauseColor = ConsoleColor.Red;
            pauseHighlightColor = ConsoleColor.Gray;
            pauseTextColor = ConsoleColor.DarkGray;
            p1ControlKeys = new ConsoleKey[] { ConsoleKey.W, ConsoleKey.A, ConsoleKey.S, ConsoleKey.D };
            p2ControlKeys = new ConsoleKey[] { ConsoleKey.UpArrow, ConsoleKey.LeftArrow, ConsoleKey.DownArrow, ConsoleKey.RightArrow };
            windowSize = WindowSize.Normal;
            MaxNameLength = 20;
            StartLength = 4;
        }

        private static void AppendConfig()
        {
            string[] config = new string[0];
            if (!Utils.TryReadLines(configPath, ref config))
            {
                config = new string[]
                {
                    "#Here you can change some settings from the Program.",
                    "",
                    "player1Color=" + playerColor[0],
                    "player2Color=" + playerColor[1],
                    "frameForegroundcolor=" + frameForegroundColor,
                    "frameBackgroundcolor=" + frameBackgroundColor,
                    "defaultForegroundcolor=" + DefaultForegroundColor,
                    "defaultBackgroundcolor=" + DefaultBackgroundColor,
                    "starColor=" + starColor,
                    "readColor=" + ReadColor,
                    "pauseColor=" +pauseColor,
                    "pauseTextColor=" +pauseTextColor,
                    "pauseHighlightColor=" +pauseHighlightColor,
                    "",
                    "frameChar=" + frameChar,
                    "starChar=" +starChar,
                    "bodyChar=" +Snake.BodyChar,
                    "headChar=" +Snake.HeadChar,
                    "",
                    "deleteCorpse=" + Snake.DeleteCorpse,
                    "#It is recommended to set pointsPerDeadBodyPart around 1/[startLength+1] (=1 point per dead snake)",
                    "#Because compared to eating a star (1 Point) or killing another player (2 Points)",
                    "#eating a whole dead snake would give too many points",
                    "pointsPerDeadBodyPart=" +Snake.PointsPerDeadBodyPart,
                    "",
                    "player1UpKey="+p1ControlKeys[0],
                    "player1LeftKey="+p1ControlKeys[1],
                    "player1DownKey="+p1ControlKeys[2],
                    "player1RightKey="+p1ControlKeys[3],
                    "player2UpKey="+p2ControlKeys[0],
                    "player2LeftKey="+p2ControlKeys[1],
                    "player2DownKey="+p2ControlKeys[2],
                    "player2RightKey="+p2ControlKeys[3],
                    "",
                    "askForName=" + askForName,
                    "preferredWindowSize=" +  windowSize,
                    "maxNameLength=" +MaxNameLength,
                    "maxScore=" +Player.MaxScore,
                    "startLength=" +StartLength,
                    "tickSpeed=" +tickSpeed
                };
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
                        case "player1color":            playerColor[0] = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                        case "player2color":            playerColor[1] = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
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
                        case "bodychar":                Snake.BodyChar = confValue[0];break;
                        case "headchar":                Snake.HeadChar = confValue[0];break;
                        case "askforname":              askForName = bool.Parse(confValue); break;
                        case "deletecorpse":            Snake.DeleteCorpse = bool.Parse(confValue); break;
                        case "pointsperdeadbodypart":   Snake.PointsPerDeadBodyPart = double.Parse(confValue); if (Snake.PointsPerDeadBodyPart < 0) { Snake.PointsPerDeadBodyPart = 0; throw new ArgumentOutOfRangeException(); } break;
                        case "player1upkey":            p1ControlKeys[0] = (ConsoleKey)Enum.Parse(typeof(ConsoleKey), confValue); break;
                        case "player1leftkey":          p1ControlKeys[1] = (ConsoleKey)Enum.Parse(typeof(ConsoleKey), confValue); break;
                        case "player1downkey":          p1ControlKeys[2] = (ConsoleKey)Enum.Parse(typeof(ConsoleKey), confValue); break;
                        case "player1rightkey":         p1ControlKeys[3] = (ConsoleKey)Enum.Parse(typeof(ConsoleKey), confValue); break;
                        case "player2upkey":            p2ControlKeys[0] = (ConsoleKey)Enum.Parse(typeof(ConsoleKey), confValue); break;
                        case "player2leftkey":          p2ControlKeys[1] = (ConsoleKey)Enum.Parse(typeof(ConsoleKey), confValue); break;
                        case "player2downkey":          p2ControlKeys[2] = (ConsoleKey)Enum.Parse(typeof(ConsoleKey), confValue); break;
                        case "player2rightkey":         p2ControlKeys[3] = (ConsoleKey)Enum.Parse(typeof(ConsoleKey), confValue); break;
                        case "preferredwindowsize":     windowSize = (WindowSize)Enum.Parse(typeof(WindowSize), confValue); break;
                        case "maxnamelength":           MaxNameLength = int.Parse(confValue);  break;
                        case "maxscore":                Player.MaxScore = int.Parse(confValue); break;
                        case "startlength":             StartLength = int.Parse(confValue); if (StartLength < 0) { StartLength = 4; throw new ArgumentOutOfRangeException(); } break;
                        case "tickspeed":               tickSpeed = int.Parse(confValue); if (tickSpeed < 0) { tickSpeed=80; throw new ArgumentOutOfRangeException(); } break;
                        default:                        throw new InvalidDataException();
                    }
                }
                catch (Exception)
                {
                    if (!line.Contains('='))
                        realName = line;
                    Utils.PrintError("Error appending " + '"' + realName + '"' + " from the config file (spelling issue?)... Using default value!");
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
                Utils.PrintError('"' + "maxNameLength" + '"' + " is too big... Using default value!");
                MaxNameLength = 20;
            }
            if (MaxNameLength < 11 + Player.MaxScore.ToString().Length)
            {
                Utils.PrintError('"' + "maxScore" + '"' + " is too small... Using default value!");
                MaxNameLength = 20;
            }

            if (Player.MaxScore.ToString().Length > MaxNameLength - 11)
            {
                Utils.PrintError('"' + "maxScore" + '"' + " is too big... Using default value!");
                Player.MaxScore = 9999;
                if (MaxNameLength < 11 + Player.MaxScore.ToString().Length)
                {
                    Utils.PrintError('"' + "maxNameLenght" + '"' + " is too big(maxNameLenght and maxScore should suit each other)... Using default value!");
                    MaxNameLength = 20;
                }
            }

            if (StartLength > (windowDimesions[0] - 8) / 2 + 3)
            {
                Utils.PrintError('"' + "startLength" + '"' + " is too big... Using default value!");
                StartLength = 4;
            }

            if (Snake.DeleteCorpse && Snake.PointsPerDeadBodyPart > 0)
            {
                Utils.PrintError("You can't get points for non-existent corpse... Using default values!");
                Snake.PointsPerDeadBodyPart = 0;
            }

            if (!askForName)
                MaxNameLength = 20;
        }

        private static int SetWindowSize()
        {
            int originalHeight = Console.WindowHeight;
            int originalWidth = Console.WindowWidth;

            while (windowSize >= WindowSize.Small)
            {
                try
                {
                    int windowHeight = (int)windowSize;
                    int windowWidth = windowHeight * 2 + 1;
                    Console.WindowHeight = 10;
                    Console.WindowWidth = 10;
                    Console.BufferHeight = windowHeight;
                    Console.BufferWidth = windowWidth;
                    Console.WindowHeight = windowHeight;
                    Console.WindowWidth = windowWidth;
                    return 0;
                }
                catch (Exception)
                {
                    windowSize -= 6;

                    Console.WindowHeight = originalHeight;
                    Console.WindowWidth = originalWidth;
                    Utils.PrintError("Can not set window size for game (Try smaller font size!)... Trying smaller window...");
                }
            }
            Utils.PrintError("Can't set Console Size for game... Quitting!", ConsoleColor.Red, 3000);
            return 1;
        }

        private static void DrawFrame()
        {
            Console.ForegroundColor = frameForegroundColor;
            Console.BackgroundColor = frameBackgroundColor;
            for (int i = 1; i <= Console.WindowHeight - 3; i++)
            {
                Console.SetCursorPosition(0, i - 1);
                coords[0, i - 1] = CollisionObject.Wall;
                Console.Write(frameChar);
                Console.SetCursorPosition(Console.WindowWidth - 1, i - 1);
                coords[Console.WindowWidth - 1, i - 1] = CollisionObject.Wall;
                Console.Write(frameChar);
            }
            for (int i = 1; i <= Console.WindowWidth - 1; i++)
            {
                Console.SetCursorPosition(i - 1, 0);
                coords[i - 1, 0] = CollisionObject.Wall;
                Console.Write(frameChar);
                Console.SetCursorPosition(i - 1, Console.WindowHeight - 4);
                coords[i - 1, Console.WindowHeight - 4] = CollisionObject.Wall;
                Console.Write(frameChar);
            }
            Console.ForegroundColor = DefaultForegroundColor;
            Console.BackgroundColor = DefaultBackgroundColor;
        }

        public static void PrintStar()
        {
            while (coords[starCoords.X, starCoords.Y] != CollisionObject.Nothing || (starCoords.Y > (Console.WindowHeight - 8) / 2 && starCoords.Y < ((Console.WindowHeight - 8) / 2 + StartLength + 1) && (starCoords.X == Console.WindowWidth / 4 || starCoords.X == Console.WindowWidth / 2 + Console.WindowWidth / 4)))
            {
                starCoords = new Point(randy.Next(1, Console.WindowWidth - 1), randy.Next(1, Console.WindowHeight - 4));
            }
            Console.ForegroundColor = starColor;
            Console.SetCursorPosition(starCoords.X, starCoords.Y);
            coords[starCoords.X, starCoords.Y] = CollisionObject.Star;
            Console.Write(starChar);
            Console.ForegroundColor = DefaultForegroundColor;
        }

        public static void Pause()  //have a break, have a KitKat :)
        {
            Console.Title = Console.Title + " [Paused]";

            Console.SetCursorPosition(Console.WindowWidth/2-5, Console.WindowHeight - 3);
            ConsoleColor colorBefore = Console.ForegroundColor;
            Console.ForegroundColor = pauseColor;
            Console.Write("Game paused");
            Console.SetCursorPosition(Console.WindowWidth / 2 - 12, Console.WindowHeight - 2);
            Console.ForegroundColor = pauseTextColor;
            Utils.PrintHighlight("Press ", "[Enter]", " to continue", pauseHighlightColor);

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
            Console.ForegroundColor = colorBefore;
            Console.Title = Console.Title.Remove(Console.Title.Length - 9);
        }
    }
}