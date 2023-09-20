using BuildPipeline.Core.CommandLine;
using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Services;
using BuildPipeline.Core.Utils;
using PropertyModels.ComponentModel.DataAnnotations;
using PropertyModels.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace BuildPipeline.Core.BuilderFramework
{
    #region Interfaces
    /// <summary>
    /// Interface IBuildTask
    /// Implements the <see cref="IImportable" />
    /// </summary>
    /// <seealso cref="IImportable" />
    public interface IBuildTask : IImportable, IEnvironmentRequirementTarget, IValidatingable
    {
        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>The settings.</value>
        IBuildTaskSettings Settings { get; }

        /// <summary>
        /// Gets the stage.
        /// </summary>
        /// <value>The stage.</value>
        IBuildTaskStage Stage { get; internal set; }

        /// <summary>
        /// Gets the pipeline.
        /// </summary>
        /// <value>The pipeline.</value>
        IBuildPipeline Pipeline { get; internal set; }

        /// <summary>
        /// Gets the active condition.
        /// </summary>
        /// <value>The active condition.</value>
        string ActiveCondition { get; }

        /// <summary>
        /// Builds the help.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool BuildHelpMessage(IBuildTaskHelpTextVisitor visitor);

        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>Task&lt;System.Int32&gt;.</returns>
        Task<int> ExecuteAsync(IBuildContext context, IExcecuteObserver observer, CancellationTokenSource cancellationTokenSource);
    }

    /// <summary>
    /// Interface IBuildTaskHelpTextVisitor
    /// </summary>
    public interface IBuildTaskHelpTextVisitor
    {
        /// <summary>
        /// Gets the formatter.
        /// </summary>
        /// <value>The formatter.</value>
        IFormatter Formatter { get; }

        /// <summary>
        /// Adds the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="message">The message.</param>
        void Add(string condition, string message);
    }
    
    /// <summary>
    /// Interface ITaskExecuteReceiver
    /// Implements the <see cref="ILoggerObserver" />
    /// </summary>
    /// <seealso cref="ILoggerObserver" />
    public interface IBuildTaskExecuteObserver : IExternalProcessEventObserver
    {
        /// <summary>
        /// Called when [progress].
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="progress">The progress.[0,100]</param>
        void OnProgress(IBuildTask task, int progress);
    }

    #region Notifier
    /// <summary>
    /// Interface IExcecuteObserver
    /// </summary>
    public interface IExcecuteObserver : IExternalProcessEventObserver
    {
        /// <summary>
        /// Logs the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="message">The message.</param>
        void Log(LoggerLevel level, string message);

        /// <summary>
        /// Called when [progress].
        /// </summary>
        /// <param name="progress">The progress.</param>
        void OnProgress(int progress);
    }

    /// <summary>
    /// Class BuildTaskExecuteObserverExtensions.
    /// </summary>
    public static class BuildTaskExecuteObserverExtensions
    {
        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="observer">The observer.</param>
        /// <param name="message">The message.</param>
        public static void LogError(this IExcecuteObserver observer, string message)
        {
            observer.Log(LoggerLevel.Error, message);
        }

        /// <summary>
        /// Logs the warning.
        /// </summary>
        /// <param name="observer">The observer.</param>
        /// <param name="message">The message.</param>
        public static void LogWarning(this IExcecuteObserver observer, string message)
        {
            observer.Log(LoggerLevel.Warning, message);
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="observer">The observer.</param>
        /// <param name="message">The message.</param>
        public static void Log(this IExcecuteObserver observer, string message)
        {
            observer.Log(LoggerLevel.Information, message);
        }

        /// <summary>
        /// Logs the verbose.
        /// </summary>
        /// <param name="observer">The observer.</param>
        /// <param name="message">The message.</param>
        public static void LogVerbose(this IExcecuteObserver observer, string message)
        {
            observer.Log(LoggerLevel.Verbose, message);
        }
    }

    /// <summary>
    /// Class InternalBuildTaskExecuteObserver.
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.IExcecuteObserver" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.IExcecuteObserver" />
    internal class InternalBuildTaskExecuteObserver : IExcecuteObserver
    {
        internal readonly IBuildTask BindingTask;
        internal readonly IBuildTaskExecuteObserver Observer;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalBuildTaskExecuteObserver" /> class.
        /// </summary>
        /// <param name="bindingTask">The binding task.</param>
        /// <param name="observer">The observer.</param>
        internal InternalBuildTaskExecuteObserver(IBuildTask bindingTask, IBuildTaskExecuteObserver observer)
        {
            BindingTask = bindingTask;
            Observer = observer;
        }

        /// <summary>
        /// Logs the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="message">The message.</param>
        public virtual void Log(LoggerLevel level, string message)
        {
            Observer.OnEvent(level, BindingTask.Settings.TaskName, message);
        }

        /// <summary>
        /// Called when [canceled].
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        public virtual void OnCanceled(CancellationTokenSource cancellationTokenSource)
        {
            Observer.OnCanceled(cancellationTokenSource);
        }

        /// <summary>
        /// Called when [idle].
        /// </summary>
        public virtual void OnIdle(Process process)
        {
            Observer.OnIdle(process);
        }

        /// <summary>
        /// Called when [progress].
        /// </summary>
        /// <param name="progress">The progress.</param>
        public virtual void OnProgress(int progress)
        {
            Observer.OnProgress(BindingTask, progress);
        }

        public virtual void OnEvent(LoggerLevel level, string tag, string message)
        {
            Observer.OnEvent(level, tag, message);
        }
    }
    #endregion

    #endregion

    #region Templates
    /// <summary>
    /// Class AbstractBuildTaskExportAttribute.
    /// This is a base class
    /// 1. Used to mark a class as the entry point of a reflective BuildTask
    /// 2. Used to directly export a class as BuildTask
    /// Implements the <see cref="Attribute" />
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public abstract class AbstractBuildTaskExportAttribute : ExportAttribute
    {
        /// <summary>
        /// Accepts the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public abstract bool Accept(IBuildContext context);

        /// <summary>
        /// Gets the task condition text.
        /// </summary>
        /// <returns>System.String.</returns>
        public virtual string Condition => "";

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractBuildTaskExportAttribute"/> class.
        /// </summary>
        public AbstractBuildTaskExportAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractBuildTaskExportAttribute"/> class.
        /// </summary>
        /// <param name="contractName">Name of the contract.</param>
        public AbstractBuildTaskExportAttribute(string contractName) :
            base(contractName)
        {
        }
    }


    /// <summary>
    /// Class AbstractBuildTask.
    /// Implements the <see cref="AbstractImportable" />
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.IBuildTask" />
    /// </summary>
    /// <seealso cref="AbstractImportable" />
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.IBuildTask" />
    public abstract class AbstractBuildTask : AbstractImportable, IBuildTask
    {
        /// <summary>
        /// Gets the name of the task.
        /// </summary>
        /// <value>The name of the task.</value>
        public string TaskName => Settings?.TaskName ?? "";

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>The settings.</value>
        public IBuildTaskSettings Settings { get; protected set; }

        /// <summary>
        /// Gets the requirements.
        /// </summary>
        /// <value>The requirements.</value>
        public IEnvironmentRequirementCollection Requirements { get; private set; } = new EnvironmentRequirementCollection();

        /// <summary>
        /// Gets or sets the class attribute.
        /// </summary>
        /// <value>The class attribute.</value>
        public AbstractBuildTaskExportAttribute ClassAttribute { get; protected set; }

        IBuildTaskStage IBuildTask.Stage { get; set; }

        IBuildPipeline IBuildTask.Pipeline { get; set; }

        /// <summary>
        /// Gets the active condition.
        /// </summary>
        /// <value>The active condition.</value>
        public virtual string ActiveCondition => ClassAttribute?.Condition ?? "";

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractBuildTask"/> class.
        /// </summary>
        public AbstractBuildTask()
        {
            ClassAttribute = GetType().GetAnyCustomAttribute<AbstractBuildTaskExportAttribute>();            
        }

        /// <summary>
        /// Accepts the specified access token.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool Accept(object accessToken)
        {
            if(ClassAttribute != null && accessToken is IBuildContext bc)
            {
                return ClassAttribute.Accept(bc);
            }

            return false;
        }

        /// <summary>
        /// Builds the help.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public virtual bool BuildHelpMessage(IBuildTaskHelpTextVisitor visitor)
        {
            string text = Settings.Options?.BuildHelpMessage(visitor.Formatter)??"";
            visitor.Add(GetBuildHelpConditionText(), text);

            return true;
        }

        /// <summary>
        /// Gets the build help condition.
        /// </summary>
        /// <returns>System.String.</returns>
        protected virtual string GetBuildHelpConditionText()
        {
            return ClassAttribute?.Condition ?? "";
        }

        /// <summary>
        /// Checks the validation.
        /// </summary>
        /// <returns>ValidationResult.</returns>
        public virtual ValidationResult CheckValidation()
        {
            var Result = Requirements.CheckRequirement();
            if (Result.Result != CheckRequirementResultType.CompletelySatisfied)
            {
                return new ValidationResult($"Does not have the necessary environment: {Result.ErrorMessage}");
            }

            if(Settings != null && Settings.Options != null)
            {
                var OptionResult = Settings.Options.CheckValidation();  

                if(OptionResult.IsFailure())
                {
                    return OptionResult;
                }
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return TaskName.IsNotNullOrEmpty() ? $"{TaskName}[{GetType().Name}]" : "Unknown Task";
        }

        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>Task&lt;System.Int32&gt;.</returns>
        public abstract Task<int> ExecuteAsync(IBuildContext context, IExcecuteObserver observer, CancellationTokenSource cancellationTokenSource);
    }
    #endregion
}
