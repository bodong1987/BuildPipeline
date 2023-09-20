using Avalonia;
using Avalonia.Svg.Skia;
using System.Reflection;

namespace BuildPipeline.GUI.Framework
{
    /// <summary>
    /// Class EditorFramework.
    /// </summary>
    public static class EditorFramework
    {
        #region Base App Info provider        
        /// <summary>
        /// Gets the application version.
        /// </summary>
        /// <value>The application version.</value>
        public static string AppVersion
        {
            get
            {
                var v = typeof(EditorFramework).Assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

                if(v != null)
                {
                    return v;
                }

                v = Assembly
                      .GetEntryAssembly()
                      ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                      ?.InformationalVersion;

                if (v != null)
                {
                    return v;
                }

                return typeof(EditorFramework).Assembly.ImageRuntimeVersion;
            }
        }
        #endregion
    }
}