using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform;
using Avalonia.PropertyGrid.Services;
using Avalonia.PropertyGrid.Utils;
using BuildPipeline.Core;
using BuildPipeline.Core.BuilderFramework;
using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Services;
using BuildPipeline.Core.Utils;
using BuildPipeline.GUI.Framework;
using BuildPipeline.GUI.Services;
using BuildPipeline.GUI.Utils;
using BuildPipeline.GUI.Views;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyModels.ComponentModel;
using PropertyModels.ComponentModel.DataAnnotations;
using PropertyModels.Extensions;
using PropertyModels.Localilzation;
using System.ComponentModel;

namespace BuildPipeline.GUI.ViewModels
{
    #region Pipelines View Model
    /// <summary>
    /// Class MainWindowDataModel.
    /// </summary>
    public class MainWindowDataModel : ReactiveObject
    {
        #region Base Properties
        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        /// <value>The name of the application.</value>
        public string AppName
        {
            get
            {
                var ls = ServiceProvider.GetService<ILocalizeService>();
                return $"{ls["AppName"]} v{EditorFramework.AppVersion}"; 
            }
        }

        /// <summary>
        /// Gets the pipelines.
        /// </summary>
        /// <value>The pipelines.</value>
        public PipelineViewModel[] Pipelines => PipelinesCore.ToArray();

        /// <summary>
        /// The pipelines core
        /// </summary>
        readonly List<PipelineViewModel> PipelinesCore = new List<PipelineViewModel>();

        PipelineViewModel SelectedPipelineCore;
        /// <summary>
        /// Gets or sets the selected pipeline.
        /// </summary>
        /// <value>The selected pipeline.</value>
        public PipelineViewModel SelectedPipeline
        {
            get => SelectedPipelineCore;
            set=> this.RaiseAndSetIfChanged(ref SelectedPipelineCore, value);
        }

        /// <summary>
        /// The configuration
        /// </summary>
        public ApplicationConfiguration Configuration = ApplicationConfiguration.OverrideFromFile();

        /// <summary>
        /// Gets the recent files.
        /// </summary>
        /// <value>The recent files.</value>
        public string[] RecentFiles => Configuration.RecentFiles.ToArray();

        private string[] ThemesPrivate = new string[] { "Fluent", "Simple" };
        /// <summary>
        /// Gets the themes.
        /// </summary>
        /// <value>The themes.</value>
        public string[] Themes => ThemesPrivate;

        private string[] StylesPrivate = new string[] { "Default", "Dark", "Light" };
        /// <summary>
        /// Gets the styles.
        /// </summary>
        /// <value>The styles.</value>
        public string[] Styles => StylesPrivate;

        /// <summary>
        /// Gets a value indicating whether [recent files visible].
        /// </summary>
        /// <value><c>true</c> if [recent files visible]; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(RecentFiles))]
        public bool RecentFilesVisible => Configuration.RecentFiles.Count > 0;

        /// <summary>
        /// Gets a value indicating whether this instance has selected pipeline.
        /// </summary>
        /// <value><c>true</c> if this instance has selected pipeline; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(SelectedPipeline))]
        public bool HasSelectedPipeline => SelectedPipeline != null;
                
        /// <summary>
        /// Gets all languages.
        /// </summary>
        /// <value>All languages.</value>
        public CultureDataModel[] AllLanguages => ApplicationSettings.Default.AvailableCultures.Select(x=>new CultureDataModel(x)).ToArray();
        #endregion

        #region Open Apis
        /// <summary>
        /// Adds the specified pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="document">The document.</param>
        /// <returns>PipelineViewModel.</returns>
        public PipelineViewModel Add(IBuildPipeline pipeline, IBuildPipelineDocument document)
        {
            // RaisePropertyChanging(nameof(Pipelines));

            var doc = BuildFramework.NewBuildPipelineDocument(pipeline, false);
            doc.RedirectToFile(document.Path);

            var view = new PipelineViewModel(Notification, pipeline, doc);
            PipelinesCore.Add(view);

            RaisePropertyChanged(nameof(Pipelines));

            SelectedPipeline = view;

            return view;
        }

        /// <summary>
        /// Adds the recent file.
        /// </summary>
        /// <param name="file">The file.</param>
        public void AddRecentFile(string file)
        {
            Configuration.AddRecentFile(file);

            RaisePropertyChanged(nameof(RecentFiles));
        }

        /// <summary>
        /// Removes the specified pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        public void Remove(IBuildPipeline pipeline)
        {
            PipelinesCore.RemoveAll(x => x.Pipeline == pipeline);

            RaisePropertyChanged(nameof(Pipelines));
        }
        #endregion

        #region Commands
        /// <summary>
        /// Gets or sets the parent window.
        /// </summary>
        /// <value>The parent window.</value>
        public readonly Window ParentWindow;

        /// <summary>
        /// The notification
        /// </summary>
        public readonly WindowNotificationManager Notification;

        /// <summary>
        /// Gets the modern style.
        /// </summary>
        /// <value>The modern style.</value>
        public ModernStyleDataModel ModernStyle { get; private set; } = new ModernStyleDataModel();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowDataModel"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public MainWindowDataModel(Window parent)
        {
            ParentWindow = parent;

            Notification = new WindowNotificationManager(ParentWindow)
            {
                Position = NotificationPosition.TopRight,
                MaxItems = 3
            };

            // active media player service first...
            ServiceProvider.GetService<IMediaPlayerService>();
        }

        /// <summary>
        /// Called when [new pipeline command].
        /// </summary>
        protected async void OnNewPipelineCommand()
        {
            var window = new NewBuildContextWindow() { DataContext = NewBuildContextViewModel.NewViewModel() };

            var result = await window.ShowDialog<NewBuildPipelineResult>(ParentWindow);

            if (result == null)
            {
                return;
            }

            Add(result.Pipeline, result.Document);
        }

        /// <summary>
        /// Called when [open pipe line context command].
        /// </summary>
        protected async void OnOpenPipeLineContextCommand()
        {
            var paths = await PathBrowserUtils.ShowOpenFileDialogAsync(ParentWindow, "Build Pipeline Document File(*.bpf)|*.bpf", false, "Open a file to load");

            if (paths == null || paths.Length == 0)
            {
                return;
            }

            var path = paths.FirstOrDefault();

            if (!path.IsFileExists())
            {
                await MessageBoxUtils.ShowErrorAsync(ParentWindow, "Error", $"Target file is not exists:{Environment.NewLine}{path}");
                return;
            }

            OpenDocument(path);
        }

        /// <summary>
        /// Opens the document.
        /// </summary>
        /// <param name="path">The path.</param>
        public async void OpenDocument(string path)
        {
            var doc = BuildFramework.LoadBuildPipelineDocument(path);

            if (doc == null)
            {
                await MessageBoxUtils.ShowErrorAsync(ParentWindow, "Error", "Failed load file, please check log for more information.");
                return;
            }

            OpenDocument(doc);

            AddRecentFile(path);
        }

        /// <summary>
        /// Opens the document.
        /// </summary>
        /// <param name="doc">The document.</param>
        public async void OpenDocument(IBuildPipelineDocument doc)
        {
            var window = new NewBuildContextWindow() { DataContext = NewBuildContextViewModel.NewViewModel(doc, true) };

            var result = await window.ShowDialog<NewBuildPipelineResult>(ParentWindow);

            if (result == null)
            {
                return;
            }

            Add(result.Pipeline, result.Document);
        }

        protected async void OnDuplicatePipelineCommand()
        {
            if (SelectedPipeline == null)
            {
                await MessageBoxUtils.ShowErrorAsync(ParentWindow, "Error", "Please select a pipeline first.");
                return;
            }

            if (SelectedPipeline.Document == null)
            {
                await MessageBoxUtils.ShowErrorAsync(ParentWindow, "Error", "Document is not exists.");
                return;
            }

            var doc = SelectedPipeline.Document.Clone();

            OpenDocument(doc);
        }

        protected async void OnSavePipeLineContextCommand()
        {
            if (SelectedPipeline == null)
            {
                await MessageBoxUtils.ShowErrorAsync(ParentWindow, "Error", "Please select a pipeline first.");
                return;
            }

            if (SelectedPipeline.Document == null)
            {
                await MessageBoxUtils.ShowErrorAsync(ParentWindow, "Error", "Document is not exists.");
                return;
            }

            if (SelectedPipeline.Document.Path.IsNotNullOrEmpty())
            {
                if (!SelectedPipeline.Document.Save(SelectedPipeline.Document.Path))
                {
                    await MessageBoxUtils.ShowErrorAsync(ParentWindow, "Error", "Failed save document, Please check log for more information.");
                    return;
                }

                AddRecentFile(SelectedPipeline.Document.Path);

                Notification.Show(new Avalonia.Controls.Notifications.Notification("Information", $"Save To {SelectedPipeline.Document.Path} success.", NotificationType.Information));
            }
            else
            {
                OnSavePipeLineContextAsCommand();
            }
        }

        protected async void OnSavePipeLineContextAsCommand()
        {
            if (SelectedPipeline == null)
            {
                await MessageBoxUtils.ShowErrorAsync(ParentWindow, "Error", "Please select a pipeline first.");
                return;
            }

            if (SelectedPipeline.Document == null)
            {
                await MessageBoxUtils.ShowErrorAsync(ParentWindow, "Error", "Document is not exists.");
                return;
            }

            var path = await PathBrowserUtils.ShowSaveFileDialogAsync(ParentWindow, "Build Pipeline Document File(*.bpf)|*.bpf", "Select a file to save", SelectedPipeline.Name + ".bpf");

            if (path.IsNullOrEmpty())
            {
                return;
            }

            if (!SelectedPipeline.Document.Save(path))
            {
                await MessageBoxUtils.ShowErrorAsync(ParentWindow, "Error", "Failed save document, Please check log for more information.");
                return;
            }

            AddRecentFile(path);

            Notification.Show(new Avalonia.Controls.Notifications.Notification("Information", $"Save To {path} success.", NotificationType.Information));
        }

        protected async void OnCloseSelectedPipelineCommand()
        {
            if (SelectedPipeline == null)
            {
                await MessageBoxUtils.ShowErrorAsync(ParentWindow, "Error", "No selected pipeline");
                return;
            }

            if (SelectedPipeline.ExecuteHandler.IsExecuting)
            {
                var result = await MessageBoxUtils.ShowMessageBoxAsync(
                ParentWindow,
                "Question",
                "close pipe line will force stop execute, do you want to continue?",
                MsBox.Avalonia.Enums.ButtonEnum.OkCancel,
                MsBox.Avalonia.Enums.Icon.Warning
                );

                if (result != MsBox.Avalonia.Enums.ButtonResult.Ok)
                {
                    return;
                }

                SelectedPipeline.ExecuteHandler.StopAllTasksAndWait(true);
            }

            Remove(SelectedPipeline.Pipeline);
        }

        #endregion
    }
    #endregion

    #region Temp Settings
    /// <summary>
    /// Class ApplicationConfiguration.
    /// </summary>
    [PathConfiguration("App.config", true)]
    public class ApplicationConfiguration : Configuration<ApplicationConfiguration>
    {
        /// <summary>
        /// The recent files
        /// </summary>
        public List<string> RecentFiles = new List<string>();

        /// <summary>
        /// Adds the recent file.
        /// </summary>
        /// <param name="file">The file.</param>
        public void AddRecentFile(string file)
        {
            RecentFiles.Remove(file);
            RecentFiles.Insert(0, file);

            Save();
        }

        /// <summary>
        /// Posts the serialized.
        /// </summary>
        public override void PostSerialized()
        {
            base.PostSerialized();

            RecentFiles.RemoveAll(x => !x.IsFileExists());
        }
    }
    #endregion

    #region ModernStyleDataModel
    /// <summary>
    /// Class ModernStyleDataModel.
    /// Implements the <see cref="ReactiveObject" />
    /// </summary>
    /// <seealso cref="ReactiveObject" />
    public class ModernStyleDataModel : ReactiveObject
    {
        bool ExtendClientAreaToDecorationsHintCore = false;
        /// <summary>
        /// Gets or sets a value indicating whether [extend client area to decorations hint].
        /// </summary>
        /// <value><c>true</c> if [extend client area to decorations hint]; otherwise, <c>false</c>.</value>
        public bool ExtendClientAreaToDecorationsHint
        {
            get => ExtendClientAreaToDecorationsHintCore;
            set => this.RaiseAndSetIfChanged(ref ExtendClientAreaToDecorationsHintCore, value);
        }

        int ExtendClientAreaTitleBarHeightHintCore = 0;
        /// <summary>
        /// Gets or sets the extend client area title bar height hint.
        /// </summary>
        /// <value>The extend client area title bar height hint.</value>
        public int ExtendClientAreaTitleBarHeightHint
        {
            get => ExtendClientAreaTitleBarHeightHintCore;
            set => this.RaiseAndSetIfChanged(ref ExtendClientAreaTitleBarHeightHintCore, value);
        }

        /// <summary>
        /// The extend client area chrome hints core
        /// </summary>
        ExtendClientAreaChromeHints ExtendClientAreaChromeHintsCore = ExtendClientAreaChromeHints.Default;

        /// <summary>
        /// Gets or sets the extend client area chrome hints.
        /// </summary>
        /// <value>The extend client area chrome hints.</value>
        public ExtendClientAreaChromeHints ExtendClientAreaChromeHints
        {
            get => ExtendClientAreaChromeHintsCore;
            set => this.RaiseAndSetIfChanged(ref ExtendClientAreaChromeHintsCore, value);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is windows style.
        /// </summary>
        /// <value><c>true</c> if this instance is windows style; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(Style))]
        public bool IsWindowsStyle => Style == ModernStyleType.WindowsMetro || (Style == ModernStyleType.Default && OperatingSystem.IsWindows());
        /// <summary>
        /// Gets a value indicating whether this instance is mac os style.
        /// </summary>
        /// <value><c>true</c> if this instance is mac os style; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(Style))]
        public bool IsMacOSStyle => Style == ModernStyleType.MacOS || (Style == ModernStyleType.Default && OperatingSystem.IsMacOS());

        /// <summary>
        /// Gets a value indicating whether this instance is classic style.
        /// </summary>
        /// <value><c>true</c> if this instance is classic style; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(Style))]
        public bool IsClassicStyle => Style == ModernStyleType.Classic || (Style == ModernStyleType.Default && OperatingSystem.IsLinux());

        /// <summary>
        /// Gets a value indicating whether this instance is modern style.
        /// </summary>
        /// <value><c>true</c> if this instance is modern style; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(Style))]
        public bool IsModernStyle => IsWindowsStyle || IsMacOSStyle;

        ModernStyleType StyleCore = ModernStyleType.Default;

        /// <summary>
        /// Gets or sets the style.
        /// </summary>
        /// <value>The style.</value>
        public ModernStyleType Style
        {
            get => StyleCore;
            set => this.RaiseAndSetIfChanged(ref StyleCore, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernStyleDataModel"/> class.
        /// </summary>
        public ModernStyleDataModel()
        {
            StyleCore = ApplicationSettings.Default.ModernStyle;

            this.PropertyChanged += OnPropertyChanged;

            // force reset values
            RaisePropertyChanged(nameof(Style));

            ApplicationSettings.Default.PropertyChanged += OnApplicationSettingChanged;
        }

        private void OnApplicationSettingChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(ApplicationSettings.Default.ModernStyle))
            {
                this.Style = ApplicationSettings.Default.ModernStyle;
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(Style))
            {
                if (IsMacOSStyle || IsWindowsStyle)
                {
                    ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
                    ExtendClientAreaTitleBarHeightHint = -1;
                    ExtendClientAreaToDecorationsHint = true;
                }
                else
                {
                    ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
                    ExtendClientAreaTitleBarHeightHint = 0;
                    ExtendClientAreaToDecorationsHint = false;
                }
            }
        }
    }
    #endregion

    #region Culture Data Model
    /// <summary>
    /// Class CultureDataModel.
    /// </summary>
    public class CultureDataModel
    {
        /// <summary>
        /// The data
        /// </summary>
        public readonly CultureInfoData Data;

        /// <summary>
        /// Initializes a new instance of the <see cref="CultureDataModel"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public CultureDataModel(CultureInfoData data)
        {
            Data = data;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Data.ToString();
        }

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        public bool IsActive => ApplicationSettings.Default.AvailableCultures.SelectedValue == Data;
    }
    #endregion
}
