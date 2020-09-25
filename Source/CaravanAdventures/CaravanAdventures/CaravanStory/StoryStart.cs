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

            if (ticks >= 2000 &&CompCache.StoryWC.storyFlags["IntroVillage_Finished"] ||CompCache.StoryWC.debugFlags["VillageFinished"])
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
                || CompCache.StoryWC.storyFlags["Start_InitialTreeWhisper"] 
                && animaTreeWhipserSustainer != null) return;
            
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
                diaNode = new DiaNode("Story_Start_Dia1_Me_End_GiftAlreadyRecieved".Translate());
                diaNode.options.Add(new DiaOption("Story_Start_Dia1_Me_End_Bye".Translate()) { resolveTree = true }); ;
            }
            TaggedString taggedString = "Story_Start_Dia1_Title".Translate();
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));
        }

        private void GrantAncientGift(Pawn initiator, object addressed)
        {
            MoteMaker.MakeStaticMote(initiator.Position, initiator.Map, ThingDefOf.Mote_PsycastAreaEffect, 10f);
            SoundDefOf.PsycastPsychicPulse.PlayOneShot(new TargetInfo(initiator));

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

        public void CheckEnsureGifted(Pawn pawn = null)
        {
            if (!CompCache.StoryWC.storyFlags["Start_CanReceiveGift"]) return;
            var pawns = new List<Pawn>();
            if (pawn == null)
            {
                pawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction?.Where(x => x?.RaceProps?.Humanlike ?? false).ToList();
                if (pawns == null || pawns?.Count == 0) return;
            }
            else pawns.AddItem(pawn);

            if (pawns.Any(x => x.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("CAAncientGift")) != null)) return;

            // todo choose best candidate
            // todo make sure the pawn isn't psychially unsensitive.
            var chosen = pawn ?? pawns.FirstOrDefault();
            if (chosen == null) return;

            Log.Message(chosen.Name + " " + chosen.NameFullColored);

            chosen.health.AddHediff(DefDatabase<HediffDef>.AllDefs.FirstOrDefault(x => x.defName == "PsychicAmplifier"), chosen.health.hediffSet.GetBrain());
            chosen.health.AddHediff(DefDatabase<HediffDef>.AllDefs.FirstOrDefault(x => x.defName == "CAAncientGift"), chosen.health.hediffSet.GetBrain());

            AddUnlockedAbilities(chosen);
        }
        
        private void AddUnlockedAbilities(Pawn chosen)
        {
            var abilityDefs =CompCache.StoryWC.debugFlags["DebugAllAbilities"] 
                ? DefDatabase<AbilityDef>.AllDefsListForReading.Where(x => x.defName.StartsWith("CAAncient"))
                : CompCache.StoryWC.GetUnlockedSpells();
           
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
            // todo is currentStoryTrigger really needed??
            Log.Message($"StoryStart Map removed, resetting StoryStart Flags.");
            if (currentStoryTrigger && !CompCache.StoryWC.storyFlags["Start_ReceivedGift"])
            {
                CompCache.StoryWC.ResetSFsStartingWith("Start_");
            }
        }


    }
}
