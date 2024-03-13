using System.Linq;
using Verse;

namespace CaravanAdventures.CaravanStory.MechChips.Abilities
{
    class GuardianShield : ThingWithComps
    {
        private Pawn owner;
        private float absorbedDamage;
        private float totalDamage = 2000;
        private int ticks;
        private float damageFluxPerSec = 20;

        public Pawn Owner { get => owner; set => owner = value; }
        public float AbsorbedDamage { get => absorbedDamage; set => absorbedDamage = value; }
        public float DamageFluxPerSec { get => damageFluxPerSec; set => damageFluxPerSec = value; }

        public override void Tick()
        {
            base.Tick();
            if (owner == null || owner.Dead || this.Map == null) Destroy();

            if (ticks > 120)
            {
                ticks = 0;
                this.Position = owner.Position;

                if (this.AbsorbedDamage > totalDamage) this.Destroy();
                if (AbsorbedDamage >= damageFluxPerSec) AbsorbedDamage -= damageFluxPerSec * 2;
            }

            ticks++;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            if (owner == null || owner.Destroyed || owner.Dead) return;
            var chip = owner.health.hediffSet.GetAllComps().OfType<HediffComp_EXT1Guardian>().FirstOrDefault();
            if (chip == null) return;
            chip.ShieldCooldown = chip.shieldCooldownBase;
        }


    }
}
