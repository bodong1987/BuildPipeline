using BuildPipeline.Core.Framework;
using Microsoft.Win32;
using System.Runtime.Versioning;

namespace BuildPipeline.Core.Services.EnvironmentServices
{
    /// <summary>
    /// Class DotnetFrameworkEnvironmentService.
    /// Implements the <see cref="AbstractEnvironmentService" />
    /// Implements the <see cref="IDotnetFrameworkEnvironmentService" />
    /// </summary>
    /// <seealso cref="AbstractEnvironmentService" />
    /// <seealso cref="IDotnetFrameworkEnvironmentService" />
    [Export]
    internal class DotnetFrameworkEnvironmentService : AbstractEnvironmentService, IDotnetFrameworkEnvironmentService
    {
        /// <summary>
        /// All versions core
        /// </summary>
        readonly List<Version> AllVersionsCore = new List<Version>();

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Name => ".NET Framework";

        /// <summary>
        /// Gets all versions.
        /// </summary>
        /// <value>All versions.</value>
        public Version[] AllVersions => AllVersionsCore.ToArray();

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
                Get1To45VersionFromRegistry();
                Get45PlusFromRegistry();

                if (AllVersionsCore.Count > 0)
                {
                    AllVersionsCore.Sort((x, y) =>
                    {
                        return Comparer<Version>.Default.Compare(y, x);
                    });

                    InstallationPath = "";
                    Version = AllVersionsCore.FirstOrDefault();

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Writes the version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="serverPack">The server pack.</param>
        private void WriteVersion(string version, string serverPack = "")
        {
            if (Version.TryParse(version, out var v))
            {
                AllVersionsCore.Add(v);
            }
        }

        /// <summary>
        /// Get1s the to45 version from registry.
        /// </summary>
        [SupportedOSPlatform("windows")]
        private void Get1To45VersionFromRegistry()
        {
            // Opens the registry key for the .NET Framework entry.
            using (RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
            {
                foreach (string versionKeyName in ndpKey.GetSubKeyNames())
                {
                    // Skip .NET Framework 4.5 version information.
                    if (versionKeyName == "v4")
                    {
                        continue;
                    }

                    if (versionKeyName.StartsWith("v"))
                    {

                        RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                        // Get the .NET Framework version value.
                        string name = (string)versionKey.GetValue("Version", "");
                        // Get the service pack (SP) number.
                        string sp = versionKey.GetValue("SP", "").ToString();

                        // Get the installation flag, or an empty string if there is none.
                        string install = versionKey.GetValue("Install", "").ToString();
                        if (string.IsNullOrEmpty(install)) // No install info; it must be in a child subkey.
                            WriteVersion(name);
                        else
                        {
                            if (!string.IsNullOrEmpty(sp) && install == "1")
                            {
                                WriteVersion(name, sp);
                            }
                        }
                        if (!string.IsNullOrEmpty(name))
                        {
                            continue;
                        }
                        foreach (string subKeyName in versionKey.GetSubKeyNames())
                        {
                            RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                            name = (string)subKey.GetValue("Version", "");
                            if (!string.IsNullOrEmpty(name))
                                sp = subKey.GetValue("SP", "").ToString();

                            install = subKey.GetValue("Install", "").ToString();
                            if (string.IsNullOrEmpty(install)) //No install info; it must be later.
                                WriteVersion(name);
                            else
                            {
                                if (!string.IsNullOrEmpty(sp) && install == "1")
                                {
                                    WriteVersion(name, sp);
                                }
                                else if (install == "1")
                                {
                                    WriteVersion(name);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get45s the plus from registry.
        /// </summary>
        [SupportedOSPlatform("windows")]
        private void Get45PlusFromRegistry()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            using (RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey(subkey))
            {
                if (ndpKey == null)
                    return;
                //First check if there's an specific version indicated
                if (ndpKey.GetValue("Version") != null)
                {
                    WriteVersion(ndpKey.GetValue("Version").ToString());
                }
                else
                {
                    if (ndpKey.GetValue("Release") != null)
                    {
                        WriteVersion(
                            CheckFor45PlusVersion(
                                    (int)ndpKey.GetValue("Release")
                                )
                        );
                    }
                }
            }

            // Checking the version using >= enables forward compatibility.
            string CheckFor45PlusVersion(int releaseKey)
            {
                if (releaseKey >= 533325)
                    return "4.8.1";
                if (releaseKey >= 528040)
                    return "4.8";
                if (releaseKey >= 461808)
                    return "4.7.2";
                if (releaseKey >= 461308)
                    return "4.7.1";
                if (releaseKey >= 460798)
                    return "4.7";
                if (releaseKey >= 394802)
                    return "4.6.2";
                if (releaseKey >= 394254)
                    return "4.6.1";
                if (releaseKey >= 393295)
                    return "4.6";
                if (releaseKey >= 379893)
                    return "4.5.2";
                if (releaseKey >= 378675)
                    return "4.5.1";
                if (releaseKey >= 378389)
                    return "4.5";
                // This code should never execute. A non-null release key should mean
                // that 4.5 or later is installed.
                return "";
            }
        }

        public override string GetHelp()
        {
            return "Please install .NET Framework, see also:\n" +
                "    https://dotnet.microsoft.com/en-us/download/dotnet-framework";
        }
    }
}
