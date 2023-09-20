using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using PropertyModels.Extensions;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;

namespace BuildPipeline.Core.Services.Implements
{
    #region Base Receiver
    /// <summary>
    /// Class SyncedBufferedExternalProcessEventObserver.
    /// Implements the <see cref="IExternalProcessEventObserver" />
    /// </summary>
    /// <seealso cref="IExternalProcessEventObserver" />
    public class SyncedBufferedExternalProcessEventObserver : IExternalProcessEventObserver
    {
        /// <summary>
        /// The buffer
        /// </summary>
        public readonly StringBuilder Buffer = new StringBuilder();

        /// <summary>
        /// Called when [idle].
        /// </summary>
        public void OnIdle(Process process)
        {
        }

        /// <summary>
        /// Called when [receive].
        /// May called from any thread...
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="message">The message.</param>
        public void OnEvent(LoggerLevel level, string tag, string message)
        {
            lock(Buffer)
            {
                Buffer.AppendLine(message);
            }            
        }

        /// <summary>
        /// May called from any thread...
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            lock(Buffer)
            {
                return Buffer.ToString();
            }            
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            lock(Buffer)
            {
                Buffer.Clear();
            }
        }

        /// <summary>
        /// Called when [canceled].
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        public void OnCanceled(CancellationTokenSource cancellationTokenSource)
        {
            
        }
    }

    /// <summary>
    /// Class ExternalProcessEventObserver.
    /// Implements the <see cref="BuildPipeline.Core.Services.IExternalProcessEventObserver" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Services.IExternalProcessEventObserver" />
    public class ExternalProcessEventObserver : IExternalProcessEventObserver
    {
        /// <summary>
        /// Called when [canceled].
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        public virtual void OnCanceled(CancellationTokenSource cancellationTokenSource)
        {
        }

        /// <summary>
        /// Called when [idle].
        /// </summary>
        /// <param name="process">The process.</param>
        public virtual void OnIdle(Process process)
        {            
        }

        /// <summary>
        /// Called when [receive].
        /// May called from any thread...
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="message">The message.</param>
        public virtual void OnEvent(LoggerLevel level, string tag, string message)
        {
            Logger.Log(level, tag, message);
        }
    }
    #endregion

    #region Services
    [Export]
    internal class ExternalProcessService : AbstractService, IExternalProcessService
    {
        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>The name of the service.</value>
        public override string ServiceName => "External Process Service";

        /// <summary>
        /// Gets a value indicating whether this instance is available.
        /// 获取当前服务是否在线
        /// </summary>
        /// <value><c>true</c> if this instance is available; otherwise, <c>false</c>.</value>
        public override bool IsAvailable => true;

        #region Implementation
        /// <summary>
        /// Starts the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="waitForExit">if set to <c>true</c> [wait for exit].</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="environmentVariables">The environment variables.</param>
        /// <returns>System.Int32.</returns>
        public int Start(
            string path, 
            string arguments, 
            bool waitForExit = true, 
            IExternalProcessEventObserver observer = null,
            CancellationTokenSource cancellationTokenSource = null,
            StringDictionary environmentVariables = null)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = path;
            processStartInfo.Arguments = arguments;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = observer != null;
            processStartInfo.RedirectStandardError = observer != null;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.WorkingDirectory = path.GetDirectoryPath();

            if(environmentVariables!=null)
            {
                foreach(string p in environmentVariables.Keys)
                {
                    processStartInfo.EnvironmentVariables.Add(p, environmentVariables[p]);
                }
            }            

            return Start(processStartInfo, waitForExit, observer, cancellationTokenSource);
        }

        /// <summary>
        /// Starts the specified process start information.
        /// </summary>
        /// <param name="processStartInfo">The process start information.</param>
        /// <param name="waitForExit">if set to <c>true</c> [wait for exit].</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>System.Int32.</returns>
        public int Start(ProcessStartInfo processStartInfo, bool waitForExit = true, IExternalProcessEventObserver observer = null, CancellationTokenSource cancellationTokenSource = null)
        {
            var path = ProcessExecuteFilePath(processStartInfo.FileName);

            if (path != processStartInfo.FileName)
            {
                Logger.Log("Auto change execute file path form {0} to {1}", processStartInfo.FileName, path);
                processStartInfo.FileName = path;
            }

            var process = new Process() { StartInfo = processStartInfo };

            if (!process.Start())
            {
                Logger.LogError("Failed start process {0} {1}", processStartInfo.FileName, processStartInfo.Arguments);
                return -1;
            }

            if (waitForExit)
            {
                try
                {
                    string appName = processStartInfo.FileName.GetFileNameWithoutExtension();

                    if (observer != null)
                    {
                        while (!process.WaitForExit(100))
                        {
                            observer.OnIdle(process);

                            if(cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
                            {
                                try
                                {
                                    observer?.OnEvent(LoggerLevel.Warning, "", "User canceled.");
                                    
                                    process.Kill(true);

                                    observer?.OnCanceled(cancellationTokenSource);

                                    break;
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                    else
                    {
                        process.WaitForExit();
                    }

                    if(observer != null)
                    {
                        if(processStartInfo.RedirectStandardOutput)
                        {
                            var output = process.StandardOutput.ReadToEnd();
                            observer.OnEvent(LoggerLevel.Information, appName, output);
                        }
                        
                        if(processStartInfo.RedirectStandardError)
                        {
                            var output = process.StandardError.ReadToEnd();
                            observer.OnEvent(LoggerLevel.Error, appName, output);
                        }
                    }

                    return process.ExitCode;
                }
                finally
                {
                    process.Dispose();
                }
            }

            return 0;
        }

        /// <summary>
        /// Starts the specified path.
        /// 启动外部进程(异步获取输出)
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="waitForExit">if set to <c>true</c> [wait for exit].</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="environmentVariables">The environment variables.</param>
        /// <returns>System.Int32.</returns>
        public async Task<int> StartAsync(string path, string arguments, bool waitForExit = true, IExternalProcessEventObserver observer = null, CancellationTokenSource cancellationTokenSource = null, StringDictionary environmentVariables = null)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = path;
            processStartInfo.Arguments = arguments;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = observer != null;
            processStartInfo.RedirectStandardError = observer != null;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.WorkingDirectory = path.GetDirectoryPath();

            if (environmentVariables != null)
            {
                foreach (string p in environmentVariables.Keys)
                {
                    processStartInfo.EnvironmentVariables.Add(p, environmentVariables[p]);
                }
            }

            return await StartAsync(processStartInfo, waitForExit, observer, cancellationTokenSource);
        }

        /// <summary>
        /// Starts the specified path.
        /// 启动外部进程(异步获取输出)
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="workingDirectory">The working directory.</param>
        /// <param name="waitForExit">if set to <c>true</c> [wait for exit].</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="environmentVariables">The environment variables.</param>
        /// <returns>System.Int32.</returns>
        public async Task<int> StartAsync(
            string path,
            string arguments,
            string workingDirectory,
            bool waitForExit = true,
            IExternalProcessEventObserver observer = null,
            CancellationTokenSource cancellationTokenSource = null,
            StringDictionary environmentVariables = null
            )
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = path;
            processStartInfo.Arguments = arguments;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = observer != null;
            processStartInfo.RedirectStandardError = observer != null;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.WorkingDirectory = workingDirectory;

            if (environmentVariables != null)
            {
                foreach (string p in environmentVariables.Keys)
                {
                    processStartInfo.EnvironmentVariables.Add(p, environmentVariables[p]);
                }
            }

            return await StartAsync(processStartInfo, waitForExit, observer, cancellationTokenSource);
        }

        /// <summary>
        /// Starts the specified process start information.
        /// 启动外部进程(异步获取输出)
        /// </summary>
        /// <param name="processStartInfo">The process start information.</param>
        /// <param name="waitForExit">if set to <c>true</c> [wait for exit].</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>System.Int32.</returns>
        public async Task<int> StartAsync(ProcessStartInfo processStartInfo, bool waitForExit = true, IExternalProcessEventObserver observer = null, CancellationTokenSource cancellationTokenSource = null)
        {
            return await Task.Run(() => StartProcessCore(processStartInfo, waitForExit, observer, cancellationTokenSource));
        }

        int StartProcessCore(ProcessStartInfo processStartInfo, bool waitForExit, IExternalProcessEventObserver observer, CancellationTokenSource cancellationTokenSource)
        {
            var path = ProcessExecuteFilePath(processStartInfo.FileName);

            if (path != processStartInfo.FileName)
            {
                Logger.Log("Auto change execute file path form {0} to {1}", processStartInfo.FileName, path);
                processStartInfo.FileName = path;
            }

            var process = new Process() { StartInfo = processStartInfo };

            string appName = processStartInfo.FileName.GetFileNameWithoutExtension();

            if (processStartInfo.RedirectStandardOutput)
            {
                process.OutputDataReceived += (s, e) =>
                {
                    // 多线程异步回调
                    if (e.Data.IsNotNullOrEmpty())
                    {
                        observer?.OnEvent(LoggerLevel.Information, appName, e.Data);
                    }
                };
            }

            if (processStartInfo.RedirectStandardError)
            {
                process.ErrorDataReceived += (s, e) =>
                {
                    // 多线程异步回调
                    if (e.Data.IsNotNullOrEmpty())
                    {
                        observer?.OnEvent(LoggerLevel.Error, appName, e.Data);
                    }
                };
            }

            Logger.Log("Start Process: {0} {1}", processStartInfo.FileName, processStartInfo.Arguments);

            if (!process.Start())
            {
                Logger.LogError("Failed start process {0} {1}", processStartInfo.FileName, processStartInfo.Arguments);
                return -1;
            }

            if (processStartInfo.RedirectStandardOutput)
            {
                process.BeginOutputReadLine();
            }

            if (processStartInfo.RedirectStandardError)
            {
                process.BeginErrorReadLine();
            }

            if(waitForExit)
            {
                if (observer != null)
                {
                    while (!process.WaitForExit(100))
                    {
                        observer.OnIdle(process);

                        if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
                        {
                            try
                            {
                                observer?.OnEvent(LoggerLevel.Warning, "", "User canceled.");
                                process.Kill(true);

                                observer?.OnCanceled(cancellationTokenSource);
                                break;
                            }
                            catch
                            {
                            }
                        }
                    }

                    return process.ExitCode;
                }
                else
                {
                    process.WaitForExit();

                    return process.ExitCode;
                }
            }

            return 0;
        }
        #endregion

        #region Patch File Names
        private readonly static List<string> WindowsExecutableFileFormats = new List<string>()
        {
            ".exe",
            ".bat",
            ".com",
            ".cmd",
            ".vbs"
        };

        private static string ProcessExecuteFilePath(string path)
        {
            if (OperatingSystem.IsWindows())
            {
                if (path.IsFileExists())
                {
                    return path;
                }

                string ext = path.GetExtension().ToLower();

                if (ext.IsNullOrEmpty())
                {
                    foreach (var i in WindowsExecutableFileFormats)
                    {
                        var tpath = path + i;

                        if (tpath.IsFileExists())
                        {
                            return tpath;
                        }
                    }
                }
            }
            else if (OperatingSystem.IsMacOS())
            {
                if (path.IsExtension(".app"))
                {
                    var name = path.GetFileNameWithoutExtension();
                    var tpath = path.JoinPath("Contents/MacOS", name);

                    return tpath;
                }
            }

            return path;
        }
        #endregion
    }
    #endregion
}
