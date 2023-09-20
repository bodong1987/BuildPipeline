using BuildPipeline.Core.BuilderFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildPipeline.Plugins.Unreal.Setup
{
    internal abstract class AbstractSetupTaskExportAttribute : AbstractBuildTaskExportAttribute
    {
        internal const string Unreal = "UnrealSetup";
        public AbstractSetupTaskExportAttribute() : base(Unreal) 
        {
        }
    }

    internal class SetupUnrealEngineTaskAttribute : AbstractSetupTaskExportAttribute
    {
        public override bool Accept(IBuildContext context)
        {
            if(context is SetupBuildContext bc)
            {
                // check another options

            }

            return true;
        }
    }
}
