using BuildPipeline.Core.BuilderFramework;
using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Utils;
using PropertyModels.ComponentModel;
using PropertyModels.ComponentModel.DataAnnotations;
using PropertyModels.Extensions;
using System.ComponentModel;

namespace BuildPipeline.GUI.ViewModels
{
    /// <summary>
    /// Class NewBuildContextViewModel.
    /// </summary>
    public class NewBuildContextViewModel : ReactiveObject
    {
        #region Proeprties
        /// <summary>
        /// Gets all factories names.
        /// </summary>
        /// <value>All factories names.</value>
        public string[] AllFactoriesNames => BuildFramework.AllFactories.Select(x=>x.Name).ToArray();

        /// <summary>
        /// Gets all factories.
        /// </summary>
        /// <value>All factories.</value>
        public IBuildContextFactory[] AllFactories => BuildFramework.AllFactories;

        /// <summary>
        /// Gets the factory.
        /// </summary>
        /// <value>The factory.</value>
        public IBuildContextFactory Factory { get; set; }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        public IBuildContext Context { get; set; }

        /// <summary>
        /// Gets or sets the pipeline.
        /// </summary>
        /// <value>The pipeline.</value>
        public IBuildPipeline Pipeline { get; set; }

        IBuildPipelineDocument DocumentCore;
        /// <summary>
        /// Gets or sets the document.
        /// </summary>
        /// <value>The document.</value>
        public IBuildPipelineDocument Document
        {
            get => DocumentCore;
            set
            {
                if(DocumentCore != value)
                {
                    SettingsList.Clear();

                    if(value != null)
                    {
                        DocumentCore = value;

                        foreach (var setting in value.Settings)
                        {
                            SettingsList.Add(new TaskSettingDataModel(this, setting));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can export.
        /// </summary>
        /// <value><c>true</c> if this instance can export; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(Factory), nameof(Context), nameof(Pipeline), nameof(Document))]
        public bool CanExport => Factory != null && Context != null && Pipeline != null && Document != null;

        /// <summary>
        /// The settings data model
        /// </summary>
        List<TaskSettingDataModel> SettingsList = new List<TaskSettingDataModel>();

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>The settings.</value>
        [DependsOnProperty(nameof(Document))]
        public TaskSettingDataModel[] Settings => SettingsList.ToArray();

        /// <summary>
        /// Gets a value indicating whether this instance has task options.
        /// </summary>
        /// <value><c>true</c> if this instance has task options; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(Document))]
        public bool HasTaskOptions => SettingsList.Count > 0;

        string ErrorMessageCore;
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>The error message.</value>        
        public string ErrorMessage
        {
            get => ErrorMessageCore;
            set => this.RaiseAndSetIfChanged(ref ErrorMessageCore, value);
        }

        /// <summary>
        /// Gets a value indicating whether this instance has error message.
        /// </summary>
        /// <value><c>true</c> if this instance has error message; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(ErrorMessage))]
        public bool HasErrorMessage => ErrorMessage.IsNotNullOrEmpty();

        /// <summary>
        /// Gets or sets the name of the factory.
        /// </summary>
        /// <value>The name of the factory.</value>
        public string FactoryName
        {
            get
            {
                return Factory?.Name??"";
            }
            set
            {
                Factory = BuildFramework.FindFactory(value);

                RaisePropertyChanged(nameof(FactoryName));

                if(Context != null)
                {
                    Context.ConditionPropertyChanged -= OnContextConditionPropertyChanged;
                }

                Context = Factory?.NewContext();

                ValidateDocument();

                RaisePropertyChanged(nameof(Context));

                if(Context != null)
                {
                    Context.ConditionPropertyChanged += OnContextConditionPropertyChanged;
                }
            }
        }
        #endregion

        #region Factory
        /// <summary>
        /// Creates new viewmodel.
        /// </summary>
        /// <returns>NewBuildContextViewModel.</returns>
        public static NewBuildContextViewModel NewViewModel()
        {
            return new NewBuildContextViewModel();
        }

        /// <summary>
        /// Creates new viewmodel.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="forceAllTasks">if set to <c>true</c> [force all tasks].</param>
        /// <returns>NewBuildContextViewModel.</returns>
        public static NewBuildContextViewModel NewViewModel(IBuildPipelineDocument document, bool forceAllTasks)
        {
            NewBuildContextViewModel dataModel = new NewBuildContextViewModel();

            dataModel.Factory = BuildFramework.FindFactory(document.FactoryName);

            if(dataModel.Factory == null)
            {
                Logger.LogError("Failed create pipeline because factory is not exists: {0}", document.FactoryName);

                return null;
            }

            dataModel.Pipeline = BuildFramework.NewBuildPipeline(document, forceAllTasks);

            if(dataModel.Pipeline == null)
            {
                Logger.LogError("Failed create pipeline from document.");
                return null;
            }

            dataModel.Context = dataModel.Pipeline.Context;
            dataModel.Document = BuildFramework.NewBuildPipelineDocument(dataModel.Pipeline, forceAllTasks);

            return dataModel;
        }
        #endregion

        #region Event Handler
        private void OnContextConditionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshPipeline();
        }

        public void ValidateDocument()
        {
            if (Document != null)
            {
                return;
            }

            RefreshPipeline();
        }

        private void RefreshPipeline()
        {
            if (Context == null)
            {
                return;
            }

            var result = Context.CheckValidation();

            if (result.IsFailure())
            {
                Pipeline = null;
                Document = null;

                RaisePropertyChanged(nameof(Pipeline));
                RaisePropertyChanged(nameof(Document));

                ErrorMessage = result.GetDisplayMessage();

                return;
            }

            Pipeline = BuildFramework.NewBuildPipeline(Context);

            RaisePropertyChanged(nameof(Pipeline));

            if (Pipeline == null)
            {
                Document = null;

                RaisePropertyChanged(nameof(Document));

                ErrorMessage = string.Empty;

                return;
            }

            IBuildPipelineDocument document = BuildFramework.NewBuildPipelineDocument(Pipeline, false);

            Document = document;
            RaisePropertyChanged(nameof(Document));

            ErrorMessage = string.Empty;
        }
        #endregion
    }

    /// <summary>
    /// Class NewBuildPipelineResult.
    /// </summary>
    public class NewBuildPipelineResult
    {
        /// <summary>
        /// The pipeline
        /// </summary>
        public readonly IBuildPipeline Pipeline;
        /// <summary>
        /// The document
        /// </summary>
        public readonly IBuildPipelineDocument Document;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewBuildPipelineResult"/> class.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="document">The document.</param>
        public NewBuildPipelineResult(IBuildPipeline pipeline, IBuildPipelineDocument document)
        {
            Pipeline = pipeline;
            Document = document;
        }
    }
    
}
