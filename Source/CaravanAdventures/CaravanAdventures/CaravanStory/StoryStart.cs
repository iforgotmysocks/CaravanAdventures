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
        private Sustainer animaTreeWhipserSustainer;
        private bool currentStoryTrigger = false;
        private Thing theTree;
        private int ticks;

        public StoryStart(Map map) : base(map)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentStoryTrigger, "currentStoryTrigger", false);
            Scribe_References.Look(ref theTree, "theTree");
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }

        public override void MapGenerated()
        {
            base.MapGenerated();
            // ModLister.RoyaltyInstalled or ModsConfig.RoyaltyActive
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            DrawTreeQuestionMark();

            if (ticks >= 200 && (CompCache.StoryWC.storyFlags["IntroVillage_Finished"] || CompCache.StoryWC.debugFlags["VillageDone"]))
            {
                AddTalkTreeAction();
                AddTreeWhisper();

                // todo maybe run on a WC, so the gift can be past on without a map present
                CheckEnsureGifted();

                ticks = 0;
            }

            ticks++;
        }

        private void DrawTreeQuestionMark()
        {
            if (theTree != null && theTree.TryGetComp<CompTalk>() != null && theTree.TryGetComp<CompTalk>().ShowQuestionMark) theTree.Map.overlayDrawer.DrawOverlay(theTree, OverlayTypes.QuestionMark);
        }

        private void AddTalkTreeAction()
        {
            if (CompCache.StoryWC.storyFlags["Start_InitialTreeAddTalkOption"]) return;
            var tree = map.spawnedThings.FirstOrDefault(x => x.def.defName == "Plant_TreeAnima") as ThingWithComps;
            if (tree == null)
            {
                Log.Message("Tree is null");
                return;
            }
            theTree = tree;
            currentStoryTrigger = true;
            StoryUtility.AssignDialog("StoryStart_TreeDialog", tree, this.GetType().ToString(), "StoryStartDialog", true);
            CompCache.StoryWC.storyFlags["Start_InitialTreeAddTalkOption"] = true;
        }

        private void DisableTreeTalkAction()
        {
            var tree = map.spawnedThings.FirstOrDefault(x => x.def.defName == "Plant_TreeAnima");
            if (tree == null)
            {
                Log.Message("Tree is null");
                return;
            }

            var comp = tree.TryGetComp<CompTalk>();
        }

        private void AddTreeWhisper()
        {
            if (CompCache.StoryWC.storyFlags["Start_ReceivedGift"] 
                || CompCache.StoryWC.storyFlags["Start_InitialTreeWhisper"] && animaTreeWhipserSustainer != null
                || CompCache.StoryWC.storyFlags["Start_InitialTreeWhisper"] && !currentStoryTrigger) return;
            
            var tree = map.spawnedThings.FirstOrDefault(x => x.def.defName == "Plant_TreeAnima");
            if (tree == null)
            {
                Log.Message("Tree is null in AddTreeWisper");
                return;
            }

            var info = SoundInfo.InMap(tree, MaintenanceType.None);
            animaTreeWhipserSustainer = DefDatabase<SoundDef>.GetNamed("CAAnimaTreeWhispers").TrySpawnSustainer(info);
            if (animaTreeWhipserSustainer != null) CompCache.StoryWC.storyFlags["Start_InitialTreeWhisper"] = true;
        }

        public void StoryStartDialog(Pawn initiator, Thing addressed)
        {
            Log.Message($"Story starts initiated by {initiator.Name} and {addressed.def.defName}");
            DiaNode diaNode = null;
            if (!CompCache.StoryWC.storyFlags["Start_CanReceiveGift"])
            {
                var endDiaNodeAccepted = new DiaNode("Story_Start_Dia1_Me_End_Accepted".Translate());
                endDiaNodeAccepted.options.Add(new DiaOption("Story_Start_Dia1_Me_End_Bye".Translate()) { action = () => GrantAncientGift(initiator, addressed), resolveTree = true });

                var endDiaNodeDenied = new DiaNode("Story_Start_Dia1_Me_End_Denied".Translate());
                endDiaNodeDenied.options.Add(new DiaOption("Story_Start_Dia1_Me_End_Bye".Translate()) { resolveTree = true });

                var subDiaNode = new DiaNode("Story_Start_Dia1_Me".Translate());
                subDiaNode.options.Add(new DiaOption("Story_Start_Dia1_Me_NoChoice".Translate()) { link = endDiaNodeAccepted });
                subDiaNode.options.Add(new DiaOption("Story_Start_Dia1_Me_SomeoneBetter".Translate()) { link = endDiaNodeDenied });

                diaNode = new DiaNode("Story_Start_Dia1".Translate());
                diaNode.options.Add(new DiaOption("Story_Start_Dia1_GuessMe".Translate()) { link = subDiaNode }); ;
            }
            else
            {
                var gifted = StoryUtility.GetGiftedPawn();

                var subDiaNode = new DiaNode("Story_Start_Dia1_2_Neg".Translate());
                subDiaNode.options.Add(new DiaOption("Story_Start_Dia1_2_Neg_Option1".Translate()) { resolveTree = true, action = () => CheckEnsureGifted(initiator, true) });

                diaNode = new DiaNode("Story_Start_Dia1_Me_End_GiftAlreadyRecieved".Translate());
                diaNode.options.Add(new DiaOption("Story_Start_Dia1_Me_End_Bye".Translate()) { resolveTree = true }); ;
                diaNode.options.Add(new DiaOption("Story_Start_Dia1_1_Neg_Option2".Translate(gifted.NameShortColored)) { link = subDiaNode }); ;

            }
            TaggedString taggedString = "Story_Start_Dia1_Title".Translate();
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));
        }

        private void GrantAncientGift(Pawn initiator, object addressed)
        {
            CompCache.StoryWC.storyFlags["Start_CanReceiveGift"] = true;
            CheckEnsureGifted(initiator);
            AddAdditionalSpells(initiator);
            if (animaTreeWhipserSustainer != null && !animaTreeWhipserSustainer.Ended) animaTreeWhipserSustainer.End();
            CompCache.StoryWC.storyFlags["Start_ReceivedGift"] = true;
        }

        private void AddAdditionalSpells(Pawn chosen)
        {
            foreach (var abilityDef in DefDatabase<AbilityDef>.AllDefsListForReading.Where(x => x.level == 1).InRandomOrder().Take(2))
            {
                chosen.abilities.GainAbility(abilityDef);
            }
        }

        public void CheckEnsureGifted(Pawn pawn = null, bool forceStrip = false)
        {
            if (!CompCache.StoryWC.storyFlags["Start_CanReceiveGift"]) return;
            var gifted = CompCache.StoryWC.questCont.StoryStart.Gifted;
            if (gifted != null && !gifted.Dead && !forceStrip) return;
            else if (gifted != null && (gifted.Dead || forceStrip))
            {
                gifted.health.hediffSet.hediffs.Remove(gifted.health.hediffSet.hediffs.FirstOrDefault(x => x.def.defName == "CAAncientGift"));
                foreach (var ability in gifted.abilities.abilities.Where(x => x.def.defName.StartsWith("CAAncient")).Reverse())
                {
                    gifted.abilities.RemoveAbility(ability.def);
                }
            }

            // todo choose best candidate
            // todo make sure the pawn isn't psychially unsensitive.
            gifted = pawn ?? PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction?.FirstOrDefault(x => x?.RaceProps?.Humanlike ?? false);

            if (gifted == null)
            {
                Log.Warning($"Gifted pawn couldn't be found, this shouldn't happen.");
                return;
            }

            Log.Message(gifted.Name + " " + gifted.NameFullColored);

            MoteMaker.MakeStaticMote(gifted.Position, gifted.Map, ThingDefOf.Mote_PsycastAreaEffect, 10f);
            SoundDefOf.PsycastPsychicPulse.PlayOneShot(new TargetInfo(gifted));

            gifted.health.AddHediff(DefDatabase<HediffDef>.AllDefs.FirstOrDefault(x => x.defName == "PsychicAmplifier"), gifted.health.hediffSet.GetBrain());
            gifted.health.AddHediff(DefDatabase<HediffDef>.AllDefs.FirstOrDefault(x => x.defName == "CAAncientGift"), gifted.health.hediffSet.GetBrain());

            AddUnlockedAbilities(gifted);
            CompCache.StoryWC.questCont.StoryStart.Gifted = gifted;
        }
        
        private void AddUnlockedAbilities(Pawn chosen)
        {
            if (CompCache.StoryWC.debugFlags["DebugAllAbilities"]) DefDatabase<AbilityDef>.AllDefsListForReading
                   .Where(x => x.defName.StartsWith("CAAncient"))
                   .ToList()
                   .ForEach(spell => {
                       if (!CompCache.StoryWC.GetUnlockedSpells().Contains(spell)) CompCache.StoryWC.GetUnlockedSpells().Add(spell);
                   });

            var abilityDefs = CompCache.StoryWC.GetUnlockedSpells();

            foreach (var abilityDef in abilityDefs)
            {
                chosen.abilities.GainAbility(abilityDef);
            }
            var lightAbilityDef = DefDatabase<AbilityDef>.AllDefs.FirstOrDefault(x => x.defName == "CAConjureLight");
            chosen.abilities.GainAbility(lightAbilityDef);
        }

        public override void MapRemoved()
        {
            base.MapRemoved();
            // todo is currentStoryTrigger really needed?? -> currently used to limit the tree dialog to only one tree, yould prolly allow for all trees with extra checks, needs testing.
            Log.Message($"StoryStart Map removed, resetting StoryStart Flags.");
            if (currentStoryTrigger && !CompCache.StoryWC.storyFlags["Start_ReceivedGift"])
            {
                CompCache.StoryWC.ResetSFsStartingWith("Start_");
            }
        }


    }
}
