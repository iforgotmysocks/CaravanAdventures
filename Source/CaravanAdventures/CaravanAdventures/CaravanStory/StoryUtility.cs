﻿using CaravanAdventures.CaravanStory.MechChips;
using CaravanAdventures.CaravanStory.Quests;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;
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
                // todo look into this shit
                for (int i = 0; i < 50; i++)
                {
                    CellFinder.TryFindRandomSpawnCellForPawnNear(casket.Position, map, out var result, 4);
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

        public static IntVec3 GetAllowedMapSizeConcideringSettings()
        {
            var testedMaxSize = new IntVec3(275, 1, 275);
            var minimumSize = new IntVec3(250, 1, 250);
            var resultSize = Find.World.info.initialMapSize;

            DLog.Message($"current map size in getallowed {resultSize}");

            if ((resultSize.x * resultSize.z) < (minimumSize.x * minimumSize.y))
            {
                DLog.Message($"Using minimum map size 250x250");
                return minimumSize;
            }
            if (!ModSettings.limitLargeMapSizesToTestedSize) return resultSize;
            if ((resultSize.x * resultSize.z) > (testedMaxSize.x * testedMaxSize.z)) return testedMaxSize;
            return resultSize;
        }

        public static void AddBountyPointsForKilledMech(Pawn mech)
        {
            var reward = 0f;
            if (mech.def == ThingDef.Named("Mech_Pikeman")) reward += 7;
            else if (mech.def == ThingDef.Named("Mech_Scyther")) reward += 10;
            else if (mech.def == ThingDef.Named("Mech_Lancer")) reward += 15;
            else if (mech.def == ThingDef.Named("Mech_CentipedeBlaster")) reward += 45;
            else if (mech.def == ThingDef.Named("CAEndBossMech")) reward += 1000;
            else if (CompCache.StoryWC.BossDefs().Contains(mech.def)) reward += 500;
            else
            {
                var addedBounty = CompatibilityDefOf.CACompatDef.mechanoidBountyToAdd.FirstOrDefault(x => x?.raceDefName != null && x?.raceDefName == mech.def.defName);
                if (addedBounty != null) reward += addedBounty.bountyPoints;
                else reward += 12;
            }
            CompCache.BountyWC.BountyPoints += reward;
            if (!ModSettings.showBountyRewardInfo) return;
            Messages.Message($"{"CABountyKillMessageBounty".Translate() + Helper.HtmlFormatting(mech?.Label.CapitalizeFirst(), "FF8C00")}  {"CABountyKillMessageReward".Translate() + Helper.HtmlFormatting(reward.ToString(), "FF8C00")}  {"CABountyKillMessageCredit".Translate() + Helper.HtmlFormatting(CompCache.BountyWC.BountyPoints.ToString(), "FF8C00")}", MessageTypeDefOf.SilentInput);
            //Messages.Message("CABountyKillMessage".Translate(reward, mech?.Label, CompCache.BountyWC.BountyPoints), MessageTypeDefOf.SilentInput);
        }

        public static void GetAssistanceFromAlliedFaction(Faction faction, Map map, float pointsMin = 4000, float pointsMax = 5000, IntVec3 spawnSpot = default)
        {
            var incidentParms = new IncidentParms
            {
                target = map,
                faction = faction,
                raidArrivalModeForQuickMilitaryAid = true,
                // todo by wealth, the richer, the less help // 7500 - 8000
                points = Rand.Range(pointsMin, pointsMax),
                raidNeverFleeIndividual = true,
                raidStrategy = RaidStrategyDefOf.ImmediateAttackFriendly,
                raidArrivalMode = PawnsArrivalModeDefOf.EdgeDrop
            };
            DLog.Message($"Assistance with {incidentParms.points} points and kind: {incidentParms.pawnKind?.defName}, targetspot default? {spawnSpot == default} if yes, a colonist should be selected as drop spot");
            if (spawnSpot == default && map.mapPawns.AnyColonistSpawned) spawnSpot = map.mapPawns.FreeColonists.Where(col => col.Spawned).RandomElement().Position;
            if (spawnSpot != default) incidentParms.spawnCenter = spawnSpot;
            if (Helper.RunSafely(() => IncidentDefOf.RaidFriendly.Worker.TryExecute(incidentParms)) != true)
            {
                Log.Error($"Dropping in the sacrileg hunter faction failed due to some incompatibility, error above.");
            };
        }

        public static void AssignDialog(string id, ThingWithComps addressed, string className, string methodName, bool repeatable = false, bool showQuestionMark = true, bool enabled = true, Pawn initiator = null, bool skipAlreadyExistsWarning = false)
        {
            if (addressed == null)
            {
                DLog.Warning("Skipping AssignDialog, addressed doesn't exist");
                return;
            }
            var comp = addressed.TryGetComp<CompTalk>();
            if (comp == null)
            {
                Log.Warning($"CompTalk on addressed {(addressed is Pawn pawn ? pawn.NameShortColored.ToString() : addressed.def.defName)} is null, which shouldn't happen");
                comp = new CompTalk();
                comp.parent = addressed;
                addressed.AllComps.Add(comp);
            }
            if (comp.actionsCt.Any(action => action.Id == id))
            {
                if (!skipAlreadyExistsWarning) Log.Warning($"CompTalk dialog id: {id} already exists");
                return;
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

            comp.actionsCt.Add(talkSetToAdd);
            comp.ShowQuestionMark = showQuestionMark;
            comp.Enabled = enabled;
        }

        public static bool RemoveDialog(ThingWithComps addressed, string id)
        {
            var comp = addressed.TryGetComp<CompTalk>();
            if (comp == null)
            {
                Log.Warning($"CompTalk on addressed {(addressed is Pawn pawn ? pawn.NameShortColored.ToString() : addressed.def.defName)} is null, canceling.");
                return false;
            }

            if (comp.actionsCt.RemoveAll(x => x.Id == id) > 0) return true;
            return false;
        }

        public static void RemoveAllDialogs(ThingWithComps addressed)
        {
            var comp = addressed.TryGetComp<CompTalk>();
            if (comp == null)
            {
                Log.Warning($"CompTalk on addressed {(addressed is Pawn pawn ? pawn.NameShortColored.ToString() : addressed.def.defName)} is null, canceling.");
                return;
            }

            comp.actionsCt.Clear();
        }

        internal static void AdjustGoodWill(int amount, Faction faction = null)
        {
            faction = FactionOfSacrilegHunters ?? null;
            if (faction == null) return;
            faction.TryAffectGoodwillWith(Faction.OfPlayer, amount);
        }

        public static void ResetCurrentStoryStageSubProgress()
        {
            if (!CompCache.StoryWC.storyFlags["TradeCaravan_DialogFinished"]) return;

            // todo clean up created world objects and reset quest progress for started stages
            // probably possible to just remove started quests, due to reset flags they should be created again

            // todo sacrileg hunter relations are screwed after planet switch? No longer attacking mechs??
            // --> Actually a SoS2 bug!

            if (CompCache.StoryWC.storyFlags["IntroVillage_InitCountDownStarted"] && !CompCache.StoryWC.storyFlags["IntroVillage_Finished"]) CompCache.StoryWC.SetSFsStartingWith("IntroVillage");
            else if (CompCache.StoryWC.storyFlags["Start_InitialTreeWhisper"] && !CompCache.StoryWC.storyFlags["Start_ReceivedGift"]) CompCache.StoryWC.SetSFsStartingWith("Start");
            else if (CompCache.StoryWC.storyFlags[CompCache.StoryWC.BuildCurrentShrinePrefix() + "InitCountDownStarted"] && !CompCache.StoryWC.storyFlags[CompCache.StoryWC.BuildCurrentShrinePrefix() + "Completed"])
            {
                CompCache.StoryWC.SetSFsStartingWith(CompCache.StoryWC.BuildCurrentShrinePrefix());
                if (CompCache.StoryWC.storyFlags["Judgment_PortalRevealed"] && !CompCache.StoryWC.storyFlags["Judgment_StoryOverDialog"]) CompCache.StoryWC.SetSFsStartingWith("Judgment");
            }

            CompCache.StoryWC.questCont.Village.StoryContact = null;

            // todo this fix doesn't currently work as the postfix is executed before the faction reset for the new planet actually happens.
            //StoryUtility.SetMechFactionHostileToHunters();
        }

        private static void SetMechFactionHostileToHunters()
        {
            var mechanoidFaction = Find.FactionManager.AllFactions.FirstOrDefault(x => x.def.defName == "Mechanoid");
            if (mechanoidFaction == null) return;
            DLog.Message($"attempt to fix mechanoid relations");
            SetStandingForPossibleFactions(mechanoidFaction, FactionRelationKind.Hostile, -100, true);
        }

        public static void RestartStory()
        {
            if (Find.World == null)
            {
                Messages.Message($"Load up a savegame to reset the storyline", MessageTypeDefOf.NegativeEvent);
                return;
            }
            if (CompCache.StoryWC.questCont.Village.StoryContact != null) CompCache.StoryWC.questCont.Village.StoryContact.Destroy();
            CompCache.StoryWC.questCont.Village.StoryContact = null;
            CompCache.StoryWC.questCont.FriendlyCaravan.storyContactBondedPawn = null;
            CompCache.StoryWC.ResetStoryVars();
            CompCache.BountyWC.ResetBountyFeature();

            StoryUtility.RemoveAllWhisperDialogs();
            StoryUtility.RemoveExistingQuestFriendlyVillages();
            Quests.QuestUtility.DeleteQuest(StoryQuestDefOf.CA_TradeCaravan);
            Quests.QuestUtility.DeleteQuest(StoryQuestDefOf.CA_StoryVillage_Arrival);
            Quests.QuestUtility.DeleteQuest(StoryQuestDefOf.CA_TheTree);
            StoryUtility.RemoveMapParentsOfDef(CaravanStorySiteDefOf.CAAncientMasterShrineMP);
            StoryUtility.RemoveMapParentsOfDef(CaravanStorySiteDefOf.CAAncientMasterShrineWO);
            StoryUtility.RemoveMapParentsOfDef(CaravanStorySiteDefOf.CALastJudgmentMP);
            Quests.QuestUtility.DeleteQuest(StoryQuestDefOf.CA_FindAncientShrine);
            if (CompCache.StoryWC.questCont.LastJudgment.Apocalypse != null) CompCache.StoryWC.questCont.LastJudgment.EndApocalypse();

            var gifted = CompCache.StoryWC.questCont.StoryStart.Gifted;
            if (gifted != null && !gifted.Dead)
            {
                gifted.health.hediffSet.hediffs.RemoveAll(x => x.def.defName.StartsWith("CAAncient"));
                foreach (var ability in gifted.abilities.abilities.Where(x => x.def.defName.StartsWith("CAAncient")).Reverse()) gifted.abilities.RemoveAbility(ability.def);
            }
            CompCache.StoryWC.questCont.StoryStart.Gifted = null;

            Messages.Message($"Story reset complete", MessageTypeDefOf.PositiveEvent, false);
        }

        private static void RemoveAllWhisperDialogs()
        {
            if (Find.Maps == null || !Find.Maps.Any()) return;
            foreach (var map in Find.Maps.Where(x => x?.ParentFaction == Faction.OfPlayer))
            {
                var tree = map.listerThings.AllThings.FirstOrDefault(x => x.def == ThingDefOf.Plant_TreeAnima) as ThingWithComps;
                if (tree == null || !tree.Spawned || tree.Destroyed) continue;
                RemoveDialog(tree, StoryStart.TreeDialogId);
            }
        }

        public static void CompleteStory()
        {
            RestartStory();
            CompCache.StoryWC.SetStoryCompleteVars();
        }

        public static void ResetLastShrineFlags(bool advanceToLastShrine = false)
        {
            StoryUtility.RemoveMapParentsOfDef(CaravanStorySiteDefOf.CAAncientMasterShrineMP);
            StoryUtility.RemoveMapParentsOfDef(CaravanStorySiteDefOf.CAAncientMasterShrineWO);
            if (advanceToLastShrine)
            {
                for (; ; )
                {
                    if (CompCache.StoryWC.GetCurrentShrineCounter() == CompCache.StoryWC.GetShrineMaxiumum) break;
                    CompCache.StoryWC.IncreaseShrineCompleteCounter();
                }
                foreach (var flag in CompCache.StoryWC.storyFlags) CompCache.StoryWC.storyFlags[flag.Key] = true;
            }
            CompCache.StoryWC.SetSFsStartingWith("Judgment_");
            CompCache.StoryWC.SetSFsStartingWith(CompCache.StoryWC.BuildMaxShrinePrefix());
        }

        public static IEnumerable<ThingDef> ReadBossDefNames()
        {
            foreach (var kind in DefDatabase<PawnKindDef>.AllDefsListForReading)
            {
                if (kind.GetModExtension<MechChipModExt>() != null) yield return kind.race;
            }
        }

        public static void GenerateFriendlyVillage(ref float villageGenerationCounter, bool respawn = false)
        {
            if (!CompCache.StoryWC.storyFlags["TradeCaravan_DialogFinished"] || (CompCache.StoryWC.storyFlags["IntroVillage_Created"] && !respawn)) return;
            if (!StoryUtility.TryGenerateDistantTile(out var tile, 6, 15))
            {
                DLog.Message($"No tile was generated");
                villageGenerationCounter = 120;
                return;
            }
            if (Faction.OfPlayer.HostileTo(StoryUtility.FactionOfSacrilegHunters))
            {
                DLog.Message($"Skipping village generation, Sac hunters are hostile.");
                villageGenerationCounter = 20000;
                return;
            }
            StoryVillageMP settlement = (StoryVillageMP)WorldObjectMaker.MakeWorldObject(CaravanStorySiteDefOf.CAStoryVillageMP);
            settlement.SetFaction(EnsureSacrilegHunters());
            //settlement.AllComps.Add(new CompStoryVillage());
            settlement.Tile = tile;
            settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement, null);
            Find.WorldObjects.Add(settlement);
            CompCache.StoryWC.questCont.Village.Settlement = settlement;

            if (!respawn)
            {
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
                Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_StoryVillage_Arrival, "StoryVillageQuestLetterContent".Translate(storyContactBondedPerson != null ? storyContactBondedPerson.NameShortColored : Faction.OfPlayer.NameColored, CompCache.StoryWC.questCont.Village.Settlement.Label).ToString().HtmlFormatting("add8e6ff", true, 15));
                if (storyContactBondedPerson != null) Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_StoryVillage_Arrival, "StoryVillageQuestLetterContentPS".Translate().ToString().HtmlFormatting("add8e6ff", true, 13));
                Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_StoryVillage_Arrival, "StoryVillageQuestLetterContentEnding".Translate(CompCache.StoryWC.questCont.Village.StoryContact.NameShortColored).ToString().HtmlFormatting("add8e6ff", true, 15));
                Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_StoryVillage_Arrival, "StoryVillageQuestDesc2".Translate(StoryUtility.FactionOfSacrilegHunters.Name.HtmlFormatting("008080"), CompCache.StoryWC.questCont.Village.Settlement.Name.HtmlFormatting("008080"), CompCache.StoryWC.questCont.Village.StoryContact.NameShortColored));
            }
            Quests.QuestUtility.UpdateQuestLocation(StoryQuestDefOf.CA_StoryVillage_Arrival, CompCache.StoryWC.questCont.Village.Settlement, true, respawn);
            CompCache.StoryWC.SetSF("IntroVillage_Created");
        }

        public static void RemoveFaction(string defname = "CAFriendlyMechanoid", string messageFactionName = "CA friendly mech")
        {
            var factionDef = DefDatabase<FactionDef>.GetNamedSilentFail(defname);
            if (factionDef == null) return;

            if (Find.World == null) return;

            var faction = Find.FactionManager.AllFactions.FirstOrDefault(x => x?.def == factionDef);
            if (faction == null)
            {
                // todo remove, log just debug
                DLog.Message($"{messageFactionName} faction is null");
                return;
            }

            Helper.RunSafely(() =>
            {
                foreach (var pawn in PawnsFinder.AllMaps_Spawned.Where(x => x.Faction == faction).Reverse<Pawn>()) pawn.Destroy();
                foreach (var pawn in PawnsFinder.AllCaravansAndTravelingTransportPods_Alive.Where(x => x.Faction == faction).Reverse<Pawn>()) pawn.Destroy();
            });

            faction.temporary = true;

            var settlements = Find.World?.worldObjects?.AllWorldObjects?.Where(x => x?.Faction?.def == factionDef);

            if (settlements != null && settlements.Count() != 0)
            {
                foreach (var settlement in settlements.Reverse<WorldObject>())
                {
                    Helper.RunSafely(() =>
                    {
                        DLog.Message($"Removing no longer needed {messageFactionName} settlement." + (ModsConfig.IdeologyActive ? "The possibly following Notify error can be ignored" : ""));
                        if (settlement is MapParent parent && parent?.HasMap == true) Current.Game.DeinitAndRemoveMap(parent.Map, false);
                        settlement.Destroy();
                    });
                }
            }

            //typeof(FactionManager).GetMethod("Remove", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(Find.FactionManager, new object[] { faction });
            DLog.Message($"{messageFactionName} should now be removed");
        }

        public static void RemoveStoryComponentsNoRoyalty()
        {
            RemoveStoryOrMod();
        }

        public static void FreshenUpPawn(Pawn pawn)
        {
            if (pawn == null)
            {
                Log.Warning($"CA Story pawn was null which shouldn't happen!");
                return;
            }
            HealthUtility.HealNonPermanentInjuriesAndRestoreLegs(pawn);
            if (pawn?.needs?.food != null) pawn.needs.food.CurLevel = pawn.needs.food.MaxLevel;
            if (pawn?.needs?.joy != null) pawn.needs.joy.CurLevel = pawn.needs.joy.MaxLevel;
            if (pawn?.needs?.rest != null) pawn.needs.rest.CurLevel = pawn.needs.rest.MaxLevel;
            if (pawn?.needs?.comfort != null) pawn.needs.comfort.CurLevel = pawn.needs.comfort.MaxLevel;

            Helper.RunSafely(() => { if (CheckIfPawnIsNaked(pawn)) GiveClothes(pawn); });
        }

        public static IntVec3 GetCenterOfSettlementBase(Map map, Faction faction, bool useMapCenterAsFallback = false)
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
            coords.ForEach(coord => DLog.Message($"Coord: {coord.x} / {coord.z}"));

            if (coords.Count == 0)
            {
                Log.Error($"Calculating GetCenterOfSettlementBase failed, returning default location");
                if (!useMapCenterAsFallback) return default;
                var mapSize = Find.World.info.initialMapSize;

                var tempLoc = CellFinder.RandomSpawnCellForPawnNear(new IntVec3(mapSize.x / 2, 1, mapSize.z / 2), map, 20);
                DLog.Message($"Default location using map center?: {useMapCenterAsFallback} : {tempLoc.x} / {tempLoc.z}");
                return tempLoc;
            }

            var centerPoint = new IntVec3(Convert.ToInt32(coords.Select(coord => coord.x).Average()), 0, Convert.ToInt32(coords.Select(coord => coord.z).Average()));
            DLog.Message($"Center: {centerPoint.x} / {centerPoint.z}");

            return centerPoint;
        }

        public static void RemoveStoryOrMod(bool removeFullMod = false)
        {
            if (Find.World == null)
            {
                if (removeFullMod) Messages.Message($"Load up a savegame to remove the mod's components", MessageTypeDefOf.NegativeEvent);
                else Messages.Message($"Load up a savegame to remove the storyline", MessageTypeDefOf.NegativeEvent);
                return;
            }

            Helper.RunSafely(RestartStory);
            Helper.RunSafely(() =>
            {
                Helper.RunSafely(() => RemoveFaction("CASacrilegHunters", "Sacrileg Hunters"));
                //foreach (var settlement in Find.World.worldObjects.Settlements.Reverse<Settlement>())
                //{
                //    if (settlement?.Faction?.def != StoryDefOf.CASacrilegHunters) continue;
                //    if (settlement.HasMap) Current.Game.DeinitAndRemoveMap(settlement.Map);
                //    settlement.Destroy();

                //    // todo remove rest of the faction!!!
                //}

                if (removeFullMod)
                {
                    ModSettings.bountyEnabled = false;
                    ModSettings.caravanCampEnabled = false;
                    ModSettings.caravanFormingFilterSelectionEnabled = false;
                    ModSettings.caravanIncidentsEnabled = false;
                    ModSettings.caravanTravelCompanionsEnabled = false;
                    ModSettings.storyEnabled = false;

                    ModSettings.autoRemoveAbandondSettlementRuins = false;
                    ModSettings.buffSettlementFoodAndSilverAvailability = false;
                    ModSettings.buffShrineRewards = false;
                    ModSettings.spDecayLevelIncrease = false;
                    ModSettings.showLetterRemoval = false;

                    var travelComp = Find.World.GetComponent<CaravanImmersion.TravelCompanionWC>();
                    if (travelComp != null) travelComp.RemoveRelations();
                }

                if (removeFullMod) Messages.Message("Mod component removal complete (experimental)", MessageTypeDefOf.NeutralEvent);
                else Messages.Message("Story removal complete (experimental)", MessageTypeDefOf.NeutralEvent);
            });
        }

        public static void FindUnfoggedMechsAndWakeUp(Map map)
        {
            map.mapPawns.SpawnedPawnsInFaction(Helper.ExpRMNewFaction)
                .Where(mech => !mech.Awake() && !mech.GetRoom().Fogged)
                .ToList()
                .ForEach(mech =>
                {
                    var comp = mech.TryGetComp<CompCanBeDormant>();
                    if (comp != null) comp.WakeUp();
                    else
                    {
                        var lord = mech.GetLord();
                        if (lord != null) lord.Notify_DormancyWakeup();
                    }
                });
        }

        [Obsolete]
        public static Faction CreateOrGetFriendlyMechFaction()
        {
            var relations = new List<FactionRelation>();
            foreach (var curFaction in Find.FactionManager.AllFactionsListForReading)
            {
                if (curFaction.def.permanentEnemy) continue;
                relations.Add(new FactionRelation
                {
                    other = curFaction,
                    baseGoodwill = curFaction == Faction.OfPlayer ? 100 : Faction.OfPlayer.GoodwillWith(curFaction),
                    kind = curFaction == Faction.OfPlayer ? FactionRelationKind.Ally : Faction.OfPlayer.RelationKindWith(curFaction)
                });
            }

            var faction = Find.FactionManager.AllFactionsListForReading.FirstOrDefault(x => x.def.defName == "CAFriendlyMechanoid");
            if (faction == null)
            {
                faction = FactionGenerator.NewGeneratedFactionWithRelations(FactionDef.Named("CAFriendlyMechanoid"), relations);
                faction.hidden = new bool?(true);
                faction.temporary = true;
                faction.factionHostileOnHarmByPlayer = false;
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
                DLog.Message($"Caravan count: {Find.WorldObjects.Caravans.Count}");
                caravan = Find.WorldObjects.Caravans.Where(x => x.Faction == Faction.OfPlayer).ToList().OrderByDescending(x => x.PawnsListForReading.Count).FirstOrDefault();
                if (caravan != null) startTile = caravan.Tile;
                else DLog.Message($"caraavn is null");
            }
            return TileFinder.TryFindNewSiteTile(out newTile, minDist, maxDist, false, TileFinderMode.Near, startTile, false);
        }

        public static void GenerateStoryContact()
        {
            if (CompCache.StoryWC.questCont.Village.StoryContact != null && !CompCache.StoryWC.questCont.Village.StoryContact.Dead) return;

            // todo rm get def of story pawn
            var girl = PawnGenerator.GeneratePawn(new PawnGenerationRequest(StoryDefOf.CASacrilegHunters_ExperiencedHunterVillage)
            {
                Context = PawnGenerationContext.NonPlayer,
                FixedBiologicalAge = 19,
                FixedChronologicalAge = 3022,
                FixedGender = Gender.Female,
                AllowAddictions = false,
                AllowGay = false,
                AllowDead = false,
                Faction = StoryUtility.EnsureSacrilegHunters(),
                //KindDef = StoryDefOf.CASacrilegHunters_ExperiencedHunterVillage,
                ProhibitedTraits = new List<TraitDef> { TraitDef.Named("Wimp") },
                MustBeCapableOfViolence = true,
            });

            // todo looks?
            //girl.story.hairDef = 
            DLog.Message("Generated main quest pawn");

            girl.story.traits.allTraits.RemoveAll(x => x.def == DefDatabase<TraitDef>.GetNamedSilentFail("Beauty"));
            girl.story.traits.GainTrait(new Trait(DefDatabase<TraitDef>.GetNamedSilentFail("Beauty"), 2));

            if (!Find.WorldPawns.Contains(girl)) Find.WorldPawns.PassToWorld(girl, PawnDiscardDecideMode.KeepForever);
            CompCache.StoryWC.questCont.Village.StoryContact = girl;

            Helper.RunSafely(() => { if (CheckIfPawnIsNaked(girl)) GiveClothes(girl); });
        }

        public static void GiveClothes(Pawn pawn)
        {
            pawn.apparel.HasBasicApparel(out var hasPants, out var hasShirt);
            var stuff = DefDatabase<ThingDef>.GetNamedSilentFail("Hyperweave") ?? ThingDefOf.Cloth;
            if (!hasPants)
            {
                var pants = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamedSilentFail("Apparel_Pants"), stuff) as Apparel;
                if (pants != null) pawn.apparel.Wear(pants);
            }
            if (hasShirt) return;
            var shirt = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamedSilentFail("Apparel_CollarShirt"), stuff) as Apparel;
            if (shirt != null) pawn.apparel.Wear(shirt);
        }

        public static bool CheckIfPawnIsNaked(Pawn pawn)
        {
            if (pawn == null) return false;
            if (pawn?.apparel?.AnyApparel == true) return false;
            return true;
        }

        public static Pawn GetGiftedPawn() => CompCache.StoryWC.questCont.StoryStart?.Gifted; // ?? PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction?.FirstOrDefault(x => (x?.RaceProps?.Humanlike ?? false) && x.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("CAAncientGift")) != null);

        public static bool IsAuraProtected(this Pawn pawn) =>
            pawn?.health?.hediffSet?.hediffs?.Any(
                x => x != null
                && (x.def == CaravanAbilities.AbilityDefOf.CAAncientProtectiveAura
                    || x.def == CaravanAbilities.AbilityDefOf.CAAncientProtectiveAuraLinked)) ?? false;

        public static bool IsAuraProtectedAndTakesShipHeat(this Pawn pawn)
        {
            var hediff = pawn?.health?.hediffSet?.hediffs?.FirstOrDefault(
                x => x != null
                && (x.def == CaravanAbilities.AbilityDefOf.CAAncientProtectiveAura
                    || x.def == CaravanAbilities.AbilityDefOf.CAAncientProtectiveAuraLinked));

            if (hediff == null) return false;
            var comp = hediff.TryGetComp<CaravanAbilities.HediffComp_AncientProtectiveAura>();
            if (comp == null) return false;
            return comp.ProtectsTheShip;
        }

        public static bool HasAuraPawn(this Map map) => map.mapPawns.AllPawnsSpawned.Any(x => IsAuraProtected(x));

        public static Pawn GetFirstPawnWith(this Map map, Func<Pawn, bool> pred) => map.mapPawns.AllPawnsSpawned.FirstOrDefault(x => pred(x));

        public static Faction EnsureSacrilegHunters(FactionRelationKind? relationKind = null, bool ignoreBetrayal = false, bool skipLeaderGeneration = false)
        {
            var sacrilegHunters = Find.FactionManager.AllFactions.FirstOrDefault(x => x.def?.defName == "CASacrilegHunters");
            if (sacrilegHunters == null)
            {
                if (DefDatabase<FactionDef>.GetNamedSilentFail("CASacrilegHunters") == null) return null;
                sacrilegHunters = Helper.RunSafely(() => FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(DefDatabase<FactionDef>.GetNamedSilentFail("CASacrilegHunters"), default, false)));

                if (sacrilegHunters == null)
                {
                    Log.Error($"Creating the sacrileg hunter faction failed due to some incompatibility. Error above.");
                    return null;
                }
                Find.FactionManager.Add(sacrilegHunters);
                var empireDef = FactionDefOf.Empire;
                if (!empireDef.permanentEnemyToEveryoneExcept.Contains(sacrilegHunters.def)) empireDef.permanentEnemyToEveryoneExcept.Add(sacrilegHunters.def);
            }
            CompCache.BountyWC.BountyFaction = sacrilegHunters;
            SetSacNeutralToPossibleFactions(sacrilegHunters);
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
                                sacrilegHunters.SetRelation(new FactionRelation() { kind = FactionRelationKind.Ally, baseGoodwill = 100, other = Faction.OfPlayer });
                                sacrilegHunters.factionHostileOnHarmByPlayer = false;
                                break;
                            case FactionRelationKind.Neutral:
                                sacrilegHunters.SetRelation(new FactionRelation() { kind = FactionRelationKind.Neutral, baseGoodwill = 0, other = Faction.OfPlayer });
                                sacrilegHunters.factionHostileOnHarmByPlayer = false;
                                break;
                            case FactionRelationKind.Hostile:
                                sacrilegHunters.SetRelation(new FactionRelation() { kind = FactionRelationKind.Hostile, baseGoodwill = -100, other = Faction.OfPlayer });
                                break;
                        }
                    }
                }
                else if (sacrilegHunters.RelationKindWith(Faction.OfPlayerSilentFail) != FactionRelationKind.Hostile) sacrilegHunters.SetRelation(new FactionRelation() { kind = FactionRelationKind.Hostile, baseGoodwill = -100, other = Faction.OfPlayer });
            }
            return sacrilegHunters;
        }

        public static Faction EnsureEvilHostileFactionForExpansion(bool skipLeaderGeneration = false)
        {
            DLog.Message($"Ensuring evil faction for expansionmod");
            var sacrilegHunters = Find.FactionManager.AllFactions.FirstOrDefault(x => x?.def?.defName != null && x.def?.defName == Helper.ExpRMNewFaction?.def?.defName);
            if (sacrilegHunters == null)
            {
                DLog.Message($"Creating evil faction for expansionmod");
                if (DefDatabase<FactionDef>.GetNamedSilentFail(Helper.ExpSettings?.primaryEnemyFactionDef?.defName) == null) return null;
                sacrilegHunters = Helper.RunSafely(() => FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(DefDatabase<FactionDef>.GetNamedSilentFail(Helper.ExpSettings?.primaryEnemyFactionDef?.defName), default, false)));

                if (sacrilegHunters == null)
                {
                    Log.Error($"Creating the evil RM faction failed due to some incompatibility. Error above.");
                    return null;
                }
                Find.FactionManager.Add(sacrilegHunters);
                var empireDef = FactionDefOf.Empire;
                if (!empireDef.permanentEnemyToEveryoneExcept.Contains(sacrilegHunters.def)) empireDef.permanentEnemyToEveryoneExcept.Add(sacrilegHunters.def);
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
            /*
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
                                sacrilegHunters.SetRelation(new FactionRelation() { kind = FactionRelationKind.Ally, baseGoodwill = 100, other = Faction.OfPlayer });
                                sacrilegHunters.factionHostileOnHarmByPlayer = false;
                                break;
                            case FactionRelationKind.Neutral:
                                sacrilegHunters.SetRelation(new FactionRelation() { kind = FactionRelationKind.Neutral, baseGoodwill = 0, other = Faction.OfPlayer });
                                sacrilegHunters.factionHostileOnHarmByPlayer = false;
                                break;
                            case FactionRelationKind.Hostile:
                                sacrilegHunters.SetRelation(new FactionRelation() { kind = FactionRelationKind.Hostile, baseGoodwill = -100, other = Faction.OfPlayer });
                                break;
                        }
                    }
                }
                else if (sacrilegHunters.RelationKindWith(Faction.OfPlayerSilentFail) != FactionRelationKind.Hostile) sacrilegHunters.SetRelation(new FactionRelation() { kind = FactionRelationKind.Hostile, baseGoodwill = -100, other = Faction.OfPlayer });
            }
            */
            return sacrilegHunters;
        }

        private static void SetSacNeutralToPossibleFactions(Faction sacrilegHunters)
        {
            foreach (var faction in Find.FactionManager.AllFactionsListForReading)
            {
                if (faction == Faction.OfPlayer || faction.def.permanentEnemy || faction == sacrilegHunters) continue;
                if (ModSettings.sacHuntersHostileTowardsEmpire && faction == Faction.OfEmpire
                    || ModSettings.sacHuntersHostileTowardsNaturalEnemies && faction.def.naturalEnemy
                    || ModsConfig.AnomalyActive && faction == Faction.OfEntities)
                {
                    faction.SetRelation(new FactionRelation() { baseGoodwill = -100, kind = FactionRelationKind.Hostile, other = sacrilegHunters });
                    continue;
                }
                faction.SetRelation(new FactionRelation() { baseGoodwill = 0, kind = FactionRelationKind.Neutral, other = sacrilegHunters });
            }
        }

        private static void SetStandingForPossibleFactions(Faction factionToAdjust, FactionRelationKind kind, int goodwill, bool skipPermanentEnemyCheck = false)
        {
            foreach (var faction in Find.FactionManager.AllFactionsListForReading)
            {
                if (faction == Faction.OfPlayer || (!skipPermanentEnemyCheck && faction.def.permanentEnemy) || faction == factionToAdjust) continue;
                Helper.RunSafely(() => faction.SetRelation(new FactionRelation() { baseGoodwill = goodwill, kind = kind, other = factionToAdjust }));
            }
        }

        public static Faction EnsureEvilMechanoidFaction(FactionRelationKind? relationKind = null, bool ignoreBetrayal = false, bool skipLeaderGeneration = false)
        {
            var mechanoidFaction = Find.FactionManager.AllFactions.FirstOrDefault(x => x.def.defName == "Mechanoid");
            if (mechanoidFaction == null)
            {
                mechanoidFaction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(DefDatabase<FactionDef>.GetNamedSilentFail("Mechanoid"), default, true));

                Find.FactionManager.Add(mechanoidFaction);
                var empireDef = FactionDefOf.Empire;
                if (!empireDef.permanentEnemyToEveryoneExcept.Contains(mechanoidFaction.def)) empireDef.permanentEnemyToEveryoneExcept.Add(mechanoidFaction.def);
            }
            SetStandingForPossibleFactions(mechanoidFaction, FactionRelationKind.Hostile, -100);
            if ((mechanoidFaction.leader == null || mechanoidFaction.leader.Dead || mechanoidFaction.leader.Destroyed) && !skipLeaderGeneration)
            {
                try
                {
                    mechanoidFaction.TryGenerateNewLeader();
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }
            //if (Faction.OfPlayerSilentFail != null && mechanoidFaction.RelationWith(Faction.OfPlayer, true) == null) mechanoidFaction.TryMakeInitialRelationsWith(Faction.OfPlayer);
            //if (mechanoidFaction != null && Faction.OfPlayerSilentFail != null)
            //{
            //if ((!CompCache.StoryWC.storyFlags["SacrilegHuntersBetrayal"] || ignoreBetrayal))
            //{
            //    if (relationKind != null && mechanoidFaction.RelationKindWith(Faction.OfPlayerSilentFail) != relationKind)
            //    {
            //        switch (relationKind)
            //        {
            //            case FactionRelationKind.Ally:
            //                mechanoidFaction.SetRelation(new FactionRelation() { kind = FactionRelationKind.Ally, baseGoodwill = 100, other = Faction.OfPlayer });
            //                mechanoidFaction.factionHostileOnHarmByPlayer = false;
            //                break;
            //            case FactionRelationKind.Neutral:
            //                mechanoidFaction.SetRelation(new FactionRelation() { kind = FactionRelationKind.Neutral, baseGoodwill = 0, other = Faction.OfPlayer });
            //                mechanoidFaction.factionHostileOnHarmByPlayer = false;
            //                break;
            //            case FactionRelationKind.Hostile:
            //                mechanoidFaction.SetRelation(new FactionRelation() { kind = FactionRelationKind.Hostile, baseGoodwill = -100, other = Faction.OfPlayer });
            //                break;
            //        }
            //    }
            //}
            //else if (mechanoidFaction.RelationKindWith(Faction.OfPlayerSilentFail) != FactionRelationKind.Hostile) 
            mechanoidFaction.SetRelation(new FactionRelation() { kind = FactionRelationKind.Hostile, baseGoodwill = -100, other = Faction.OfPlayer });
            //}
            return mechanoidFaction;
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
                DLog.Message($"Trying to destroy settlement {settlement.Name}");
                DLog.Message($"settlement mp: {settlement.def.defName}");
                if (settlement.def.defName != "StoryVillageMP") return;
                if (settlement.HasMap) Current.Game.DeinitAndRemoveMap(settlement.Map, false);
                settlement.Destroy();
            }
            var destroyedSettlement = CompCache.StoryWC.questCont.Village.DestroyedSettlement;
            if (destroyedSettlement != null)
            {
                DLog.Message($"Trying to destroy destroyed settlement");
                DLog.Message($"destroyed settlement mp: {destroyedSettlement.def.defName}");
                if (destroyedSettlement.def != WorldObjectDefOf.DestroyedSettlement) return;
                if (destroyedSettlement.HasMap) Current.Game.DeinitAndRemoveMap(destroyedSettlement.Map, false);
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
                DLog.Message($"Destroying site {site.def.label}");
                var mapParent = site as MapParent;
                if (mapParent != null)
                {
                    if (mapParent.HasMap) Current.Game.DeinitAndRemoveMap(mapParent.Map, false);
                    else DLog.Warning($"Didn't have a map to remove: {site.Label} {site.def.defName}");
                }
                else DLog.Warning($"Couldn't convert site to MapParent and check for a map to remove: {site.Label} {site.def.defName}");
                site.Destroy();
            }
        }

        public static void CallBombardment(IntVec3 position, Map map, Pawn instigator, float impactRadius = 7.9f, float explosionRangeMin = 5.9f, float explosionRangeMax = 5.9f, int explosionCount = 6, int intervalAndWarmupDuration = 60)
        {
            var bombardment = (Bombardment)GenSpawn.Spawn(ThingDefOf.Bombardment, position, map, WipeMode.Vanish);
            bombardment.impactAreaRadius = impactRadius;
            bombardment.explosionRadiusRange = new FloatRange(explosionRangeMin, explosionRangeMax);
            bombardment.bombIntervalTicks = intervalAndWarmupDuration; // 60
            bombardment.randomFireRadius = 1;
            bombardment.explosionCount = explosionCount;
            bombardment.warmupTicks = intervalAndWarmupDuration; // 60
            bombardment.instigator = instigator;
            SoundDefOf.OrbitalStrike_Ordered.PlayOneShotOnCamera(null);
        }

        public static Pawn GetFittingMechBoss(bool endboss = false)
        {
            // we skip the devourer as the first boss as it can be rather... tricky to fight.
            var possibleBosses = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x =>
            (Helper.ExpRM || x.RaceProps.IsMechanoid)
            && x.defName.ToLower().StartsWith(Helper.ExpRM ? Helper.ExpSettings?.bossDefPrefix?.ToLower() ?? "cabossmech" : "cabossmech")
            && (Helper.ExpRM || (CompCache.StoryWC.mechBossKillCounters.Count != 0 || x.defName != "CABossMechDevourer")));

            var selected = (endboss
                ? (Helper.ExpRM
                    ? Helper.ExpSettings?.endBossPawnKindDef ?? StoryDefOf.CAEndBossMech
                    : StoryDefOf.CAEndBossMech)
                : possibleBosses.Where(boss => !CompCache.StoryWC.mechBossKillCounters?.Keys?.Contains(boss) ?? false)?.RandomElement()) ?? possibleBosses.RandomElement();
            var bossPawn = PawnGenerator.GeneratePawn(selected, Helper.ExpRMNewFaction);
            if (bossPawn != null)
            {
                var modext = bossPawn.kindDef.GetModExtension<MechChipModExt>();
                if (modext?.mechChipDefs?.Count != null)
                {
                    foreach (var chipDef in modext.mechChipDefs) bossPawn.health.AddHediff(chipDef);
                }
            }
            return bossPawn;
        }

        public static Faction FactionOfSacrilegHunters { get => Find.FactionManager.FirstFactionOfDef(StoryDefOf.CASacrilegHunters); private set => FactionOfSacrilegHunters = value; }

        public static bool FloodUnfogAdjacent(FogGrid fogGrid, Map map, IntVec3 c, bool showMessages = true)
        {
            fogGrid.Unfog(c);
            bool flag = false;
            FloodUnfogResult floodUnfogResult = default(FloodUnfogResult);
            for (int i = 0; i < 4; i++)
            {
                IntVec3 intVec = c + GenAdj.CardinalDirections[i];
                if (intVec.InBounds(map) && intVec.Fogged(map))
                {
                    Building edifice = intVec.GetEdifice(map);
                    if (edifice == null || !edifice.def.MakeFog)
                    {
                        flag = true;
                        floodUnfogResult = FloodFillerFog.FloodUnfog(intVec, map);
                    }
                    else
                    {
                        fogGrid.Unfog(intVec);
                    }
                }
            }
            for (int j = 0; j < 8; j++)
            {
                IntVec3 c2 = c + GenAdj.AdjacentCells[j];
                if (c2.InBounds(map))
                {
                    Building edifice2 = c2.GetEdifice(map);
                    if (edifice2 != null && edifice2.def.MakeFog)
                    {
                        fogGrid.Unfog(c2);
                    }
                }
            }
            if (flag && showMessages)
            {
                if (floodUnfogResult.mechanoidFound)
                {
                    Find.LetterStack.ReceiveLetter("LetterLabelAreaRevealed".Translate(), "AreaRevealedWithMechanoids".Translate(), LetterDefOf.ThreatBig, new TargetInfo(c, map, false), null, null, null, null);
                    return true;
                }
                if (!floodUnfogResult.allOnScreen || floodUnfogResult.cellsUnfogged >= 600)
                {
                    Find.LetterStack.ReceiveLetter("LetterLabelAreaRevealed".Translate(), "AreaRevealed".Translate(), LetterDefOf.NeutralEvent, new TargetInfo(c, map, false), null, null, null, null);
                }
                return true;
            }
            return false;
        }

        internal static float GetIncPoints(float normal, float? performance = null, int? custDevider = null)
        {
            if (ModSettings.storyMode == StoryMode.Performance)
            {
                if (performance != null) return (float)performance;
                else return normal / (custDevider ?? 6);
            }
            return normal;
        }

        internal static void StripGiftFromPawn(Pawn gifted)
        {
            gifted.health.hediffSet.hediffs.Remove(gifted.health.hediffSet.hediffs.FirstOrDefault(x => x.def.defName == "CAAncientGift"));
            foreach (var ability in gifted.abilities.abilities.Where(x => x.def.defName.StartsWith("CAAncient")).Reverse())
            {
                gifted.abilities.RemoveAbility(ability.def);
            }
        }
    }
}
