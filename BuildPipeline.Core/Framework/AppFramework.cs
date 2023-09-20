using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Utils;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace BuildPipeline.Core.Framework
{
    /// <summary>
    /// Class AppFramework.
    /// </summary>
    public static class AppFramework
    {
        #region Constructors
        /// <summary>
        /// Initializes static members of the <see cref="AppFramework"/> class.
        /// </summary>
        static AppFramework()
        {
            AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            SearchConfigurationPathes();
        }

        /// <summary>
        /// The project URL
        /// </summary>
        public const string ProjectUrl = "https://github.com/bodong1987/BuildPipeline";

        /// <summary>
        /// check is windows
        /// </summary>
        /// <returns>System.Boolean.</returns>
        public static bool IsWindows => OperatingSystem.IsWindows();

        /// <summary>
        /// check is macos
        /// </summary>
        /// <returns>System.Boolean.</returns>
        public static bool IsMacOS => OperatingSystem.IsMacOS();

        /// <summary>
        /// Gets a value indicating whether this instance is linux.
        /// </summary>
        /// <value><c>true</c> if this instance is linux; otherwise, <c>false</c>.</value>
        public static bool IsLinux => OperatingSystem.IsLinux();

        /// <summary>
        /// check is arm or arm64
        /// </summary>
        /// <returns>System.Boolean.</returns>
        public static bool IsArm
        {
            get
            {
                var arch = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
                return arch == System.Runtime.InteropServices.Architecture.Arm ||
                    arch == System.Runtime.InteropServices.Architecture.Arm64;
            }
        }

        /// <summary>
        /// check is arm64
        /// </summary>
        /// <returns>System.Boolean.</returns>
        public static bool IsArm64
        {
            get
            {
                var arch = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
                return arch == System.Runtime.InteropServices.Architecture.Arm64;
            }
        }

        /// <summary>
        /// Handles the <see cref="E:UnhandledException" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.LogError("UnhandledException:{0}", e.ExceptionObject.ToString());
        }

        /// <summary>
        /// Gets or sets a value indicating whether [enable first change exception record].
        /// </summary>
        /// <value><c>true</c> if [enable first change exception record]; otherwise, <c>false</c>.</value>
        public static bool EnableFirstChangeExceptionRecord { get; set; } = true;

        /// <summary>
        /// Handles the <see cref="E:FirstChanceException" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FirstChanceExceptionEventArgs"/> instance containing the event data.</param>
        private static void OnFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            if(EnableFirstChangeExceptionRecord)
            {
                Logger.LogWarning("FirstChanceException:{0}\n{1}", e.Exception.Message, e.Exception.StackTrace);
            }            
        }


        #endregion

        #region App Paths
        /// <summary>
        /// Gets the application path.
        /// </summary>
        /// <returns>System.String.</returns>
        public static string GetRuntimeApplicationPath()
        {
            var asm = System.Reflection.Assembly.GetEntryAssembly();

            if (asm != null)
            {
                return asm.Location;
            }

            Process p = Process.GetCurrentProcess();
            return p.MainModule.FileName;
        }

        /// <summary>
        /// Gets the runtime application directory.
        /// </summary>
        /// <returns>System.String.</returns>
        public static string GetRuntimeApplicationDirectory()
        {
            return Path.GetDirectoryName(GetRuntimeApplicationPath());
        }

        /// <summary>
        /// Gets the publish application path.
        /// </summary>
        /// <returns>System.String.</returns>
        public static string GetPublishApplicationPath()
        {
#if DEBUG
            var runtimePath = GetRuntimeApplicationPath();
            var targetFramework = runtimePath.GetDirectoryName();

            var path = Path.GetFullPath(Path.Combine(runtimePath.GetDirectoryPath(), "../../Release", targetFramework, runtimePath.GetFileName()));
            return path;
#else
            return GetRuntimeApplicationPath();
#endif
        }

        /// <summary>
        /// Gets the publish application directory.
        /// </summary>
        /// <returns>System.String.</returns>
        public static string GetPublishApplicationDirectory()
        {
            return Path.GetFullPath(Path.GetDirectoryName(GetPublishApplicationPath()));
        }

        /// <summary>
        /// The application path
        /// </summary>
        public static readonly string ApplicationPath = GetRuntimeApplicationPath();

        /// <summary>
        /// The application directory
        /// </summary>
        public static readonly string ApplicationDirectory = GetRuntimeApplicationPath().GetDirectoryPath();

        /// <summary>
        /// The publish application directory
        /// </summary>
        public static readonly string PublishApplicationDirectory = GetPublishApplicationPath().GetDirectoryPath();

        /// <summary>
        /// The application name
        /// </summary>
        public static readonly string ApplicationName = GetRuntimeApplicationPath().GetFileName();

        /// <summary>
        /// The document directory
        /// </summary>
        public static readonly string SystemDocumentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        /// <summary>
        /// The system temporary directory
        /// </summary>
        public static readonly string SystemTempDirectory = Path.GetTempPath();

        /// <summary>
        /// The document directory
        /// </summary>
        public static readonly string DocumentDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BuildPipe");

        /// <summary>
        /// Gets the application temporary directory.
        /// </summary>
        /// <value>The application temporary directory.</value>
        public static string ApplicationTempDirectory { get; private set; } = SystemTempDirectory;

        /// <summary>
        /// Gets the application temporary document directory.
        /// </summary>
        /// <value>The application temporary document directory.</value>
        public static string ApplicationTempDocumentDirectory { get; private set; } = SystemTempDirectory;

        /// <summary>
        /// Gets the application temporary configuration directory.
        /// </summary>
        /// <value>The application temporary configuration directory.</value>
        public static string ApplicationTempConfigurationDirectory { get; private set; } = SystemTempDirectory;
        #endregion

        #region Search Paths
        /// <summary>
        /// Files the path.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>System.String.</returns>
        public static string FilePath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(PublishApplicationDirectory, relativePath));
        }
        /// <summary>
        /// Files the path.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>System.String.</returns>
        public static string RumtimeFilePath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(ApplicationDirectory, relativePath));
        }

        #endregion

        #region Configs
        private readonly static List<string> ConfigurationDirectoriesCore = new List<string>();
        /// <summary>
        /// Gets the configuration directories.
        /// </summary>
        /// <value>The configuration directories.</value>
        public static string[] ConfigurationDirectories => ConfigurationDirectoriesCore.ToArray();

        private static void SearchConfigurationPathes()
        {
            var root = GetPublishApplicationDirectory();

            var rootConfig = root.JoinPath("Config");

            ConfigurationDirectoriesCore.Add(rootConfig);

            if(Directory.Exists(rootConfig))
            {
                foreach (var i in Directory.GetDirectories(root))
                {
                    var testPath = i.JoinPath("Config");

                    if (testPath.IsDirectoryExists())
                    {
                        ConfigurationDirectoriesCore.Add(testPath);
                    }
                }
            }
        }
        #endregion
    }
}
