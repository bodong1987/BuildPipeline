namespace BuildPipeline.Core.BuilderFramework
{
    /// <summary>
    /// Interface IBuildPipeline
    /// </summary>
    public interface IBuildPipeline : IValidatingable
    {
        /// <summary>
        /// The context
        /// </summary>
        IBuildContext Context { get; }

        /// <summary>
        /// The graph
        /// </summary>
        IBuildTaskGraph Graph { get; }

        /// <summary>
        /// Builds the help message.
        /// </summary>
        /// <returns>System.String.</returns>
        string BuildHelpMessage();

        /// <summary>
        /// Populates the task execute command line.
        /// </summary>
        /// <param name="tasks">The tasks.</param>
        /// <param name="method">The method.</param>
        /// <returns>System.String.</returns>
        string PopulateTaskExecuteCommandLine(IBuildTask[] tasks, CommandLineFormatMethod method);
    }
}
