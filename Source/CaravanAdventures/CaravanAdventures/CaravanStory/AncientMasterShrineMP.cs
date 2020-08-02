using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

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

        public override MapGeneratorDef MapGeneratorDef => CaravanStorySiteDefOf.AncientMasterShrineMG;

        public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.wonBattle, "wonBattle", false, false);
			Scribe_Values.Look(ref bossDefeatedAndRewardsGiven, "bossDefeatedAndRewardsGiven");
			Scribe_Values.Look(ref boss, "boss");
		}

		public void Init()
        {
			//boss = FindBossNew();

			Log.Message($"compare mechs. mapPawns: {Map.mapPawns.AllPawns.Where(x => x.RaceProps.IsMechanoid).Count()} ourlist: {generatedMechs.Count}");
			
			foreach (var pawn in Map.mapPawns.AllPawns.Where(x => x.RaceProps.IsMechanoid))
            {
				Log.Message($"defname: {pawn.def.defName} kind: {pawn.kindDef.defName} other: {pawn.def.label} kindlabel: {pawn.kindDef.label}");
            }

			var notMatching = generatedMechs.Where(x => !Map.mapPawns.AllPawns.Where(y => y.RaceProps.IsMechanoid).Any(z => z.ThingID == x.ThingID));
			Log.Message($"Not matching count: {notMatching.Count()}");

			Log.Message($"Map has boss: {boss != null}");

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
				this.CheckWonBattle();
			}
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
			// todo info about spell? Letter?

			WakeAllMechanoids();
			bossDefeatedAndRewardsGiven = true;
		}

        private void WakeAllMechanoids()
        {
			Map.mapPawns.AllPawns.Where(x => x.RaceProps.IsMechanoid && !x.Dead).ToList().ForEach(mech => mech.TryGetComp<CompWakeUpDormant>().Activate());
        }

        private void CheckWonBattle()
		{
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

			if (boss != null && boss.Dead)
			{
				Find.LetterStack.ReceiveLetter("MasterShrineVictoryBossLetterLabel".Translate(boss.Name), "MasterShrineVictoryBossLetterMessage".Translate(boss.Name, "abilitynametodo"), LetterDefOf.PositiveEvent, this, null, null, null, null);
				StoryWC.storyFlags[StoryWC.BuildCurrentShrinePrefix() + "Completed"] = true;
				StoryWC.IncreaseShrineCompleteCounter();
			}

			else Find.LetterStack.ReceiveLetter("MasterShrineVictoryLetterLabel".Translate(), "MasterShrineVictoryLetterMessage".Translate(), LetterDefOf.PositiveEvent, this, null, null, null, null);
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
