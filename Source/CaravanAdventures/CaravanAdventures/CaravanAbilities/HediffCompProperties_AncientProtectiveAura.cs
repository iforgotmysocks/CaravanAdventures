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
    public class HediffCompProperties_AncientProtectiveAura : HediffCompProperties
    {
        public
            HediffCompProperties_AncientProtectiveAura()
        {
            this.compClass = typeof(HediffComp_AncientProtectiveAura_Old);
        }

        public bool linked = false;
    }
}
