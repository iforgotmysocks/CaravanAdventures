using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse.AI.Group;
using Verse;

namespace CaravanAdventures.CaravanStory.Lords
{
    class Trigger_ChanceOnMechHarmNPCBuilding : Trigger
	{
		public Trigger_ChanceOnMechHarmNPCBuilding(float chance)
		{
			this.chance = chance;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			return signal.type == TriggerSignalType.BuildingDamaged && signal.dinfo.Def.ExternalViolenceFor(signal.thing) && signal.thing.def.category == ThingCategory.Building && signal.dinfo.Instigator != null && signal.dinfo.Instigator.Faction == Faction.OfMechanoids && signal.thing.Faction != Faction.OfMechanoids && Rand.Value < this.chance;
		}

		private float chance = 1f;
	}
}
