using ColossalFramework.UI;
using ICities;
using System;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// UI helper methods.
    /// </summary>
    internal static class UI
    {
        /// <summary>
        /// Creates a slider value label.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="control">The control.</param>
        /// <param name="name">The slider name (for logging).</param>
        /// <param name="text">The text.</param>
        /// <returns>
        /// The label.
        /// </returns>
        public static UILabel CreateOptionsLabel(UIHelperBase group, object control, string name, string text)
        {
            //// TODO: Maybe tis could be a textfield instead?

            try
            {
                Log.Debug(typeof(UI), "CreateSliderLabel", group, control, name, text);

                UIComponent root = (UIComponent)((UIHelper)group).self;
                LogPosition("CreateSliderLabel", name, "root", root);

                UILabel label = root.AddUIComponent<UILabel>();
                LogPosition("CreateSliderLabel", name, "label", label);

                label.text = text;

                return label;
            }
            catch (Exception ex)
            {
                Log.Error(typeof(UI), "CreateSliderLabel", ex, name);
            }

            return null;
        }

        /// <summary>
        /// Logs the components position.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="block">The block.</param>
        /// <param name="connectedName">Name of the connected control.</param>
        /// <param name="componentName">Name of the component.</param>
        /// <param name="component">The component.</param>
        public static void LogPosition(object source, string block, string connectedName, string componentName, UIComponent component)
        {
            Log.Debug(source, block, connectedName, componentName, component, "Position", component.absolutePosition, component.relativePosition, component.position, component.width, component.height, component.anchor);
        }

        /// <summary>
        /// Logs the position of the component.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="connectedName">Name of the connected control.</param>
        /// <param name="componentName">Name of the component.</param>
        /// <param name="component">The component.</param>
        private static void LogPosition(string block, string connectedName, string componentName, UIComponent component)
        {
            Log.Debug(typeof(UI), block, connectedName, componentName, component, "Position", component.absolutePosition, component.relativePosition, component.position, component.width, component.height, component.anchor);
        }
    }
}
