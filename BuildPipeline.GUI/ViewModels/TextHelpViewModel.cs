using PropertyModels.ComponentModel;

namespace BuildPipeline.GUI.ViewModels
{
    /// <summary>
    /// Class TextHelpViewModel.
    /// Implements the <see cref="ReactiveObject" />
    /// </summary>
    /// <seealso cref="ReactiveObject" />
    public class TextHelpViewModel : ReactiveObject
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }
        /// <summary>
        /// Gets or sets the command line.
        /// </summary>
        /// <value>The command line.</value>
        public string CommandLine { get; set; }
    }
}
