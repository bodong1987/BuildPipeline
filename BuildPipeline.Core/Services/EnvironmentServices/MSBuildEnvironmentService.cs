using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using Microsoft.Win32;
using System.Runtime.Versioning;

namespace BuildPipeline.Core.Services.EnvironmentServices
{
    /// <summary>
    /// Interface MSBuildInstallation
    /// Implements the <see cref="IMSBuildInstallation" />
    /// </summary>
    /// <seealso cref="IMSBuildInstallation" />
    internal class MSBuildInstallation : IMSBuildInstallation
    {
        /// <summary>
        /// Gets the ms build.
        /// </summary>
        /// <value>The ms build.</value>
        public string MsBuild { get; set; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        public Version Version { get; set; }
    }

    /// <summary>
    /// Class MSBuildEnvironmentService.
    /// Implements the <see cref="AbstractEnvironmentService" />
    /// Implements the <see cref="IMSBuildEnvironmentService" />
    /// </summary>
    /// <seealso cref="AbstractEnvironmentService" />
    /// <seealso cref="IMSBuildEnvironmentService" />
    [Export]
    internal class MSBuildEnvironmentService : AbstractEnvironmentService, IMSBuildEnvironmentService
    {
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Name => "MSBuild";

        readonly List<IMSBuildInstallation> InstallationsCore = new List<IMSBuildInstallation>();

        /// <summary>
        /// Gets the installations.
        /// </summary>
        /// <value>The installations.</value>
        public IMSBuildInstallation[] Installations => InstallationsCore.ToArray();

        /// <summary>
        /// Checks this instance.
        /// </summary>
        public override bool CheckService()
        {
            if (OperatingSystem.IsWindows())
            {
                FindMSBuildAlongWithVisualStudio();
                FindMSBuildInProgramFiles();
            }

            FindMSBuildInEnvironmentVariable();

            if (InstallationsCore.Count > 0)
            {
                InstallationsCore.Sort((x, y) =>
                {
                    return Comparer<Version>.Default.Compare(y.Version, x.Version);
                });
                                
                InstallationPath = InstallationsCore.FirstOrDefault()?.MsBuild?.GetDirectoryPath();
                Version = InstallationsCore.FirstOrDefault()?.Version;

                return true;
            }

            return false;
        }

        [SupportedOSPlatform("windows")]
        private void FindMSBuildAlongWithVisualStudio()
        {
            string[] Entries = new string[]
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\"
            };

            foreach(var entry in Entries)
            {
                try
                {
                    using (var UninstallKeys = Registry.LocalMachine.OpenSubKey(entry))
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
                                        var vspath = path;

                                        string rpath = "MSBuild/Current/Bin/MSBuild.exe";
                                        string tpath = vspath.JoinPath(rpath);

                                        if (tpath.IsFileExists())
                                        {
                                            InstallationsCore.Add(new MSBuildInstallation()
                                            {
                                                MsBuild = tpath,
                                                Version = GetMSBuildVersion(tpath)
                                            });

                                            continue;
                                        }

                                        if (Version.TryParse(version, out var v))
                                        {
                                            tpath = vspath.JoinPath($"MSBuild/{v.Major}.0/Bin/MSBuild.exe");

                                            if (tpath.IsFileExists())
                                            {
                                                InstallationsCore.Add(new MSBuildInstallation()
                                                {
                                                    MsBuild = tpath,
                                                    Version = GetMSBuildVersion(tpath)
                                                });

                                                continue;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    Logger.LogError("FindMSBuildAlongWithVisualStudio Exception: {0}\n{1}", e.Message, e.StackTrace);
                }
            }
        }

        [SupportedOSPlatform("windows")]
        private void FindMSBuildInProgramFiles()
        {
            string[] rootPathes = new string[]
            {
                Environment.ExpandEnvironmentVariables("%ProgramW6432%"),
                Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%")
            };

            foreach (var root in rootPathes)
            {
                string path = root.JoinPath("MSBuild");

                if (path.IsDirectoryExists())
                {
                    var dirs = Directory.GetDirectories(path);

                    foreach (var dir in dirs)
                    {
                        var p = dir.JoinPath("Bin/MSBuild.exe");

                        if (p.IsFileExists())
                        {
                            InstallationsCore.Add(new MSBuildInstallation()
                            {
                                MsBuild = p,
                                Version = GetMSBuildVersion(p)
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds the ms build in environment variable.
        /// </summary>
        private void FindMSBuildInEnvironmentVariable()
        {
            var path = EnvironmentUtils.SearchExecutableFile("MSBuild");

            if (path.IsFileExists())
            {
                InstallationsCore.Add(new MSBuildInstallation()
                {
                    MsBuild = path,
                    Version = GetMSBuildVersion(path)
                });
            }
        }

        private Version GetMSBuildVersion(string path)
        {
            if (!path.IsFileExists())
            {
                return null;
            }

            string text = ExternalProcessUtils.InvokeAndGetOutput(path, " /version");

            var lines = text.Split('\n');
            foreach (var i in lines.Reverse())
            {
                if (Version.TryParse(i.Trim(), out var v))
                {
                    return v;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the help.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string GetHelp()
        {
            return "You can install MSbuild along with Visual Studio:\n" +
                "https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild \n" +
                "https://visualstudio.microsoft.com/";
        }
    }
}
