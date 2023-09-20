using BuildPipeline.Core.CommandLine;
using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using PropertyModels.Extensions;
using System.ComponentModel;
using System.Xml.Serialization;

namespace BuildPipeline.Core.BuilderFramework.Scriptable
{
    /// <summary>
    /// Class ScriptableBuildTaskSettings.
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.BuildTaskSettings" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.BuildTaskSettings" />
    public class ScriptableBuildTaskSettings : BuildTaskSettings, IEnvironmentRequirementTarget
    {
        string ScriptPathCore = "";

        /// <summary>
        /// Gets or sets the script path.
        /// </summary>
        /// <value>The script path.</value>

        public string ScriptPath
        {
            get => ScriptPathCore;
            set
            {
                if(ScriptPathCore != value)
                {
                    ScriptPathCore = value;

                    if(ScriptPathCore != null)
                    {
                        AbsoluteScriptPath = AppFramework.GetPublishApplicationDirectory().JoinPath(ScriptPathCore);
                    }
                    else
                    {
                        AbsoluteScriptPath = "";
                    }
                }
            }
        }

        /// <summary>
        /// Gets the absolute script path.
        /// </summary>
        /// <value>The absolute script path.</value>
        [XmlIgnore]
        public string AbsoluteScriptPath { get; private set; }

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>The mode.</value>
        public ScriptableExecuteMode Mode { get; set; } = ScriptableExecuteMode.External;

        /// <summary>
        /// Gets or sets the script service.
        /// </summary>
        /// <value>The script service.</value>
        [XmlIgnore]
        public IScriptRuntimeService ScriptService { get; internal set; }

        /// <summary>
        /// Gets or sets the match condition.
        /// </summary>
        /// <value>The match condition.</value>
        [XmlIgnore]
        public string ActiveCondition { get; set; } = "";

        string TaskDescriptionCore = "";
        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        [ReadOnly(true)]
        [XmlIgnore]
        public override string TaskDescription
        {
            get => ScriptService != null && ScriptPath.IsNotNullOrEmpty()? $"{TaskDescriptionCore}[{ScriptService.ServiceName}]({ScriptPath})" : TaskDescriptionCore;
            internal set
            {
                TaskDescriptionCore = value;
            }
        }

        /// <summary>
        /// Gets the requirements.
        /// </summary>
        /// <value>The requirements.</value>
        [XmlIgnore]
        public IEnvironmentRequirementCollection Requirements { get; set; } = new EnvironmentRequirementCollection();

        /// <summary>
        /// Gets the default options.
        /// </summary>
        /// <value>The default options.</value>
        [XmlIgnore]
        public ScriptableBuildTaskOptions DefaultOptions { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptableBuildTaskSettings"/> class.
        /// </summary>
        public ScriptableBuildTaskSettings() 
        {
            Options = new ScriptableBuildTaskOptions();   
        }

        /// <summary>
        /// Makes the default options.
        /// </summary>
        internal void MakeDefaultOptions()
        {
            DefaultOptions = (Options as ScriptableBuildTaskOptions).Clone() as ScriptableBuildTaskOptions;

            Logger.Assert(DefaultOptions != null);
        }

        /// <summary>
        /// Formats the options command line.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>System.String.</returns>
        public override string FormatOptionsCommandLine(CommandLineFormatMethod method)
        {
            return Parser.FormatCommandLine(this.Options, method, this.DefaultOptions);
        }

        #region Factory
        /// <summary>
        /// Creates the settings.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <returns>ScriptableBuildTaskSettings.</returns>
        public static ScriptableBuildTaskSettings CreateSettings(
            string name,
            string description,
            int order
            )
        {
            ScriptableBuildTaskSettings settings = new ScriptableBuildTaskSettings();
            settings.TaskName = name;
            settings.TaskDescription = description;
            settings.TaskOrder = order;

            return settings;
        }
        #endregion
    }
}
