namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Frame boundaries.
    /// </summary>
    internal struct FrameBoundaries
    {
        /// <summary>
        /// The first identifier.
        /// </summary>
        public ushort FirstId;

        /// <summary>
        /// The last identifier.
        /// </summary>
        public ushort LastId;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameBoundaries"/> struct.
        /// </summary>
        /// <param name="firstFrame">The first frame.</param>
        /// <param name="lastFrame">The last frame.</param>
        public FrameBoundaries(uint firstFrame, uint lastFrame)
        {
            this.FirstId = (ushort)firstFrame;
            this.LastId = (ushort)lastFrame;
        }
    }
}
