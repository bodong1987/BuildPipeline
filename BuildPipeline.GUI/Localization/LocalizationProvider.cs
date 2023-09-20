using BuildPipeline.Core;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Services;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace BuildPipeline.GUI.Localization
{
    #region Provider
    /// <summary>
    /// Class LocalizationProvider.
    /// </summary>
    public static class LocalizationProvider
    {
        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <value>The service.</value>
        public static ILocalizeService Service { get; private set; }

        static LocalizationProvider()
        {
            Service = ServiceProvider.GetService<ILocalizeService>();
        }
    }
    #endregion
}
