using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;

namespace BuildPipeline.Core.Services.EnvironmentServices
{
    /// <summary>
    /// Class AndroidNDKInstallation.
    /// Implements the <see cref="IAndroidNDKInstallation" />
    /// </summary>
    /// <seealso cref="IAndroidNDKInstallation" />
    internal class AndroidNDKInstallation : IAndroidNDKInstallation
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
        /// Initializes a new instance of the <see cref="AndroidNDKInstallation"/> class.
        /// </summary>
        /// <param name="installation">The installation.</param>
        /// <param name="environmentVariable">The environment variable.</param>
        public AndroidNDKInstallation(string installation, string environmentVariable = null)
        {
            Installation = installation;
            EnvironmentName = environmentVariable;
        }
    }

    /// <summary>
    /// Class AndroidNDKEnvironmentService.
    /// Implements the <see cref="AbstractEnvironmentService" />
    /// Implements the <see cref="IAndroidNDKEnvironmentService" />
    /// </summary>
    /// <seealso cref="AbstractEnvironmentService" />
    /// <seealso cref="IAndroidNDKEnvironmentService" />
    [Export]
    internal class AndroidNDKEnvironmentService : AbstractEnvironmentService, IAndroidNDKEnvironmentService
    {
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Name => "AndroidNDK";

        /// <summary>
        /// All installations core
        /// </summary>
        readonly List<IAndroidNDKInstallation> AllInstallationsCore = new List<IAndroidNDKInstallation>();

        /// <summary>
        /// Gets all installations.
        /// </summary>
        /// <value>All installations.</value>
        public IAndroidNDKInstallation[] AllInstallations => AllInstallationsCore.ToArray();

        /// <summary>
        /// Checks this instance.
        /// </summary>
        public override bool CheckService()
        {
            CheckFromEnvironment("NDK_ROOT");
            CheckFromEnvironment("ANDROID_NDK_ROOT");

            InstallationPath = AllInstallationsCore.FirstOrDefault()?.Installation;

            return AllInstallationsCore.Count > 0;            
        }

        /// <summary>
        /// Checks from environment.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        private void CheckFromEnvironment(string variableName)
        {
            var value = Environment.GetEnvironmentVariable(variableName);

            CheckDirectory(value, variableName);
        }

        /// <summary>
        /// Checks the directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="envVar">The env variable.</param>
        private void CheckDirectory(string path, string envVar = null)
        {
            if (path.IsDirectoryExists() &&
                path.JoinPath("toolchains").IsDirectoryExists() &&
                (path.JoinPath("ndk-build").IsFileExists() || path.JoinPath("ndk-build.cmd").IsFileExists())
                )
            {
                AllInstallationsCore.Add(new AndroidNDKInstallation(path, envVar));
            }
        }

        /// <summary>
        /// Gets the help.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string GetHelp()
        {
            return "You must install Android NDK, and create a environment variable NDK_ROOT for it.\n see also:\n" +
                "https://developer.android.com/ndk/downloads?hl=zh-cn";
        }
    }
}
