using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace CaravanAdventures.CaravanStory
{
    public class AncientMasterShrineMP : MapParent
    {
        public bool WonBattle => this.WonBattle;
        private bool wonBattle;

        public List<Pawn> generatedSoldiers = new List<Pawn>();
        public List<Pawn> generatedMechs = new List<Pawn>();
        public List<Pawn> generatedBandits = new List<Pawn>();
        public Pawn boss = null;
        private bool bossDefeatedAndRewardsGiven;
        private int constTicks = -1;
        private int checkDormantTicks = 0;

        private LastJudgmentMP lastJudgmentMP;
        public Thing lastJudgmentEntrance;
        private int checkRangeForJudgmentTicks = 0;
        public IntVec3 portalSpawnPosition = new IntVec3(25, 0, 3);
        private bool bossWasSpawned;
        private bool lastJudgementEntraceWasSpawned;
        private bool abandonShrine = false;

        public override MapGeneratorDef MapGeneratorDef => CaravanStorySiteDefOf.CAAncientMasterShrineMG;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.wonBattle, "wonBattle", false, false);
            Scribe_References.Look(ref boss, "boss");
            Scribe_Values.Look(ref constTicks, "constTicks", -1);
            Scribe_Values.Look(ref bossDefeatedAndRewardsGiven, "bossDefeatedAndRewardsGiven", false);
            Scribe_Values.Look(ref checkDormantTicks, "checkDormantTicks", 0);
            Scribe_Values.Look(ref checkRangeForJudgmentTicks, "checkRangeForJudgmentTicks", 0);
            Scribe_Values.Look(ref bossWasSpawned, "bossWasSpawned", false);
            Scribe_Values.Look(ref lastJudgementEntraceWasSpawned, "lastJudgementEntraceWasSpawned", false);
            // todo are mechs enough? Need them for comparison later - dont think so, i should drop them
            Scribe_Collections.Look(ref generatedMechs, "generatedMechs", LookMode.Reference);
            Scribe_Values.Look(ref abandonShrine, "abandonShrine", false);

            Scribe_References.Look(ref lastJudgmentEntrance, "lastJudgmentEntrance");
            Scribe_References.Look(ref lastJudgmentMP, "lastJudgmentMap");
        }

        public void Init()
        {
            // debug
            //Log.Message($"compare mechs. mapPawns: {Map.mapPawns.AllPawns.Where(x => x.RaceProps.IsMechanoid).Count()} ourlist: {generatedMechs.Count}");
            //foreach (var pawn in Map.mapPawns.AllPawns.Where(x => x.RaceProps.IsMechanoid))
            //         {
            //	Log.Message($"defname: {pawn.def.defName} kind: {pawn.kindDef.defName} other: {pawn.def.label} kindlabel: {pawn.kindDef.label}");
            //         }
            //var notMatching = generatedMechs.Where(x => !Map.mapPawns.AllPawns.Where(y => y.RaceProps.IsMechanoid).Any(z => z.ThingID == x.ThingID));
            //Log.Message($"Not matching count: {notMatching.Count()}");
            //Log.Message($"Map has boss: {boss != null}");

            if (boss != null || lastJudgmentEntrance != null)
            {
                wonBattle = true;

                GetComponent<TimedDetectionPatrols>().Init();
                // todo balance strenth! maybe include shrine counter
                GetComponent<TimedDetectionPatrols>().StartDetectionCountdown(60000, -1, (int)(8000 * (1 + CompCache.StoryWC.GetCurrentShrineCounter() / 10)));

                if (boss != null) bossWasSpawned = true;
                if (lastJudgmentEntrance != null) lastJudgementEntraceWasSpawned = true;
            }
            else
            {
                // todo do we want a raid here? 
                GetComponent<TimedDetectionPatrols>().Init();
                GetComponent<TimedDetectionPatrols>().StartDetectionCountdown(180000, -1, (int)(8000 * (1 + CompCache.StoryWC.GetCurrentShrineCounter() / 10)));
                GetComponent<TimedDetectionPatrols>().ToggleIncreaseStrenthByCounter = true;
            }
            CompCache.StoryWC.SetShrineSF("Created");
            Quests.QuestUtility.UpdateQuestLocation(Quests.StoryQuestDefOf.CA_FindAncientShrine, this);
        }

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            // todo add scenatio of keeping map until last judgement is completed and left, 
            if (!base.Map.mapPawns.AnyPawnBlockingMapRemoval && boss == null && (CompCache.StoryWC.GetCurrentShrineCounter() - 1 != CompCache.StoryWC.GetShrineMaxiumum || CompCache.StoryWC.storyFlags["Judgment_Completed"])
                || !base.Map.mapPawns.AnyPawnBlockingMapRemoval && bossDefeatedAndRewardsGiven && (CompCache.StoryWC.GetCurrentShrineCounter() - 1 != CompCache.StoryWC.GetShrineMaxiumum || CompCache.StoryWC.storyFlags["Judgment_Completed"]))
            {
                // resetting flags here due to shrine map being a bandit map without boss!!
                if (!bossWasSpawned && !lastJudgementEntraceWasSpawned)
                {
                    DLog.Message($"Resetting shrine flags for current shrine: {CompCache.StoryWC.GetCurrentShrineCounter()}");
                    CompCache.StoryWC.ResetCurrentShrineFlags();
                }

                alsoRemoveWorldObject = true;
                return true;
            }
            alsoRemoveWorldObject = false;
            return false;
        }

        public override void PostRemove()
        {
            base.PostRemove();
            Log.Message($"post remove happening?");
            if (abandonShrine && !bossDefeatedAndRewardsGiven) return;
            if (bossWasSpawned)
            {
                if (CompCache.StoryWC.GetCurrentShrineCounter() == 2) Quests.QuestUtility.AppendQuestDescription(Quests.StoryQuestDefOf.CA_FindAncientShrine, Helper.HtmlFormatting("Story_Shrine1_QuestUpdate_1".Translate(), "f59b42"), false, true);
                LeavingShrineDialog();
            }
        }

        private void LeavingShrineDialog()
        {
            var storyChar = CompCache.StoryWC.questCont.Village.StoryContact;
            switch (CompCache.StoryWC.GetCurrentShrineCounter())
            {
                case 2:
                    var diaNode3 = new DiaNode("Story_Shrine1_Apocalypse_Dia1_3".Translate(storyChar.NameShortColored));
                    diaNode3.options.Add(new DiaOption("Story_Shrine1_Apocalypse_Dia1_3_Option1".Translate()) { resolveTree = true });

                    var diaNode2 = new DiaNode("Story_Shrine1_Apocalypse_Dia1_2".Translate(storyChar.NameShortColored));
                    diaNode2.options.Add(new DiaOption("Story_Shrine1_Apocalypse_Dia1_2_Option1".Translate()) { link = diaNode3 });

                    var diaNode = new DiaNode("Story_Shrine1_Apocalypse_Dia1_1".Translate(storyChar.NameShortColored));
                    diaNode.options.Add(new DiaOption("Story_Shrine1_Apocalypse_Dia1_1_Option1".Translate()) { link = diaNode2 });

                    TaggedString taggedString = "Story_Shrine1_Apocalypse_Dia1Title".Translate(storyChar.NameShortColored);
                    Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
                    Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));
                    break;
                case 3:
                    diaNode3 = new DiaNode("Story_Shrine2_Apocalypse_Dia1_3".Translate(storyChar.NameShortColored));
                    diaNode3.options.Add(new DiaOption("Story_Shrine2_Apocalypse_Dia1_3_Option1".Translate()) { resolveTree = true });

                    diaNode2 = new DiaNode("Story_Shrine2_Apocalypse_Dia1_2".Translate(storyChar.NameShortColored));
                    diaNode2.options.Add(new DiaOption("Story_Shrine2_Apocalypse_Dia1_2_Option1".Translate()) { link = diaNode3 });

                    diaNode = new DiaNode("Story_Shrine2_Apocalypse_Dia1_1".Translate(storyChar.NameShortColored));
                    diaNode.options.Add(new DiaOption("Story_Shrine2_Apocalypse_Dia1_1_Option1".Translate()) { link = diaNode2 });

                    taggedString = "Story_Shrine1_Apocalypse_Dia1Title".Translate(storyChar.NameShortColored);
                    Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
                    Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));
                    break;
            }

        }

        public override void Tick()
        {
            base.Tick();
            if (base.HasMap)
            {
                CheckBossDefeated();
                CheckWonBattle();

                if (!CompCache.StoryWC.storyFlags["Judgment_Completed"] && lastJudgmentEntrance != null && checkRangeForJudgmentTicks >= 60)
                {
                    CheckGenerateAndEnterLastJudgment();
                    checkRangeForJudgmentTicks = 0;
                }

                if (checkDormantTicks == 240)
                {
                    StoryUtility.FindUnfoggedMechsAndWakeUp(Map);
                    checkDormantTicks = 0;
                }

                if (constTicks == 2400)
                {
                    if (boss != null || lastJudgmentEntrance != null)
                    {
                        StoryUtility.EnsureSacrilegHunters();

                        // todo different dialogs for other shrines, maybe they betray the player the 3rd or 4th shrine.
                        HunterAssistanceDialog();
                    }
                }

                constTicks++;
                checkRangeForJudgmentTicks++;
                checkDormantTicks++;
            }
        }

        private void CheckGenerateAndEnterLastJudgment()
        {

            var triggerCells = new IntVec3[] { lastJudgmentEntrance.Position }; // GenRadial.RadialCellsAround(lastJudgmentEntrance.Position, 1, true);
            if (!triggerCells.Any(cell => cell.GetFirstPawn(Map) == CompCache.StoryWC.questCont.StoryStart.Gifted)) return;

            if (lastJudgmentMP == null)
            {
                LongEventHandler.QueueLongEvent(delegate ()
                {
                    CompCache.StoryWC.questCont.LastJudgment.CreateLastJudgment(ref lastJudgmentMP, Tile);
                }, "GeneratingMapForNewEncounter", false, null, true);
            }
            LongEventHandler.QueueLongEvent(delegate ()
            {
                var gifted = CompCache.StoryWC.questCont.StoryStart.Gifted;
                if (gifted.Spawned) gifted.DeSpawn();
                GenSpawn.Spawn(gifted, portalSpawnPosition, lastJudgmentMP.Map);
                gifted.drafter.Drafted = true;
                Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                lastJudgmentMP.Init(this);
                CameraJumper.TryJump(gifted);
            }, "GeneratingMapForNewEncounter", false, null, true);
        }

        private void LetterNoMasterShrine() => Find.LetterStack.ReceiveLetter("MasterShrineVictoryLetterLabel".Translate(), "MasterShrineVictoryLetterMessage".Translate(), LetterDefOf.PositiveEvent, this, null, null, null, null);

        private void HunterAssistanceDialog()
        {
            //var endDiaNodeAccepted = new DiaNode("Story_Start_Dia1_Me_End_Accepted".Translate());
            //endDiaNodeAccepted.options.Add(new DiaOption("Story_Start_Dia1_Me_End_Bye".Translate()) { action = () => GrantAncientGift(initiator, addressed), resolveTree = true });

            //var endDiaNodeDenied = new DiaNode("Story_Start_Dia1_Me_End_Denied".Translate());
            //endDiaNodeDenied.options.Add(new DiaOption("Story_Start_Dia1_Me_End_Bye".Translate()) { resolveTree = true });

            //var subDiaNode = new DiaNode("Story_Start_Dia1_Me".Translate());
            //subDiaNode.options.Add(new DiaOption("Story_Start_Dia1_Me_NoChoice".Translate()) { link = endDiaNodeAccepted });
            //subDiaNode.options.Add(new DiaOption("Story_Start_Dia1_Me_SomeoneBetter".Translate()) { link = endDiaNodeDenied });

            var storyChar = CompCache.StoryWC.questCont.Village.StoryContact;

            if (CompCache.StoryWC.GetCurrentShrineCounter() == 1)
            {
                var diaNode = new DiaNode("Story_Shrine1_SacrilegHunters_Dia1_1".Translate(storyChar.NameShortColored));
                diaNode.options.Add(new DiaOption("Story_Shrine1_SacrilegHunters_Dia1_1_Option1".Translate()) { resolveTree = true, action = () => StoryUtility.GetAssistanceFromAlliedFaction(StoryUtility.FactionOfSacrilegHunters, Map) });
                diaNode.options.Add(new DiaOption("Story_Shrine1_SacrilegHunters_Dia1_1_Option2".Translate()) { resolveTree = true });

                TaggedString taggedString = "Story_Shrine1_SacrilegHunters_DiaTitle".Translate(storyChar.NameShortColored);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
                Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));
            }
            else
            {
                var diaNode = new DiaNode("Story_Shrine2_SacrilegHunters_Dia1_1".Translate());
                diaNode.options.Add(new DiaOption("Story_Shrine2_SacrilegHunters_Dia1_1_Option1".Translate()) { resolveTree = true, action = () => StoryUtility.GetAssistanceFromAlliedFaction(StoryUtility.FactionOfSacrilegHunters, Map) });
                diaNode.options.Add(new DiaOption("Story_Shrine2_SacrilegHunters_Dia1_1_Option2".Translate()) { resolveTree = true });

                TaggedString taggedString = "Story_Shrine1_SacrilegHunters_DiaTitle".Translate(storyChar.NameShortColored);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
                Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));
            }
        }

        private void CheckBossDefeated()
        {
            // todo - test if .destroyed needs to be added (in case of the body being completely nuked)?
            if ((boss != null && !boss.Dead) || (!bossWasSpawned && boss == null && CompCache.StoryWC.storyFlags[CompCache.StoryWC.BuildCurrentShrinePrefix() + "Created"]) || bossDefeatedAndRewardsGiven) return;

            var gifted = StoryUtility.GetGiftedPawn();
            if (gifted == null) Log.Warning("gifted pawn was null, which shouldn't happen. Spell was stored for when another gifted pawn awakes");

            AbilityDef spell = null;
            var endSpell = CaravanAbilities.AbilityDefOf.Named("CAAncientCoordinator");
            //Log.Message($"Unlocked spellcount: {CompCache.StoryWC.GetUnlockedSpells().Count} Database count: {DefDatabase<AbilityDef>.AllDefsListForReading.Where(x => x.defName.StartsWith("CAAncient")).Count()}");
            if (CompCache.StoryWC.GetUnlockedSpells().Count < DefDatabase<AbilityDef>.AllDefsListForReading.Where(x => x.defName.StartsWith("CAAncient") && x != endSpell).Count())
            {
                spell = DefDatabase<AbilityDef>.AllDefsListForReading.Where(x => x.defName.StartsWith("CAAncient") && x != endSpell && !CompCache.StoryWC.GetUnlockedSpells().Contains(x)).InRandomOrder().FirstOrDefault();
                LearnSpell(gifted, spell);
            }
            else if (CompCache.StoryWC.GetCurrentShrineCounter() >= CompCache.StoryWC.GetShrineMaxiumum && !CompCache.StoryWC.GetUnlockedSpells().Contains(endSpell))
            {
                spell = endSpell;
                LearnSpell(gifted, spell);
            }

            CompCache.StoryWC.SetShrineSF("Completed");
            CompCache.StoryWC.IncreaseShrineCompleteCounter();
            CompCache.StoryWC.mechBossKillCounters[boss.kindDef] = CompCache.StoryWC.mechBossKillCounters.TryGetValue(boss.kindDef, out var result) ? result + 1 : 1;

            BossDefeatedDialog(gifted, boss, spell);
            bossDefeatedAndRewardsGiven = true;

            StoryUtility.AdjustGoodWill(75);
            Quests.QuestUtility.AppendQuestDescription(Quests.StoryQuestDefOf.CA_FindAncientShrine,
                (CompCache.StoryWC.GetCurrentShrineCounter(true) - 1 > CompCache.StoryWC.GetShrineMaxiumum
                    ? "Story_Shrine1_QuestRewardUpdate_1_WithoutSpell"
                    : "Story_Shrine1_QuestRewardUpdate_1")
                .Translate(
                    GenDate.DateFullStringAt(
                        (long)GenDate.TickGameToAbs(Find.TickManager.TicksGame),
                        Find.WorldGrid.LongLatOf(Tile)
                    ),
                    CompCache.StoryWC.GetCurrentShrineCounter(true) - 1,
                    gifted.NameShortColored,
                    boss.LabelCap,
                    spell.label.CapitalizeFirst().Colorize(Color.cyan)
                )
            );
        }

        private void LearnSpell(Pawn gifted, AbilityDef spell)
        {
            if (spell == null)
            {
                Log.Error($"Got no spell from boss. -> CheckBossDefeated()");
                return;
            }
            CompCache.StoryWC.GetUnlockedSpells().Add(spell);
            if (gifted != null)
            {
                gifted.abilities.GainAbility(spell);
                Find.LetterStack.ReceiveLetter("Story_Shrine1_AbilityGainedLetterTitle".Translate(spell.LabelCap), "Story_Shrine1_AbilityGainedLetterDesc".Translate(boss.Label, gifted.NameShortColored, spell.label.Colorize(UnityEngine.Color.cyan)), LetterDefOf.PositiveEvent);
            }
        }

        private void BossDefeatedDialog(Pawn gifted, Pawn boss, AbilityDef spell)
        {
            var diaNode2 = new DiaNode("Story_Shrine1_BossDefeated_Dia1_2".Translate(boss.def.label));
            diaNode2.options.Add(new DiaOption("Story_Shrine1_BossDefeated_Dia1_1_Option2".Translate()) { resolveTree = true, action = () => WakeAllMechanoids() });

            var diaNode = new DiaNode(spell == null ? "Story_Shrine1_BossDefeated_Dia1_1_NoSpell".Translate(boss.def.LabelCap.ToString().HtmlFormatting("ff0000")) : "Story_Shrine1_BossDefeated_Dia1_1".Translate(boss.def.LabelCap.ToString().HtmlFormatting("ff0000"), gifted.NameShortColored, spell.LabelCap.ToString().HtmlFormatting("008080")));
            diaNode.options.Add(new DiaOption("Story_Shrine1_BossDefeated_Dia1_1_Option1".Translate()) { link = diaNode2 });

            TaggedString taggedString = "Story_Shrine1_BossDefeated_Dia1Title".Translate(gifted.Name.ToStringShort);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));

            // todo new tale
            //TaleRecorder.RecordTale(TaleDefOf.CaravanAmbushDefeated, new object[]
            //{
            //	base.Map.mapPawns.FreeColonists.RandomElement<Pawn>()
            //});

            // todo gifted null
        }

        private void WakeAllMechanoids()
        {
            Map.mapPawns.AllPawns.Where(x => x.RaceProps.IsMechanoid && !x.Dead).ToList().ForEach(mech => mech.TryGetComp<CompWakeUpDormant>().Activate());
            FreeAllMechsOnMap();

            GetComponent<TimedDetectionPatrols>().ToggleIncreaseStrenthByCounter = true;
        }

        private void FreeAllMechsOnMap()
        {
            var sendLetter = false;
            foreach (var room in Map.regionGrid.allRooms.Where(room => room.ContainsThing(ThingDefOf.AncientCryptosleepCasket) && room.Fogged).Reverse())
            {
                try
                {
                    if (!room.Fogged) continue;
                    var cellToStartUnfog = room.Cells.FirstOrDefault(cell => !room.BorderCells.Contains(cell));
                    if (StoryUtility.FloodUnfogAdjacent(room.Map.fogGrid, Map, cellToStartUnfog, false)) sendLetter = true;

                    var wallToBreak = room.BorderCells.Where(x => x.GetFirstBuilding(Map)?.def == ThingDefOf.Wall && GenRadial.RadialCellsAround(x, 1, false).Any(y => y.InBounds(Map) && y.UsesOutdoorTemperature(Map) && y.Walkable(Map))).InRandomOrder()?.Select(x => x.GetFirstBuilding(Map))?.FirstOrDefault();
                    if (wallToBreak != null) wallToBreak.Destroy();
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }

            if (!sendLetter) return;
            Find.LetterStack.ReceiveLetter("LetterLabelAreaRevealed".Translate(), "AreaRevealedWithMechanoids".Translate(), LetterDefOf.ThreatBig, null, null, null, null, null);
        }

        private void CheckWonBattle()
        {
            // todo - completely redo battle won, we don't really need it. (currently it's being set to wonBattle = true upon killing the boss)
            if (this.wonBattle)
            {
                return;
            }
            if (boss != null && !boss.Dead) return;
            if (GenHostility.AnyHostileActiveThreatToPlayer(base.Map, false))
            {
                return;
            }
            //string forceExitAndRemoveMapCountdownTimeLeftString = TimedForcedExit.GetForceExitAndRemoveMapCountdownTimeLeftString(60000);

            else
            {
                Find.LetterStack.ReceiveLetter("MasterShrineVictoryLetterLabel".Translate(), "MasterShrineVictoryLetterMessage".Translate(), LetterDefOf.PositiveEvent, this, null, null, null, null);
                Find.LetterStack.ReceiveLetter("NoMasterShrineLetterLabel".Translate(), "NoMasterShrineLetterMessage".Translate(), LetterDefOf.NegativeEvent, this, null, null, null, null);
            }
            // todo new tale
            //TaleRecorder.RecordTale(TaleDefOf.CaravanAmbushDefeated, new object[]
            //{
            //	base.Map.mapPawns.FreeColonists.RandomElement<Pawn>()
            //});
            this.wonBattle = true;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var baseGiz in base.GetGizmos())
            {
                yield return baseGiz;
            }

            if (Find.WorldSelector.SingleSelectedObject == this)
            {
                var giveUpCommand = new Command_Action
                {
                    defaultLabel = "Story_Shrine1_GiveUpOnShrineLabel".Translate(),
                    defaultDesc = "Story_Shrine1_GiveUpOnShrineDesc".Translate(),
                    order = 198f,
                    icon = ContentFinder<Texture2D>.Get("UI/commands/AbandonHome", true),
                    action = () =>
                    {
                        if (Map.mapPawns.AnyColonistSpawned)
                        {
                            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Story_Shrine_AbandonColonists".Translate(), delegate
                            {
                                AbandonShrineAndResetFlags();
                            }, false, null));
                        }
                        else AbandonShrineAndResetFlags();
                    }
                };

                yield return giveUpCommand;
            }
        }

        public void AbandonShrineAndResetFlags()
        {
            // todo cleanup + notify story to tick on
            SoundDefOf.Click.PlayOneShot(null);
            if (!bossDefeatedAndRewardsGiven) CompCache.StoryWC.ResetCurrentShrineFlags();
            this.abandonShrine = true;
            Current.Game.DeinitAndRemoveMap(Map);
            this.Destroy();
        }

        private Pawn FindBossNew() => Map.mapPawns.AllPawns.FirstOrDefault(x => CompCache.StoryWC.BossDefs().Contains(x.def));
        private Pawn FindBoss() => this?.Map?.spawnedThings?.FirstOrDefault(x => x is Pawn pawn && CompCache.StoryWC.BossDefs().Contains(pawn.def)) as Pawn ?? null;
    }
}
