using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using System.Collections.Specialized;
using System.Diagnostics;

namespace BuildPipeline.Core.Services
{
    #region Interfaces
    /// <summary>
    /// Interface IExternalProcessEventObserver
    /// Implements the <see cref="ILoggerObserver" />
    /// </summary>
    /// <seealso cref="ILoggerObserver" />
    public interface IExternalProcessEventObserver : ILoggerObserver
    {
        /// <summary>
        /// Called when [idle].
        /// </summary>
        /// <param name="process">The process.</param>
        void OnIdle(Process process);

        /// <summary>
        /// Called when [canceled].
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        void OnCanceled(CancellationTokenSource cancellationTokenSource);
    }

    /// <summary>
    /// Interface IExternalProcessService
    /// Implements the <see cref="IService" />
    /// </summary>
    /// <seealso cref="IService" />
    public interface IExternalProcessService : IService
    {
        #region Async
        /// <summary>
        /// Starts the specified path.
        /// Start external process(get output asynchronously)
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="waitForExit">if set to <c>true</c> [wait for exit].</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="environmentVariables">The environment variables.</param>
        /// <returns>System.Int32.</returns>
        Task<int> StartAsync(
            string path, 
            string arguments, 
            bool waitForExit, 
            IExternalProcessEventObserver observer, 
            CancellationTokenSource cancellationTokenSource = null,
            StringDictionary environmentVariables = null
            );

        /// <summary>
        /// Starts the specified path.
        /// Start external process(get output asynchronously)
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="workingDirectory">The working directory.</param>
        /// <param name="waitForExit">if set to <c>true</c> [wait for exit].</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="environmentVariables">The environment variables.</param>
        /// <returns>System.Int32.</returns>
        Task<int> StartAsync(
            string path,
            string arguments,
            string workingDirectory,
            bool waitForExit,
            IExternalProcessEventObserver observer,
            CancellationTokenSource cancellationTokenSource = null,
            StringDictionary environmentVariables = null
            );

        /// <summary>
        /// Starts the specified process start information.
        /// Start external process(get output asynchronously)
        /// </summary>
        /// <param name="processStartInfo">The process start information.</param>
        /// <param name="waitForExit">if set to <c>true</c> [wait for exit].</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>System.Int32.</returns>
        Task<int> StartAsync(
            ProcessStartInfo processStartInfo,
            bool waitForExit, 
            IExternalProcessEventObserver observer,
            CancellationTokenSource cancellationTokenSource = null
            );
        #endregion

        #region Synced
        /// <summary>
        /// Start external process (get output synchronously)
        /// In this case, all output will be obtained after the process ends.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="waitForExit">if set to <c>true</c> [wait for exit].</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="environmentVariables">The environment variables.</param>
        /// <returns>System.Int32.</returns>
        int Start(
            string path, 
            string arguments, 
            bool waitForExit, 
            IExternalProcessEventObserver observer,
            CancellationTokenSource cancellationTokenSource = null,
            StringDictionary environmentVariables = null);

        /// <summary>
        /// Starts the specified process start information.
        /// Start external process (get output synchronously)
        /// In this case, all output will be obtained after the process ends.
        /// </summary>
        /// <param name="processStartInfo">The process start information.</param>
        /// <param name="waitForExit">if set to <c>true</c> [wait for exit].</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>System.Int32.</returns>
        int Start(ProcessStartInfo processStartInfo, 
            bool waitForExit,
            IExternalProcessEventObserver observer,
            CancellationTokenSource cancellationTokenSource = null
            );
        #endregion
    }
    #endregion
}
