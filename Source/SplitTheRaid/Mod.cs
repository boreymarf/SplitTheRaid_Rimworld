using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using static SplitTheRaid.UIHelper;

namespace SplitTheRaid
{
    public class SplitTheRaidMod : Mod
    {
        public override string SettingsCategory() => "Split The Raid";
        public static SplitTheRaidSettings Settings;

        private readonly UIHelper.IntRangeExpanded _splitDelayHoursRange;

        List<QueuedIncident> customQueuedRaids = new List<QueuedIncident>();

        // this sucks, but whatever
        private string splitPointsMultiplierBuffer;
        private string splitMinPointsAmountToSplitBuffer;
        private string splitExtraRaidsBuffer;

        public SplitTheRaidMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<SplitTheRaidSettings>();
            _splitDelayHoursRange = new UIHelper.IntRangeExpanded(0, 200, 1);
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Color originalColor = GUI.color;

            const float tabHeight = 28f;
            const float tabWidth = 120f;
            Rect tabsRect = new Rect(inRect.x, inRect.y, inRect.width, tabHeight);
            Rect contentRect = new Rect(inRect.x, inRect.y + tabHeight + 8f, inRect.width, inRect.height - tabHeight - 8f);

            Rect rect = new Rect(tabsRect.x + tabWidth * 0, tabsRect.y, tabWidth, tabHeight);
            UIHelper.DrawTabButton(rect, ref Settings.raidHandlingMode, RaidHandlingMode.Disabled, "Disabled", "SplitTheRaid.TabDisabledTooltip");

            rect = new Rect(tabsRect.x + tabWidth * 1, tabsRect.y, tabWidth, tabHeight);
            UIHelper.DrawTabButton(rect, ref Settings.raidHandlingMode, RaidHandlingMode.Split, "Split", "SplitTheRaid.TabSplitTooltip");

            rect = new Rect(tabsRect.x + tabWidth * 2, tabsRect.y, tabWidth, tabHeight);
            UIHelper.DrawTabButton(rect, ref Settings.raidHandlingMode, RaidHandlingMode.Constant, "Constant", "SplitTheRaid.TabConstantTooltip");

            Widgets.DrawLineHorizontal(inRect.x, tabsRect.yMax, inRect.width, Color.gray);

            // Contents
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(contentRect);

            switch (Settings.raidHandlingMode)
            {
                case RaidHandlingMode.Disabled:
                    listing.Label("SplitTheRaid.DisabledMode".Translate());
                    break;
                case RaidHandlingMode.Split:
                    listing.Label("SplitTheRaid.SplitModeSettings".Translate());

                    listing.Gap(12f);

                    listing.CheckboxLabeled("SplitTheRaid.SplitAffectSieges".Translate(), ref Settings.splitAffectSieges);
                    listing.CheckboxLabeled("SplitTheRaid.SplitAffectAllyRaids".Translate(), ref Settings.splitAffectAllyRaids);
                    listing.CheckboxLabeled("SplitTheRaid.SplitKeepSameFaction".Translate(), ref Settings.splitKeepSameFaction);
                    listing.CheckboxLabeled("SplitTheRaid.SplitKeepSameStrategy".Translate(), ref Settings.splitKeepSameStrategy);

                    listing.Gap();

                    // Points multiplier
                    UIHelper.FloatInput(listing, ref Settings.splitPointsMultiplier, ref splitPointsMultiplierBuffer, "SplitTheRaid.Settings.SplitPointsMultiplier".Translate(), 0, 100);
                    float newValue = listing.Slider(Settings.splitPointsMultiplier, 0.1f, 5f);
                    Settings.splitPointsMultiplier = RoundToStep(newValue, 0.01f);

                    // Min amount of points for split
                    UIHelper.IntInput(listing, ref Settings.splitMinPointsAmountToSplit, ref splitMinPointsAmountToSplitBuffer, "SplitTheRaid.SplitMinPointsAmountToSplit".Translate(), 0, 100000);
                    Settings.splitMinPointsAmountToSplit = Mathf.RoundToInt(listing.Slider((float)Settings.splitMinPointsAmountToSplit, 0f, 10000f));

                    // Extra raids amount
                    UIHelper.IntInput(listing, ref Settings.splitExtraRaids, ref splitExtraRaidsBuffer, "SplitTheRaid.SplitExtraRaids".Translate(), 0, 100);
                    Settings.splitExtraRaids = Mathf.RoundToInt(listing.Slider((float)Settings.splitExtraRaids, 0f, 5f));

                    // Hours between raids
                    listing.Label("SplitTheRaid.SplitHoursBetweenRaids".Translate());
                    _splitDelayHoursRange.Draw(listing, ref Settings.splitDelayHoursRange);




                    break;
                case RaidHandlingMode.Constant:
                    listing.Label("SplitTheRaid.ConstantModeSettings".Translate());
                    // ...
                    break;
            }

            listing.Gap(12f);
            listing.GapLine(4);
            listing.Gap(12f);

            listing.Label("Debug menu.");


            if (Find.Storyteller != null)
            {

                IncidentHelper.UpdateCustomRaidList();

                bool clicked = listing.ButtonTextLabeled($"Current amount of delayed raids: {IncidentHelper.CustomQueuedRaids.Count}", "Clear");
                if (clicked)
                {
                    IncidentHelper.ClearAllCustomRaids();
                }
            }
            else
            {
                listing.Label("The save is not running :3c");
            }

            listing.End();
        }
    }
}