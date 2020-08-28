﻿using CaravanAdventures.CaravanStory.MechChips;
using CaravanAdventures.CaravanStory.Quests;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;

namespace CaravanAdventures.CaravanStory
{
    static class StoryUtility
    {
        public static bool CanSpawnSpotCloseToCaskets(Room mainRoom, Map map, out IntVec3 pos)
        {
            var casket = mainRoom.ContainedThings(ThingDefOf.AncientCryptosleepCasket).RandomElement();
            pos = default;
            if (casket != null)
            {
                for (int i = 0; i < 50; i++)
                {
                    CellFinder.TryFindRandomSpawnCellForPawnNear_NewTmp(casket.Position, map, out var result, 4);
                    if (mainRoom.Cells.Contains(result))
                    {
                        pos = result;
                        return true;
                    }
                }
            }
            else
            {
                //pos = mainRoom.Cells.Where(x => x.Standable(map) && !x.Filled(map)).InRandomOrder().FirstOrDefault();
                return false;
            }

            return false;
        }

        internal static void GenerateFriendlyVillage()
        {
            if (StoryWC.storyFlags.TryGetValue("IntroVillage_Created", out var res) && res) return;
            if (!StoryUtility.TryGenerateDistantTile(out var tile, 6, 15))
            {
                Log.Message($"No tile was generated");
                return;
            }
            StoryVillageMP settlement = (StoryVillageMP)WorldObjectMaker.MakeWorldObject(CaravanStorySiteDefOf.StoryVillageMP);
            settlement.SetFaction(EnsureSacrilegHunters());
            //settlement.AllComps.Add(new CompStoryVillage());
            settlement.Tile = tile;
            settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement, null);
            Find.WorldObjects.Add(settlement);

            var pawn = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.FirstOrDefault();
            //Quests.QuestUtility.GenerateStoryVillageQuest(pawn, pawn.Faction);
            Quests.QuestUtility.GenerateStoryQuest(StoryQuestDefOf.CA_StoryVillage_Arrival);

            StoryWC.SetSF("IntroVillage_Created");
        }

        internal static Faction CreateOrGetFriendlyMechFaction()
        {
            var relations = new List<FactionRelation>();
            foreach (var curFaction in Find.FactionManager.AllFactionsListForReading)
            {
                if (curFaction.def.permanentEnemy) continue;
                relations.Add(new FactionRelation
                {
                    other = curFaction,
                    goodwill = curFaction == Faction.OfPlayer ? 100 : Faction.OfPlayer.GoodwillWith(curFaction),
                    kind = curFaction == Faction.OfPlayer ? FactionRelationKind.Ally : Faction.OfPlayer.RelationKindWith(curFaction)
                });
            }

            var faction = Find.FactionManager.AllFactionsListForReading.FirstOrDefault(x => x.def.defName == "FriendlyMechanoid");
            if (faction == null)
            {
                faction = FactionGenerator.NewGeneratedFactionWithRelations(FactionDef.Named("FriendlyMechanoid"), relations);
                faction.hidden = new bool?(true);
                faction.temporary = true;
                faction.hostileFromMemberCapture = false;
                Find.FactionManager.Add(faction);
            }
            else
            {
                foreach (var relation in relations)
                {
                    if (faction == relation.other) continue;
                    faction.SetRelation(relation);
                }
            }

            return faction;
        }

        public static bool TryGenerateDistantTile(out int newTile, int minDist, int maxDist)
        {
            int startTile = -1;
            Caravan caravan;
            if (Find.AnyPlayerHomeMap == null)
            {
                Log.Message($"Caravan count: {Find.WorldObjects.Caravans.Count}");
                caravan = Find.WorldObjects.Caravans.Where(x => x.Faction == Faction.OfPlayer).ToList().OrderByDescending(x => x.PawnsListForReading.Count).FirstOrDefault();
                if (caravan != null) startTile = caravan.Tile;
                else Log.Message($"caraavn is null");
            }
            return TileFinder.TryFindNewSiteTile(out newTile, minDist, maxDist, false, false, startTile);
        }

        internal static void GenerateStoryContact()
        {
            if (StoryWC.StoryContact != null && !StoryWC.StoryContact.Dead) return;
            var girl = PawnGenerator.GeneratePawn(new PawnGenerationRequest()
            {
                FixedBiologicalAge = 22,
                FixedChronologicalAge = 3022,
                FixedGender = Gender.Female,
                AllowAddictions = false,
                AllowGay = false,
                AllowDead = false,
                Faction = StoryUtility.EnsureSacrilegHunters(FactionRelationKind.Ally),
                KindDef = PawnKindDefOf.Villager,
            });

            // todo looks?
            //girl.story.hairDef = 

            girl.story.traits.allTraits.RemoveAll(x => x.def == TraitDefOf.Beauty);
            girl.story.traits.GainTrait(new Trait(TraitDefOf.Beauty, 2));

            StoryWC.StoryContact = girl;
        }

        internal static Pawn GetGiftedPawn() => PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction?.FirstOrDefault(x => (x?.RaceProps?.Humanlike ?? false) && x.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AncientGift")) != null);

        public static Faction EnsureSacrilegHunters(FactionRelationKind relationKind = FactionRelationKind.Neutral, bool ignoreBetrayal = false)
        {
            var sacrilegHunters = Find.FactionManager.AllFactions.FirstOrDefault(x => x.def.defName == "SacrilegHunters");
            if (sacrilegHunters == null)
            {
                sacrilegHunters = FactionGenerator.NewGeneratedFaction(DefDatabase<FactionDef>.GetNamedSilentFail("SacrilegHunters"));
                Find.FactionManager.Add(sacrilegHunters);
                var empireDef = FactionDefOf.Empire;
                empireDef.permanentEnemyToEveryoneExcept.Add(sacrilegHunters.def);
                Faction.Empire.TrySetNotHostileTo(sacrilegHunters);
            }
            if (sacrilegHunters.leader == null || sacrilegHunters.leader.Dead || sacrilegHunters.leader.Destroyed) sacrilegHunters.TryGenerateNewLeader();
            if (Faction.OfPlayerSilentFail != null && sacrilegHunters.RelationWith(Faction.OfPlayer, true) == null) sacrilegHunters.TryMakeInitialRelationsWith(Faction.OfPlayer);
            if (sacrilegHunters != null && Faction.OfPlayerSilentFail != null)
            {
                if ((!StoryWC.storyFlags["SacrilegHuntersBetrayal"] || ignoreBetrayal))
                {
                    if (sacrilegHunters.RelationKindWith(Faction.OfPlayerSilentFail) != relationKind)
                    {
                        switch (relationKind)
                        {
                            case FactionRelationKind.Ally:
                                sacrilegHunters.SetRelation(new FactionRelation() { kind = FactionRelationKind.Ally, goodwill = 100, other = Faction.OfPlayer });
                                break;
                            case FactionRelationKind.Neutral:
                                sacrilegHunters.SetRelation(new FactionRelation() { kind = FactionRelationKind.Neutral, goodwill = 0, other = Faction.OfPlayer });
                                break;
                            case FactionRelationKind.Hostile:
                                sacrilegHunters.SetRelation(new FactionRelation() { kind = FactionRelationKind.Hostile, goodwill = -100, other = Faction.OfPlayer });
                                break;
                        }
                    }
                }
                else if (sacrilegHunters.RelationKindWith(Faction.OfPlayerSilentFail) != FactionRelationKind.Hostile) sacrilegHunters.SetRelation(new FactionRelation() { kind = FactionRelationKind.Hostile, goodwill = -100, other = Faction.OfPlayer });
            }
            return sacrilegHunters;
        }

        internal static void CallBombardment(IntVec3 position, Map map, Pawn instigator)
        {
            Bombardment bombardment = (Bombardment)GenSpawn.Spawn(ThingDefOf.Bombardment, position, map, WipeMode.Vanish);
            bombardment.impactAreaRadius = 7.9f;
            bombardment.explosionRadiusRange = new FloatRange(5.9f, 5.9f);
            bombardment.bombIntervalTicks = 60;
            bombardment.randomFireRadius = 1;
            bombardment.explosionCount = 6;
            bombardment.warmupTicks = 60;
            bombardment.instigator = instigator;
            SoundDefOf.OrbitalStrike_Ordered.PlayOneShotOnCamera(null);
        }

        internal static Pawn GetFittingMechBoss()
        {
            var possibleBosses = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.RaceProps.IsMechanoid && x.defName.ToLower().StartsWith("cabossmech"));
            var selected = possibleBosses.FirstOrDefault(boss => !StoryWC.mechBossKillCounters?.Keys?.ToList()?.Contains(boss.defName) ?? false) ?? possibleBosses.RandomElement();
            var bossPawn = PawnGenerator.GeneratePawn(selected, Faction.OfMechanoids);
            if (bossPawn != null) bossPawn.health.AddHediff(DefDatabase<HediffDef>.GetNamed(bossPawn.def.GetModExtension<MechChipModExt>().mechChipDefName));
            return bossPawn;
        }
    }
}
