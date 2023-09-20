using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core;
using BuildPipeline.GUI.Utils;
using BuildPipeline.Core.Utils;

namespace BuildPipeline.GUI.Views
{
    /// <summary>
    /// Class AboutAppWindow.
    /// Implements the <see cref="Window" />
    /// </summary>
    /// <seealso cref="Window" />
    public partial class AboutAppWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AboutAppWindow"/> class.
        /// </summary>
        public AboutAppWindow()
        {
            InitializeComponent();            
        }

        /// <summary>
        /// Called when the <see cref="P:Avalonia.StyledElement.DataContext" /> property changes.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
        }

        /// <summary>
        /// Raises the Close event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnClose(object sender, RoutedEventArgs e)
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

            if(!e.Handled && e.Key == Key.Escape)
            {
                Close();
            }
        }

        private async void OnClickWebsite(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenWebBrowserUtils.OpenUnsafe(AppFramework.ProjectUrl);
            }
            catch (Exception ex)
            {
                await MessageBoxUtils.ShowErrorAsync(this, ServiceProvider.GetService<ILocalizeService>()["Error"], ex.Message);
            }
        }
    }
}
