using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CaravanAdventures.CaravanIncidents
{
    public class DamselInDistressMapParent : CaravansBattlefield
    {
        public Pawn girl = null;
        public List<Pawn> attackers = new List<Pawn>();
        private bool[] diaFlags = new bool[10];
        private float joinDiaRange = 5f;

        public override void Tick()
        {
            base.Tick();
            if (base.HasMap)
            {
                var pawns = this.Map.mapPawns.FreeColonists.ToList();
                if (attackers == null) return;
                if (Find.TickManager.TicksGame % 60 == 0 && IsDefeated(Map, attackers[0].Faction))
                {
                    if (!diaFlags[0] && pawns.Any(pawn => pawn.Position.DistanceTo(girl.Position) > joinDiaRange))
                    {
                        var diaNode = new DiaNode("CaravanDamselInDistress_Saved_AsksToJoin".Translate(girl.NameShortColored));
                        diaNode.options.Add(new DiaOption("Okay".Translate()) { resolveTree = true });
                        diaFlags[0] = true;
                        Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, "CaravanDamselInDistress_Saved_AsksToJoinTitle".Translate(girl)));
                    }
                    else if (!diaFlags[1] && pawns.Any(pawn => pawn.Position.DistanceTo(girl.Position) <= joinDiaRange))
                    {
                        var diaNode = new DiaNode("CaravanDamselInDistress_AsksToJoin".Translate());
                        diaNode.options.Add(new DiaOption("CaravanDamselInDistress_JoinAllow".Translate()) { action = () => DamselInDistressUtility.GirlJoins(pawns, girl), resolveTree = true });
                        diaNode.options.Add(new DiaOption("CaravanDamselInDistress_JoinDeny".Translate()) { resolveTree = true });
                        Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, "CaravanDamselInDistress_AsksToJoinTitle".Translate(girl.NameShortColored)));
                        diaFlags[1] = true;
                    }
                }
            }
        }

        private static bool IsDefeated(Map map, Faction faction)
        {
            List<Pawn> list = map.mapPawns.SpawnedPawnsInFaction(faction);
            for (int i = 0; i < list.Count; i++)
            {
                Pawn pawn = list[i];
                if (GenHostility.IsActiveThreatToPlayer(pawn))
                {
                    return false;
                }
            }
            return true;
        }

        //public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        //{
        //    if (!base.Map.mapPawns.AnyPawnBlockingMapRemoval)
        //    {
        //        alsoRemoveWorldObject = true;
        //        return true;
        //    }
        //    alsoRemoveWorldObject = false;
        //    return false;
        //}



        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref diaFlags, "diaFlags", null, false);
        }
    }
}
