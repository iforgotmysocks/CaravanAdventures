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

        public static void GetAssistanceFromAlliedFaction(Faction faction, Map map, int pointsMin = 4000, int pointsMax = 5000, IntVec3? spawnSpot = null)
        {
            var incidentParms = new IncidentParms
            {
                target = map,
                faction = faction,
                raidArrivalModeForQuickMilitaryAid = true,
                // todo by wealth, the richer, the less help // 7500 - 8000
                points = Rand.Range(pointsMin, pointsMax),  // DiplomacyTuning.RequestedMilitaryAidPointsRange.RandomInRange;
                raidNeverFleeIndividual = true,
                spawnCenter = spawnSpot ?? map.mapPawns.FreeColonists.RandomElement().Position,
            };
            IncidentDefOf.RaidFriendly.Worker.TryExecute(incidentParms);
        }

        public static void AssignDialog(string id, ThingWithComps addressed, string className, string methodName, bool repeatable = false, bool showQuestionMark = true, bool enabled = true, Pawn initiator = null)
        {
            if (addressed == null)
            {
                Log.Warning("Skipping AssignDialog, addressed doesn't exist");
                return;
            }
            var comp = addressed.TryGetComp<CompTalk>();
            if (comp == null)
            {
                Log.Warning($"CompTalk on pawn {CompCache.StoryWC.questCont?.Village?.StoryContact?.Name} is null, which shouldn't happen");
                comp = new CompTalk();
                comp.parent = addressed;
                addressed.AllComps.Add(comp);
            }
            var talkSetToAdd = new TalkSet()
            {
                Id = id,
                Addressed = addressed,
                Initiator = initiator,
                // typeof(QuestCont_FriendlyCaravan).ToString()
                ClassName = className,
                MethodName = methodName,
                Repeatable = repeatable,
            };
            if (comp.actionsCt.Any(action => action.Id == talkSetToAdd.Id)) Log.Warning($"CompTalk dialog id: {talkSetToAdd.Id} already exists");
            else
            {
                comp.actionsCt.Add(talkSetToAdd);
                comp.ShowQuestionMark = showQuestionMark;
                comp.Enabled = enabled;
            }
        }

        internal static void RestartStory()
        {
            if (CompCache.StoryWC.questCont.Village.StoryContact != null) CompCache.StoryWC.questCont.Village.StoryContact.Destroy();
            CompCache.StoryWC.questCont.Village.StoryContact = null;
            CompCache.StoryWC.questCont.FriendlyCaravan.storyContactBondedPawn = null;
            CompCache.StoryWC.ResetStoryVars();
            StoryUtility.RemoveExistingQuestFriendlyVillages();
            StoryUtility.RemoveMapParentsOfDef(CaravanStorySiteDefOf.AncientMasterShrineMP);
            StoryUtility.RemoveMapParentsOfDef(CaravanStorySiteDefOf.AncientMasterShrineWO);
            Log.Message($"Story reset complete");
        }

        public static void GenerateFriendlyVillage(ref float villageGenerationCounter)
        {
            if (!CompCache.StoryWC.storyFlags["TradeCaravan_DialogFinished"] || CompCache.StoryWC.storyFlags["IntroVillage_Created"]) return;
            if (!StoryUtility.TryGenerateDistantTile(out var tile, 6, 15))
            {
                Log.Message($"No tile was generated");
                return;
            }
            if (Faction.OfPlayer.HostileTo(StoryUtility.FactionOfSacrilegHunters))
            {
                Log.Message($"Skipping village generation, Sac hunters are hostile.");
                villageGenerationCounter = 20000;
                return;
            }
            StoryVillageMP settlement = (StoryVillageMP)WorldObjectMaker.MakeWorldObject(CaravanStorySiteDefOf.StoryVillageMP);
            settlement.SetFaction(EnsureSacrilegHunters());
            //settlement.AllComps.Add(new CompStoryVillage());
            settlement.Tile = tile;
            settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement, null);
            Find.WorldObjects.Add(settlement);
            CompCache.StoryWC.questCont.Village.Settlement = settlement;
            Quests.QuestUtility.GenerateStoryQuest(StoryQuestDefOf.CA_StoryVillage_Arrival,
                true,
                "StoryVillageQuestName",
                null,
                "StoryVillageQuestDesc",
                new object[] {
                    CompCache.StoryWC.questCont.Village.StoryContact.NameShortColored,
                    CompCache.StoryWC.questCont.Village.StoryContact.Faction.Name,
                    CompCache.StoryWC.questCont.Village.Settlement.Name.Colorize(UnityEngine.Color.cyan)
                });

            var storyContactBondedPerson = CompCache.StoryWC.questCont.FriendlyCaravan?.storyContactBondedPawn;
            Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_StoryVillage_Arrival, "StoryVillageQuestLetterContent".Translate(storyContactBondedPerson.NameShortColored, CompCache.StoryWC.questCont.Village.Settlement.Label).ToString().HtmlFormatting("add8e6ff", true, 15));
            if (storyContactBondedPerson != null) Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_StoryVillage_Arrival, "StoryVillageQuestLetterContentPS".Translate().ToString().HtmlFormatting("add8e6ff", true, 13));
            Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_StoryVillage_Arrival, "StoryVillageQuestLetterContentEnding".Translate(CompCache.StoryWC.questCont.Village.StoryContact.NameShortColored).ToString().HtmlFormatting("add8e6ff", true, 15));

            Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_StoryVillage_Arrival, "StoryVillageQuestDesc2".Translate(StoryUtility.FactionOfSacrilegHunters.Name.HtmlFormatting("008080"), CompCache.StoryWC.questCont.Village.Settlement.Name.HtmlFormatting("008080"), CompCache.StoryWC.questCont.Village.StoryContact.NameShortColored));

            CompCache.StoryWC.SetSF("IntroVillage_Created");
        }

        public static void FreshenUpPawn(Pawn pawn)
        {
            HealthUtility.HealNonPermanentInjuriesAndRestoreLegs(pawn);
            if (pawn?.needs?.food != null) pawn.needs.food.CurLevel = pawn.needs.food.MaxLevel;
            if (pawn?.needs?.joy != null) pawn.needs.joy.CurLevel = pawn.needs.joy.MaxLevel;
            if (pawn?.needs?.rest != null) pawn.needs.rest.CurLevel = pawn.needs.rest.MaxLevel;
            if (pawn?.needs?.comfort != null) pawn.needs.comfort.CurLevel = pawn.needs.comfort.MaxLevel;
        }

        public static StoryWC GetSWC()
        {
            return Find.World.GetComponent<StoryWC>();
        }

        public static IntVec3 GetCenterOfSettlementBase(Map map, Faction faction)
        {
            var coords = new List<IntVec3>();
            map.regionGrid.allRooms
                .Where(room => !room.Regions
                .Any(region => region.DangerFor(map.mapPawns.AllPawnsSpawned
                .Where(x => x.Faction == faction).FirstOrDefault()) == Danger.Deadly) 
                && !room.UsesOutdoorTemperature)
                .ToList()
                .ForEach(room => coords
                    .Add(new IntVec3(
                        room.Cells.Select(cell => cell.x).Max() - ((room.Cells.Select(cell => cell.x).Max() - room.Cells.Select(cell => cell.x).Min()) / 2),
                        0,
                        room.Cells.Select(cell => cell.z).Max() - ((room.Cells.Select(cell => cell.z).Max() - room.Cells.Select(cell => cell.z).Min()) / 2)
                    )
                )
            );
            coords.ForEach(coord => Log.Message($"Coord: {coord.x} / {coord.z}"));

            var centerPoint = new IntVec3(Convert.ToInt32(coords.Select(coord => coord.x).Average()), 0, Convert.ToInt32(coords.Select(coord => coord.z).Average()));
            Log.Message($"Center: {centerPoint.x} / {centerPoint.z}");

            return centerPoint;
        }

        public static Faction CreateOrGetFriendlyMechFaction()
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

        public static void GenerateStoryContact()
        {
            if (CompCache.StoryWC.questCont.Village.StoryContact != null && !CompCache.StoryWC.questCont.Village.StoryContact.Dead) return;
            var girl = PawnGenerator.GeneratePawn(new PawnGenerationRequest()
            {
                Context = PawnGenerationContext.NonPlayer,
                FixedBiologicalAge = 19,
                FixedChronologicalAge = 3022,
                FixedGender = Gender.Female,
                AllowAddictions = false,
                AllowGay = false,
                AllowDead = false,
                Faction = StoryUtility.EnsureSacrilegHunters(),
                KindDef = StoryDefOf.SacrilegHunters_ExperiencedHunter,
                ProhibitedTraits = new List<TraitDef> { TraitDef.Named("Wimp") },
                MustBeCapableOfViolence = true,
            });

            // todo looks?
            //girl.story.hairDef = 
            Log.Message("Generated main quest pawn");

            girl.story.traits.allTraits.RemoveAll(x => x.def == TraitDefOf.Beauty);
            girl.story.traits.GainTrait(new Trait(TraitDefOf.Beauty, 2));

            if (!Find.WorldPawns.Contains(girl)) Find.WorldPawns.PassToWorld(girl, PawnDiscardDecideMode.KeepForever);
            CompCache.StoryWC.questCont.Village.StoryContact = girl;
        }

        public static Pawn GetGiftedPawn() => PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction?.FirstOrDefault(x => (x?.RaceProps?.Humanlike ?? false) && x.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AncientGift")) != null);

        public static Faction EnsureSacrilegHunters(FactionRelationKind? relationKind = null, bool ignoreBetrayal = false, bool skipLeaderGeneration = false)
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
                if ((!CompCache.StoryWC.storyFlags["SacrilegHuntersBetrayal"] || ignoreBetrayal))
                {
                    if (relationKind != null && sacrilegHunters.RelationKindWith(Faction.OfPlayerSilentFail) != relationKind)
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

        public static void RemoveExistingQuestFriendlyVillages()
        {
            //var settlements = Find.WorldObjects.Settlements.Where(x => x?.Faction?.def?.defName == "SacrilegHunters");
            //Log.Message($"settlement count: {settlements.Count()}");
            //foreach (var settlement in settlements.Reverse())
            //{
            //    Log.Message($"Trying to destroy settlement {settlement.Name}");
            //    Log.Message($"settlement mp: {settlement.def.defName}");
            //    if (settlement.def.defName != "StoryVillageMP") continue;
            //    if (settlement.HasMap) Current.Game.DeinitAndRemoveMap(settlement.Map);
            //    settlement.Destroy();
            //}

            var settlement = CompCache.StoryWC.questCont.Village.Settlement;
            if (settlement != null)
            {
                Log.Message($"Trying to destroy settlement {settlement.Name}");
                Log.Message($"settlement mp: {settlement.def.defName}");
                if (settlement.def.defName != "StoryVillageMP") return;
                if (settlement.HasMap) Current.Game.DeinitAndRemoveMap(settlement.Map);
                settlement.Destroy();
            }
            var destroyedSettlement = CompCache.StoryWC.questCont.Village.DestroyedSettlement;
            if (destroyedSettlement != null)
            {
                Log.Message($"Trying to destroy destroyed settlement");
                Log.Message($"destroyed settlement mp: {destroyedSettlement.def.defName}");
                if (destroyedSettlement.def != WorldObjectDefOf.DestroyedSettlement) return;
                if (destroyedSettlement.HasMap) Current.Game.DeinitAndRemoveMap(destroyedSettlement.Map);
                destroyedSettlement.Destroy();
            }

            Quests.QuestUtility.DeleteQuest(StoryQuestDefOf.CA_TradeCaravan);
            Quests.QuestUtility.DeleteQuest(StoryQuestDefOf.CA_StoryVillage_Arrival);
        }

        public static void RemoveMapParentsOfDef(WorldObjectDef def)
        {
            var sites = Find.WorldObjects.AllWorldObjects.Where(x => x.def == def);
            if (sites == null) return;
            foreach (var site in sites.Reverse<WorldObject>())
            {
                Log.Message($"Destroying site {site.def.label}");
                var mapParent = site as MapParent;
                if (mapParent != null)
                {
                    if (mapParent.HasMap) Current.Game.DeinitAndRemoveMap(mapParent.Map);
                    else Log.Warning($"Didn't have a map to remove: {site.Label} {site.def.defName}");
                }
                else Log.Warning($"Couldn't convert site to MapParent and check for a map to remove: {site.Label} {site.def.defName}");
                site.Destroy();
            }
        }

        public static void CallBombardment(IntVec3 position, Map map, Pawn instigator)
        {
            var bombardment = (Bombardment)GenSpawn.Spawn(ThingDefOf.Bombardment, position, map, WipeMode.Vanish);
            bombardment.impactAreaRadius = 7.9f;
            bombardment.explosionRadiusRange = new FloatRange(5.9f, 5.9f);
            bombardment.bombIntervalTicks = 60;
            bombardment.randomFireRadius = 1;
            bombardment.explosionCount = 6;
            bombardment.warmupTicks = 60;
            bombardment.instigator = instigator;
            SoundDefOf.OrbitalStrike_Ordered.PlayOneShotOnCamera(null);
        }

        public static Pawn GetFittingMechBoss()
        {
            var possibleBosses = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.RaceProps.IsMechanoid && x.defName.ToLower().StartsWith("cabossmech"));
            var selected = possibleBosses.FirstOrDefault(boss => !CompCache.StoryWC.mechBossKillCounters?.Keys?.ToList()?.Contains(boss.defName) ?? false) ?? possibleBosses.RandomElement();
            var bossPawn = PawnGenerator.GeneratePawn(selected, Faction.OfMechanoids);
            if (bossPawn != null) bossPawn.health.AddHediff(DefDatabase<HediffDef>.GetNamed(bossPawn.def.GetModExtension<MechChipModExt>().mechChipDefName));
            return bossPawn;
        }

        public static Faction FactionOfSacrilegHunters { get => Find.FactionManager.FirstFactionOfDef(StoryDefOf.SacrilegHunters); private set => FactionOfSacrilegHunters = value; }
    }
}
