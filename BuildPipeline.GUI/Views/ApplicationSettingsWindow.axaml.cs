using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using BuildPipeline.GUI.Utils;

namespace BuildPipeline.GUI.Views
{
    /// <summary>
    /// Class ApplicationSettingsWindow.
    /// Implements the <see cref="Window" />
    /// </summary>
    /// <seealso cref="Window" />
    public partial class ApplicationSettingsWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationSettingsWindow"/> class.
        /// </summary>
        public ApplicationSettingsWindow()
        {
            InitializeComponent();
        }

        private void OnReset(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.Default.Reset();
        }

        private void OnOK(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.Default.Save();

            Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.Default.Reload();

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
                ApplicationSettings.Default.Reload();
                Close();
            }
        }
    }
}
