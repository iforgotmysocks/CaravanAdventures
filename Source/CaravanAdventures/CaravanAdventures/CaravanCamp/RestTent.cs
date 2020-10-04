using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanCamp
{
    class RestTent : Tent
    {
        public List<Pawn> Occupants { get; set; }

        public RestTent()
        {
            Occupants = new List<Pawn>();
        }
    }
}
