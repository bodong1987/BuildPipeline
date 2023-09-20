using System.ComponentModel.DataAnnotations;

namespace BuildPipeline.Core.BuilderFramework
{
    /// <summary>
    /// Interface IValidatingable
    /// </summary>
    public interface IValidatingable
    {
        /// <summary>
        /// Checks the validation.
        /// </summary>
        /// <returns>ValidationResult.</returns>
        ValidationResult CheckValidation();
    }
}
