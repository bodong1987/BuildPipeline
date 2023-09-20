using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace BuildPipeline.GUI.Views
{
    /// <summary>
    /// Class TextHelpWindow.
    /// Implements the <see cref="Window" />
    /// </summary>
    /// <seealso cref="Window" />
    public partial class TextHelpWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextHelpWindow"/> class.
        /// </summary>
        public TextHelpWindow()
        {
            InitializeComponent();
        }

        private void OnOK(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Called before the <see cref="E:Avalonia.Input.InputElement.KeyDown" /> event occurs.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!e.Handled && e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
