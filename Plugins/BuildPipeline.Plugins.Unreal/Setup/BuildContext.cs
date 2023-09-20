using BuildPipeline.Core.BuilderFramework;
using BuildPipeline.Core.CommandLine;
using BuildPipeline.Core.Extensions.IO;
using PropertyModels.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BuildPipeline.Plugins.Unreal.Setup
{
    internal class SetupBuildContext : AbstractBuildContext
    {
        public override string Name => AbstractSetupTaskExportAttribute.Unreal;

        [Option("engine", HelpText = "Unreal Engine project root directory", Required = true)]
        [ConditionProperty]
        [PathBrowsable(PathBrowsableType.Directory)]
        [EnginePathValidation]
        public string EngineDirectory { get; set; } = "";

        public override ValidationResult CheckValidation()
        {
            if(!EngineDirectory.IsDirectoryExists())
            {
                return BuildInvalidSettingResult($"Invalid engine root directory.");
            }

            return base.CheckValidation();
        }

        public override string ToString()
        {
            return $"{Name} EngineDirectory={EngineDirectory}";
        }
    }

    internal class EnginePathValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string path)
            {
                if (!path.IsDirectoryExists())
                {
                    return new ValidationResult("Directory is not exists.", new string[] { validationContext.DisplayName });
                }

                if (!path.JoinPath("Engine/Binaries/DotNET/GitDependencies").IsDirectoryExists())
                {
                    return new ValidationResult("Although the folder exists, it does not appear to be the correct engine directory", new string[] { validationContext.DisplayName });
                }

                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid path", new string[] { validationContext.DisplayName });
        }
    }
}