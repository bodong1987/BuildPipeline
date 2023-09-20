using BuildPipeline.Core.Framework;

namespace BuildPipeline.Core.Services
{
    #region Service Interfaces

    #region Base Interface
    /// <summary>
    /// Interface IEnvironmentService
    /// Implements the <see cref="IService" />
    /// </summary>
    /// <seealso cref="IService" />
    public interface IEnvironmentService : IService
    {
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        string Name { get; }

        /// <summary>
        /// Gets the installation path.
        /// </summary>
        /// <value>The installation path.</value>
        string InstallationPath { get; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        Version Version { get; }

        /// <summary>
        /// Gets the help.
        /// </summary>
        /// <returns>System.String.</returns>
        string GetHelp();

        /// <summary>
        /// Asynchronous checking environment
        /// </summary>
        /// <returns>Task.</returns>
        Task CheckAsync();

        /// <summary>
        /// Checks this instance.
        /// Synchronous check environment
        /// </summary>
        void Check();
    }
    #endregion

    #region Python
    /// <summary>
    /// Interface IPythonEnviromentService
    /// Implements the <see cref="BuildPipeline.Core.Services.IEnvironmentService" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Services.IEnvironmentService" />
    public interface IPythonEnvironmentService : IEnvironmentService
    {
        /// <summary>
        /// Gets the python3.
        /// </summary>
        /// <value>The python3.</value>
        string Python3 { get; }
    }
    #endregion

    #region Perl
    /// <summary>
    /// Interface IPerlEnvironmentService
    /// Implements the <see cref="BuildPipeline.Core.Services.IEnvironmentService" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Services.IEnvironmentService" />
    public interface IPerlEnvironmentService : IEnvironmentService
    {
        /// <summary>
        /// Gets the perl.
        /// </summary>
        /// <value>The perl.</value>
        string Perl { get; }

        /// <summary>
        /// Gets the size of the point.
        /// 0 = unknown
        /// 4 = 32bit
        /// 8 = 64bit
        /// </summary>
        /// <value>The size of the point.</value>
        string PointSize { get; }
    }
    #endregion

    #region Visual Studio
    /// <summary>
    /// Enum VisualStudioType
    /// </summary>
    public enum VisualStudioType
    {
        /// <summary>
        /// The vs unknown
        /// </summary>
        VS_Unknown,

        /// <summary>
        /// The vs 2010
        /// </summary>
        VS_2010,
        /// <summary>
        /// The vs 2012
        /// </summary>
        VS_2012,
        /// <summary>
        /// The vs 2013
        /// </summary>
        VS_2013,
        /// <summary>
        /// The vs 2015
        /// </summary>
        VS_2015,

        /// <summary>
        /// The vs 2017
        /// </summary>
        VS_2017,

        /// <summary>
        /// The vs 2019
        /// </summary>
        VS_2019,

        /// <summary>
        /// The vs 2022
        /// </summary>
        VS_2022,

        /// <summary>
        /// The vs lastest
        /// </summary>
        VS_Lastest,

        /// <summary>
        /// The vs for mac
        /// </summary>
        VS_ForMac,
    }

    /// <summary>
    /// Enum VSArchitectureType
    /// </summary>
    public enum VSArchitectureType
    {
        /// <summary>
        /// The win64
        /// </summary>
        Win64,
        /// <summary>
        /// The win32
        /// </summary>
        Win32,
        /// <summary>
        /// The ar M64
        /// </summary>
        ARM64,
        /// <summary>
        /// The arm
        /// </summary>
        ARM,

        /// <summary>
        /// The i a64
        /// </summary>
        IA64
    }


    /// <summary>
    /// Interface IVisualStudioInstallation
    /// </summary>
    public interface IVisualStudioInstallation
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the unique identifier.
        /// </summary>
        /// <value>The unique identifier.</value>
        string Guid { get; }

        /// <summary>
        /// Gets the installation.
        /// </summary>
        /// <value>The installation.</value>
        string Installation { get; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        Version Version { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is visual CPP installed.
        /// </summary>
        /// <value><c>true</c> if this instance is visual CPP installed; otherwise, <c>false</c>.</value>
        bool IsVisualCPPInstalled { get; }

        /// <summary>
        /// Gets the type of the vs.
        /// </summary>
        /// <value>The type of the vs.</value>
        VisualStudioType VSType { get; }

        /// <summary>
        /// Gets the name of the friendly.
        /// </summary>
        /// <value>The name of the friendly.</value>
        string FriendlyName { get; }
    }

    /// <summary>
    /// Interface IVisualStudioEnvironmentService
    /// Implements the <see cref="BuildPipeline.Core.Services.IEnvironmentService" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Services.IEnvironmentService" />
    public interface IVisualStudioEnvironmentService : IEnvironmentService
    {
        /// <summary>
        /// Gets the installations.
        /// </summary>
        /// <value>The installations.</value>
        IVisualStudioInstallation[] Installations { get; }

        /// <summary>
        /// Gets the prefer installation.
        /// </summary>
        /// <value>The prefer installation.</value>
        IVisualStudioInstallation PreferInstallation { get; }
    }
    #endregion

    #region MSBuild
    /// <summary>
    /// Interface IMSBuildInstallation
    /// </summary>
    public interface IMSBuildInstallation
    {
        /// <summary>
        /// Gets the ms build.
        /// </summary>
        /// <value>The ms build.</value>
        string MsBuild { get; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        Version Version { get; }
    }

    /// <summary>
    /// Interface IMSBuildEnvironmentService
    /// Implements the <see cref="BuildPipeline.Core.Services.IEnvironmentService" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Services.IEnvironmentService" />
    public interface IMSBuildEnvironmentService : IEnvironmentService
    {
        /// <summary>
        /// Gets the installations.
        /// </summary>
        /// <value>The installations.</value>
        IMSBuildInstallation[] Installations { get; }
    }
    #endregion

    #region Java
    /// <summary>
    /// Interface IJavaEnvironmentService
    /// Implements the <see cref="BuildPipeline.Core.Services.IEnvironmentService" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Services.IEnvironmentService" />
    public interface IJavaEnvironmentService : IEnvironmentService
    {
        /// <summary>
        /// Gets the java path.
        /// </summary>
        /// <value>The java path.</value>
        string JavaPath { get; }

        /// <summary>
        /// Gets the java c path.
        /// </summary>
        /// <value>The java c path.</value>
        string JavaCPath { get; }

        /// <summary>
        /// Gets the java version.
        /// </summary>
        /// <value>The java version.</value>
        Version JavaVersion { get; }

        /// <summary>
        /// Gets the java c version.
        /// </summary>
        /// <value>The java c version.</value>
        Version JavaCVersion { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is java SDK installation.
        /// </summary>
        /// <value><c>true</c> if this instance is java SDK installation; otherwise, <c>false</c>.</value>
        bool IsJavaSDKInstallation { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is environment variable defined.
        /// </summary>
        /// <value><c>true</c> if this instance is environment variable defined; otherwise, <c>false</c>.</value>
        bool IsEnvironmentVariableDefined { get; }

        /// <summary>
        /// Gets the name of the environment variable.
        /// </summary>
        /// <value>The name of the environment variable.</value>
        string EnvironmentVariableName { get; }

        /// <summary>
        /// Gets the environment java path.
        /// </summary>
        /// <value>The environment java path.</value>
        string EnvironmentJavaPath { get; }
    }
    #endregion

    #region XCode
    /// <summary>
    /// Interface IXCodeEnvironmentService
    /// Implements the <see cref="BuildPipeline.Core.Services.IEnvironmentService" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Services.IEnvironmentService" />
    public interface IXCodeEnvironmentService : IEnvironmentService
    {
        /// <summary>
        /// Gets the x code build path.
        /// </summary>
        /// <value>The x code build path.</value>
        string XCodeBuildPath { get; }

        /// <summary>
        /// Gets the x code build version.
        /// </summary>
        /// <value>The x code build version.</value>
        Version XCodeBuildVersion { get; }
    }
    #endregion

    #region Windows SDK
    /// <summary>
    /// Interface IWindowsSDKEnvironmentService
    /// Implements the <see cref="BuildPipeline.Core.Services.IEnvironmentService" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Services.IEnvironmentService" />
    public interface IWindowsSDKEnvironmentService : IEnvironmentService
    {
        /// <summary>
        /// Gets all versions.
        /// </summary>
        /// <value>All versions.</value>
        Version[] AllVersions { get; }
    }
    #endregion

    #region .NET Framework
    /// <summary>
    /// Interface IDotnetFrameworkEnvironmentService
    /// .NET Framework 环境信息
    /// Implements the <see cref="BuildPipeline.Core.Services.IEnvironmentService" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Services.IEnvironmentService" />
    public interface IDotnetFrameworkEnvironmentService : IEnvironmentService
    {
        /// <summary>
        /// Gets all versions.
        /// </summary>
        /// <value>All versions.</value>
        Version[] AllVersions { get; }
    }
    #endregion

    #region .NET
    /// <summary>
    /// Interface IDotnetSDKInformation
    /// </summary>
    public interface IDotnetSDKInformation
    {
        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        Version Version { get; }

        /// <summary>
        /// Gets the installation.
        /// </summary>
        /// <value>The installation.</value>
        string Installation { get; }
    }

    /// <summary>
    /// Interface IDotnetRuntimeInformation
    /// </summary>
    public interface IDotnetRuntimeInformation
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }
        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        Version Version { get; }
        /// <summary>
        /// Gets the installation.
        /// </summary>
        /// <value>The installation.</value>
        string Installation { get; }
    }

    /// <summary>
    /// Interface IDotnetEnvironmentService
    /// .NET environment detection service
    /// Implements the <see cref="BuildPipeline.Core.Services.IEnvironmentService" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Services.IEnvironmentService" />
    public interface IDotnetEnvironmentService : IEnvironmentService
    {
        /// <summary>
        /// Gets the dotnet path.
        /// </summary>
        /// <value>The dotnet path.</value>
        string DotnetPath { get; }

        /// <summary>
        /// Gets the sd ks.
        /// </summary>
        /// <value>The sd ks.</value>
        IDotnetSDKInformation[] SDKs { get; }

        /// <summary>
        /// Gets the runtimes.
        /// </summary>
        /// <value>The runtimes.</value>
        IDotnetRuntimeInformation[] Runtimes { get; }
    }
    #endregion

    #region Subversion
    /// <summary>
    /// Interface ISubversionEnvironmentService
    /// Implements the <see cref="BuildPipeline.Core.Services.IEnvironmentService" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Services.IEnvironmentService" />
    public interface ISubversionEnvironmentService : IEnvironmentService
    {
        /// <summary>
        /// Gets the SVN path.
        /// </summary>
        /// <value>The SVN path.</value>
        string SvnPath { get; }
    }
    #endregion

    #region Git
    /// <summary>
    /// Interface IGitEnvironmentService
    /// Implements the <see cref="BuildPipeline.Core.Services.IEnvironmentService" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Services.IEnvironmentService" />
    public interface IGitEnvironmentService : IEnvironmentService
    {
        /// <summary>
        /// Gets the git path.
        /// </summary>
        /// <value>The git path.</value>
        string GitPath { get; }

        /// <summary>
        /// Gets the git LFS path.
        /// </summary>
        /// <value>The git LFS path.</value>
        string GitLFSPath { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is git LFS installed.
        /// </summary>
        /// <value><c>true</c> if this instance is git LFS installed; otherwise, <c>false</c>.</value>
        bool IsGitLFSInstalled { get; }

        /// <summary>
        /// Gets the git version.
        /// </summary>
        /// <value>The git version.</value>
        Version GitVersion { get; }

        /// <summary>
        /// Gets the git LFS version.
        /// </summary>
        /// <value>The git LFS version.</value>
        Version GitLFSVersion { get; }
    }
    #endregion

    #region Android SDK
    /// <summary>
    /// Interface IAndroidSDKInstallation
    /// </summary>
    public interface IAndroidSDKInstallation
    {
        /// <summary>
        /// Gets a value indicating whether this instance is found by environment variable.
        /// </summary>
        /// <value><c>true</c> if this instance is found by environment variable; otherwise, <c>false</c>.</value>
        bool IsFoundByEnvironmentVariable { get; }

        /// <summary>
        /// Gets the name of the environment.
        /// </summary>
        /// <value>The name of the environment.</value>
        string EnvironmentName { get; }

        /// <summary>
        /// Gets the installation.
        /// </summary>
        /// <value>The installation.</value>
        string Installation { get; }

        /// <summary>
        /// Gets the API levels.
        /// </summary>
        /// <value>The API levels.</value>
        string[] APILevels { get; }

        /// <summary>
        /// Gets the build tools.
        /// </summary>
        /// <value>The build tools.</value>
        string[] BuildTools { get; }
    }

    /// <summary>
    /// Interface IAndroidSDKEnvironmentService
    /// Implements the <see cref="BuildPipeline.Core.Services.IEnvironmentService" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Services.IEnvironmentService" />
    public interface IAndroidSDKEnvironmentService : IEnvironmentService
    {
        /// <summary>
        /// Gets all installations.
        /// </summary>
        /// <value>All installations.</value>
        IAndroidSDKInstallation[] AllInstallations { get; }
    }
    #endregion

    #region Android NDK
    /// <summary>
    /// Interface IAndroidNDKInstallation
    /// </summary>
    public interface IAndroidNDKInstallation
    {
        /// <summary>
        /// Gets a value indicating whether this instance is found by environment variable.
        /// </summary>
        /// <value><c>true</c> if this instance is found by environment variable; otherwise, <c>false</c>.</value>
        bool IsFoundByEnvironmentVariable { get; }

        /// <summary>
        /// Gets the name of the environment.
        /// </summary>
        /// <value>The name of the environment.</value>
        string EnvironmentName { get; }

        /// <summary>
        /// Gets the installation.
        /// </summary>
        /// <value>The installation.</value>
        string Installation { get; }
    }

    /// <summary>
    /// Interface IAndroidNDKEnvironmentService
    /// Implements the <see cref="BuildPipeline.Core.Services.IEnvironmentService" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Services.IEnvironmentService" />
    public interface IAndroidNDKEnvironmentService : IEnvironmentService
    {
        /// <summary>
        /// Gets all installations.
        /// </summary>
        /// <value>All installations.</value>
        public IAndroidNDKInstallation[] AllInstallations { get; }
    }
    #endregion

    #region CMake
    /// <summary>
    /// Interface ICMakeEnvironmentService
    /// Implements the <see cref="BuildPipeline.Core.Services.IEnvironmentService" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Services.IEnvironmentService" />
    public interface ICMakeEnvironmentService : IEnvironmentService
    {
        /// <summary>
        /// Gets the c make.
        /// </summary>
        /// <value>The c make.</value>
        string CMake { get; }

        /// <summary>
        /// Gets the c make target command line.
        /// </summary>
        /// <param name="installation">The installation.</param>
        /// <param name="architectureType">Type of the architecture.</param>
        /// <returns>System.String.</returns>
        string GetCMakeTargetCommandLine(IVisualStudioInstallation installation, VSArchitectureType architectureType);

        /// <summary>
        /// Gets the name of the platform.
        /// </summary>
        /// <param name="installation">The installation.</param>
        /// <param name="architectureType">Type of the architecture.</param>
        /// <returns>System.String.</returns>
        string GetPlatformName(IVisualStudioInstallation installation, VSArchitectureType architectureType);
    }
    #endregion

    #region SevenZip
    /// <summary>
    /// Interface ISevenZipEnvironmentService
    /// Implements the <see cref="BuildPipeline.Core.Services.IEnvironmentService" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Services.IEnvironmentService" />
    public interface ISevenZipEnvironmentService : IEnvironmentService
    {
        /// <summary>
        /// Gets the seven zip path.
        /// </summary>
        /// <value>The seven zip path.</value>
        string SevenZipPath { get; }
    }
    #endregion

    #endregion

    #region Implement Templates
    /// <summary>
    /// Class AbstractEnviromentService.
    /// Implements the <see cref="AbstractImportable" />
    /// Implements the <see cref="BuildPipeline.Core.Services.IEnvironmentService" />
    /// </summary>
    /// <seealso cref="AbstractImportable" />
    /// <seealso cref="BuildPipeline.Core.Services.IEnvironmentService" />
    public abstract class AbstractEnvironmentService : AbstractService, IEnvironmentService
    {
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>The name of the service.</value>
        public override string ServiceName => Name;
               
        /// <summary>
        /// Gets the installation path.
        /// </summary>
        /// <value>The installation path.</value>
        public string InstallationPath { get; protected set; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        public Version Version { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this instance is available.
        /// </summary>
        /// <value><c>true</c> if this instance is available; otherwise, <c>false</c>.</value>
        public override bool IsAvailable
        {
            get
            {
                if(State == ServiceStateType.Created)
                {
                    Check();
                }

                return State == ServiceStateType.Available;
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>Task.</returns>
        public Task CheckAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                Check();
            });
        }

        /// <summary>
        /// Checks this instance.
        /// </summary>
        public void Check()
        {
            bool Result = false;

            try
            {
                State = ServiceStateType.Checking;

                Result = CheckService();
            }
            finally
            {
                State = Result ? ServiceStateType.Available : ServiceStateType.UnAvaliable;
            }
        }

        /// <summary>
        /// Checks this instance.
        /// </summary>
        public abstract bool CheckService();

        /// <summary>
        /// Gets the help.
        /// </summary>
        /// <returns>System.String.</returns>
        public abstract string GetHelp();

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return ServiceName;
        }
    }
    #endregion
}
