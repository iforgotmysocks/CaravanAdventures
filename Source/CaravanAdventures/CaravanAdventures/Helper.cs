using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Verse;

namespace CaravanAdventures
{
    static class Helper
    {
        public static bool Debug() => ModSettings.debug;

        public static void AdjustSettlementPrices()
        {
            if (Find.World != null)
            {
                var npcSettlements = Find.WorldObjects.AllWorldObjects.Where(settlement => settlement.def == WorldObjectDefOf.Settlement && !settlement.Faction.IsPlayer);
                foreach (var settlement in npcSettlements.OfType<Settlement>())
                {
                    if (!settlement.trader.EverVisited) continue;
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

        public static System.Reflection.Assembly GetAssembly(string assemblyString, List<(string assemblyString, System.Reflection.Assembly assembly)> detectedAssemblies)
        {
            var selAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.FullName.ToLower().StartsWith(assemblyString.ToLower()));
            if (selAssembly == null) return null;
            detectedAssemblies.Add((assemblyString, selAssembly));
            return selAssembly;
        }

        public static IEnumerable<T> PickSomeInRandomOrder<T>(this IEnumerable<T> items, int count)
        {
            var random = new System.Random(DateTime.Now.Millisecond);
            var randomSortTable = new Dictionary<double, T>();
            foreach (var item in items) randomSortTable[random.NextDouble()] = item;
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
                Log.Error(e.ToString());
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

        internal static void PrintWorldPawns()
        {
            if (Find.World == null) return;
            DLog.Message($"wp total: {Find.World.worldPawns.AllPawnsAliveOrDead.Count} " +
                $"wp alive: {Find.World.worldPawns.AllPawnsAlive.Count} " +
                $"dead: {Find.World.worldPawns.AllPawnsDead.Count}");
            DLog.Message($"wp player: {Find.World.worldPawns.AllPawnsAliveOrDead.Where(x => x.Faction == Faction.OfPlayer).Count()} " +
                $"wp alive: {Find.World.worldPawns.AllPawnsAlive.Where(x => x.Faction == Faction.OfPlayer).Count()} " +
                $"dead: {Find.World.worldPawns.AllPawnsDead.Where(x => x.Faction == Faction.OfPlayer).Count()}");

            foreach (var pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists)
            {
                DLog.Message($"Name: {pawn.NameShortColored} Temp: {pawn.AmbientTemperature} Spawned: {pawn.Spawned}");
            }
        }

        public static void RunSafely(Action action, bool suppressError = false, string failMessage = "", bool useDLog = false) => RunSafely(() => { action(); return 0; }, suppressError, failMessage, useDLog);

        public static T RunSafely<T>(Func<T> action, bool suppressError = false, string failMessage = "", bool useDLog = false)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                if (!suppressError)
                {
                    if (!useDLog) Log.Error(failMessage + ex.ToString());
                    else DLog.Error(failMessage + ex.ToString());
                }
            }

            return default(T);
        }

        public static void RunSavelyWithDelay(Action action, ref float counter, int timeout = 2000, bool suppressError = false, string failMessage = "", bool useDLog = false)
        {
            RunSavelyWithDelay(() => { action(); return 0; }, ref counter, timeout, suppressError, failMessage, useDLog);
        }

        public static T RunSavelyWithDelay<T>(Func<T> action, ref float counter, int timeout = 2000, bool suppressError = false, string failMessage = "", bool useDLog = false)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                counter += timeout;
                if (!suppressError)
                {
                    if (!useDLog) Log.Error(failMessage + ex.ToString());
                    else DLog.Error(failMessage + ex.ToString());
                }
            }

            return default(T);
        }

        public static IEnumerable<ThingDef> FilteredStuffs(this IEnumerable<ThingDef> things, IEnumerable<StuffCategoryDef> stuffCatsToExlude, IEnumerable<ThingDef> stuffsToExclude = null)
        {
            if (!things?.Any() ?? true)
            {
                Log.Error($"Tried to process faulty enumerable.");
                return null;
            }
            var res = things.ToList();
            var fallback = things.OrderBy(x => x?.BaseMarketValue ?? 0).FirstOrDefault();
            if (stuffCatsToExlude != null) res.RemoveAll(x => x.stuffCategories != null
                && x.stuffCategories.Any(sc => stuffCatsToExlude.Contains(sc)));
            if (stuffsToExclude != null) res.RemoveAll(x => stuffsToExclude.Contains(x));
            if (!res.Any()) res.Add(fallback);
            return res;
        }
    }
}
