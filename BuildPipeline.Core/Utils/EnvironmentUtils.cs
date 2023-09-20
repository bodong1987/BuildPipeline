using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using System.Collections;
using System.Collections.Specialized;

namespace BuildPipeline.Core.Utils
{
    /// <summary>
    /// Class EnvironmentUtils.
    /// </summary>
    public static class EnvironmentUtils
    {
        /// <summary>
        /// Gets the paths.
        /// </summary>
        /// <returns>System.String[].</returns>
        public static string[] GetPaths()
        {
            var value = Environment.GetEnvironmentVariable("Path");

            if(value == null)
            {
                value = Environment.GetEnvironmentVariable("PATH");

                if(value == null)
                {
                    return new string[] { };
                }
            }

            var values = value.Split(Path.PathSeparator);

            return values;
        }

        /// <summary>
        /// Searches the path.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>System.String.</returns>
        public static string SearchPath(string filename)
        {
            // current directory...
            {
                var directory = Directory.GetCurrentDirectory();

                var path = directory.JoinPath(filename);

                if(path.IsFileExists())
                {
                    return path;
                }
            }

            // runtime path...
            {
                var directory = AppFramework.GetRuntimeApplicationDirectory();
                var path = directory.JoinPath(filename);

                if(path.IsFileExists())
                {
                    return path;
                }

            }

            // PATH
            foreach(var i in GetPaths())
            {
                var path = i.JoinPath(filename);

                if(path.IsFileExists())
                {
                    return path;
                }
            }

            return null;
        }

        /// <summary>
        /// Searches the executable file in System PATH and current working directory
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>System.String.</returns>
        public static string SearchExecutableFile(string name)
        {
            if(OperatingSystem.IsWindows() && !name.IsExtension(".exe"))
            {
                name = name + ".exe";
            }

            return SearchPath(name);
        }

        /// <summary>
        /// Gets the environment variables.
        /// </summary>
        /// <returns>StringDictionary.</returns>
        public static StringDictionary GetEnvironmentVariables()
        {
            StringDictionary dict = new StringDictionary();

            foreach(var p in Environment.GetEnvironmentVariables().Cast<DictionaryEntry>())
            {
                dict[p.Key.ToString()] = p.Value.ToString();
            }

            return dict;
        }
    }
}
