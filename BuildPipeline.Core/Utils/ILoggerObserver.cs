using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Framework;
using System.Diagnostics;
using PropertyModels.Extensions;

namespace BuildPipeline.Core.Utils
{
    /// <summary>
    /// Enum LoggerLevel
    /// </summary>
    public enum LoggerLevel
    {
        /// <summary>
        /// The verbose
        /// </summary>
        Verbose = -10,

        /// <summary>
        /// The information
        /// </summary>
        Information = -9,

        /// <summary>
        /// The warning
        /// </summary>
        Warning = -8,

        /// <summary>
        /// The error
        /// </summary>
        Error = -7,

        /// <summary>
        /// The total
        /// </summary>
        Total = -5
    }

    /// <summary>
    /// Interface ILoggerObserver
    /// </summary>
    public interface ILoggerObserver
    {
        /// <summary>
        /// Called when [receive].
        /// May called from any thread...
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="message">The message.</param>
        void OnEvent(LoggerLevel level, string tag, string message);
    }

    /// <summary>
    /// Class SystemLoggerObserver.
    /// Implements the <see cref="BuildPipeline.Core.Utils.ILoggerObserver" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Utils.ILoggerObserver" />
    public class SystemLoggerObserver : ILoggerObserver
    {
        /// <summary>
        /// Called when [receive].
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="message">The message.</param>
        public void OnEvent(LoggerLevel level, string tag, string message)
        {
            Logger.Log(level, tag, message);
        }
    }

    /// <summary>
    /// Class DebuggerLoggerObserver.
    /// Implements the <see cref="BuildPipeline.Core.Utils.ILoggerObserver" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Utils.ILoggerObserver" />
    public class DebuggerLoggerObserver : ILoggerObserver
    {
        /// <summary>
        /// Called when [receive].
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="message">The message.</param>
        public void OnEvent(LoggerLevel level, string tag, string message)
        {            
            Debug.WriteLine(message);

            var oldColor = Console.ForegroundColor;

            if (level == LoggerLevel.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("[Error]" + message);
            }
            else if (level == LoggerLevel.Warning)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;

                Console.WriteLine("[Warning]" + message);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;

                Console.WriteLine(message);
            }

            Console.ForegroundColor = oldColor;
        }
    }

    /// <summary>
    /// Class LoggerFileObserver.
    /// Implements the <see cref="BuildPipeline.Core.Utils.ILoggerObserver" />
    /// Implements the <see cref="IDisposable" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Utils.ILoggerObserver" />
    /// <seealso cref="IDisposable" />
    public class LoggerFileObserver : ILoggerObserver, IDisposable
    {
        /// <summary>
        /// The log stream
        /// </summary>
        private FileStream LogStream;

        /// <summary>
        /// The file path
        /// </summary>
        public readonly string FilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerFileObserver"/> class.
        /// </summary>
        public LoggerFileObserver() :
            this(GenLogFilename())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerFileObserver"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public LoggerFileObserver(string path)
        {
            FilePath = path;

            var dir = path.GetDirectoryPath();

            if (!dir.IsDirectoryExists())
            {
                Directory.CreateDirectory(dir);
            }
            else
            {
                try
                {
                    // delete all old files
                    string[] Files = Directory.GetFiles(dir, "*.*");

                    // 日志最多保留7天
                    var OutDateFlag = DateTime.Now.AddDays(-7);
                    foreach (var f in Files)
                    {
                        FileInfo fi = new FileInfo(f);
                                                
                        if (fi.CreationTime < OutDateFlag)
                        {
                            try
                            {
                                fi.Delete();
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                catch
                {

                }
            }

            LogStream = new FileStream(
                FilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read
                );
        }

        /// <summary>
        /// Gens the log filename.
        /// </summary>
        /// <returns>System.String.</returns>
        static string GenLogFilename()
        {
            var name = AppFramework.GetPublishApplicationPath().GetFileNameWithoutExtension();
            var LogDir = AppFramework.GetPublishApplicationDirectory().JoinPath("logs");
            var logFilePath = Path.Combine(LogDir, name + "-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", System.Globalization.CultureInfo.InvariantCulture) + ".log");

            return logFilePath;
        }


        /// <summary>
        /// Called when [receive].
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="message">The message.</param>
        public void OnEvent(LoggerLevel level, string tag, string message)
        {
            if (LogStream != null && message.IsNotNullOrEmpty())
            {
                lock (LogStream)
                {
                    var time = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]", System.Globalization.CultureInfo.InvariantCulture);
                    var detailTag = tag.IsNotNullOrEmpty() ? $"[{tag}]" : "";

                    var bytes = System.Text.Encoding.UTF8.GetBytes($"{time}{detailTag}{message.Trim()}{Environment.NewLine}");
                    LogStream.Write(bytes, 0, bytes.Length);
                    LogStream.Flush();
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (LogStream != null)
            {
                LogStream.Flush();
                LogStream.Dispose();
                LogStream = null;
            }
        }
    }
}
