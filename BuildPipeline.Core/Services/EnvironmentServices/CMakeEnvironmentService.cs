using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using PropertyModels.Extensions;

namespace BuildPipeline.Core.Services.EnvironmentServices
{
    /// <summary>
    /// Class CMakeEnvironmentService.
    /// Implements the <see cref="AbstractEnvironmentService" />
    /// Implements the <see cref="ICMakeEnvironmentService" />
    /// </summary>
    /// <seealso cref="AbstractEnvironmentService" />
    /// <seealso cref="ICMakeEnvironmentService" />
    [Export]
    internal class CMakeEnvironmentService : AbstractEnvironmentService, ICMakeEnvironmentService
    {
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Name => "CMake";

        /// <summary>
        /// Gets the c make.
        /// </summary>
        /// <value>The c make.</value>
        public string CMake
        {
            get
            {
                if(OperatingSystem.IsWindows())
                {
                    return InstallationPath.JoinPath("cmake.exe");
                }

                return InstallationPath.JoinPath("cmake");
            }
        }

        /// <summary>
        /// Gets the c make target command line.
        /// </summary>
        /// <param name="installation">The installation.</param>
        /// <param name="architectureType">Type of the architecture.</param>
        /// <returns>System.String.</returns>
        public string GetCMakeTargetCommandLine(IVisualStudioInstallation installation, VSArchitectureType architectureType)
        {
            if (installation.VSType == VisualStudioType.VS_ForMac || installation.VSType == VisualStudioType.VS_Unknown)
            {
                return "";
            }

            var str = installation.VSType.ToString();
            str = str.Substring(str.IndexOf('_') + 1);

            if (installation.VSType >= VisualStudioType.VS_2019)
            {
                return $"-G\"Visual Studio {(installation.Version != null ? installation.Version.Major : 17)} {str}\" -A\"{GetPlatformName(installation, architectureType)}\"";
            }
            else
            {
                if(architectureType == VSArchitectureType.Win32)
                {
                    // in these version, win32 is default one
                    return $"-G\"Visual Studio {(installation.Version != null ? installation.Version.Major : 17)} {str}\"";
                }
                else
                {
                    return $"-G\"Visual Studio {(installation.Version != null ? installation.Version.Major : 17)} {str} {GetPlatformName(installation, architectureType)}\"";
                }
            }
        }

        /// <summary>
        /// Gets the name of the platform.
        /// </summary>
        /// <param name="installation">The installation.</param>
        /// <param name="architectureType">Type of the architecture.</param>
        /// <returns>System.String.</returns>
        public string GetPlatformName(IVisualStudioInstallation installation, VSArchitectureType architectureType)
        {
            if (installation.VSType >= VisualStudioType.VS_2019)
            {
                if (architectureType == VSArchitectureType.Win64)
                {
                    return "x64";
                }
                else if (architectureType == VSArchitectureType.Win32)
                {
                    return "x86";
                }
            }

            return architectureType.ToString();
        }

        /// <summary>
        /// Gets the help.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string GetHelp()
        {
            return "Please install cmake and make sure add cmake to Environment Variable $PATH, see also:\n" +
                "https://cmake.org/download/";
        }

        /// <summary>
        /// Checks this instance.
        /// </summary>
        public override bool CheckService()
        {
            var path = EnvironmentUtils.SearchExecutableFile("cmake");

            if (!path.IsFileExists())
            {
                return false;
            }

            InstallationPath = path.GetDirectoryPath();

            ParseVersion(path);

            return true;
        }

        /// <summary>
        /// Parses the version.
        /// </summary>
        /// <param name="path">The path.</param>
        private void ParseVersion(string path)
        {
            var text = ExternalProcessUtils.InvokeAndGetOutput(path, " --version")?.Trim();

            if (text.IsNullOrEmpty())
            {
                return;
            }

            int index = text.IndexOf('\n');

            if (index != -1)
            {
                text = text.Substring(0, index);
            }

            int i1 = text.IndexOf(ch => { return char.IsDigit(ch); });
            if (i1 == -1)
            {
                return;
            }

            text = text.Substring(i1).Trim();

            // support for rc version like this: 
            /*
             * cmake version 3.26.0-rc2
                CMake suite maintained and supported by Kitware (kitware.com/cmake).
             */
            int i2 = text.IndexOf(ch => { return !char.IsDigit(ch) && ch != '.'; });

            if (i2 != -1)
            {
                text = text.Substring(0, i2).Trim();
            }

            if (Version.TryParse(text, out var version))
            {
                Version = version;
            }
        }
    }
}
