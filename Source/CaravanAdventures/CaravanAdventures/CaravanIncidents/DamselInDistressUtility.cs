using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CaravanAdventures.CaravanIncidents
{
    class DamselInDistressUtility
    {
		private const float RelationWithColonistWeight = 20f;
		private const float ChanceToRedressWorldPawn = 0.2f;

		public static DamselInDistressMapParent NewDamselInDistressMapParent(int tile, List<Pawn> attackers, Pawn girl)
        {
			var damselMapParent = (DamselInDistressMapParent)WorldObjectMaker.MakeWorldObject(CaravanIncidentMapParentDefOfs.CADamselInDistressMapParent);
			damselMapParent.Tile = tile;
            damselMapParent.attackers = attackers;
            damselMapParent.girl = girl;
			damselMapParent.SetFaction(attackers[0].Faction);
			Find.WorldObjects.Add(damselMapParent);
			return damselMapParent;
        }

		public static Map SetupCaravanAttackMap(DamselInDistressMapParent mapParent, Caravan caravan, List<Pawn> enemies, bool sendLetterIfRelatedPawns)
		{
			var num = CaravanIncidentUtility.CalculateIncidentMapSize(caravan.PawnsListForReading, enemies);
            var map = GetOrGenerateMapForIncident(caravan, new IntVec3(num, 1, num), CaravanIncidentMapParentDefOfs.CADamselInDistressMapParent);
            //var map = MapGenerator.GenerateMap(new IntVec3(num, 1, num), mapParent, mapParent.MapGeneratorDef, mapParent.ExtraGenStepDefs, null);

			IntVec3 playerStartingSpot;
			IntVec3 root;
			MultipleCaravansCellFinder.FindStartingCellsFor2Groups(map, out playerStartingSpot, out root);
			CaravanEnterMapUtility.Enter(caravan, map, (Pawn x) => CellFinder.RandomSpawnCellForPawnNear(playerStartingSpot, map, 4), CaravanDropInventoryMode.DoNotDrop, true);
			for (int i = 0; i < enemies.Count; i++)
			{
				IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(root, map, 4);
				GenSpawn.Spawn(enemies[i], loc, map, Rot4.Random, WipeMode.Vanish, false);
			}
			if (sendLetterIfRelatedPawns)
			{
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(enemies, "LetterRelatedPawnsGroupGeneric".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, true, true);
			}
			IntVec3 girlLoc = CellFinder.RandomSpawnCellForPawnNear(root, map, 4);
			GenSpawn.Spawn(mapParent.girl, girlLoc, map, Rot4.Random, WipeMode.Vanish, false);
			return map;
		}

		public static Map GetOrGenerateMapForIncident(Caravan caravan, IntVec3 size, WorldObjectDef suggestedMapParentDef)
		{
			int tile = caravan.Tile;
			bool flag = Current.Game.FindMap(tile) == null;
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(tile, size, suggestedMapParentDef);
			if (flag && orGenerateMap != null)
			{
				orGenerateMap.retainedCaravanData.Notify_GeneratedTempIncidentMapFor(caravan);
			}
			return orGenerateMap;
		}

		public static Pawn GenerateGirl(int tile)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, GetFactionForGirl(), PawnGenerationContext.NonPlayer, tile, false, false, false, false, true, 0, true, false, false, true, false, false, false, false, false, 0f, 0f, null, 0, null, null, null, null, null, 20f, null, Gender.Female, null, null, null, null));
			pawn.story.traits.allTraits.RemoveAll(x => x.def == DefDatabase<TraitDef>.GetNamedSilentFail("Beauty"));
			pawn.story.traits.GainTrait(new Trait(DefDatabase<TraitDef>.GetNamedSilentFail("Beauty") , 2)); 
			if (!pawn.story.traits.allTraits.Any(x => x.def == DefDatabase<TraitDef>.GetNamedSilentFail("Tough") || x.def == TraitDefOf.Wimp) && Rand.Chance(0.4f)) pawn.story.traits.GainTrait(new Trait(DefDatabase<TraitDef>.GetNamedSilentFail("Tough")));
			HealthUtility.DamageUntilDowned(pawn, false);
			HealthUtility.DamageLegsUntilIncapableOfMoving(pawn, false);
			return pawn;
		}

		public static Faction GetFactionForGirl()
        {
			var selectedFaction = Find.FactionManager.AllFactions.Where(x => x.HostileTo(Faction.OfPlayer) && x?.def?.humanlikeFaction == true).InRandomOrder().FirstOrDefault();
			if (selectedFaction == null) selectedFaction = Find.FactionManager.AllFactions.FirstOrDefault(x => x == Faction.OfAncients);
			return selectedFaction;
        }

		public static void GirlJoins(List<Pawn> pawns, Pawn girl, Caravan caravan = null, bool prisoner = false)
		{
			var faction = caravan?.Faction ?? pawns.FirstOrDefault(x => x.RaceProps.Humanlike).Faction;
			if (!prisoner)
			{
                girl.SetFaction(faction);
                girl.guest.Released = true;
            }
			else girl.guest.SetGuestStatus(girl.Faction, GuestStatus.Prisoner);

			if (caravan != null)
			{
				if (!girl.IsWorldPawn()) Find.WorldPawns.PassToWorld(girl, PawnDiscardDecideMode.Decide);
				if (girl?.holdingOwner != null) girl.holdingOwner.Remove(girl);
                if (!prisoner) girl.SetFaction(faction);
                caravan.AddPawn(girl, true);
			}
		}


	}
}
