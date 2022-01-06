using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace CaravanAdventures.CaravanStory
{
    class LastJudgmentMP : MapParent
    {
        public override MapGeneratorDef MapGeneratorDef => CaravanStorySiteDefOf.CALastJudgmentMG;
		private Pawn endBoss;
		private Thing portalHome;
		private AncientMasterShrineMP ancientShrineMP;

		public bool WonBattle
		{
			get
			{
				return this.wonBattle;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.wonBattle, "wonBattle", false, false);
			Scribe_References.Look(ref endBoss, "endboss");
			Scribe_References.Look(ref portalHome, "portalHome");
			Scribe_References.Look(ref ancientShrineMP, "mapParentParent");
		}

		public void Init(AncientMasterShrineMP mapParent)
		{
			this.ancientShrineMP = mapParent;
			this.Map.exitMapGrid.Grid.Clear();
			endBoss = StoryUtility.GetFittingMechBoss(true);
			GenSpawn.Spawn(endBoss, new IntVec3(25, 0, 42), Map);
            var newLord = LordMaker.MakeNewLord(endBoss.Faction, new LordJob_AssaultColony(Faction.OfMechanoids, false, false, false, false, false), Map);
            if (newLord != null) newLord.AddPawn(endBoss);
			CompCache.StoryWC.SetSF("Judgment_Created");
		}

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
		{
			if (!base.Map.mapPawns.AnyPawnBlockingMapRemoval)
			{
				alsoRemoveWorldObject = true;
				return true;
			}
			alsoRemoveWorldObject = false;
			return false;
		}

		public override void Tick()
		{
			base.Tick();
			if (base.HasMap)
			{
				this.CheckWonBattle();
				CheckBossDefeated();
				if (wonBattle) CheckSpawnPortalAndBringHome();
			}
		}

		private void CheckBossDefeated()
		{
			if (endBoss != null && !endBoss.Dead && !endBoss.Destroyed || CompCache.StoryWC.storyFlags["Judgment_StoryOverDialog"]) return;

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
			CompCache.StoryWC.mechBossKillCounters[endBoss.kindDef] = CompCache.StoryWC.mechBossKillCounters.TryGetValue(endBoss.kindDef, out var result) ? result + 1 : 1;
            CompCache.StoryWC.questCont.LastJudgment.EndApocalypse();
            CompCache.StoryWC.SetSF("Judgment_StoryOverDialog");
			BossDefeatedDialog(gifted, endBoss, spell);
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
					endBoss.LabelCap,
					spell.label.CapitalizeFirst().Colorize(Color.cyan)
				)
			);
			Quests.QuestUtility.AppendQuestDescription(Quests.StoryQuestDefOf.CA_FindAncientShrine, Helper.HtmlFormatting("Story_Shrine5_QuestUpdate_2".Translate(endBoss.NameShortColored), "b6f542"), false, true);
			Quests.QuestUtility.AppendQuestDescription(Quests.StoryQuestDefOf.CA_FindAncientShrine, "Story_Shrine5_QuestUpdate_Info_1".Translate(), false, true);
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
				Find.LetterStack.ReceiveLetter("Story_Shrine1_AbilityGainedLetterTitle".Translate(spell.LabelCap), "Story_Shrine1_AbilityGainedLetterDesc".Translate(endBoss.Label, gifted.NameShortColored, spell.label.Colorize(UnityEngine.Color.cyan)), LetterDefOf.PositiveEvent);
			}
		}

		private void BossDefeatedDialog(Pawn gifted, Pawn boss, AbilityDef spell)
		{
			var diaNode2 = new DiaNode("Story_Shrine5_BossDefeated_Dia1_2".Translate(boss.def.label));
			diaNode2.options.Add(new DiaOption("Story_Shrine5_BossDefeated_Dia1_2_Option1".Translate()) { resolveTree = true, action = () => CheckStoryOverDialogAndDisableApocalypse() });

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

		private void CheckStoryOverDialogAndDisableApocalypse()
		{
			StoryUtility.GenerateStoryContact();
			var storyChar = CompCache.StoryWC.questCont.Village.StoryContact;
			var diaNode = new DiaNode("Story_Shrine5_Apocalypse_Dia1_1".Translate(storyChar.NameShortColored));
			diaNode.options.Add(new DiaOption("Story_Shrine5_Apocalypse_Dia1_1_Option1".Translate()) { resolveTree = true });

			TaggedString taggedString = "Story_Shrine1_Apocalypse_Dia1Title".Translate(storyChar.NameShortColored, storyChar.Faction.NameColored);
			Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
			Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));

			CompCache.BountyWC.BountyPoints += 4000;
		}

		private void CheckSpawnPortalAndBringHome()
        {
            if (portalHome == null)
            {
				var portal = ThingMaker.MakeThing(StoryDefOf.CAShrinePortal);
				portalHome = GenSpawn.Spawn(portal, ancientShrineMP.portalSpawnPosition, Map, WipeMode.Vanish);
			}

			var triggerCells = new IntVec3[] { portalHome.Position }; // GenRadial.RadialCellsAround(lastJudgmentEntrance.Position, 1, true);
			if (!triggerCells.Any(cell => cell.GetFirstPawn(Map) == CompCache.StoryWC.questCont.StoryStart.Gifted)) return;

			var gifted = CompCache.StoryWC.questCont.StoryStart.Gifted;
			if (gifted.Spawned) gifted.DeSpawn();
			GenSpawn.Spawn(gifted, ancientShrineMP.lastJudgmentEntrance.Position, ancientShrineMP.Map);
			gifted.drafter.Drafted = true;
			//Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
			CompCache.StoryWC.SetSF("Judgment_Completed");
			ancientShrineMP.lastJudgmentEntrance.Destroy();
			CameraJumper.TryJump(gifted);
		}

		public override void PostMapGenerate()
		{
			base.PostMapGenerate();
			//base.GetComponent<TimedDetectionRaids>().StartDetectionCountdown(240000, -1);
		}

		private void CheckWonBattle()
		{
			if (this.wonBattle)
			{
				return;
			}
			if (GenHostility.AnyHostileActiveThreatToPlayer(base.Map, false))
			{
				return;
			}

			//TaleRecorder.RecordTale(TaleDefOf.CaravanAmbushDefeated, new object[]
			//{
			//	base.Map.mapPawns.FreeColonists.RandomElement<Pawn>()
			//});
			this.wonBattle = true;
		}

        public override void PostRemove()
        {
            base.PostRemove();
			if (endBoss != null && !endBoss.Destroyed) endBoss.Destroy();
			if (CompCache.StoryWC.storyFlags["Judgment_StoryOverDialog"] && !CompCache.StoryWC.storyFlags["Judgment_Completed"]) CompCache.StoryWC.SetSF("Judgment_Completed");
			if (CompCache.StoryWC.storyFlags["Judgment_Completed"]) LeavingShrineDialog();
		}

		private void LeavingShrineDialog()
		{
			var storyChar = CompCache.StoryWC.questCont.Village.StoryContact;
			var gifted = CompCache.StoryWC.questCont.StoryStart.Gifted;
			switch (CompCache.StoryWC.GetCurrentShrineCounter(true))
			{
				case 6:
					var diaNode = new DiaNode("Story_Shrine5_Apocalypse_Dia2_1".Translate(gifted.NameShortColored));
					diaNode.options.Add(new DiaOption("Story_Shrine5_Apocalypse_Dia2_1_Option1".Translate()) { resolveTree = true });

					var taggedString = "Story_Shrine5_Apocalypse_Dia2Title".Translate(storyChar.NameShortColored, storyChar.Faction.NameColored);
					Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
					Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));
					break;
			}
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
			return new List<FloatMenuOption>();
        }

        private bool wonBattle;
	}
}

