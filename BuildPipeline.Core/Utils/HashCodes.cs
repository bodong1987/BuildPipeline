namespace BuildPipeline.Core.Utils
{
    /// <summary>
    /// Class HashCodes.
    /// </summary>
    public static class HashCodes
    {
        #region Interfaces
        /// <summary>
        /// Combines the specified h.
        /// </summary>
        /// <param name="h">The h.</param>
        /// <param name="hashes">The hashes.</param>
        /// <returns>Int32.</returns>
        public static Int32 Combine(Int32 h, params Int32[] hashes)
        {
            var v = h;

            foreach(var i in hashes)
            {
                v = CombineCore(v, i);
            }

            return v;
        }

        /// <summary>
        /// Combines the specified h.
        /// </summary>
        /// <param name="h">The h.</param>
        /// <param name="hashes">The hashes.</param>
        /// <returns>UInt32.</returns>
        public static UInt32 Combine(UInt32 h, params UInt32[] hashes)
        {
            var v = h;

            foreach (var i in hashes)
            {
                v = CombineCore(v, i);
            }

            return v;
        }

        /// <summary>
        /// Combines the specified h.
        /// </summary>
        /// <param name="h">The h.</param>
        /// <param name="hashes">The hashes.</param>
        /// <returns>Int64.</returns>
        public static Int64 Combine(Int64 h, params Int64[] hashes)
        {
            var v = h;

            foreach (var i in hashes)
            {
                v = CombineCore(v, i);
            }

            return v;
        }

        /// <summary>
        /// Combines the specified h.
        /// </summary>
        /// <param name="h">The h.</param>
        /// <param name="hashes">The hashes.</param>
        /// <returns>UInt64.</returns>
        public static UInt64 Combine(UInt64 h, params UInt64[] hashes)
        {
            var v = h;

            foreach (var i in hashes)
            {
                v = CombineCore(v, i);
            }

            return v;
        }
        #endregion

        #region Internals
        /// <summary>
        /// Combines the hash codes.
        /// </summary>
        /// <param name="h1">The h1.</param>
        /// <param name="h2">The h2.</param>
        /// <returns>System.Int32.</returns>
        private static Int32 CombineCore(Int32 h1, Int32 h2)
        {
            // this is where the magic happens
            return (((h1 << 5) + h1) ^ h2);
        }

        /// <summary>
        /// Combines the hash codes.
        /// </summary>
        /// <param name="h1">The h1.</param>
        /// <param name="h2">The h2.</param>
        /// <returns>System.UInt32.</returns>
        private static UInt32 CombineCore(UInt32 h1, UInt32 h2)
        {
            // this is where the magic happens
            return (((h1 << 5) + h1) ^ h2);
        }

        /// <summary>
        /// Combines the hash codes.
        /// </summary>
        /// <param name="h1">The h1.</param>
        /// <param name="h2">The h2.</param>
        /// <returns>System.Int64.</returns>
        private static Int64 CombineCore(Int64 h1, Int64 h2)
        {
            // this is where the magic happens
            return (((h1 << 5) + h1) ^ h2);
        }

        /// <summary>
        /// Combines the hash codes.
        /// </summary>
        /// <param name="h1">The h1.</param>
        /// <param name="h2">The h2.</param>
        /// <returns>System.UInt64.</returns>
        private static UInt64 CombineCore(UInt64 h1, UInt64 h2)
        {
            // this is where the magic happens
            return (((h1 << 5) + h1) ^ h2);
        }
        #endregion
    }
}
