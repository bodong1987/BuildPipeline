using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework.Implements;
using BuildPipeline.Core.Utils;
using PropertyModels.Extensions;
using System.Reflection;

namespace BuildPipeline.Core.Framework
{
    /// <summary>
    /// Interface IPlugin
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the runtime path.
        /// </summary>
        /// <value>The runtime path.</value>
        string RuntimePath { get; }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>The path.</value>
        string Path { get; }

        /// <summary>
        /// Gets the scripts path.
        /// </summary>
        /// <value>The scripts path.</value>
        string ScriptsPath { get; }
        
        /// <summary>
        /// Gets the assemblies.
        /// </summary>
        /// <value>The assemblies.</value>
        Assembly[] Assemblies { get; }
    }

    namespace Implements
    {
        /// <summary>
        /// Class Plugin.
        /// Implements the <see cref="BuildPipeline.Core.Framework.IPlugin" />
        /// </summary>
        /// <seealso cref="BuildPipeline.Core.Framework.IPlugin" />
        internal class Plugin : IPlugin
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; set; }
            /// <summary>
            /// Gets or sets the path.
            /// </summary>
            /// <value>The path.</value>
            public string Path { get; set; }
            /// <summary>
            /// Gets or sets the scripts path.
            /// </summary>
            /// <value>The scripts path.</value>
            public string ScriptsPath { get; set; }
            /// <summary>
            /// Gets the assemblies.
            /// </summary>
            /// <value>The assemblies.</value>
            public Assembly[] Assemblies => AssembliesCore.ToArray();

            /// <summary>
            /// Gets the runtime path.
            /// </summary>
            /// <value>The runtime path.</value>
            public string RuntimePath { get; set; }

            /// <summary>
            /// The assemblies core
            /// </summary>
            readonly List<Assembly> AssembliesCore = new List<Assembly>();
            /// <summary>
            /// Loads the assembly.
            /// </summary>
            /// <param name="path">The path.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            internal bool LoadAssembly(string path)
            {
                try
                {
                    Assembly assembly = AssemblyLoader.Load(path);
                    if(assembly != null)
                    {
                        AssembliesCore.Add(assembly);
                        return true;
                    }

                    return false;
                }
                catch(Exception e)
                {
                    Logger.LogError("Failed load assembly:{0}\n{1}", path, e.Message);
                    return false;
                }
            }
        }
    }


    /// <summary>
    /// Class PluginLoader.
    /// </summary>
    public static class PluginLoader
    {
        /// <summary>
        /// The scripts directory name
        /// </summary>
        public const string ScriptsDirectoryName = "scripts";

        /// <summary>
        /// The plugin directory
        /// </summary>
        public readonly static string PluginDirectory;

        /// <summary>
        /// The plugin dictionary
        /// </summary>
        public readonly static Dictionary<string, IPlugin> PluginDict = new Dictionary<string, IPlugin>();

        /// <summary>
        /// Initializes static members of the <see cref="PluginLoader"/> class.
        /// </summary>
        static PluginLoader()
        {
            var appDir = AppFramework.GetRuntimeApplicationDirectory();
            var pluginsDir = appDir.JoinPath("plugins");
            PluginDirectory = pluginsDir;
        }

        /// <summary>
        /// Loads the plugins.
        /// </summary>
        public static void LoadPlugins()
        {
            LoadPlugins(PluginDirectory);
        }

        /// <summary>
        /// Loads the plugins.
        /// </summary>
        /// <param name="directory">The directory.</param>
        public static void LoadPlugins(string directory)
        {
            if(!directory.IsDirectoryExists())
            {
                return;
            }

            foreach(var pluginDir in Directory.GetDirectories(directory))
            {
                LoadPlugin(pluginDir);
            }
        }

        /// <summary>
        /// Loads the name of the plugin.
        /// </summary>
        /// <param name="name">The name.</param>
        public static void LoadPluginName(string name)
        {
            var path = Path.Combine(PluginDirectory, name);

            if(path.IsDirectoryExists())
            {
                LoadPlugin(path);
            }
            else
            {
                Logger.LogError("Plugin {0} is not exists.", name);
            }
        }

        /// <summary>
        /// Loads the plugin.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool LoadPlugin(string directory)
        {
            if(!directory.IsDirectoryExists())
            {
                Logger.LogError("Failed load plugin from path:{0}", directory ?? "Empty Path");
                return false;
            }

            string name = directory.GetFileName();

            if (IsPluginLoaded(name))
            {
                Logger.LogError("Failed load plugin from path:{0}, it is already exists.", directory);
                return false;
            }

            AssemblyLoader.RegisterAdditionalSearchDirectory(directory);
            
            var files = Directory.GetFiles(directory, "*.dll").ToList().FindAll(x =>
            {
                return x.GetFileName().iStartsWith("BuildPipeline.");
            });

            if(files.Count<=0)
            {
                Logger.LogError("Failed load plugin form path:{0}, no valid dll.", directory);
                return false;
            }

            Plugin p = new Plugin();
            p.Name = name;
            p.RuntimePath = directory;
            p.Path = ProcessRuntimeDirectory(directory);
            p.ScriptsPath = ProcessRuntimeDirectory(directory.JoinPath(ScriptsDirectoryName));

            foreach(var f in files)
            {
                p.LoadAssembly(f);
            }

            PluginDict[p.Name] = p;
            return true;
        }

        /// <summary>
        /// Processes the runtime directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>System.String.</returns>
        private static string ProcessRuntimeDirectory(string path)
        {
#if DEBUG
            return path.Replace($"{Path.DirectorySeparatorChar}Debug{Path.DirectorySeparatorChar}", $"{Path.DirectorySeparatorChar}Release{Path.DirectorySeparatorChar}");
#else
            return path;
#endif
        }

        /// <summary>
        /// Determines whether [is plugin loaded] [the specified name].
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if [is plugin loaded] [the specified name]; otherwise, <c>false</c>.</returns>
        public static bool IsPluginLoaded(string name)
        {
            return PluginDict.ContainsKey(name);
        }

        /// <summary>
        /// Gets the plugin.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>IPlugin.</returns>
        public static IPlugin GetPlugin(string name)
        {
            PluginDict.TryGetValue(name, out IPlugin plugin);
            return plugin;
        }

    }
}
