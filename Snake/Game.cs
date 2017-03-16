using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Snake
{
    public static class Game //Class to provide some stuff to easier make a snake game
    {
        public static int TickSpeed { get; set; } = 80;
        public static char FrameChar { get; set; } = '\u2592';
        public static List<Player> Players { get; set; } = new List<Player>();
        public static ConsoleColor ReadColor { get; set; } = ConsoleColor.White;
        public static CollisionObject[,] CoordinateSystem { get; set; }

        private static int windowHeight;
        public static int WindowHeight
        {
            get { return windowHeight; }
            set { Console.WindowHeight = 1; Console.WindowHeight = Console.BufferHeight = windowHeight = value; }
        }

        private static int windowWidth;
        public static int WindowWidth
        {
            get { return windowWidth; }
            set { Console.WindowWidth = 1; Console.WindowWidth = Console.BufferWidth = windowWidth = value; }
        }

        private static ConsoleColor pauseColor = ConsoleColor.Red;
        public static ConsoleColor PauseColor
        {
            get { return pauseColor; }
            set { pauseColor = value; }
        }

        private static ConsoleColor pauseHighlightColor = ConsoleColor.Gray;
        public static ConsoleColor PauseHighlightColor
        {
            get { return pauseHighlightColor; }
            set { pauseHighlightColor = value; }
        }

        private static ConsoleColor pauseTextColor = ConsoleColor.DarkGray;
        public static ConsoleColor PauseTextColor
        {
            get { return pauseTextColor; }
            set { pauseTextColor = value; }
        }

        private static ConsoleColor frameForegroundColor = ConsoleColor.DarkGray;
        public static ConsoleColor FrameForegroundColor
        {
            get { return frameForegroundColor; }
            set { frameForegroundColor = value; }
        }

        private static ConsoleColor frameBackgroundColor = ConsoleColor.Black;
        public static ConsoleColor FrameBackgroundColor
        {
            get { return frameBackgroundColor; }
            set { frameBackgroundColor = value; }
        }

        private static WindowSize windowSize;
        public static WindowSize WindowSize
        {
            get { return windowSize; }
            set
            {
                windowSize = value;
                int originalHeight = Console.WindowHeight;
                int originalWidth = Console.WindowWidth;
                while (WindowSize >= WindowSize.Small)
                {
                    try
                    {
                        WindowHeight = (int)WindowSize;
                        WindowWidth = WindowHeight * 2 + 1;
                        break;
                    }
                    catch (Exception)
                    {
                        WindowSize -= 6;

                        WindowHeight = originalHeight;
                        WindowWidth = originalWidth;
                        Utils.PrintError("Can not set window size for game (Try smaller font size!)... Trying smaller window...");
                    }
                }
                if (WindowSize < WindowSize.Small)
                {
                    Utils.PrintError("Can't set Console Size for game... Quitting!", ConsoleColor.Red, 3000);
                    Environment.Exit(1);
                }
            }
        }

        public static void Pause()  //have a break, have a KitKat :)
        {
            Console.Title = Console.Title + " [Paused]";

            Console.SetCursorPosition(windowWidth / 2 - 5, windowHeight - 3);
            ConsoleColor colorBefore = Console.ForegroundColor;
            Console.ForegroundColor = pauseColor;
            Console.Write("Game paused");
            Console.SetCursorPosition(windowWidth / 2 - 12, windowHeight - 2);

            Console.ForegroundColor = pauseTextColor;
            Utils.PrintHighlight("Press ", "[Enter]", " to continue", pauseHighlightColor);

            do
            {
                Thread.Sleep(1);
            }
            while (!(Console.ReadKey(true).Key == ConsoleKey.Enter));

            string space = new string(' ', 25);
            Console.SetCursorPosition(windowWidth / 2 - 5, windowHeight - 3);
            Console.Write(space);
            Console.SetCursorPosition(windowWidth / 2 - 12, windowHeight - 2);
            Console.Write(space);
            Console.ForegroundColor = colorBefore;
            Console.Title = Console.Title.Remove(Console.Title.Length - 9);
        }

        public static void DrawFrame()
        {
            ConsoleColor foregroundColorBefore = Console.ForegroundColor;
            ConsoleColor backgroundColorBefore = Console.BackgroundColor;

            Console.ForegroundColor = frameForegroundColor;
            Console.BackgroundColor = frameBackgroundColor;
            for (int i = 1; i <= windowHeight - 3; i++)
            {
                Console.SetCursorPosition(0, i - 1);
                CoordinateSystem[0, i - 1] = CollisionObject.Wall;
                Console.Write(FrameChar);
                Console.SetCursorPosition(windowWidth - 1, i - 1);
                CoordinateSystem[windowWidth - 1, i - 1] = CollisionObject.Wall;
                Console.Write(FrameChar);
            }
            for (int i = 1; i <= windowWidth - 1; i++)
            {
                Console.SetCursorPosition(i - 1, 0);
                CoordinateSystem[i - 1, 0] = CollisionObject.Wall;
                Console.Write(FrameChar);
                Console.SetCursorPosition(i - 1, windowHeight - 4);
                CoordinateSystem[i - 1, windowHeight - 4] = CollisionObject.Wall;
                Console.Write(FrameChar);
            }
            Console.ForegroundColor = foregroundColorBefore;
            Console.BackgroundColor = backgroundColorBefore;
        }

        public static void Countdown(ConsoleColor[] colors)
        {
            ConsoleColor colorBefore = Console.ForegroundColor;
            string defaultIndent = new string(' ', Console.WindowWidth / 2 - 14);

            Console.ForegroundColor = colors[0];
            Console.CursorTop = Console.WindowHeight / 2 - 15;
            Console.WriteLine(defaultIndent + "        333333333           \n" + defaultIndent + "   33333333333333333        \n" + defaultIndent + "33333333333333333333333     \n" + defaultIndent + "3333333333333333333333333   \n" + defaultIndent + "33333333333333333333333333  \n" + defaultIndent + "33333           33333333333 \n" + defaultIndent + "3                  333333333\n" + defaultIndent + "                    33333333\n" + defaultIndent + "                    33333333\n" + defaultIndent + "                   333333333\n" + defaultIndent + "                  333333333 \n" + defaultIndent + "                33333333333 \n" + defaultIndent + "            33333333333333  \n" + defaultIndent + "    33333333333333333333    \n" + defaultIndent + "    333333333333333333      \n" + defaultIndent + "    333333333333333333      \n" + defaultIndent + "    333333333333333333333   \n" + defaultIndent + "     333333333333333333333  \n" + defaultIndent + "                33333333333 \n" + defaultIndent + "                  3333333333\n" + defaultIndent + "                   333333333\n" + defaultIndent + "                    33333333\n" + defaultIndent + "                    33333333\n" + defaultIndent + "                   333333333\n" + defaultIndent + "                  3333333333\n" + defaultIndent + "3333          3333333333333 \n" + defaultIndent + "333333333333333333333333333 \n" + defaultIndent + "3333333333333333333333333   \n" + defaultIndent + "333333333333333333333333    \n" + defaultIndent + "33333333333333333333        ");
            Thread.Sleep(1000);
            Console.Clear();

            Console.ForegroundColor = colors[1];
            Console.CursorTop = Console.WindowHeight / 2 - 15;
            Console.WriteLine(defaultIndent + "      222222222222          \n" + defaultIndent + " 22222222222222222222       \n" + defaultIndent + "222222222222222222222222    \n" + defaultIndent + "2222222222222222222222222   \n" + defaultIndent + "222222222222222222222222222 \n" + defaultIndent + "222222222222222222222222222 \n" + defaultIndent + "22222            22222222222\n" + defaultIndent + "22                2222222222\n" + defaultIndent + "                  2222222222\n" + defaultIndent + "                  2222222222\n" + defaultIndent + "                  2222222222\n" + defaultIndent + "                 2222222222 \n" + defaultIndent + "                22222222222 \n" + defaultIndent + "              222222222222  \n" + defaultIndent + "             222222222222   \n" + defaultIndent + "           222222222222     \n" + defaultIndent + "          222222222222      \n" + defaultIndent + "         222222222222       \n" + defaultIndent + "       222222222222         \n" + defaultIndent + "      222222222222          \n" + defaultIndent + "     22222222222            \n" + defaultIndent + "    22222222222             \n" + defaultIndent + "  222222222222              \n" + defaultIndent + "  2222222222                \n" + defaultIndent + " 2222222222                 \n" + defaultIndent + "2222222222222222222222222222\n" + defaultIndent + "2222222222222222222222222222\n" + defaultIndent + "2222222222222222222222222222\n" + defaultIndent + "2222222222222222222222222222\n" + defaultIndent + "2222222222222222222222222222");
            Thread.Sleep(1000);
            Console.Clear();

            Console.ForegroundColor = colors[2];
            Console.CursorTop = Console.WindowHeight / 2 - 15;
            Console.WriteLine(defaultIndent + "                111         \n" + defaultIndent + "             111111         \n" + defaultIndent + "         1111111111         \n" + defaultIndent + "     11111111111111         \n" + defaultIndent + " 111111111111111111         \n" + defaultIndent + "1111111111111111111         \n" + defaultIndent + "1111111111111111111         \n" + defaultIndent + "1111111111111111111         \n" + defaultIndent + "1111111111111111111         \n" + defaultIndent + "11111     111111111         \n" + defaultIndent + "          111111111         \n" + defaultIndent + "          111111111         \n" + defaultIndent + "          111111111         \n" + defaultIndent + "          111111111         \n" + defaultIndent + "          111111111         \n" + defaultIndent + "          111111111         \n" + defaultIndent + "          111111111         \n" + defaultIndent + "          111111111         \n" + defaultIndent + "          111111111         \n" + defaultIndent + "          111111111         \n" + defaultIndent + "          111111111         \n" + defaultIndent + "          111111111         \n" + defaultIndent + "          111111111         \n" + defaultIndent + "          111111111         \n" + defaultIndent + "          111111111         \n" + defaultIndent + "1111111111111111111111111111\n" + defaultIndent + "1111111111111111111111111111\n" + defaultIndent + "1111111111111111111111111111\n" + defaultIndent + "1111111111111111111111111111\n" + defaultIndent + "1111111111111111111111111111");
            Thread.Sleep(1000);

            Console.ForegroundColor = colorBefore;
        }
        
        public static bool Quit(int sig)
        {
            if (sig != 5 && sig != 6)
            {
                string input;
                Console.SetCursorPosition((windowWidth-20)/2, windowHeight - 3);
                Console.Write("Really quit? (Y/N): ");
                do
                {
                    Console.CursorLeft = (windowWidth + 20) / 2;
                    Console.Write("   ");
                    Console.CursorLeft = (windowWidth + 20) / 2;
                    input = Utils.Read(3, ReadColor).ToLower();
                }
                while (input != "n" && input != "no" && input != "y" && input != "yes");

                if (input == "n" || input == "no")
                {
                    Console.SetCursorPosition((windowWidth - 20) / 2, windowHeight - 3);
                    Console.Write(new string(' ', 23));
                    return true;
                }
            }
            List<string> highscoreFile = new List<string>();
            string[] tmp = new string[0]; 
            if (!Utils.TryReadLines(Application.StartupPath + "\\highscore", ref tmp))
            {
                if (Players[0].Highscore < Players[1].Highscore)
                {
                    Player ptmp = Players[0];
                    Players[0] = Players[1];
                    Players[1] = ptmp;
                }

                for (int i = 1; i < 2; i++)
                    highscoreFile.Add((i+1) + ". " + (Players[i].Name + ": " + new string(' ', 27)).Substring(0, 27) + Players[i].Highscore);
            }
            else
            {
                List<int> index = new List<int>() { -1, -1 };
                List<int> oldIndex = new List<int>() { -1, -1 };
                highscoreFile.AddRange(tmp);
                for (int i = 0; i < highscoreFile.Count; i++)
                    for (int j = 0; j < 2; j++)
                    {
                        if (int.Parse(highscoreFile[i].Substring(27)) < Players[j].Highscore)
                            index[j] = i;

                        if (highscoreFile[i].Substring(2, Players[j].Name.Length) == Players[j].Name)
                            oldIndex[j] = i;
                    }

                index.RemoveAll(x => x < 0);
                oldIndex.RemoveAll(x => x < 0);

                for (int i = 0; i < oldIndex.Count; i++)
                {
                    highscoreFile.RemoveAt(oldIndex[i]);
                    for(int j=i+1;j<oldIndex.Count;j++)
                    {
                        if (oldIndex[j] < 1) ;
                    }
                }




            }
            File.WriteAllLines(Application.StartupPath + "\\highscore", highscoreFile.ToArray()); 
            Environment.Exit(0);
            return false;
        }
    }
}
    