using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SplitTheRaid
{
    [HarmonyPatch(typeof(IncidentWorker_Raid), "TryExecuteWorker")]
    public static class Patch_Raid_TryExecuteWorker
    {
        [HarmonyPrefix]
        static void Prefix(ref IncidentParms parms, IncidentWorker_Raid __instance)
        {
            var settings = SplitTheRaidMod.Settings;
            var strategiesSettings = settings.strategySettings;

            // Generate info before hand
            if (!__instance.TryGenerateRaidInfo(parms, out _))
            {
                // If something failed, I'm not gonna deal with that
                Log.Error("SplitTheRaid: faid to TryGenerateRaidInfo.");
                return;
            }

            if (parms.raidStrategy == null)
            {
                Log.Error("SplitTheRaid: raidStrategy is null somehow, skipping raid.");
                return;
            }

            if (!strategiesSettings.TryGetValue(parms.raidStrategy.defName, out var raidSettings))
            {
                Log.Error($"SplitTheRaid: Unknown strategy {parms.raidStrategy.defName}, cannot find a strategy setting for it!");
                return;
            }

            if (settings.modDisabled)
            {
                return;
            }

            if (parms is IncidentParmsPatched)
            {
                // Check random strategy for being allowed (if it's random)
                if (!settings.keepSameStrategy && !raidSettings.allowRandomPick)
                {
                    var allowedNames = strategiesSettings
                        .Where(kvp => kvp.Value.allowRandomPick)
                        .Select(kvp => kvp.Key)
                        .Where(name => DefDatabase<RaidStrategyDef>.GetNamed(name, false) != null)
                        .ToList();

                    string chosenName;
                    if (allowedNames.Count > 0)
                    {
                        chosenName = allowedNames.RandomElement();
                    }
                    else
                    {
                        Log.Warning("SplitTheRaid: No strategy with allowRandomPick. Using ImmediateAttack.");
                        chosenName = "ImmediateAttack";
                    }

                    RaidStrategyDef newStrategy = DefDatabase<RaidStrategyDef>.GetNamed(chosenName);
                    if (newStrategy != null)
                    {
                        parms.raidStrategy = newStrategy;
                    }
                    else
                    {
                        Log.Error($"SplitTheRaid: Failed to find RaidStrategyDef named '{chosenName}'");
                    }
                }

                // Stop the code here
                return;
            }

            bool isAlly = parms.faction != null && parms.faction.RelationWith(Faction.OfPlayer).kind == FactionRelationKind.Ally;
            if (isAlly && settings.affectAllyRaids == false)
            {
                return;
            }

            if (raidSettings.allowPointsModification)
            {
                parms.points *= settings.pointsMultiplier;
            }

            if (raidSettings.allowExtraRaids)
            {
                IncidentParms newParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, parms.target);
                newParms = IncidentParmsPatched.ConvertParms(parms);
                newParms.forced = true;
                newParms.faction = settings.keepSameFaction ? parms.faction : null;
                newParms.raidStrategy = settings.keepSameStrategy ? parms.raidStrategy : null;
                newParms.spawnCenter = settings.keepSameSpawnLocation ? parms.spawnCenter : IntVec3.Invalid;

                IncidentDef raidType = isAlly ? IncidentDefOf.RaidFriendly : IncidentDefOf.RaidEnemy;

                int priority = 1000; // dunno what priority does

                int targetTick = Find.TickManager.TicksGame;
                for (int i = 0; i < settings.extraRaids; i++)
                {
                    int delayHours = settings.delayHoursRange.RandomInRange;
                    int delay = delayHours * GenDate.TicksPerHour;
                    Log.Message($"SplitTheRaid: New delay is {delayHours} hours or {delay} ticks.");
                    targetTick += delay;
                    Find.Storyteller.incidentQueue.Add(raidType, targetTick, newParms, priority);
                    Log.Message($"SplitTheRaid: Duplicate raid scheduled at tick {targetTick}, queue size: {Find.Storyteller.incidentQueue.Count}");
                }
            }

        }
    }
}