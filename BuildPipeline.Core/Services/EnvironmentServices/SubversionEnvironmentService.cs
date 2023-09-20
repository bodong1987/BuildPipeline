using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using PropertyModels.Extensions;

namespace BuildPipeline.Core.Services.EnvironmentServices
{
    /// <summary>
    /// Class SubversionEnvironmentService.
    /// Implements the <see cref="AbstractEnvironmentService" />
    /// Implements the <see cref="ISubversionEnvironmentService" />
    /// </summary>
    /// <seealso cref="AbstractEnvironmentService" />
    /// <seealso cref="ISubversionEnvironmentService" />
    [Export]
    internal class SubversionEnvironmentService : AbstractEnvironmentService, ISubversionEnvironmentService
    {
        /// <summary>
        /// Gets the SVN path.
        /// </summary>
        /// <value>The SVN path.</value>
        public string SvnPath { get; private set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Name => "Subversion";

        /// <summary>
        /// Gets the help.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string GetHelp()
        {
            return "Please install Subversion(svn), see also:\n" +
                "    [All platform] https://subversion.apache.org/\n" +
                "    [Windows, install with command line tools] https://tortoisesvn.net/\n" +
                "    [Macos, install with homebrew] https://brew.sh/";
        }

        /// <summary>
        /// Checks this instance.
        /// </summary>
        public override bool CheckService()
        {
            var path = EnvironmentUtils.SearchExecutableFile("svn");
            if (!path.IsFileExists())
            {
                return false;
            }

            InstallationPath = path.GetDirectoryPath();
            SvnPath = path;

            // check version
            string version = ExternalProcessUtils.InvokeAndGetOutput(SvnPath, " --version --quiet");
            version = version.Trim();

            if (version.IsNotNullOrEmpty() && Version.TryParse(version, out var v))
            {
                Version = v;
            }

            return true;
        }
    }
}
