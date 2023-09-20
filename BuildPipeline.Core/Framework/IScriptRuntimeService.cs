using BuildPipeline.Core.BuilderFramework;

namespace BuildPipeline.Core.Framework
{
    /// <summary>
    /// Enum ScriptableExecuteMode
    /// </summary>
    public enum ScriptableExecuteMode
    {
        /// <summary>
        /// The internal
        /// </summary>
        Internal,

        /// <summary>
        /// The external
        /// </summary>
        External
    }

    /// <summary>
    /// Interface IScriptRuntimeService
    /// Implements the <see cref="IService" />
    /// </summary>
    /// <seealso cref="IService" />
    public interface IScriptRuntimeService : IService
    {
        /// <summary>
        /// Gets the extensions.
        /// </summary>
        /// <value>The extensions.</value>
        string[] Extensions { get; }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool Initialize();

        /// <summary>
        /// Accepts the script.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool AcceptScript(string path);

        /// <summary>
        /// Executes the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>System.Int32.</returns>
        dynamic Execute(string path);

        /// <summary>
        /// Executes the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="method">The method.</param>
        /// <returns>dynamic.</returns>
        dynamic Execute(string path, string method);

        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="executeMode">The execute mode.</param>
        /// <returns>Task&lt;System.Int32&gt;.</returns>
        Task<int> ExecuteAsync(string path,
            string arguments,
            IExcecuteObserver observer, 
            CancellationTokenSource cancellationTokenSource, 
            ScriptableExecuteMode executeMode
            );
    }
}
