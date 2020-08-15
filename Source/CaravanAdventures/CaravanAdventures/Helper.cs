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
    class Helper
    {
        public static bool Debug() => ModSettings.Get().debug;
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
    }
}
