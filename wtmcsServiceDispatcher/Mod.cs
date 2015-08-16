using ICities;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Mod interface.
    /// </summary>
    public class Mod : IUserMod
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mod"/> class.
        /// </summary>
        public Mod()
        {
            Log.NoOp();
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public string Description
        {
            get
            {
                return Library.Description;
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get
            {
                return Library.Title;
            }
        }

        /// <summary>
        /// Called when initializing mod settings UI.
        /// </summary>
        /// <param name="helper">The helper.</param>
        public void OnSettingsUI(UIHelperBase helper)
        {
            Log.Debug(this, "OnSettingsUI");

            try
            {
            }
            catch (System.Exception ex)
            {
                Log.Error(this, "OnSettingsUI", ex);
            }
        }
    }
}