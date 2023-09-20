using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Services;
using PropertyModels.Extensions;

namespace BuildPipeline.Core.BuilderFramework.Reflection
{
    /// <summary>
    /// Class BuildTaskMethodAttribute.
    /// Implements the <see cref="Attribute" />
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class BuildTaskMethodAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the task.
        /// </summary>
        /// <value>The name of the task.</value>
        public string TaskName { get; set; }

        /// <summary>
        /// Gets or sets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public string TaskDescription { get; set; } = "";

        /// <summary>
        /// Gets the task order.
        /// </summary>
        /// <value>The task order.</value>
        public int TaskOrder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can asynchronous.
        /// </summary>
        /// <value><c>true</c> if this instance can asynchronous; otherwise, <c>false</c>.</value>
        public bool CanAsync { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether [wait result].
        /// </summary>
        /// <value><c>true</c> if [wait result]; otherwise, <c>false</c>.</value>
        public bool WaitResult { get; set; } = true;
        /// <summary>
        /// Gets or sets a value indicating whether this instance can failure.
        /// </summary>
        /// <value><c>true</c> if this instance can failure; otherwise, <c>false</c>.</value>
        public bool CanFailure { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildTaskMethodAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="order">The order.</param>
        public BuildTaskMethodAttribute(string name, int order)
        {
            TaskName = name;
            TaskOrder = order;
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public virtual void CopyTo(BuildTaskSettings settings)
        {
            settings.TaskDescription = TaskDescription.IsNullOrEmpty() ? TaskName : TaskDescription;
            settings.TaskName = TaskName;
            settings.TaskOrder = TaskOrder;
            settings.CanAsync = CanAsync;
            settings.CanFailure = CanFailure;
            settings.WaitResult = WaitResult;
        }
    }

    /// <summary>
    /// Class AbstractRequirementAttribute.
    /// Implements the <see cref="Attribute" />
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public abstract class AbstractRequirementAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the requirement.
        /// </summary>
        /// <value>The requirement.</value>
        public IEnvironmentRequirement Requirement { get; protected set; }
    }


    /// <summary>
    /// Class RequirementServiceAttribute.
    /// Implements the <see cref="Attribute" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RequireServiceAttribute<T> : AbstractRequirementAttribute
        where T : class, IService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequireServiceAttribute{T}"/> class.
        /// </summary>
        public RequireServiceAttribute()
        {
            Requirement = new ServiceRequirement<T>();
        }
    }

    /// <summary>
    /// Class RequiementEnvironmentAttribute.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RequireEnvironmentAttribute<T> : AbstractRequirementAttribute
        where T : class, IEnvironmentService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequireEnvironmentAttribute{T}"/> class.
        /// </summary>
        /// <param name="minVersion">The minimum version.</param>
        /// <param name="maxVersion">The maximum version.</param>
        public RequireEnvironmentAttribute(string minVersion = null, string maxVersion = null)
        {
            Requirement = new EnvironmentServiceRequirement<T>(minVersion, maxVersion);
        }
    }

    /// <summary>
    /// Class RequireAttribtue.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RequireAttribtue<T> : AbstractRequirementAttribute
        where T : class, IEnvironmentRequirement, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequireAttribtue{T}"/> class.
        /// </summary>
        public RequireAttribtue()
        {
            Requirement = new T();
        }
    }

}
