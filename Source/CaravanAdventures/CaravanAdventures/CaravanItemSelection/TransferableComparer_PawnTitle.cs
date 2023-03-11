using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanItemSelection
{
    public class TransferableComparer_PawnTitle : TransferableComparer
    {
        public override int Compare(Transferable lhs, Transferable rhs)
        {
            var lT = (lhs?.AnyThing as Pawn)?.story?.title ?? "";
            var rT = (rhs?.AnyThing as Pawn)?.story?.title ?? "";

            return lT.CompareTo(rT);
        }
    }
}
