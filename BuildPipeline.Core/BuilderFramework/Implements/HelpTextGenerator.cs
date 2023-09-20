using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Utils;
using BuildPipeline.Core.CommandLine;
using System.Text;
using System.ComponentModel;
using BuildPipeline.Core.Services;
using BuildPipeline.Core.Framework;
using PropertyModels.Extensions;

namespace BuildPipeline.Core.BuilderFramework.Implements
{
    /// <summary>
    /// Class HelpTextGenerator.
    /// </summary>
    internal class HelpTextGenerator
    {
        /// <summary>
        /// The builder
        /// </summary>
        readonly StringBuilder Builder = new StringBuilder();

        /// <summary>
        /// Prevents a default instance of the <see cref="HelpTextGenerator"/> class from being created.
        /// </summary>
        HelpTextGenerator() { }

        #region Factories
        /// <summary>
        /// Creates the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>HelpTextGenerator.</returns>
        public static HelpTextGenerator Create(string[] args)
        {
            HelpTextGenerator generator = new HelpTextGenerator();
            generator.GenHelpText(args);

            return generator;
        }

        /// <summary>
        /// Creates the specified factory name.
        /// </summary>
        /// <param name="factoryName">Name of the factory.</param>
        /// <returns>HelpTextGenerator.</returns>
        public static HelpTextGenerator Create(string factoryName)
        {
            var factory = BuildFramework.FindFactory(factoryName);

            if (factory == null)
            {
                Logger.LogError("Failed find factory:{0}", factoryName);
                return null;
            }

            HelpTextGenerator generator = new HelpTextGenerator();
            generator.GenHelpText(factory);
            return generator;
        }

        /// <summary>
        /// Creates the specified factory.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <returns>HelpTextGenerator.</returns>
        public static HelpTextGenerator Create(IBuildContextFactory factory)
        {
            if(factory == null)
            {
                return null;
            }

            HelpTextGenerator generator = new HelpTextGenerator();
            generator.GenHelpText(factory);
            return generator;
        }
        #endregion

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Builder.ToString();
        }

        /// <summary>
        /// Gens the help text.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private void GenHelpText(string[] args)
        {
            var options = Parser.Default.Parse<HelpTextGeneratorOptions>(args)?.Value;

            foreach(var name in options != null && options.PipelineNames!=null && options.PipelineNames.Count()>0?options.PipelineNames:BuildFramework.AllFactories.Select(x=>x.Name))
            {
                var factory = BuildFramework.FindFactory(name);

                if(factory != null)
                {
                    Builder.AppendLine($"HelpText of Pipeline <<<{name}>>>:");
                    GenHelpText(factory);                    
                }
                else
                {
                    Builder.AppendLine($"No Pipeline <<<{name}>>> exists.");
                }

                Builder.AppendLine();
                Builder.AppendLine();
            }
        }

        /// <summary>
        /// Gens the help text.
        /// </summary>
        /// <param name="factory">The factory.</param>
        private void GenHelpText(IBuildContextFactory factory)
        {
            var context = factory.NewContext();

            if(context == null)
            {
                return;
            }

            context.CollectMode = TaskCollectMode.PureCollect;
            string text = Parser.GetHelpText(context);

            Builder.AppendLine(text);

            BuildTaskCollector collector = new BuildTaskCollector();
            collector.Collect(context);

            // group by types
            SortedList<string, List<IBuildTask>> Caches = new SortedList<string, List<IBuildTask>>();

            foreach(var task in collector.AllTasks)
            {                
                List<IBuildTask> TaskLists;
                string key = task.ActiveCondition ?? "";

                if(!Caches.TryGetValue(key, out TaskLists))
                {
                    TaskLists = new List<IBuildTask>();
                    Caches.Add(key, TaskLists);
                }

                TaskLists.Add(task);
            }

            GenRequirements(context.Requirements, 4);

            foreach (var v in Caches.Values)
            {
                GenHelpText(Builder, v, v.FirstOrDefault()?.ActiveCondition??"");
            }
        }

        /// <summary>
        /// Gens the help text.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="tasks">The tasks.</param>
        /// <param name="condition">The condition.</param>
        private void GenHelpText(StringBuilder builder, List<IBuildTask> tasks, string condition) 
        {
            builder.AppendLine($"{Environment.NewLine}    {condition}");

            foreach (var task in tasks.OrderBy(x => x.ImportPriority))
            {
                HelpTextGeneratorTextVisitor visitor = new HelpTextGeneratorTextVisitor(
                    builder, 
                    task, 
                    new Formatter(Parser.DefaultIndent, Parser.DefaultBlank-8), 
                    8,
                    0);
                
                task.BuildHelpMessage(visitor);

                GenRequirements(task.Requirements, 12);
            }
        }

        /// <summary>
        /// Gens the requirements.
        /// </summary>
        /// <param name="requirements">The requirements.</param>
        /// <param name="indent">The indent.</param>
        private void GenRequirements(IEnvironmentRequirementCollection requirements, int indent)
        {
            if(requirements.Count() <=0)
            {
                return;
            }

            var titleIndent = CommandLine.Formatter.BuildBlankText(indent);
            var requireIndent = CommandLine.Formatter.BuildBlankText(indent + 4);

            Builder.AppendLine($"{titleIndent}Requirements:");
            foreach (var requirement in requirements)
            {
                Builder.AppendLine($"{requireIndent}{requirement.RequirementDescription}");
            }
        }
    }

    /// <summary>
    /// Class HelpTextGeneratorOptions.
    /// </summary>
    class HelpTextGeneratorOptions
    {
        /// <summary>
        /// Gets or sets the pipeline names.
        /// </summary>
        /// <value>The pipeline names.</value>
        [Option("pipelines", HelpText = "generate helptext for them", Required = false)]
        public BindingList<string> PipelineNames { get; set; } = new BindingList<string>();
    }

    /// <summary>
    /// Class HelpTextGeneratorTextVisitor.
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.IBuildTaskHelpTextVisitor" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.IBuildTaskHelpTextVisitor" />
    class HelpTextGeneratorTextVisitor : IBuildTaskHelpTextVisitor
    {
        /// <summary>
        /// The builder
        /// </summary>
        StringBuilder Builder;
        /// <summary>
        /// The task
        /// </summary>
        IBuildTask Task;
        /// <summary>
        /// The indent
        /// </summary>
        int Indent;
        /// <summary>
        /// The title indent
        /// </summary>
        string TitleIndent;
        /// <summary>
        /// The message indent
        /// </summary>
        string MessageIndent;

        /// <summary>
        /// Gets the formatter.
        /// </summary>
        /// <value>The formatter.</value>
        public IFormatter Formatter { get; private set; }

        public const int DefaultIndent = 4;
        public const int DefaultMessageIndent = 4;

        public readonly ILocalizeService Localize;

        /// <summary>
        /// Initializes a new instance of the <see cref="HelpTextGeneratorTextVisitor" /> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="task">The task.</param>
        /// <param name="formatter">The formatter.</param>
        /// <param name="indent">The indent.</param>
        /// <param name="messageIndent">The message indent.</param>
        internal HelpTextGeneratorTextVisitor(
            StringBuilder builder, 
            IBuildTask task, 
            IFormatter formatter, 
            int indent = DefaultIndent,
            int messageIndent = DefaultMessageIndent
            )
        {
            Localize = ServiceProvider.GetService<ILocalizeService>();

            Logger.Assert(Localize != null);

            Builder = builder;
            this.Task = task;
            this.Indent = indent;

            TitleIndent = CommandLine.Formatter.BuildBlankText(Indent);
            MessageIndent = TitleIndent + CommandLine.Formatter.BuildBlankText(messageIndent);

            if(formatter == null)
            {
                Formatter = new Formatter(Parser.DefaultIndent, Parser.DefaultBlank - 12);
            }
            else
            {
                Formatter = formatter;
            }
        }

        /// <summary>
        /// Adds the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="message">The message.</param>
        public void Add(string condition, string message)
        {
            var taskName = $"{TitleIndent}- Task is {Task.Settings.TaskName}:";
            Builder.Append(taskName);

            if(Task.Settings.TaskDescription.IsNotNullOrEmpty())
            {
                int blank = 47;
                if (taskName.Length < blank)
                {
                    Builder.Append(CommandLine.Formatter.BuildBlankText(blank - taskName.Length));
                    Builder.Append(Localize[Task.Settings.TaskDescription]);
                }
            }
            

            Builder.AppendLine();

            if (message.Trim().IsNotNullOrEmpty())
            {
                string[] lines = message.Split(Environment.NewLine);
                foreach (var line in lines)
                {
                    Builder.AppendLine($"{MessageIndent}{line}");
                }
            }
            else
            {
                Builder.AppendLine($"{MessageIndent}    No Arguments.");
            }
        }
    }
}
