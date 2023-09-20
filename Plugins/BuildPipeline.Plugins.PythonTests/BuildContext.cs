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