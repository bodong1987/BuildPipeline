using BuildPipeline.Core.Framework;

namespace BuildPipeline.Core.BuilderFramework
{
    /// <summary>
    /// Enum BuildPipelineExecuteType
    /// </summary>
    public enum BuildPipelineExecuteType
    {
        /// <summary>
        /// The external proc
        /// </summary>
        ExternalProc,

        /// <summary>
        /// The internal proc
        /// </summary>
        InternalProc,

        /// <summary>
        /// The automatic
        /// </summary>
        Auto = InternalProc
    }

    /// <summary>
    /// Interface IBuildPipelineExecuteService
    /// Implements the <see cref="IService" />
    /// </summary>
    /// <seealso cref="IService" />
    public interface IBuildPipelineExecuteService : IService
    {
        /// <summary>
        /// Executes the specified pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="executeType">Type of the execute.</param>
        /// <returns>System.Int32.</returns>
        int Execute(IBuildPipeline pipeline, IBuildTaskExecuteObserver observer, CancellationTokenSource cancellationTokenSource = null, BuildPipelineExecuteType executeType= BuildPipelineExecuteType.Auto);

        /// <summary>
        /// Executes the specified stage.
        /// </summary>
        /// <param name="stage">The stage.</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="executeType">Type of the execute.</param>
        /// <returns>System.Int32.</returns>
        int Execute(IBuildTaskStage stage, IBuildTaskExecuteObserver observer, CancellationTokenSource cancellationTokenSource = null, BuildPipelineExecuteType executeType = BuildPipelineExecuteType.Auto);

        /// <summary>
        /// Executes the specified task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="executeType">Type of the execute.</param>
        /// <returns>System.Int32.</returns>
        int Execute(IBuildTask task, IBuildTaskExecuteObserver observer, CancellationTokenSource cancellationTokenSource = null, BuildPipelineExecuteType executeType = BuildPipelineExecuteType.Auto);
    }
}
