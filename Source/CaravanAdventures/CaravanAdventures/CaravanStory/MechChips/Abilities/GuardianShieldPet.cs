using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanStory.MechChips.Abilities
{
    class GuardianShieldPet : ThingWithComps
    {
        private Pawn owner;
        public Pawn Owner { get => owner; set => owner = value; }
        public override void Draw()
        {
            base.Draw();
        }

        public override void Tick()
        {
            base.Tick();
            this.Position = owner.Position;
        }


    }
}
