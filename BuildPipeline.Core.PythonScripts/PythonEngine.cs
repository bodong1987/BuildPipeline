using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using BuildPipeline.Core.Extensions.IO;

namespace BuildPipeline.Core.PythonScripts
{
    /// <summary>
    /// Class PythonEngine.
    /// </summary>
    public static class PythonEngine
    {
        /// <summary>
        /// The runtime
        /// </summary>
        public static readonly PythonRuntime Runtime;

        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value><c>true</c> if this instance is initialized; otherwise, <c>false</c>.</value>
        public static bool IsInitialized { get; private set; }

        static PythonEngine()
        {
            try
            {
                Runtime = PythonRuntime.Create("Main", true);
            }
            catch (Exception e)
            {
                Logger.LogError("Failed create python engine : {0}", e.Message);
            }
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public static void Initialize()
        {
            List<string> scriptPathes = new List<string>() {
                AppFramework.GetPublishApplicationDirectory().JoinPath("scripts"),
                AppFramework.GetPublishApplicationDirectory().JoinPath("scripts/stdlib")
            };
            scriptPathes.AddRange(PluginLoader.PluginDict.Values.Select(x => x.ScriptsPath));

            Runtime.AddSearchPathes(scriptPathes);

            IsInitialized = true;
        }
    }
}