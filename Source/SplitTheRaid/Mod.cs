using System.Collections.Generic;
using System.Security.Cryptography;
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

        private readonly UIHelper.IntRangeExpanded _delayHoursRange;
        private string pointsMultiplierBuffer;
        private string minPointsAmountToSplitBuffer;
        private string extraRaidsBuffer;

        public SplitTheRaidMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<SplitTheRaidSettings>();
            _delayHoursRange = new UIHelper.IntRangeExpanded(0, 200, 1);
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            // Checkboxes
            listing.CheckboxLabeled("SplitTheRaid.ModDisabled".Translate(), ref Settings.modDisabled);
            listing.CheckboxLabeled("SplitTheRaid.AffectAllyRaids".Translate(), ref Settings.affectAllyRaids);
            listing.CheckboxLabeled("SplitTheRaid.KeepSameFaction".Translate(), ref Settings.keepSameFaction);
            listing.CheckboxLabeled("SplitTheRaid.KeepSameStrategy".Translate(), ref Settings.keepSameStrategy);
            listing.CheckboxLabeled("SplitTheRaid.KeepSameSpawnLocation".Translate(), ref Settings.keepSameSpawnLocation);

            listing.Gap(12f);

            if (listing.ButtonText("SplitTheRaid.OpenStrategyConfig".Translate()))
            {
                Find.WindowStack.Add(new Dialog_StrategySettings());
            }

            listing.Gap(12f);

            // Inputs
            UIHelper.FloatInput(listing, ref Settings.pointsMultiplier, ref pointsMultiplierBuffer,
                "SplitTheRaid.PointsMultiplier".Translate(), 0, 100);
            float newValue = listing.Slider(Settings.pointsMultiplier, 0.1f, 5f);
            Settings.pointsMultiplier = RoundToStep(newValue, 0.01f);

            UIHelper.IntInput(listing, ref Settings.minPointsAmountToSplit, ref minPointsAmountToSplitBuffer,
                "SplitTheRaid.MinPointsAmountToSplit".Translate(), 0, 100000);
            Settings.minPointsAmountToSplit = Mathf.RoundToInt(listing.Slider((float)Settings.minPointsAmountToSplit, 0f, 10000f));

            UIHelper.IntInput(listing, ref Settings.extraRaids, ref extraRaidsBuffer,
                "SplitTheRaid.ExtraRaids".Translate(), 0, 100);
            Settings.extraRaids = Mathf.RoundToInt(listing.Slider((float)Settings.extraRaids, 0f, 5f));

            listing.Label("SplitTheRaid.HoursBetweenRaids".Translate());
            _delayHoursRange.Draw(listing, ref Settings.delayHoursRange);

            listing.Gap(12f);
            listing.GapLine(4);
            listing.Gap(12f);

            // Debug menu
            listing.Label("Debug menu.");
            if (Find.Storyteller != null)
            {
                IncidentHelper.UpdateCustomRaidList();
                bool clicked = listing.ButtonTextLabeled("SplitTheRaid.DelayedRaidsAmount".Translate(IncidentHelper.CustomQueuedRaids.Count), "SplitTheRaid.DelayedRaidsClear".Translate());
                if (clicked)
                    IncidentHelper.ClearAllCustomRaids();
            }
            else
            {
                listing.Label("SplitTheRaid.SaveNotRunnning".Translate());
            }

            listing.End();
        }
    }

    public class Dialog_StrategySettings : Window
    {
        private Vector2 scrollPosition = Vector2.zero;

        // Factional units for columns
        private const float col0widthFr = 4f;
        private const float col1widthFr = 1f;
        private const float col2widthFr = 1f;
        private const float col3widthFr = 1f;
        private const float totalFr = col0widthFr + col1widthFr + col2widthFr + col3widthFr;

        // Reserved heights
        private const float descriptionHeight = 30f;
        private const float headerHeight = 24f;
        private const float rowHeight = 30f;
        private const float tableBodyHeight = 340f;

        private const float widthScrollbarOffset = 16f;

        private Color headerColor = new Color(0.2f, 0.2f, 0.2f);

        public Dialog_StrategySettings()
        {
            //optionalTitle = "Strategy Configuration";
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            draggable = false;
            resizeable = false;
            absorbInputAroundWindow = true;
            forcePause = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            // This somehow fixes a bug with a strategy table not showing strategies
            // though I don't like to check settings every frame
            SplitTheRaidMod.Settings.EnsureStrategyDefaults();

            // Table description
            Rect descriptionRect = new Rect(inRect.x, inRect.y, inRect.width, descriptionHeight);
            Widgets.Label(descriptionRect, "SplitTheRaid.TableDescription".Translate());

            // Change all text of next labels to be middle left

            // Table header
            Rect headerRect = new Rect(inRect.x, inRect.y + descriptionRect.height, inRect.width - widthScrollbarOffset, headerHeight);
            Widgets.DrawBoxSolid(headerRect, headerColor);

            float col0Width = headerRect.width * (col0widthFr / totalFr);
            float col1Width = headerRect.width * (col1widthFr / totalFr);
            float col2Width = headerRect.width * (col2widthFr / totalFr);
            float col3Width = headerRect.width * (col3widthFr / totalFr);

            Widgets.Label(new Rect(inRect.x, headerRect.y, col0Width, headerHeight), "SplitTheRaid.StrategyHeader".Translate());
            Widgets.Label(new Rect(inRect.x + col0Width, headerRect.y, col1Width, headerHeight), "SplitTheRaid.ExtraHeader".Translate());
            Widgets.Label(new Rect(inRect.x + col0Width + col1Width, headerRect.y, col2Width, headerHeight), "SplitTheRaid.PointsHeader".Translate());
            Widgets.Label(new Rect(inRect.x + col0Width + col1Width + col2Width, headerRect.y, col3Width, headerHeight), "SplitTheRaid.RandomHeader".Translate());

            TooltipHandler.TipRegion(new Rect(0f, headerRect.y, col0Width, headerHeight), "SplitTheRaid.ColumnStrategyTooltip".Translate());
            TooltipHandler.TipRegion(new Rect(col0Width, headerRect.y, col1Width, headerHeight), "SplitTheRaid.CheckExtraTooltip".Translate());
            TooltipHandler.TipRegion(new Rect(col0Width + col1Width, headerRect.y, col2Width, headerHeight), "SplitTheRaid.CheckPointsTooltip".Translate());
            TooltipHandler.TipRegion(new Rect(col0Width + col1Width + col2Width, headerRect.y, col3Width, headerHeight), "SplitTheRaid.CheckRandomTooltip".Translate());

            var settings = SplitTheRaidMod.Settings.strategySettings;

            Rect scrollVisibleRect = new Rect(inRect.x, inRect.y + descriptionRect.height + headerHeight, inRect.width, tableBodyHeight);
            float viewHeight = settings.Count * rowHeight;
            Rect viewRect = new Rect(0f, 0f, inRect.width - widthScrollbarOffset, viewHeight);
            Widgets.BeginScrollView(scrollVisibleRect, ref scrollPosition, viewRect);

            // Used for scrolling and position of elements inside the scroll view
            float scrollView = 0f;

            int i = 0;
            foreach (KeyValuePair<string, RaidStrategySettings> kvp in settings)
            {
                string key = kvp.Key;
                RaidStrategySettings raidSettings = kvp.Value;

                bool strategyExists = DefDatabase<RaidStrategyDef>.GetNamed(key, false) != null;

                Rect rowRect = new Rect(0f, scrollView, inRect.width - widthScrollbarOffset, rowHeight);
                if (i % 2 == 1)
                    Widgets.DrawBoxSolid(rowRect, new Color(0.22f, 0.22f, 0.22f));

                // Strategy def name
                Text.Anchor = TextAnchor.MiddleLeft;

                Color originalColor = GUI.color;
                if (!strategyExists)
                    GUI.color = Color.gray;

                Rect labelRect = new Rect(4f, scrollView, col0Width - 8f, rowHeight);
                Widgets.Label(labelRect, key);
                GUI.color = originalColor;
                Text.Anchor = TextAnchor.UpperLeft;

                if (!strategyExists)
                {
                    TooltipHandler.TipRegion(labelRect, "SplitTheRaid.StrategyMissingTooltip".Translate());
                }

                // Checkboxes
                Rect extraRect = new Rect(col0Width + (col1Width - 24f) / 2f, scrollView + 2f, 24f, 24f);
                Widgets.Checkbox(extraRect.position, ref raidSettings.allowExtraRaids);
                TooltipHandler.TipRegion(extraRect, "SplitTheRaid.CheckExtraTooltip".Translate());

                Rect ptsRect = new Rect(col0Width + col1Width + (col2Width - 24f) / 2f, scrollView + 2f, 24f, 24f);
                Widgets.Checkbox(ptsRect.position, ref raidSettings.allowPointsModification);
                TooltipHandler.TipRegion(ptsRect, "SplitTheRaid.CheckPointsTooltip".Translate());

                Rect rndRect = new Rect(col0Width + col1Width + col2Width + (col3Width - 24f) / 2f, scrollView + 2f, 24f, 24f);
                Widgets.Checkbox(rndRect.position, ref raidSettings.allowRandomPick);
                TooltipHandler.TipRegion(rndRect, "SplitTheRaid.CheckRandomTooltip".Translate());

                i += 1;
                scrollView += rowHeight;
            }

            Widgets.EndScrollView();

        }
    }
}