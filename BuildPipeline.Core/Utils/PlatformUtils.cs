namespace BuildPipeline.Core.Utils
{
    /// <summary>
    /// Enum PlatformType
    /// </summary>
    public enum PlatformType
    {
        /// <summary>
        /// The unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// The windows
        /// </summary>
        Windows,

        /// <summary>
        /// The mac os
        /// </summary>
        MacOS
    }

    /// <summary>
    /// Class PlatformUtils.
    /// </summary>
    public class PlatformUtils
    {
        /// <summary>
        /// Determines whether this instance is windows.
        /// </summary>
        /// <returns><c>true</c> if this instance is windows; otherwise, <c>false</c>.</returns>
        public static bool IsWindows()
        {
            return OperatingSystem.IsWindows();
        }

        /// <summary>
        /// Determines whether [is mac os].
        /// </summary>
        /// <returns><c>true</c> if [is mac os]; otherwise, <c>false</c>.</returns>
        public static bool IsMacOS()
        {
            return OperatingSystem.IsMacOS();
        }

        /// <summary>
        /// Gets the platform.
        /// </summary>
        /// <returns>PlatformType.</returns>
        public static PlatformType GetPlatform()
        {
            if(IsWindows())
            {
                return PlatformType.Windows;
            }
            else if (IsMacOS())
            {
                return PlatformType.MacOS;
            }

            return PlatformType.Unknown;
        }

        /// <summary>
        /// Determines whether the specified platform type is platform.
        /// </summary>
        /// <param name="platformType">Type of the platform.</param>
        /// <returns><c>true</c> if the specified platform type is platform; otherwise, <c>false</c>.</returns>
        public static bool IsPlatform(PlatformType platformType)
        {
            return GetPlatform() == platformType;
        }
    }
}
