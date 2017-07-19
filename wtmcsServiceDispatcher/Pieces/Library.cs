namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Mod info.
    /// </summary>
    internal static class Library
    {
        /// <summary>
        /// The description.
        /// </summary>
        public const string Description = "Dispatches Cities: Skylines services.";

        /// <summary>
        /// The name.
        /// </summary>
        public const string Name = "wtmcsServiceDispatcher";

        /// <summary>
        /// The steam workshop file identifier.
        /// </summary>
        public const uint SteamWorkshopFileId = 512341354;

        /// <summary>
        /// The title.
        /// </summary>
        public const string Title = "Central Services Dispatcher (WtM)";

        /// <summary>
        /// Gets a value indicating whether this is a debug build.
        /// </summary>
        /// <value>
        /// <c>true</c> if this is a debug build; otherwise, <c>false</c>.
        /// </value>
        public static bool IsDebugBuild
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }
    }
}