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

		public override MapGeneratorDef MapGeneratorDef => CaravanStorySiteDefOf.AncientMasterShrineMG;

        public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.wonBattle, "wonBattle", false, false);
		}

		public void Init()
        {
			boss = FindBoss();
			Log.Message($"Map has boss: {boss != null}");

			StoryWC.storyFlags[StoryWC.BuildCurrentShrinePrefix() + "Created"] = true;
		}

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
		{
			if (!base.Map.mapPawns.AnyPawnBlockingMapRemoval && boss == null || !base.Map.mapPawns.AnyPawnBlockingMapRemoval && boss != null && boss.Dead)
			{
				Cleanup();
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
			}
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
				Find.LetterStack.ReceiveLetter("MasterShrineVictoryBossLetterLabel".Translate(boss.Name.ToStringFull), "MasterShrineVictoryBossLetterMessage".Translate(boss.Name, "abilitynametodo"), LetterDefOf.PositiveEvent, this, null, null, null, null);
				StoryWC.storyFlags[StoryWC.BuildCurrentShrinePrefix() + "Completed"] = true;
				StoryWC.IncreaseShrineCompleteCounter();
			}

			else Find.LetterStack.ReceiveLetter("MasterShrineVictoryLetterLabel".Translate(), "MasterShrineVictoryLetterMessage".Translate(), LetterDefOf.PositiveEvent, this, null, null, null, null);
			// todo new tale
			TaleRecorder.RecordTale(TaleDefOf.CaravanAmbushDefeated, new object[]
			{
				base.Map.mapPawns.FreeColonists.RandomElement<Pawn>()
			});
			this.wonBattle = true;
		}

		private void Cleanup()
		{
			if (boss == null) StoryWC.ResetCurrentShrineFlags();
            
			foreach (var coll in new List<List<Pawn>> {generatedBandits, generatedMechs, generatedSoldiers})
            {
				foreach (var pawn in coll.Reverse<Pawn>())
                {
				// todo send to world if relationship
					pawn.Destroy();
                }
            }

			if (boss != null) boss.Destroy();
		}

		private Pawn FindBoss() => this?.Map?.spawnedThings?.FirstOrDefault(x => x is Pawn pawn && StoryWC.GetBossDefNames().Contains(pawn.def.defName)) as Pawn ?? null;
	}
}
