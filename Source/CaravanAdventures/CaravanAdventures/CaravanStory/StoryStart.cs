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

        public StoryStart(Map map) : base(map)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentStoryTrigger, "currentStoryTrigger", false);
            Scribe_References.Look(ref theTree, "theTree");
            // todo save the sustainer?
            //Scribe_Deep.Look(ref animaTreeWhipserSustainer, "animaTreeWhipserSustainer");
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

            // todo get rid of modulu
            if (Find.TickManager.TicksGame % 2000 == 0)
            {
                AddTalkTreeAction();
                AddTreeWhisper();
                CheckEnsureGifted();
            }

        }

        private void DrawTreeQuestionMark() { if (theTree != null && theTree.TryGetComp<CompTalk>().ShowQuestionMark) theTree.Map.overlayDrawer.DrawOverlay(theTree, OverlayTypes.QuestionMark); }

        private void AddTalkTreeAction()
        {
            if (StoryWC.storyFlags["Start_InitialTreeAddTalkOption"]) return;

            var tree = map.spawnedThings.FirstOrDefault(x => x.def.defName == "Plant_TreeAnima");
            if (tree == null)
            {
                Log.Message("Tree is null");
                return;
            }

            theTree = tree;

            currentStoryTrigger = true;
            Log.Message("adding tree action");
            var comp = tree.TryGetComp<CompTalk>();
            if (comp == null) Log.Message("Tree comp is null");
            comp.actionsCt.Add(new TalkSet()
            {
                Id = "StoryStart_TreeDialog",
                Addressed = tree,
                Initiator = null,
                ClassName = this.GetType().ToString(),
                MethodName = "StoryStartDialog",
                Repeatable = true
            });
            comp.Enabled = true;
            comp.ShowQuestionMark = true;
            StoryWC.storyFlags["Start_InitialTreeAddTalkOption"] = true;
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
            if (StoryWC.storyFlags["Start_InitialTreeWhisper"]) return;
            var tree = map.spawnedThings.FirstOrDefault(x => x.def.defName == "Plant_TreeAnima");
            if (tree == null)
            {
                Log.Message("Tree is null");
                return;
            }

            var info = SoundInfo.InMap(tree, MaintenanceType.None);
            animaTreeWhipserSustainer = DefDatabase<SoundDef>.GetNamed("AnimaTreeWhispers").TrySpawnSustainer(info);
            if (animaTreeWhipserSustainer != null) StoryWC.storyFlags["Start_InitialTreeWhisper"] = true;
        }

        public void StoryStartDialog(Pawn initiator, Thing addressed)
        {
            Log.Message($"Story starts initiated by {initiator.Name} and {addressed.def.defName}");
            DiaNode diaNode = null;
            if (!StoryWC.storyFlags["Start_CanReceiveGift"])
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

            StoryWC.storyFlags["Start_CanReceiveGift"] = true;
            CheckEnsureGifted(initiator);
            AddAdditionalSpells(initiator);
            if (animaTreeWhipserSustainer != null && !animaTreeWhipserSustainer.Ended) animaTreeWhipserSustainer.End();
            StoryWC.storyFlags["Start_ReceivedGift"] = true;
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
            //Log.Message($"Can receive gift flag: {StoryWC.storyFlags["Start_CanReceiveGift"]}");
            if (!StoryWC.storyFlags["Start_CanReceiveGift"]) return;
            var pawns = new List<Pawn>();
            // todo IsColonistPlayerControlled doesn't work here, when pawn is breaking it won't be playercontrolled anymore
            if (pawn == null)
            {
                pawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction?.Where(x => x?.RaceProps?.Humanlike ?? false).ToList();
                if (pawns == null || pawns?.Count == 0)
                {
                    //Log.Message($"No pawns found, skipping.");
                    return;
                }
            }
            else pawns.AddItem(pawn);

            if (pawns.Any(x => x.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AncientGift")) != null))
            {
                //Log.Message($"A gifted char already exists, skipping");
                return;
            }

            // todo choose best candidate
            // todo make sure the pawn isn't psychially unsensitive.
            var chosen = pawn ?? pawns.FirstOrDefault();
            if (chosen == null)
            {
                //Log.Message("Couldn't find pawn, skipping");
                return;
            }

            Log.Message(chosen.Name + " " + chosen.NameFullColored);

            chosen.health.AddHediff(DefDatabase<HediffDef>.AllDefs.FirstOrDefault(x => x.defName == "PsychicAmplifier"), chosen.health.hediffSet.GetBrain());
            chosen.health.AddHediff(DefDatabase<HediffDef>.AllDefs.FirstOrDefault(x => x.defName == "AncientGift"), chosen.health.hediffSet.GetBrain());

            AddUnlockedAbilities(chosen);
        }
        
        private void AddUnlockedAbilities(Pawn chosen)
        {
            var abilityDefs = StoryWC.debugFlags["DebugAllAbilities"] 
                ? DefDatabase<AbilityDef>.AllDefsListForReading.Where(x => x.defName.StartsWith("Ancient"))
                : StoryWC.GetUnlockedSpells();
           
            foreach (var abilityDef in abilityDefs)
            {
                chosen.abilities.GainAbility(abilityDef);
            }
            var lightAbilityDef = DefDatabase<AbilityDef>.AllDefs.FirstOrDefault(x => x.defName == "ConjureLight");
            chosen.abilities.GainAbility(lightAbilityDef);
        }

        public override void MapRemoved()
        {
            base.MapRemoved();
            // todo reset certain storyFlags if required so story can be triggered or continued on another tile?
            if (currentStoryTrigger && !StoryWC.storyFlags["Start_ReceivedGift"])
            {
                foreach (var key in StoryWC.storyFlags.Keys.ToList())
                {
                    StoryWC.storyFlags[key] = false;
                }
            }
        }

     


    }
}
