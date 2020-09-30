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
		}

		public void Init()
		{
			this.Map.exitMapGrid.Grid.Clear();
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
			}
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
			return;
			if (GenHostility.AnyHostileActiveThreatToPlayer(base.Map, false))
			{
				return;
			}
			//TimedDetectionRaids component = base.GetComponent<TimedDetectionRaids>();
			//component.SetNotifiedSilently();
			//string detectionCountdownTimeLeftString = component.DetectionCountdownTimeLeftString;
			TaleRecorder.RecordTale(TaleDefOf.CaravanAmbushDefeated, new object[]
			{
				base.Map.mapPawns.FreeColonists.RandomElement<Pawn>()
			});
			this.wonBattle = true;
		}

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
			return new List<FloatMenuOption>();
        }

        private bool wonBattle;
	}
}

