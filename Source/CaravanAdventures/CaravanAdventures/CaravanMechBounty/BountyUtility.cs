using RimWorld;
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

            if ((veteran?.story?.Childhood?.disallowedTraits?.Any() ?? true)
                || (veteran?.story?.Childhood?.DisabledWorkTypes?.Any() ?? true
                || CanUseTribalBackstory(tribal) && (!veteran?.story?.Childhood?.spawnCategories?.Contains(tribalCatName) ?? false)))
                veteran.story.Childhood = DefDatabase<BackstoryDef>.AllDefsListForReading
                    .Where(backstory =>
                    (CanUseTribalBackstory(tribal) ? backstory.spawnCategories.Contains(tribalCatName) : true)
                    && (!backstory?.DisabledWorkTypes?.Any() ?? true)
                    && (!backstory?.disallowedTraits?.Any() ?? true)
                    && backstory?.slot == BackstorySlot.Childhood
                    && ((backstory?.skillGains?.Any(x => x.Key == SkillDefOf.Shooting && x.Value > 0) ?? false)
                        || (backstory?.skillGains?.Any(x => x.Key == SkillDefOf.Melee && x.Value > 0) ?? false))
                    ).InRandomOrder().FirstOrDefault();
        }

        private static void CheckVeteranAdultBackstory(Pawn veteran)
        {
            if ((veteran?.story?.Adulthood?.disallowedTraits?.Any() ?? true)
              || (veteran?.story?.Adulthood?.DisabledWorkTypes?.Any() ?? true))
                veteran.story.Adulthood = DefDatabase<BackstoryDef>.AllDefsListForReading
                    .Where(backstory =>
                    (!backstory?.DisabledWorkTypes?.Any() ?? true)
                    && (!backstory?.disallowedTraits?.Any() ?? true)
                    && backstory.slot == BackstorySlot.Adulthood
                    && ((backstory?.skillGains?.Any(x => x.Key == SkillDefOf.Shooting && x.Value > 0) ?? false)
                        || (backstory?.skillGains?.Any(x => x.Key == SkillDefOf.Melee && x.Value > 0) ?? false))
                    ).InRandomOrder().FirstOrDefault();
        }

        private static void AdjustVeteranSkills(Pawn veteran)
        {
            var count = 0;
            foreach (var skill in veteran.skills.skills.InRandomOrder())
            {
                skill.passion = Passion.Minor;

                switch (skill?.def?.defName)
                {
                    case "Shooting":
                    case "Melee":
                        skill.Level = Rand.Range(15, 19);
                        break;
                    case "Medicine":
                    case "Construction":
                        skill.Level = Rand.Range(7, 15);
                        break;
                    case "Artistic":
                    case "Intellectual":
                        skill.passion = Passion.None;
                        break;
                    default:
                        if (count == 0) skill.passion = Passion.None;
                        count++;
                        break;
                }
            }
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
