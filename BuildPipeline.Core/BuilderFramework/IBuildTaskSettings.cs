using BuildPipeline.Core.Serialization;

namespace BuildPipeline.Core.BuilderFramework
{
    /// <summary>
    /// Interface IBuildTaskSettings
    /// </summary>
    public interface IBuildTaskSettings
    {
        /// <summary>
        /// Gets the name of the task.
        /// </summary>
        /// <value>The name of the task.</value>
        string TaskName { get; }

        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        string TaskDescription { get; }

        /// <summary>
        /// Gets the task order.
        /// </summary>
        /// <value>The task order.</value>
        int TaskOrder { get; }

        /// <summary>
        /// Gets a value indicating whether this instance can asynchronous.
        /// </summary>
        /// <value><c>true</c> if this instance can asynchronous; otherwise, <c>false</c>.</value>
        bool CanAsync { get; }

        /// <summary>
        /// Gets a value indicating whether [wait result].
        /// </summary>
        /// <value><c>true</c> if [wait result]; otherwise, <c>false</c>.</value>
        bool WaitResult { get; }

        /// <summary>
        /// Gets a value indicating whether this instance can failure.
        /// </summary>
        /// <value><c>true</c> if this instance can failure; otherwise, <c>false</c>.</value>
        bool CanFailure { get; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        IBuildTaskOptions Options { get; }

        /// <summary>
        /// Formats the options command line.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>System.String.</returns>
        string FormatOptionsCommandLine(CommandLineFormatMethod method);
    }

    /// <summary>
    /// Class BuildTaskSettings.
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.IBuildTaskSettings" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.IBuildTaskSettings" />    
    [ObjectXmlSerializePolicy(XmlSerializationPolicy.IgnoreFields)]
    public class BuildTaskSettings : IBuildTaskSettings
    {
        /// <summary>
        /// Gets the name of the task.
        /// </summary>
        /// <value>The name of the task.</value>
        public string TaskName { get; internal set; } = "";

        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public virtual string TaskDescription { get; internal set; } = "";

        /// <summary>
        /// Gets the task order.
        /// </summary>
        /// <value>The task order.</value>
        public int TaskOrder { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether this instance can asynchronous.
        /// </summary>
        /// <value><c>true</c> if this instance can asynchronous; otherwise, <c>false</c>.</value>
        public bool CanAsync { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether [wait result].
        /// </summary>
        /// <value><c>true</c> if [wait result]; otherwise, <c>false</c>.</value>
        public bool WaitResult { get; internal set; } = true;

        /// <summary>
        /// Gets a value indicating whether this instance can failure.
        /// </summary>
        /// <value><c>true</c> if this instance can failure; otherwise, <c>false</c>.</value>
        public bool CanFailure { get; internal set; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        public IBuildTaskOptions Options { get; internal set; }

        /// <summary>
        /// Formats the options command line.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>System.String.</returns>
        public virtual string FormatOptionsCommandLine(CommandLineFormatMethod method)
        {
            if(Options != null)
            {
                return Options.FormatCommandLine(method);
            }

            return "";
        }
    }
}
