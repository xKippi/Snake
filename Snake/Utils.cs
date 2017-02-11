﻿using System;
using System.IO;

namespace Snake
{
    static class Utils
    {
        public static bool TryReadLines(string file, ref string[] fileInLines)
        {
            if (File.Exists(file))
            {
                fileInLines = File.ReadAllLines(file);
                return true;
            }
            return false;
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
        public static void PrintHighlight(string begin, string highlight, ConsoleColor highlightColor)
        {
            PrintHighlight(begin, highlight, "", highlightColor);
        }
        public static void PrintHighlight(string highlight, ConsoleColor highlightColor)
        {
            PrintHighlight("", highlight, "", highlightColor);
        }

        public static string Read(int maxLength = 20)
        {
            ConsoleKeyInfo key = new ConsoleKeyInfo();
            Console.ForegroundColor = Game.ReadColor;
            bool cursorWasVisible = Console.CursorVisible;
            Console.CursorVisible = true;

            string eingabe = "";
            string[] temp = new string[2] { "", "" };
            int counter = 0;
            do
            {
                key = Console.ReadKey(true);
                if ((key.Key != ConsoleKey.Backspace) && (key.Key != ConsoleKey.Enter) && (eingabe.Length < maxLength) && (key.Key != ConsoleKey.RightArrow) && (key.Key != ConsoleKey.DownArrow) && (key.Key != ConsoleKey.LeftArrow) && (key.Key != ConsoleKey.Delete) && (key.Key != ConsoleKey.Tab) && (key.Key != ConsoleKey.Spacebar))
                {
                    if (!(key.Key != ConsoleKey.Spacebar && key.KeyChar == '\0'))
                    {
                        temp[0] += key.KeyChar;     
                        eingabe = temp[0] + temp[1];
                        Console.Write(key.KeyChar + temp[1]);
                        try
                        {
                            foreach (char c in temp[1])
                            {
                                Console.Write("\b");
                            }
                        }
                        catch (NullReferenceException) { }
                    }
                }
                else if (key.Key == ConsoleKey.Backspace && temp[0].Length > 0)
                {
                    temp[0] = temp[0].Substring(0, (temp[0].Length - 1));
                    eingabe = temp[0] + temp[1];
                    Console.Write("\b \b" + temp[1] + " ");
                    try
                    {
                        foreach (char c in temp[1])
                        {
                            Console.Write("\b");
                        }
                        Console.Write("\b");
                    }
                    catch (NullReferenceException) { }
                }
                else if (key.Key == ConsoleKey.LeftArrow && temp[0].Length > 0)
                {
                    counter++;
                    temp[0] = eingabe.Substring(0, (eingabe.Length - counter));
                    temp[1] = eingabe.Substring(eingabe.Length - counter);
                    Console.Write("\b");
                }
                else if (key.Key == ConsoleKey.RightArrow && counter != 0)
                {
                    counter--;
                    temp[0] = eingabe.Substring(0, (eingabe.Length - counter));
                    temp[1] = eingabe.Substring(eingabe.Length - counter);
                    Console.Write(temp[0][temp[0].Length - 1]);
                }
                else if (key.Key == ConsoleKey.Delete && counter != 0)
                {
                    counter--;
                    temp[1] = eingabe.Substring(eingabe.Length - counter);
                    eingabe = temp[0] + temp[1];
                    Console.Write(temp[1] + " ");

                    foreach (char c in temp[1])
                    {
                        Console.Write("\b");
                    }
                    Console.Write("\b");
                }
            }
            while (key.Key != ConsoleKey.Enter);
            Console.Write(temp[1]);
            
            if (!cursorWasVisible)
                Console.CursorVisible = false;
            Console.ForegroundColor = Game.DefaultForegroundColor;
            return eingabe;
        }
    }
}
