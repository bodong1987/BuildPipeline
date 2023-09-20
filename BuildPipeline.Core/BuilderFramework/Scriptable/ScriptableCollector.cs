using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using PropertyModels.Extensions;

namespace BuildPipeline.Core.BuilderFramework.Scriptable
{
    /// <summary>
    /// Class ScriptableCollector.
    /// Implements the <see cref="AbstractImportable" />
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.IBuildTaskCollector" />
    /// </summary>
    /// <seealso cref="AbstractImportable" />
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.IBuildTaskCollector" />
    [Export("BuildTaskCollectors")]
    internal class ScriptableCollector : AbstractImportable, IBuildTaskCollector
    {
        /// <summary>
        /// Accepts the specified access token.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool Accept(object accessToken)
        {
            var services = ServiceProvider.GetServices(typeof(IScriptRuntimeService));

            return services.Length > 0;
        }

        /// <summary>
        /// Collects the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="existsTasks">The exists tasks.</param>
        /// <returns>IBuildTask[].</returns>
        public void Collect(IBuildContext context, IList<IBuildTask> existsTasks)
        {
            var plugin = PluginLoader.GetPlugin(context.Name);

            if(plugin == null)
            {
                Logger.LogError("Failed find plugin {0}", context.Name);
                return;
            }

            if(!plugin.ScriptsPath.IsDirectoryExists())
            {
                return;
            }

            var services = ServiceProvider.GetServices(typeof(IScriptRuntimeService)).Select(x=> x as IScriptRuntimeService);

            foreach(var file in Directory.GetFiles(plugin.ScriptsPath, "task_*.*", SearchOption.AllDirectories))
            {
                foreach(var s in services)
                {
                    if(s.AcceptScript(file))
                    {
                        Collect(context, existsTasks, s, file);

                        break;
                    }
                }
            }
        }

        private void Collect(IBuildContext context, IList<IBuildTask> existsTasks, IScriptRuntimeService service, string path)
        {
            dynamic Result = null;

            try
            {
                Result = service.Execute(path, ScriptableBuildTask.BuildTaskSettingsFactoryName);

                if(Result == null)
                {
                    return;
                }
            }
            catch(Exception e)
            {
                Logger.LogError("Error when Execute {0}:\n{1}", path, e.Message);
                return;
            }

            ScriptableBuildTaskSettings settings = Result as ScriptableBuildTaskSettings;
            settings.ScriptPath = path.MakeRelative(AppFramework.GetPublishApplicationDirectory());
            settings.ScriptService = service;

            if(context.CollectMode != TaskCollectMode.PureCollect)
            {
                // need condition match
                if (settings.ActiveCondition.IsNotNullOrEmpty() && context.Condition.IsNotNullOrEmpty() &&
                    !settings.ActiveCondition.iEquals(context.Condition))
                {
                    return;
                }
            }            

            ScriptableBuildTask task = new ScriptableBuildTask(settings);

            if (context.CollectMode != TaskCollectMode.PureCollect &&
                context.Arguments != null &&
                task.Settings != null &&
                task.Settings.Options != null &&
                !task.Settings.Options.OverrideFromCommandLineArgs(context.Arguments, out string message))
            {
                throw new ArgumentException($"Failed parse command line for task:{task}\n{message}");
            }

            existsTasks.Add(task);
        }
    }
}
