using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace SplitTheRaid
{
    internal class UIHelper
    {
        public class IntRangeExpanded
        {
            // Settings
            private readonly int _globalMin;
            private readonly int _globalMax;
            private readonly int _step; // isn't used
            private readonly int _controlId;

            // Input buffers
            private string _localInputMinBuf;
            private string _localInputMaxBuf;
            private int _localInputMinVal;
            private int _localInputMaxVal;
            private int _localInputMinValPrevious;
            private int _localInputMaxValPrevious;

            // Range buffers
            private IntRange _localIntRangePrevious;

            public IntRangeExpanded(int globalMin, int globalMax, int step = 1, int controlId = 0)
            {
                _globalMin = globalMin;
                _globalMax = globalMax;
                _step = step;
                _controlId = controlId == 0 ? GetHashCode() : controlId;
            }

            public void Draw(Listing_Standard listing, ref IntRange range)
            {
                // Draw slider
                Rect sliderRect = listing.GetRect(28f);
                Widgets.IntRange(sliderRect, _controlId, ref range, _globalMin, _globalMax, null, 0);

                // Draw inputs
                Rect row = listing.GetRect(Text.LineHeight);
                float gap = 8f;
                float half = (row.width - gap) * 0.5f;
                Rect minCol = new Rect(row.x, row.y, half, row.height);
                Widgets.TextFieldNumeric(minCol, ref _localInputMinVal, ref _localInputMinBuf, _globalMin, _globalMax);
                Rect maxCol = new Rect(row.x + half + gap, row.y, half, row.height);
                Widgets.TextFieldNumeric(maxCol, ref _localInputMaxVal, ref _localInputMaxBuf, _globalMin, _globalMax);

                // Range changed
                if (_localIntRangePrevious != range)
                {
                    _localInputMinBuf = range.min.ToString();
                    _localInputMinVal = range.min;
                    _localInputMaxBuf = range.max.ToString();
                    _localInputMaxVal = range.max;

                }
                // Min input changed
                else if (_localInputMinValPrevious != _localInputMinVal)
                {
                    // If valid
                    if (_localInputMinVal <= _localInputMaxVal)
                    {
                        range.min = _localInputMinVal;
                    }
                }
                // Max input changed
                else if (_localInputMaxValPrevious != _localInputMaxVal)
                {
                    // If valid
                    if (_localInputMinVal <= _localInputMaxVal)
                    {
                        range.max = _localInputMaxVal;
                    }
                }

                _localInputMinValPrevious = _localInputMinVal;
                _localInputMaxValPrevious = _localInputMaxVal;
                _localIntRangePrevious = range;
            }
        }

        public class IntInputExpanded
        {
            // Settings
            private readonly int _inputMin;
            private readonly int _inputMax;
            private readonly int _sliderMin;
            private readonly int _sliderMax;
            private readonly bool _drawSlider;
            private readonly int _controlId;

            // Input buffers
            private string _localInputBuf;
            private string _localInputBufPrevious;
            private int _localInputVal;
            private int _localInputValPrevious;

            // Slider buffers
            private int _localSliderVal;
            private int _localSliderValPrevious;

            public IntInputExpanded(int inputMin, int inputMax, int sliderMin, int sliderMax, bool drawSlider = false, int controlId = 0)
            {
                _inputMin = inputMin;
                _inputMax = inputMax;
                _sliderMin = sliderMin;
                _sliderMax = sliderMax;
                _drawSlider = drawSlider;
                _controlId = controlId == 0 ? GetHashCode() : controlId;
            }

            public void Draw(Listing_Standard listing, ref int value, string label)
            {

                // Draw label and field
                Rect rect = listing.GetRect(Text.LineHeight);

                float labelWidth = rect.width * 0.9f;
                float inputWidth = rect.width * 0.1f;

                Rect labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
                Rect inputRect = new Rect(rect.x + labelWidth, rect.y, inputWidth, rect.height);

                Widgets.Label(labelRect, label);
                Widgets.TextFieldNumeric(inputRect, ref _localInputVal, ref _localInputBuf, _inputMin, _inputMax);

                // Draw slider
                if (_drawSlider)
                {
                    _localSliderVal = Mathf.RoundToInt(listing.Slider(value, _sliderMin, _sliderMax));
                }

                // Slider changed
                if (_localSliderValPrevious != _localSliderVal)
                {
                    value = _localSliderVal;
                    _localInputBuf = _localSliderVal.ToString();
                }
                // Input changed
                else if (_localInputValPrevious != _localInputVal)
                {
                    if (_localInputBuf != "")
                    {
                        value = _localInputVal;
                        _localSliderVal = _localInputVal;
                    }
                }

                _localSliderValPrevious = _localSliderVal;
                _localInputValPrevious = _localInputVal;
            }
        }

        public class FloatInputExpanded
        {
            // Settings
            private readonly float _inputMin;
            private readonly float _inputMax;
            private readonly float _sliderMin;
            private readonly float _sliderMax;
            private readonly bool _drawSlider;
            private readonly int _controlId;
            private readonly float _step; // not used (kept for compatibility)

            // Input buffers
            private string _localInputBuf;
            private string _localInputBufPrevious;
            private float _localInputVal;
            private float _localInputValPrevious;

            // Slider buffers
            private float _localSliderVal;
            private float _localSliderValPrevious;

            private bool _initialized;

            public FloatInputExpanded(float inputMin, float inputMax, float sliderMin, float sliderMax, bool drawSlider = false, int controlId = 0, float step = 1f)
            {
                _inputMin = inputMin;
                _inputMax = inputMax;
                _sliderMin = sliderMin;
                _sliderMax = sliderMax;
                _drawSlider = drawSlider;
                _controlId = controlId == 0 ? GetHashCode() : controlId;
                _step = step;
            }

            public void Draw(Listing_Standard listing, ref float value, string label)
            {
                // Initialize local state from external value on first draw
                if (!_initialized)
                {
                    _localInputVal = RoundToStep(value, 0.01f);
                    _localSliderVal = _localInputVal;
                    _localInputBuf = (Mathf.RoundToInt(_localInputVal * 100)).ToString();
                    _initialized = true;
                }

                // Draw label and field
                Rect rect = listing.GetRect(Text.LineHeight);

                float labelWidth = rect.width * 0.9f;
                float inputWidth = rect.width * 0.1f;

                Rect labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
                Rect inputRect = new Rect(rect.x + labelWidth, rect.y, inputWidth, rect.height);

                Widgets.Label(labelRect, label);
                Widgets.TextFieldPercent(inputRect, ref _localInputVal, ref _localInputBuf, _inputMin, _inputMax);

                // Ensure percent value is integer (round to nearest 0.01)
                _localInputVal = RoundToStep(_localInputVal, 0.01f);

                // Draw slider
                if (_drawSlider)
                {
                    _localSliderVal = listing.Slider(value, _sliderMin, _sliderMax);
                }

                // Slider changed
                if (_localSliderValPrevious != _localSliderVal)
                {
                    value = _localSliderVal;
                    _localInputVal = value;
                    _localInputBuf = (Mathf.RoundToInt(value * 100)).ToString();
                }
                // Input changed
                else if (_localInputValPrevious != _localInputVal)
                {
                    if (_localInputBuf != "")
                    {
                        value = _localInputVal;
                        _localSliderVal = _localInputVal;
                        _localInputBuf = (Mathf.RoundToInt(_localInputVal * 100)).ToString();
                    }
                }

                _localSliderValPrevious = _localSliderVal;
                _localInputValPrevious = _localInputVal;
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