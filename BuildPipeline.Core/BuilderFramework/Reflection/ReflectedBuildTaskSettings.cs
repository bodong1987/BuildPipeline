using BuildPipeline.Core.Serialization;

namespace BuildPipeline.Core.BuilderFramework.Reflection
{
    /// <summary>
    /// Class ReflectedBuildTaskSettings.
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.BuildTaskSettings" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.BuildTaskSettings" />
    [ObjectXmlSerializePolicy(XmlSerializationPolicy.IgnoreFields)]
    internal class ReflectedBuildTaskSettings : BuildTaskSettings
    {
        /// <summary>
        /// For serialization...
        /// Initializes a new instance of the <see cref="ReflectedBuildTaskSettings"/> class.        /// 
        /// </summary>
        public ReflectedBuildTaskSettings()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectedBuildTaskSettings" /> class.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        public ReflectedBuildTaskSettings(ReflectedBuildTaskMethod methodInfo)
        {
            methodInfo.Attribute.CopyTo(this);

            Options = methodInfo.OptionsType != null ? Activator.CreateInstance(methodInfo.OptionsType) as IBuildTaskOptions : null;
        }
    }

}
