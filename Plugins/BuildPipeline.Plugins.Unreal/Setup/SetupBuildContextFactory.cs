using BuildPipeline.Core.BuilderFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildPipeline.Plugins.Unreal.Setup
{
    [BuildFactory(AbstractSetupTaskExportAttribute.Unreal)]
    internal class SetupBuildContextFactory : AbstractBuildContextFactory<SetupBuildContext>
    {
    }
}
