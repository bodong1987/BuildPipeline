using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Utils;
using System.Reflection;

namespace BuildPipeline.Core.Framework
{
    /// <summary>
    /// Class AssemblyLoader.
    /// </summary>
    public static class AssemblyLoader
    {
        private readonly static List<Assembly> AllAssemblies = new List<Assembly>();
        private static HashSet<Assembly> ProcessedAssemblyLoadingDict = new HashSet<Assembly>();
        private static HashSet<Assembly> ProcessedAssemblyLoadedDict = new HashSet<Assembly>();

        static readonly List<string> AdditionalSearchDirectoriesCore = new List<string>();

        /// <summary>
        /// Gets the additional search directories.
        /// </summary>
        /// <value>The additional search directories.</value>
        public static string[] AdditionalSearchDirectories => AdditionalSearchDirectoriesCore.ToArray();

        /// <summary>
        /// Initializes static members of the <see cref="AssemblyLoader"/> class.
        /// </summary>
        static AssemblyLoader()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoaded;
        }

        /// <summary>
        /// Registers the additional search directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool RegisterAdditionalSearchDirectory(string directory)
        {
            if(directory.IsDirectoryExists() && !AdditionalSearchDirectoriesCore.Contains(directory))
            {
                AdditionalSearchDirectoriesCore.Add(directory);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Uns the register additional search directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        public static void UnRegisterAdditionalSearchDirectory(string directory)
        {
            AdditionalSearchDirectoriesCore.Remove(directory);
        }

        /// <summary>
        /// Handles the <see cref="E:AssemblyResolve" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="ResolveEventArgs"/> instance containing the event data.</param>
        /// <returns>Assembly.</returns>
        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string name = args.Name;
            int index = args.Name.IndexOf(',');
            if(index != -1)
            {
                name = args.Name.Substring(0, index);
            }
            
            var asm = GetAssemblyByName(name);

            if(asm != null)
            {
                return asm;
            }

            foreach (var assembly in AllAssemblies)
            {
                if (assembly.FullName == args.Name)
                {
                    return assembly;
                }
            }

            // search under app directory
            {
                var location = typeof(AssemblyLoader).Assembly.Location;
                var dir = Path.GetDirectoryName(location);

                var file = Path.Combine(dir, name + ".dll");

                if (File.Exists(file))
                {
                    return Assembly.LoadFile(file);
                }
            }

            {
                foreach(var dir in AdditionalSearchDirectoriesCore)
                {
                    var file = Path.Combine(dir, name + ".dll");

                    if(File.Exists(file))
                    {
                        return Assembly.LoadFile(file);
                    }
                }
            }            
            

            return null;
        }

        /// <summary>
        /// Handles the <see cref="E:AssemblyLoaded" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="AssemblyLoadEventArgs"/> instance containing the event data.</param>
        private static void OnAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        {
            
        }

        /// <summary>
        /// Raises the assembly loading events.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        private static void RaiseAssemblyLoadingEvents(Assembly assembly)
        {
            if (ProcessedAssemblyLoadingDict.Contains(assembly))
            {
                return;
            }

            ProcessedAssemblyLoadingDict.Add(assembly);

            foreach (var t in assembly.GetTypes())
            {
                if (t.IsClass && !t.IsAbstract)
                {
                    object[] attrs = t.GetCustomAttributes(true);

                    foreach (var a in attrs)
                    {
                        if (a.GetType().Name.Equals("AssemblyLoadingAttribute", StringComparison.CurrentCultureIgnoreCase))
                        {
                            Activator.CreateInstance(t);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Raises the assembly loaded events.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        private static void RaiseAssemblyLoadedEvents(Assembly assembly)
        {
            if (ProcessedAssemblyLoadedDict.Contains(assembly))
            {
                return;
            }

            ProcessedAssemblyLoadedDict.Add(assembly);

            foreach (var t in assembly.GetTypes())
            {
                if (t.IsClass && !t.IsAbstract)
                {
                    object[] attrs = t.GetCustomAttributes(true);

                    foreach (var a in attrs)
                    {
                        if (a.GetType().Name.Equals("AssemblyLoadedAttribute", StringComparison.CurrentCultureIgnoreCase))
                        {
                            Activator.CreateInstance(t);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the name of the assembly by.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns>Assembly.</returns>
        private static Assembly GetAssemblyByName(string assemblyName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GetName().Name.Equals(assemblyName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return asm;
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether [is assembly loaded] [the specified assembly].
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns><c>true</c> if [is assembly loaded] [the specified assembly]; otherwise, <c>false</c>.</returns>
        public static bool IsAssemblyLoaded(Assembly assembly)
        {
            return AllAssemblies.Contains(assembly);
        }

        /// <summary>
        /// Gets all assemblies.
        /// </summary>
        /// <returns>Assembly[].</returns>
        public static Assembly[] GetAllAssemblies()
        {
            return AllAssemblies.ToArray();
        }

        /// <summary>
        /// Loads the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="triggerEvents">if set to <c>true</c> [trigger events].</param>
        /// <returns>Assembly.</returns>
        public static Assembly Load(string path, bool triggerEvents = true)
        {
            Logger.Assert(path != null);

            path = Path.GetFullPath(path);

            if (!File.Exists(path))
            {
                return null;
            }

            string name = Path.GetFileNameWithoutExtension(path);

            Assembly assembly = GetAssemblyByName(name);

            if (assembly != null)
            {
                if(IsAssemblyLoaded(assembly))
                {
                    return assembly;
                }
            }

            assembly = Assembly.LoadFile(path);

            if (assembly == null)
            {
                return null;
            }

            AllAssemblies.Add(assembly);

            if (triggerEvents)
            {
                RaiseAssemblyLoadingEvents(assembly);

                RaiseAssemblyLoadedEvents(assembly);
            }

            ExtensibilityFramework.AddPart(assembly);

            return assembly;
        }

        /// <summary>
        /// Loads the specified pathes.
        /// </summary>
        /// <param name="pathes">The pathes.</param>
        /// <param name="triggerEvents">if set to <c>true</c> [trigger events].</param>
        /// <returns>Assembly[].</returns>
        public static Assembly[] Load(string[] pathes,  bool triggerEvents = true)
        {
            List<Assembly> assemblies = new List<Assembly>();

            foreach(var p in pathes)
            {
                var assembly = Load(p, false);

                if(assembly != null)
                {
                    assemblies.Add(assembly);
                }
            }

            if(triggerEvents)
            {
                foreach (var assembly in assemblies)
                {
                    RaiseAssemblyLoadingEvents(assembly);
                }

                foreach (var assembly in assemblies)
                {
                    RaiseAssemblyLoadedEvents(assembly);
                }
            }

            return assemblies.ToArray();
        }
    }

    /// <summary>
    /// Class AssemblyLoadingAttribute.
    /// Implements the <see cref="System.Attribute" />
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class AssemblyLoadingAttribute : Attribute
    {

    }

    /// <summary>
    /// Class AssemblyLoadedAttribute.
    /// Implements the <see cref="System.Attribute" />
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class AssemblyLoadedAttribute : Attribute
    {
    }
}
