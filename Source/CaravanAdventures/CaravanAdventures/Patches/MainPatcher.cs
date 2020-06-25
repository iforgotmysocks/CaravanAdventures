using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;

namespace CaravanAdventures.Patches
{
    [StaticConstructorOnStartup]
    static class MainPatcher
    {

        static MainPatcher()
        {
            var harmony = new Harmony("iforgotmysocks.CaravanAdventures");
            CaravanTravel.ApplyPatches(harmony);
        }


    }
}
