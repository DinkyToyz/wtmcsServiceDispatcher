using System;

namespace AssemblyInfo
{
    /// <summary>
    /// Build stamps.
    /// </summary>
    public static class PreBuildStamps
    {
        /// <summary>
        /// Build-stamped ticks.
        /// </summary>
        private static long ticks = 636374936544348989; /*:TICKS:*/

        /// <summary>
        /// Gets the ticks.
        /// </summary>
        /// <value>
        /// The ticks.
        /// </value>
        public static long Ticks => ticks;

        /// <summary>
        /// Gets build-stamped date-time.
        /// </summary>
        /// <value>
        /// The date-time.
        /// </value>
        public static DateTime DateTime
        {
            get
            {
                return new DateTime(ticks);
            }
        }

        /// <summary>
        /// Gets build-stamped year.
        /// </summary>
        /// <value>
        /// The year.
        /// </value>
        public static int Year
        {
            get
            {
                return PreBuildStamps.DateTime.Year;
            }
        }
    }
}