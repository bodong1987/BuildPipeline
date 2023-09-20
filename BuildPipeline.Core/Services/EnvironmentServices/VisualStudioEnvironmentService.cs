using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using Claunia.PropertyList; // for macos info.plist parser
using Microsoft.Win32;
using PropertyModels.Extensions;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace BuildPipeline.Core.Services.EnvironmentServices
{
    /// <summary>
    /// Class VisualStudioInstallation.
    /// Implements the <see cref="IVisualStudioInstallation" />
    /// </summary>
    /// <seealso cref="IVisualStudioInstallation" />
    internal class VisualStudioInstallation : IVisualStudioInstallation
    {
        /// <summary>
        /// Gets the unique identifier.
        /// </summary>
        /// <value>The unique identifier.</value>
        public string Guid { get; set; }

        /// <summary>
        /// Gets the installation.
        /// </summary>
        /// <value>The installation.</value>
        public string Installation { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        public Version Version { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is visual CPP installed.
        /// </summary>
        /// <value><c>true</c> if this instance is visual CPP installed; otherwise, <c>false</c>.</value>
        public bool IsVisualCPPInstalled { get; set; }

        public VisualStudioType VSType
        {
            get
            {
                if(OperatingSystem.IsMacOS())
                {
                    return VisualStudioType.VS_ForMac;
                }

                if(Version == null)
                {
                    return VisualStudioType.VS_Unknown;
                }

                int MajorVersion = Version.Major;

                if(MajorVersion > 17)
                {
                    return VisualStudioType.VS_Lastest;
                }

                switch (MajorVersion)
                {
                    case 17:
                        return VisualStudioType.VS_2022;
                    case 16:
                        return VisualStudioType.VS_2019;
                    case 15:
                        return VisualStudioType.VS_2017;
                    case 14:
                        return VisualStudioType.VS_2015;
                    case 12:
                        return VisualStudioType.VS_2013;
                    case 11:
                        return VisualStudioType.VS_2012;
                    case 10:
                        return VisualStudioType.VS_2010;
                    default:
                        return VisualStudioType.VS_Unknown;
                }
            }
        }

        public string FriendlyName
        {
            get
            {
                switch (VSType)
                {
                    case VisualStudioType.VS_ForMac:
                        return "Visual Studio for Mac";
                    default:
                        {
                            var str = VSType.ToString();
                            str = str.Substring(str.IndexOf('_') + 1);

                            return $"Microsoft Visual Studio {str}";
                        }
                }
            }
        }

        public string CMakeTargetName
        {
            get
            {
                if(VSType == VisualStudioType.VS_ForMac || VSType == VisualStudioType.VS_Unknown)
                {
                    return "";
                }

                var str = VSType.ToString();
                str = str.Substring(str.IndexOf('_') + 1);

                return $"Visual Studio {(Version != null ? Version.Major:17)} {str}";
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return FriendlyName;
        }
    }

    /// <summary>
    /// Class VisualStudioEnvironmentService.
    /// Implements the <see cref="AbstractEnvironmentService" />
    /// Implements the <see cref="IVisualStudioEnvironmentService" />
    /// </summary>
    /// <seealso cref="AbstractEnvironmentService" />
    /// <seealso cref="IVisualStudioEnvironmentService" />
    [Export]
    internal class VisualStudioEnvironmentService : AbstractEnvironmentService, IVisualStudioEnvironmentService
    {
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Name => "Visual Studio";

        /// <summary>
        /// The installations core
        /// </summary>
        readonly List<VisualStudioInstallation> InstallationsCore = new List<VisualStudioInstallation>();

        /// <summary>
        /// Gets the installations.
        /// </summary>
        /// <value>The installations.</value>
        public IVisualStudioInstallation[] Installations => InstallationsCore.ToArray();

        /// <summary>
        /// Gets the prefer installation.
        /// </summary>
        /// <value>The prefer installation.</value>
        public IVisualStudioInstallation PreferInstallation { get; private set; }

        /// <summary>
        /// Accepts the specified access token.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool Accept(object accessToken)
        {
            return OperatingSystem.IsWindows() || OperatingSystem.IsMacOS();
        }

        /// <summary>
        /// Checks this instance.
        /// </summary>
        public override bool CheckService()
        {
            if (OperatingSystem.IsWindows())
            {
                CheckVisualStudioInstallOnWindows();
            }
            else if (OperatingSystem.IsMacOS())
            {
                CheckVisualStudioForMac();
            }

            return InstallationPath.IsNotNullOrEmpty();
        }
        #region MacOS
        /// <summary>
        /// Checks the visual studio for mac.
        /// </summary>
        [SupportedOSPlatform("macos")]
        private void CheckVisualStudioForMac()
        {
            string path = Path.Combine("/Applications/Visual Studio.app/Contents/MacOS/VisualStudio");

            if (!path.IsFileExists())
            {
                return;
            }

            InstallationPath = path.GetDirectoryPath();

            string plistPath = InstallationPath.JoinPath("../Info.plist");
            if (plistPath.IsFileExists())
            {
                try
                {
                    NSDictionary root = (NSDictionary)PropertyListParser.Parse(new FileInfo(plistPath));

                    string version = root.ObjectForKey("CFBundleShortVersionString").ToString();

                    if (Version.TryParse(version, out var v))
                    {
                        Version = v;
                    }
                }
                catch (Exception e)
                {
                    Logger.LogWarning("Failed get Visual Studio for Mac version from file:{0}\n{1}", plistPath, e.Message);
                }
            }
        }
        #endregion

        #region Windows
        /// <summary>
        /// Checks the visual studio install on windows.
        /// </summary>
        [SupportedOSPlatform("windows")]
        private void CheckVisualStudioInstallOnWindows()
        {
            // search in regstry
            using (var UninstallKeys = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\"))
            {
                if (UninstallKeys != null)
                {
                    foreach (var item in UninstallKeys.GetSubKeyNames())
                    {
                        using (var key = UninstallKeys.OpenSubKey(item))
                        {
                            var path = key.GetValue("InstallLocation") as string;
                            var version = key.GetValue("DisplayVersion") as string;

                            if (!string.IsNullOrEmpty(path) &&
                                !string.IsNullOrEmpty(version) &&
                                path.Contains("Microsoft Visual Studio")
                                )
                            {
                                VisualStudioInstallation installation = new VisualStudioInstallation();
                                installation.Guid = item;
                                installation.Installation = path;
                                installation.Name = key.GetValue("DisplayName") as string ?? "";

                                if (Version.TryParse(version, out var v))
                                {
                                    installation.Version = v;
                                }

                                if (path.JoinPath("VC/bin/cl.exe").IsFileExists() ||
                                    path.JoinPath("VC/Tools/MSVC").IsDirectoryExists())
                                {
                                    installation.IsVisualCPPInstalled = true;
                                }

                                InstallationsCore.Add(installation);
                            }
                        }
                    }
                }
            }

            // search in program files
            CheckInProgramFiles();

            InstallationsCore.Sort((x, y) =>
            {
                return Comparer<Version>.Default.Compare(x.Version, y.Version) * -1;
            });

            PreferInstallation = InstallationsCore.FirstOrDefault();

            if (PreferInstallation != null)
            {
                InstallationPath = PreferInstallation.Installation;
                Version = PreferInstallation.Version;
            }
        }

        [SupportedOSPlatform("windows")]
        private void CheckInProgramFiles()
        {
            string programFiles = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");

            if(!programFiles.IsDirectoryExists())
            {
                return;
            }

            var dirs = Directory.GetDirectories(programFiles, "Microsoft Visual Studio*");

            foreach(var dir in dirs)
            {
                var path = dir.JoinPath("Common7/IDE/devenv.exe");

                if(!path.IsFileExists())
                {
                    continue;
                }

                // is exists in restry.
                if(InstallationsCore.Find(x=>x.Installation.TrimEnd('\\', '/') == dir.TrimEnd('\\', '/')) != null)
                {
                    continue;
                }

                var versionInfo = FileVersionInfo.GetVersionInfo(path);

                VisualStudioInstallation installation = new VisualStudioInstallation();
                installation.Guid = null;
                installation.Installation = dir;
                installation.Name = versionInfo.FileDescription;

                if (Version.TryParse(versionInfo.ProductVersion, out var v))
                {
                    installation.Version = v;
                }

                if (path.JoinPath("VC/bin/cl.exe").IsFileExists() ||
                    path.JoinPath("VC/Tools/MSVC").IsDirectoryExists())
                {
                    installation.IsVisualCPPInstalled = true;
                }

                InstallationsCore.Add(installation);
            }
        }

        #endregion

        /// <summary>
        /// Gets the help.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string GetHelp()
        {
            return "You need install Visual studio first. see also: https://visualstudio.microsoft.com/downloads/";
        }
    }
}
