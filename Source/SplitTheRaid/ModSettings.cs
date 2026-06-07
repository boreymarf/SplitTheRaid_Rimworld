using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;

namespace SplitTheRaid
{
    public enum RaidHandlingMode
    {
        Disabled,
        Split,
        Constant,
    }
    public class SplitTheRaidSettings : ModSettings
    {
        // Mode
        public RaidHandlingMode raidHandlingMode = RaidHandlingMode.Split;

        // Split mode
        public bool splitAffectSieges = false;
        public bool splitAffectAllyRaids = false;
        public bool splitKeepSameFaction = false;
        public bool splitKeepSameStrategy = false;
        public float splitPointsMultiplier = 0.5f;
        public int splitMinPointsAmountToSplit = 1;
        public int splitExtraRaids = 1;
        public IntRange splitDelayHoursRange = new IntRange(24, 80);
        public int splitDelayHoursMax = 80;


        // Constant mode

        public override void ExposeData()
        {
            Scribe_Values.Look(ref splitAffectSieges, "splitAffectSieges", false);
            Scribe_Values.Look(ref splitAffectAllyRaids, "splitAffectAllyRaids", false);
            Scribe_Values.Look(ref splitKeepSameFaction, "splitKeepSameFaction", false);
            Scribe_Values.Look(ref splitKeepSameStrategy, "splitKeepSameStrategy", false);
            Scribe_Values.Look(ref splitPointsMultiplier, "splitPointsMultiplier", 0.5f);
            Scribe_Values.Look(ref splitMinPointsAmountToSplit, "splitMinPointsAmountToSplit", 75);
            Scribe_Values.Look(ref splitExtraRaids, "splitExtraRaids", 1);
            Scribe_Values.Look(ref splitDelayHoursRange.min, "splitDelayHoursRangeMin", 24);
            Scribe_Values.Look(ref splitDelayHoursRange.max, "splitDelayHoursRangeMax", 80);

            base.ExposeData();
        }
    }
}