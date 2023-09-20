using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Services;
using BuildPipeline.Core.Utils;
using PropertyModels.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace BuildPipeline.Core.BuilderFramework.Implements
{
    /// <summary>
    /// Class BuildPipelineBuiltinExecuteService.
    /// Implements the <see cref="AbstractService" />
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.IBuildPipelineExecuteService" />
    /// </summary>
    /// <seealso cref="AbstractService" />
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.IBuildPipelineExecuteService" />
    [Export]
    public class BuildPipelineBuiltinExecuteService : AbstractService, IBuildPipelineExecuteService
    {
        string ExternalProcPath = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildPipelineBuiltinExecuteService"/> class.
        /// </summary>
        public BuildPipelineBuiltinExecuteService()
        {
            string path = null;

            if (OperatingSystem.IsWindows())
            {
                path = EnvironmentUtils.SearchExecutableFile("BuildPipeline.Proc.exe");
            }
            else
            {
                path = EnvironmentUtils.SearchExecutableFile("BuildPipeline.Proc.dll");
            }

            if(path.IsFileExists())
            {
                ExternalProcPath = path;
            }
            else
            {
                Logger.LogError("Failed find BuildPipeline.Proc");
            }
        }

        /// <summary>
        /// Executes the specified pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="executeType">Type of the execute.</param>
        /// <returns>System.Int32.</returns>
        public int Execute(IBuildPipeline pipeline, IBuildTaskExecuteObserver observer, CancellationTokenSource cancellationTokenSource = null, BuildPipelineExecuteType executeType = BuildPipelineExecuteType.InternalProc)
        {
            foreach (var stage in pipeline.Graph)
            {
                int result = RunStage(pipeline, stage, observer, cancellationTokenSource, executeType);

                if (result != 0)
                {
                    return result;
                }
            }

            return 0;
        }

        /// <summary>
        /// Executes the specified stage.
        /// </summary>
        /// <param name="stage">The stage.</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="executeType">Type of the execute.</param>
        /// <returns>System.Int32.</returns>
        public int Execute(IBuildTaskStage stage, IBuildTaskExecuteObserver observer, CancellationTokenSource cancellationTokenSource = null, BuildPipelineExecuteType executeType = BuildPipelineExecuteType.InternalProc)
        {
            Logger.Assert(stage != null && stage.Pipeline != null);

            return RunStage(stage.Pipeline, stage, observer, cancellationTokenSource, executeType);
        }

        /// <summary>
        /// Executes the specified task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="executeType">Type of the execute.</param>
        /// <returns>System.Int32.</returns>
        public int Execute(IBuildTask task, IBuildTaskExecuteObserver observer, CancellationTokenSource cancellationTokenSource, BuildPipelineExecuteType executeType)
        {
            Logger.Assert(task != null && task.Stage != null && task.Pipeline != null);

            return RunTaskSync(task.Pipeline, task, observer, cancellationTokenSource, executeType);
        }


        /// <summary>
        /// Runs the stage.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="stage">The stage.</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="executeType">Type of the execute.</param>
        /// <returns>System.Int32.</returns>
        private int RunStage(IBuildPipeline pipeline, IBuildTaskStage stage, IBuildTaskExecuteObserver observer, CancellationTokenSource cancellationTokenSource, BuildPipelineExecuteType executeType)
        {
            var tasks = stage.Tasks;

            if (tasks.Length == 0)
            {
                return 0;
            }

            if (tasks.Length == 1)
            {
                return RunTaskSync(pipeline, tasks.FirstOrDefault(), observer, cancellationTokenSource, executeType);
            }
            else
            {
                List<Task<int>> list = new List<Task<int>>();

                bool NeedWait = false;

                foreach (var task in tasks)
                {
                    var t = Task.Factory.StartNew(() =>
                    {
                        return RunTaskSync(pipeline, task, observer, cancellationTokenSource, executeType);
                    });

                    list.Add(t);

                    NeedWait |= task.Settings.WaitResult;
                }

                if (NeedWait)
                {
                    Task.WaitAll(list.ToArray());

                    foreach (var i in list)
                    {
                        if (i.Result != 0)
                        {
                            return i.Result;
                        }
                    }
                }

                return 0;
            }
        }

        /// <summary>
        /// Runs the task synchronize.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="task">The task.</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="executeType">Type of the execute.</param>
        /// <returns>System.Int32.</returns>
        private int RunTaskSync(IBuildPipeline pipeline, IBuildTask task, IBuildTaskExecuteObserver observer, CancellationTokenSource cancellationTokenSource, BuildPipelineExecuteType executeType)
        {
            Logger.Assert(task != null);

            var checkResult = task.CheckValidation();

            if (checkResult.IsFailure())
            {
                observer.OnEvent(LoggerLevel.Error, task.Settings.TaskName, $"{checkResult.ErrorMessage}");
                return -1;
            }

            Logger.Log("Begin execute task [{0}]<<<{1}>>>", task.ImportPriority, task.Settings.TaskName);
            Stopwatch stopwatch = Stopwatch.StartNew();

            int ResultCode = -1;

            if (executeType == BuildPipelineExecuteType.InternalProc ||
                executeType == BuildPipelineExecuteType.Auto)
            {
                var t = task.ExecuteAsync(pipeline.Context, new InternalBuildTaskExecuteObserver(task, observer), cancellationTokenSource);
                t.Wait();
                ResultCode = t.Result;
            }
            else
            {
                IExternalProcessService service = ServiceProvider.GetService<IExternalProcessService>();
                Logger.Assert(service != null);

                if(!ExternalProcPath.IsFileExists())
                {
                    Logger.LogError("External Proc Path is not exists. please check your build tools environment.");

                    return ResultCode;
                }

                // format pipeline name                
                var arguments = pipeline.PopulateTaskExecuteCommandLine(new IBuildTask[] { task }, CommandLineFormatMethod.Simplify);

                Task<int> t = null;

                if(OperatingSystem.IsWindows())
                {
                    t = service.StartAsync(ExternalProcPath, arguments, true, new InternalBuildTaskExecuteObserver(task, observer), cancellationTokenSource);
                }
                else
                {
                    IDotnetEnvironmentService dotnetService = ServiceProvider.GetService<IDotnetEnvironmentService>();

                    Logger.Assert(dotnetService != null && dotnetService.IsAvailable);                                        

                    t = service.StartAsync(dotnetService.DotnetPath, $" \"{ExternalProcPath}\" " + arguments, true, new InternalBuildTaskExecuteObserver(task, observer), cancellationTokenSource);
                }
                t.Wait();
                ResultCode = t.Result;
            }

            Logger.Log("Finish execute task [{0}]<<<{1}>>> Exit Code: {2} TimeUsage: {3}", task.ImportPriority, task.Settings.TaskName, ResultCode, stopwatch.Elapsed);

            return ResultCode;
        }
    }
}
