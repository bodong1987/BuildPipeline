using BuildPipeline.Core.Utils;
using System.Diagnostics;

namespace BuildPipeline.Core.BuilderFramework.Implements
{
    /// <summary>
    /// Class BuildTaskExecuteObserver.
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.IBuildTaskExecuteObserver" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.IBuildTaskExecuteObserver" />
    internal class BuildTaskExecuteObserver : IBuildTaskExecuteObserver
    {
        public virtual void OnCanceled(CancellationTokenSource cancellationTokenSource)
        {
        }

        /// <summary>
        /// Called when [idle].
        /// </summary>
        public virtual void OnIdle(Process process)
        {            
        }

        /// <summary>
        /// Called when [progress].
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="progress">The progress.[0,100]</param>
        public virtual void OnProgress(IBuildTask task, int progress)
        {
            Logger.Log(LoggerLevel.Verbose, task.Settings.TaskName, $"{task.Settings.TaskName} : {progress}%");
        }

        /// <summary>
        /// Called when [receive].
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="message">The message.</param>
        public virtual void OnEvent(LoggerLevel level, string tag, string message)
        {
            Logger.Log(level, tag, message);
        }
    }
}
