using System;
using System.Collections.Generic;
using System.Linq;
using CaravanAdventures.CaravanAbilities;
using CaravanAdventures.CaravanItemSelection;
using CaravanAdventures.CaravanStory;
using RimWorld;
using Verse;

namespace CaravanAdventures
{
    [StaticConstructorOnStartup]
    static class InitPatches
    {
        public static bool storyPatchesLoaded = false;
        public static bool meleeAttackPatched;
        static InitPatches()
        {
            Helper.RunSafely(FilterCombs.InitFilterSets);

            if (ModsConfig.RoyaltyActive && ModSettings.storyEnabled)
            {
                Helper.RunSafely(PatchAddNewMechanoidPawnGroupMakerDef);
                Helper.RunSafely(PatchTreeDef_AddTalkOption);
                Helper.RunSafely(PatchHumanDef_AddTalkOption);
                Helper.RunSafely(PatchRemoveRoyalTitleRequirements);
                if (ModSettings.noFreeStuff) Helper.RunSafely(PatchSacHunterItemDropStats);
                AdjustProtectiveAuraIncomingDamageMultiplier();
                storyPatchesLoaded = true;
            }
            if (!ModSettings.storyEnabled && ModsConfig.RoyaltyActive)
            {
                Helper.RunSafely(DisableFactionVillageCreation);
            }

            if (ModSettings.caravanCampEnabled) Helper.RunSafely(PatchAddCaravanDecisionsComp);
            if (ModSettings.caravanCampEnabled) Helper.RunSafely(PatchAddPsychiteTeaToCampFire);
            if (!ModSettings.caravanIncidentsEnabled) Helper.RunSafely(PatchIncidentsTo0Chance);
            if (ModSettings.buffSettlementFoodAndSilverAvailability) Helper.RunSafely(PatchIncreaseBaseWealthAndFood);
            if (ModSettings.buffShrineRewards) Helper.RunSafely(PatchAncientShrineDefs_MoreShrinesAndBetterRewards);
            if (ModSettings.increaseFireFoamPopperDetectionRange) Helper.RunSafely(PatchIncreaseFireFoamPopperDetectionRange);
            if (!ModSettings.enableSortingByPawnTitle) Helper.RunSafely(PatchRemovePawnTitleComparerDef);
            if (ModSettings.enableEngageMeleeFeature) Helper.RunSafely(PatchHumanDef_AddMeleeAttackComp);

            CompatibilityPatches.ExecuteCompatibilityPatches();
        }

        private static void PatchRemovePawnTitleComparerDef()
        {
            DLog.Message($"removing comparerdef");
            var comparerDef = DefDatabase<TransferableSorterDef>.GetNamed("CAPawnTitle");
            if (comparerDef == null) return;
            typeof(DefDatabase<TransferableSorterDef>).GetMethod("Remove", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).Invoke(null, new object[] { comparerDef });
            DLog.Message($"removed comparerdef");
        }

        private static void PatchSacHunterItemDropStats()
        {
            foreach (var kind in DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.defaultFactionType == StoryDefOf.CASacrilegHunters))
            {
                kind.biocodeWeaponChance = 1f;
                if (kind?.techHediffsRequired != null) kind.techHediffsRequired.Add(ThingDef.Named("DeathAcidifier"));
            }
        }

        private static void DisableFactionVillageCreation()
        {
            DLog.Message($"Disabling story village spawns");
            CaravanStory.StoryDefOf.CASacrilegHunters.settlementGenerationWeight = 0;
            CaravanStory.StoryDefOf.CASacrilegHunters.requiredCountAtGameStart = 0;
        }

        private static void PatchAddCaravanDecisionsComp()
        {
            if (!ModSettings.caravanCampEnabled) return;
            var caravanDef = WorldObjectDefOf.Caravan;
            if (caravanDef != null) caravanDef.comps.Add(new WorldObjectCompProperties() { compClass = typeof(CaravanImprovements.CompCaravanDecisions) });
        }

        private static void PatchAddNewMechanoidPawnGroupMakerDef()
        {
            var mechFactionDef = FactionDefOf.Mechanoid;

            var mechs = DefDatabase<PawnKindDef>.AllDefs.Where(x => x.defName.ToLower().StartsWith("mech_") && x.combatPower > 50 && x.combatPower <= 400).Select(x => new PawnGenOption() { kind = x, selectionWeight = 10f }).ToList();
            if (!mechs?.Any() ?? true)
            {
                mechs = new List<PawnGenOption>() { 
                    // todo move defs to a modextension or another def to allow mod support
                    new PawnGenOption() { kind = PawnKindDef.Named("Mech_CentipedeBlaster"), selectionWeight = 10f },
                    new PawnGenOption() { kind = PawnKindDef.Named("Mech_Lancer"), selectionWeight = 10f },
                    new PawnGenOption() { kind = PawnKindDef.Named("Mech_Scyther"), selectionWeight = 10f },
                    new PawnGenOption() { kind = PawnKindDef.Named("Mech_Pikeman"), selectionWeight = 10f }
                };
            }

            mechFactionDef.pawnGroupMakers.Add(new PawnGroupMaker()
            {
                kindDef = CaravanStory.StoryDefOf.CAMechanoidPawnGroupKindCombatMixed,
                commonality = 100,
                options = mechs
            });
        }

        private static void PatchIncreaseBaseWealthAndFood()
        {
            var bases = DefDatabase<TraderKindDef>.AllDefsListForReading.Where(def => def.defName.ToLower().StartsWith("base_"));
            foreach (var curBase in bases)
            {
                DLog.Message($"adjusting base {curBase.defName}");
                var silverGen = curBase.stockGenerators.FirstOrDefault(gen => gen.HandlesThingDef(ThingDefOf.Silver));
                if (silverGen != null) silverGen.countRange = new IntRange(silverGen.countRange.min * 3, silverGen.countRange.max * 2);

                var foodGen = curBase.stockGenerators.FirstOrDefault(gen => gen.HandlesThingDef(ThingDefOf.MealSimple));
                if (foodGen != null) foodGen.countRange = new IntRange(foodGen.countRange.min * 2, Convert.ToInt32(foodGen.countRange.max * 1.5));
            }
        }

        private static void PatchRemoveRoyalTitleRequirements()
        {
            if (!ModsConfig.RoyaltyActive || !ModSettings.removeRoyalTitleRequirements) return;
            foreach (var def in DefDatabase<RoyalTitleDef>.AllDefsListForReading)
            {
                if (ModSettings.removeOnlyAcolyteAndKnightRoyalTitleRequirements && !new[] { RoyalTitleDefOf.Knight, DefDatabase<RoyalTitleDef>.GetNamedSilentFail("Esquire"), DefDatabase<RoyalTitleDef>.GetNamedSilentFail("Acolyte") }.Contains(def)) continue;
                def.disabledJoyKinds = new List<JoyKindDef>();
                def.disabledWorkTags = WorkTags.None;
                def.requiredApparel = new List<ApparelRequirement>();
                def.bedroomRequirements = new List<RoomRequirement>();
                def.foodRequirement = default;
                def.throneRoomRequirements = new List<RoomRequirement>();
                def.requiredMinimumApparelQuality = QualityCategory.Awful;
            }
        }

        private static void PatchTreeDef_AddTalkOption()
        {
            var tree = DefDatabase<ThingDef>.GetNamed("Plant_TreeAnima");

            if (tree == null)
            {
                DLog.Message("Tree is null");
                return;
            }
            if (!tree.comps.Any(x => x is CompProperties_Talk)) tree.comps.Add(new CompProperties_Talk());
        }

        private static void PatchHumanDef_AddTalkOption()
        {
            var humanDef = DefDatabase<ThingDef>.GetNamed("Human");

            if (humanDef == null)
            {
                DLog.Message("HumanDef is null");
                return;
            }
            // todo reenable?
            var compProp = new CompProperties_Talk();
            if (!humanDef.comps.Any(x => x is CompProperties_Talk)) humanDef.comps.Add(compProp);
        }

        private static void PatchHumanDef_AddMeleeAttackComp()
        {
            var humanDef = DefDatabase<ThingDef>.GetNamed("Human");

            if (humanDef == null)
            {
                DLog.Message("HumanDef is null");
                return;
            }

            var compProp = new CompProperties_EngageMelee();
            if (!humanDef.comps.Any(x => x is CompProperties_EngageMelee)) humanDef.comps.Add(compProp);
            meleeAttackPatched = true;
        }

        private static void PatchAncientShrineDefs_MoreShrinesAndBetterRewards()
        {
            // todo ModOptions
            var scatterShrinesDef = DefDatabase<GenStepDef>.GetNamed("ScatterShrines");
            var genStep = scatterShrinesDef.genStep as GenStep_ScatterShrines;
            if (genStep != null)
            {
                genStep.countPer10kCellsRange.min *= 2;
                genStep.countPer10kCellsRange.max *= 2;
            }

            var templeContentsDef = DefDatabase<ThingSetMakerDef>.GetNamed("MapGen_AncientTempleContents");
            var root = templeContentsDef.root as ThingSetMaker_Sum;
            if (root != null)
            {
                var option = root.options[1];
                option.chance = 0.8f;

                // todo add new option cat to add another group of special tiems 

                var newOption = option;
                var thingSetMaker_Count = newOption.thingSetMaker as ThingSetMaker_Count;
                // maybe reserve this for special rewards?
                thingSetMaker_Count.fixedParams.filter.Allows(ThingDefOf.VanometricPowerCell);
                thingSetMaker_Count.fixedParams.filter.Allows(ThingDefOf.InfiniteChemreactor);
                root.options.Add(newOption);

                {
                    //var newItems = new ThingDef[]
                    //{
                    //    DefDatabase<ThingDef>.GetNamed("MechSerumHealer"),
                    //    DefDatabase<ThingDef>.GetNamed("MechSerumResurrector"),
                    //};

                    //var newFilter = new ThingFilter();
                    //foreach (var item in newItems)
                    //{
                    //    newFilter.Allows(item);
                    //}

                    //root.options.Add(new ThingSetMaker_Sum.Option
                    //{
                    //    chance = 1,
                    //    thingSetMaker = new ThingSetMaker_Count()
                    //    {
                    //        fixedParams = new ThingSetMakerParams()
                    //        {
                    //            filter = newFilter,
                    //        }
                    //    }
                    //});
                }
            }
        }

        private static void PatchAddPsychiteTeaToCampFire()
        {
            var campFire = DefDatabase<ThingDef>.GetNamed(CaravanCamp.CampDefOf.CACampfireRoast.defName);
            if (campFire == null) return;
            var recipes = new[] { DefDatabase<RecipeDef>.GetNamed("Make_PsychiteTea"), DefDatabase<RecipeDef>.GetNamed("Make_PsychiteTeaBulk") };
            if (recipes.All(x => x == null)) return;
            foreach (var recipe in recipes) if (recipe != null) campFire.recipes.Add(recipe);
        }

        private static void PatchIncidentsTo0Chance()
        {
            var incidents = DefDatabase<IncidentDef>.AllDefsListForReading.Where(x => x.defName.StartsWith("CACaravan"));
            foreach (var incident in incidents)
            {
                incident.allowedBiomes = new List<BiomeDef>();
                incident.baseChance = 0;
            }
        }

        private static void PatchIncreaseFireFoamPopperDetectionRange()
        {
            var def = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(x => x.defName.StartsWith("FirefoamPopper"));
            if (def == null) return;
            var triggerComp = (def.comps.FirstOrDefault(x => x is CompProperties_ProximityFuse)) as CompProperties_ProximityFuse;
            if (triggerComp == null) return;
            triggerComp.radius = 5;
        }

        public static void AdjustProtectiveAuraIncomingDamageMultiplier()
        {
            Helper.RunSafely(() =>
            {
                var ancientGift = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(x => x == CaravanAbilities.AbilityDefOf.CAAncientProtectiveAura);
                var stage = ancientGift?.stages?.FirstOrDefault();
                var statFactor = stage?.statFactors?.FirstOrDefault(x => x?.stat == StatDefOf.IncomingDamageFactor);
                if (statFactor == null) return;
                statFactor.value = ModSettings.ancientProtectiveAuraDamageReduction;

                ancientGift = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(x => x == CaravanAbilities.AbilityDefOf.CAAncientProtectiveAuraLinked);
                stage = ancientGift?.stages?.FirstOrDefault();
                statFactor = stage?.statFactors?.FirstOrDefault(x => x?.stat == StatDefOf.IncomingDamageFactor);
                if (statFactor == null) return;
                statFactor.value = ModSettings.ancientProtectiveAuraDamageReduction;
            });
        }
    }
}
