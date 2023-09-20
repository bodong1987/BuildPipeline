using BuildPipeline.Core.CommandLine;
using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Services;
using BuildPipeline.Core.Utils;
using PropertyModels.Extensions;
using System;
using System.Collections;
using System.Text;

namespace BuildPipeline.Core.BuilderFramework
{
    #region Interfaces
    /// <summary>
    /// Enum CheckRequirementResultType
    /// </summary>
    public enum CheckRequirementResultType
    {
        /// <summary>
        /// The completely satisfied
        /// </summary>
        CompletelySatisfied,

        /// <summary>
        /// The partially satisfied
        /// </summary>
        PartiallySatisfied,

        /// <summary>
        /// The dissatisfied
        /// </summary>
        Dissatisfied,

        /// <summary>
        /// The unconcerned
        /// </summary>
        Unconcerned,
    }

    /// <summary>
    /// Struct CheckRequirementResult
    /// </summary>
    public struct CheckRequirementResult
    {
        /// <summary>
        /// The result
        /// </summary>
        public readonly CheckRequirementResultType Result;
        /// <summary>
        /// The error message
        /// </summary>
        public readonly string ErrorMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckRequirementResult"/> struct.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="errorMessage">The error message.</param>
        public CheckRequirementResult(CheckRequirementResultType result, string errorMessage)
        {
            Result = result;
            ErrorMessage = errorMessage;
        }
    }

    /// <summary>
    /// Interface IEnvironmentRequirement
    /// </summary>
    public interface IEnvironmentRequirement 
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the requirement description.
        /// Quick description
        /// </summary>
        /// <value>The requirement description.</value>
        string RequirementDescription { get; }

        /// <summary>
        /// Gets the requirement help.
        /// description + help text
        /// </summary>
        /// <value>The requirement help.</value>
        string RequirementHelp { get; }

        /// <summary>
        /// Gets the platforms.
        /// </summary>
        /// <value>The platforms.</value>
        PlatformType[] ActivePlatforms { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        bool IsActive { get; }

        /// <summary>
        /// Checks the requirement.
        /// </summary>
        /// <returns>CheckRequirementResult.</returns>
        CheckRequirementResult CheckRequirement();

        /// <summary>
        /// Checks the requirement asynchronous.
        /// </summary>
        /// <returns>Task&lt;CheckRequirementResult&gt;.</returns>
        Task<CheckRequirementResult> CheckRequirementAsync();
    }

    /// <summary>
    /// Interface IEnvironmentRequirementCollection
    /// </summary>
    public interface IEnvironmentRequirementCollection : IEnumerable<IEnvironmentRequirement>
    {
        /// <summary>
        /// Gets the requirements.
        /// </summary>
        /// <value>The requirements.</value>
        IEnvironmentRequirement[] Requirements { get; }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        int Count { get; }

        /// <summary>
        /// Adds the requirement.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        void Require(IEnvironmentRequirement requirement);

        /// <summary>
        /// Removes the requirement.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        void Dismiss(IEnvironmentRequirement requirement);

        /// <summary>
        /// Clears this instance.
        /// </summary>
        void Clear();

        /// <summary>
        /// Requires the environment.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="minVersion">The minimum version.</param>
        /// <param name="maxVersion">The maximum version.</param>
        void RequireEnvironment<T>(Version minVersion = null, Version maxVersion = null) where T : class, IEnvironmentService;

        /// <summary>
        /// Requires the service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void RequireService<T>() where T : class, IService;

        /// <summary>
        /// Checks the requirement.
        /// </summary>
        /// <returns>CheckRequirementResult.</returns>
        CheckRequirementResult CheckRequirement();

        /// <summary>
        /// Checks the requirement asynchronous.
        /// </summary>
        /// <returns>Task&lt;CheckRequirementResult&gt;.</returns>
        Task<CheckRequirementResult> CheckRequirementAsync();
    }
    #endregion

    #region Some Template Environment Requirement
    /// <summary>
    /// Class AbstractEnvironmentRequirement.
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.IEnvironmentRequirement" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.IEnvironmentRequirement" />
    public abstract class AbstractEnvironmentRequirement : IEnvironmentRequirement
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the requirement description.
        /// Quick description
        /// </summary>
        /// <value>The requirement description.</value>
        public abstract string RequirementDescription {get;}

        /// <summary>
        /// Gets the requirement help.
        /// description + help text
        /// </summary>
        /// <value>The requirement help.</value>
        public abstract string RequirementHelp { get; }

        /// <summary>
        /// Gets the platforms.        
        /// </summary>
        /// <value>The platforms.</value>
        public PlatformType[] ActivePlatforms { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        public bool IsActive => ActivePlatforms == null || ActivePlatforms.Any(platform => PlatformUtils.IsPlatform(platform));

        /// <summary>
        /// Checks the requirement.
        /// </summary>
        /// <returns>CheckRequirementResult.</returns>
        public abstract CheckRequirementResult CheckRequirement();

        /// <summary>
        /// Checks the requirement asynchronous.
        /// </summary>
        /// <returns>Task&lt;CheckRequirementResult&gt;.</returns>
        public async Task<CheckRequirementResult> CheckRequirementAsync()
        {
            return await Task.Run(() => CheckRequirement());
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return RequirementDescription;
        }
    }


    /// <summary>
    /// Class ServiceRequirement.
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.IEnvironmentRequirement" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.IEnvironmentRequirement" />
    public class ServiceRequirement<T> : AbstractEnvironmentRequirement
        where T : class, IService
    {
        /// <summary>
        /// Gets or sets the type of the invalid result.
        /// Overall, what is returned when environment monitoring fails?
        /// </summary>
        /// <value>The type of the invalid result.</value>
        public CheckRequirementResultType InvalidResultType { get; set; } = CheckRequirementResultType.Dissatisfied;              

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get
            {
                var service = ServiceProvider.GetService<T>();

                return $"{service?.ServiceName ?? typeof(T).Name}";
            }
        }

        /// <summary>
        /// Gets the requirement description.
        /// Quick description
        /// </summary>
        /// <value>The requirement description.</value>
        public override string RequirementDescription
        {
            get
            {
                var service = ServiceProvider.GetService<T>();

                return BuildAlignText("Require Service:", Name);
            }
        }

        /// <summary>
        /// Gets the requirement help.
        /// description + help text
        /// </summary>
        /// <value>The requirement help.</value>
        public override string RequirementHelp 
        {
            get
            {
                return $"You need implement and import a {typeof(T).FullName}";
            }
        }

        /// <summary>
        /// Builds the align text.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="name">The name.</param>
        /// <returns>System.String.</returns>
        protected string BuildAlignText(string tag, string name)
        {
            int length = tag.Length < 31 ? 31 - tag.Length : 0;

            return $"{tag}{Formatter.BuildBlankText(length)}{name}";
        }

        /// <summary>
        /// Gens the error.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>CheckRequirementResult.</returns>
        protected CheckRequirementResult GenError(string errorMessage)
        {
            return new CheckRequirementResult(InvalidResultType, errorMessage);
        }


        /// <summary>
        /// Checks the requirement.
        /// </summary>
        /// <returns>CheckRequirementResult.</returns>
        public override CheckRequirementResult CheckRequirement()
        {
            T service = ServiceProvider.GetService<T>();

            if (service == null)
            {
                return GenError($"Service {typeof(T).FullName} is not exists.");
            }

            if(!service.IsAvailable)
            {
                return GenError($"Service {typeof(T).FullName} is not available.");
            }

            return new CheckRequirementResult(CheckRequirementResultType.CompletelySatisfied, "");
        }              

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return (int)typeof(T).FullName.GetDeterministicHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is ServiceRequirement<T> && obj.GetHashCode() == GetHashCode();
        }
    }

    /// <summary>
    /// Class EnvironmentServiceRequirement.
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.IEnvironmentRequirement" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.IEnvironmentRequirement" />
    public class EnvironmentServiceRequirement<T> : ServiceRequirement<T>
        where T : class, IEnvironmentService
    {
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
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get
            {
                var service = ServiceProvider.GetService<T>();

                string name = "";

                if (service != null)
                {
                    name = service.Name;
                }
                else
                {
                    name = typeof(T).Name;
                    if (name.iEndsWith("EnvironmentService"))
                    {
                        name = name.Left(name.Length - "EnvironmentService".Length);
                    }
                }

                return name;
            }
        }

        /// <summary>
        /// Gets the requirement description.
        /// </summary>
        /// <value>The requirement description.</value>
        public override string RequirementDescription
        {
            get
            {
                var name = Name;                

                if (MinVersion != null && MaxVersion != null)
                {
                    return BuildAlignText("Require Environment:", $"{name}, version range is [{MinVersion}, {MaxVersion}]");
                }
                else if(MinVersion != null)
                {
                    return BuildAlignText("Require Environment:", $"{name}, minimum version is {MinVersion}");
                }
                else if(MaxVersion != null)
                {
                    return BuildAlignText("Require Environment:", $"{name}, highest version is {MaxVersion}");
                }

                return BuildAlignText("Require Environment:", $"{name}");
            }
        }


        /// <summary>
        /// Gets the requirement help.
        /// description + help text
        /// </summary>
        /// <value>The requirement help.</value>
        public override string RequirementHelp
        {
            get
            {
                var desc = RequirementDescription;

                var service = ServiceProvider.GetService<T>();

                if(service != null)
                {
                    return $"{desc}{Environment.NewLine}{service.GetHelp()}";
                }
                else
                {
                    return desc;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentServiceRequirement{T}" /> class.
        /// If there is no need to limit the version range, just do not provide the corresponding parameters.
        /// </summary>
        /// <param name="minVersion">The minimum version.</param>
        /// <param name="maxVersion">The maximum version.</param>
        public EnvironmentServiceRequirement(Version minVersion = null, Version maxVersion = null)
        {
            MinVersion = minVersion;
            MaxVersion = maxVersion;

            if (minVersion != null && MaxVersion != null)
            {
                Logger.Ensure<ArgumentException>(minVersion <= MaxVersion, "EnvironmentServiceRequirement construct error, argument error: minVersion must <= maxVersion.");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentServiceRequirement{T}"/> class.
        /// </summary>
        /// <param name="minVersion">The minimum version.</param>
        /// <param name="maxVersion">The maximum version.</param>
        public EnvironmentServiceRequirement(string minVersion, string maxVersion = null) :
            this(minVersion.IsNotNullOrEmpty()?Version.Parse(minVersion):null, maxVersion.IsNotNullOrEmpty() ? Version.Parse(maxVersion):null)
        {
        }
        
        /// <summary>
        /// Checks the requirement.
        /// </summary>
        /// <returns>CheckRequirementResult.</returns>
        public override CheckRequirementResult CheckRequirement()
        {
            var service = ServiceProvider.GetService<T>();

            if (service == null)
            {
                return GenError($"Service {typeof(T).FullName} is not exists.");
            }

            if (!service.IsAvailable)
            {
                return GenError($"Service {typeof(T).FullName} is not available, see also:{Environment.NewLine}{service.GetHelp()}");
            }

            // need check version
            string message = null;
            if (!CheckVersion(service, out message))
            {
                return GenError(message);
            }

            return new CheckRequirementResult(CheckRequirementResultType.CompletelySatisfied, "");
        }

        /// <summary>
        /// Checks the version.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="message">The message.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        protected bool CheckVersion(T service, out string message)
        {
            return CheckVersion(service.Version, service.Name, out message);   
        }

        /// <summary>
        /// Checks the version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="name">The name.</param>
        /// <param name="message">The message.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        protected bool CheckVersion(Version version, string name, out string message)
        {
            message = "";

            if (MinVersion != null || MaxVersion != null)
            {
                if (version == null)
                {
                    message = $"Not version info for {name}";
                    return false;
                }

                if (MinVersion != null)
                {
                    if (version < MinVersion)
                    {
                        message = $"Current {name}'s version {version} is below the minimum version requirement {MinVersion}";
                        return false;
                    }
                }

                if (MaxVersion != null)
                {
                    if (version > MaxVersion)
                    {
                        message = $"Current {name}'s version {version} is higher than the highest version requirement {MinVersion}";
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            int code = base.GetHashCode();

            if(MinVersion != null)
            {
                code = HashCodes.Combine(code, (int)MinVersion.ToString().GetDeterministicHashCode());
            }

            if(MaxVersion != null)
            {
                code = HashCodes.Combine(code, (int)MaxVersion.ToString().GetDeterministicHashCode());
            }

            return code;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if(obj is EnvironmentServiceRequirement<T> e)
            {
                return MinVersion == e.MinVersion && MaxVersion == e.MaxVersion;
            }

            return false;
        }
    }
    #endregion

    #region Template Collections
    /// <summary>
    /// Class EnvironmentRequirementCollection.
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.IEnvironmentRequirementCollection" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.IEnvironmentRequirementCollection" />
    class EnvironmentRequirementCollection : IEnvironmentRequirementCollection
    {
        /// <summary>
        /// The requirements core
        /// </summary>
        readonly List<IEnvironmentRequirement> RequirementsCore = new List<IEnvironmentRequirement>();

        /// <summary>
        /// Gets the requirements.
        /// </summary>
        /// <value>The requirements.</value>
        public IEnvironmentRequirement[] Requirements => RequirementsCore.ToArray();

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count => RequirementsCore.Count;

        /// <summary>
        /// Adds the requirement.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        public void Require(IEnvironmentRequirement requirement)
        {
            Logger.Ensure<ArgumentNullException>(requirement != null, nameof(requirement));

            RequirementsCore.Add(requirement);
        }

        /// <summary>
        /// Requires the environment.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="minVersion">The minimum version.</param>
        /// <param name="maxVersion">The maximum version.</param>
        public void RequireEnvironment<T>(Version minVersion = null, Version maxVersion = null) where T : class, IEnvironmentService
        {
            Require(new EnvironmentServiceRequirement<T>(minVersion, maxVersion));
        }

        /// <summary>
        /// Requires the service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RequireService<T>() where T : class, IService
        {
            Require(new ServiceRequirement<T>());
        }

        /// <summary>
        /// Removes the requirement.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        public void Dismiss(IEnvironmentRequirement requirement)
        {
            Logger.Ensure<ArgumentNullException>(requirement != null, nameof(requirement));

            RequirementsCore.Remove(requirement);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<IEnvironmentRequirement> GetEnumerator()
        {
            return RequirementsCore.GetEnumerator();
        }


        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return RequirementsCore.GetEnumerator();
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            RequirementsCore.Clear();
        }

        /// <summary>
        /// Checks the requirement.
        /// </summary>
        /// <returns>CheckRequirementResult.</returns>
        public CheckRequirementResult CheckRequirement()
        {
            StringBuilder stringBuilder = new StringBuilder();

            CheckRequirementResultType Result = CheckRequirementResultType.CompletelySatisfied;

            foreach (var i in RequirementsCore)
            {
                if(!i.IsActive)
                {
                    continue;
                }

                var result = i.CheckRequirement();

                if (result.Result == CheckRequirementResultType.Dissatisfied)
                {
                    return result;
                }

                if (Result < result.Result)
                {
                    Result = result.Result;
                    stringBuilder.AppendLine(result.ErrorMessage);
                }
            }

            return new CheckRequirementResult(Result, stringBuilder.ToString());
        }

        /// <summary>
        /// Check requirement as an asynchronous operation.
        /// </summary>
        /// <returns>A Task&lt;CheckRequirementResult&gt; representing the asynchronous operation.</returns>
        public async Task<CheckRequirementResult> CheckRequirementAsync()
        {
            return await Task.Run(() => CheckRequirement());
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            if(Count >0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach(var i in RequirementsCore)
                {
                    stringBuilder.AppendLine(i.RequirementDescription);
                }

                return $"{Count} Requirements{Environment.NewLine}{stringBuilder.ToString()}";
            }
            else
            {
                return $"{Count} Requirements";
            }            
        }
    }
    #endregion

    /// <summary>
    /// Class EnvironmentRequirementExtensions.
    /// </summary>
    public static class EnvironmentRequirementExtensions
    {
        /// <summary>
        /// Gets the active platforms.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        /// <returns>System.String.</returns>
        public static string GetActivePlatforms(this IEnvironmentRequirement requirement)
        {
            return GetPlatforms(requirement?.ActivePlatforms);
        }

        /// <summary>
        /// Gets the platforms.
        /// </summary>
        /// <param name="platforms">The platforms.</param>
        /// <returns>System.String.</returns>
        public static string GetPlatforms(PlatformType[] platforms)
        {
            if (platforms != null && platforms.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var platform in platforms)
                {
                    if (platform != platforms.First())
                    {
                        sb.Append(',');
                    }

                    sb.Append(platform.ToString());
                }

                return sb.ToString();
            }

            return "All Platforms";
        }
    }
}
