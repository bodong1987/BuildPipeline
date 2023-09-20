using BuildPipeline.Core;
using BuildPipeline.Core.BuilderFramework;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Services;
using BuildPipeline.GUI.Utils;
using PropertyModels.ComponentModel;

namespace BuildPipeline.GUI.ViewModels
{
    /// <summary>
    /// Class RequirementDataModel.
    /// Implements the <see cref="ReactiveObject" />
    /// </summary>
    /// <seealso cref="ReactiveObject" />
    public class RequirementDataModel : ReactiveObject
    {
        IEnvironmentRequirement RequirementCore;

        /// <summary>
        /// Gets the platform.
        /// </summary>
        /// <value>The platform.</value>
        public string Platform => RequirementCore.GetActivePlatforms();

        /// <summary>
        /// Gets or sets the requirement.
        /// </summary>
        /// <value>The requirement.</value>
        public IEnvironmentRequirement Requirement 
        {
            get => RequirementCore;
            set => this.RaiseAndSetIfChanged(ref RequirementCore, value);
        }

        CheckRequirementResultType StateCore = CheckRequirementResultType.Unconcerned;
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        public CheckRequirementResultType State
        {
            get => StateCore;
            set => this.RaiseAndSetIfChanged(ref StateCore, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementDataModel"/> class.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        public RequirementDataModel(IEnvironmentRequirement requirement)
        {
            RequirementCore = requirement;

            ValidateStateAsync();
        }

        /// <summary>
        /// Validates the state.
        /// </summary>
        public async void ValidateStateAsync()
        {
            var result = await RequirementCore.CheckRequirementAsync();
            State = result.Result;
        }
    }

    /// <summary>
    /// Class RequirementsDataModel.
    /// Implements the <see cref="ReactiveObject" />
    /// </summary>
    /// <seealso cref="ReactiveObject" />
    public class RequirementsDataModel : ReactiveObject
    {
        readonly List<RequirementDataModel> RequirementsCore = new List<RequirementDataModel>();

        /// <summary>
        /// Gets the requirements.
        /// </summary>
        /// <value>The requirements.</value>
        public RequirementDataModel[] Requirements => RequirementsCore.ToArray();

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title
        {
            get
            {
                string fmt = ServiceProvider.GetService<ILocalizeService>()["Requirements {0}"];

                try
                {
                    return string.Format(fmt, RequirementsCore.Count);
                }
                catch
                {
                }

                return $"{RequirementsCore.Count} Requirements";
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has requirements.
        /// </summary>
        /// <value><c>true</c> if this instance has requirements; otherwise, <c>false</c>.</value>
        public bool HasRequirements => RequirementsCore.Count > 0;

        /// <summary>
        /// The is detail visible core
        /// change this for default value
        /// </summary>
        bool IsDetailVisibleCore = ApplicationSettings.Default.AutoExpandRequirements;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is detail visible.
        /// </summary>
        /// <value><c>true</c> if this instance is detail visible; otherwise, <c>false</c>.</value>
        public bool IsDetailVisible
        {
            get => IsDetailVisibleCore;
            set => this.RaiseAndSetIfChanged(ref  IsDetailVisibleCore, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsDataModel"/> class.
        /// </summary>
        public RequirementsDataModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsDataModel"/> class.
        /// </summary>
        /// <param name="requirements">The requirements.</param>
        public RequirementsDataModel(IEnumerable<IEnvironmentRequirement> requirements)
        {
            Add(requirements);
        }

        /// <summary>
        /// Adds the specified requirement.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        public void Add(IEnvironmentRequirement requirement)
        {
            if(RequirementsCore.Find(x=>x.Equals(requirement)) == null)
            {
                RequirementsCore.Add(new RequirementDataModel(requirement));

                RaisePropertyChanged(nameof(Requirements));
            }
        }

        /// <summary>
        /// Adds the specified requirements.
        /// </summary>
        /// <param name="requirements">The requirements.</param>
        public void Add(IEnumerable<IEnvironmentRequirement> requirements)
        {
            bool IsChanged = false;

            foreach(var r in requirements)
            {
                if(RequirementsCore.Find(x=>x.Equals(r)) == null)
                {
                    RequirementsCore.Add(new RequirementDataModel(r));

                    IsChanged = true;
                }
            }

            if(IsChanged)
            {
                RaisePropertyChanged(nameof(Requirements));
            }
        }

        /// <summary>
        /// Called when [toggle detail visible command].
        /// </summary>
        public void OnToggleDetailVisibleCommand()
        {
            IsDetailVisible = !IsDetailVisibleCore;
        }
    }
}
