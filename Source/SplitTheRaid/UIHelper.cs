using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SplitTheRaid
{
    internal class UIHelper
    {
        public class FloatRangeExpanded
        {
            private string _minBuf;
            private string _maxBuf;
            private float _prevMin;
            private float _prevMax;
            private float _localMin;
            private float _localMax;

            // Settings
            private readonly float _globalMin;
            private readonly float _globalMax;
            private readonly float _step;
            private readonly int _controlId;

            public FloatRangeExpanded(float globalMin, float globalMax, float step = 0.1f, int controlId = 0)
            {
                _globalMin = globalMin;
                _globalMax = globalMax;
                _step = step;
                _controlId = controlId == 0 ? GetHashCode() : controlId;
            }

            public void Draw(Listing_Standard listing, ref FloatRange range)
            {
                if (_minBuf == null) _minBuf = range.min.ToString("0.###");
                if (_maxBuf == null) _maxBuf = range.max.ToString("0.###");

                // Save old values before slider
                _prevMin = range.min;
                _prevMax = range.max;

                Rect sliderRect = listing.GetRect(28f);
                Widgets.FloatRange(sliderRect, _controlId, ref range, _globalMin, _globalMax, null, ToStringStyle.FloatTwo, 0f, GameFont.Small, null, _step);

                // If slider changed values, update buffers
                if (!Mathf.Approximately(_prevMin, range.min) || !Mathf.Approximately(_prevMax, range.max))
                {
                    _minBuf = range.min.ToString("0.###");
                    _maxBuf = range.max.ToString("0.###");
                }

                // Inputs
                Rect row = listing.GetRect(Text.LineHeight);
                float gap = 8f;
                float half = (row.width - gap) * 0.5f;

                Rect minCol = new Rect(row.x, row.y, half, row.height);
                Widgets.TextFieldNumeric(minCol, ref _localMin, ref _minBuf, _globalMin, _globalMax);

                Rect maxCol = new Rect(row.x + half + gap, row.y, half, row.height);
                Widgets.TextFieldNumeric(maxCol, ref _localMax, ref _maxBuf, _globalMin, _globalMax);

                if (_localMin < _localMax)
                {
                    range.min = _localMin;
                    range.max = _localMax;
                }

                range.min = Mathf.Clamp(range.min, _globalMin, _globalMax);
                range.max = Mathf.Clamp(range.max, _globalMin, _globalMax);
                if (range.min > range.max)
                {
                    range.max = range.min;
                    _maxBuf = range.max.ToString("0.###");
                }
            }
        }
        public class IntRangeExpanded
        {
            private string _minBuf;
            private string _maxBuf;
            private int _prevMin;
            private int _prevMax;
            private int _localMin;
            private int _localMax;

            private readonly int _globalMin;
            private readonly int _globalMax;
            private readonly int _step; // is not used
            private readonly int _controlId;

            public IntRangeExpanded(int globalMin, int globalMax, int step = 1, int controlId = 0)
            {
                _globalMin = globalMin;
                _globalMax = globalMax;
                _step = step;
                _controlId = controlId == 0 ? GetHashCode() : controlId;
            }

            public void Draw(Listing_Standard listing, ref IntRange range)
            {
                // Can't do that in the constructor
                // since we can't store ref of the range
                if (_minBuf == null) _minBuf = range.min.ToString();
                if (_maxBuf == null) _maxBuf = range.max.ToString();

                _prevMin = range.min;
                _prevMax = range.max;

                Rect sliderRect = listing.GetRect(28f);
                // Используем Widgets.IntRange (существует в RimWorld)
                Widgets.IntRange(sliderRect, _controlId, ref range, _globalMin, _globalMax, null, 0);

                if (_prevMin != range.min || _prevMax != range.max)
                {
                    _minBuf = range.min.ToString();
                    _maxBuf = range.max.ToString();
                }

                Rect row = listing.GetRect(Text.LineHeight);
                float gap = 8f;
                float half = (row.width - gap) * 0.5f;

                Rect minCol = new Rect(row.x, row.y, half, row.height);
                Widgets.TextFieldNumeric(minCol, ref _localMin, ref _minBuf, _globalMin, _globalMax);

                Rect maxCol = new Rect(row.x + half + gap, row.y, half, row.height);
                Widgets.TextFieldNumeric(maxCol, ref _localMax, ref _maxBuf, _globalMin, _globalMax);

                if (_localMin < _localMax)
                {
                    range.min = _localMin;
                    range.max = _localMax;
                }

                // TODO: Later make it unrestricted or something

                range.min = Mathf.Clamp(range.min, _globalMin, _globalMax);
                range.max = Mathf.Clamp(range.max, _globalMin, _globalMax);
                if (range.min > range.max)
                {
                    range.max = range.min;
                    _maxBuf = range.max.ToString();
                }
            }
        }

        public static void IntInput(Listing_Standard listing, ref int value, ref string buffer, string label, int min = 0, int max = 100)
        {
            Rect rect = listing.GetRect(Text.LineHeight);

            float labelWidth = rect.width * 0.9f;
            float inputWidth = rect.width * 0.1f;

            Rect labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
            Rect inputRect = new Rect(rect.x + labelWidth, rect.y, inputWidth, rect.height);

            Widgets.Label(labelRect, label);
            Widgets.TextFieldNumeric(inputRect, ref value, ref buffer, min, max);

            if (value.ToString() != buffer && buffer != "")
            {
                buffer = value.ToString();
            }
        }

        public static void FloatInput(Listing_Standard listing, ref float value, ref string buffer, string label, float min = 0f, float max = 5f)
        {
            Rect rect = listing.GetRect(Text.LineHeight);

            float labelWidth = rect.width * 0.9f;
            float inputWidth = rect.width * 0.1f;

            Rect labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
            Rect inputRect = new Rect(rect.x + labelWidth, rect.y, inputWidth, rect.height);

            Widgets.Label(labelRect, label);
            Widgets.TextFieldPercent(inputRect, ref value, ref buffer, min, max);

            if ((value * 100).ToString() != buffer && buffer != "")
            {
                buffer = (value * 100).ToString();
            }
        }

        public static float RoundToStep(float value, float step)
        {
            int decimals = (int)Mathf.Round(-Mathf.Log10(step));
            float rounded = Mathf.Round(value / step) * step;
            return (float)Math.Round(rounded, decimals, MidpointRounding.AwayFromZero);
        }
    }
}