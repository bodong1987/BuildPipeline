namespace BuildPipeline.Core.BuilderFramework
{
    /// <summary>
    /// Enum TaskAssociateType
    /// </summary>
    public enum TaskAssociateType
    {
        /// <summary>
        /// All
        /// </summary>
        All,
        
        /// <summary>
        /// The forward
        /// </summary>
        Previous,

        /// <summary>
        /// The back
        /// </summary>
        Next,

        /// <summary>
        /// The previous all
        /// </summary>
        PreviousAll,

        /// <summary>
        /// The next all
        /// </summary>
        NextAll,
    }

    /// <summary>
    /// Interface IBuildTaskStage
    /// </summary>
    public interface IBuildTaskStage : IEnumerable<IBuildTask>
    {
        /// <summary>
        /// Gets the order identifier.
        /// </summary>
        /// <value>The order identifier.</value>
        int OrderId { get; }

        /// <summary>
        /// Gets the pipeline.
        /// </summary>
        /// <value>The pipeline.</value>
        IBuildPipeline Pipeline {get; internal set; }

        /// <summary>
        /// Gets the tasks.
        /// </summary>
        /// <value>The tasks.</value>
        IBuildTask[] Tasks { get; }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        int Count { get; }

        /// <summary>
        /// Gets the previous.
        /// </summary>
        /// <value>The previous.</value>
        IBuildTaskStage Previous { get; }

        /// <summary>
        /// Gets the next.
        /// </summary>
        /// <value>The next.</value>
        IBuildTaskStage Next { get; }
    }

    /// <summary>
    /// Interface IBuildTaskGraph
    /// </summary>
    public interface IBuildTaskGraph : IEnumerable<IBuildTaskStage>
    {
        /// <summary>
        /// Builds the graph.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="includeTasks">The include tasks.</param>
        /// <param name="excludeTasks">The exclude tasks.</param>
        void BuildGraph(IBuildContext context, IEnumerable<IBuildTask> includeTasks, IEnumerable<IBuildTask> excludeTasks);

        /// <summary>
        /// Gets the include tasks.
        /// </summary>
        /// <returns>IBuildTask[].</returns>
        IBuildTask[] GetIncludeTasks();

        /// <summary>
        /// Gets the exclude tasks.
        /// </summary>
        /// <returns>IBuildTask[].</returns>
        IBuildTask[] GetExcludeTasks();

        /// <summary>
        /// Gets all tasks.
        /// </summary>
        /// <returns>IBuildTask[].</returns>
        IBuildTask[] GetAllTasks();

        /// <summary>
        /// Gets the associate tasks.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="associateType">Type of the associate.</param>
        /// <returns>IBuildTask[].</returns>
        IBuildTask[] GetAssociateTasks(IBuildTask task, TaskAssociateType associateType = TaskAssociateType.Next);

        /// <summary>
        /// Finds the task.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>IBuildTask.</returns>
        IBuildTask FindTask(string name);
    }
}
