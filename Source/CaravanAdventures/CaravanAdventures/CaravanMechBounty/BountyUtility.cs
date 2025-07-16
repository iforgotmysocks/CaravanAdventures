using RimWorld;
using System.Linq;
using Verse;

namespace CaravanAdventures.CaravanMechBounty
{
    class BountyUtility
    {
        public static Pawn GenerateVeteran(TraitDef selPersonality = null, TraitDef selSkill = null, bool tribal = false)
        {
            var veteran = PrepareVeteranPawn(selPersonality, selSkill, tribal);
            if (veteran == null) return null;
            CheckVeteranChildhoodBackstory(veteran, tribal);
            CheckVeteranAdultBackstory(veteran);
            AdjustVeteranSkills(veteran, tribal);
            ConfigureVeteranHediffs(veteran);
            veteran.Notify_DisabledWorkTypesChanged();
            return veteran;
        }

        private static Pawn PrepareVeteranPawn(TraitDef selPersonality, TraitDef selSkill, bool tribal)
        {
            var genPawnRequest = new PawnGenerationRequest(ModSettings.storyEnabled ? CaravanStory.StoryDefOf.CASacrilegHunters_ExperiencedHunter : PawnKindDefOf.SpaceRefugee, Faction.OfPlayer)
            {
                MustBeCapableOfViolence = true,
                AllowAddictions = false,
                FixedIdeo = ModsConfig.IdeologyActive ? Faction.OfPlayerSilentFail?.ideos?.PrimaryIdeo : default,
                CanGeneratePawnRelations = false,
            };
            var veteran = PawnGenerator.GeneratePawn(genPawnRequest);
            if (veteran == null) return null;
            if (veteran.ageTracker.AgeBiologicalYears < 18 || veteran.ageTracker.AgeBiologicalYears > 30)
            {
                var newAge = veteran.ageTracker.AgeBiologicalTicks = 60000 * 60 * Rand.Range(20, 30);
                veteran.ageTracker.AgeBiologicalTicks = newAge;
                if (veteran.ageTracker.AgeChronologicalTicks < newAge) veteran.ageTracker.AgeChronologicalTicks = newAge;
            }
            foreach (var trait in veteran.story.traits.allTraits.Reverse<Trait>()) veteran.story.traits.allTraits.Remove(trait);
            if (DefDatabase<TraitDef>.GetNamedSilentFail("Tough") != null) veteran.story.traits.GainTrait(new Trait(DefDatabase<TraitDef>.GetNamedSilentFail("Tough")));

            var beautyChance = tribal ? 0.5f : 0.2f;
            if (DefDatabase<TraitDef>.GetNamedSilentFail("Beauty") != null && Rand.Chance(beautyChance)) veteran.story.traits.GainTrait(new Trait(DefDatabase<TraitDef>.GetNamedSilentFail("Beauty"), 2));
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
                    && ((backstory?.skillGains?.Any(x => x.skill == SkillDefOf.Shooting && x.amount > 0) ?? false)
                        || (backstory?.skillGains?.Any(x => x.skill == SkillDefOf.Melee && x.amount > 0) ?? false))
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
                    && ((backstory?.skillGains?.Any(x => x.skill == SkillDefOf.Shooting && x.amount > 0) ?? false)
                        || (backstory?.skillGains?.Any(x => x.skill == SkillDefOf.Melee && x.amount > 0) ?? false))
                    ).InRandomOrder().FirstOrDefault();
        }

        private static void AdjustVeteranSkills(Pawn veteran, bool tribal)
        {
            var majorCombatPassion = Rand.Chance(0.3f);
            var majorOther = Rand.Chance(0.3f);

            var importantSkills = new[] { "Medicine", "Construction", "Plants", "Intellectual" };
            var combatSkills = new[] { "Shooting", "Melee" };

            if (ModSettings.useEqualMinorPassionsForVeterans || tribal && !majorCombatPassion && !majorOther) AssignEqualMinorPassions(veteran, tribal, importantSkills, combatSkills);
            else AssignWithMajorPassions(veteran, ref majorCombatPassion, ref majorOther, tribal, importantSkills, combatSkills);
        }

        private static void AssignEqualMinorPassions(Pawn veteran, bool tribal, string[] importantSkills, string[] combatSkills)
        {
            SetBaseLevelAndMinorPassions(veteran, importantSkills, combatSkills);
            if (!tribal) return;
            var isCombatPassion = Rand.Chance(0.75f);
            if (isCombatPassion)
            {
                var flag = Rand.Chance(0.5f);
                SkillRecord selectedSkill = null;
                if (flag) selectedSkill = veteran.skills.skills.FirstOrDefault(x => x.def.defName == "Shooting");
                else selectedSkill = veteran.skills.skills.FirstOrDefault(x => x.def.defName == "Melee");
                if (selectedSkill != null) selectedSkill.passion = Passion.Major;
            }
            else
            {
                var skill = veteran.skills.skills.Where(x => !combatSkills.Contains(x.def.defName)).RandomElement();
                if (skill != null) skill.passion = Passion.Major;
            }
        }

        private static void AssignWithMajorPassions(Pawn veteran, ref bool majorCombatPassion, ref bool majorOther, bool tribal, string[] importantSkills, string[] combatSkills)
        {
            SetBaseLevelAndMinorPassions(veteran, importantSkills, combatSkills);
            var passionsToTake = 0;
            if (majorCombatPassion)
            {
                var flag = Rand.Chance(0.5f);
                SkillRecord selectedSkill = null;
                if (flag) selectedSkill = veteran.skills.skills.FirstOrDefault(x => x.def.defName == "Shooting");
                else selectedSkill = veteran.skills.skills.FirstOrDefault(x => x.def.defName == "Melee");
                if (selectedSkill != null)
                {
                    selectedSkill.passion = Passion.Major;
                    passionsToTake++;
                }
            }
            if (majorOther)
            {
                var skill = veteran.skills.skills.Where(x => !combatSkills.Contains(x.def.defName)).RandomElement();
                if (skill != null)
                {
                    skill.passion = Passion.Major;
                    passionsToTake++;
                }
            }
            if (tribal) passionsToTake--;
            if (passionsToTake <= 0) return;
            for (int i = 0; i < passionsToTake; i++)
            {
                var skill = veteran.skills.skills.Where(x => !combatSkills.Contains(x.def.defName) && !importantSkills.Contains(x.def.defName) && x.passion != Passion.Major).RandomElement();
                if (skill == null) continue;
                skill.passion = Passion.None;
            }
        }

        private static void SetBaseLevelAndMinorPassions(Pawn veteran, string[] importantSkills, string[] combatSkills)
        {
            foreach (var skill in veteran.skills.skills.InRandomOrder())
            {
                skill.passion = Passion.Minor;
                if (combatSkills.Contains(skill.def.defName)) skill.Level = Rand.Range(15, 19);
                if (importantSkills.Contains(skill.def.defName)) skill.Level = Rand.Range(7, 15);
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

            var deathAcidifier = veteran.health.hediffSet.hediffs.FirstOrDefault(hediff => hediff.def == HediffDef.Named("DeathAcidifier"));
            if (deathAcidifier != null) veteran.health.hediffSet.hediffs.Remove(deathAcidifier);

            foreach (var hediff in veteran.health.hediffSet.hediffs.Where(hediff => hediff.def.isBad).Reverse()) HealthUtility.Cure(hediff);
        }

        public static bool CanUseTribalBackstory(bool useTribal) => useTribal && MeditationFocusDefOf.Natural != null && (MeditationFocusDefOf.Natural?.requiredBackstoriesAny?.Any() ?? false);
    }
}
