
using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Services;
using PropertyModels.ComponentModel;
using PropertyModels.ComponentModel.DataAnnotations;
using PropertyModels.Extensions;
using System.Collections;

namespace BuildPipeline.GUI.ViewModels
{
    /// <summary>
    /// Enum ServiceState
    /// </summary>
    public enum ServiceState
    {
        /// <summary>
        /// The checking
        /// </summary>
        Checking,
        /// <summary>
        /// The available
        /// </summary>
        Available,
        /// <summary>
        /// The not available
        /// </summary>
        NotAvailable
    }


    /// <summary>
    /// Class EnvironmentServiceDataModel.
    /// Implements the <see cref="ReactiveObject" />
    /// </summary>
    /// <seealso cref="ReactiveObject" />
    public class EnvironmentServiceDataModel : ReactiveObject
    {
        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <value>The service.</value>
        public IEnvironmentService Service { get; private set; }

        ServiceState _State = ServiceState.Checking;

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        public ServiceState State
        {
            get => _State;
            set => this.RaiseAndSetIfChanged(ref _State, value);
        }

        /// <summary>
        /// Gets a value indicating whether [need help].
        /// </summary>
        /// <value><c>true</c> if [need help]; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(State))]
        public bool NeedHelp => !IsAvailable;

        /// <summary>
        /// Gets a value indicating whether this instance is available.
        /// </summary>
        /// <value><c>true</c> if this instance is available; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(State))]
        public bool IsAvailable => State == ServiceState.Available;

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        [DependsOnProperty(nameof(State))]
        public Version Version => IsChecking ? null : Service.Version;

        /// <summary>
        /// The installation directory
        /// </summary>
        [DependsOnProperty(nameof(State))]
        public string InstallationDirectory => IsChecking ? "" : Service.InstallationPath;

        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>The name of the service.</value>
        [DependsOnProperty(nameof(State))]
        public string ServiceName => Service.Name;

        /// <summary>
        /// Gets a value indicating whether this instance is checking.
        /// </summary>
        /// <value><c>true</c> if this instance is checking; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(State))]
        public bool IsChecking => State == ServiceState.Checking;

        /// <summary>
        /// Gets a value indicating whether this instance is un available.
        /// </summary>
        /// <value><c>true</c> if this instance is un available; otherwise, <c>false</c>.</value>
        [DependsOnProperty(nameof(State))]
        public bool IsUnAvailable => State == ServiceState.NotAvailable;

        /// <summary>
        /// Gets the help text.
        /// </summary>
        /// <value>The help text.</value>
        [DependsOnProperty(nameof(State))]
        public string HelpText => IsUnAvailable && !IsChecking ? Service.GetHelp() : "";

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentServiceDataModel"/> class.
        /// </summary>
        /// <param name="service">The service.</param>
        public EnvironmentServiceDataModel(IEnvironmentService service)
        {
            Service = service;  
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Service.ServiceName;
        }
    }

    /// <summary>
    /// Class EnviromentServiceListDataModel.
    /// </summary>    
    public class EnvironmentServiceListDataModel : ReactiveObject, IEnumerable<EnvironmentServiceDataModel>
    {
        /// <summary>
        /// The environments core
        /// </summary>
        List<EnvironmentServiceDataModel> EnvironmentsCore = new List<EnvironmentServiceDataModel>();

        /// <summary>
        /// Gets the environments.
        /// </summary>
        /// <value>The environments.</value>
        public EnvironmentServiceDataModel[] Environments => EnvironmentsCore.ToArray();

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentServiceListDataModel"/> class.
        /// </summary>
        public EnvironmentServiceListDataModel()
        {
            List<IEnvironmentService> Services = new List<IEnvironmentService>();

            foreach(var assembly in ExtensibilityFramework.GetAssemblies())
            {
                foreach(var type in assembly.GetTypes())
                {
                    if(type.IsInterface && type != typeof(IEnvironmentService) && type.IsImplementFrom<IEnvironmentService>())
                    {
                        var service = ServiceProvider.GetService(type) as IEnvironmentService;

                        if(service != null)
                        {
                            Services.Add(service);                            
                        }
                    }
                }
            }

            Services.OrderBy(x => x.ServiceName).ToList().ForEach(service =>
            {
                EnvironmentsCore.Add(new EnvironmentServiceDataModel(service));
            });
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<EnvironmentServiceDataModel> GetEnumerator()
        {
            return EnvironmentsCore.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return EnvironmentsCore.GetEnumerator();
        }
    }
}
