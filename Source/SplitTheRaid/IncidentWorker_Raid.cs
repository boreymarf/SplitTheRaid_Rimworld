using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Unity.Burst.Intrinsics;
using Unity.IO.LowLevel.Unsafe;
using Verse;

namespace SplitTheRaid
{
    [HarmonyPatch(typeof(IncidentWorker_Raid), "TryExecuteWorker")]
    public static class Patch_Raid_TryExecuteWorker
    {
        [HarmonyPrefix]
        static bool Prefix(ref IncidentParms parms, IncidentWorker_Raid __instance)
        {
            var settings = SplitTheRaidMod.Settings;

            if (settings.raidHandlingMode != RaidHandlingMode.Split) {
                return true;
            }

            if (parms is IncidentParmsPatched)
            {
                Log.Message("SplitTheRaid: Duplicate raid fired, skipping");
                return true;
            }
            
            // Generate info before hand
            if (!__instance.TryGenerateRaidInfo(parms, out _))
            {
                // If something failed, I'm not gonna deal with that
                return true;
            }

            if (parms.raidStrategy.defName == "Siege" && settings.splitAffectSieges == false)
            {
                return true;
            }

            bool isAlly = parms.faction != null && parms.faction.RelationWith(Faction.OfPlayer).kind == FactionRelationKind.Ally;
            if (isAlly && settings.splitAffectAllyRaids == false)
            {
                return true;
            }

            parms.points = parms.points * settings.splitPointsMultiplier;

            IncidentParms newParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, parms.target);
            newParms = IncidentParmsPatched.ConvertParms(parms);
            newParms.forced = true;
            newParms.faction = settings.splitKeepSameFaction ? parms.faction : null;

            Log.Message($"SplitTheRaid: settings.splitKeepSameFaction {settings.splitKeepSameFaction} | newParms.faction = settings.splitKeepSameFaction ? parms.faction : null; {newParms.faction}");

            newParms.raidStrategy = settings.splitKeepSameStrategy ? parms.raidStrategy : null;

            IncidentDef raidType = isAlly ? IncidentDefOf.RaidFriendly : IncidentDefOf.RaidEnemy;
            
            int priority = 1000;

            int targetTick = Find.TickManager.TicksGame;
            for (int i = 0; i < settings.splitExtraRaids; i++)
            {
                int delayHours = settings.splitDelayHoursRange.RandomInRange;
                int delay = delayHours * GenDate.TicksPerHour;
                Log.Message($"SplitTheRaid: New delay is {delayHours} hours or {delay} ticks.");
                targetTick += delay;
                Find.Storyteller.incidentQueue.Add(raidType, targetTick, newParms, priority);
                Log.Message($"SplitTheRaid: Duplicate raid scheduled at tick {targetTick}, queue size: {Find.Storyteller.incidentQueue.Count}");
            }

            

            return true;
        }
        [HarmonyPostfix]
        static void Postfix(ref IncidentParms parms)
        {
            //Faction faction = parms.faction;
            //bool isAlly = faction != null && faction.RelationWith(Faction.OfPlayer).kind == FactionRelationKind.Ally;

            //Log.Message($"SplitTheRaid: isAlly {isAlly}");

            //if (parms is IncidentParmsExtra)
            //{
            //    Log.Message("SplitTheRaid: Duplicate raid fired, skipping");
            //    return;
            //}

            //if (isAlly && !SplitTheRaidMod.Settings.splitAffectAllyRaids)
            //{
            //    // Skip
            //    return;
            //}

            //IncidentParmsExtra splitParms = IncidentParmsExtra.ConvertParms(parms);
            //splitParms.forced = true;

            //if (isAlly)
            //{
            //    Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidFriendly, Find.TickManager.TicksGame + 1000, splitParms, 1000);
            //}
            //else
            //{
            //    Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidEnemy, Find.TickManager.TicksGame + 1000, splitParms, 1000);
            //}

            //Log.Message($"SplitTheRaid: Duplicate raid scheduled at tick {Find.TickManager.TicksGame + 1000}, queue size: {Find.Storyteller.incidentQueue.Count}");

            //return;
        }
    }
}