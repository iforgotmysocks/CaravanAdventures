using RimWorld;
using System;
using System.Linq;
using Verse;

namespace CaravanAdventures.CaravanMechBounty
{
    class BountyUtility
    {
        public static Pawn GenerateVeteran(TraitDef selPersonality = null, TraitDef selSkill = null, bool tribal = false)
        {
            var veteran = PrepareVeteranPawn(selPersonality, selSkill);
            if (veteran == null) return null;
            CheckVeteranChildhoodBackstory(veteran, tribal);
            CheckVeteranAdultBackstory(veteran);
            AdjustVeteranSkills(veteran);
            ConfigureVeteranHediffs(veteran);
            return veteran;
        }

        private static Pawn PrepareVeteranPawn(TraitDef selPersonality, TraitDef selSkill)
        {
            var genPawnRequest = new PawnGenerationRequest(ModSettings.storyEnabled ? CaravanStory.StoryDefOf.CASacrilegHunters_ExperiencedHunter : PawnKindDefOf.SpaceRefugee, Faction.OfPlayer)
            {
                MustBeCapableOfViolence = true,
                AllowAddictions = false,
                FixedIdeo = ModsConfig.IdeologyActive ? Faction.OfPlayerSilentFail?.ideos?.PrimaryIdeo : default
            };
            var veteran = PawnGenerator.GeneratePawn(genPawnRequest);
            if (veteran == null) return null;
            if (veteran.ageTracker.AgeBiologicalYears > 50)
            {
                var newAge = veteran.ageTracker.AgeBiologicalTicks = 60000 * 60 * Rand.Range(20, 45);
                veteran.ageTracker.AgeBiologicalTicks = newAge;
                if (veteran.ageTracker.AgeChronologicalTicks < newAge) veteran.ageTracker.AgeChronologicalTicks = newAge;
            }
            foreach (var trait in veteran.story.traits.allTraits.Reverse<Trait>()) veteran.story.traits.allTraits.Remove(trait);
            if (TraitDefOf.Tough != null) veteran.story.traits.GainTrait(new Trait(TraitDefOf.Tough));
            if (selPersonality != null) veteran.story.traits.GainTrait(new Trait(selPersonality, selPersonality.degreeDatas.OrderByDescending(data => data.degree).FirstOrDefault().degree));
            if (selSkill != null) veteran.story.traits.GainTrait(new Trait(selSkill, selSkill.degreeDatas.OrderByDescending(data => data.degree).FirstOrDefault().degree));
            return veteran;
        }

        private static void CheckVeteranChildhoodBackstory(Pawn veteran, bool tribal)
        {
            var tribalCatName = MeditationFocusDefOf.Natural?.requiredBackstoriesAny?.FirstOrDefault()?.categoryName ?? "Tribal";

            if ((veteran?.story?.childhood?.disallowedTraits?.Any() ?? true)
                || (veteran?.story?.childhood?.DisabledWorkTypes?.Any() ?? true
                || CanUseTribalBackstory(tribal) && (!veteran?.story?.childhood?.spawnCategories?.Contains(tribalCatName) ?? false)))
                veteran.story.childhood = BackstoryDatabase.allBackstories
                    .Where(backstory =>
                    (CanUseTribalBackstory(tribal) ? backstory.Value.spawnCategories.Contains(tribalCatName) : true)
                    && (!backstory.Value?.DisabledWorkTypes?.Any() ?? true)
                    && (!backstory.Value?.disallowedTraits?.Any() ?? true)
                    && backstory.Value?.slot == BackstorySlot.Childhood
                    && ((backstory.Value?.skillGainsResolved?.Any(x => x.Key == SkillDefOf.Shooting && x.Value > 0) ?? false)
                        || (backstory.Value?.skillGainsResolved?.Any(x => x.Key == SkillDefOf.Melee && x.Value > 0) ?? false))
                    ).InRandomOrder().FirstOrDefault().Value;
        }

        private static void CheckVeteranAdultBackstory(Pawn veteran)
        {
            if ((veteran?.story?.adulthood?.disallowedTraits?.Any() ?? true)
              || (veteran?.story?.adulthood?.DisabledWorkTypes?.Any() ?? true))
                veteran.story.adulthood = BackstoryDatabase.allBackstories
                    .Where(backstory =>
                    (!backstory.Value?.DisabledWorkTypes?.Any() ?? true)
                    && (!backstory.Value?.disallowedTraits?.Any() ?? true)
                    && backstory.Value.slot == BackstorySlot.Adulthood
                    && ((backstory.Value?.skillGainsResolved?.Any(x => x.Key == SkillDefOf.Shooting && x.Value > 0) ?? false)
                        || (backstory.Value?.skillGainsResolved?.Any(x => x.Key == SkillDefOf.Melee && x.Value > 0) ?? false))
                    ).InRandomOrder().FirstOrDefault().Value;
        }

        private static void AdjustVeteranSkills(Pawn veteran)
        {
            veteran.skills.skills.FirstOrDefault(skill => skill.def == SkillDefOf.Shooting).Level = Rand.Range(15, 19);
            veteran.skills.skills.FirstOrDefault(skill => skill.def == SkillDefOf.Shooting).passion = Rand.Chance(0.8f) ? Passion.Major : Passion.Minor;

            veteran.skills.skills.FirstOrDefault(skill => skill.def == SkillDefOf.Melee).Level = Rand.Range(15, 19);
            veteran.skills.skills.FirstOrDefault(skill => skill.def == SkillDefOf.Melee).passion = Rand.Chance(0.8f) ? Passion.Major : Passion.Minor;

            veteran.skills.skills.FirstOrDefault(skill => skill.def == SkillDefOf.Medicine).Level = Rand.Range(7, 15);
            veteran.skills.skills.FirstOrDefault(skill => skill.def == SkillDefOf.Medicine).passion = Rand.Chance(0.5f) ? Passion.Major : Passion.Minor;

            veteran.skills.skills.FirstOrDefault(skill => skill.def == SkillDefOf.Construction).Level = Rand.Range(7, 15);
            veteran.skills.skills.FirstOrDefault(skill => skill.def == SkillDefOf.Construction).passion = Rand.Chance(0.5f) ? Passion.Major : Passion.Minor;
        }

        private static void ConfigureVeteranHediffs(Pawn veteran)
        {
            if (ModsConfig.RoyaltyActive)
            {
                var gland = veteran.health.hediffSet.hediffs.FirstOrDefault(hediff => hediff.def == HediffDef.Named("StoneskinGland"));
                if (gland != null && Rand.Chance(0.5f)) veteran.health.hediffSet.hediffs.Remove(gland);
            }

            var joywire = veteran.health.hediffSet.hediffs.FirstOrDefault(hediff => hediff.def == HediffDef.Named("Joywire"));
            if (joywire != null) veteran.health.hediffSet.hediffs.Remove(joywire);

            foreach (var hediff in veteran.health.hediffSet.hediffs.Where(hediff => hediff.def.isBad).Reverse()) HealthUtility.Cure(hediff);
        }

        public static bool CanUseTribalBackstory(bool useTribal) => useTribal && MeditationFocusDefOf.Natural != null && (MeditationFocusDefOf.Natural?.requiredBackstoriesAny?.Any() ?? false);
    }
}
