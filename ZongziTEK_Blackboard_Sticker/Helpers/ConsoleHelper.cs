using System;
using System.Diagnostics;

namespace ZongziTEK_Blackboard_Sticker.Helpers
{
    public class ConsoleHelper
    {
        public static void WriteLog(string message, string level)
        {
            // Timestamp
            var timestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            Console.Write($"{timestamp} | ");

            // Level
            var levelColor = level.ToLower() switch
            {
                "info" => ConsoleColor.Green,
                "warn" => ConsoleColor.Yellow,
                "error" => ConsoleColor.Red,
                "debug" => ConsoleColor.Gray,
                _ => ConsoleColor.White
            };

            if (levelColor != ConsoleColor.White)
            {
                Console.ForegroundColor = levelColor;
                Console.Write(level);
                Console.ResetColor();
            }
            else
            {
                Console.Write(level);
            }

            Console.Write(" | ");

            // Source
            var callerTypeName = new StackTrace(1).GetFrame(0)?.GetMethod()?.DeclaringType?.FullName ?? "Unknown";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(callerTypeName);
            Console.ResetColor();

            // Message
            Console.WriteLine($" | {message}");
        }

        public static void WriteZongziTEK()
        {
            string[] zongzitek = {
            @" ________  ________  ________   ________  ________  ___  _________  _______   ___  __",
            @"|\_____  \|\   __  \|\   ___  \|\   ____\|\_____  \|\  \|\___   ___\\  ___ \ |\  \|\  \",
            @" \|___/  /\ \  \|\  \ \  \\ \  \ \  \___| \|___/  /\ \  \|___ \  \_\ \   __/|\ \  \/  /|_",
            @"     /  / /\ \  \\\  \ \  \\ \  \ \  \  ___   /  / /\ \  \   \ \  \ \ \  \_|/_\ \   ___  \",
            @"    /  /_/__\ \  \\\  \ \  \\ \  \ \  \|\  \ /  /_/__\ \  \   \ \  \ \ \  \_|\ \ \  \\ \  \",
            @"   |\________\ \_______\ \__\\ \__\ \_______\\________\ \__\   \ \__\ \ \_______\ \__\\ \__\",
            @"    \|_______|\|_______|\|__| \|__|\|_______|\|_______|\|__|    \|__|  \|_______|\|__| \|__|"
            };

            Console.ForegroundColor = ConsoleColor.Cyan;
            foreach (var line in zongzitek)
            {
                Console.WriteLine(line);
            }
            Console.ResetColor();
        }
    }
}
