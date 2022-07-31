using RimWorld;
using System.Linq;
using Verse;

namespace CaravanAdventures.CaravanStory.Quests
{
    class QuestCont_FriendlyCaravan : IExposable
    {
        public float friendlyCaravanCounter = -1f;
        public Pawn storyContactBondedPawn;

        public float BaseDelayFriendlyCaravan => Helper.Debug() ? 1000f : 60000f * 5f;
        public float BaseDelayFurtherFriendlyCaravan => Helper.Debug() ? 60000f * 3f : 60000f * 8f;

        public void ExposeData()
        {
            Scribe_Values.Look(ref friendlyCaravanCounter, "friendlyCaravanCounter", -1);
            Scribe_References.Look(ref storyContactBondedPawn, "storyContactBondedPawn");
        }

        public QuestCont_FriendlyCaravan()
        {

        }

        public void FriendlyCaravan_Conversation(Pawn initiator, Pawn addressed)
        {
            var rewardDef = ModSettings.noFreeStuff 
                ? DefDatabase<ThingDef>.GetNamedSilentFail("WoodLog") 
                : (DefDatabase<ResearchProjectDef>.GetNamedSilentFail("Electricity")?.ProgressPercent == 1f
                    ? DefDatabase<ThingDef>.GetNamedSilentFail("VanometricPowerCell")
                    : DefDatabase<ThingDef>.GetNamedSilentFail("Apparel_ShieldBelt"));

            DiaNode diaNode = null;

            var diaNode6_1 = new DiaNode("TradeCaravan_Dia1_6_1".Translate(addressed.NameShortColored, rewardDef != null ? rewardDef.LabelCap.ToString().Colorize(UnityEngine.Color.green) : "", GenderUtility.GetPossessive(addressed.gender)));
            diaNode6_1.options.Add(new DiaOption("TradeCaravan_Dia1_6_1_Option1".Translate()) { action = () => ConversationFinished(initiator, addressed, true, rewardDef), resolveTree = true });
            var diaNode6_2 = new DiaNode("TradeCaravan_Dia1_6_2".Translate(addressed.NameShortColored));
            diaNode6_2.options.Add(new DiaOption("TradeCaravan_Dia1_6_2_Option1".Translate()) { action = () => ConversationFinished(initiator, addressed), resolveTree = true });

            var diaNode5 = new DiaNode("TradeCaravan_Dia1_5".Translate(addressed.NameShortColored, GenderUtility.GetPossessive(addressed.gender)));
            diaNode5.options.Add(new DiaOption("TradeCaravan_Dia1_5_Option1".Translate()) { action = () => StoryUtility.AdjustGoodWill(75), link = diaNode6_1 });
            diaNode5.options.Add(new DiaOption("TradeCaravan_Dia1_5_Option2".Translate()) { action = () => StoryUtility.AdjustGoodWill(50), link = diaNode6_2 });

            var diaNode4 = new DiaNode("TradeCaravan_Dia1_4".Translate(addressed.NameShortColored, GenderUtility.GetPossessive(addressed.gender)));
            diaNode4.options.Add(new DiaOption("TradeCaravan_Dia1_4_Option1".Translate(addressed.NameShortColored, initiator.NameShortColored)) { link = diaNode5 });

            var diaNode3 = new DiaNode("TradeCaravan_Dia1_3".Translate(addressed.NameShortColored));
            diaNode3.options.Add(new DiaOption("TradeCaravan_Dia1_3_Option1".Translate()) { link = diaNode4 });
            diaNode3.options.Add(new DiaOption("TradeCaravan_Dia1_3_Option2".Translate()) { action = () => { ConversationFinished(initiator, addressed); StoryUtility.AdjustGoodWill(30); }, resolveTree = true });

            var diaNode2 = new DiaNode("TradeCaravan_Dia1_2".Translate(addressed.NameShortColored, GenderUtility.GetPossessive(addressed.gender)));
            diaNode2.options.Add(new DiaOption("TradeCaravan_Dia1_2_Option1".Translate()) { link = diaNode3 });

            diaNode = new DiaNode("TradeCaravan_Dia1_1".Translate(addressed.NameShortColored));
            diaNode.options.Add(new DiaOption("TradeCaravan_Dia1_1_Option1".Translate()) { link = diaNode2 });

            TaggedString taggedString = "TradeCaravan_Dia1_DiaTitle".Translate(addressed.NameShortColored);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));
        }

        private void ConversationFinished(Pawn initiator, Pawn addressed, bool reward = false, ThingDef rewardDef = null)
        {
            CompCache.StoryWC.questCont.FriendlyCaravan.storyContactBondedPawn = initiator;
            CompCache.StoryWC.SetSF("TradeCaravan_DialogFinished");
            Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_TradeCaravan, "TradeCaravanQuestInfoTalkedTo".Translate(addressed.NameShortColored));
            if (reward && rewardDef != null && rewardDef.IsBuildingArtificial)
            {
                var cell = ThingMaker.MakeThing(rewardDef);
                cell.SetFaction(Faction.OfPlayer);
                cell = cell.TryMakeMinified();
                GenSpawn.Spawn(cell, addressed.Position, addressed.Map, WipeMode.VanishOrMoveAside);
                Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_TradeCaravan, "TradeCaravanQuestInfoTalkedToReward".Translate(addressed.NameShortColored, rewardDef.LabelCap.ToString().Colorize(UnityEngine.Color.green)));
            }
            else if (reward && rewardDef != null)
            {
                var belt = ThingMaker.MakeThing(rewardDef);
                var beltQual = belt.TryGetComp<CompQuality>();
                if (beltQual != null) beltQual.SetQuality(QualityCategory.Masterwork, ArtGenerationContext.Colony);
                GenSpawn.Spawn(belt, addressed.Position, addressed.Map, WipeMode.VanishOrMoveAside);
                Quests.QuestUtility.AppendQuestDescription(StoryQuestDefOf.CA_TradeCaravan, "TradeCaravanQuestInfoTalkedToReward".Translate(addressed.NameShortColored, rewardDef.LabelCap.ToString().Colorize(UnityEngine.Color.green)));
            }
            Quests.QuestUtility.CompleteQuest(StoryQuestDefOf.CA_TradeCaravan);
        }

        public void TryCreateFriendlyCaravan(ref float friendlyCaravanCounter, Map map = null)
        {
            if (CompCache.StoryWC.storyFlags["TradeCaravan_DialogFinished"]) return;

            DLog.Message($"creating caravan");
            if (Faction.OfPlayer.HostileTo(StoryUtility.FactionOfSacrilegHunters))
            {
                DLog.Message($"Skipping, Sac hunters are hostile.");
                friendlyCaravanCounter = 20000f;
                return;
            }

            BiomeDef sos2Def = null; 
            if (CompatibilityPatches.InDetectedAssemblies("shipshaveinsides")) sos2Def = DefDatabase<BiomeDef>.GetNamed(Patches.Compatibility.SoS2Patch.OuterSpaceBiomeName, false);

            var selectedMap = Find.Maps.Where(cmap => cmap.ParentFaction == Faction.OfPlayerSilentFail 
                && (sos2Def == null ? true : cmap.Biome != sos2Def))
                ?.OrderByDescending(cmap => cmap.wealthWatcher.WealthItems)
                ?.FirstOrDefault();

            if (selectedMap == null)
            {
                DLog.Message($"Story caravan couldn't be created, no player map found");
                friendlyCaravanCounter = 10000f;
                return;
            }
            StoryUtility.EnsureSacrilegHunters();
            if (StoryUtility.FactionOfSacrilegHunters == null)
            {
                Log.Error($"Caravan Adventures Story caravan not created, incompatibility with faction generation!");
                friendlyCaravanCounter = 10000f;
                return;
            }
            var incidentParms = new IncidentParms
            {
                target = selectedMap,
                points = 2000f,
                faction = StoryUtility.FactionOfSacrilegHunters,
                raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn
            };

            if (Helper.RunSavely(() => StoryDefOf.CAFriendlyCaravan.Worker.TryExecute(incidentParms)))
            {
                DLog.Message($"CA trade caravan created successfully");
                friendlyCaravanCounter = BaseDelayFurtherFriendlyCaravan;
            }
            else
            {
                DLog.Warning($"CA Trade caravan couldn't be generated for quest, retrying in 3 minutes");
                friendlyCaravanCounter = 10800;
                return;
            }

            if (!CompCache.StoryWC.storyFlags["TradeCaravan_Arrived"]) QuestUtility.GenerateStoryQuest(StoryQuestDefOf.CA_TradeCaravan, true, "TradeCaravanQuestName", null, "TradeCaravanQuestDesc");
            CompCache.StoryWC.SetSF("TradeCaravan_Arrived");
            StoryUtility.AssignDialog("FriendlyCaravan_Conversation",
               CompCache.StoryWC.questCont.Village.StoryContact,
               typeof(QuestCont_FriendlyCaravan).ToString(),
               "FriendlyCaravan_Conversation");
            DLog.Message($"added conv to mainpawn {CompCache.StoryWC.questCont.Village.StoryContact.Name}");
        }


    }
}
