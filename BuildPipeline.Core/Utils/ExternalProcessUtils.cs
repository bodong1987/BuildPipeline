using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Services.Implements;
using BuildPipeline.Core.Services;

namespace BuildPipeline.Core.Utils
{
    /// <summary>
    /// Class ExternalProcessUtils.
    /// </summary>
    public static class ExternalProcessUtils
    {
        /// <summary>
        /// Invokes the and get output.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>System.String.</returns>
        public static string InvokeAndGetOutput(string path, string arguments)
        {
            var externalProcessService = ServiceProvider.GetService<IExternalProcessService>();

            Logger.Assert(externalProcessService != null);

            if(externalProcessService == null)
            {
                return "";
            }

            SyncedBufferedExternalProcessEventObserver Receiver = new SyncedBufferedExternalProcessEventObserver();

            externalProcessService.Start(path, arguments, true, Receiver);

            string outputText = Receiver.ToString();

            return outputText;
        }
    }
}
