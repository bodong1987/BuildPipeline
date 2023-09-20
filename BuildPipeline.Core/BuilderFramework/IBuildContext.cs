using BuildPipeline.Core.CommandLine;
using BuildPipeline.Core.Serialization;
using PropertyModels.ComponentModel;
using PropertyModels.ComponentModel.DataAnnotations;
using PropertyModels.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace BuildPipeline.Core.BuilderFramework
{
    #region Interfaces    
    /// <summary>
    /// Enum TaskStartMode
    /// </summary>
    public enum TaskStartMode
    {
        /// <summary>
        /// The common
        /// Which task is specified will be executed.
        /// </summary>
        Common,

        /// <summary>
        /// The start point
        /// Start from a specified task and execute all subsequent tasks
        /// </summary>
        StartPoint
    }

    /// <summary>
    /// Enum TaskCollectMode
    /// </summary>
    public enum TaskCollectMode
    {
        /// <summary>
        /// The common
        /// </summary>
        Common,

        /// <summary>
        /// The pure collect
        /// </summary>
        PureCollect,
    }

    /// <summary>
    /// Class ConditionPropertyAttribute.
    /// Implements the <see cref="Attribute" />
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ConditionPropertyAttribute : Attribute
    {
    }

    /// <summary>
    /// Interface IBuildContext
    /// </summary>
    public interface IBuildContext :
        ICommandLineConvertible, 
        IReactiveObject,
        IValidatingable, 
        IEnvironmentRequirementTarget
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the name of the friendly.
        /// </summary>
        /// <value>The name of the friendly.</value>
        string FriendlyName { get; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>The arguments.</value>
        string[] Arguments { get; internal set; }

        /// <summary>
        /// Gets the condition.
        /// </summary>
        /// <value>The condition.</value>
        string Condition { get; }

        /// <summary>
        /// Gets the factory.
        /// </summary>
        /// <value>The factory.</value>
        IBuildContextFactory Factory { get; internal set; }

        /// <summary>
        /// Gets the task names.
        /// </summary>
        /// <value>The task names.</value>
        BindingList<string> TaskNames { get; set; }

        /// <summary>
        /// Gets the exclude task names.
        /// </summary>
        /// <value>The exclude task names.</value>
        BindingList<string> ExcludeTaskNames { get; set; }

        /// <summary>
        /// Gets the start mode.
        /// </summary>
        /// <value>The start mode.</value>
        TaskStartMode StartMode { get; }

        /// <summary>
        /// Gets the collect mode.
        /// </summary>
        /// <value>The collect mode.</value>
        TaskCollectMode CollectMode { get; internal set; }

        /// <summary>
        /// Occurs when [condition property changed].
        /// </summary>
        event EventHandler<PropertyChangedEventArgs> ConditionPropertyChanged;
    }

    /// <summary>
    /// Class IBuildContextExtensions.
    /// </summary>
    public static class IBuildContextExtensions
    {
        /// <summary>
        /// Formats the command line for task.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="taskNames">The task names.</param>
        /// <param name="method">The method.</param>
        /// <returns>System.String.</returns>
        public static string FormatCommandLineForTask(this IBuildContext context, string[] taskNames, CommandLineFormatMethod method)
        {
            var oldTaskNames = context.TaskNames;
            var oldExcludeNames = context.ExcludeTaskNames;
            try
            {
                context.TaskNames = new BindingList<string>(taskNames);
                context.ExcludeTaskNames = new BindingList<string>();

                return context.FormatCommandLine(method);
            }
            finally
            {
                context.TaskNames = oldTaskNames;
                context.ExcludeTaskNames = oldExcludeNames;
            }
        }

        /// <summary>
        /// Formats the command line arguments for task.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="taskNames">The task names.</param>
        /// <param name="method">The method.</param>
        /// <returns>System.String[].</returns>
        public static string[] FormatCommandLineArgsForTask(this IBuildContext context, string[] taskNames, CommandLineFormatMethod method)
        {
            var oldTaskNames = context.TaskNames;
            var oldExcludeNames = context.ExcludeTaskNames;
            try
            {
                context.TaskNames = new BindingList<string>(taskNames);
                context.ExcludeTaskNames = new BindingList<string>();

                return context.FormatCommandLineArgs(method);
            }
            finally
            {
                context.TaskNames = oldTaskNames;
                context.ExcludeTaskNames = oldExcludeNames;
            }
        }
    }


    #endregion

    #region Template Implements    
    /// <summary>
    /// Class AbstractBuildContext.
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.IBuildContext" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.IBuildContext" />
    [ObjectXmlSerializePolicy(XmlSerializationPolicy.IgnoreFields)]
    public abstract class AbstractBuildContext: ReactiveObject, IBuildContext
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        [XmlIgnore]
        public abstract string Name { get; }

        /// <summary>
        /// Gets the name of the friendly.
        /// </summary>
        /// <value>The name of the friendly.</value>
        public virtual string FriendlyName => Name;

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>The arguments.</value>        
        string[] IBuildContext.Arguments { get; set; }

        /// <summary>
        /// Gets the condition.
        /// </summary>
        /// <value>The condition.</value>
        [Browsable(false)]
        public virtual string Condition => "";

        IBuildContextFactory FactoryCore;
        /// <summary>
        /// Gets or sets the factory.
        /// </summary>
        /// <value>The factory.</value>
        IBuildContextFactory IBuildContext.Factory { get => FactoryCore; set => FactoryCore = value; }

        /// <summary>
        /// Gets the requirements.
        /// </summary>
        /// <value>The requirements.</value>
        [XmlIgnore]
        public IEnvironmentRequirementCollection Requirements { get; private set; } = new EnvironmentRequirementCollection();

        /// <summary>
        /// Gets the task names.
        /// </summary>
        /// <value>The task names.</value> 
        [Option('t', "tasks", HelpText = "Specify a sequence of task names to execute.", Required = false)]
        [Browsable(false)]
        public BindingList<string> TaskNames { get; set; } = new BindingList<string>();

        /// <summary>
        /// Gets the exclude task names.
        /// </summary>
        /// <value>The exclude task names.</value>
        [Option('e', "excludes", HelpText = "Specify a sequence of task names to exclude.", Required = false)]
        [Browsable(false)]
        public BindingList<string> ExcludeTaskNames { get; set; } = new BindingList<string>();

        /// <summary>
        /// Gets the start mode.
        /// </summary>
        /// <value>The start mode.</value>
        [Option('m', "mode", HelpText= "Specify the start mode.", Required = false)]        
        public TaskStartMode StartMode { get; set; } = TaskStartMode.Common;

        /// <summary>
        /// Gets the collect mode.
        /// </summary>
        /// <value>The collect mode.</value>
        [XmlIgnore]
        TaskCollectMode IBuildContext.CollectMode { get; set; } = TaskCollectMode.Common;
        
        /// <summary>
        /// Occurs when [condition property changed].
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs> ConditionPropertyChanged;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractBuildContext"/> class.
        /// </summary>
        public AbstractBuildContext()
        {
            this.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var property = GetType().GetProperty(e.PropertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

            if(property != null && property.IsDefined<ConditionPropertyAttribute>())
            {
                ConditionPropertyChanged?.Invoke(sender, e);
            }
        }

        /// <summary>
        /// Called when [condition property changed].
        /// </summary>
        /// <param name="name">The name.</param>
        public void RaiseConditionPropertyChanged(string name)
        {
            ConditionPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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

            if(!ValidatorUtils.TryValidateObject(this, out var message))
            {
                return new ValidationResult(message);
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Gets the command line remind message.
        /// </summary>
        /// <returns>System.String.</returns>
        protected virtual string GetCommandLineRemindMessage()
        {
            var message = BuildHelpMessage();

            return $"please see also in command mode:{Environment.NewLine}{message}";
        }

        /// <summary>
        /// Builds the invalid setting result.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>ValidatingResult.</returns>
        protected virtual ValidationResult BuildInvalidSettingResult(string message)
        {
            if(BuildFramework.IsCommandLineMode)
            {
                return new ValidationResult($"{message}{Environment.NewLine}{GetCommandLineRemindMessage()}");
            }
            else
            {
                return new ValidationResult(message);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Name;
        }

        #region Command line Convertable
        /// <summary>
        /// Formats the command line.
        /// </summary>
        /// <returns>System.String.</returns>
        public virtual string FormatCommandLine(CommandLineFormatMethod method)
        {
            return Parser.FormatCommandLine(this, method);
        }

        /// <summary>
        /// Formats the command line arguments.
        /// </summary>
        /// <returns>System.String[].</returns>
        public string[] FormatCommandLineArgs(CommandLineFormatMethod method)
        {
            var text = this.FormatCommandLine(method);

            if (text.IsNullOrEmpty())
            {
                return new string[] { };
            }

            return Splitter.SplitCommandLineIntoArguments(text).ToArray();
        }

        /// <summary>
        /// Overrides from command line.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="message">The message.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool OverrideFromCommandLine(string arguments, out string message)
        {
            return OverrideFromCommandLineArgs(Splitter.SplitCommandLineIntoArguments(arguments), out message);
        }

        /// <summary>
        /// Overrides from command line arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="message">The message.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public virtual bool OverrideFromCommandLineArgs(IEnumerable<string> args, out string message)
        {
            var result = Parser.Default.Parse(args, this);

            message = result.ErrorMessage;
            if (result.Result == ParserResultType.Parsed)
            {
                return ValidatorUtils.TryValidateObject(result.Value, out message);
            }

            return false;
        }

        /// <summary>
        /// Builds the help message.
        /// </summary>
        /// <param name="formatter">The formatter.</param>
        /// <returns>System.String.</returns>
        public virtual string BuildHelpMessage(IFormatter formatter = null)
        {
            return Parser.GetHelpText(this, formatter);
        }
        #endregion
    }
    #endregion
}
