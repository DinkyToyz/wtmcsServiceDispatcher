namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Global objects.
    /// </summary>
    internal static class Global
    {
        /// <summary>
        /// The buildings.
        /// </summary>
        public static Buildings Buildings = null;

        /// <summary>
        /// The current frame.
        /// </summary>
        public static uint CurrentFrame = 0;

        /// <summary>
        /// The hearse dispatcher.
        /// </summary>
        public static HearseDispatcher HearseDispatcher = null;

        /// <summary>
        /// A level is loaded.
        /// </summary>
        public static bool LevelLoaded = false;

        /// <summary>
        /// The minimum recheck interval for handled targets when pretending.
        /// </summary>
        public static uint PretendRecheckHandledInterval = RecheckHandledInterval * 10;

        /// <summary>
        /// The minimum recheck interval for targets when pretending.
        /// </summary>
        public static uint PretendRecheckInterval = RecheckInterval * 10;

        /// <summary>
        /// Pretend to handle hearses (find vehicles for target, but don't actually assign them).
        /// </summary>
        public static bool PretendToHandleHearses = true;

        /// <summary>
        /// The minimum recheck interval for handled targets.
        /// </summary>
        public static uint RecheckHandledInterval = 240;

        /// <summary>
        /// The minimum recheck interval for targets.
        /// </summary>
        public static uint RecheckInterval = 60;

        /// <summary>
        /// The minimum object update interval.
        /// </summary>
        public static uint ObjectUpdateInterval = 120;

        /// <summary>
        /// The settings.
        /// </summary>
        public static Settings Settings = null;
    }
}