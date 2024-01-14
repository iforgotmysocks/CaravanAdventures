using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static System.Net.Mime.MediaTypeNames;

namespace CaravanAdventures.CaravanStory
{
    public class IncidentWorker_UnusualInfestation : IncidentWorker_Infestation
    {
        public new const float HivePoints = 220f;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 cell;
            if (base.CanFireNowSub(parms) && Faction.OfInsects != null && HiveUtility.TotalSpawnedHivesCount(map) < 30)
            {
                return InfestationCellFinder.TryFindCell(out cell, map);
            }
            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            Thing thing = InfestationUtility.SpawnTunnels(Mathf.Max(GenMath.RoundRandom(parms.points / HivePoints), 1), map, spawnAnywhereIfNoGoodCell: true, parms.infestationLocOverride.HasValue, null, parms.infestationLocOverride);

            ChoiceLetter choiceLetter = LetterMaker.MakeLetter((parms.customLetterLabel ?? "CALabelStrangeInfestation").Translate(), (parms.customLetterText ?? "CATextStrangeInfestation").Translate(), parms.customLetterDef ?? LetterDefOf.ThreatBig, new LookTargets() { targets = new List<RimWorld.Planet.GlobalTargetInfo> { thing } }, parms.faction, parms.quest, parms.letterHyperlinkThingDefs);
            Find.LetterStack.ReceiveLetter(choiceLetter);

            Find.TickManager.slower.SignalForceNormalSpeedShort();
            return true;
        }
    }
}
