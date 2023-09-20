using BuildPipeline.Core.Framework;
using System.Collections;
using System.Diagnostics;
using System.Text;

namespace BuildPipeline.Core.Utils
{
    /// <summary>
    /// Delegate LoggerEventHandler
    /// </summary>
    /// <param name="level">The level.</param>
    /// <param name="tag">The tag.</param>
    /// <param name="message">The message.</param>
    public delegate void LoggerEventHandler(LoggerLevel level, string tag, string message);

    /// <summary>
    /// Class Logger.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Occurs when [on system log event].
        /// </summary>
        public static event LoggerEventHandler OnSystemLogEvent;

        /// <summary>
        /// The file observer
        /// </summary>
        public readonly static LoggerFileObserver FileReceiver;

        /// <summary>
        /// The debugger observer
        /// </summary>
        public readonly static DebuggerLoggerObserver DebuggerReceiver;

        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        /// <value>The level.</value>
        public static LoggerLevel Level { get; set; } = LoggerLevel.Information;

        /// <summary>
        /// Gets a value indicating whether this instance is verbose enabled.
        /// </summary>
        /// <value><c>true</c> if this instance is verbose enabled; otherwise, <c>false</c>.</value>
        public static bool IsVerboseEnabled => Level <= LoggerLevel.Verbose;

        /// <summary>
        /// Gets a value indicating whether this instance is common information enabled.
        /// </summary>
        /// <value><c>true</c> if this instance is common infomation enabled; otherwise, <c>false</c>.</value>
        public static bool IsCommonInfomationEnabled => Level <= LoggerLevel.Information;

        /// <summary>
        /// Gets a value indicating whether this instance is warning enabled.
        /// </summary>
        /// <value><c>true</c> if this instance is warning enabled; otherwise, <c>false</c>.</value>
        public static bool IsWarningEnabled => Level <= LoggerLevel.Warning;

        /// <summary>
        /// Gets a value indicating whether this instance is error enabled.
        /// </summary>
        /// <value><c>true</c> if this instance is error enabled; otherwise, <c>false</c>.</value>
        public static bool IsErrorEnabled => Level <= LoggerLevel.Error;

        static Logger()
        {
            try
            {
                FileReceiver = new LoggerFileObserver();
                DebuggerReceiver = new DebuggerLoggerObserver();

                OnSystemLogEvent += (s, t, e) => 
                { 
                    FileReceiver.OnEvent(s, t, e);
                    DebuggerReceiver.OnEvent(s, t, e);
                };

                FileReceiver.OnEvent(LoggerLevel.Information, "", "Log File System Startup...");

                FileReceiver.OnEvent(LoggerLevel.Information, "", $"Application File:{AppFramework.ApplicationPath}");
                FileReceiver.OnEvent(LoggerLevel.Information, "", $"Application Temporary Directory:{AppFramework.ApplicationTempDirectory}");

                if (Environment.CommandLine.Contains("--verbose"))
                {
                    Level = LoggerLevel.Verbose;
                    FileReceiver.OnEvent(LoggerLevel.Information, "", "Verbose Mode Enabled.");
                }

                foreach (var configPath in AppFramework.ConfigurationDirectories)
                {
                    FileReceiver.OnEvent(LoggerLevel.Information, "", $"Config Directory:{configPath}");
                }

                if(IsVerboseEnabled)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine("Environment Variables:");

                    List<DictionaryEntry> caches = new List<DictionaryEntry>();
                    caches.AddRange(Environment.GetEnvironmentVariables().Cast<DictionaryEntry>());
                    caches.Sort((x, y) =>
                    {
                        return Comparer<string>.Default.Compare(x.Key?.ToString(), y.Key?.ToString());
                    });

                    foreach (var i in caches)
                    {
                        builder.AppendLine($"    {i.Key} = {(i.Value ?? "")}");
                    }

                    FileReceiver.OnEvent(LoggerLevel.Verbose, "", builder.ToString());
                }
            }
            catch(Exception e)
            {
                Logger.LogError(e.Message);
            }
        }

        /// <summary>
        /// Logs the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="message">The message.</param>
        public static void Log(LoggerLevel level, string tag, string message)
        {
            if(level >= Level)
            {
                OnSystemLogEvent?.Invoke(level, tag, message);
            }            
        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void LogError(string format, params object[] args)
        {
            if(args!=null && args.Length > 0)
            {
                Log(LoggerLevel.Error, null, string.Format(format,args));
            }
            else
            {
                Log(LoggerLevel.Error, null, format);
            }
        }

        /// <summary>
        /// Logs the warning.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void LogWarning(string format, params object[] args)
        {
            if(!IsWarningEnabled)
            {
                return;
            }

            if (args != null && args.Length > 0)
            {
                Log(LoggerLevel.Warning, null, string.Format(format, args));
            }
            else
            {
                Log(LoggerLevel.Warning, null, format);
            }
        }

        /// <summary>
        /// Logs the specified format.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void Log(string format, params object[] args)
        {
            if(!IsCommonInfomationEnabled)
            {
                return;
            }

            if (args != null && args.Length > 0)
            {
                Log(LoggerLevel.Information, null, string.Format(format, args));
            }
            else
            {
                Log(LoggerLevel.Information, null, format);
            }
        }

        /// <summary>
        /// Logs the verbose.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void LogVerbose(string format, params object[] args)
        {
            if(!IsVerboseEnabled)
            {
                return;
            }

            if (args != null && args.Length > 0)
            {
                Log(LoggerLevel.Verbose, null, string.Format(format, args));
            }
            else
            {
                Log(LoggerLevel.Verbose, null, format);
            }
        }

        // <summary>
        /// <summary>
        /// Asserts the specified condition.
        /// </summary>
        /// <param name="condition">if set to <c>true</c> [condition].</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <font color="red">Badly formed XML comment.</font>
        [Conditional("DEBUG")]
        public static void Assert(bool condition, string format, params object[] args)
        {
            if (condition)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();

            if(args != null && args.Length > 0)
            {
                sb.Append("Assertion:");
                sb.AppendLine(string.Format(format, args));
                sb.Append(Environment.StackTrace);

                LogError(sb.ToString());
            }
            else
            {
                sb.Append("Assertion:");
                sb.AppendLine(format);
                sb.Append(Environment.StackTrace);

                LogError(sb.ToString());
            }            

            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Asserts the specified b condition.
        /// </summary>
        /// <param name="condition">if set to <c>true</c> [condition].</param>
        [Conditional("DEBUG")]
        public static void Assert(bool condition)
        {
            if (condition)
            {
                return;
            }

            LogError($"Assertion:{Environment.NewLine}{Environment.StackTrace}");

            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Ensures the specified condition.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition">if set to <c>true</c> [condition].</param>
        /// <param name="params">The parameters.</param>        
        public static void Ensure<T>(bool condition, params object[] @params)
            where T : Exception, new()
        {
            if (!condition)
            {
                try
                {
                    if (@params != null && @params.Length > 0)
                    {
                        var exp = Activator.CreateInstance(typeof(T), @params) as Exception;
                        throw exp; //-V3141
                    }
                    else
                    {
                        throw new T();
                    }
                }
                catch
                {
                    throw new T();
                }
            }
        }
    }
}
