using Avalonia.Controls.Notifications;
using BuildPipeline.Core;
using BuildPipeline.Core.BuilderFramework;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Services;
using BuildPipeline.GUI.Services;
using BuildPipeline.GUI.Utils;
using PropertyModels.ComponentModel;
using PropertyModels.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BuildPipeline.GUI.ViewModels
{
    /// <summary>
    /// Class PipelineViewModel.
    /// </summary>
    public class PipelineViewModel : ReactiveObject
    {
        #region Properties
        /// <summary>
        /// The notification
        /// </summary>
        public WindowNotificationManager Notification { get; set; }

        /// <summary>
        /// Gets the pipeline.
        /// </summary>
        /// <value>The pipeline.</value>
        public IBuildPipeline Pipeline { get; private set; }

        /// <summary>
        /// Gets the document.
        /// </summary>
        /// <value>The document.</value>
        public IBuildPipelineDocument Document { get; internal set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name => Pipeline.Context.Name;

        /// <summary>
        /// Gets the name of the friendly.
        /// </summary>
        /// <value>The name of the friendly.</value>
        public string FriendlyName => Pipeline.Context.FriendlyName;

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description => Pipeline.Context.ToString();

        /// <summary>
        /// The stages core
        /// </summary>
        public ObservableCollection<BasePipelineNode> Nodes { get; set; } = new ObservableCollection<BasePipelineNode>();

        /// <summary>
        /// Gets or sets the selected stages.
        /// </summary>
        /// <value>The selected stages.</value>
        public ObservableCollection<BasePipelineNode> SelectedNodes { get; set; } = new ObservableCollection<BasePipelineNode>();

        BasePipelineNode SelectedNodeCore;
        /// <summary>
        /// Gets or sets the selected stage.
        /// </summary>
        /// <value>The selected stage.</value>
        public BasePipelineNode SelectedNode
        {
            get
            {
                return SelectedNodeCore;
            }
            set
            {
                if (SelectedNodeCore != null)
                {
                    SelectedNodeCore.PropertyChanged -= OnSelectedNodePropertyChanged;
                }

                this.RaiseAndSetIfChanged(ref SelectedNodeCore, value);

                if (SelectedNodeCore != null)
                {
                    SelectedNodeCore.PropertyChanged += OnSelectedNodePropertyChanged;
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="E:StagePropertyChanged" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnSelectedNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ExecuteOutput")
            {
                RaisePropertyChanged(nameof(CurrentNodeExecuteOutput));
                RaisePropertyChanged(nameof(CurrentCaret));
            }
            else if(e.PropertyName == "ExecuteCommandLine")
            {
                RaisePropertyChanged(nameof(CurrentTaskCommandLine));
            }
        }

        /// <summary>
        /// Gets the execute handler.
        /// </summary>
        /// <value>The execute handler.</value>
        public PipelineExecuteHandler ExecuteHandler { get; internal set; }

        /// <summary>
        /// Gets the current task description.
        /// </summary>
        /// <value>The current task description.</value>
        [DependsOnProperty(nameof(SelectedNode))]
        public string CurrentTaskDescription => SelectedNode?.TaskDescription ?? string.Empty;

        /// <summary>
        /// Gets or sets the format method.
        /// </summary>
        /// <value>The format method.</value>
        [DependsOnProperty(nameof(SelectedNode))]
        public CommandLineFormatMethod FormatMethod
        {
            get => SelectedNode != null ? SelectedNode.FormatMethod : CommandLineFormatMethod.Simplify;
            set 
            {
                if(SelectedNode != null)
                {
                    SelectedNode.FormatMethod = value;
                    RaisePropertyChanged(nameof(FormatMethod));
                }
            }
        }

        /// <summary>
        /// Gets the current task command line.
        /// </summary>
        /// <value>The current task command line.</value>
        [DependsOnProperty(nameof(SelectedNode), nameof(FormatMethod))]
        public string CurrentTaskCommandLine => SelectedNode?.ExecuteCommandLine ?? "";

        /// <summary>
        /// Gets the current stage execute output.
        /// </summary>
        /// <value>The current stage execute output.</value>
        [DependsOnProperty(nameof(SelectedNode))]
        public string CurrentNodeExecuteOutput => SelectedNode?.ExecuteOutput.ToString();

        /// <summary>
        /// Gets the current caret.
        /// </summary>
        /// <value>The current caret.</value>
        [DependsOnProperty(nameof(SelectedNode))]
        public int CurrentCaret => SelectedNode != null ? SelectedNode.ExecuteOutput.Length : int.MaxValue;

        /// <summary>
        /// Gets the requirements.
        /// </summary>
        /// <value>The requirements.</value>
        [DependsOnProperty(nameof(SelectedNode))]
        public RequirementsDataModel Requirements => SelectedNode?.Requirements;

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        [DependsOnProperty(nameof(SelectedNode))]
        public IBuildTaskOptions Options => SelectedNode?.Options;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is option visible.
        /// </summary>
        /// <value><c>true</c> if this instance is option visible; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(SelectedNode))]
        public bool IsOptionVisible
        {
            get => SelectedNode != null && SelectedNode.IsOptionVisible;
            set
            {
                if(SelectedNode != null)
                {
                    SelectedNode.IsOptionVisible = value;

                    RaisePropertyChanged(nameof(IsOptionVisible));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has requirements.
        /// </summary>
        /// <value><c>true</c> if this instance has requirements; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(SelectedNode))]
        public bool HasRequirements => Requirements != null && Requirements.HasRequirements;

        /// <summary>
        /// Gets a value indicating whether this instance has selected node.
        /// </summary>
        /// <value><c>true</c> if this instance has selected node; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(SelectedNode))]
        public bool HasSelectedNode => SelectedNode != null;

        /// <summary>
        /// Gets a value indicating whether this instance has command line options.
        /// </summary>
        /// <value><c>true</c> if this instance has command line options; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(SelectedNode))]
        public bool HasCommandLineOptions => Options != null;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineViewModel" /> class.
        /// </summary>
        /// <param name="notificationManager">The notification manager.</param>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="document">The document.</param>
        public PipelineViewModel(WindowNotificationManager notificationManager, IBuildPipeline pipeline, IBuildPipelineDocument document = null)
        {
            Notification = notificationManager;
            Pipeline = pipeline;
            Document = document;

            ExecuteHandler = new PipelineExecuteHandler(this);

            foreach (var stage in pipeline.Graph)
            {
                Nodes.Add(new PipelineStageNode(stage));
            }

            ExecuteHandler.PipelineExecuteFinished += OnPipelineExecuteFinished;

            ServiceProvider.GetService<ILocalizeService>().OnCultureChanged += (s, e) =>
            {
                this.RaisePropertyChanged(nameof(CurrentTaskDescription));
            };
        }

        private void OnPipelineExecuteFinished(object sender, PipelineExecuteEventArgs e)
        {
            ILocalizeService service = ServiceProvider.GetService<ILocalizeService>();

            if (e.IsCanceled)
            {
                PlayNotificationSound("canceled.mp3");

                Notification.Show(new Avalonia.Controls.Notifications.Notification(service["Warning"], service["User canceled."], NotificationType.Warning));

                return;
            }

            if (e.ExecuteResult == 0)
            {
                PlayNotificationSound("success.mp3");
                Notification.Show(new Avalonia.Controls.Notifications.Notification(service["Success"], service["All tasks execute finished."], NotificationType.Success));                
            }
            else
            {
                PlayNotificationSound("error.mp3");
                Notification.Show(new Avalonia.Controls.Notifications.Notification(service["Error"],
                    string.Format(service["Execute failed, exit code:{0}"], e.ExecuteResult), 
                    NotificationType.Error));
            }
        }

        private void PlayNotificationSound(string name)
        {
            if (ApplicationSettings.Default.EnableAudioNotification)
            {
                string path = AppFramework.FilePath($"assets/sounds/{name}");
                if(path.IsFileExists())
                {
                    ServiceProvider.GetService<IMediaPlayerService>()?.PlayAudioAsync(path);
                }
            }
        }
        #endregion

        /// <summary>
        /// Called when [toggle task options visible command].
        /// </summary>
        public void OnToggleTaskOptionsVisibleCommand()
        {
            IsOptionVisible = !IsOptionVisible;
        }
    }

    /// <summary>
    /// Class CommandLineFormatMethodProvider.
    /// </summary>
    public static class CommandLineFormatMethodProvider
    {
        /// <summary>
        /// Gets the methods.
        /// </summary>
        /// <value>The methods.</value>
        public static CommandLineFormatMethod[] Methods => Enum.GetValues<CommandLineFormatMethod>();
    }

}
