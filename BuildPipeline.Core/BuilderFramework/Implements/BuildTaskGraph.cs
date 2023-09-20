using BuildPipeline.Core.Utils;
using System.Collections;
using System.Text;

namespace BuildPipeline.Core.BuilderFramework.Implements
{
    /// <summary>
    /// Class BuildTaskGraph.
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.IBuildTaskGraph" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.IBuildTaskGraph" />
    internal class BuildTaskGraph : IBuildTaskGraph
    {
        /// <summary>
        /// All tasks
        /// </summary>
        readonly List<IBuildTask> IncludeTasks = new List<IBuildTask>();

        /// <summary>
        /// The exclude tasks
        /// </summary>
        readonly List<IBuildTask> ExcludeTasks = new List<IBuildTask>();

        /// <summary>
        /// All tasks
        /// </summary>
        readonly List<IBuildTask> AllTasks = new List<IBuildTask>();

        /// <summary>
        /// The stages
        /// </summary>
        readonly SortedList<int, BuildStage> Stages = new SortedList<int, BuildStage>();

        /// <summary>
        /// The task dictionary
        /// </summary>
        readonly Dictionary<string, IBuildTask> TaskDict = new Dictionary<string, IBuildTask>();        

        /// <summary>
        /// Builds the graph.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="includeTasks">The include tasks.</param>
        /// <param name="excludeTasks">The exclude tasks.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.InvalidProgramException"></exception>
        public void BuildGraph(IBuildContext context, IEnumerable<IBuildTask> includeTasks, IEnumerable<IBuildTask> excludeTasks)
        {
            IncludeTasks.Clear();
            Stages.Clear();
            TaskDict.Clear();
            ExcludeTasks.Clear();
            AllTasks.Clear();

            List<IBuildTask> tasks = new List<IBuildTask>();
            tasks.AddRange(includeTasks);
            tasks.AddRange(excludeTasks);
            AllTasks.AddRange(tasks.OrderBy(x => x.Settings.TaskOrder));

            ExcludeTasks.AddRange(excludeTasks);

            var condTasks = includeTasks.OrderBy(x=>x.Settings.TaskOrder).ToList();

            BuildStage previousStage = null;

            foreach(var task in condTasks)
            {
                if(TaskDict.TryGetValue(task.Settings.TaskName, out var t))
                {
                    string msg = string.Format("Found Repeat Task name in same pipeline {0}, existsOne={1}, newOne={2}", context.Name, t, task);
                    Logger.LogError(msg);

                    throw new InvalidOperationException(msg);
                }
                else
                {
                    TaskDict.Add(task.Settings.TaskName, task);
                }

                if (previousStage == null)
                {
                    previousStage = new BuildStage(task);
                    Stages.Add(previousStage.OrderId, previousStage);
                }
                else
                {
                    if(previousStage.OrderId == task.ImportPriority)
                    {
                        var existsTask = previousStage.Tasks.FirstOrDefault();

                        if(!existsTask.Settings.CanAsync || !task.Settings.CanAsync)
                        {
                            throw new InvalidProgramException($"{existsTask.Settings.TaskName}({existsTask.GetType().FullName}) and {task.Settings.TaskName}({task.GetType().FullName}) need CanAsync=True");
                        }

                        previousStage.AddTask(task);
                    }
                    else
                    {
                        var newStage = new BuildStage(task);
                        
                        newStage.PreviousStage = previousStage;
                        previousStage.NextStage = newStage;

                        Stages.Add(newStage.OrderId, newStage);

                        previousStage = newStage;
                    }
                }
                
                IncludeTasks.Add(task);
            }
        }

        /// <summary>
        /// Finds the task.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>IBuildTask.</returns>
        public IBuildTask FindTask(string name)
        {
            TaskDict.TryGetValue(name, out var task);
            return task;
        }

        /// <summary>
        /// Gets the include tasks.
        /// </summary>
        /// <returns>IBuildTask[].</returns>
        public IBuildTask[] GetIncludeTasks()
        {
            return IncludeTasks.ToArray();
        }

        /// <summary>
        /// Gets the exclude tasks.
        /// </summary>
        /// <returns>IBuildTask[].</returns>
        public IBuildTask[] GetExcludeTasks()
        {
            return ExcludeTasks.ToArray();
        }

        /// <summary>
        /// Gets all tasks.
        /// </summary>
        /// <returns>IBuildTask[].</returns>
        public IBuildTask[] GetAllTasks()
        {
            return AllTasks.ToArray();
        }

        /// <summary>
        /// Gets the associate tasks.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="associateType">Type of the associate.</param>
        /// <returns>IBuildTask[].</returns>
        public IBuildTask[] GetAssociateTasks(IBuildTask task, TaskAssociateType associateType = TaskAssociateType.All)
        {
            if(Stages.TryGetValue(task.ImportPriority, out var result))
            {
                if(associateType == TaskAssociateType.All)
                {
                    return result.Tasks;
                }
                else if(associateType == TaskAssociateType.Previous)
                {
                    if(result.PreviousStage!=null)
                    {
                        return result.PreviousStage.Tasks;
                    }
                }
                else if(associateType == TaskAssociateType.Next)
                {
                    if(result.NextStage!=null)
                    {
                        return result.NextStage.Tasks;
                    }
                }
                else if(associateType == TaskAssociateType.PreviousAll)
                {
                    List<IBuildTask> list = new List<IBuildTask>();

                    BuildStage previous = result.PreviousStage;

                    while(previous != null)
                    {
                        list.InsertRange(0, previous.Tasks);

                        previous = previous.PreviousStage;
                    }

                    return list.ToArray();
                }
                else if(associateType == TaskAssociateType.NextAll)
                {
                    List<IBuildTask> list = new List<IBuildTask>();

                    BuildStage next = result.NextStage;

                    while(next != null)
                    {
                        list.AddRange(next.Tasks);

                        next = next.NextStage;
                    }

                    return list.ToArray();
                }
            }

            return new IBuildTask[] { };
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<IBuildTaskStage> GetEnumerator()
        {
            return Stages.Values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Stages.Values.GetEnumerator();
        }
    }

    #region Stage
    /// <summary>
    /// Class BuildStage.
    /// </summary>
    internal class BuildStage : IBuildTaskStage
    {
        /// <summary>
        /// Gets the order identifier.
        /// </summary>
        /// <value>The order identifier.</value>
        public int OrderId { get; private set; }

        /// <summary>
        /// The tasks core
        /// </summary>
        readonly List<IBuildTask> TasksCore = new List<IBuildTask>();

        /// <summary>
        /// Gets the tasks.
        /// </summary>
        /// <value>The tasks.</value>
        public IBuildTask[] Tasks => TasksCore.ToArray();

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count => TasksCore.Count;

        /// <summary>
        /// Gets or sets the previous stage.
        /// </summary>
        /// <value>The previous stage.</value>
        internal BuildStage PreviousStage { get; set; }

        /// <summary>
        /// Gets or sets the next stage.
        /// </summary>
        /// <value>The next stage.</value>
        internal BuildStage NextStage { get; set; }

        /// <summary>
        /// Gets the previous.
        /// </summary>
        /// <value>The previous.</value>
        public IBuildTaskStage Previous => PreviousStage;

        /// <summary>
        /// Gets the next.
        /// </summary>
        /// <value>The next.</value>
        public IBuildTaskStage Next => NextStage;

        /// <summary>
        /// Gets the pipeline.
        /// </summary>
        /// <value>The pipeline.</value>
        IBuildPipeline IBuildTaskStage.Pipeline { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildStage"/> class.
        /// </summary>
        /// <param name="task">The task.</param>
        public BuildStage(IBuildTask task)
        {
            OrderId = task.ImportPriority;

            AddTask(task);
        }

        /// <summary>
        /// Adds the task.
        /// </summary>
        /// <param name="task">The task.</param>
        public void AddTask(IBuildTask task)
        {
            TasksCore.Add(task);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"[{OrderId}]-({TasksCore.Count})");
            foreach(var i in TasksCore)
            {
                stringBuilder.Append($"{i.Settings.TaskName}, ");
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<IBuildTask> GetEnumerator()
        {
            return TasksCore.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return TasksCore.GetEnumerator();
        }
    }
    #endregion

}
