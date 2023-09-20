using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Utils;

namespace BuildPipeline.Core.BuilderFramework.Reflection
{
    /// <summary>
    /// Class ReflectedBuildTask.
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.AbstractBuildTask" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.AbstractBuildTask" />
    internal class ReflectedBuildTask : AbstractBuildTask
    {
        /// <summary>
        /// The method information
        /// </summary>
        public readonly ReflectedBuildTaskMethod MethodInfo;

        /// <summary>
        /// Gets the import priority.
        /// </summary>
        /// <value>The import priority.</value>
        public override int ImportPriority => MethodInfo.Attribute.TaskOrder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectedBuildTask"/> class.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        public ReflectedBuildTask(ReflectedBuildTaskMethod methodInfo)
        {
            MethodInfo = methodInfo;
            ClassAttribute = methodInfo.ClassAttribute;
            Settings = new ReflectedBuildTaskSettings(methodInfo);

            foreach(var r in methodInfo.Requirements)
            {
                Requirements.Require(r);
            }
        }
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{TaskName}[{MethodInfo.Method.DeclaringType.Name}.{MethodInfo.Method.Name}]";
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
            List<object> args = new List<object>() { context, observer, cancellationTokenSource };

            if(Settings.Options != null)
            {
                args.Add(Settings.Options);
            }

            var returnType = MethodInfo.Method.GetReturnType();
            if (returnType == typeof(int))
            {
                int value = (int)MethodInfo.Method.Invoke(null, args.ToArray());

                return value;
            }
            else if(returnType == typeof(Task<int>))
            {
                return await (Task<int>)MethodInfo.Method.Invoke(null, args.ToArray());
            }

            Logger.LogError("Invalid Reflected Build Task: {0}[Return Type is not valid.]", MethodInfo);
            return -1;
        }
    }
}
