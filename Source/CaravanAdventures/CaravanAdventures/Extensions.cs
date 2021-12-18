using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Verse;
using RimWorld;

namespace CaravanAdventures
{
    public static class Extensions
    {
        public static IEnumerable<Pawn> ContainedMechs(this Room room, bool onlyAsleep = false)
        {
            // likely very slow, better find mechs over spawnedPawns
            foreach (var region in room.Regions)
            {
                foreach (var thing in region.ListerThings.AllThings)
                {
                    if (thing is Pawn mech && mech?.Faction == Faction.OfMechanoids)
                    {
                        if (onlyAsleep && mech.Awake()) continue;
                        yield return mech;
                    }
                }
            }
        }
       
    }
}
