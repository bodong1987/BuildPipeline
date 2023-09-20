using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using PropertyModels.Extensions;

namespace BuildPipeline.Core.Services.EnvironmentServices
{
    /// <summary>
    /// Class PythonEnviromentService.
    /// Implements the <see cref="AbstractEnvironmentService" />
    /// Implements the <see cref="IPythonEnvironmentService" />
    /// </summary>
    /// <seealso cref="AbstractEnvironmentService" />
    /// <seealso cref="IPythonEnvironmentService" />
    [Export]
    internal class PythonEnviromentService : AbstractEnvironmentService, IPythonEnvironmentService
    {
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Name => "Python3";

        /// <summary>
        /// Gets the python3.
        /// </summary>
        /// <value>The python3.</value>
        public string Python3 { get; private set; }

        private string FindPython3ExecuteFilePath()
        {
            string appName = "python";

            if (OperatingSystem.IsMacOS())
            {
                appName = "python3";
            }
            else if (OperatingSystem.IsWindows())
            {
                appName = "python.exe";
            }

            string python3Dir = Environment.GetEnvironmentVariable("PYTHON_3");
            if (python3Dir.IsDirectoryExists())
            {
                string path = python3Dir.JoinPath(appName);

                if (path.IsFileExists())
                {
                    return path;
                }
                else
                {
                    Logger.LogWarning("Failed find {0} under [PYTHON_3](1)", appName, python3Dir);
                }
            }

            string pythonPath = EnvironmentUtils.SearchExecutableFile(appName);

            if (!pythonPath.IsFileExists())
            {
                Logger.LogWarning("Failed find {0} in Enviroment pathes...", appName);

                return null;
            }

            return pythonPath;
        }

        private void CheckPythonVersion(string pythonPath)
        {
            if (!Python3.IsFileExists())
            {
                return;
            }

            string outputText = ExternalProcessUtils.InvokeAndGetOutput(Python3, " --version");

            if (!outputText.iStartsWith("Python"))
            {
                return;
            }

            var versionText = outputText.Substring("Python".Length);
            var trimText = versionText.Trim();

            if (Version.TryParse(trimText, out var version))
            {
                Version = version;
            }
            else
            {
                Logger.LogError("Failed parse python version from string: {0}", outputText);
            }
        }

        public override bool CheckService()
        {
            string pythonPath = FindPython3ExecuteFilePath();

            if (pythonPath.IsNullOrEmpty())
            {
                Logger.LogError("Failed find python3 execute file.");
                return false;
            }

            InstallationPath = pythonPath.GetDirectoryPath();
            Python3 = pythonPath;

            CheckPythonVersion(Python3);

            return true;
        }

        /// <summary>
        /// Gets the help.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string GetHelp()
        {
            return "You need to install python3 on your computer, " +
                "and make sure that the python3 installation directory is added to the system 'PATH' environment variable, " +
                "or specified through the PYTHON_3 environment variable." +
                "\nsee also:https://www.python.org/downloads/";
        }
    }
}
