using Verse;

namespace CaravanAdventures
{
    class DLog
    {
        private static bool shouldLog => ModSettings.debugMessages;
        public static void Message(string message)
        {
            if (shouldLog) Log.Message(message);
        }

        public static void Warning(string message)
        {
            if (shouldLog) Log.Warning(message);
        }

        public static void Error (string message)
        {
            if (shouldLog) Log.Error(message);
        }

    }
}
