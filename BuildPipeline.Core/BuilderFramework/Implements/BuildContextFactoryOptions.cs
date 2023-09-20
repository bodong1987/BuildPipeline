namespace BuildPipeline.Core.BuilderFramework.Implements
{
    /// <summary>
    /// Class BuildContextFactoryOptions.
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.IBuildContextFactoryOptions" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.IBuildContextFactoryOptions" />
    internal class BuildContextFactoryOptions : IBuildContextFactoryOptions
    {
        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>The arguments.</value>
        public string[] Arguments { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildContextFactoryOptions"/> class.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public BuildContextFactoryOptions(string[] args)
        {
            Arguments = args;
        }
    }
}
