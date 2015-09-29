using System;
using ColossalFramework.UI;
using ICities;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// UI extension methods.
    /// </summary>
    public static class UIExtensions
    {
        /// <summary>
        /// Adds an extended slider to a UI helper.
        /// </summary>
        /// <param name="helper">The UI helper.</param>
        /// <param name="text">The text label.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="step">The step size.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="eventCallback">The value changed event callback.</param>
        /// <returns>The extended slider.</returns>
        public static ExtendedSlider AddExtendedSlider(this UIHelperBase helper, string text, float min, float max, float step, float defaultValue, OnValueChanged eventCallback)
        {
            return new ExtendedSlider(helper, text, min, max, step, defaultValue, eventCallback);
        }

        /// <summary>
        /// Adds an extended slider to a UI helper.
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
        /// <returns>The extended slider.</returns>
        public static ExtendedSlider AddExtendedSlider(this UIHelperBase helper, string text, float min, float max, float step, float defaultValue, bool allowFloats, string textFormat, OnValueChanged eventCallback)
        {
            return new ExtendedSlider(helper, text, min, max, step, defaultValue, allowFloats, textFormat, eventCallback);
        }

        /// <summary>
        /// Adds an extended slider to a UI helper.
        /// </summary>
        /// <param name="helper">The UI helper.</param>
        /// <param name="text">The text label.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="step">The step size.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="allowFloats">If set to <c>true</c> allow non-integers.</param>
        /// <param name="eventCallback">The value changed event callback.</param>
        /// <returns>The extended slider.</returns>
        public static ExtendedSlider AddExtendedSlider(this UIHelperBase helper, string text, float min, float max, float step, float defaultValue, bool allowFloats, OnValueChanged eventCallback)
        {
            return new ExtendedSlider(helper, text, min, max, step, defaultValue, allowFloats, null, eventCallback);
        }

        /// <summary>
        /// Returns the UI helper as a UI component.
        /// </summary>
        /// <param name="helper">The UI helper.</param>
        /// <returns>The UI component.</returns>
        /// <exception cref="System.ArgumentNullException">Helper is null.</exception>
        /// <exception cref="System.ArgumentException">Helper is not UIHelper.</exception>
        /// <exception cref="System.InvalidCastException">Helper self did not return component.</exception>
        public static UIComponent Component(this UIHelperBase helper)
        {
            if (helper == null)
            {
                throw new ArgumentNullException("Helper is null");
            }

            if (!(helper is UIHelper))
            {
                throw new ArgumentException("Helper is not UIHelper");
            }

            object component = ((UIHelper)helper).self;

            if (!(component is UIComponent))
            {
                throw new InvalidCastException("Helper self did not return component");
            }

            return (UIComponent)component;
        }
    }
}
