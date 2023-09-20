using BuildPipeline.Core.Utils;

namespace BuildPipeline.Core.BuilderFramework.Reflection.Requirements
{
    /// <summary>
    /// Class OperatingSystemRequirement.
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.AbstractEnvironmentRequirement" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.AbstractEnvironmentRequirement" />
    public class OperatingSystemRequirement : AbstractEnvironmentRequirement
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public override string Name => "Operating System";

        /// <summary>
        /// Gets or sets the platforms.
        /// </summary>
        /// <value>The platforms.</value>
        public PlatformType[] Platforms { get; set; }

        /// <summary>
        /// Gets the platforms description.
        /// </summary>
        /// <value>The platforms description.</value>
        public string PlatformsDescription => EnvironmentRequirementExtensions.GetPlatforms(Platforms);

        /// <summary>
        /// Gets the requirement description.
        /// 简单描述
        /// </summary>
        /// <value>The requirement description.</value>
        public override string RequirementDescription
        {
            get
            {
                if (MinVersion != null && MaxVersion != null)
                {
                    return $"Require Operating System: {PlatformsDescription}, version range is [{MinVersion}, {MaxVersion}]";
                }
                else if (MinVersion != null)
                {
                    return $"Require Operating System: {PlatformsDescription}, minimum version is {MinVersion}";
                }
                else if (MaxVersion != null)
                {
                    return $"Require Operating System: {PlatformsDescription}, highest version is {MaxVersion}";
                }

                return $"Require Operating System: {PlatformsDescription}";
            }
        }

        /// <summary>
        /// Gets the requirement help.
        /// 描述+帮助文本
        /// </summary>
        /// <value>The requirement help.</value>
        public override string RequirementHelp => RequirementDescription;

        /// <summary>
        /// Gets or sets the minimum version.
        /// </summary>
        /// <value>The minimum version.</value>
        public Version MinVersion { get; set; }

        /// <summary>
        /// Gets or sets the maximum version.
        /// </summary>
        /// <value>The maximum version.</value>
        public Version MaxVersion { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperatingSystemRequirement"/> class.
        /// </summary>
        /// <param name="platforms">The platforms.</param>
        public OperatingSystemRequirement(params PlatformType[] platforms)
        {
            Platforms = platforms;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperatingSystemRequirement"/> class.
        /// </summary>
        /// <param name="minVersion">The minimum version.</param>
        /// <param name="platforms">The platforms.</param>
        public OperatingSystemRequirement(Version minVersion, params PlatformType[] platforms)
        {
            Platforms = platforms;
            MinVersion = minVersion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperatingSystemRequirement"/> class.
        /// </summary>
        /// <param name="minVersion">The minimum version.</param>
        /// <param name="maxVersion">The maximum version.</param>
        /// <param name="platforms">The platforms.</param>
        public OperatingSystemRequirement(Version minVersion, Version maxVersion, params PlatformType[] platforms)
        {
            Platforms = platforms;
            MinVersion = minVersion;
            MaxVersion = maxVersion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperatingSystemRequirement" /> class.
        /// </summary>
        /// <param name="minVersion">The minimum version.</param>
        /// <param name="platforms">The platforms.</param>
        public OperatingSystemRequirement(string minVersion, params PlatformType[] platforms)
        {
            Platforms = platforms;
            MinVersion = Version.Parse(minVersion);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperatingSystemRequirement"/> class.
        /// </summary>
        /// <param name="minVersion">The minimum version.</param>
        /// <param name="maxVersion">The maximum version.</param>
        /// <param name="platforms">The platforms.</param>
        public OperatingSystemRequirement(string minVersion, string maxVersion, params PlatformType[] platforms)
        {
            Platforms = platforms;
            MinVersion = Version.Parse(minVersion);
            MaxVersion = Version.Parse(maxVersion);
        }

        /// <summary>
        /// Checks the requirement.
        /// </summary>
        /// <returns>CheckRequirementResult.</returns>
        public override CheckRequirementResult CheckRequirement()
        {
            if(Platforms != null && Platforms.Length > 0 && !Platforms.Any(x => PlatformUtils.IsPlatform(x)))
            {
                return new CheckRequirementResult(CheckRequirementResultType.Dissatisfied, $"Can only be running on {PlatformsDescription}");
            }

            if(MinVersion != null && Environment.OSVersion.Version < MinVersion)
            {
                return new CheckRequirementResult(CheckRequirementResultType.Dissatisfied, $"{Environment.OSVersion} is lower than {MinVersion}");
            }

            if(MaxVersion != null && Environment.OSVersion.Version > MaxVersion)
            {
                return new CheckRequirementResult(CheckRequirementResultType.Dissatisfied, $"{Environment.OSVersion} is higher than {MaxVersion}");
            }

            return new CheckRequirementResult(CheckRequirementResultType.CompletelySatisfied, "");
        }
    }

    /// <summary>
    /// Class RequireOperatingSystemAttribute.
    /// Implements the <see cref="AbstractRequirementAttribute" />
    /// </summary>
    /// <seealso cref="AbstractRequirementAttribute" />
    public class RequireOperatingSystemAttribute : AbstractRequirementAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequireOperatingSystemAttribute"/> class.
        /// </summary>
        /// <param name="platforms">The platforms.</param>
        public RequireOperatingSystemAttribute(params PlatformType[] platforms)
        {
            Requirement = new OperatingSystemRequirement(platforms);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequireOperatingSystemAttribute"/> class.
        /// </summary>
        /// <param name="minVersion">The minimum version.</param>
        /// <param name="platforms">The platforms.</param>
        public RequireOperatingSystemAttribute(string minVersion, params PlatformType[] platforms)
        {
            Requirement = new OperatingSystemRequirement(minVersion, platforms);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequireOperatingSystemAttribute"/> class.
        /// </summary>
        /// <param name="minVersion">The minimum version.</param>
        /// <param name="maxVersion">The maximum version.</param>
        /// <param name="platforms">The platforms.</param>
        public RequireOperatingSystemAttribute(string minVersion, string maxVersion, params PlatformType[] platforms)
        {
            Requirement = new OperatingSystemRequirement(minVersion, maxVersion, platforms);
        }
    }
}
