using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using PropertyModels.Extensions;

namespace BuildPipeline.Core.BuilderFramework.Implements
{
    /// <summary>
    /// Class BuildContextFactoryCollector.
    /// </summary>
    internal class BuildContextFactoryCollector
    {
        internal readonly List<IBuildContextFactory> AllFactories = new List<IBuildContextFactory>();

        /// <summary>
        /// Collects this instance.
        /// </summary>
        public void Collect()
        {
            var parts = ExtensibilityFramework.GetExportParts(typeof(IBuildContextFactory).FullName);

            if (parts == null)
            {
                Logger.LogWarning("Failed find task by ContractName = {0}", typeof(IBuildContextFactory).FullName);
                return;
            }

            foreach (var part in parts)
            {
                if (part.Type.IsImplementFrom<IBuildContextFactory>())
                {
                    var factory = part.CreatePartObject(CreationPolicy.Shared) as IBuildContextFactory;

                    if (factory != null && factory.Accept(this))
                    {
                        AllFactories.Add(factory);
                    }
                }
            }
        }
    }
}
