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
        public HediffCompProperties_AncientProtectiveAura()
        {
            // todo why dafuq was _Old referenced here? -> probably didn't matter since the compclass was set via xml
            this.compClass = typeof(HediffComp_AncientProtectiveAura);
        }

        public bool linked = false;
    }
}
