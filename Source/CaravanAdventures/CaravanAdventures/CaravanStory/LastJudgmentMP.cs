using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;

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
				// todo replace with better check for boss dead
				if (wonBattle)
				{
					CheckStoryOverDialogAndDisableApocalypse();
					CheckSpawnPortalAndBringHome();
				}
			}
		}

        private void CheckStoryOverDialogAndDisableApocalypse()
        {
			if (CompCache.StoryWC.storyFlags["Judgment_StoryOverDialog"]) return;

			// todo dialog
			CompCache.StoryWC.questCont.LastJudgment.Apocalypse.End();
			CompCache.StoryWC.SetSF("Judgment_StoryOverDialog");
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
			Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
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

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
			return new List<FloatMenuOption>();
        }

        private bool wonBattle;
	}
}

