using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using PropertyModels.Extensions;

namespace BuildPipeline.Core.Services.EnvironmentServices
{
    /// <summary>
    /// Class PerlEnvironmentService.
    /// Implements the <see cref="AbstractEnvironmentService" />
    /// Implements the <see cref="IPerlEnvironmentService" />
    /// </summary>
    /// <seealso cref="AbstractEnvironmentService" />
    /// <seealso cref="IPerlEnvironmentService" />
    [Export]
    internal class PerlEnvironmentService : AbstractEnvironmentService, IPerlEnvironmentService
    {
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Name => "Perl";

        /// <summary>
        /// Gets the perl.
        /// </summary>
        /// <value>The perl.</value>
        public string Perl { get; private set; }

        /// <summary>
        /// Gets the size of the point.
        /// 0 = unknown
        /// 4 = 32bit
        /// 8 = 64bit
        /// </summary>
        /// <value>The size of the point.</value>
        public string PointSize { get; private set; } = "0";

        /// <summary>
        /// Checks this instance.
        /// </summary>
        public override bool CheckService()
        {
            string perlPath = EnvironmentUtils.SearchExecutableFile("perl");

            if (!perlPath.IsFileExists())
            {
                Logger.LogError("Failed find Perl in System PATH Environment variables.");
                return false;
            }

            InstallationPath = perlPath.GetDirectoryPath();
            Perl = perlPath;

            CheckPerlVersion();
            CheckArchType();

            return true;
        }

        private void CheckPerlVersion()
        {
            string outputText = ExternalProcessUtils.InvokeAndGetOutput(Perl, " -e \"print $^V\"");
            if (!outputText.iStartsWith("v"))
            {
                return;
            }

            outputText = outputText.Substring(1).Trim();

            if (Version.TryParse(outputText, out var version))
            {
                Version = version;
            }
            else
            {
                Logger.LogError("Failed parse perl version from string: {0}", outputText);
            }
        }

        private void CheckArchType()
        {
            string outputText = ExternalProcessUtils.InvokeAndGetOutput(Perl, " -V:ptrsize");
            if (!outputText.iStartsWith("ptrsize"))
            {
                return;
            }

            int i1 = outputText.IndexOf('\'');
            int i2 = outputText.LastIndexOf('\'');

            if (i1 != -1 && i2 != -1 && i1 < i2)
            {
                PointSize = outputText.Substring(i1 + 1, i2 - i1 - 1);
            }
        }

        public override string GetHelp()
        {
            return "Please install perl first. try:\n" +
                "https://www.activestate.com/products/perl/ or \n" +
                "https://strawberryperl.com/";
        }
    }
}
