using Verse;

namespace SplitTheRaid
{
    public class RaidStrategySettings : IExposable
    {
        public bool allowExtraRaids = false;
        public bool allowPointsModification = false;
        public bool allowRandomPick = false;

        public void ExposeData()
        {
            Scribe_Values.Look(ref allowExtraRaids, "allowExtraRaids", false);
            Scribe_Values.Look(ref allowPointsModification, "allowPointsModification", false);
            Scribe_Values.Look(ref allowRandomPick, "allowRandomPick", false);
        }
    }
}