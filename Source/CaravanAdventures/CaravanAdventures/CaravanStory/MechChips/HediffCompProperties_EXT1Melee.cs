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
    public class HediffCompProperties_EXT1Melee : HediffCompProperties
    {
        public HediffCompProperties_EXT1Melee()
        {
            this.compClass = typeof(HediffComp_EXT1Melee);
        }
    }
}
