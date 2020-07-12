using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace CaravanAdventures.CaravanAbilities
{
    public class HediffCompProperties_ConjuredLight : HediffCompProperties
    {
        public HediffCompProperties_ConjuredLight()
        {
            this.compClass = typeof(HediffComp_ConjuredLight);
        }
    }
}
