using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld.Planet;
using Verse.AI.Group;

namespace CaravanAdventures.CaravanImprovements
{
    public class UnloadItemsAlert : Alert
    {
        protected override Color BGColor => new Color(0.1f, 1f, 0.1f, 0.1f);

        public UnloadItemsAlert()
        {
            defaultLabel = "DropItemAlert".Translate();
            defaultExplanation = "DropItemDesc".Translate();
            defaultPriority = AlertPriority.High;
        }

        public override AlertReport GetReport()
        {
            var map = Find.CurrentMap;
            if (map == null) return false;

            // todo when a new caravan is being formed, the comp to be disabled
            //var pawns = map.mapPawns.AllPawnsSpawned.Where(x => !x.Dead && (x?.Faction?.IsPlayer ?? false));
            //if (pawns.Any(x => x.GetLord()?.LordJob != null && x.GetLord().LordJob.GetType() == typeof(LordJob_FormAndSendCaravan))) return false;
            return map.GetComponent<CompUnloadItems>()?.Unload ?? false;
        }

        protected override void OnClick()
        {
            var map = Find.CurrentMap;
            if (map == null) return;

            if (Event.current.type == EventType.Used && Event.current.button == 0)
            {
                var pawns = map.mapPawns.AllPawnsSpawned.Where(x => !x.Dead && (x?.Faction?.IsPlayer ?? false));
                foreach (var pawn in pawns)
                {
                    pawn.inventory.UnloadEverything = true;
                }
            }

            var comp = map.GetComponent<CompUnloadItems>();
            if (comp != null) comp.Unload = false;
        }

    }
}
