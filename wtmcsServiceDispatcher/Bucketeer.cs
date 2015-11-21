using ColossalFramework;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Handles frame based buckets for object updates.
    /// </summary>
    internal class Bucketeer
    {
        /// <summary>
        /// The factor.
        /// </summary>
        private uint factor;

        /// <summary>
        /// The mask.
        /// </summary>
        private uint mask;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bucketeer"/> class.
        /// </summary>
        /// <param name="mask">The mask.</param>
        /// <param name="factor">The factor.</param>
        public Bucketeer(uint mask, uint factor)
        {
            this.mask = mask;
            this.factor = factor;
        }

        /// <summary>
        /// Gets the bucket boundaries.
        /// </summary>
        /// <param name="bucket">The bucket.</param>
        /// <returns>The bucket boundaries.</returns>
        public Boundaries GetBoundaries(uint bucket)
        {
            bucket = bucket & this.mask;

            return new Boundaries(bucket * this.factor, ((bucket + 1) * this.factor) - 1);
        }

        /// <summary>
        /// Gets the end bucket.
        /// </summary>
        /// <returns>The end bucket.</returns>
        public uint GetEnd()
        {
            return Singleton<SimulationManager>.instance.m_currentFrameIndex & this.mask;
        }

        /// <summary>
        /// Gets the next bucket.
        /// </summary>
        /// <param name="bucket">The current/last bucket.</param>
        /// <returns>The next bucket.</returns>
        public uint GetNext(uint bucket)
        {
            return bucket & this.mask;
        }

        /// <summary>
        /// Bucket boundaries.
        /// </summary>
        public struct Boundaries
        {
            /// <summary>
            /// The first identifier.
            /// </summary>
            public readonly ushort FirstId;

            /// <summary>
            /// The last identifier.
            /// </summary>
            public readonly ushort LastId;

            /// <summary>
            /// Initializes a new instance of the <see cref="Boundaries" /> struct.
            /// </summary>
            /// <param name="firstId">The first object id.</param>
            /// <param name="lastId">The last object id.</param>
            public Boundaries(uint firstId, uint lastId)
            {
                this.FirstId = (ushort)firstId;
                this.LastId = (ushort)lastId;
            }
        }
    }
}