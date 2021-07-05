using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures
{
    class DLog
    {
        private static bool shouldLog => ModSettings.debugMessages;
        public static void Message(string message, bool ignoreLimit = false)
        {
            if (shouldLog) Log.Message(message);
        }

        public static void Warning(string message, bool ignoreLimit = false)
        {
            if (shouldLog) Log.Warning(message);
        }

        public static void Error (string message, bool ignoreLimit = false)
        {
            if (shouldLog) Log.Error(message);
        }

    }
}
