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
        /// Logs the component data.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="block">The block.</param>
        /// <param name="component">The component.</param>
        /// <param name="componentName">Name of the component.</param>
        /// <param name="connectedName">Name of the connected component.</param>
        public static void LogComponent(object source, string block, UIComponent component, string componentName = null, string connectedName = null)
        {
            LogComponent(source, block, component, componentName, connectedName, 0);
        }

        /// <summary>
        /// Logs the UI helper group.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="block">The block.</param>
        /// <param name="group">The group.</param>
        /// <param name="componentName">Name of the component.</param>
        /// <param name="connectedName">Name of the connected component.</param>
        public static void LogGroup(object source, string block, UIHelperBase group, string componentName = null, string connectedName = null)
        {
            try
            {
                if (group != null && group is UIHelper)
                {
                    LogComponent(source, block, group.Component(), componentName, connectedName, 0);
                }
            }
            catch (Exception ex)
            {
                Log.Debug(source, block, connectedName, componentName, ex.GetType(), ex.Message);
            }
        }

        /// <summary>
        /// Logs the components position.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="block">The block.</param>
        /// <param name="component">The component.</param>
        /// <param name="componentName">Name of the component.</param>
        /// <param name="connectedName">Name of the connected control.</param>
        public static void LogPosition(object source, string block, UIComponent component, string componentName = null, string connectedName = null)
        {
            if (String.IsNullOrEmpty(componentName))
            {
                componentName = component.cachedName;
            }

            Log.Debug(source, block, connectedName, componentName, component, "Position", component.absolutePosition, component.relativePosition, component.position, component.width, component.height, component.anchor);
        }

        /// <summary>
        /// Logs the component data.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="connectedName">Name of the connected component.</param>
        /// <param name="componentName">Name of the component.</param>
        /// <param name="component">The component.</param>
        private static void LogComponent(string block, string connectedName, string componentName, UIComponent component)
        {
            LogComponent(typeof(UI), block, component, componentName, connectedName, 0);
        }

        /// <summary>
        /// Logs the component data.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="block">The block.</param>
        /// <param name="component">The component.</param>
        /// <param name="componentName">Name of the component.</param>
        /// <param name="connectedName">Name of the connected component.</param>
        /// <param name="depth">The recursion depth.</param>
        private static void LogComponent(object source, string block, UIComponent component, string componentName, string connectedName, int depth)
        {
            string componentPath = null;

            try
            {
                if (component != null && component is UIComponent)
                {
                    Log.InfoList info = new Log.InfoList();

                    componentPath = componentName;

                    if (String.IsNullOrEmpty(componentPath))
                    {
                        componentPath = component.cachedName;

                        if (String.IsNullOrEmpty(componentPath))
                        {
                            componentPath = "?";
                        }
                    }

                    if (!String.IsNullOrEmpty(connectedName))
                    {
                        componentPath = connectedName + "/" + componentPath;
                    }

                    try
                    {
                        foreach (var property in component.GetType().GetProperties())
                        {
                            if (property != null)
                            {
                                try
                                {
                                    info.Add(property.Name, property.GetValue(component, null));
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                    catch
                    {
                    }

                    Log.Debug(source, block, depth, componentPath, component.GetType(), component, info);

                    if (depth < 32)
                    {
                        depth++;

                        foreach (UIComponent child in component.components)
                        {
                            if (child != null)
                            {
                                LogComponent(source, block, child, null, componentPath, depth);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (String.IsNullOrEmpty(componentPath))
                {
                    Log.Debug(source, block, connectedName, componentName, ex.GetType(), ex.Message);
                }
                else
                {
                    Log.Debug(source, block, componentPath, ex.GetType(), ex.Message);
                }
            }
        }

        /// <summary>
        /// Logs the position of the component.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="component">The component.</param>
        /// <param name="componentName">Name of the component.</param>
        /// <param name="connectedName">Name of the connected control.</param>
        private static void LogPosition(string block, UIComponent component, string componentName = null, string connectedName = null)
        {
            LogPosition(typeof(UI), block, component, componentName, connectedName);
        }
    }
}