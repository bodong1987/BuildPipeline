using BuildPipeline.Core.Framework;

namespace BuildPipeline.Core.BuilderFramework
{
    /// <summary>
    /// Interface IBuildTaskCollector
    /// Implements the <see cref="IImportable" />
    /// </summary>
    /// <seealso cref="IImportable" />
    public interface IBuildTaskCollector : IImportable
    {
        /// <summary>
        /// Collects the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="existsTasks">The exists tasks.</param>
        /// <returns>IBuildTask[].</returns>
        void Collect(IBuildContext context, IList<IBuildTask> existsTasks);
    }
}
