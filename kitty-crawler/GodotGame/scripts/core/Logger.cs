using System;
using Godot;

namespace Game.Core
{
    public static class Logger
    {
        public static void Log(LogLevel level, int frame = 2, params object[] message)
        {
            var dateTime = DateTime.Now;
            string timeStamp = $"[{dateTime:yyyy-MM-dd HH:mm:ss}]";
            var callingMethod = new System.Diagnostics.StackTrace().GetFrame(frame).GetMethod();
            string callerType = callingMethod?.DeclaringType?.Name ?? "Unknown";
            string callerName = callingMethod?.Name ?? "Unknown";

            string logMessage = $"{timeStamp} [{level}] [{callerType}] [{callerName}]";

            string color = "CYAN";

            switch (level)
            {
                case LogLevel.DEBUG:
                    color = "WHITE";
                    break;
                case LogLevel.INFO:
                    color = "CYAN";
                    break;
                case LogLevel.WARNING:
                    color = "YELLOW";
                    break;
                case LogLevel.ERROR:
                    color = "RED";
                    break;
                default:
                    break;


            }

            GD.PrintRich([$"[color={color}]{logMessage}[/color]", ..message]);
        }

        public static void Debug(params object[] message)
        {
            Log(LogLevel.DEBUG, 2, message);
        }

        public static void Info(params object[] message)
        {
            Log(LogLevel.INFO, 2, message);
        }

        public static void Warning(params object[] message)
        {
            Log(LogLevel.WARNING, 2, message);
        }

        public static void Error(params object[] message)
        {
            Log(LogLevel.ERROR, 2, message);
        }
    }
}
