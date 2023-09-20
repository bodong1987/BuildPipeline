using System.Runtime.Versioning;
using BuildPipeline.Core.BuilderFramework.Implements;
using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using PropertyModels.Extensions;

namespace BuildPipeline.Core.BuilderFramework
{
    /// <summary>
    /// Class BuildFramework.
    /// </summary>
    public static class BuildFramework
    {
        #region Properties
        static readonly BuildContextFactoryCollector Collector = new BuildContextFactoryCollector();

        /// <summary>
        /// Gets all factories.
        /// </summary>
        /// <value>All factories.</value>
        public static IBuildContextFactory[] AllFactories => Collector.AllFactories.ToArray();

        /// <summary>
        /// Gets or sets a value indicating whether this instance is command line mode.
        /// </summary>
        /// <value><c>true</c> if this instance is command line mode; otherwise, <c>false</c>.</value>
        public static bool IsCommandLineMode { get; internal set; }
        #endregion

        #region Open Apis
        /// <summary>
        /// Initializes static members of the <see cref="BuildFramework"/> class.
        /// </summary>
        static BuildFramework()
        {
            if(OperatingSystem.IsMacOS())
            {
                CopyRuntimeNativeLibrariesIfRequired();
            }
        }

        /// <summary>
        /// on macos, some native libraries can't be load success, so we copy the target libaries to the runtime path
        /// </summary>
        [SupportedOSPlatform("macos")]
        private static void CopyRuntimeNativeLibrariesIfRequired()
        { 
            if(AppFramework.IsArm)
            {
                string[] arm64Libs = new string[]
                {
                    "runtimes/osx-arm64/native/libMono.Unix.dylib"
                };


                CopyRuntimeNativeLibrariesIfRequired(arm64Libs);
            }
            else
            {
                string[] intelLibs = new string[]
                {
                    "runtimes/osx-x64/native/libMono.Unix.dylib"
                };


                CopyRuntimeNativeLibrariesIfRequired(intelLibs);
            }
        }

        private static void CopyRuntimeNativeLibrariesIfRequired(IEnumerable<string> files)
        {
            foreach(var file in files)
            {
                if(!file.IsFileExists())
                {
                    continue;
                }

                var name = file.GetFileName();
                var destPath = AppFramework.GetRuntimeApplicationDirectory().JoinPath(name);

                if(CheckIsCopyRequired(destPath, file))
                {
                    try
                    {
                        File.Copy(file, destPath, true);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static bool CheckIsCopyRequired(string source, string dest)
        {
            try
            {
                FileInfo sf = new FileInfo(source);
                FileInfo df = new FileInfo(dest);

                return sf.Length != df.Length;
            }
            catch
            {

            }

            return true;
        }

        /// <summary>
        /// Finds the factory.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>IBuildContextFactory.</returns>
        public static IBuildContextFactory FindFactory(string name)
        {
            return Collector.AllFactories.Find(x=>x.Name == name);
        }

        /// <summary>
        /// Creates new context.
        /// </summary>
        /// <param name="factoryName">Name of the factory.</param>
        /// <returns>IBuildContext.</returns>
        public static IBuildContext NewContext(string factoryName)
        {
            var factory = FindFactory(factoryName);

            return factory != null ? factory.NewContext() : null;
        }

        /// <summary>
        /// Creates new context.
        /// </summary>
        /// <param name="factoryName">Name of the factory.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>IBuildContext.</returns>
        public static IBuildContext NewContext(string factoryName, string[] args)
        {
            try
            {
                var factory = FindFactory(factoryName);

                return factory != null ? factory.CreateContext(new BuildContextFactoryOptions(args)) : null;
            }
            catch(Exception e)
            {
                Logger.LogError(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Creates new build pipeline.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>IBuildPipeline.</returns>
        public static IBuildPipeline NewBuildPipeline(IBuildContext context)
        {
            return BuiltinPipeline.Create(context);
        }

        /// <summary>
        /// Creates new buildpipeline.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="forceAllTasks">if set to <c>true</c> [force all tasks].</param>
        /// <returns>IBuildPipeline.</returns>
        public static IBuildPipeline NewBuildPipeline(IBuildPipelineDocument document, bool forceAllTasks = false)
        {
            return BuiltinPipeline.Create(document, forceAllTasks);
        }

        /// <summary>
        /// Creates new buildpipelinedocument.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="forceAllTasks">if set to <c>true</c> [force all tasks].</param>
        /// <returns>IBuildPipelineDocument.</returns>
        public static IBuildPipelineDocument NewBuildPipelineDocument(IBuildPipeline pipeline, bool forceAllTasks = false)
        {
            if(pipeline == null)
            {
                return null;
            }

            BuildPipelineDocument document = new BuildPipelineDocument(pipeline, forceAllTasks);

            return document;
        }


        /// <summary>
        /// Loads the build pipeline document.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>IBuildPipelineDocument.</returns>
        public static IBuildPipelineDocument LoadBuildPipelineDocument(string path)
        {
            BuildPipelineDocument document = new BuildPipelineDocument();

            if(!document.Load(path))
            {
                Logger.LogError("Failed load BuildPipelineDocument from {0}", path);
                return null;
            }

            return document;
        }
        #endregion

        #region Main Entry
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>System.Int32.</returns>
        public static int Main(string[] args)
        {
            Logger.Log($"ComamndLine:{Environment.CommandLine}");

            if(args.Length == 0)
            {
                Logger.Log("No operation find in args.");

                return -1;
            }

            string pluginName = args.First().Trim();

            if(pluginName.iEquals("--help"))
            {
                Initialize(true);

                HelpTextGenerator generator = HelpTextGenerator.Create(args.Skip(1).ToArray());
                Logger.Log(generator.ToString());
                return 0;
            }

            if(pluginName.IsNullOrEmpty())
            {
                Logger.LogError("Missing Plugin name...");
                return -1;
            }

            string factoryName = args.Skip(1).FirstOrDefault();

            if(factoryName.IsNullOrEmpty())
            {
                Logger.LogError("missing Factory name...");
                return -1;
            }

            Initialize(true, pluginName);

            string[] arguments = args.Skip(2).ToArray();

            var context = NewContext(factoryName, arguments);

            if(context == null)
            {
                Logger.LogError("Failed create Build Context for {0} with command line:{1}", factoryName, Environment.CommandLine);
                return -1;
            }

            var buildPipeline = NewBuildPipeline(context);

            if(buildPipeline == null)
            {
                Logger.LogError("Failed find factory Name = {0}", factoryName);
                return -1;
            }

            IBuildPipelineExecuteService service = ServiceProvider.GetService<IBuildPipelineExecuteService>();

            Logger.Assert(service != null);

            return service.Execute(buildPipeline, new BuildTaskExecuteObserver(), null, BuildPipelineExecuteType.InternalProc);
        }

        /// <summary>
        /// Gets the command line help text.
        /// </summary>
        /// <returns>System.String.</returns>
        public static string GetCommandLineHelpText()
        {
            HelpTextGenerator generator = HelpTextGenerator.Create(new string[] {"--help"});
            return generator?.ToString();
        }

        /// <summary>
        /// Gets the command line help text.
        /// </summary>
        /// <param name="factoryName">Name of the factory.</param>
        /// <returns>System.String.</returns>
        public static string GetCommandLineHelpText(string factoryName)
        {
            HelpTextGenerator generator = HelpTextGenerator.Create(factoryName);

            return generator?.ToString();
        }

        /// <summary>
        /// load all factory
        /// </summary>
        /// <param name="isCommandLineMode">if set to <c>true</c> [is command line mode].</param>
        public static void Initialize(bool isCommandLineMode)
        {
            IsCommandLineMode = isCommandLineMode;

            ReadAdditionalEnvironmentVariables();

            PluginLoader.LoadPlugins();

            InitializeScriptRuntimes();

            Collector.Collect();
        }

        /// <summary>
        /// load one plugin
        /// </summary>
        /// <param name="isCommandLineMode">if set to <c>true</c> [is command line mode].</param>
        /// <param name="pluginName">Name of the plugin.</param>
        public static void Initialize(bool isCommandLineMode, string pluginName)
        {
            IsCommandLineMode = isCommandLineMode;

            ReadAdditionalEnvironmentVariables();

            PluginLoader.LoadPluginName(pluginName);

            InitializeScriptRuntimes();

            Collector.Collect();
        }

        /// <summary>
        /// Initializes the script runtimes.
        /// </summary>
        private static void InitializeScriptRuntimes()
        {
            var services = ServiceProvider.GetServices(typeof(IScriptRuntimeService)).Select(x => x as IScriptRuntimeService);
            foreach(var service in services)
            {
                if(!service.Initialize())
                {
                    Logger.LogError("Failed initialize {0}", service.ServiceName);
                }
            }
        }

        private static void ReadAdditionalEnvironmentVariables()
        {
            if(!OperatingSystem.IsMacOS())
            {
                return;
            }

            string homePath = Environment.GetEnvironmentVariable("HOME");

            if(!homePath.IsDirectoryExists())
            {
                return;
            }

            var bash_profile = homePath.JoinPath(".bash_profile");

            if(!bash_profile.IsFileExists())
            {
                return;
            }

            ParseExtraEnvironmentVariableSettings(bash_profile);
        }

        private static void ParseExtraEnvironmentVariableSettings(string path)
        {
            var text = File.ReadAllText(path);
            var items = text.Split('\n');

            foreach(var i in items)
            {
                var line = i.Trim();

                if(!line.iStartsWith("export"))
                {
                    continue;
                }

                line = line.Substring("export".Length).Trim();

                int index = line.IndexOf('=');

                if(index == -1)
                {
                    continue;
                }

                var key = line.Substring(0, index).Trim();
                var value = line.Substring(index + 1).Trim();

                if(key.IsNotNullOrEmpty() && value.IsNotNullOrEmpty() && Environment.GetEnvironmentVariable(key) == null)
                {
                    Environment.SetEnvironmentVariable(key, value);
                    Logger.Log("Set Extra Enviroment Variable: {0}={1}", key, value);
                }
            }
        }
        #endregion
    }
}
