using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using PropertyModels.Extensions;

namespace BuildPipeline.Core.Services.EnvironmentServices
{
    /// <summary>
    /// Class DotnetSDKInformation.
    /// Implements the <see cref="IDotnetSDKInformation" />
    /// </summary>
    /// <seealso cref="IDotnetSDKInformation" />
    internal class DotnetSDKInformation : IDotnetSDKInformation
    {
        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        public Version Version { get; set; }

        /// <summary>
        /// Gets the installation.
        /// </summary>
        /// <value>The installation.</value>
        public string Installation { get; set; }
    }

    /// <summary>
    /// Class DotnetRuntimeInformation.
    /// Implements the <see cref="IDotnetRuntimeInformation" />
    /// </summary>
    /// <seealso cref="IDotnetRuntimeInformation" />
    internal class DotnetRuntimeInformation : IDotnetRuntimeInformation
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        public Version Version { get; set; }

        /// <summary>
        /// Gets the installation.
        /// </summary>
        /// <value>The installation.</value>
        public string Installation { get; set; }
    }

    /// <summary>
    /// Class DotnetEnvironmentService.
    /// Implements the <see cref="AbstractEnvironmentService" />
    /// Implements the <see cref="IDotnetEnvironmentService" />
    /// </summary>
    /// <seealso cref="AbstractEnvironmentService" />
    /// <seealso cref="IDotnetEnvironmentService" />
    [Export]
    internal class DotnetEnvironmentService : AbstractEnvironmentService, IDotnetEnvironmentService
    {
        /// <summary>
        /// The sd ks core
        /// </summary>
        readonly List<DotnetSDKInformation> SDKsCore = new List<DotnetSDKInformation>();

        /// <summary>
        /// The runtimes core
        /// </summary>
        readonly List<DotnetRuntimeInformation> RuntimesCore = new List<DotnetRuntimeInformation>();

        /// <summary>
        /// Gets the sd ks.
        /// </summary>
        /// <value>The sd ks.</value>
        public IDotnetSDKInformation[] SDKs => SDKsCore.ToArray();

        /// <summary>
        /// Gets the runtimes.
        /// </summary>
        /// <value>The runtimes.</value>
        public IDotnetRuntimeInformation[] Runtimes => RuntimesCore.ToArray();

        /// <summary>
        /// Gets the dotnet path.
        /// </summary>
        /// <value>The dotnet path.</value>
        public string DotnetPath { get; private set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Name => ".NET";

        /// <summary>
        /// Checks this instance.
        /// </summary>
        public override bool CheckService()
        {
            var path = EnvironmentUtils.SearchExecutableFile("dotnet");

            if (!path.IsFileExists())
            {
                return false; 
            }

            InstallationPath = path.GetDirectoryPath();
            DotnetPath = path;

            CheckDotnetSDKs();
            CheckDotnetRuntimes();

            Version = RuntimesCore.FirstOrDefault()?.Version;

            return true;
        }

        /// <summary>
        /// Checks the dotnet sd ks.
        /// </summary>
        private void CheckDotnetSDKs()
        {
            var text = ExternalProcessUtils.InvokeAndGetOutput(DotnetPath, " --list-sdks");

            var lines = text.Split('\n');

            foreach (var i in lines)
            {
                var line = i.Trim();

                if (line.IsNotNullOrEmpty())
                {
                    ParseSDK(line);
                }
            }

            SDKsCore.Sort((x, y) =>
            {
                return Comparer<Version>.Default.Compare(y.Version, x.Version);
            });
        }

        /// <summary>
        /// Parses the SDK.
        /// </summary>
        /// <param name="line">The line.</param>
        private void ParseSDK(string line)
        {
            DotnetSDKInformation sdk = new DotnetSDKInformation();

            var splitIndex = line.IndexOf(' ');
            var version = line.Substring(0, splitIndex);

            if (Version.TryParse(version, out var v))
            {
                sdk.Version = v;
            }

            int i1 = line.IndexOf('[', splitIndex + 1);
            int i2 = line.LastIndexOf(']');

            if (i1 != -1 && i2 != -1 && i1 < i2)
            {
                sdk.Installation = line.Substring(i1 + 1, i2 - i1 - 1);
            }

            SDKsCore.Add(sdk);
        }

        /// <summary>
        /// Checks the dotnet runtimes.
        /// </summary>
        private void CheckDotnetRuntimes()
        {
            var text = ExternalProcessUtils.InvokeAndGetOutput(DotnetPath, " --list-runtimes");

            var lines = text.Split('\n');

            foreach (var i in lines)
            {
                var line = i.Trim();

                if (line.IsNotNullOrEmpty())
                {
                    ParseRuntime(line);
                }
            }

            RuntimesCore.Sort((x, y) =>
            {
                return Comparer<Version>.Default.Compare(y.Version, x.Version);
            });
        }

        /// <summary>
        /// Parses the runtime.
        /// </summary>
        /// <param name="line">The line.</param>
        private void ParseRuntime(string line)
        {
            var lines = line.Split(' ');

            if (lines.Length >= 3)
            {
                DotnetRuntimeInformation runtime = new DotnetRuntimeInformation();

                runtime.Name = lines[0].Trim();

                if (Version.TryParse(lines[1], out var v))
                {
                    runtime.Version = v;
                }

                int i1 = line.IndexOf('[');
                int i2 = line.LastIndexOf(']');

                if (i1 != -1 && i2 != -1 && i1 < i2)
                {
                    runtime.Installation = line.Substring(i1 + 1, i2 - i1 - 1);
                }

                RuntimesCore.Add(runtime);
            }
        }

        /// <summary>
        /// Gets the help.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string GetHelp()
        {
            return "You must install .NET SDK/Runtime first, see also:\n" +
                "https://dotnet.microsoft.com/en-us/download/visual-studio-sdks";
        }
    }
}
