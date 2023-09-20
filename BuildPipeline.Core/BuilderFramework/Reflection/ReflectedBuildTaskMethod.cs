using BuildPipeline.Core.Extensions;
using PropertyModels.Extensions;
using System.Reflection;

namespace BuildPipeline.Core.BuilderFramework.Reflection
{
    /// <summary>
    /// Class ReflectedBuildTaskMethod.
    /// </summary>
    internal class ReflectedBuildTaskMethod : IEnvironmentRequirementTarget
    {
        /// <summary>
        /// The method
        /// </summary>
        public readonly MethodInfo Method;
        /// <summary>
        /// The attribute
        /// </summary>
        public readonly BuildTaskMethodAttribute Attribute;

        /// <summary>
        /// The class attribute
        /// </summary>
        public readonly AbstractBuildTaskExportAttribute ClassAttribute;

        /// <summary>
        /// The requirements
        /// </summary>
        public IEnvironmentRequirementCollection Requirements { get; private set; } = new EnvironmentRequirementCollection();

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name => Attribute.TaskName;

        /// <summary>
        /// The options
        /// </summary>
        public readonly Type OptionsType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectedBuildTaskMethod" /> class.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="classAttribute">The class attribute.</param>
        public ReflectedBuildTaskMethod(MethodInfo method, BuildTaskMethodAttribute attribute, AbstractBuildTaskExportAttribute classAttribute)
        {
            Method = method;
            Attribute = attribute;
            ClassAttribute = classAttribute;

            var rqAttrs = method.GetCustomAttributes<AbstractRequirementAttribute>();
            foreach (var attr in rqAttrs)
            {
                Requirements.Require(attr.Requirement);
            }

            var parameters = Method.GetParameters();

            if (parameters != null && parameters.Length >= 4)
            {
                ParameterInfo optionInfo = parameters[3];

                if (optionInfo.ParameterType.IsImplementFrom<ICommandLineConvertible>() &&
                    optionInfo.ParameterType.IsClass &&
                    !optionInfo.ParameterType.IsAbstract)
                {
                    OptionsType = optionInfo.ParameterType;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{Method.DeclaringType.FullName}:{Method.Name}";
        }
    }
}
