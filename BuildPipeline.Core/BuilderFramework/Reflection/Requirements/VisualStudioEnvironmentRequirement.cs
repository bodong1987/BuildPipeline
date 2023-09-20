using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Services;
using BuildPipeline.Core.Utils;

namespace BuildPipeline.Core.BuilderFramework.Reflection.Requirements
{
    /// <summary>
    /// Class VisualStudioEnvironmentRequirement.    
    /// </summary>
    public class VisualStudioEnvironmentRequirement : EnvironmentServiceRequirement<IVisualStudioEnvironmentService>
    {
        /// <summary>
        /// Gets or sets a value indicating whether [visual CPP required].
        /// </summary>
        /// <value><c>true</c> if [visual CPP required]; otherwise, <c>false</c>.</value>
        public bool VisualCPPRequired { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualStudioEnvironmentRequirement"/> class.
        /// </summary>
        public VisualStudioEnvironmentRequirement() 
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualStudioEnvironmentRequirement"/> class.
        /// </summary>
        /// <param name="minVersion">The minimum version.</param>
        public VisualStudioEnvironmentRequirement(VisualStudioType minVersion) :
            base(Version.Parse(GetVSType(minVersion)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualStudioEnvironmentRequirement"/> class.
        /// </summary>
        /// <param name="minVersion">The minimum version.</param>
        /// <param name="maxVersion">The maximum version.</param>
        public VisualStudioEnvironmentRequirement(VisualStudioType minVersion, VisualStudioType maxVersion) :
            base(Version.Parse(GetVSType(minVersion)), Version.Parse(GetVSType(maxVersion)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualStudioEnvironmentRequirement"/> class.
        /// </summary>
        /// <param name="minVersion">The minimum version.</param>
        /// <param name="maxVersion">The maximum version.</param>
        public VisualStudioEnvironmentRequirement(Version minVersion, Version maxVersion = null) :
            base(minVersion, maxVersion)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualStudioEnvironmentRequirement"/> class.
        /// </summary>
        /// <param name="minVersion">The minimum version.</param>
        /// <param name="maxVersion">The maximum version.</param>
        public VisualStudioEnvironmentRequirement(string minVersion, string maxVersion = null) :
            base(minVersion, maxVersion)
        {
        }

        /// <summary>
        /// Checks the requirement.
        /// </summary>
        /// <returns>CheckRequirementResultType.</returns>
        public override CheckRequirementResult CheckRequirement()
        {
            IVisualStudioEnvironmentService service = ServiceProvider.GetService<IVisualStudioEnvironmentService>();

            if (service == null)
            {
                return GenError("IVisualStudioEnvironmentService is not available.");
            }

            foreach(var i in service.Installations)
            {
                string message;
                if(CheckVersion(i.Version, i.Name, out message))
                {
                    if(!VisualCPPRequired || (VisualCPPRequired && i.IsVisualCPPInstalled))
                    {
                        return new CheckRequirementResult(CheckRequirementResultType.CompletelySatisfied, "");
                    }
                    else
                    {
                        Logger.LogVerbose("Missing Visual C++ installation for {0}", i.Name);
                    }
                }
            }

            return base.CheckRequirement();
        }

        #region Converter
        /// <summary>
        /// Gets the type of the vs.
        /// see also: https://en.wikipedia.org/wiki/Visual_Studio
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>System.String.</returns>
        public static string GetVSType(VisualStudioType type)
        {
            switch (type)
            {
                case VisualStudioType.VS_Lastest:
                case VisualStudioType.VS_Unknown:
                    return "";
                case VisualStudioType.VS_2010:
                    return "10.0";
                case VisualStudioType.VS_2012:
                    return "11.0";
                case VisualStudioType.VS_2013:
                    return "12.0";
                case VisualStudioType.VS_2015:
                    return "14.0";
                case VisualStudioType.VS_2017:
                    return "15.0";
                case VisualStudioType.VS_2019:
                    return "16.0";
                case VisualStudioType.VS_2022:
                    return "17.0";
            }

            return "0.0";
        }
        #endregion
    }

    /// <summary>
    /// Class RequireVisualStudioAttribute.    
    /// </summary>
    public class RequireVisualStudioAttribute : AbstractRequirementAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequireVisualStudioAttribute"/> class.
        /// </summary>
        /// <param name="minVersionType">Minimum type of the version.</param>
        public RequireVisualStudioAttribute(VisualStudioType minVersionType) 
        {
            Requirement = new VisualStudioEnvironmentRequirement(minVersionType);
        }
    }
}
