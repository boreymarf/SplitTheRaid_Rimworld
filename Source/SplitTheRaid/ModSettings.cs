using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace SplitTheRaid
{
    public class SplitTheRaidSettings : ModSettings
    {
        public bool modDisabled = false;
        public bool affectAllyRaids = false;
        public bool affectQuestRaids = false;
        public bool keepSameFaction = true;
        public bool keepSameStrategy = true;
        public bool keepSameArrivalMode = true;
        public bool keepSameSpawnLocation = false;
        public float pointsMultiplier = 0.5f;
        public int minPointsAmountToSplit = 75;
        public int extraRaids = 1;
        public IntRange delayHoursRange = new IntRange(24, 24);
        public bool silenceWarnings = false;

        public Dictionary<string, RaidStrategySettings> strategySettings = new Dictionary<string, RaidStrategySettings>();
        private List<string> strategyKeysBuffer = new List<string>();
        private List<RaidStrategySettings> strategyValuesBuffer = new List<RaidStrategySettings>();

        public override void ExposeData()
        {
            Scribe_Values.Look(ref modDisabled, "SplitTheRaid.modDisabled", false);
            Scribe_Values.Look(ref affectAllyRaids, "SplitTheRaid.affectAllyRaids", false);
            Scribe_Values.Look(ref affectQuestRaids, "SplitTheRaid.affectQuestRaids", false);
            Scribe_Values.Look(ref keepSameFaction, "SplitTheRaid.keepSameFaction", true);
            Scribe_Values.Look(ref keepSameStrategy, "SplitTheRaid.keepSameStrategy", true);
            Scribe_Values.Look(ref keepSameArrivalMode, "SplitTheRaid.keepSameArrivalMode", true);
            Scribe_Values.Look(ref keepSameSpawnLocation, "SplitTheRaid.keepSameSpawnLocation", false);
            Scribe_Values.Look(ref pointsMultiplier, "SplitTheRaid.pointsMultiplier", 0.5f);
            Scribe_Values.Look(ref minPointsAmountToSplit, "SplitTheRaid.minPointsAmountToSplit", 75);
            Scribe_Values.Look(ref extraRaids, "SplitTheRaid.extraRaids", 1);
            Scribe_Values.Look(ref delayHoursRange.min, "SplitTheRaid.delayHoursRangeMin", 24);
            Scribe_Values.Look(ref delayHoursRange.max, "SplitTheRaid.delayHoursRangeMax", 24);
            Scribe_Values.Look(ref silenceWarnings, "SplitTheRaid.silenceWarnings", false);

            Scribe_Collections.Look(
                ref strategySettings,
                "SplitTheRaid.strategySettings",
                LookMode.Value,
                LookMode.Deep,
                ref strategyKeysBuffer,
                ref strategyValuesBuffer
            );

            base.ExposeData();

            EnsureStrategyDefaults();
        }

        // I'm having troubles with defaults, but works for now
        public void EnsureStrategyDefaults()
        {
            foreach (var def in DefDatabase<RaidStrategyDef>.AllDefsListForReading)
            {
                if (!strategySettings.ContainsKey(def.defName))
                {
                    strategySettings[def.defName] = GetDefaultStrategySettings(def.defName);
                }
            }
        }

        public static RaidStrategySettings GetDefaultStrategySettings(string defName)
        {
            switch (defName)
            {
                case "ImmediateAttack":
                case "ImmediateAttackFriendly":
                case "ImmediateAttackSmart":
                case "EmergeFromWater":
                case "ImmediateAttackBreaching":
                case "ImmediateAttackBreachingSmart":
                case "ShamblerAssault":
                case "StageThenAttack":
                case "ImmediateAttackSappers":
                    return new RaidStrategySettings
                    {
                        allowPointsModification = true,
                        allowRandomPick = true,
                        allowExtraRaids = true
                    };
                default:
                    return new RaidStrategySettings();
            }
        }
    }
}