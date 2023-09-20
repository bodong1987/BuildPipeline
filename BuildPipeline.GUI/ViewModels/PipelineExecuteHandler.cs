using Avalonia.Threading;
using BuildPipeline.Core.BuilderFramework;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using PropertyModels.ComponentModel;
using System.Diagnostics;

namespace BuildPipeline.GUI.ViewModels
{
    /// <summary>
    /// Class PipelineExecuteHandler.
    /// </summary>
    public class PipelineExecuteHandler : ReactiveObject, IBuildTaskExecuteObserver
    {
        /// <summary>
        /// The pipeline
        /// </summary>
        public readonly IBuildPipeline Pipeline;

        /// <summary>
        /// The data model
        /// </summary>
        public readonly PipelineViewModel DataModel;

        private bool IsExecutingCore;
        /// <summary>
        /// Gets a value indicating whether this instance is executing.
        /// </summary>
        /// <value><c>true</c> if this instance is executing; otherwise, <c>false</c>.</value>
        public bool IsExecuting
        {
            get => IsExecutingCore;
            set => this.RaiseAndSetIfChanged(ref IsExecutingCore, value);
        }

        BasePipelineNode RunningNode = null;
        CancellationTokenSource RunningSource = null;
        AutoResetEvent CancelEvent;

        private bool IsStoppingCore;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is stopping.
        /// </summary>
        /// <value><c>true</c> if this instance is stopping; otherwise, <c>false</c>.</value>
        public bool IsStopping
        {
            get => IsStoppingCore;
            set => this.RaiseAndSetIfChanged(ref IsStoppingCore, value);
        }

        /// <summary>
        /// Occurs when [pipeline execute finished].
        /// </summary>
        public event EventHandler<PipelineExecuteEventArgs> PipelineExecuteFinished;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineExecuteHandler"/> class.
        /// </summary>
        /// <param name="dataModel">The data model.</param>
        public PipelineExecuteHandler(PipelineViewModel dataModel)
        {
            DataModel = dataModel;
            Pipeline = dataModel.Pipeline;
        }

        /// <summary>
        /// Executes all tasks.
        /// </summary>
        public void ExecuteAllTasks()
        {
            ExecuteStages(DataModel.Nodes.ToArray());
        }

        /// <summary>
        /// Executes the selected tasks.
        /// </summary>
        public void ExecuteSelectedTasks()
        {
            ExecuteStages(DataModel.SelectedNodes.ToArray());
        }

        private async void ExecuteStages(IEnumerable<BasePipelineNode> nodes)
        {
            if (IsExecuting)
            {
                Logger.LogError("Can't execute stages when current stages are running...");
                return;
            }

            IsExecuting = true;

            int ExeResult = -1;

            try
            {
                RunningSource = new CancellationTokenSource();

                IBuildPipelineExecuteService service = ServiceProvider.GetService<IBuildPipelineExecuteService>();
                Logger.Assert(service != null);

                foreach (var node in nodes)
                {
                    RunningNode = node;

                    DataModel.SelectedNode = node;                    

                    node.ExecuteResultType = TaskExecuteResultType.Executing;

                    node.ClearExecuteOutput();

                    try
                    {
                        ExeResult = await Task.Run(() =>
                        {
                            BuildPipelineExecuteType executeType = BuildPipelineExecuteType.ExternalProc;

#if DEBUG
                            if(Debugger.IsAttached)
                            {
                                executeType = BuildPipelineExecuteType.InternalProc;
                            }                            
#endif
                            if(node is PipelineStageNode stageNode)
                            {
                                return service.Execute(stageNode.Stage, this, RunningSource, executeType);
                            }
                            else if(node is PipelineTaskNode taskNode)
                            {
                                return service.Execute(taskNode.Task, this, RunningSource, executeType);
                            }

                            return -1;
                        });

                        node.ExecuteResultType = RunningSource.IsCancellationRequested ?
                            TaskExecuteResultType.Canceled :
                            (ExeResult == 0 ? TaskExecuteResultType.Success : TaskExecuteResultType.Failure);                        
                    }
                    catch(Exception e)
                    {
                        Logger.LogError("{0}", e.Message);
                        node.ExecuteResultType= TaskExecuteResultType.Failure;                        
                    }
                    finally
                    {
                        RunningNode = null;
                    }

                    if(RunningSource.Token.IsCancellationRequested)
                    {
                        Logger.Log("task is canceled.");
                        break;
                    }

                    if(node.ExecuteResultType != TaskExecuteResultType.Success)
                    {
                        break;
                    }
                }
            }
            finally
            {
                IsExecuting = false;
                IsStopping = false;

                bool isCanceled = RunningSource.IsCancellationRequested;

                RunningSource.Dispose();
                RunningSource = null;

                PipelineExecuteFinished?.Invoke(this, new PipelineExecuteEventArgs()
                {
                    ExecuteResult = ExeResult,
                    IsCanceled = isCanceled
                });
            }
        }

        /// <summary>
        /// Stops all tasks.
        /// </summary>
        public void StopAllTasks()
        {
            StopAllTasksAndWait(false);
        }

        /// <summary>
        /// Stops all tasks and wait.
        /// </summary>
        /// <param name="waitCancel">if set to <c>true</c> [wait cancel].</param>
        public void StopAllTasksAndWait(bool waitCancel = false)
        {
            if (IsStopping || !IsExecuting)
            {
                return;
            }

            IsStopping = true;

            Logger.Assert(RunningSource != null);

            if (RunningSource != null)
            {
                try
                {
                    if (waitCancel)
                    {
                        CancelEvent = new AutoResetEvent(false);                        
                    }

                    RunningSource.Cancel();

                    if(waitCancel)
                    {
                        Logger.Log("Need wait task cancel.");
                        CancelEvent.WaitOne();
                    }
                }
                finally
                {
                    CancelEvent?.Dispose();
                }                
            }
        }

        /// <summary>
        /// Called when [progress].
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="progress">The progress.[0,100]</param>
        public void OnProgress(IBuildTask task, int progress)
        {

        }

        /// <summary>
        /// Called when [idle].
        /// </summary>
        public void OnIdle(Process process)
        {
        }

        /// <summary>
        /// Called when [receive].
        /// May called from any thread...
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="message">The message.</param>
        public void OnEvent(Core.Utils.LoggerLevel level, string tag, string message)
        {
            var rNode = RunningNode;
            if (rNode != null)
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    rNode.AppendLog(message);                    
                });
            }
        }

        /// <summary>
        /// Called when [canceled].
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        public void OnCanceled(CancellationTokenSource cancellationTokenSource)
        {
            Logger.Log("Cancel Finished, raise event.");
            CancelEvent?.Set();
        }
    }

    /// <summary>
    /// Class PipelineExecuteEventArgs.
    /// Implements the <see cref="EventArgs" />
    /// </summary>
    /// <seealso cref="EventArgs" />
    public class PipelineExecuteEventArgs : EventArgs
    {
        /// <summary>
        /// The execute result
        /// </summary>
        public int ExecuteResult;

        /// <summary>
        /// The is canceled
        /// </summary>
        public bool IsCanceled;
    }
}
