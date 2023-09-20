# **BuildPipeline**
&nbsp;&nbsp;&nbsp;&nbsp;Complex and large-scale projects usually require many steps from compilation to release, and these steps are usually completed by scripts. Different projects usually choose different scripts or development languages, and sometimes use different solutions in the same project. This brings about a problem that there is a lack of unified standards and specifications to manage these steps, which will ultimately lead to maintenance difficulty.  
&nbsp;&nbsp;&nbsp;&nbsp;Therefore, I implemented a basic library based on .net 6.0 to provide a unified task model for the output pipeline. Based on this core library, all tasks you develop will have unified standards and are easy to understand, use and debug.At the same time, I also made a GUI tool based on Avalonia. This tool is equivalent to the shell of all the core libraries and plug-ins you make. It has the ability to run across platforms and can help you build an archive environment on any client machine and help You detect whether the current environment meets the requirements and provide necessary help if the conditions are not met.  
The main functions and features of this library are:
* Cross-platform support, as long as it supports .NET 6, it can run  
* Easy to integrate, if you want to implement your own pipeline, you only need to create your own C# project and implement several necessary classes.  
* Easy to use, the entry corresponding to each independent pipeline task is just a static function  
* Unified command line parameter specifications, the framework can automatically provide command line parameters for each of your tasks, so that tasks can be executed in sequence or independently.  
* Multi-process framework, making operations such as canceling tasks very fast  

The features of the gui tool are :  
* Based on Avalonia, so any client machine that supports Avalonia can run. It has been tested on Windows, MacOS, and Ubuntu  
* The tool performs tasks by calling sub-processes, so the task itself does not affect the stability and performance of the tool itself.  
* The tool can provide a visual interface for configuring various parameters of the entire pipeline and pipeline tasks, and automatically provide you with the command line parameters needed to execute the task.  
* Supports serialization, you can save and re-read the configured pipeline configuration file at any time  

## How To Build
* Make sure your development environment supports .NET 6.0 so you can install Visual Studio 20222, Visual Studio Mac or other  
* Make sure that Python3 is installed and that its executable is in the system environment variable PATH.  
* Just open BuildPipeline.sln, Build it.

## How To Use
* The first method: directly reference the NUGET package:  
    https://www.nuget.org/packages/bodong.BuildPipeline.Core/  
    if you want to use Python script support, reference this too: https://www.nuget.org/packages/bodong.BuildPipeline.Core.PythonScripts/  

* The second method: clone or Fork this project, use source code directly.

## Start Tutorial

### Create Your Plugin Project
The framework uses a plug-in architecture, so for the framework to discover your pipeline and tasks, you only need to create a new C# project according to the specifications. This project should be named BuildPipeline.Plugins.{YourPluginName}.At the same time, you also need to add a reference to the BuildPipeline.Core project, or directly reference the corresponding NUGET package.  
After the new creation is completed, you need to manually edit the csproj and add the following PostBuild command to the project configuration to ensure that the python script will copy your compilation results to the correct directory during compilation.
```xml
    <Target Name="PostBuild" AfterTargets="PostBuildEvent">
        <Exec Command="python $(SolutionDir).\Scripts\post_build.py --outputType=$(OutputType) --targetDir=$(TargetDir) --projectDir=$(ProjectDir) --projectName=$(ProjectName) --configurationName=$(ConfigurationName)" Condition="$([MSBuild]::IsOSPlatform('Windows'))" />
        <Exec Command="python3 $(SolutionDir).\Scripts\post_build.py --outputType=$(OutputType) --targetDir=$(TargetDir) --projectDir=$(ProjectDir) --projectName=$(ProjectName) --configurationName=$(ConfigurationName)" Condition="$([MSBuild]::IsOSPlatform('MacOS'))" />
    </Target>
```

Please refer to the examples in the Plugins directory for more information.

### Implement Your Build Context Factory
&nbsp;&nbsp;&nbsp;&nbsp;A pipeline must have its own IBuildContext, and the entrance to a pipeline is IBuildContextFactory. Therefore, these interfaces need to be implemented by us. Of course, most of the time we should not implement them directly from these interfaces. The framework provides some template base classes to help you quickly implement these interfaces.  
&nbsp;&nbsp;&nbsp;&nbsp;An IBuildContextFactory represents a pipeline type. By marking it with the BuildFactoryAttribute, the framework will automatically identify these classes.
```C#
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
```

### Implement Your Build Context
&nbsp;&nbsp;&nbsp;&nbsp;An IBuildContext represents the main context of a pipeline, which generally should include the target project path, key configurations, etc. For example, if you are creating a new pipeline for building game installation packages for your Unity3d game project or Unreal Engine game project, then the IBuildContext of this pipeline should contain the following content: engine installation path, project path, installation package target platform, etc.  
&nbsp;&nbsp;&nbsp;&nbsp;Here is a simple pipeline example for compiling a VC++ project,Some concepts are also explained in the code comments, please pay attention:  

```C#
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
```

### Implement Your Build Tasks Export Attribute
In order to implement the BuildTask class, you should implement an AbstractBuildTaskExportAttribute for your pipeline. This export tag is used to mark a class that needs to be analyzed.
```C#
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
```
At this point, you can start to implement specific task functions.
```C#
// Classes marked with the AbstractBuildTaskExportAttribute derived class will be automatically recognized by the framework.
// The framework will automatically analyze the functions in this class that meet the requirements and automatically create them into a task.
[BuildCppExample]
internal static class BuildVCProjectUtils
{
    // 
}
```
Next, you only need to add static functions to this class according to the specification.  

### Implement Your Build Task 
Static functions that can be used to automatically create tasks should be public and should always exist with the following signature:  
```C#
public static async Task<int> ${YourMethodName}(${YourContext} context, IExcecuteObserver observer, CancellationTokenSource cancellationTokenSource);
public static async Task<int> ${YourMethodName}(${YourContext} context, IExcecuteObserver observer, CancellationTokenSource cancellationTokenSource, ${OptionalOptions} options);
```
* This static method should always be public.
* This static method should always be async and should always return Task<int>. Returning 0 indicates success, other values ​​indicate failure.
* This static method should have three or four parameters, of which the fourth parameter is optional. If it is to be used, the type of this parameter should implement IBuildTaskOptions.
* The first parameter must be a type that implements IBuildContext, usually your own BuildContext, to avoid converting from IBuildContext.
* The second parameter and the third parameter are fixed and can only be of these two types. They are used to output logs and execute cancellation policies respectively.
* These static functions should be marked with BuildTaskMethodAttribute and provide more details, such as whether to allow failure, etc.  
* These static functions can also be marked with custom RequireServiceAttribute and RequireEnvironmentAttribute, so that the framework will obtain the necessary conditions for the correct execution of this task, including but not limited to development environment, etc.

Here's a practical example:
```C#
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
        vsInstallation = vsService.Installations.Reverse().ToList().Find(x => x.VSType == options.VSType);
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
```
At this point, you have completed the development of a simple pipeline task. This task can be executed sequentially in the pipeline or independently.

## Command Line Execute
![build-Example](./Docs/Images/Build-Example.png)
After creating pipeline tasks, each task has its own command line, and the GUI tool will help you provide this command line information. Just pass these command lines to BuildPipeline.Proc, and the framework will automatically execute the task function you implement.
![build-Example2](./Docs/Images/Build-Example2.png)

The first two parameters of the command line are fixed, the first is the plug-in name, and the second is the pipeline name. The remaining parameters are the command line parameters of BuildContext and BuildTaskOptions. The default values ​​will be automatically ignored. Of course, you can also use the complete command, such as:
```
CPPExample CppExample  --project G:\BuildPipeline\ExampleProjects\ExampleCPPProject\ExampleCPPProject.vcxproj --tasks Build --excludes  --mode Common   --configuration Release --arch x86 x64 --clean True --vs VS_2022  
```

## GUI Tools
The entrance to the GUI tool is BuildPipeline.GUI.Desktop.exe under Windows. For other platforms, you can use dotnet BuildPipeline.GUI.Desktop.dll to run it. The GUI tool provides a graphical interface for creating pipelines, selecting pipeline tasks, and configuring tasks. Parameters and so on.   
In addition, the GUI tool also provides environment detection functions, command line help viewing, etc. The pipeline configuration can be serialized through functions such as opening and saving.
![command-help](./Docs/Images/command-help.png)

## Advanced Topics
### Built-in services
The framework has a large number of built-in services, which are mainly used to detect the system environment. You can customize these services at any time. The current detection capabilities include:
```
.NET
.NET Framework
7zip
Android NDK
Android SDK
CMake
Java
Git
Subversion
MSBuild
Python
Perl
Visual Studio
Windows SDK(Windows Only)
```
So you can use these services directly to add constraints to your tasks.  
If these constraints don't meet your needs, you can implement your own service or replace the default implementation. To implement your own service or replace the built-in service with your own service, you only need to implement the docking interface in your plug-in and derive it from AbstractService or AbstractEnvironmentService, and then override the ImportPriority attribute so that it returns a value higher than the built-in one. Replacement can be achieved by serving a larger value. Note that this custom service needs to be marked with ExportAttribute.
for example:
```C#
 [Export]
 internal class MyCustomXCodeEnvironmentService : AbstractEnvironmentService, IXCodeEnvironmentService
 {
    /// <summary>
    /// Gets the import priority.
    /// </summary>
    /// <value>The import priority.</value>
    public override int ImportPriority => base.ImportPriority + 10000;
     /// <summary>
     /// Gets the description.
     /// </summary>
     /// <value>The description.</value>
     public override string Name => "XCode";

     /// ignore some codes here ...

     /// <summary>
     /// Gets the help.
     /// </summary>
     /// <returns>System.String.</returns>
     public override string GetHelp()
     {
         return "You must install Xcode and start it at least once so that Xcode can successfully install related dependencies, see also:\n" +
             "    https://developer.apple.com/download/all/";
     }
 }
```

### Localization
The localization file is located in the Publish/Release/net6.0/assets/localization directory and is named after CultureInfo. This is a simple json dictionary file. You only need to add the text you want to localize into it.  
The localization file of the plug-in should not be placed here, but should be placed in the plugins/{PluginName}/assets/localication directory of the corresponding plug-in.

### Python Support
To enable your plugin to support Python scripts, you need to do two things:
* Add a dependency for your plugin on the project or NUGET package BuildPipeline.Core.PythonScripts  
* Add a class marked with [AssemblyLoaded] in your plug-in. When the framework finds these types when loading the plug-in, it will automatically create an object of this class, so you can add additional initialization code in the constructor of this class, so We can register the Assembly of PythonScripts into the framework here.  

for example :
```C#
using BuildPipeline.Core.BuilderFramework;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.PythonScripts;
using BuildPipeline.Core.Utils;

namespace BuildPipeline.Plugins.PythonTests
{
    public class BuildContext : AbstractBuildContext
    {
        public const string ContractName = "PythonTests";

        public override string Name => ContractName;
    }

    [BuildFactory(BuildContext.ContractName)]
    internal class SetupBuildContextFactory : AbstractBuildContextFactory<BuildContext>
    {
        public override bool Accept(object accessToken)
        {
            return base.Accept(accessToken);
        }
    }

    // this class will create an instance when this Assembly loaded
    [AssemblyLoaded]
    class AssemblyLoadedNotifier
    {
        public AssemblyLoadedNotifier() 
        {
            // register python plugin's assembly to extensibility framework
            ExtensibilityFramework.AddPart(typeof(PythonEngine).Assembly);
        }
    }
}
```
This is the simplest pipeline plug-in using python. Only IBuildContextFactory and IBuildContext are provided. The rest is handled in python.  
You must add your task script file under this path:  
![python-task-location](./Docs/Images/python-task-location.png)
{ApplicationPath}/plugins/{YourPluginName}/scripts  
Start your python script file name with task_, such as task_test.py.
This is an example about python script task:
```python
import sys
import os

_my_dir = os.path.dirname(os.path.realpath(__file__))
_lib_dir = os.path.join(_my_dir, "../../../scripts")
sys.path.append(_lib_dir)

from baselib import task_definitions

task_def = task_definitions.Task("test", 1024, "Python Test Task", Condition="")
task_def.add_argument("boolVar", "Boolean Variable", True, Require = True)
task_def.add_argument("intVar", "Int32 Variable", 1024, Require = False)
task_def.add_argument("strVar", "String Variable", "String Text", Require = False)
task_def.add_argument("listVar", "List Variable", ["list value0", "list value1"], Require=False)

if __name__ == "__main__" :
    args = task_def.parse_command()
    print("--boolVar = " + str(args.boolVar))
    print("--intVar = " + str(args.intVar))
    print("--strVar = " + str(args.strVar))
    print("--listVar = " + str(args.listVar))
```
**Warning: The object name of the task definition must be task_def, because the framework obtains the task description through this name**  
When the framework recognizes this py file as a task, it will execute the code once through IronPython and obtain task information, parameters, etc. through task_def. For performance reasons during actual execution, an external Python service will be used to execute the script instead of through IronPython.  
![python-task](./Docs/Images/python-task.png)





