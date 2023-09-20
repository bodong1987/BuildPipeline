using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using Microsoft.Win32;
using System.Runtime.Versioning;

namespace BuildPipeline.Core.Services.EnvironmentServices
{
    /// <summary>
    /// Class SevenZipEnvironmentService.
    /// Implements the <see cref="AbstractEnvironmentService" />
    /// Implements the <see cref="ISevenZipEnvironmentService" />
    /// </summary>
    /// <seealso cref="AbstractEnvironmentService" />
    /// <seealso cref="ISevenZipEnvironmentService" />
    [Export]
    internal class SevenZipEnvironmentService : AbstractEnvironmentService, ISevenZipEnvironmentService
    {
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Name => "7Zip";

        /// <summary>
        /// Gets the seven zip path.
        /// </summary>
        /// <value>The seven zip path.</value>
        public string SevenZipPath { get; private set; }

        public override bool CheckService()
        {
            string[] exeNames = new string[]
            {
                "7z",
                "7zz"
            };

            foreach (string exeName in exeNames)
            {
                string path = EnvironmentUtils.SearchExecutableFile(exeName);

                if (path.IsFileExists())
                {
                    InstallationPath = path.GetDirectoryPath();
                    SevenZipPath = path;

                    Read7ZipVersion();

                    return true;
                }
            }

            if (OperatingSystem.IsWindows())
            {
                if (SearchInRegistry())
                {
                    return true;
                }

                if (SearchInProgramFiles())
                {
                    return true;
                }
            }

            return SearchWithSelfContain();
        }

        private bool SearchWithSelfContain()
        {
            string path = AppFramework.FilePath("tools/7z");

            if(OperatingSystem.IsWindows())
            {
                path = path.JoinPath("Windows/7z.exe");
            }
            else
            {
                if(path.JoinPath("MacOS/7z").IsFileExists())
                {
                    path = path.JoinPath("MacOS/7z");
                }
                else
                {
                    path = path.JoinPath("MacOS/7zz");
                }
            }

            if(!path.IsFileExists())
            {
                return false;
            }

            InstallationPath = path.GetDirectoryPath();
            SevenZipPath = path;

            Read7ZipVersion();

            return true;
        }

        [SupportedOSPlatform("windows")]
        private bool SearchInProgramFiles()
        {
            string[] rootPathes = new string[]
            {
                Environment.ExpandEnvironmentVariables("%ProgramW6432%"),
                Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%")
            };

            foreach (var rootPath in rootPathes)
            {
                var dir = rootPath.JoinPath("7-Zip");

                if (dir.IsDirectoryExists() && dir.JoinPath("7z.exe").IsFileExists())
                {
                    InstallationPath = dir;
                    SevenZipPath = dir.JoinPath("7z.exe");

                    Read7ZipVersion();

                    return true;
                }
            }

            return false;
        }

        [SupportedOSPlatform("windows")]
        private bool SearchInRegistry()
        {
            string[] sourcePathes = new string[]
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\",
            };

            foreach (string sourcePath in sourcePathes)
            {
                using (var UninstallKeys = Registry.LocalMachine.OpenSubKey(sourcePath))
                {
                    if (UninstallKeys != null)
                    {
                        foreach (var item in UninstallKeys.GetSubKeyNames())
                        {
                            using (var key = UninstallKeys.OpenSubKey(item))
                            {
                                var name = key.GetValue("DisplayName") as string;
                                var version = key.GetValue("DisplayVersion") as string;

                                if (!string.IsNullOrEmpty(name) &&
                                    !string.IsNullOrEmpty(version) &&
                                    name.StartsWith("7-Zip")
                                    )
                                {
                                    var path = key.GetValue("InstallLocation") as string;

                                    if (path.IsDirectoryExists() && path.JoinPath("7z.exe").IsFileExists())
                                    {
                                        InstallationPath = path;
                                        SevenZipPath = path.JoinPath("7z.exe");

                                        if (Version.TryParse(version, out var v))
                                        {
                                            Version = v;
                                        }

                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        private void Read7ZipVersion()
        {
            if (SevenZipPath.IsFileExists())
            {
                string text = ExternalProcessUtils.InvokeAndGetOutput(SevenZipPath, "").Trim();

                int index = text.IndexOf(':');

                if(index == -1)
                {
                    return;
                }

                text = text.Substring(0, index);
                index = text.IndexOf('.');

                if(index == -1)
                {
                    return;
                }

                int i1 = text.LastIndexOf(' ', index);
                int i2 = text.IndexOf(' ', index);

                if(i1 != i2 && i1 != -1 && i2 != -1)
                {
                    text = text.Substring(i1, i2 - i1).Trim();

                    if(Version.TryParse(text, out var v))
                    {
                        Version = v;
                    }
                }
            }
        }

        public override string GetHelp()
        {
            return "Please install 7zip, see also:\n" +
                "https://www.7-zip.org/download.html\n" +
                "https://superuser.com/questions/548349/how-can-i-install-7zip-so-i-can-run-it-from-terminal-on-os-x";
        }
    }
}
