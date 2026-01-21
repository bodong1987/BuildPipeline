using BuildPipeline.Core.BuilderFramework;
using BuildPipeline.Core.BuilderFramework.Reflection;
using BuildPipeline.Core.BuilderFramework.Reflection.Requirements;
using BuildPipeline.Core.CommandLine;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Services;
using BuildPipeline.Core.Utils;
using PropertyModels.ComponentModel.DataAnnotations;
using PropertyModels.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace BuildPipeline.Plugins.CPPExample
{
    // This attribute is used to mark that a class should be regarded as a BuildTask class.
    // When a class is regarded as a BuildTask class,
    // then all its public and static functions that meet the requirements will be recognized as a specific BuildTask.
    internal class BuildCppExampleAttribute : AbstractBuildTaskExportAttribute
    {
        internal const string CppExample = nameof(CppExample);

        public BuildCppExampleAttribute() : base(CppExample) 
        {
        }

        public override bool Accept(IBuildContext context)
        {
            if (context is BuildContext bc)
            {
                // check another options

            }
            else
            {
                // this export attribute can only support this BuildContext
                return false;
            }

            // only valid on windows
            return PlatformUtils.IsWindows();
        }
    }

    [Flags]
    public enum EBuildConfiguration
    {
        Debug = 1<<0,
        Release = 1 <<1,
    }

    [Flags]
    public enum EBuildArchType
    {
        x86 = 1 << 0,
        x64 = 1 << 1,
    }

    // An IBuildContext represents the basic configuration of a pipeline.
    // Each IBuildContext can be used to create an IBuildPipeline.
    // Each IBuildPipeline can have an indefinite number of IBuildTasks.
    internal class BuildContext : AbstractBuildContext
    {
        // show the BuildContext Name
        public override string Name => BuildCppExampleAttribute.CppExample;

        // Project path 
        // Option Attribtue allows the properties of this IBuildContext to be serialized into command line parameters or read from command line parameters.
        [Option("project", Required = true)]
        // This Attribute is used by GUI tools. With this Attribute, this attribute in the GUI tool will automatically provide a path selection button.
        [PathBrowsable(PathBrowsableType.File, Filters = "Visual C++ Project File(*.vcxproj)|*.vcxproj")]
        // This attribute marks this attribute as an important attribute and should be refreshed when it changes.
        // For example, when your IBuildContext has an attribute that is the target platform,
        // the specific task lists used by different target platforms are different, so you need Refresh.
        // Properties like this should be marked [ConditionProperty]
        [ConditionProperty]
        public string ProjectPath { get; set; } = AppFramework.GetPublishApplicationDirectory().JoinPath(@"../../../ExampleProjects\ExampleCPPProject/ExampleCPPProject.vcxproj");

        // Verify the validity of IBuildContext.
        // The lack of some key configurations will inevitably cause the pipeline to fail.
        // In this case, you can directly report an error when creating the pipeline IBuildPipe or before executing it.
        public override ValidationResult CheckValidation()
        {
            if(!ProjectPath.IsFileExists())
            {
                return BuildInvalidSettingResult("Project Path is not exists.");
            }

            return base.CheckValidation();
        }
    }

    /*
     *   This attribute [BuildFacotyr(...)] marks this class as an IBuildContextFactory, which has the ability to create IBuildContext
     *   The default implementation provided by the framework can be obtained from a template base class implementation such as AbstractBuildContextFactory, 
     *   so that we do not have to implement all interfaces of IBuildContextFactory
    */
    [BuildFactory(BuildCppExampleAttribute.CppExample)]     
    internal class SetupBuildContextFactory : AbstractBuildContextFactory<BuildContext> 
    {
        public override bool Accept(object accessToken)
        {
            // this test CPPExample Project is Visual C++ project... so we make this Factory only valid on windows...
            if(!PlatformUtils.IsWindows())
            {
                return false;
            }

            return base.Accept(accessToken);
        }
    }

    // Classes marked with the AbstractBuildTaskExportAttribute derived class will be automatically recognized by the framework.
    // The framework will automatically analyze the functions in this class that meet the requirements and automatically create them into a task.
    [BuildCppExample]
    internal static class BuildVCProjectUtils
    {
        #region Build
        public class BuildProjectOptions : BuildTaskOptions
        {
            [Option("configuration")]
            public EBuildConfiguration BuildConfigurations { get; set; } = EBuildConfiguration.Release;

            [Option("arch")]
            public EBuildArchType ArchTypes { get; set; } = EBuildArchType.x64;

            [Option("clean")]
            public bool NeedClean { get; set; } = true;

            [Option("vs")]
            public VisualStudioType VSType { get; set; } = VisualStudioType.VS_2022;
        }

        // mark this function is a build task
        [BuildTaskMethod("Build", 10, TaskDescription = "Build C++ Project by MSBuild")]
        [RequireService<IExternalProcessService>]   // mark this task need external process service available
        [RequireService<IVisualStudioEnvironmentService>] // mark this task need visual studio, because the demo project is created by vs2022
        [RequireEnvironment<IMSBuildEnvironmentService>] // mark this task need MSbuild.
        public static async Task<int> BuildProjectAsync(BuildContext context, IExcecuteObserver observer, CancellationTokenSource cancellationTokenSource, BuildProjectOptions options)
        {
            // get external process service
            IExternalProcessService service = ServiceProvider.GetService<IExternalProcessService>();

            if (service == null)
            {
                observer.LogError("No IExternalProcessService");
                return -1;
            }

            // check is visual studio available
            IVisualStudioEnvironmentService vsService = ServiceProvider.GetService<IVisualStudioEnvironmentService>();

            IVisualStudioInstallation vsInstallation = null;

            if (options.VSType == VisualStudioType.VS_Unknown ||
                options.VSType == VisualStudioType.VS_Lastest ||
                options.VSType == VisualStudioType.VS_ForMac)
            {
                vsInstallation = vsService.PreferInstallation;
            }
            else
            {
                vsInstallation = Enumerable.Reverse(vsService.Installations).ToList().Find(x => x.VSType == options.VSType);
            }

            if (vsInstallation == null)
            {
                observer.LogError($"Failed find specify visual studio type, target={options.VSType}");
                return -1;
            }

            // get msbuild info.
            IMSBuildEnvironmentService msbuildService = ServiceProvider.GetService<IMSBuildEnvironmentService>();
            if (msbuildService == null)
            {
                observer.LogError("IMSBuildEnvironmentService is not available.");
                return -1;
            }

            string msBuildPath = msbuildService.InstallationPath.JoinPath("MSBuild");

            foreach(var config in options.BuildConfigurations.GetUniqueFlags())
            {
                foreach(var archType in options.ArchTypes.GetUniqueFlags())
                {
                    // build it
                    int result = await BuildProjectAsyncInternal(
                        service,
                        observer,
                        cancellationTokenSource,
                        msBuildPath,
                        context.ProjectPath,
                        config,
                        archType,
                        true
                        );

                    if(result != 0)
                    {
                        return -1;
                    }
                }
            }

            return 0;
        }

        private static async Task<int> BuildProjectAsyncInternal(
            IExternalProcessService service,
            IExcecuteObserver observer,
            CancellationTokenSource cancellationTokenSource,
            string msBuild, 
            string projectPath, 
            EBuildConfiguration configuration, 
            EBuildArchType buildArchType, 
            bool needClean
            )
        {
            string Target = needClean ? "Rebuild" : "Build";
            string Platform = buildArchType.ToString();

            string command = $" \"{projectPath}\" /target:{Target} /p:Configuration={configuration} /p:Platform={Platform} ";

            int result = await service.StartAsync(msBuild, command, true, observer, cancellationTokenSource);

            if(result != 0 || (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested))
            {
                observer.LogError($"Failed build {projectPath} with {configuration}/{buildArchType}...");
                return -1;
            }

            return 0;
        }
        #endregion

        #region Archive        
        public class ArchiveOptions : BuildTaskOptions
        {
            [Option("target", Required = false)]
            [PathBrowsable(PathBrowsableType.File, SaveMode = true, Filters = "7z file(*.7z)|*.7z|zip files(*.zip)|*.zip")]
            public string TargetFile { get; set; } = "";

            [Option("configuration")]
            public EBuildConfiguration BuildConfigurations { get; set; } = EBuildConfiguration.Release;

            [Option("arch")]
            public EBuildArchType ArchTypes { get; set; } = EBuildArchType.x64;

            [Option("password", HelpText = "Archive file's password", Required = false)]
            [DisplayName("Archive file's password")]
            public string Password { get; set; } = null;

            public override ValidationResult CheckValidation()
            {
                if(TargetFile.IsNullOrEmpty())
                {
                    return new ValidationResult("If you want to archive output file. please provide TargetFile.");
                }

                if(!TargetFile.GetFileName().IsValidFileName())
                {
                    return new ValidationResult($"{TargetFile} is not an valid file path");
                }

                return base.CheckValidation();
            }
        }

        private static string PrepareCompressTargetFile(string projectPath, EBuildConfiguration configuration, EBuildArchType archType)
        {
            string dir = projectPath.GetDirectoryPath();

            string archName = archType == EBuildArchType.x86 ? "" : "x64";

            string TargetDir = dir.JoinPath(archName, configuration.ToString());
            string fileName = projectPath.GetFileNameWithoutExtension();

            string NewFileName = $"{fileName}_{configuration}_{archType}.exe";            
            string SourceFile = TargetDir.JoinPath(fileName + ".exe");

            if(SourceFile.IsFileExists())
            {
                string TempDir = AppFramework.ApplicationTempDirectory.JoinPath("BuildPipelineTemp");

                if(!TempDir.IsDirectoryExists())
                {
                    Directory.CreateDirectory(TempDir);
                }
                string CompressTargetFile = TempDir.JoinPath(NewFileName + ".exe");

                File.Copy(SourceFile, CompressTargetFile, true);

                return CompressTargetFile;
            }

            return null;
        }

        [BuildTaskMethod("Archive", 11, TaskDescription = "Compress output file to a achive file")]
        [RequireService<ICompressService>]
        public static async Task<int> ArchiveAsync(BuildContext context, IExcecuteObserver observer, CancellationTokenSource cancellationTokenSource, ArchiveOptions options)
        {
            if(options.TargetFile.IsNullOrEmpty())
            {
                return 0;
            }

            ICompressService compressService = ServiceProvider.GetService<ICompressService>();

            if(compressService == null)
            {
                observer.LogError("No ICompressService");
                return -1;
            }

            List<string> Files = new List<string>();

            foreach(var config in options.BuildConfigurations.GetUniqueFlags())
            {
                foreach(var arch in options.ArchTypes.GetUniqueFlags())
                {
                    string path = PrepareCompressTargetFile(context.ProjectPath, config, arch);

                    if (path.IsFileExists())
                    {
                        Files.Add(path);
                    }                    
                }
            }

            return await Task.Run(() =>
            {
                if(!compressService.Compress(Files.ToArray(), options.TargetFile, null, cancellationTokenSource, options.Password))
                {
                    observer.LogError($"Failed archive build result to {options.TargetFile}");
                    return -1;
                }

                observer.Log("Archive finished.");

                return 0;
            });
        }
        #endregion
    }
}