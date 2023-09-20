using PropertyModels.ComponentModel;
using PropertyModels.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BuildPipeline.Core.BuilderFramework
{
    /// <summary>
    /// Interface IBuildTaskOption
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.ICommandLineConvertible" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.ICommandLineConvertible" />
    public interface IBuildTaskOptions : ICommandLineConvertible, IValidatingable, IReactiveObject
    {
    }

    /// <summary>
    /// Class BuildTaskOptions.
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.CommandLineConvertible" />
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.IBuildTaskOptions" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.CommandLineConvertible" />
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.IBuildTaskOptions" />
    public class BuildTaskOptions : CommandLineConvertible, IBuildTaskOptions, IReactiveObject
    {
        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when [property changing].
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging;

        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the property changing.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void RaisePropertyChanging(string propertyName)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        /// <summary>
        /// Checks the validation.
        /// </summary>
        /// <returns>ValidationResult.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual ValidationResult CheckValidation()
        {
            if (!ValidatorUtils.TryValidateObject(this, out var message))
            {
                return new ValidationResult(message);
            }

            return ValidationResult.Success;
        }
    }
}
