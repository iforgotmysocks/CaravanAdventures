using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CaravanAdventures.CaravanMechBounty
{
    class BountyUtility
    {
        public static Pawn GenerateVeteran()
        {
            var skillTraits = new Dictionary<TraitDef, int>() {
                { TraitDef.Named("Nimble"), 0},
                { TraitDefOf.ShootingAccuracy, Rand.Chance(0.5f) ? 1 : -1},
                { TraitDefOf.SpeedOffset, 2 }
            };
            var personalityTraits = new[] { TraitDefOf.Cannibal, TraitDefOf.Bloodlust, TraitDefOf.Psychopath, TraitDefOf.Transhumanist, TraitDef.Named("Masochist") };
            var genPawnRequest = new PawnGenerationRequest(CaravanStory.StoryDefOf.CASacrilegHunters_ExperiencedHunter, Faction.OfPlayer)
            {
                MustBeCapableOfViolence = true,
                AllowAddictions = false,
            };
            var veteran = PawnGenerator.GeneratePawn(genPawnRequest);
            if (veteran.ageTracker.AgeBiologicalYears > 50)
            {
                var newAge = veteran.ageTracker.AgeBiologicalTicks = 60000 * 60 * Rand.Range(20, 45);
                veteran.ageTracker.AgeBiologicalTicks = newAge;
                if (veteran.ageTracker.AgeChronologicalTicks < newAge) veteran.ageTracker.AgeChronologicalTicks = newAge;
            }
            foreach (var trait in veteran.story.traits.allTraits.Reverse<Trait>()) veteran.story.traits.allTraits.Remove(trait);
            veteran.story.traits.GainTrait(new Trait(TraitDefOf.Tough));
            var skillTrait = skillTraits.RandomElement();
            veteran.story.traits.GainTrait(new Trait(skillTrait.Key, skillTrait.Value));
            var personalityTrait = personalityTraits.RandomElement();
            veteran.story.traits.GainTrait(new Trait(personalityTrait));

            if (veteran.story.childhood.disallowedTraits == null 
                || veteran.story.childhood.disallowedTraits.Any()
                || veteran.story.childhood.DisabledWorkTypes == null
                || veteran.story.childhood.DisabledWorkTypes.Any())
                veteran.story.childhood = BackstoryDatabase.allBackstories
                    .Where(backstory =>
                    (backstory.Value.DisabledWorkTypes == null 
                        || !backstory.Value.DisabledWorkTypes.Any())
                    && (backstory.Value.disallowedTraits == null 
                        || !backstory.Value.disallowedTraits.Any())
                    && backstory.Value.slot == BackstorySlot.Childhood
                    && backstory.Value.skillGainsResolved != null
                    && (backstory.Value.skillGainsResolved.FirstOrDefault().Key == SkillDefOf.Shooting 
                        || backstory.Value.skillGainsResolved.FirstOrDefault().Key == SkillDefOf.Melee)
                    && backstory.Value.skillGainsResolved.FirstOrDefault().Value > 0).InRandomOrder().FirstOrDefault().Value;

            if (veteran.story.adulthood.disallowedTraits == null
                || veteran.story.adulthood.disallowedTraits.Any()
                || veteran.story.adulthood.DisabledWorkTypes == null
                || veteran.story.adulthood.DisabledWorkTypes.Any())
                veteran.story.adulthood = BackstoryDatabase.allBackstories
                    .Where(backstory =>
                    (backstory.Value.DisabledWorkTypes == null 
                        || !backstory.Value.DisabledWorkTypes.Any())
                    && (backstory.Value.disallowedTraits == null 
                        || !backstory.Value.disallowedTraits.Any())
                    && backstory.Value.slot == BackstorySlot.Adulthood
                    && backstory.Value.skillGainsResolved != null
                    && (backstory.Value.skillGainsResolved.FirstOrDefault().Key == SkillDefOf.Shooting
                        || backstory.Value.skillGainsResolved.FirstOrDefault().Key == SkillDefOf.Melee)
                    && backstory.Value.skillGainsResolved.FirstOrDefault().Value > 0).InRandomOrder().FirstOrDefault().Value;

            veteran.skills.skills.FirstOrDefault(skill => skill.def == SkillDefOf.Shooting).Level = Rand.Range(15, 19);
            veteran.skills.skills.FirstOrDefault(skill => skill.def == SkillDefOf.Shooting).passion = Rand.Chance(0.8f) ? Passion.Major : Passion.Minor;

            veteran.skills.skills.FirstOrDefault(skill => skill.def == SkillDefOf.Melee).Level = Rand.Range(15, 19);
            veteran.skills.skills.FirstOrDefault(skill => skill.def == SkillDefOf.Melee).passion = Rand.Chance(0.8f) ? Passion.Major : Passion.Minor;

            veteran.skills.skills.FirstOrDefault(skill => skill.def == SkillDefOf.Medicine).Level = Rand.Range(7, 15);
            veteran.skills.skills.FirstOrDefault(skill => skill.def == SkillDefOf.Medicine).passion = Rand.Chance(0.5f) ? Passion.Major : Passion.Minor;

            veteran.skills.skills.FirstOrDefault(skill => skill.def == SkillDefOf.Construction).Level = Rand.Range(7, 15);
            veteran.skills.skills.FirstOrDefault(skill => skill.def == SkillDefOf.Construction).passion = Rand.Chance(0.5f) ? Passion.Major : Passion.Minor;

            var gland = veteran.health.hediffSet.hediffs.FirstOrDefault(hediff => hediff.def == HediffDef.Named("StoneskinGland"));
            if (gland != null && Rand.Chance(0.5f)) veteran.health.hediffSet.hediffs.Remove(gland);

            foreach (var hediff in veteran.health.hediffSet.hediffs.Where(hediff => hediff.def.isBad).Reverse()) HealthUtility.CureHediff(hediff);

            return veteran;
        }
    }
}
