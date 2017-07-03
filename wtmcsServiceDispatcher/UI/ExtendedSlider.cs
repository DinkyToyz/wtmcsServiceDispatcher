using System;
using ColossalFramework.UI;
using ICities;

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
            this.Initialize(helper, text, min, max, step, defaultValue, allowFloats, null, eventCallback);
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
            this.Initialize(helper, text, min, max, step, defaultValue, allowFloats, textFormat, eventCallback);
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
            this.Initialize(helper, text, min, max, step, defaultValue, allowFloats, null, eventCallback);
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
                this.allowFloats = value;
                if (this.textField != null)
                {
                    this.textField.allowFloats = value;
                }

                if (this.autoTextFormat)
                {
                    if (value)
                    {
                        this.textFormat = "F2";
                    }
                    else
                    {
                        this.textFormat = "F0";
                    }
                }
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
                return this.FormatText(this.slider.value);
            }

            set
            {
                try
                {
                    this.isUpdating = true;

                    float number = this.ParseText(value, true);
                    this.slider.value = number;
                }
                finally
                {
                    this.isUpdating = false;
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
                return this.slider.value;
            }

            set
            {
                try
                {
                    this.isUpdating = true;

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
                }
                finally
                {
                    this.isUpdating = false;
                }
            }
        }

        /// <summary>
        /// Formats the value as text.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The formatted value.</returns>
        private string FormatText(float value)
        {
            return value.ToString(this.textFormat);
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
        /// <param name="textFormat">The text format.</param>
        /// <param name="eventCallback">The value changed event callback.</param>
        private void Initialize(UIHelperBase helper, string text, float min, float max, float step, float defaultValue, bool allowFloats, string textFormat, OnValueChanged eventCallback)
        {
            this.SetTextFormat(allowFloats, textFormat);

            this.slider = helper.AddSlider(
                text,
                min,
                max,
                step,
                defaultValue,
                value =>
                {
                    if (!this.isUpdating)
                    {
                        try
                        {
                            this.isUpdating = true;

                            this.textField.text = this.FormatText(value);

                            eventCallback(value);
                        }
                        finally
                        {
                            this.isUpdating = false;
                        }
                    }
                }) as UISlider;

            this.textField = helper.AddTextfield(
                text,
                this.FormatText(defaultValue),
                value =>
                {
                    if (submitOnFocusLost && !this.isUpdating)
                    {
                        try
                        {
                            this.isUpdating = true;

                            float number = this.ParseText(value, false);
                            if (number != this.slider.value)
                            {
                                this.slider.value = number;
                            }
                        }
                        finally
                        {
                            this.isUpdating = false;
                        }
                    }
                },
                value =>
                {
                    if (!this.isUpdating)
                    {
                        try
                        {
                            this.isUpdating = true;

                            float number = this.ParseText(value, false);
                            if (number != slider.value)
                            {
                                this.slider.value = number;
                                this.textField.text = this.FormatText(number);

                                eventCallback(number);
                            }
                        }
                        finally
                        {
                            this.isUpdating = false;
                        }
                    }
                }) as UITextField;

            this.textField.numericalOnly = true;
            this.textField.allowFloats = this.AllowFloats;
            this.textField.allowNegative = min < 0;
            this.textField.submitOnFocusLost = this.submitOnFocusLost;
            this.textField.eventVisibilityChanged += (component, value) =>
            {
                this.HideTextFieldLabel();
            };

            this.HideTextFieldLabel();
        }

        /// <summary>
        /// Logs some debug info.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="label">The label.</param>
        /// <param name="hint">A hint.</param>
        private void LogTextFieldDbg(UIComponent parent, UILabel label, string hint)
        {
            Log.DevDebug(this, "LogTextFieldDbg", hint, label.text, parent.position, parent.size, label.position, label.size, this.textField.position, this.textField.size);
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
                if (value != Math.Truncate(value))
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

            this.AllowFloats = allowFloats;
        }
    }
}