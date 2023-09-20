using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using PropertyModels.Extensions;

namespace BuildPipeline.Core.Services.EnvironmentServices
{
    /// <summary>
    /// Class JavaEnvironmentService.
    /// Implements the <see cref="AbstractEnvironmentService" />
    /// Implements the <see cref="IJavaEnvironmentService" />
    /// </summary>
    /// <seealso cref="AbstractEnvironmentService" />
    /// <seealso cref="IJavaEnvironmentService" />
    [Export]
    internal class JavaEnvironmentService : AbstractEnvironmentService, IJavaEnvironmentService
    {
        #region Properties
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Name => "Java";

        /// <summary>
        /// Gets the java path.
        /// </summary>
        /// <value>The java path.</value>
        public string JavaPath { get; private set; }

        /// <summary>
        /// Gets the java c path.
        /// </summary>
        /// <value>The java c path.</value>
        public string JavaCPath { get; private set; }

        /// <summary>
        /// Gets the java version.
        /// </summary>
        /// <value>The java version.</value>
        public Version JavaVersion { get; private set; }

        /// <summary>
        /// Gets the java c version.
        /// </summary>
        /// <value>The java c version.</value>
        public Version JavaCVersion { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is java SDK installation.
        /// </summary>
        /// <value><c>true</c> if this instance is java SDK installation; otherwise, <c>false</c>.</value>
        public bool IsJavaSDKInstallation => JavaCPath != null;

        /// <summary>
        /// Gets a value indicating whether this instance is environment variable defined.
        /// </summary>
        /// <value><c>true</c> if this instance is environment variable defined; otherwise, <c>false</c>.</value>
        public bool IsEnvironmentVariableDefined { get; private set; }

        /// <summary>
        /// Gets the name of the environment variable.
        /// </summary>
        /// <value>The name of the environment variable.</value>
        public string EnvironmentVariableName => "JAVA_HOME";

        /// <summary>
        /// Gets the environment java path.
        /// </summary>
        /// <value>The environment java path.</value>
        public string EnvironmentJavaPath { get; private set; }
        #endregion

        #region Interfaces
        /// <summary>
        /// Checks this instance.
        /// </summary>
        public override bool CheckService()
        {
            CheckJDK();
            CheckJRE();

            // merge result            
            InstallationPath = JavaCPath ?? JavaPath;
            Version = JavaCVersion ?? JavaVersion;

            CheckFromEnvironment();

            return JavaCPath != null || JavaPath != null;
        }

        /// <summary>
        /// Gets the help.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string GetHelp()
        {
            return "Please install java runtime first, see also:\n" +
                "    https://www.java.com/en/download/manual.jsp\n" +
                "    https://www.oracle.com/in/java/technologies/downloads/\n" +
                "    https://openjdk.org/";
        }

        #endregion

        #region JDK
        /// <summary>
        /// Checks the JDK.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool CheckJDK()
        {
            string path = EnvironmentUtils.SearchExecutableFile("javac");

            if (!path.IsFileExists())
            {
                return false;
            }

            JavaCPath = path;
            InstallationPath = path.GetDirectoryPath();

            CheckJavaCVersion();

            return true;
        }

        /// <summary>
        /// Checks the java c version.
        /// </summary>
        private void CheckJavaCVersion()
        {
            string outputText = ExternalProcessUtils.InvokeAndGetOutput(JavaCPath, " -version");
            if (outputText.iStartsWith("Unrecognized option:"))
            {
                return;
            }

            if (outputText.iStartsWith("javac "))
            {
                var text = outputText.Substring("javac ".Length).Trim();

                if (Version.TryParse(text, out var version))
                {
                    JavaCVersion = version;
                }
            }
        }
        #endregion

        #region JRE
        private bool CheckJRE()
        {
            string path = EnvironmentUtils.SearchExecutableFile("java");

            if (!path.IsFileExists())
            {
                return false;
            }

            JavaPath = path;

            CheckJavaVersion();

            return true;
        }

        private void CheckJavaVersion()
        {
            string outputText = ExternalProcessUtils.InvokeAndGetOutput(JavaPath, " -version");
            if (outputText.IsNullOrEmpty() || outputText.iStartsWith("Unrecognized option:"))
            {
                return;
            }

            int firstLineIndex = outputText.IndexOf('\n');
            if (firstLineIndex != -1)
            {
                outputText = outputText.Substring(0, firstLineIndex).Trim();
            }

            int i1 = outputText.IndexOf('"');
            int i2 = outputText.LastIndexOf('"');

            if (i1 != -1 && i2 != -1 && i1 < i2)
            {
                var versionText = outputText.Substring(i1 + 1, i2 - i1 - 1);

                if (Version.TryParse(versionText, out var version))
                {
                    JavaVersion = version;
                }
            }
        }
        #endregion

        #region Environment
        private void CheckFromEnvironment()
        {
            string value = Environment.GetEnvironmentVariable(EnvironmentVariableName);

            IsEnvironmentVariableDefined = value.IsNotNullOrEmpty();

            if (!value.IsDirectoryExists())
            {
                Logger.LogWarning("{0}={1}, path is not exists.", EnvironmentVariableName, value ?? "");
                return;
            }

            EnvironmentJavaPath = value;

        }
        #endregion
    }
}
