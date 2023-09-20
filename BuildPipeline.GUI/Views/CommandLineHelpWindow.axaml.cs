using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace BuildPipeline.GUI.Views
{
    /// <summary>
    /// Class CommandLineHelpWindow.
    /// Implements the <see cref="Window" />
    /// </summary>
    /// <seealso cref="Window" />
    public partial class CommandLineHelpWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineHelpWindow"/> class.
        /// </summary>
        public CommandLineHelpWindow()
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
