using ColossalFramework.UI;
using ICities;
using System;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Options slider with text field.
    /// </summary>
    public class ExtendedSlider
    {
        /// <summary>
        /// Allow non-integers.
        /// </summary>
        private bool allowFloats = true;

        /// <summary>
        /// Use automatic automatic text format.
        /// </summary>
        private bool autoTextFormat = true;

        /// <summary>
        /// The zero is represented by an empty string.
        /// </summary>
        private bool emptyZero = false;

        /// <summary>
        /// This instance is initialized.
        /// </summary>
        private bool isInitialized = false;

        /// <summary>
        /// The values are updating and should not generate callbacks and sets.
        /// </summary>
        private bool isUpdating = false;

        /// <summary>
        /// The slider.
        /// </summary>
        private UISlider slider = null;

        /// <summary>
        /// Submit text field on lost focus.
        /// </summary>
        private bool submitOnFocusLost = false;

        /// <summary>
        /// The text field.
        /// </summary>
        private UITextField textField = null;

        /// <summary>
        /// The text format.
        /// </summary>
        private string textFormat = "F2";

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedSlider"/> class.
        /// </summary>
        /// <param name="helper">The UI helper.</param>
        /// <param name="text">The text label.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="step">The step size.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="eventCallback">The value changed event callback.</param>
        public ExtendedSlider(UIHelperBase helper, string text, float min, float max, float step, float defaultValue, OnValueChanged eventCallback)
        {
            bool allowFloats = min != Math.Truncate(min) || max != Math.Truncate(max) || step != Math.Truncate(step) || defaultValue != Math.Truncate(defaultValue);
            this.Initialize(helper, text, min, max, step, defaultValue, allowFloats, false, null, eventCallback);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedSlider" /> class.
        /// </summary>
        /// <param name="helper">The UI helper.</param>
        /// <param name="text">The text label.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="step">The step size.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="allowFloats">If set to <c>true</c> allow non-integers.</param>
        /// <param name="textFormat">The text format.</param>
        /// <param name="eventCallback">The value changed event callback.</param>
        public ExtendedSlider(UIHelperBase helper, string text, float min, float max, float step, float defaultValue, bool allowFloats, string textFormat, OnValueChanged eventCallback)
        {
            this.Initialize(helper, text, min, max, step, defaultValue, allowFloats, false, textFormat, eventCallback);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedSlider" /> class.
        /// </summary>
        /// <param name="helper">The UI helper.</param>
        /// <param name="text">The text label.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="step">The step size.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="allowFloats">If set to <c>true</c> allow non-integers.</param>
        /// <param name="eventCallback">The value changed event callback.</param>
        public ExtendedSlider(UIHelperBase helper, string text, float min, float max, float step, float defaultValue, bool allowFloats, OnValueChanged eventCallback)
        {
            this.Initialize(helper, text, min, max, step, defaultValue, allowFloats, false, null, eventCallback);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedSlider" /> class.
        /// </summary>
        /// <param name="helper">The UI helper.</param>
        /// <param name="text">The text label.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="step">The step size.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="allowFloats">If set to <c>true</c> allow non-integers.</param>
        /// <param name="zeroIsEmpty">if set to <c>true</c> zero is empty.</param>
        /// <param name="eventCallback">The value changed event callback.</param>
        public ExtendedSlider(UIHelperBase helper, string text, float min, float max, float step, float defaultValue, bool allowFloats, bool zeroIsEmpty, OnValueChanged eventCallback)
        {
            this.Initialize(helper, text, min, max, step, defaultValue, allowFloats, zeroIsEmpty, null, eventCallback);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to allow non-integers.
        /// </summary>
        /// <value>
        ///   <c>true</c> if non-integers should be allowed; otherwise, <c>false</c>.
        /// </value>
        public bool AllowFloats
        {
            get
            {
                return this.allowFloats;
            }

            set
            {
                this.SetAllowFloats(value);

                this.Update(() =>
                    {
                        this.textField.text = this.FormatText();
                    });
            }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text
        {
            get
            {
                if (!this.isInitialized)
                {
                    throw new InvalidOperationException("Control is not initialized");
                }

                return this.FormatText(this.slider.value);
            }

            set
            {
                if (!this.isInitialized)
                {
                    throw new InvalidOperationException("Control is not initialized");
                }

                bool updated = this.Update(() =>
                    {
                        float number = this.ParseText(value, true);

                        this.slider.value = number;
                        this.textField.text = this.FormatText(number);
                    });

                if (!updated)
                {
                    throw new InvalidOperationException("Cannot set text while control is updating");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the text field submits on lost focus.
        /// </summary>
        /// <value>
        /// <c>true</c> if the text field should submit on lost focus; otherwise, <c>false</c>.
        /// </value>
        public bool TextFieldSubmitOnFocusLost
        {
            get
            {
                return this.submitOnFocusLost;
            }

            set
            {
                this.submitOnFocusLost = value;
                if (this.textField != null)
                {
                    this.textField.submitOnFocusLost = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the text format.
        /// </summary>
        /// <value>
        /// The text format.
        /// </value>
        public string TextFormat
        {
            get
            {
                return this.textFormat;
            }

            set
            {
                this.SetTextFormat(this.AllowFloats, value);

                this.Update(() =>
                {
                    this.textField.text = this.FormatText();
                });
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public float Value
        {
            get
            {
                if (!this.isInitialized)
                {
                    throw new InvalidOperationException("Control is not initialized");
                }

                return this.slider.value;
            }

            set
            {
                if (!this.isInitialized)
                {
                    throw new InvalidOperationException("Control is not initialized");
                }

                bool updated = this.Update(() =>
                {
                    if (value > this.slider.maxValue)
                    {
                        value = this.slider.maxValue;
                    }
                    else if (value < this.slider.minValue)
                    {
                        value = this.slider.minValue;
                    }

                    if (!this.AllowFloats)
                    {
                        value = (float)Math.Truncate(value);
                    }

                    this.slider.value = value;
                    this.textField.text = this.FormatText(value);
                });

                if (!updated)
                {
                    throw new InvalidOperationException("Cannot set value while control is updating");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether zero is represented by an empty string.
        /// </summary>
        /// <value>
        ///   <c>true</c> if zero is an empty string; otherwise, <c>false</c>.
        /// </value>
        public bool ZeroIsEmpty
        {
            get => this.emptyZero;
            set
            {
                this.emptyZero = value;

                this.Update(() =>
                    {
                        if (this.slider.value == 0.0f)
                        {
                            this.textField.text = this.FormatText();
                        }
                    });
            }
        }

        /// <summary>
        /// Formats the value as text.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The formatted value.</returns>
        private string FormatText(float value)
        {
            return (this.emptyZero && value == 0.0f) ? "" : value.ToString(this.textFormat);
        }

        /// <summary>
        /// Formats the value as text.
        /// </summary>
        /// <returns>
        /// The formatted value.
        /// </returns>
        private string FormatText()
        {
            return this.FormatText(this.slider.value);
        }

        /// <summary>
        /// Gets the maximum length of the text.
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="allowFloats">if set to <c>true</c> [allow floats].</param>
        /// <returns>The maximum length of the text.</returns>
        private int GetMaxTextLength(float min, float max, bool allowFloats)
        {
            int maxLen;
            if (allowFloats)
            {
                maxLen = Math.Max(Math.Max(max.ToString("G17").Length, min.ToString("G17").Length), 18);
            }
            else
            {
                maxLen = Math.Max(((long)Math.Truncate(max)).ToString().Length, ((long)Math.Truncate(min)).ToString().Length);
            }

            return Math.Max(Math.Max(this.FormatText(min).Length, this.FormatText(max).Length), maxLen);
        }

        /// <summary>
        /// Hides the text field label.
        /// </summary>
        private void HideTextFieldLabel()
        {
            if (this.textField != null)
            {
                UIComponent parent = this.textField.parent;
                UILabel label = parent.Find<UILabel>("Label");
                if (label != null)
                {
                    label.text = "";
                    label.height = 0;

                    ////parent.RemoveUIComponent(label);
                    ////label.Hide();
                }
            }
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="helper">The UI helper.</param>
        /// <param name="text">The text label.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="step">The step size.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="allowFloats">If set to <c>true</c> allow non-integers.</param>
        /// <param name="emptyZero">if set to <c>true</c> empty zero.</param>
        /// <param name="textFormat">The text format.</param>
        /// <param name="eventCallback">The value changed event callback.</param>
        private void Initialize(UIHelperBase helper, string text, float min, float max, float step, float defaultValue, bool allowFloats, bool emptyZero, string textFormat, OnValueChanged eventCallback)
        {
            this.SetTextFormat(allowFloats, textFormat);
            this.emptyZero = emptyZero;

            this.slider = helper.AddSlider(
                text,
                min,
                max,
                step,
                defaultValue,
                value =>
                {
                    this.Update(() =>
                    {
                        this.textField.text = this.FormatText(value);
                        eventCallback(value);
                    });
                }) as UISlider;

            this.textField = helper.AddTextfield(
                text,
                this.FormatText(defaultValue),
                value =>
                {
                    if (this.submitOnFocusLost)
                    {
                        this.Update(() =>
                        {
                            float number = this.ParseText(value, false);

                            if (number != this.slider.value)
                            {
                                this.slider.value = number;
                            }
                        });
                    }
                },
                value =>
                {
                    this.Update(() =>
                    {
                        float number = this.ParseText(value, false);

                        if (number != slider.value)
                        {
                            this.slider.value = number;
                            this.textField.text = this.FormatText(number);

                            eventCallback(number);
                        }
                    });
                }) as UITextField;

            UIComponent sliderParent = this.slider.parent;
            UILabel sliderLabel = sliderParent.Find<UILabel>("Label");
            if (sliderLabel != null)
            {
                sliderLabel.width *= 2;
            }

            this.textField.numericalOnly = true;
            this.textField.allowFloats = this.AllowFloats;
            this.textField.allowNegative = min < 0;
            this.textField.submitOnFocusLost = this.submitOnFocusLost;
            this.textField.maxLength = this.GetMaxTextLength(min, max, allowFloats);

            this.textField.eventVisibilityChanged += (component, value) =>
            {
                this.HideTextFieldLabel();
            };

            this.HideTextFieldLabel();

            this.isInitialized = true;
        }

        /// <summary>
        /// Logs some debug info.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="label">The label.</param>
        /// <param name="hint">A hint.</param>
        private void TextFieldDebugLog(UIComponent parent, UILabel label, string hint)
        {
            Log.DevDebug(this, "TextFieldDebugLog", hint, label.text, parent.position, parent.size, label.position, label.size, this.textField.position, this.textField.size);
        }

        private void DevDebugLog()
        {
            Log.DevDebug(this, "DevDebugLog", this.isInitialized, this.isUpdating, this.submitOnFocusLost, this.autoTextFormat, this.allowFloats, this.emptyZero, this.textFormat);

            if (this.slider != null)
            {
                Log.DevDebug(this, "DevDebugLog", "Slider", slider.enabled, slider.canFocus, slider.minValue, slider.maxValue, slider.stepSize, slider.value);
            }

            if (this.textField != null)
            {
                Log.DevDebug(this, "DevDebugLog", "TextField", textField.enabled, textField.canFocus, textField.numericalOnly, textField.allowFloats, textField.allowNegative, textField.maxLength, textField.text);
            }
        }

        /// <summary>
        /// Parses the text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="throwOnError">If set to <c>true</c> throw exception if text is not valid.</param>
        /// <returns>The parsed value if text is valid; otherwise the current value.</returns>
        /// <exception cref="System.ArgumentException">
        /// Text is not a valid float.
        /// or
        /// Text is not a valid integer.
        /// </exception>
        private float ParseText(string text, bool throwOnError)
        {
            float value;
            if (!float.TryParse(text, out value))
            {
                if (throwOnError)
                {
                    throw new ArgumentException("Text is not a valid float");
                }

                return this.slider.value;
            }

            if (!this.AllowFloats)
            {
                if (throwOnError && value != Math.Truncate(value))
                {
                    throw new ArgumentException("Text is not a valid integer");
                }

                value = (float)Math.Truncate(value);
            }

            if (value > this.slider.maxValue)
            {
                value = this.slider.maxValue;
            }
            else if (value < this.slider.minValue)
            {
                value = this.slider.minValue;
            }

            return value;
        }

        /// <summary>
        /// Sets the allow floats value and formats.
        /// </summary>
        /// <param name="allowFloats">if set to <c>true</c> allow floats.</param>
        private void SetAllowFloats(bool allowFloats)
        {
            this.allowFloats = allowFloats;
            if (this.autoTextFormat)
            {
                if (allowFloats)
                {
                    this.textFormat = "F2";
                }
                else
                {
                    this.textFormat = "F0";
                }
            }

            if (this.textField != null)
            {
                this.textField.allowFloats = allowFloats;

                if (this.slider != null)
                {
                    this.textField.maxLength = this.GetMaxTextLength(this.slider.minValue, this.slider.maxValue, allowFloats);
                }
            }
        }

        /// <summary>
        /// Sets the text format.
        /// </summary>
        /// <param name="allowFloats">If set to <c>true</c> allow non-integers.</param>
        /// <param name="textFormat">The text format. Set to null for automatic text format.</param>
        private void SetTextFormat(bool allowFloats, string textFormat)
        {
            if (!String.IsNullOrEmpty(textFormat))
            {
                this.autoTextFormat = false;
                this.textFormat = textFormat;
            }
            else
            {
                this.autoTextFormat = true;
            }

            this.SetAllowFloats(allowFloats);
        }

        /// <summary>
        /// Updates this instance using the specified updater.
        /// </summary>
        /// <param name="updater">The updater.</param>
        /// <returns>True unless control is already updating, or not yet initialized.</returns>
        private bool Update(Action updater)
        {
            if (!this.isInitialized)
            {
                return false;
            }

            bool updated = false;
            try
            {
                lock (this)
                {
                    if (this.isUpdating)
                    {
                        return false;
                    }

                    updated = true;
                    this.isUpdating = true;
                }

                updater();
                return true;
            }
            finally
            {
                if (updated)
                {
                    lock (this)
                    {
                        this.isUpdating = false;
                    }
                }
            }
        }
    }
}