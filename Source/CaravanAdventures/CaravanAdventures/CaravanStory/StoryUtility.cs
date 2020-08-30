using CaravanAdventures.CaravanStory.MechChips;
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
            if (StoryWC.storyFlags["IntroVillage_Created"]) return;
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
            StoryUtility.GetSWC().questCont.Village.Settlement = settlement;
            Quests.QuestUtility.GenerateStoryQuest(StoryQuestDefOf.CA_StoryVillage_Arrival);

            StoryWC.SetSF("IntroVillage_Created");
        }

        internal static StoryWC GetSWC()
        {
            return Find.World.GetComponent<StoryWC>();
        }

        internal static IntVec3 GetCenterOfSettlementBase(Map map)
        {
            var coords = new List<IntVec3>();
            // todo improve, doesn't quite find the middle yet - maybe use map size and just calculate middle?
            map.regionGrid.allRooms
                .Where(room => !room.Regions
                .Any(region => region.DangerFor(map.mapPawns.AllPawnsSpawned
                .Where(x => x.Faction.def.defName == "SacrilegHunters").FirstOrDefault()) == Danger.Deadly))
                .ToList()
                .ForEach(room => coords
                .Add(new IntVec3(
                    room.Cells.Select(cell => cell.x).Max() - ((room.Cells.Select(cell => cell.x).Max() - room.Cells.Select(cell => cell.x).Min()) / 2),
                    0,
                    room.Cells.Select(cell => cell.z).Max() - ((room.Cells.Select(cell => cell.z).Max() - room.Cells.Select(cell => cell.z).Min()) / 2)
                ))
            );
            coords.ForEach(coord => Log.Message($"Coord: {coord.x} / {coord.z}"));

            var centerPoint = new IntVec3(Convert.ToInt32(coords.Select(coord => coord.x).Average()), 0, Convert.ToInt32(coords.Select(coord => coord.z).Average()));
            Log.Message($"Center: {centerPoint.x} / {centerPoint.z}");

            return centerPoint;
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

        internal static void AssignVillageDialog()
        {
            if (StoryUtility.GetSWC().questCont.Village.StoryContact == null)
            {
                Log.Message("Skipping, pawn doesn't exist");
                return;
            }
            var comp = StoryUtility.GetSWC().questCont.Village.StoryContact.TryGetComp<CompTalk>();
            if (comp == null)
            {
                Log.Warning($"CompTalk on pawn {StoryUtility.GetSWC().questCont?.Village?.StoryContact?.Name} is null, which shouldn't happen");
                comp = new CompTalk();
                StoryUtility.GetSWC().questCont.Village.StoryContact.AllComps.Add(comp);
            }
            comp.actions.Add(new TalkSet()
            {
                Id = "StoryStart_PawnDia",
                Addressed = StoryUtility.GetSWC().questCont.Village.StoryContact,
                Initiator = null,
                ClassName = typeof(StoryVillageMP).ToString(),
                MethodName = "ConversationFinished",
                Repeatable = false,
            });
            comp.ShowQuestionMark = true;
            comp.Enabled = true;
        }

        internal static void GenerateStoryContact()
        {
            if (StoryUtility.GetSWC().questCont.Village.StoryContact != null && !StoryUtility.GetSWC().questCont.Village.StoryContact.Dead) return;
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
            Log.Message("Generated main quest pawn");

            girl.story.traits.allTraits.RemoveAll(x => x.def == TraitDefOf.Beauty);
            girl.story.traits.GainTrait(new Trait(TraitDefOf.Beauty, 2));

            if (!Find.WorldPawns.Contains(girl)) Find.WorldPawns.PassToWorld(girl, PawnDiscardDecideMode.KeepForever);
            StoryUtility.GetSWC().questCont.Village.StoryContact = girl;
        }

        internal static Pawn GetGiftedPawn() => PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction?.FirstOrDefault(x => (x?.RaceProps?.Humanlike ?? false) && x.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AncientGift")) != null);

        public static Faction EnsureSacrilegHunters(FactionRelationKind relationKind = FactionRelationKind.Neutral, bool ignoreBetrayal = false, bool skipLeaderGeneration = false)
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
            if ((sacrilegHunters.leader == null || sacrilegHunters.leader.Dead || sacrilegHunters.leader.Destroyed) && !skipLeaderGeneration)
            {
                try
                {
                    sacrilegHunters.TryGenerateNewLeader();
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }
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

        internal static void RemoveExistingQuestFriendlyVillages()
        {
            var settlements = Find.WorldObjects.Settlements.Where(x => x?.Faction?.def?.defName == "SacrilegHunters");
            Log.Message($"settlement count: {settlements.Count()}");
            foreach (var settlement in settlements.Reverse())
            {
                Log.Message($"Trying to destroy settlement {settlement.Name}");
                Log.Message($"settlement mp: {settlement.def.defName}");
                if (settlement.def.defName != "StoryVillageMP") continue;
                settlement.forceRemoveWorldObjectWhenMapRemoved = true;
                settlement.Destroy();
                settlement.CheckRemoveMapNow();
            }
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
