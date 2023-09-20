using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Framework;
using PropertyModels.Extensions;

namespace BuildPipeline.Core.BuilderFramework.Implements
{
    #region Collector
    /// <summary>
    /// Class BuildTaskCollector.
    /// </summary>
    internal class BuildTaskCollector : AbstractComposableTarget
    {
        /// <summary>
        /// All tasks
        /// </summary>
        internal readonly List<IBuildTask> AllTasks = new List<IBuildTask>();

        /// <summary>
        /// The filtered tasks
        /// </summary>
        internal readonly List<IBuildTask> FilteredTasks = new List<IBuildTask>();

        /// <summary>
        /// The graph
        /// </summary>
        internal readonly BuildTaskGraph Graph = new BuildTaskGraph();

        /// <summary>
        /// The collectors
        /// </summary>
        [Import("BuildTaskCollectors")]
        public List<IBuildTaskCollector> Collectors = new List<IBuildTaskCollector>();

        public BuildTaskCollector()
        {
            ExtensibilityFramework.ComposeParts(this, this);
        }

        /// <summary>
        /// Collects the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Collect(IBuildContext context)
        {
            foreach(var c in Collectors)
            {
                c.Collect(context, AllTasks);
            }

            PostCollect(context);
        }

        private void PostCollect(IBuildContext context)
        {
            if (context.CollectMode == TaskCollectMode.PureCollect)
            {
                FilteredTasks.AddRange(AllTasks);
            }
            else if (context.CollectMode == TaskCollectMode.Common)
            {
                foreach (var task in AllTasks)
                {
                    if (context.StartMode == TaskStartMode.Common)
                    {
                        if (AcceptTask(context, task))
                        {
                            FilteredTasks.Add(task);
                        }
                    }
                    else if (context.StartMode == TaskStartMode.StartPoint)
                    {
                        if (AcceptTask(context, task))
                        {
                            FilteredTasks.Add(task);
                        }
                        else if (FilteredTasks.Count > 0 &&
                            !IsExcludeTask(context, task) &&
                            FilteredTasks.FirstOrDefault().Settings.TaskOrder <= task.Settings.TaskOrder)
                        {
                            FilteredTasks.Add(task);
                        }
                    }
                }
            }

            if (context.CollectMode != TaskCollectMode.PureCollect)
            {
                Graph.BuildGraph(context, FilteredTasks, AllTasks.FindAll(x=> !FilteredTasks.Contains(x)));
            }
        }

        #region Filters

        private static bool AcceptTask(IBuildContext context, IBuildTask task)
        {
            if (context == null)
            {
                return false;
            }

            if(context.TaskNames != null && context.TaskNames.Count() > 0)
            {
                if(context.TaskNames.IndexOf(x=> x.iEquals(task.Settings.TaskName)) == -1)
                {
                    return false;
                }
            }

            if(IsExcludeTask(context, task))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether [is exclude task] [the specified context].
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="task">The task.</param>
        /// <returns><c>true</c> if [is exclude task] [the specified context]; otherwise, <c>false</c>.</returns>
        private static bool IsExcludeTask(IBuildContext context, IBuildTask task)
        {
            if (context != null && context.ExcludeTaskNames != null)
            {
                if (context.ExcludeTaskNames.IndexOf(x => x.iEquals(task.Settings.TaskName)) != -1)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
    #endregion

    #region Internal Extensibility Framework Collector
    [Export("BuildTaskCollectors")]
    internal class InternalExtensibilityCollector : AbstractImportable, IBuildTaskCollector
    {
        public void Collect(IBuildContext context, IList<IBuildTask> existsTasks)
        {
            var parts = ExtensibilityFramework.GetExportParts(context.Name);

            if (parts == null)
            {
                // Logger.LogWarning("Failed find task by ContractName = {0}", context.Name);
                return;
            }

            foreach (var part in parts)
            {
                if (part.Type.IsImplementFrom<IBuildTask>())
                {
                    var task = part.CreatePartObject(CreationPolicy.Shared) as IBuildTask;

                    if (task != null && (context.CollectMode == TaskCollectMode.PureCollect || task.Accept(context)))
                    {
                        // try set options for each task...
                        if (context.CollectMode != TaskCollectMode.PureCollect &&
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
        }
    }
    #endregion

}
