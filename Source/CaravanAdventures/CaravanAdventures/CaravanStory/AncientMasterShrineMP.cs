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

		public override MapGeneratorDef MapGeneratorDef => CaravanStorySiteDefOf.AncientMasterShrineMG;

        public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.wonBattle, "wonBattle", false, false);
			Scribe_Values.Look(ref bossDefeatedAndRewardsGiven, "bossDefeatedAndRewardsGiven");
			Scribe_References.Look(ref boss, "boss");
			Scribe_Values.Look(ref constTicks, "constTicks");
			Scribe_Values.Look(ref bossDefeatedAndRewardsGiven, "bossDefeatedAndRewardsGiven");

			// todo are mechs enough? Need them for comparison later
			Scribe_Collections.Look(ref generatedMechs, "generatedMechs", LookMode.Reference);
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

			if (boss != null)
			{
				wonBattle = true;

				GetComponent<TimedDetectionPatrols>().Init();
                GetComponent<TimedDetectionPatrols>().StartDetectionCountdown(60000, -1);
            }
			StoryWC.storyFlags[StoryWC.BuildCurrentShrinePrefix() + "Created"] = true;
		}

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
		{
			if (!base.Map.mapPawns.AnyPawnBlockingMapRemoval && boss == null || !base.Map.mapPawns.AnyPawnBlockingMapRemoval && boss != null && boss.Dead)
			{
				if (boss == null) StoryWC.ResetCurrentShrineFlags();
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
				CheckBossDefeated();
				CheckWonBattle();

				if (constTicks == 2400)
				{
					if (boss != null)
					{
						StoryUtility.EnsureSacrilegHunters();

						// todo different dialogs for other shrines, maybe they betray the player the 3rd or 4th shrine.
						CreateShrineDialog();
						StoryUtility.GetAssistanceFromAlliedFaction(StoryUtility.FactionOfSacrilegHunters, Map);
					}
				}

				constTicks++;
			}
		}

        private void LetterNoMasterShrine() => Find.LetterStack.ReceiveLetter("MasterShrineVictoryLetterLabel".Translate(), "MasterShrineVictoryLetterMessage".Translate(), LetterDefOf.PositiveEvent, this, null, null, null, null);

		private void CreateShrineDialog()
		{
			//var endDiaNodeAccepted = new DiaNode("Story_Start_Dia1_Me_End_Accepted".Translate());
			//endDiaNodeAccepted.options.Add(new DiaOption("Story_Start_Dia1_Me_End_Bye".Translate()) { action = () => GrantAncientGift(initiator, addressed), resolveTree = true });

			//var endDiaNodeDenied = new DiaNode("Story_Start_Dia1_Me_End_Denied".Translate());
			//endDiaNodeDenied.options.Add(new DiaOption("Story_Start_Dia1_Me_End_Bye".Translate()) { resolveTree = true });

			//var subDiaNode = new DiaNode("Story_Start_Dia1_Me".Translate());
			//subDiaNode.options.Add(new DiaOption("Story_Start_Dia1_Me_NoChoice".Translate()) { link = endDiaNodeAccepted });
			//subDiaNode.options.Add(new DiaOption("Story_Start_Dia1_Me_SomeoneBetter".Translate()) { link = endDiaNodeDenied });

			var diaNode = new DiaNode("Story_Shrine1_SacrilegHunters_Dia1_1".Translate());
			diaNode.options.Add(new DiaOption("Story_Shrine1_SacrilegHunters_Dia1_1_Option1".Translate()) { resolveTree = true }); ;

			TaggedString taggedString = "Story_Shrine1_SacrilegHunters_DiaTitle".Translate();
			Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, taggedString));
			Find.Archive.Add(new ArchivedDialog(diaNode.text, taggedString));
		}

    

		private void CheckBossDefeated()
        {
			if (boss == null || boss != null && !boss.Dead || bossDefeatedAndRewardsGiven) return;
			// todo popup window doing some story dialog bla

			var gifted = StoryUtility.GetGiftedPawn();
			if (gifted == null) Log.Warning("gifted pawn was null, which shouldn't happen. Spell was stored for when another gifted pawn awakes");

			var spell = DefDatabase<AbilityDef>.AllDefsListForReading.Where(x => x.defName.StartsWith("Ancient") && !StoryWC.GetUnlockedSpells().Contains(x)).InRandomOrder().FirstOrDefault();
			if (spell == null) return;
			else Log.Message($"Got spell");
			StoryWC.GetUnlockedSpells().Add(spell);
			if (gifted != null) gifted.abilities.GainAbility(spell);

			StoryWC.SetShrineSF("Completed");
			StoryWC.IncreaseShrineCompleteCounter();
			StoryWC.mechBossKillCounters[boss.def.defName] = StoryWC.mechBossKillCounters.TryGetValue(boss.def.defName, out var result) ? result + 1 : 0;

			BossDefeatedDialog(gifted, boss, spell);
			bossDefeatedAndRewardsGiven = true;
		}

        private void BossDefeatedDialog(Pawn gifted, Pawn boss, AbilityDef spell)
        {
			var diaNode2 = new DiaNode("Story_Shrine1_BossDefeated_Dia1_2".Translate(boss.def.label));
			diaNode2.options.Add(new DiaOption("Story_Shrine1_BossDefeated_Dia1_1_Option2".Translate()) { resolveTree = true, action = () => WakeAllMechanoids() });

			var diaNode = new DiaNode("Story_Shrine1_BossDefeated_Dia1_1".Translate(boss.def.label, gifted.Name.ToStringShort, spell.label));
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

		private Pawn FindBossNew() => Map.mapPawns.AllPawns.FirstOrDefault(x => StoryWC.GetBossDefNames().Contains(x.def.defName));
		private Pawn FindBoss() => this?.Map?.spawnedThings?.FirstOrDefault(x => x is Pawn pawn && StoryWC.GetBossDefNames().Contains(pawn.def.defName)) as Pawn ?? null;
	}
}
