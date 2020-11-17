using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Verse;

namespace CaravanAdventures
{
    static class Helper
    {
        public static bool Debug() => ModSettings.Get().debug;

        public static void AdjustSettlementPrices()
        {
            if (Find.World != null)
            {
                var npcSettlements = Find.WorldObjects.AllWorldObjects.Where(settlement => settlement.def == WorldObjectDefOf.Settlement && !settlement.Faction.IsPlayer);
                foreach (var settlement in npcSettlements.OfType<Settlement>())
                {
                    var silverStacks = settlement.trader.StockListForReading.Where(def => def.def == ThingDefOf.Silver);
                    foreach (var stack in silverStacks)
                    {
                        var before = stack.stackCount;
                        if (stack.stackCount < 3000) stack.stackCount *= 3;
                        else stack.stackCount *= 2;

                        Log.Message($"Settlement: {settlement.Name} of {settlement.Faction} increased silver from {before} to {stack.stackCount}");
                    }
                }

            }
        }

        public static IEnumerable<T> PickSomeInRandomOrder<T>(IEnumerable<T> items, int count)
        {
            var random = new System.Random(DateTime.Now.Millisecond);
            var randomSortTable = new Dictionary<double, T>();

            foreach (var item in items)
            {
                randomSortTable[random.NextDouble()] = item;
            }

            return randomSortTable.OrderBy(x => x.Key).Take(count).Select(x => x.Value);
        }

        public static void ExportSettings(ModSettings settings, string settingsLocation, bool updateBeforeExport = true)
        {
            try
            {
                if (!Directory.Exists(settingsLocation)) Directory.CreateDirectory(Path.GetDirectoryName(settingsLocation));

                var writer = new XmlSerializer(typeof(ModSettings));
                using (var file = System.IO.File.Create(settingsLocation))
                {
                    if (updateBeforeExport)
                    {
                    }

                    writer.Serialize(file, settings);
                    file.Close();
                }
            }
            catch (Exception e)
            {
                Log.Message(e.ToString());
            }
        }

        public static ModSettings ImportSettings(ModSettings settings, string settingsLocation)
        {

            if (!File.Exists(settingsLocation))
            {
                // todo maybe throw this out? it's just here to set standard values -> also adjust ExportSettings (updateBeforeExport)
                ExportSettings(settings, settingsLocation, false);
                return settings;
            }
            else
            {
                object importedObj = null;
                var writer = new XmlSerializer(typeof(ModSettings));
                using (StreamReader sr = new StreamReader(settingsLocation))
                {
                    importedObj = writer.Deserialize(sr);
                }

                return (ModSettings)importedObj;
            }
        }

        public static string HtmlFormatting(this string s, string code, bool italic = false, int size = 0, bool bolt = false)
        {
            var buildString = string.Format("<color=#{0}>{1}</color>", code, s);
            if (size != 0) buildString = string.Format("<size={0}>{1}</size>", size, buildString);
            if (italic) buildString = "<i>" + buildString + "</i>";
            if (bolt) buildString = "<b>" + buildString + "</b>";
            return buildString;
        }
    }
}
