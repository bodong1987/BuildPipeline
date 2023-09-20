using BuildPipeline.Core.BuilderFramework;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Services;
using BuildPipeline.Core.Utils;

namespace BuildPipeline.Core.PythonScripts
{
    /// <summary>
    /// Class PythonScriptRuntimeService.
    /// Implements the <see cref="AbstractService" />
    /// Implements the <see cref="BuildPipeline.Core.Scripts.IScriptRuntimeService" />
    /// </summary>
    /// <seealso cref="AbstractService" />
    /// <seealso cref="BuildPipeline.Core.Scripts.IScriptRuntimeService" />
    [Export]
    internal class PythonScriptRuntimeService : AbstractService, IScriptRuntimeService
    {
        public string[] Extensions => new string[] { ".py" };

        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>The name of the service.</value>
        public override string ServiceName => "Python Script Runtime Service";

        /// <summary>
        /// Accepts the script.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool AcceptScript(string path)
        {
            return path.IsExtension(".py");
        }


        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Initialize()
        {
            PythonEngine.Initialize();

            return PythonEngine.Runtime != null && PythonEngine.IsInitialized;
        }

        /// <summary>
        /// Executes the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>System.Int32.</returns>
        public dynamic Execute(string path)
        {
            Logger.Assert(PythonEngine.IsInitialized);
            Logger.Assert(PythonEngine.Runtime != null);

            var oldFlag = AppFramework.EnableFirstChangeExceptionRecord;

            try
            {
                AppFramework.EnableFirstChangeExceptionRecord = false;

                return PythonEngine.Runtime.Execute(path, PythonEngine.Runtime.Engine.CreateScope());
            }
            finally
            {
                AppFramework.EnableFirstChangeExceptionRecord = oldFlag;
            }
            
        }

        /// <summary>
        /// Executes the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="method">The method.</param>
        /// <returns>dynamic.</returns>
        public dynamic Execute(string path, string method)
        {
            Logger.Assert(PythonEngine.IsInitialized);
            Logger.Assert(PythonEngine.Runtime != null);

            var oldFlag = AppFramework.EnableFirstChangeExceptionRecord;

            try
            {                
                AppFramework.EnableFirstChangeExceptionRecord = false;
                return PythonEngine.Runtime.Execute(path, method, PythonEngine.Runtime.Engine.CreateScope());
            }
            finally
            {
                AppFramework.EnableFirstChangeExceptionRecord = oldFlag;
            }
        }

        /// <summary>
        /// Execute as an asynchronous operation.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="executeMode">The execute mode.</param>
        /// <returns>A Task&lt;System.Int32&gt; representing the asynchronous operation.</returns>
        public async Task<int> ExecuteAsync(string path, string arguments, IExcecuteObserver observer, CancellationTokenSource cancellationTokenSource, ScriptableExecuteMode executeMode)
        {
            if(executeMode == ScriptableExecuteMode.Internal)
            {
                try
                {
                    var result = Execute(path);

                    return 0;
                }
                catch(Exception e)
                {
                    observer.LogError($"Execute script file {path} exception: {e.Message}");
                    return -1;
                }
            }
            else if(executeMode == ScriptableExecuteMode.External)
            {
                IPythonEnvironmentService service = ServiceProvider.GetService<IPythonEnvironmentService>();

                if(service == null || !service.IsAvailable)
                {
                    observer.LogError("Python Environment is not exists.");
                    return -1;
                }

                IExternalProcessService eService = ServiceProvider.GetService<IExternalProcessService>();

                if(eService == null || !eService.IsAvailable)
                {
                    observer.LogError("External Process Service is not exists.");
                    return -1;
                }

                return await eService.StartAsync(service.Python3, $"\"{path}\" {arguments}", path.GetDirectoryPath(), true, observer, cancellationTokenSource);
            }

            return 0;
        }
    }
}
