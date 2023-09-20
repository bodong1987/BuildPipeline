using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using PropertyModels.Extensions;
using System.Reflection;

namespace BuildPipeline.Core.BuilderFramework.Reflection
{
    #region Internal Reflection Collector
    /// <summary>
    /// Class ReflectedCollector.
    /// Implements the <see cref="AbstractImportable" />
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.IBuildTaskCollector" />
    /// </summary>
    /// <seealso cref="AbstractImportable" />
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.IBuildTaskCollector" />
    [Export("BuildTaskCollectors")]
    internal class ReflectedCollector : AbstractImportable, IBuildTaskCollector
    {
        /// <summary>
        /// Collects the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="existsTasks">The exists tasks.</param>
        /// <returns>IBuildTask[].</returns>
        public void Collect(IBuildContext context, IList<IBuildTask> existsTasks)
        {
            foreach (var assembly in AssemblyLoader.GetAllAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && type.IsDefined<AbstractBuildTaskExportAttribute>())
                    {
                        var attr = type.GetAnyCustomAttribute<AbstractBuildTaskExportAttribute>();

                        if (attr != null && attr.ContractName == context.Name && (context.CollectMode == TaskCollectMode.PureCollect || attr.Accept(context)))
                        {
                            CollectInClass(context, existsTasks, attr, type);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Collects the in class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="existsTasks">The exists tasks.</param>
        /// <param name="classAttr">The class attribute.</param>
        /// <param name="type">The type.</param>
        /// <exception cref="System.ArgumentException">Failed parse command line for task:{task}\n{message}</exception>
        private void CollectInClass(IBuildContext context, IList<IBuildTask> existsTasks, AbstractBuildTaskExportAttribute classAttr, Type type)
        {
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                var mainAttr = method.GetAnyCustomAttribute<BuildTaskMethodAttribute>();
                if (mainAttr != null)
                {
                    var parameters = method.GetParameters().ToList();

                    if (parameters.Count < 3 ||
                        parameters.Count > 4 ||
                        !typeof(IBuildContext).IsAssignableFrom(parameters[0].ParameterType) ||
                        !typeof(IExcecuteObserver).IsAssignableFrom(parameters[1].ParameterType) ||
                        !typeof(CancellationTokenSource).IsAssignableFrom(parameters[2].ParameterType) ||
                        (parameters.Count == 4 &&
                            !typeof(IBuildTaskOptions).IsAssignableFrom(parameters[3].ParameterType)
                        )
                        )
                    {
                        Logger.LogError("If you want to create a Build task method from ({0}|{1}),\n" +
                            "Arguments count must be 3 or 4\n" +
                            "first must be T context, T must be implement from IBuildContext\n" +
                            "second argument must be IExcecuteObserver observer\n" +
                            "third argument must be CancellationTokenSource cancellationTokenSource\n" +
                            "optional third argument must be T options, T must be implement from IBuildTaskOptions", type.FullName, method.Name);

                        continue;
                    }

                    var returnType = method.GetReturnType();

                    if (returnType != typeof(int) && returnType != typeof(Task<int>))
                    {
                        Logger.LogError("If you want to create a Build task method from ({0}|{1}), the return type must be int or Task<int>... ", type.FullName, method.Name);

                        continue;
                    }

                    ReflectedBuildTaskMethod methodInfo = new ReflectedBuildTaskMethod(method, mainAttr, classAttr);

                    ReflectedBuildTask task = new ReflectedBuildTask(methodInfo);

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
    }
    #endregion

}
