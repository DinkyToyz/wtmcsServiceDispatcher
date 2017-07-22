namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Fletcher 16bit checksum methods, facilitating simple and small error control.
    /// </summary>
    internal static class FletcherChecksum
    {
        /// <summary>
        /// Calculates a Fletcher cehcksum and returns bytes that can be appended to data for simple error control.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="postEndIndex">The end index plus one.</param>
        public static ControlBytes GetControlBytes(byte[] data, int startIndex, int postEndIndex)
        {
            int sum1;
            int sum2;

            CalculateCheckSum(data, startIndex, postEndIndex, out sum1, out sum2);

            ControlBytes control;
            control.First = (byte)(255 - ((sum1 + sum2) % 255));
            control.Second = (byte)(255 - ((sum1 + control.First) % 255));

            return control;
        }

        /// <summary>
        /// Validates the specified data using fletcher checksum and control bytes.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="postEndIndex">The end index plus one.</param>
        /// <returns>True is checksum is valid.</returns>
        public static bool Validate(byte[] data, int startIndex, int postEndIndex)
        {
            int sum1;
            int sum2;

            CalculateCheckSum(data, startIndex, postEndIndex, out sum1, out sum2);

            return (sum1 == 0 && sum2 == 0);
        }

        /// <summary>
        /// Calculates a Fletcher checksum.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="postEndIndex">The end index plus one.</param>
        /// <param name="checkSum1">The first checksum.</param>
        /// <param name="checkSum2">The second checksum.</param>
        private static void CalculateCheckSum(byte[] data, int startIndex, int postEndIndex, out int checkSum1, out int checkSum2)
        {
            checkSum1 = 0;
            checkSum2 = 0;

            if (data != null)
            {
                for (int i = startIndex; i < postEndIndex; i++)
                {
                    checkSum1 = (checkSum1 + data[i]) % 255;
                    checkSum2 = (checkSum2 + checkSum1) % 255;
                }
            }
        }

        /// <summary>
        /// Fletcher checksum meant to be appended to data to facilitate simple error control.
        /// </summary>
        public struct ControlBytes
        {
            /// <summary>
            /// The first control byte.
            /// </summary>
            public byte First;

            /// <summary>
            /// The second control byte.
            /// </summary>
            public byte Second;
        }
    }
}