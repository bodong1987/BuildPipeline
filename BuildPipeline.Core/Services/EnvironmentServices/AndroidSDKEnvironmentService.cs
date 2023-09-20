using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using System.Runtime.Versioning;

namespace BuildPipeline.Core.Services.EnvironmentServices
{
    /// <summary>
    /// Class AndroidSDKInstallation.
    /// Implements the <see cref="IAndroidSDKInstallation" />
    /// </summary>
    /// <seealso cref="IAndroidSDKInstallation" />
    internal class AndroidSDKInstallation : IAndroidSDKInstallation
    {
        /// <summary>
        /// Gets a value indicating whether this instance is found by environment variable.
        /// </summary>
        /// <value><c>true</c> if this instance is found by environment variable; otherwise, <c>false</c>.</value>
        public bool IsFoundByEnvironmentVariable => EnvironmentName != null;

        /// <summary>
        /// Gets the name of the environment.
        /// </summary>
        /// <value>The name of the environment.</value>
        public string EnvironmentName { get; private set; }

        /// <summary>
        /// Gets the installation.
        /// </summary>
        /// <value>The installation.</value>
        public string Installation { get; private set; }

        /// <summary>
        /// The API levels core
        /// </summary>
        readonly List<string> APILevelsCore = new List<string>();
        /// <summary>
        /// The build tools core
        /// </summary>
        readonly List<string> BuildToolsCore = new List<string>();

        /// <summary>
        /// Gets the API levels.
        /// </summary>
        /// <value>The API levels.</value>
        public string[] APILevels => APILevelsCore.ToArray();

        /// <summary>
        /// Gets the build tools.
        /// </summary>
        /// <value>The build tools.</value>
        public string[] BuildTools => BuildToolsCore.ToArray();

        /// <summary>
        /// Initializes a new instance of the <see cref="AndroidSDKInstallation" /> class.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="envVariableName">Name of the env variable.</param>
        public AndroidSDKInstallation(string directory, string envVariableName = null)
        {
            Installation = directory;
            EnvironmentName = envVariableName;

            string buildtoolsPath = directory.JoinPath("build-tools");
            if (buildtoolsPath.IsDirectoryExists())
            {
                BuildToolsCore.AddRange(Directory.GetDirectories(buildtoolsPath).Select(x => x.GetFileName()));
            }

            string platforms = directory.JoinPath("platforms");
            if (platforms.IsDirectoryExists())
            {
                APILevelsCore.AddRange(Directory.GetDirectories(platforms).Select(x => x.GetFileName()));
            }
        }
    }

    /// <summary>
    /// Class AndroidSDKEnvironmentService.
    /// Implements the <see cref="AbstractEnvironmentService" />
    /// Implements the <see cref="IAndroidSDKEnvironmentService" />
    /// </summary>
    /// <seealso cref="AbstractEnvironmentService" />
    /// <seealso cref="IAndroidSDKEnvironmentService" />
    [Export]
    internal class AndroidSDKEnvironmentService : AbstractEnvironmentService, IAndroidSDKEnvironmentService
    {
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Name => "AndroidSDK";

        /// <summary>
        /// All installations core
        /// </summary>
        readonly List<IAndroidSDKInstallation> AllInstallationsCore = new List<IAndroidSDKInstallation>();
        /// <summary>
        /// Gets all installations.
        /// </summary>
        /// <value>All installations.</value>
        public IAndroidSDKInstallation[] AllInstallations => AllInstallationsCore.ToArray();

        /// <summary>
        /// Checks this instance.
        /// </summary>
        public override bool CheckService()
        {
            CheckByEnvironmentVariable("ANDROID_SDK_ROOT");
            CheckByEnvironmentVariable("ANDROID_HOME");

            if (OperatingSystem.IsWindows())
            {
                CheckFormWindows();
            }
            else if (OperatingSystem.IsMacOS())
            {
                CheckForMacOS();
            }

            InstallationPath = AllInstallations.FirstOrDefault()?.Installation;

            return AllInstallationsCore.Count > 0;
        }

        /// <summary>
        /// Checks the by environment variable.
        /// </summary>
        /// <param name="environmentVariable">The environment variable.</param>
        private void CheckByEnvironmentVariable(string environmentVariable)
        {
            var value = Environment.GetEnvironmentVariable(environmentVariable);

            CheckDirectory(value);
        }

        /// <summary>
        /// Checks from program data.
        /// </summary>
        [SupportedOSPlatform("windows")]
        private void CheckFormWindows()
        {
            string programFiles = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
            string programFilesX86 = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");

            CheckInWindowsProgramFiles(programFiles);
            CheckInWindowsProgramFiles(programFilesX86);

            // add more default install locations ???
        }

        /// <summary>
        /// Checks the in path.
        /// </summary>
        /// <param name="directory">The directory.</param>
        [SupportedOSPlatform("windows")]
        private void CheckInWindowsProgramFiles(string directory)
        {
            if (!directory.IsDirectoryExists())
            {
                return;
            }

            CheckDirectory(directory.JoinPath("Android/android-sdk"));
        }

        /// <summary>
        /// Determines whether [is android SDK directory] [the specified directory].
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns><c>true</c> if [is android SDK directory] [the specified directory]; otherwise, <c>false</c>.</returns>
        private bool IsAndroidSDKDirectory(string directory)
        {
            return directory.IsDirectoryExists() &&
                directory.JoinPath("platform-tools").IsDirectoryExists();
        }

        /// <summary>
        /// Checks the directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        private void CheckDirectory(string directory)
        {
            if (IsAndroidSDKDirectory(directory))
            {
                AllInstallationsCore.Add(new AndroidSDKInstallation(directory));
            }
        }

        [SupportedOSPlatform("macos")]
        private void CheckForMacOS()
        {
            CheckDirectory("${HOME}/Library/Android/sdk");

            // add more default install locations ???

        }

        /// <summary>
        /// Gets the help.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string GetHelp()
        {
            return "You must install Android SDK first, see also:\n" +
                "https://developer.android.com/studio";
        }
    }
}
