using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;

namespace SplitTheRaid
{
    internal static class IncidentHelper
    {
        private static List<QueuedIncident> _customQueuedRaids = new List<QueuedIncident>();

        public static List<QueuedIncident> CustomQueuedRaids => _customQueuedRaids;
        public static bool HasCustomRaids => _customQueuedRaids.Count > 0;

        public static void UpdateCustomRaidList()
        {
            _customQueuedRaids.Clear();

            if (Find.Storyteller?.incidentQueue == null)
                return;

            foreach (QueuedIncident queued in Find.Storyteller.incidentQueue)
            {
                if (queued.FiringIncident.parms is IncidentParmsPatched)
                {
                    _customQueuedRaids.Add(queued);
                }
            }
        }

        public static void ClearAllCustomRaids()
        {
            if (Find.Storyteller?.incidentQueue == null)
                return;

            var queue = Find.Storyteller.incidentQueue;
            var listField = typeof(IncidentQueue).GetField("queuedIncidents",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (listField == null)
            {
                Log.Error("SplitTheRaid: Failed to get queuedIncidents field");
                return;
            }

            var list = (List<QueuedIncident>)listField.GetValue(queue);
            if (list == null)
                return;

            list.RemoveAll(q => q.FiringIncident.parms is IncidentParmsPatched);
            UpdateCustomRaidList();
        }
    }
}