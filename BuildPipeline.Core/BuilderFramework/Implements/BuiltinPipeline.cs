using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Utils;
using PropertyModels.ComponentModel.DataAnnotations;
using PropertyModels.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BuildPipeline.Core.BuilderFramework.Implements
{
    /// <summary>
    /// Class BuiltinPipeline.
    /// </summary>
    internal class BuiltinPipeline : IBuildPipeline
    {
        #region Properties
        /// <summary>
        /// The context
        /// </summary>
        public IBuildContext Context { get; private set; }

        /// <summary>
        /// The graph
        /// </summary>
        public IBuildTaskGraph Graph { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltinPipeline" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="graph">The graph.</param>
        private BuiltinPipeline(IBuildContext context, IBuildTaskGraph graph)
        {
            Context = context;
            Graph = graph;

            foreach(var stage in graph)
            {
                stage.Pipeline = this;

                foreach(var t in stage)
                {
                    t.Pipeline = this;
                    t.Stage = stage;
                }
            }
        }
        #endregion

        #region Factory
        /// <summary>
        /// Creates the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>BuildPipeline.</returns>
        /// <exception cref="ArgumentNullException">context</exception>
        public static BuiltinPipeline Create(IBuildContext context)
        {
            if (context == null)
            { 
                throw new ArgumentNullException(nameof(context));
            }

            var result = context.CheckValidation();

            if (result.IsFailure())
            {
                Logger.LogError("Can't create BuildPipeline from invalid Context.\n{0}", result.ErrorMessage);
                return null;
            }

            BuildTaskCollector collector = new BuildTaskCollector(); 
            collector.Collect(context);

            BuiltinPipeline buildPipeline = new BuiltinPipeline(context, collector.Graph);

            return buildPipeline;
        }

        /// <summary>
        /// Creates the specified document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="forceAllTasks">if set to <c>true</c> [force all tasks].</param>
        /// <returns>BuildPipeline.</returns>
        /// <exception cref="System.ArgumentNullException">document</exception>
        /// <exception cref="System.ArgumentNullException">document.Context</exception>
        public static BuiltinPipeline Create(IBuildPipelineDocument document, bool forceAllTasks)
        {
            if(document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if(document.Context == null)
            {
                throw new ArgumentNullException("document.Context");
            }

            var result = document.Context.CheckValidation();
            if (result.IsFailure())
            {
                Logger.LogError("Can't create BuildPipeline from invalid Context.\n{0}", result.ErrorMessage);
                return null;
            }

            BuildTaskCollector collector = new BuildTaskCollector();
            collector.Collect(document.Context);

            foreach(var task in forceAllTasks?collector.Graph.GetAllTasks() : collector.Graph.GetIncludeTasks())
            {
                if(task.Settings.Options != null)
                {
                    var settings = document.Settings.ToList().Find(x => x.TaskName == task.Settings.TaskName);

                    if (settings != null && settings.Options != null)
                    {
                        string arguments = settings.Options.FormatCommandLine(CommandLineFormatMethod.Complete);
                        string message = "";

                        if(!task.Settings.Options.OverrideFromCommandLine(arguments, out message))
                        {
                            // 
                            Logger.LogWarning("Failed reload task [{0}] options from command:{1}", task.Settings.TaskName, arguments);
                        }
                    }
                }
            }

            BuiltinPipeline buildPipeline = new BuiltinPipeline(document.Context, collector.Graph);

            return buildPipeline;
        }
        #endregion

        #region Validate
        /// <summary>
        /// Checks the validation.
        /// </summary>
        /// <returns>ValidationResult.</returns>
        public ValidationResult CheckValidation()
        {
            if(Context == null)
            {
                return new ValidationResult("Missing Context");
            }

            var result = Context.CheckValidation();

            if(result.IsFailure())
            {
                return result;
            }

            foreach(var i in Graph.GetIncludeTasks())
            {
                result = i.CheckValidation();

                if(result.IsFailure())
                {
                    return new ValidationResult($"Task <{i.Settings.TaskName}> can't be executed:{result.ErrorMessage}");
                }    
            }

            return ValidationResult.Success;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Builds the help message.
        /// </summary>
        /// <returns>System.String.</returns>
        public string BuildHelpMessage()
        {
            HelpTextGenerator generator = HelpTextGenerator.Create(Context.Name);

            return generator?.ToString()??"";
        }

        /// <summary>
        /// Populates the task execute command line.
        /// </summary>
        /// <param name="tasks">The tasks.</param>
        /// <param name="method">The method.</param>
        /// <returns>System.String.</returns>
        public string PopulateTaskExecuteCommandLine(IBuildTask[] tasks, CommandLineFormatMethod method)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"{Context.Factory.PluginName} ");
            stringBuilder.Append($"{Context.Name} ");

            // format context arguments
            string contextArgs = Context.FormatCommandLineForTask(tasks.Select(x=>x.Settings.TaskName).ToArray(), method);
            stringBuilder.Append($" {contextArgs} ");

            // format option arguments
            foreach(var task in tasks)
            {
                string optionsArgs = task.Settings.FormatOptionsCommandLine(method) ?? "";

                if (optionsArgs.IsNotNullOrEmpty())
                {
                    stringBuilder.Append($" {optionsArgs} ");
                }
            }            

            var arguments = stringBuilder.ToString();
            return arguments;
        }
        #endregion
    }
}
