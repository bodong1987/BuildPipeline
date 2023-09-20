using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Utils;
using PropertyModels.Extensions;
using System.Text;

namespace BuildPipeline.Core.Framework
{
    #region Base Interfaces
    /// <summary>
    /// Enum ServiceStateType
    /// </summary>
    public enum ServiceStateType
    {
        /// <summary>
        /// The created
        /// </summary>
        Created,

        /// <summary>
        /// The checking
        /// </summary>
        Checking,

        /// <summary>
        /// The available
        /// </summary>
        Available,
        /// <summary>
        /// The un avaliable
        /// </summary>
        UnAvaliable
    }

    /// <summary>
    /// Interface IService
    /// Implements the <see cref="BuildPipeline.Core.Framework.IImportable" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Framework.IImportable" />
    [ExportIgnore]
    public interface IService : IImportable
    {
        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>The name of the service.</value>
        string ServiceName { get; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>The state.</value>
        ServiceStateType State { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is available.
        /// </summary>
        /// <value><c>true</c> if this instance is avaliable; otherwise, <c>false</c>.</value>
        bool IsAvailable { get; }
    }

    /// <summary>
    /// Class AbstractService.
    /// Implements the <see cref="BuildPipeline.Core.Framework.AbstractImportable" />
    /// Implements the <see cref="BuildPipeline.Core.Framework.IService" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.Framework.AbstractImportable" />
    /// <seealso cref="BuildPipeline.Core.Framework.IService" />
    [ExportIgnore]
    public abstract class AbstractService : AbstractImportable, IService
    {
        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>The name of the service.</value>
        public virtual string ServiceName => GetType().Name;

        /// <summary>
        /// Gets a value indicating whether this instance is available.
        /// </summary>
        /// <value><c>true</c> if this instance is available; otherwise, <c>false</c>.</value>
        public virtual bool IsAvailable => true;

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>The state.</value>
        public ServiceStateType State { get; protected set; } = ServiceStateType.Created;
    }

    #endregion

    #region Service Provider
    /// <summary>
    /// Class ServiceProvider.
    /// </summary>
    public static class ServiceProvider
    {
        private static Dictionary<Type, IService> ServicesDict = new Dictionary<Type, IService>();
        private static Dictionary<Type, List<IService>> ServicesMultiDict = new Dictionary<Type, List<IService>>();

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>T.</returns>
        public static T GetService<T>() 
            where T : class, IService
        {
            return GetService(typeof(T)) as T;
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>IService.</returns>
        public static IService GetService(Type type)
        {
            if (ServicesDict.TryGetValue(type, out var service))
            {
                return service;
            }

            var list = GetServices(type);

            var selectedService = list.FirstOrDefault();

            if(!ServicesDict.TryAdd(type, selectedService))
            {
                return ServicesDict[type];
            }

            if(selectedService != null && Logger.IsVerboseEnabled)
            {
                StringBuilder builder = new StringBuilder();
                foreach (var i in list)
                {
                    builder.AppendLine($"        [{i.ImportPriority}]{i.GetType().FullName}");
                }

                Logger.LogVerbose("Match Service {0} with {1}\n    Total Services[{3}]:\n{2}", type.FullName, selectedService.GetType().FullName, builder.ToString(), list.Length);
            }            

            return selectedService;
        }

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>IService[].</returns>
        public static IService[] GetServices(Type type)
        {
            if(ServicesMultiDict.TryGetValue(type, out var result))
            {
                return result.ToArray();
            }

            var contractName = type.FullName;

            var exports = ExtensibilityFramework.GetExportParts(contractName);

            if (exports == null)
            {
                ServicesDict.Add(type, null);
                return new IService[] { };
            }

            List<IService> list = new List<IService>();

            foreach (var exportPart in exports)
            {
                if (exportPart.Type.IsImplementFrom(type))
                {
                    var tempService = exportPart.CreatePartObject(CreationPolicy.Shared) as IService;

                    if (tempService != null &&
                        tempService.Accept(typeof(ServiceProvider)))
                    {
                        list.Add(tempService);
                    }
                }
            }

            ExtensibilityFramework.SortImportable(ref list);

            if(!ServicesMultiDict.TryAdd(type, list))
            {
                return ServicesMultiDict[type].ToArray();
            }

            return list.ToArray();
        }
    }
    #endregion
}
