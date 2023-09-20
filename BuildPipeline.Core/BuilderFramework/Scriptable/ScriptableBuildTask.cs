using BuildPipeline.Core.Utils;

namespace BuildPipeline.Core.BuilderFramework.Scriptable
{
    /// <summary>
    /// Class ScriptableBuildTask.
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.AbstractBuildTask" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.AbstractBuildTask" />
    internal class ScriptableBuildTask : AbstractBuildTask
    {
        /// <summary>
        /// The build task settings factory name
        /// </summary>
        public const string BuildTaskSettingsFactoryName = "task_def.get_settings";


        public override string ActiveCondition => (Settings as ScriptableBuildTaskSettings).ActiveCondition;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptableBuildTask"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public ScriptableBuildTask(ScriptableBuildTaskSettings settings)
        {
            Settings = settings;
            settings.MakeDefaultOptions();

            foreach (var r in settings.Requirements)
            {
                this.Requirements.Require(r);
            }
        }

        /// <summary>
        /// Execute as an asynchronous operation.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>A Task&lt;System.Int32&gt; representing the asynchronous operation.</returns>
        public override async Task<int> ExecuteAsync(IBuildContext context, IExcecuteObserver observer, CancellationTokenSource cancellationTokenSource)
        {
            ScriptableBuildTaskSettings settings = Settings as ScriptableBuildTaskSettings;

            if(settings == null)
            {
                return -1;
            }

            Logger.Assert(settings != null);

            if(settings.ScriptService == null)
            {
                observer.LogError("Script service is not valid.");
                return -1;
            }

            string arguments = settings.Options.FormatCommandLine(CommandLineFormatMethod.Complete);

            return await settings.ScriptService.ExecuteAsync(settings.AbsoluteScriptPath, arguments, observer, cancellationTokenSource, settings.Mode);
        }
    }
}
