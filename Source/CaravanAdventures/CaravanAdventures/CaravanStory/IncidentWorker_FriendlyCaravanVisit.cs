using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace CaravanAdventures.CaravanStory
{
    class IncidentWorker_FriendlyCaravanVisit : IncidentWorker_TraderCaravanArrival
    {

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return false;
            return base.CanFireNowSub(parms);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            //base.TryExecuteWorker(parms);
            parms.faction = StoryUtility.FactionOfSacrilegHunters;

            Map map = (Map)parms.target;
            if (!base.TryResolveParms(parms))
            {
                return false;
            }
            if (parms.faction.HostileTo(Faction.OfPlayer))
            {
                return false;
            }
            List<Pawn> list = SpawnPawns(parms);
            if (list.Count == 0)
            {
                return false;
            }
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].needs != null && list[i].needs.food != null)
                {
                    list[i].needs.food.CurLevel = list[i].needs.food.MaxLevel;
                }
            }
            TraderKindDef traderKind = null;
            for (int j = 0; j < list.Count; j++)
            {
                Pawn pawn = list[j];
                if (pawn.TraderKind != null)
                {
                    traderKind = pawn.TraderKind;
                    break;
                }
            }
            this.SendLetter(parms, list, traderKind);
            IntVec3 chillSpot;
            RCellFinder.TryFindRandomSpotJustOutsideColony(list[0], out chillSpot);
            LordJob_TradeWithColony lordJob = new LordJob_TradeWithColony(parms.faction, chillSpot);
            LordMaker.MakeNewLord(parms.faction, lordJob, map, list);
            return true;
        }

        public new List<Pawn> SpawnPawns(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(this.PawnGroupKindDef, parms, true), false).ToList<Pawn>();
            var mainPawn = CompCache.StoryWC.questCont.Village.StoryContact;
            
            list.Add(mainPawn);
            StoryUtility.FreshenUpPawn(mainPawn);
            foreach (Thing newThing in list)
            {
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 5, null);
                GenSpawn.Spawn(newThing, loc, map, WipeMode.Vanish);
            }
            return list;
        }

        protected override void ResolveParmsPoints(IncidentParms parms)
        {
            if (parms.points < 1000) base.ResolveParmsPoints(parms);
        }
    }
}
