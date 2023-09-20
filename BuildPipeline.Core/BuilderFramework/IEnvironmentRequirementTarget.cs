namespace BuildPipeline.Core.BuilderFramework
{
    /// <summary>
    /// Interface IEnvironmentRequirementTarget
    /// </summary>
    public interface IEnvironmentRequirementTarget
    {
        /// <summary>
        /// Gets the requirements.
        /// </summary>
        /// <value>The requirements.</value>
        IEnvironmentRequirementCollection Requirements { get; }
    }
}
