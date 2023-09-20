using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Framework;
using Microsoft.Win32;
using PropertyModels.Extensions;
using System.Runtime.Versioning;

namespace BuildPipeline.Core.Services.EnvironmentServices
{
    /// <summary>
    /// Class WindowsSDKEnvironmentService.
    /// Implements the <see cref="AbstractEnvironmentService" />
    /// Implements the <see cref="IWindowsSDKEnvironmentService" />
    /// </summary>
    /// <seealso cref="AbstractEnvironmentService" />
    /// <seealso cref="IWindowsSDKEnvironmentService" />
    [Export]
    internal class WindowsSDKEnvironmentService : AbstractEnvironmentService, IWindowsSDKEnvironmentService
    {
        /// <summary>
        /// All versions core
        /// </summary>
        readonly List<Version> AllVersionsCore = new List<Version>();

        /// <summary>
        /// Gets all versions.
        /// </summary>
        /// <value>All versions.</value>
        public Version[] AllVersions => AllVersionsCore.ToArray();

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Name => "Windows SDK";

        /// <summary>
        /// Accepts the specified access token.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool Accept(object accessToken)
        {
            return OperatingSystem.IsWindows();
        }

        /// <summary>
        /// Checks this instance.
        /// </summary>
        public override bool CheckService()
        {
            if (OperatingSystem.IsWindows())
            {
                GetWindowsSDKInfoFromRegistry();
            }

            return AllVersionsCore.Count > 0;
        }

        /// <summary>
        /// Gets the windows SDK information from registry.
        /// </summary>
        [SupportedOSPlatform("windows")]
        private void GetWindowsSDKInfoFromRegistry()
        {
            using (var UninstallKeys = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\"))
            {
                if (UninstallKeys != null)
                {
                    foreach (var item in UninstallKeys.GetSubKeyNames())
                    {
                        using (var key = UninstallKeys.OpenSubKey(item))
                        {
                            var displayName = key.GetValue("DisplayName") as string;

                            if (displayName.IsNotNullOrEmpty() && displayName.iStartsWith("Windows Software Development Kit - Windows"))
                            {
                                var version = key.GetValue("DisplayVersion") as string;

                                if (version.IsNotNullOrEmpty() && Version.TryParse(version, out var v))
                                {
                                    AllVersionsCore.Add(v);
                                }
                            }
                        }
                    }
                }
            }

            if (AllVersionsCore.Count > 0)
            {
                AllVersionsCore.Sort((x, y) =>
                {
                    return Comparer<Version>.Default.Compare(y, x);
                });

                InstallationPath = "";
                Version = AllVersionsCore.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the help.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string GetHelp()
        {
            return "Please install Windows SDK by Visual Studio Setup, or download from here:\n" +
                "   https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/";
        }
    }
}
