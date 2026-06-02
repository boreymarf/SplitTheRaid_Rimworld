using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
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

        private string buffer_1;
        private string buffer_2;
        private string buffer_3;

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

            // "Disabled" button
            Rect rectBasic = new Rect(tabsRect.x + tabWidth * 0, tabsRect.y, tabWidth, tabHeight);
            if (Settings.raidHandlingMode == RaidHandlingMode.Disabled)
                GUI.color = Color.green;
            if (Widgets.ButtonText(rectBasic, "Disabled"))
                Settings.raidHandlingMode = RaidHandlingMode.Disabled;
            GUI.color = originalColor;
            TooltipHandler.TipRegion(rectBasic, "The mod is disabled.");

            // "Split" button
            Rect rectRaids = new Rect(tabsRect.x + tabWidth * 1, tabsRect.y, tabWidth, tabHeight);
            if (Settings.raidHandlingMode == RaidHandlingMode.Split)
                GUI.color = Color.green;
            if (Widgets.ButtonText(rectRaids, "Split"))
                Settings.raidHandlingMode = RaidHandlingMode.Split;
            GUI.color = originalColor;
            TooltipHandler.TipRegion(rectRaids, "Description");

            // "Constant" button
            Rect rectOther = new Rect(tabsRect.x + tabWidth * 2, tabsRect.y, tabWidth, tabHeight);
            if (Settings.raidHandlingMode == RaidHandlingMode.Constant)
                GUI.color = Color.green;
            if (Widgets.ButtonText(rectOther, "Constant"))
                Settings.raidHandlingMode = RaidHandlingMode.Constant;
            GUI.color = originalColor;
            TooltipHandler.TipRegion(rectOther, "Description");

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

                    listing.Gap();

                    // Points multiplier
                    UIHelper.FloatInput(listing, ref Settings.splitPointsMultiplier, ref buffer_1, "SplitTheRaid.Settings.SplitPointsMultiplier".Translate(), 0, 100);
                    float newValue = listing.Slider(Settings.splitPointsMultiplier, 0.1f, 5f);
                    Settings.splitPointsMultiplier = RoundToStep(newValue, 0.01f);

                    // Min amount of points for split
                    UIHelper.IntInput(listing, ref Settings.splitMinPointsAmountToSplit, ref buffer_2, "SplitTheRaid.SplitMinPointsAmountToSplit".Translate(), 0, 100000);
                    Settings.splitMinPointsAmountToSplit = Mathf.RoundToInt(listing.Slider((float)Settings.splitMinPointsAmountToSplit, 0f, 10000f));

                    // Extra raids amount
                    UIHelper.IntInput(listing, ref Settings.splitExtraRaids, ref buffer_3, "SplitTheRaid.SplitExtraRaids".Translate(), 0, 100);
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

            // listing.Label("Debug menu.");

            listing.End();
        }
    }
}