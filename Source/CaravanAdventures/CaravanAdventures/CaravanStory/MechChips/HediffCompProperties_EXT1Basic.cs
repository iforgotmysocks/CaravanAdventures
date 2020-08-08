using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace CaravanAdventures.CaravanStory.MechChips
{
    public class HediffCompProperties_EXT1Basic : HediffCompProperties
    {
        public HediffCompProperties_EXT1Basic()
        {
            this.compClass = typeof(HediffComp_EXT1Basic);
        }
    }
}
