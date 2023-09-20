using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using Claunia.PropertyList;
using PropertyModels.Extensions;
using System.Runtime.Versioning;

namespace BuildPipeline.Core.Services.EnvironmentServices
{
    /// <summary>
    /// Class XCodeEnvironmentService.
    /// Implements the <see cref="AbstractEnvironmentService" />
    /// Implements the <see cref="IXCodeEnvironmentService" />
    /// </summary>
    /// <seealso cref="AbstractEnvironmentService" />
    /// <seealso cref="IXCodeEnvironmentService" />
    [Export]
    internal class XCodeEnvironmentService : AbstractEnvironmentService, IXCodeEnvironmentService
    {
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Name => "XCode";

        /// <summary>
        /// Gets the x code build path.
        /// </summary>
        /// <value>The x code build path.</value>
        public string XCodeBuildPath { get; private set; }

        /// <summary>
        /// Gets the x code build version.
        /// </summary>
        /// <value>The x code build version.</value>
        public Version XCodeBuildVersion { get; private set; }

        /// <summary>
        /// Accepts the specified access token.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool Accept(object accessToken)
        {
            return OperatingSystem.IsMacOS();
        }

        /// <summary>
        /// Checks this instance.
        /// </summary>
        public override bool CheckService()
        {
            if (!OperatingSystem.IsMacOS())
            {
                return false;
            }

            CheckXCodeApp();
            CheckXCodeBuild();

            // try merge if need
            if (Version == null)
            {
                Version = XCodeBuildVersion;
            }

            return InstallationPath.IsNotNullOrEmpty();
        }

        /// <summary>
        /// Checks the x code application.
        /// </summary>
        [SupportedOSPlatform("macos")]
        private void CheckXCodeApp()
        {
            string xcode = "/Applications/Xcode.app/Contents/MacOS/Xcode";
            if (!xcode.IsFileExists())
            {
                return;
            }

            InstallationPath = xcode.GetDirectoryPath().JoinPath("../../");

            // read version from version.plist
            string versionPlist = InstallationPath.JoinPath("Contents/version.plist");

            if (versionPlist.IsFileExists())
            {
                FileInfo fi = new FileInfo(versionPlist);

                NSDictionary root = (NSDictionary)PropertyListParser.Parse(fi);

                var versionText = root.ObjectForKey("CFBundleShortVersionString")?.ToString();

                if (Version.TryParse(versionText, out var v))
                {
                    Version = v;
                }
            }
        }

        /// <summary>
        /// Checks the x code build.
        /// </summary>
        [SupportedOSPlatform("macos")]
        private void CheckXCodeBuild()
        {
            string path = EnvironmentUtils.SearchExecutableFile("xcodebuild");

            if (!path.IsFileExists())
            {
                Logger.LogWarning("Failed find xcodebuild");
                return;
            }

            XCodeBuildPath = path;

            string text = ExternalProcessUtils.InvokeAndGetOutput(path, " -version");

            if (!text.iStartsWith("Xcode"))
            {
                return;
            }

            string version = text.Substring(0, text.IndexOf('\n')).Substring("Xcode".Length).Trim();
            if (Version.TryParse(version, out var ver))
            {
                XCodeBuildVersion = ver;
            }
        }

        /// <summary>
        /// Gets the help.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string GetHelp()
        {
            return "You must install Xcode and start it at least once so that Xcode can successfully install related dependencies, see also:\n" +
                "    https://developer.apple.com/download/all/";
        }
    }
}
