using BuildPipeline.Core;
using BuildPipeline.Core.BuilderFramework;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Services;
using BuildPipeline.GUI.Utils;
using PropertyModels.ComponentModel;
using PropertyModels.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using System.Text;

namespace BuildPipeline.GUI.ViewModels
{
    #region Task Nodes
    /// <summary>
    /// Enum TaskExecuteResultType
    /// </summary>
    public enum TaskExecuteResultType
    {
        /// <summary>
        /// The not executed
        /// </summary>
        NotExecuted = -3,

        /// <summary>
        /// The executing
        /// </summary>
        Executing = -2,

        /// <summary>
        /// The failure
        /// </summary>
        Failure = -1,

        /// <summary>
        /// The success
        /// </summary>
        Success = 0,

        /// <summary>
        /// The canceled
        /// </summary>
        Canceled = 1,
    }


    /// <summary>
    /// Class BasePipelineNode.
    /// </summary>
    public abstract class BasePipelineNode : ReactiveObject
    {
        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        /// <value>The children.</value>
        public ObservableCollection<BasePipelineNode> Children { get; set; } = new ObservableCollection<BasePipelineNode>();

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        TaskExecuteResultType ExecuteResultTypeCore = TaskExecuteResultType.NotExecuted;
        /// <summary>
        /// Gets or sets the type of the execute result.
        /// </summary>
        /// <value>The type of the execute result.</value>
        public TaskExecuteResultType ExecuteResultType
        {
            get => ExecuteResultTypeCore;
            set => this.RaiseAndSetIfChanged(ref ExecuteResultTypeCore, value);
        }
        
        /// <summary>
        /// Gets or sets the execute output.
        /// </summary>
        /// <value>The execute output.</value>
        public StringBuilder ExecuteOutput { get; set; } = new StringBuilder();

        /// <summary>
        /// Gets or sets the execute command line.
        /// </summary>
        /// <value>The execute command line.</value>
        [DependsOnProperty(nameof(FormatMethod))]
        public string ExecuteCommandLine { get; set; } = null;
                
        CommandLineFormatMethod FormatMethodCore = ApplicationSettings.Default.CmdFormatMethod;
        /// <summary>
        /// Gets or sets the format method.
        /// </summary>
        /// <value>The format method.</value>
        public CommandLineFormatMethod FormatMethod
        {
            get => FormatMethodCore;
            set
            {
                if(FormatMethodCore != value)
                {
                    FormatMethodCore = value;

                    OnPopulateCommandLine();

                    RaisePropertyChanged(nameof(FormatMethod));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is not executed.
        /// </summary>
        /// <value><c>true</c> if this instance is not executed; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(ExecuteResultType))]
        public bool IsNotExecuted => ExecuteResultType == TaskExecuteResultType.NotExecuted;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is executing.
        /// </summary>
        /// <value><c>true</c> if this instance is executing; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(ExecuteResultType))]
        public bool IsExecuting => ExecuteResultType == TaskExecuteResultType.Executing;

        /// <summary>
        /// Gets a value indicating whether this instance is execute failure.
        /// </summary>
        /// <value><c>true</c> if this instance is execute failure; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(ExecuteResultType))]
        public bool IsExecuteFailure => ExecuteResultType == TaskExecuteResultType.Failure;

        /// <summary>
        /// Gets a value indicating whether this instance is execute success.
        /// </summary>
        /// <value><c>true</c> if this instance is execute success; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(ExecuteResultType))]
        public bool IsExecuteSuccess => ExecuteResultType == TaskExecuteResultType.Success;

        /// <summary>
        /// Gets a value indicating whether this instance is execute canceled.
        /// </summary>
        /// <value><c>true</c> if this instance is execute canceled; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(ExecuteResultType))]
        public bool IsExecuteCanceled => ExecuteResultType == TaskExecuteResultType.Canceled;
        
        /// <summary>
        /// Gets the execute result description.
        /// </summary>
        /// <value>The execute result description.</value>
        [DependsOnProperty(nameof(ExecuteResultType))]
        public string ExecuteResultDescription
        {
            get
            {
                switch (ExecuteResultType)
                {
                    case TaskExecuteResultType.NotExecuted:
                        return ServiceProvider.GetService<ILocalizeService>()["Not Executed."];
                    case TaskExecuteResultType.Failure:
                        return ServiceProvider.GetService<ILocalizeService>()["Execute Failure!"];
                    case TaskExecuteResultType.Success:
                        return ServiceProvider.GetService<ILocalizeService>()["Execute Success."];
                    case TaskExecuteResultType.Canceled:
                        return ServiceProvider.GetService<ILocalizeService>()["Execute canceled."];
                    default:
                        return "";
                }
            }
        }

        /// <summary>
        /// Gets or sets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public abstract string TaskDescription { get; }

        RequirementsDataModel RequirementsCore = new RequirementsDataModel();
        /// <summary>
        /// Gets or sets the requirements.
        /// </summary>
        /// <value>The requirements.</value>
        public RequirementsDataModel Requirements
        {
            get => RequirementsCore;
            set => this.RaiseAndSetIfChanged(ref RequirementsCore, value);
        }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        public virtual IBuildTaskOptions Options => null;

        bool IsOptionVisibleCore = ApplicationSettings.Default.AutoExpandTaskOptionsProperties;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is option visible.
        /// </summary>
        /// <value><c>true</c> if this instance is option visible; otherwise, <c>false</c>.</value>
        public bool IsOptionVisible
        {
            get => IsOptionVisibleCore;
            set => this.RaiseAndSetIfChanged(ref IsOptionVisibleCore, value);
        }

        /// <summary>
        /// Adds the task.
        /// </summary>
        /// <param name="task">The task.</param>
        public void AddTask(IBuildTask task)
        {
            Children.Add(new PipelineTaskNode(task));
        }

        /// <summary>
        /// Clears the execute output.
        /// </summary>
        public void ClearExecuteOutput()
        {
            // RaisePropertyChanging(nameof(ExecuteOutput));
            ExecuteOutput.Clear();
            RaisePropertyChanged(nameof(ExecuteOutput));
        }

        /// <summary>
        /// Appends the log.
        /// </summary>
        /// <param name="message">The message.</param>
        public void AppendLog(string message)
        {
            // RaisePropertyChanging(nameof(ExecuteOutput));
            ExecuteOutput.AppendLine(message);
            RaisePropertyChanged(nameof(ExecuteOutput));
        }

        /// <summary>
        /// Called when [populate command line].
        /// </summary>
        protected virtual void OnPopulateCommandLine()
        {

        }
    }

    /// <summary>
    /// Class PipelineTaskNode.
    /// Implements the <see cref="BuildPipeline.GUI.ViewModels.BasePipelineNode" />
    /// </summary>
    /// <seealso cref="BuildPipeline.GUI.ViewModels.BasePipelineNode" />
    public class PipelineTaskNode : BasePipelineNode
    {
        /// <summary>
        /// The task
        /// </summary>
        public IBuildTask Task { get; internal set; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        public override IBuildTaskOptions Options => Task?.Settings?.Options;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineTaskNode"/> class.
        /// </summary>
        internal PipelineTaskNode()
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineTaskNode"/> class.
        /// </summary>
        /// <param name="task">The task.</param>
        public PipelineTaskNode(IBuildTask task)
        {
            SetSingleTask(task);
        }

        /// <summary>
        /// Sets the single task.
        /// </summary>
        /// <param name="task">The task.</param>
        internal void SetSingleTask(IBuildTask task)
        {
            Task = task;
            Name = Task.Settings.TaskName;

            ExecuteCommandLine = Task.Pipeline.PopulateTaskExecuteCommandLine(new IBuildTask[] { Task }, this.FormatMethod);

            Requirements.Add(task.Pipeline.Context.Requirements);
            Requirements.Add(task.Requirements);

            if(Task.Settings.Options!=null)
            {
                Task.Settings.Options.PropertyChanged += (s, e) => OnPopulateCommandLine();
            }
        }

        /// <summary>
        /// Called when [populate command line].
        /// </summary>
        protected override void OnPopulateCommandLine()
        {
            ExecuteCommandLine = Task.Pipeline.PopulateTaskExecuteCommandLine(new IBuildTask[] { Task }, this.FormatMethod);
            RaisePropertyChanged(nameof(ExecuteCommandLine));
        }

        /// <summary>
        /// Gets or sets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public override string TaskDescription 
        { 
            get
            {
                return ServiceProvider.GetService<ILocalizeService>()[Task.Settings.TaskDescription];
            }
        }
    }

    /// <summary>
    /// Class PipelineStageNode.
    /// Implements the <see cref="BuildPipeline.GUI.ViewModels.PipelineTaskNode" />
    /// </summary>
    /// <seealso cref="BuildPipeline.GUI.ViewModels.PipelineTaskNode" />
    public class PipelineStageNode : PipelineTaskNode
    {
        /// <summary>
        /// The stage
        /// </summary>
        public readonly IBuildTaskStage Stage;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineStageNode"/> class.
        /// </summary>
        /// <param name="stage">The stage.</param>
        public PipelineStageNode(IBuildTaskStage stage)
        {
            Stage = stage;

            if (stage.Count > 1)
            {
                Requirements.Add(stage.Pipeline.Context.Requirements);

                StringBuilder stringBuilder = new StringBuilder();
                foreach (var t in stage)
                {
                    if (t != stage.First())
                    {
                        stringBuilder.Append(",");
                    }

                    stringBuilder.Append(t.Settings.TaskName);

                    Requirements.Add(t.Requirements);
                }

                Name = stringBuilder.ToString();

                foreach (var t in Stage)
                {
                    AddTask(t);
                }

                ExecuteCommandLine = Task.Pipeline.PopulateTaskExecuteCommandLine(Stage.ToArray(), this.FormatMethod);

                stringBuilder.Clear();
            }
            else if (stage.Count == 1)
            {
                SetSingleTask(stage.Tasks.First());
            }
        }

        /// <summary>
        /// Called when [populate command line].
        /// </summary>
        protected override void OnPopulateCommandLine()
        {
            ExecuteCommandLine = Task.Pipeline.PopulateTaskExecuteCommandLine(Stage.ToArray(), this.FormatMethod);
            RaisePropertyChanged(nameof(ExecuteCommandLine));
        }

        /// <summary>
        /// Gets or sets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public override string TaskDescription
        {
            get
            {
                if(Stage.Count == 1)
                {
                    return base.TaskDescription;
                }

                StringBuilder stringBuilder = new StringBuilder();

                foreach (var t in Stage)
                {
                    if (t != Stage.First())
                    {
                        stringBuilder.Append(',');
                    }

                    stringBuilder.Append(ServiceProvider.GetService<ILocalizeService>()[t.Settings.TaskDescription]);
                }

                return stringBuilder.ToString();
            }
        }
    }
    #endregion
}
