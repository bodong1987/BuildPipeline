using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.CommandLine;
using PropertyModels.Extensions;
using System.Reflection;
using BuildPipeline.Core.Extensions.IO;

namespace BuildPipeline.Core.BuilderFramework
{
    /// <summary>
    /// Interface IBuildContextFactoryOptions
    /// </summary>
    public interface IBuildContextFactoryOptions
    {
        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>The arguments.</value>
        string[] Arguments { get; }
    }

    /// <summary>
    /// Interface IBuildContextFactory
    /// Implements the <see cref="IImportable" />
    /// </summary>
    /// <seealso cref="IImportable" />
    public interface IBuildContextFactory : IImportable
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the name of the plugin.
        /// </summary>
        /// <value>The name of the plugin.</value>
        string PluginName { get; }

        /// <summary>
        /// Gets the source assembly.
        /// </summary>
        /// <value>The source assembly.</value>
        Assembly SourceAssembly { get; }

        /// <summary>
        /// Creates the context.
        /// Build context based on command arguments and other options
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>IBuildContext.</returns>
        IBuildContext CreateContext(IBuildContextFactoryOptions options);

        /// <summary>
        /// Creates new context.
        /// Create a new, empty context. Usually such contexts cannot be used directly.
        /// </summary>
        /// <returns>IBuildContext.</returns>
        IBuildContext NewContext();
    }

    /// <summary>
    /// Class BuildFactoryAttribute.
    /// Implements the <see cref="Attribute" />
    /// </summary>
    /// <seealso cref="Attribute" />
    public class BuildFactoryAttribute : ExportAttribute
    {
        /// <summary>
        /// The name
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildFactoryAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public BuildFactoryAttribute(string name)
        {
            Name = name;
        }
    }


    /// <summary>
    /// Class AbstractBuildContextFactory.
    /// Implements the <see cref="AbstractImportable" />
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.IBuildContextFactory" />
    /// </summary>
    /// <seealso cref="AbstractImportable" />
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.IBuildContextFactory" />
    public abstract class AbstractBuildContextFactory<TBuildContextType> : AbstractImportable, IBuildContextFactory
        where TBuildContextType : class, IBuildContext, new()
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractBuildContextFactory{T}"/> class.
        /// </summary>
        public AbstractBuildContextFactory()
        {
            var attr = GetType().GetAnyCustomAttribute<BuildFactoryAttribute>();
            Name = attr?.Name;
            PluginName = GetPluginName();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractBuildContextFactory{T}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public AbstractBuildContextFactory(string name)
        {
            Name = name;
            PluginName = GetPluginName();
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public virtual string Name { get; protected set; }

        /// <summary>
        /// Gets the name of the plugin.
        /// </summary>
        /// <value>The name of the plugin.</value>
        public virtual string PluginName { get; protected set; }

        /// <summary>
        /// Gets the source assembly.
        /// </summary>
        /// <value>The source assembly.</value>
        public virtual Assembly SourceAssembly => GetType().Assembly;

        private string GetPluginName()
        {
            var fullName = GetType().Assembly.Location.GetFileNameWithoutExtension();

            if(fullName.StartsWith("BuildPipeline.Plugins."))
            {
                return fullName.Substring("BuildPipeline.Plugins.".Length);
            }

            return fullName;
        }

        /// <summary>
        /// Creates the context.
        /// Build context based on command arguments and other options
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>IBuildContext.</returns>
        public virtual IBuildContext CreateContext(IBuildContextFactoryOptions options)
        {
            if(options == null)
            {
                return NewContext();
            }

            TBuildContextType context = new TBuildContextType();

            var parser = new Parser();

            var result = parser.Parse(options.Arguments, context);

            if (result.Result == ParserResultType.NotParsed)
            {
                throw new ArgumentException(result.ErrorMessage);
            }

            if (context != null)
            {
                context.Factory = this;
                context.Arguments = options.Arguments;
            }

            return context;
        }

        /// <summary>
        /// Creates new context.
        /// Create a new, empty context.Usually such contexts cannot be used directly.
        /// </summary>
        /// <returns>IBuildContext.</returns>
        public virtual IBuildContext NewContext()
        {
            var context = new TBuildContextType();
            context.Factory = this;

            return context;
        }
    }
}
