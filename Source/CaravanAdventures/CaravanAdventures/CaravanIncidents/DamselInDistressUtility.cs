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
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, DownedRefugeeQuestUtility.GetRandomFactionForRefugee(), PawnGenerationContext.NonPlayer, tile, false, false, false, false, true, false, 20f, true, true, true, true, false, false, false, false, 0f, null, 1f, null, null, null, null, new float?(0.2f), null, null, Gender.Female, null, null, null, null));
			pawn.story.traits.allTraits.RemoveAll(x => x.def == TraitDefOf.Beauty);
			pawn.story.traits.GainTrait(new Trait(TraitDefOf.Beauty, 2));
			if (!pawn.story.traits.allTraits.Any(x => x.def == TraitDefOf.Tough) && Rand.Range(1,5) > 2) pawn.story.traits.GainTrait(new Trait(TraitDefOf.Tough));
			HealthUtility.DamageUntilDowned(pawn, false);
			HealthUtility.DamageLegsUntilIncapableOfMoving(pawn, false);
			return pawn;
		}

		public static void GirlJoins(List<Pawn> pawns, Pawn girl, Caravan caravan = null)
		{
			var faction = pawns.FirstOrDefault()?.Faction;
			girl.SetFaction(faction);

			if (caravan != null) caravan.AddPawn(girl, true);
		}


	}
}
