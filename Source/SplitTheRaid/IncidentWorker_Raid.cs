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

            if (settings.modDisabled)
            {
                return;
            }

            if (parms.quest != null && !settings.affectQuestRaids)
            {
                //Log.Message("Not dealing with raids with quests");
                return;
            }

            // Generate info before hand
            bool raidInfoGenerated = false;
            for (int attempt = 1; attempt <= 5; attempt++)
            {
                if (__instance.TryGenerateRaidInfo(parms, out _))
                {
                    raidInfoGenerated = true;
                    break;
                }
                if (!settings.silenceWarnings)
                {
                    Log.Warning($"SplitTheRaid: TryGenerateRaidInfo failed, attempt {attempt}/5.");
                }
            }
            if (!raidInfoGenerated)
            {
                // If something failed, I'm not gonna deal with that
                Log.Error("SplitTheRaid: Game's TryGenerateRaidInfo failed for some reason! Skipping further logic.");
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

            if (parms is IncidentParmsPatched)
            {
                // Check random strategy for being allowed (if it's random)
                if (!settings.keepSameStrategy && !raidSettings.allowRandomPick)
                {
                    IncidentParms parmsCopy = parms;
                    var allowedNames = strategiesSettings
                        .Where(kvp => kvp.Value.allowRandomPick)
                        .Select(kvp => kvp.Key)
                        .Where(name =>
                        {
                            var def = DefDatabase<RaidStrategyDef>.GetNamed(name, false);
                            if (def == null)
                                return false;
                            return def.Worker.CanUseWith(parmsCopy, parmsCopy.pawnGroupKind); // I don't know if it's correct way
                        })
                        .ToList();

                    if (allowedNames.Count > 0)
                    {
                        string chosenName = allowedNames.RandomElement();
                        RaidStrategyDef newStrategy = DefDatabase<RaidStrategyDef>.GetNamed(chosenName, false);
                        if (newStrategy != null)
                        {
                            parms.raidStrategy = newStrategy;
                        }
                    }
                    else if (!settings.silenceWarnings)
                    {
                        Log.Warning("SplitTheRaid: No allowed random strategy found, keeping original strategy.");
                    }
                }

                // End code.
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
                    //Log.Message($"SplitTheRaid: New delay is {delayHours} hours or {delay} ticks.");
                    targetTick += delay;
                    Find.Storyteller.incidentQueue.Add(raidType, targetTick, newParms, priority);
                    //Log.Message($"SplitTheRaid: Duplicate raid scheduled at tick {targetTick}, queue size: {Find.Storyteller.incidentQueue.Count}");

                }
            }
        }
    }
}