using Snake;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Snake
{
    public enum WindowSize { Small = 40, Normal = 46, Big = 52 }
    public enum Direction { Up, Left, Down, Right };
    public enum CollisionObject { Nothing, Snake1, Snake2, Wall, SnakeHead, Star, Corpse };
}

internal class Program
{
    private static readonly Random randy =                  new Random();
    private static List<ConsoleKeyInfo> keyBuffer =         new List<ConsoleKeyInfo>();
    private static ConsoleColor defaultForegroundColor =    ConsoleColor.Gray;              //Console.ForegroundColor?
    private static ConsoleColor defaultBackgroundColor =    ConsoleColor.Black;             //Console.BackgroundColor?
    private static ConsoleColor[] countdownColors =         { ConsoleColor.Red, ConsoleColor.Yellow, ConsoleColor.Green };
    private static ConsoleColor[] playerColor =             { ConsoleColor.DarkGreen, ConsoleColor.DarkCyan };
    private static ConsoleKey[] p1ControlKeys =             { ConsoleKey.W, ConsoleKey.A, ConsoleKey.S, ConsoleKey.D };
    private static ConsoleKey[] p2ControlKeys =             { ConsoleKey.UpArrow, ConsoleKey.LeftArrow, ConsoleKey.DownArrow, ConsoleKey.RightArrow };
    private static WindowSize preferedWindowSize =          WindowSize.Normal;
    private static int startLength =                        4;
    private static bool askForName =                        true;
    private static bool countdownEnabled =                  true;
    private static string configPath =                      Application.StartupPath + "\\snake.conf";


    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;
        Console.Title = "Snake - Multiplayer";

        Console.ForegroundColor = defaultForegroundColor;
        Console.BackgroundColor = defaultBackgroundColor;

        if (!File.Exists(configPath))
        {
            Utils.PrintHighlight("", "Recommended font is Consolas with fontsize 16!", "\nPress any key to continue...", ConsoleColor.Cyan);
            Console.ReadKey(true);
        }

        AppendConfig();

        Console.ForegroundColor = defaultForegroundColor;
        Console.BackgroundColor = defaultBackgroundColor;

        Game.WindowSize=preferedWindowSize;

        Game.CoordinateSystem = new CollisionObject[Game.WindowWidth, Game.WindowHeight - 3];
        Star.Coordinates = new Point(randy.Next(1, Game.WindowWidth - 1), randy.Next(1, Game.WindowHeight - 4));

        Console.Clear();

        string[] playerNames = { "Player 1", "Player 2" };
        if (askForName)
        {
            for (int i = 0; i < playerNames.Length; i++)
            {
                Utils.PrintHighlight("", "Player " + (i + 1), " Name: ", playerColor[i]);
                playerNames[i] = Utils.Read(Game.ReadColor);
                Console.WriteLine();
                if (playerNames[i].Length == 0) playerNames[i] = "Player " + (i + 1);
            }
        }

        Console.Clear();

        Game.Players.Add(new Player(playerNames[0], 1, playerColor[0], p1ControlKeys));
        Game.Players.Add(new Player(playerNames[1], 2, playerColor[1], p2ControlKeys));

        if (countdownEnabled)
        {
            Game.Countdown(countdownColors);
            Console.Clear();
        }

        Game.DrawFrame();
        Game.Players[0].PrintScore(0, Game.WindowHeight - 3);
        Game.Players[1].PrintScore(Game.WindowWidth - 21, Game.WindowHeight - 3);

        Game.Players[0].Snake = new Snake.Snake(Game.WindowWidth / 4, (Game.WindowHeight - 8) / 2, startLength, Game.Players[0]);
        Game.Players[1].Snake = new Snake.Snake(Game.WindowWidth / 2 + Game.WindowWidth / 4, (Game.WindowHeight - 8) / 2, startLength, Game.Players[1]);

        Star.Print();

        new Thread(() =>
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (keyBuffer.Count > 0
                    )
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
            if (keyBuffer.Count > 0)
            {
                if (keyBuffer[0].Key == ConsoleKey.Escape || keyBuffer[0].Key == ConsoleKey.P)
                {
                    Game.Pause();
                    time = Environment.TickCount;
                }
                if (keyBuffer[0].Key == ConsoleKey.Q)
                {
                    Game.Quit(0);
                    time = Environment.TickCount;
                }
                for (int i = 0; i < 4; i++)
                {
                    int oppositeDir = (i < 2) ? i + 2 : i - 2;
                    if (keyBuffer[0].Key == Game.Players[0].ControlKeys[i])
                        if (Game.Players[0].Snake.Dir != (Direction)oppositeDir)
                            Game.Players[0].Snake.Dir = (Direction)i;
                    if (keyBuffer[0].Key == Game.Players[1].ControlKeys[i])
                        if (Game.Players[1].Snake.Dir != (Direction)oppositeDir)
                            Game.Players[1].Snake.Dir = (Direction)i;
                }
                keyBuffer.RemoveAt(0);
            }
            Star.Coordinates = new Point(randy.Next(1, Game.WindowWidth - 1), randy.Next(1, Game.WindowHeight - 4));

            if (Game.Players[0].Snake.CanRespawn)
            {
                Game.Players[0].Snake = new Snake.Snake(Game.WindowWidth / 4, (Game.WindowHeight - 8) / 2, startLength, Game.Players[0]);
            }
            else if (!Game.Players[0].Snake.IsDead)
            {
                Snake.Snake.HandleCollision(Game.Players[0].Snake, Game.Players[1].Snake);
                if (!Game.Players[0].Snake.IsDead)
                {
                    Game.Players[0].Snake.Move();
                }
            }

            if (Game.Players[1].Snake.CanRespawn)
            {
                Game.Players[1].Snake = new Snake.Snake(Game.WindowWidth / 2 + Game.WindowWidth / 4, (Game.WindowHeight - 8) / 2, startLength, Game.Players[1]);
            }
            else if (!Game.Players[1].Snake.IsDead)
            {
                Snake.Snake.HandleCollision(Game.Players[1].Snake, Game.Players[0].Snake);
                if (!Game.Players[1].Snake.IsDead)
                    Game.Players[1].Snake.Move();
            }

            while (time + Game.TickSpeed >= Environment.TickCount)
            {
                Thread.Sleep(1);
            }
        }
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
                    "frameForegroundcolor=" + Game.FrameForegroundColor,
                    "frameBackgroundcolor=" + Game.FrameBackgroundColor,
                    "defaultForegroundcolor=" + defaultForegroundColor,
                    "defaultBackgroundcolor=" + defaultBackgroundColor,
                    "starColor=" + Star.Color,
                    "readColor=" + Game.ReadColor,
                    "countdownColors={" + countdownColors[0] + "," + countdownColors[1] + "," + countdownColors[2] + "}",
                    "pauseColor=" +Game.PauseColor,
                    "pauseTextColor=" +Game.PauseTextColor,
                    "pauseHighlightColor=" +Game.PauseHighlightColor,
                    "",
                    "frameChar=" + Game.FrameChar,
                    "starChar=" +Star.Char,
                    "bodyChar=" +Snake.Snake.BodyChar,
                    "headChar=" +Snake.Snake.HeadChar,
                    "",
                    "deleteCorpse=" + Snake.Snake.DeleteCorpse,
                    "#It is recommended to set pointsPerDeadBodyPart around 1/[startLength+1] (=1 point per dead snake)",
                    "#Because compared to eating a star (1 Point) or killing another player (2 Points)",
                    "#eating a whole dead snake would give too many points",
                    "pointsPerDeadBodyPart=" +Snake.Snake.PointsPerDeadBodyPart,
                    "",
                    "player1Keys={" + p1ControlKeys[0] + "," + p1ControlKeys[1] + "," + p1ControlKeys[2] + "," + p1ControlKeys[3] + "}",
                    "player2Keys={" + p2ControlKeys[0] + "," + p2ControlKeys[1] + "," + p2ControlKeys[2] + "," + p2ControlKeys[3] + "}",
                    "",
                    "askForName=" + askForName,
                    "countdownEnabled=" +countdownEnabled,
                    "",
                    "preferredWindowSize=" + preferedWindowSize,
                    "",
                    "maxScore=" +Player.MaxScore,
                    "startLength=" +startLength,
                    "tickSpeed=" +Game.TickSpeed
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
                realName = line.Substring(0, line.IndexOf('=')).Trim();
                name = realName.ToLower();
                confValue = line.Substring(line.IndexOf('=') + 1).Trim();

                List<string> confValues = new List<string>();
                if (confValue.Contains("{") && confValue.Contains("}"))
                {
                    confValue = confValue.Remove(0,1);
                    confValue = confValue.Replace('}', ',');
                    while(confValue.Contains(","))
                    {
                        int index = confValue.IndexOf(',');
                        confValues.Add(confValue.Substring(0, index));
                        confValue = confValue.Remove(0, index+1);
                    }
                }
                switch (name)
                {
                    case "player1color":            playerColor[0] = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                    case "player2color":            playerColor[1] = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                    case "frameforegroundcolor":    Game.FrameForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                    case "framebackgroundcolor":    Game.FrameBackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                    case "defaultforegroundcolor":  defaultForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                    case "defaultbackgroundcolor":  defaultBackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                    case "starcolor":               Star.Color = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                    case "readcolor":               Game.ReadColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                    case "countdowncolors":         for (int i = 0; i < confValues.Count; i++) { countdownColors[i] = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValues[i]); } break;
                    case "pausecolor":              Game.PauseColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                    case "pausetextcolor":          Game.PauseTextColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                    case "pausehighlightcolor":     Game.PauseHighlightColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), confValue); break;
                    case "framechar":               Game.FrameChar = confValue[0]; break;
                    case "starchar":                Star.Char = confValue[0]; break;
                    case "bodychar":                Snake.Snake.BodyChar = confValue[0]; break;
                    case "headchar":                Snake.Snake.HeadChar = confValue[0]; break;
                    case "askforname":              askForName = bool.Parse(confValue); break;
                    case "countdownenabled":        countdownEnabled = bool.Parse(confValue); break;
                    case "deletecorpse":            Snake.Snake.DeleteCorpse = bool.Parse(confValue); break;
                    case "pointsperdeadbodypart":   Snake.Snake.PointsPerDeadBodyPart = double.Parse(confValue); if (Snake.Snake.PointsPerDeadBodyPart < 0) { Snake.Snake.PointsPerDeadBodyPart = 0; throw new ArgumentOutOfRangeException(); } break;
                    case "player1keys":             for (int i = 0; i < confValues.Count; i++) { p1ControlKeys[i] = (ConsoleKey)Enum.Parse(typeof(ConsoleKey), confValues[i]); } break;
                    case "player2keys":             for (int i = 0; i < confValues.Count; i++) { p2ControlKeys[i] = (ConsoleKey)Enum.Parse(typeof(ConsoleKey), confValues[i]); } break;
                    case "preferredwindowsize":     preferedWindowSize = (WindowSize)Enum.Parse(typeof(WindowSize), confValue); break;
                    case "maxscore":                Player.MaxScore = int.Parse(confValue); if (Player.MaxScore < 0 || Player.MaxScore > 999999999) { Player.MaxScore = 9999; throw new ArgumentOutOfRangeException(); } break;
                    case "startlength":             startLength = int.Parse(confValue); if (startLength < 0) { startLength = 4; throw new ArgumentOutOfRangeException(); } break;
                    case "tickspeed":               Game.TickSpeed = int.Parse(confValue); if (Game.TickSpeed < 0) { Game.TickSpeed = 80; throw new ArgumentOutOfRangeException(); } break;
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

        int[] windowDimesions = { (int)preferedWindowSize, (int)preferedWindowSize * 2 + 1 };

        while (windowDimesions[0] > Console.LargestWindowHeight || windowDimesions[1] > Console.LargestWindowWidth)
        {
            windowDimesions[0] -= 6;
            windowDimesions[1] = windowDimesions[0] * 2 - 1;
            if (windowDimesions[0] < 40)
                return;
        }

        if (startLength > (windowDimesions[0] - 8) / 2 + 3)
        {
            Utils.PrintError('"' + "startLength" + '"' + " is too big... Using default value!");
            startLength = 4;
        }

        if (Snake.Snake.DeleteCorpse && Snake.Snake.PointsPerDeadBodyPart > 0)
        {
            Utils.PrintError("Error appending PointsPerDeadBodyPart: You can't get points for non-existent corpse! Using default value!");
            Snake.Snake.PointsPerDeadBodyPart = 0;
        }
    }
}