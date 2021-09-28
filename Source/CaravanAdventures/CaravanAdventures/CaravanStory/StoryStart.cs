using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using RimWorld.Planet;
using Verse.Sound;
using HarmonyLib;
using Verse.AI;
using UnityEngine;
using System.Reflection;

namespace CaravanAdventures.CaravanStory
{
    class StoryStart : MapComponent
    {
        private Sustainer animaTreeWhisperSustainer;
        private Thing theTree;
        private int ticks = 0;
        private bool whisperStopped = false;

        public StoryStart(Map map) : base(map)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref theTree, "theTree");
            Scribe_Values.Look(ref whisperStopped, "whisperStopped", false);
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }

        public override void MapGenerated()
        {
            base.MapGenerated();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            EnsureWhisperStopped();
            if (!ModsConfig.RoyaltyActive || !ModSettings.storyEnabled) return;

            DrawTreeQuestionMark();

            if (ticks >= 1000)
            {
                ticks = 0;

                if ((CompCache.StoryWC.storyFlags["IntroVillage_Finished"]
                        || CompCache.StoryWC.debugFlags["VillageDone"])
                    && (CompCache.StoryWC.storyFlags["Start_ReceivedGift"]
                        || map.IsPlayerHome))
                {
                    GetTheTree();
                    AddTalkTreeAction();
                    AddTreeWhisper();
                    CheckRemoveWhisperTemporarily();
                    StartTreeQuest();
                    CheckEnsureGifted();
                }
            }

            ticks++;
        }

        private void GetTheTree()
        {
            var tree = map.spawnedThings.FirstOrDefault(x => x.def.defName == "Plant_TreeAnima") as ThingWithComps;
            if (tree == null)
            {
                theTree = null;
                DLog.Message("Tree is null");
                return;
            }
            theTree = tree;
        }

        private void CheckRemoveWhisperTemporarily()
        {
            if (animaTreeWhisperSustainer == null) return;
            if (ModSettings.whisperDisabledManually || theTree == null)
            {
                DLog.Message($"disabling whispers temporarily");
                animaTreeWhisperSustainer.End();
            }
        }

        private void StartTreeQuest()
        {
            if (CompCache.StoryWC.storyFlags["Start_TreeWhisperQuestStarted"]
                || !CompCache.StoryWC.storyFlags["Start_InitialTreeWhisper"]) return;

            Quests.QuestUtility.GenerateStoryQuest(Quests.StoryQuestDefOf.CA_TheTree,
                true,
                "CA_Story_TheTree_QuestName",
                null,
                "CA_Story_TheTree_QuestDesc");

            CompCache.StoryWC.SetSF("Start_TreeWhisperQuestStarted");
        }

        private void DrawTreeQuestionMark()
        {
            if (theTree != null
                && !theTree.Destroyed
                && (!CompCache.StoryWC.storyFlags["Start_ReceivedGift"] && !CompCache.StoryWC.storyFlags["Start_TreeTalkedTo"])
                && theTree.TryGetComp<CompTalk>() != null
                && theTree.TryGetComp<CompTalk>().ShowQuestionMark)
                theTree.Map.overlayDrawer.DrawOverlay(theTree, OverlayTypes.QuestionMark);
        }

        private void AddTalkTreeAction()
        {
            if (theTree == null) return;
            StoryUtility.AssignDialog("StoryStart_TreeDialog", (ThingWithComps)theTree, this.GetType().ToString(), "StoryStartDialog", true, true, true, null, true);
            CompCache.StoryWC.storyFlags["Start_InitialTreeAddTalkOption"] = true;
        }

        private void DisableTreeTalkAction()
        {
            if (theTree == null) return;
            var comp = theTree.TryGetComp<CompTalk>();
        }

        private void AddTreeWhisper()
        {
            if (CompCache.StoryWC.storyFlags["Start_ReceivedGift"]
                || CompCache.StoryWC.storyFlags["Start_TreeTalkedTo"]
                || CompCache.StoryWC.storyFlags["Start_InitialTreeWhisper"] && animaTreeWhisperSustainer != null
                || ModSettings.whisperDisabledManually) return;

            if (theTree == null)
            {
                DLog.Message("Tree is null in AddTreeWisper");
                return;
            }

            var info = SoundInfo.InMap(theTree, MaintenanceType.None);
            animaTreeWhisperSustainer = DefDatabase<SoundDef>.GetNamed("CAAnimaTreeWhispers").TrySpawnSustainer(info);
            if (animaTreeWhisperSustainer != null) CompCache.StoryWC.storyFlags["Start_InitialTreeWhisper"] = true;
        }

        private void EnsureWhisperStopped()
        {
            if (!whisperStopped && ((CompCache.StoryWC.storyFlags["Start_TreeTalkedTo"] || CompCache.StoryWC.storyFlags["Start_ReceivedGift"]) || !ModSettings.storyEnabled))
            {
                whisperStopped = true;
                if (animaTreeWhisperSustainer != null)
                {
                    if (!animaTreeWhisperSustainer.Ended) DLog.Message($"Ending whisper sustainer in fallback check on {map?.Parent?.Label ?? ""}");
                    animaTreeWhisperSustainer.End();
                    DLog.Message($"Ended whisper sustainer in fallback check on {map?.Parent?.Label ?? ""}");
                }
                else DLog.Message($"Sustainer already null in fallback check on {map?.Parent?.Label ?? ""}");
            }
        }

        public void StoryStartDialog(Pawn initiator, Thing addressed)
        {
            DLog.Message($"Story starts initiated by {initiator.Name} and {addressed.def.defName}");
            DiaNode diaNode = null;
            if (!CompCache.StoryWC.storyFlags["Start_CanReceiveGift"])
            {
                var endDiaNodeAccepted = new DiaNode("Story_Start_Dia1_Me_End_Accepted".Translate());
                endDiaNodeAccepted.options.Add(new DiaOption("Story_Start_Dia1_Me_End_Bye".Translate()) { action = () => TalkedToTree(initiator, addressed), resolveTree = true });

                var endDiaNodeDenied = new DiaNode("Story_Start_Dia1_Me_End_Denied".Translate());
                endDiaNodeDenied.options.Add(new DiaOption("Story_Start_Dia1_Me_End_Bye".Translate()) { resolveTree = true, action = () => TalkedToTree(initiator, addressed, false) });

                var subDiaNode = new DiaNode("Story_Start_Dia1_Me".Translate());
                subDiaNode.options.Add(new DiaOption("Story_Start_Dia1_Me_NoChoice".Translate()) { link = endDiaNodeAccepted });
                subDiaNode.options.Add(new DiaOption("Story_Start_Dia1_Me_SomeoneBetter".Translate()) { link = endDiaNodeDenied });

                diaNode = new DiaNode("Story_Start_Dia1".Translate());
                diaNode.options.Add(new DiaOption("Story_Start_Dia1_GuessMe".Translate()) { link = subDiaNode });
            }
            else
            {
                var gifted = StoryUtility.GetGiftedPawn();

                var subDiaNode = new DiaNode("Story_Start_Dia1_2_Neg".Translate());
                subDiaNode.options.Add(new DiaOption("Story_Start_Dia1_2_Neg_Option1".Translate()) { resolveTree = true, action = () => CheckEnsureGifted(initiator, true) });

                diaNode = new DiaNode("Story_Start_Dia1_Me_End_GiftAlreadyRecieved".Translate());
                diaNode.options.Add(new DiaOption("Story_Start_Dia1_Me_End_Bye".Translate()) { resolveTree = true });
                if (gifted != initiator) diaNode.options.Add(new DiaOption("Story_Start_Dia1_1_Neg_Option2".Translate(gifted.NameShortColored, GenderUtility.GetPossessive(gifted.gender))) { link = subDiaNode });
            }

            if (CompCache.StoryWC.storyFlags["Judgment_Completed"])
            {
                var apoActive = CompCache.StoryWC.questCont.LastJudgment?.Apocalypse != null;
                var subDiaNode = new DiaNode(apoActive
                    ? "CA_Story_Start_MachinesDisabled".Translate()
                    : "CA_Story_Start_MachinesEnabled".Translate());
                subDiaNode.options.Add(new DiaOption("CA_Story_Start_Bye".Translate()) { resolveTree = true, action = () => TriggerApocalypse(initiator, !apoActive) });

                diaNode.options.Add(new DiaOption("CA_Story_Start_EnableAncientMachines".Translate()) { link = subDiaNode }); ;
            }

            TaggedString taggedString = "Story_Start_Dia1_Title".Translate();
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));
        }

        private void TriggerApocalypse(Pawn initiator, bool enable)
        {
            if (enable) CompCache.StoryWC.questCont.LastJudgment.StartApocalypse(-20);
            else CompCache.StoryWC.questCont.LastJudgment.EndApocalypse();
        }

        private void TalkedToTree(Pawn initiator, object addressed, bool acceptedGift = true)
        {
            CompCache.StoryWC.SetSF("Start_TreeTalkedTo");
            foreach (var susMap in Find.Maps)
            {
                if (susMap == null) continue;
                var comp = susMap.GetComponent<StoryStart>();
                if (comp == null || comp.animaTreeWhisperSustainer == null) continue;
                DLog.Message($"Ending sustainer on {susMap.Parent.Label}");
                comp.animaTreeWhisperSustainer.End();
            }

            if (acceptedGift) GrantAncientGift(initiator, addressed);
            else Quests.QuestUtility.AppendQuestDescription(Quests.StoryQuestDefOf.CA_TheTree, "CA_Story_DeniedGift".Translate(initiator.NameShortColored), false, true);
        }

        private void GrantAncientGift(Pawn initiator, object addressed)
        {
            CompCache.StoryWC.storyFlags["Start_CanReceiveGift"] = true;
            CheckEnsureGifted(initiator);
            CompCache.StoryWC.storyFlags["Start_ReceivedGift"] = true;

            Quests.QuestUtility.AppendQuestDescription(Quests.StoryQuestDefOf.CA_TheTree, "CA_Story_ReceivedGiftLetterDesc".Translate(initiator.NameShortColored, GenderUtility.GetPronoun(initiator.gender)));
            Quests.QuestUtility.CompleteQuest(Quests.StoryQuestDefOf.CA_TheTree);
        }

        private void AddAdditionalSpells(Pawn chosen)
        {
            foreach (var abilityDef in DefDatabase<AbilityDef>.AllDefsListForReading
                .Where(x => !chosen.abilities.abilities.Select(ab => ab.def).Contains(x) && x.level != 0)
                .OrderBy(ab => ab.level).ThenBy(x => Guid.NewGuid()).Take(2))
            {
                chosen.abilities.GainAbility(abilityDef);
            }
        }

        public void CheckEnsureGifted(Pawn pawn = null, bool forceStrip = false)
        {
            if (!CompCache.StoryWC.storyFlags["Start_CanReceiveGift"]) return;
            var gifted = CompCache.StoryWC.questCont.StoryStart.Gifted;
            if (gifted != null && !gifted.Dead && !gifted.Destroyed && gifted.Faction == Faction.OfPlayer && !forceStrip) return;
            else if (gifted != null && (gifted.Dead || gifted.Faction != Faction.OfPlayer || forceStrip))
            {
                gifted.health.hediffSet.hediffs.Remove(gifted.health.hediffSet.hediffs.FirstOrDefault(x => x.def.defName == "CAAncientGift"));
                foreach (var ability in gifted.abilities.abilities.Where(x => x.def.defName.StartsWith("CAAncient")).Reverse())
                {
                    gifted.abilities.RemoveAbility(ability.def);
                }
            }

            // todo choose best candidate
            // todo make sure the pawn isn't psychially unsensitive.
            gifted = pawn ?? PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction?.FirstOrDefault(x => (x?.RaceProps?.Humanlike ?? false) && !x.HasExtraHomeFaction() && !x.HasExtraMiniFaction());

            if (gifted == null)
            {
                Log.Warning($"Gifted pawn couldn't be found, this shouldn't happen.");
                return;
            }

            DLog.Message(gifted.Name + " " + gifted.NameFullColored);

            MoteMaker.MakeStaticMote(gifted.Position, gifted.Map, ThingDefOf.Mote_CastPsycast, 10f);
            SoundDefOf.PsycastPsychicPulse.PlayOneShot(new TargetInfo(gifted));

            gifted.health.AddHediff(DefDatabase<HediffDef>.AllDefs.FirstOrDefault(x => x.defName == "PsychicAmplifier"), gifted.health.hediffSet.GetBrain());
            gifted.health.AddHediff(DefDatabase<HediffDef>.AllDefs.FirstOrDefault(x => x.defName == "CAAncientGift"), gifted.health.hediffSet.GetBrain());

            var spellCount = gifted?.abilities?.abilities?.Count;
            AddUnlockedAbilities(gifted);
            CompCache.StoryWC.questCont.StoryStart.Gifted = gifted;
            if (spellCount == 0 || spellCount == 1) AddAdditionalSpells(gifted);
            Find.LetterStack.ReceiveLetter("CA_Story_ReceivedGiftLetterTitle".Translate(), "CA_Story_ReceivedGiftLetterDesc".Translate(gifted.NameShortColored, GenderUtility.GetPronoun(gifted.gender)), LetterDefOf.PositiveEvent);
        }

        private void AddUnlockedAbilities(Pawn chosen)
        {
            if (CompCache.StoryWC.debugFlags["DebugAllAbilities"]) DefDatabase<AbilityDef>.AllDefsListForReading
                   .Where(x => x.defName.StartsWith("CAAncient"))
                   .ToList()
                   .ForEach(spell =>
                   {
                       if (!CompCache.StoryWC.GetUnlockedSpells().Contains(spell)) CompCache.StoryWC.GetUnlockedSpells().Add(spell);
                   });

            var abilityDefs = CompCache.StoryWC.GetUnlockedSpells();

            foreach (var abilityDef in abilityDefs)
            {
                chosen.abilities.GainAbility(abilityDef);
            }
            var lightAbilityDef = DefDatabase<AbilityDef>.AllDefs.FirstOrDefault(x => x.defName == "CAConjureLight");
            if (lightAbilityDef != null) chosen.abilities.GainAbility(lightAbilityDef);
        }

        public override void MapRemoved()
        {
            base.MapRemoved();

            if (!CompCache.StoryWC.storyFlags["Start_ReceivedGift"])
            {
                DLog.Message($"StoryStart Map removed, resetting StoryStart Flags.");
                CompCache.StoryWC.SetSFsStartingWith("Start_");
            }
        }


    }
}
