using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using PropertyModels.Extensions;

namespace BuildPipeline.Core.Services.EnvironmentServices
{
    /// <summary>
    /// Class GitEnvironmentService.
    /// Implements the <see cref="AbstractEnvironmentService" />
    /// Implements the <see cref="IGitEnvironmentService" />
    /// </summary>
    /// <seealso cref="AbstractEnvironmentService" />
    /// <seealso cref="IGitEnvironmentService" />
    [Export]
    internal class GitEnvironmentService : AbstractEnvironmentService, IGitEnvironmentService
    {
        /// <summary>
        /// Gets the git path.
        /// </summary>
        /// <value>The git path.</value>
        public string GitPath { get; private set; }

        /// <summary>
        /// Gets the git LFS path.
        /// </summary>
        /// <value>The git LFS path.</value>
        public string GitLFSPath { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is git LFS installed.
        /// </summary>
        /// <value><c>true</c> if this instance is git LFS installed; otherwise, <c>false</c>.</value>
        public bool IsGitLFSInstalled => GitLFSPath.IsNotNullOrEmpty();

        /// <summary>
        /// Gets the git version.
        /// </summary>
        /// <value>The git version.</value>
        public Version GitVersion => Version;

        /// <summary>
        /// Gets the git LFS version.
        /// </summary>
        /// <value>The git LFS version.</value>
        public Version GitLFSVersion { get; private set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Name => "Git";

        /// <summary>
        /// Gets the help.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string GetHelp()
        {
            return "Please install git, see also:\n" +
                "  git: https://git-scm.com/\n" +
                "  git-lfs: https://git-lfs.com/\n";
        }

        /// <summary>
        /// Checks this instance.
        /// </summary>
        public override bool CheckService()
        {
            CheckGit();
            CheckGitLFS();

            return InstallationPath.IsNotNullOrEmpty();
        }

        #region Git
        /// <summary>
        /// Checks the git.
        /// </summary>
        private void CheckGit()
        {
            var path = EnvironmentUtils.SearchExecutableFile("git");

            if (!path.IsFileExists())
            {
                return;
            }

            InstallationPath = path.GetDirectoryPath();
            GitPath = path;
            CheckGitVersion();
        }

        /// <summary>
        /// Checks the git version.
        /// </summary>
        private void CheckGitVersion()
        {
            Logger.Assert(GitPath.IsNotNullOrEmpty());

            var versionText = ExternalProcessUtils.InvokeAndGetOutput(GitPath, " --version");

            versionText = versionText.Trim();

            if (!versionText.iStartsWith("git version"))
            {
                return;
            }

            var version = versionText.Substring("git version".Length);
            int index = version.IndexOf(ch =>
            {
                return !char.IsDigit(ch) && ch != ' ' && ch != '.';
            });

            if (index == -1)
            {
                return;
            }

            version = version.Substring(0, index).Trim(' ', '\t', '.');
            if (Version.TryParse(version, out var v))
            {
                Version = v;
            }
        }
        #endregion

        #region Git-LFS
        /// <summary>
        /// Checks the git LFS.
        /// </summary>
        private void CheckGitLFS()
        {
            var path = EnvironmentUtils.SearchExecutableFile("git-lfs");
            if (!path.IsFileExists())
            {
                return;
            }

            GitLFSPath = path;

            CheckGitLFSVersion();
        }

        /// <summary>
        /// Checks the git LFS version.
        /// </summary>
        private void CheckGitLFSVersion()
        {
            Logger.Assert(GitLFSPath.IsNotNullOrEmpty());

            var version = ExternalProcessUtils.InvokeAndGetOutput(GitLFSPath, "--version");

            version = version.Trim();

            if (!version.iStartsWith("git-lfs/"))
            {
                return;
            }

            version = version.Substring("git-lfs/".Length);
            int index = version.IndexOf(' ');
            if (index != -1)
            {
                version = version.Substring(0, index).Trim();
            }

            if (Version.TryParse(version, out var v))
            {
                GitLFSVersion = v;
            }
        }
        #endregion
    }
}
