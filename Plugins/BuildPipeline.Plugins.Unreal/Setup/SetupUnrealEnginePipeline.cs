using BuildPipeline.Core.BuilderFramework;
using BuildPipeline.Core.BuilderFramework.Reflection;
using BuildPipeline.Core.CommandLine;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Services;
using PropertyModels.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildPipeline.Plugins.Unreal.Setup
{
    internal class DownloadDependenciesOptions : BuildTaskOptions
    {
        [Option('t', "threads", HelpText = "Use N threads when downloading new files")]
        public int ThreadsCount { get; set; } = 5;

        [Option("proxy", HelpText = "Sets the HTTP proxy address and credentials (user:password@url)")]
        public string ProxyServer { get; set; } = "";
    }


    [SetupUnrealEngineTask]
    internal static class SetupUnrealEnginePipeline
    {
        [BuildTaskMethod("Download", 10, TaskDescription = "Download git dependencies")]
        [RequireEnvironment<IDotnetEnvironmentService>]
        public static async Task<int> DownloadDependencies(SetupBuildContext context, IExcecuteObserver observer, CancellationTokenSource cancellationTokenSource, DownloadDependenciesOptions options)
        {
            IExternalProcessService service = ServiceProvider.GetService<IExternalProcessService>();

            if (service == null)
            {
                observer.LogError($"Failed get service:{typeof(IExternalProcessService).Name}");
                return -1;
            }

            string GitDependenciesPath = context.EngineDirectory.JoinPath("Engine\\Binaries\\DotNET\\GitDependencies");

            if(AppFramework.IsWindows)
            {
                GitDependenciesPath = Path.Combine(GitDependenciesPath, "win-x64/GitDependencies.exe");
            }
            else if(AppFramework.IsMacOS)
            {
                GitDependenciesPath = Path.Combine(GitDependenciesPath, "osx-x64/GitDependencies");
            }
            else
            {
                GitDependenciesPath = Path.Combine(GitDependenciesPath, "linux-x64/GitDependencies");
            }

            if(!GitDependenciesPath.IsFileExists())
            {
                observer.LogError($"{GitDependenciesPath} is not exists.");
                return -1;
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"--threads={options.ThreadsCount} ");
            if(options.ProxyServer.IsNotNullOrEmpty())
            {
                stringBuilder.Append($"--proxy={options.ProxyServer} ");
            }

            return await service.StartAsync(GitDependenciesPath, stringBuilder.ToString(), true, observer, cancellationTokenSource);
        }
    }
}
