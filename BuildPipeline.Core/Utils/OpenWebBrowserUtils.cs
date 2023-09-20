using BuildPipeline.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildPipeline.Core.Utils
{
    /// <summary>
    /// Class OpenWebBrowserUtils.
    /// </summary>
    public static class OpenWebBrowserUtils
    {
        /// <summary>
        /// Opens the specified HTTP URL.
        /// </summary>
        /// <param name="httpUrl">The HTTP URL.</param>
        public static void Open(string httpUrl)
        {
            try
            {
                OpenUnsafe(httpUrl);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed open {0} in default webbrowser:{1}", httpUrl, ex.Message);
            }
        }

        /// <summary>
        /// Opens url with unsafe mode.
        /// </summary>
        /// <param name="httpUrl">The httpUrl.</param>
        public static void OpenUnsafe(string httpUrl)
        {
            System.Diagnostics.Process.Start("explorer", httpUrl);
        }
    }
}
