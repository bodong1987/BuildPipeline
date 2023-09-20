using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.GUI.Utils;
using BuildPipeline.GUI.ViewModels;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using BuildPipeline.Core.Utils;
using Avalonia.Input;
using Avalonia.PropertyGrid.Services;
using PropertyModels.ComponentModel;
using PropertyModels.Localilzation;
using System.Globalization;

namespace BuildPipeline.GUI.Views;

public partial class MainWindow : Window
{
    /// <summary>
    /// The view model
    /// </summary>
    public readonly MainWindowDataModel ViewModel;

    bool CanExitNow = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        ViewModel = new MainWindowDataModel(this);

        InitializeComponent();

        // force active settings, avoid in design mode
        if (!Design.IsDesignMode)
        {
            ApplicationSettings.Default.Reload();
        }

        DataContext = ViewModel;

        SubscribeToWindowState();
        SubscribeLocalizationEvents();
    }

    #region Localization

    private void SubscribeLocalizationEvents()
    {
        ILocalizeService Service = ServiceProvider.GetService<ILocalizeService>();

        Logger.Assert(Service != null);

        LocalizationService.Default.AddExtraService(new PropertyGridLocalizationAdapter(Service));

        Service.OnCultureChanged += (s, e) =>
        {
            var CultureData = Service.AvailableCultures.SelectedValue;

            LocalizationService.Default.SelectCulture(CultureData.CultureInfo.Name);
        };
    }
    #endregion

    #region Event Handlers
    private void OnRecentFileClicked(object sender, RoutedEventArgs e)
    {
        var item = e.Source as MenuItem;

        if (item == null)
        {
            return;
        }

        var path = item.DataContext as string;

        if (path.IsFileExists())
        {
            ViewModel.OpenDocument(path);
        }
    }

    private void OnClose(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    /// <summary>
    /// Handles the <see cref="E:Closing" /> event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
    private async void OnClosing(object sender, CancelEventArgs e)
    {
        e.Cancel = !CanExitNow;

        if (!CanExitNow)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                WaitForAllTaskExit();

                CanExitNow = true;
            });

            Close();

            Logger.Log("Close application.");
        }
    }

    private void WaitForAllTaskExit()
    {
        foreach (var model in ViewModel.Pipelines)
        {
            model.ExecuteHandler.StopAllTasksAndWait(true);
        }
    }

    /// <summary>
    /// Handles the <see cref="E:OpenHelpDocuments" /> event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private async void OnOpenHelpDocuments(object sender, RoutedEventArgs e)
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

    private void OnAboutApp(object sender, RoutedEventArgs e)
    {
        var window = new AboutAppWindow();

        window.ShowDialog(this);
    }

    private async void OnViewAllCommandLineHelp(object sender, RoutedEventArgs e)
    {
        CommandLineHelpWindow window = new CommandLineHelpWindow()
        {
            DataContext = new CommandLinesDataModel()
        };

        await window.ShowDialog(this);
    }

    private async void OnViewApplicationLogs(object sender, RoutedEventArgs e)
    {
        if (Logger.FileReceiver == null)
        {
            return;
        }

        var path = Logger.FileReceiver.FilePath;

        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start("explorer.exe", $"\"{path}\"");
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", $"\"{path}\"");
            }
        }
        catch (Exception ex)
        {
            await MessageBoxUtils.ShowWarningAsync(this, "Warning", ex.Message);
        }
    }

    /// <summary>
    /// Handles the <see cref="E:OpenAppDirectory" /> event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private async void OnOpenAppDirectory(object sender, RoutedEventArgs e)
    {
        var directory = AppFramework.GetRuntimeApplicationDirectory();

        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start("explorer.exe", $"\"{directory}\"");
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", $"\"{directory}\"");
            }
        }
        catch (Exception ex)
        {
            await MessageBoxUtils.ShowWarningAsync(this, "Warning", ex.Message);
        }
    }

    private async void OnShowEnvironmentServices(object sender, RoutedEventArgs e)
    {
        var dataModel = new EnvironmentServiceListDataModel();

        EnviromentServiceWindow window = new EnviromentServiceWindow()
        {
            DataContext = dataModel
        };

        foreach (var env in dataModel.Environments)
        {
            env.State = ServiceState.Checking;
        }

        // force start in backgroud thread
        _ = Task.Run(() =>
        {
            Parallel.ForEach(dataModel.Environments, (service, state, f) =>
            {
                service.Service.Check();

                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    service.State = service.Service.IsAvailable ? ServiceState.Available : ServiceState.NotAvailable;
                });
            });
        });

        await window.ShowDialog(this);
    }

    private void OnChangeToDarkStyle(object sender, RoutedEventArgs e)
    {
        ApplicationSettings.Default.Style = StyleType.Dark;
        ApplicationSettings.Default.Save();
    }

    private void OnChangeToLightStyle(object sender, RoutedEventArgs e)
    {
        ApplicationSettings.Default.Style = StyleType.Light;
        ApplicationSettings.Default.Save();
    }

    private void OnChangeToDefaultStyle(object sender, RoutedEventArgs e)
    {
        ApplicationSettings.Default.Style = StyleType.Default;
        ApplicationSettings.Default.Save();
    }

    private async void OnSettings(object sender, RoutedEventArgs e)
    {
        ApplicationSettingsWindow window = new ApplicationSettingsWindow();
        await window.ShowDialog(this);
    }

    /// <summary>
    /// Handles the <see cref="E:ShowEnvironmentVariables" /> event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private async void OnShowEnvironmentVariables(object sender, RoutedEventArgs e)
    {
        StringBuilder builder = new StringBuilder();

        List<DictionaryEntry> caches = new List<DictionaryEntry>();
        caches.AddRange(Environment.GetEnvironmentVariables().Cast<DictionaryEntry>());
        caches.Sort((x, y) =>
        {
            return Comparer<string>.Default.Compare(x.Key?.ToString(), y.Key?.ToString());
        });

        foreach (var i in caches)
        {
            builder.AppendLine($"    {i.Key} = {(i.Value ?? "")}");
        }

        TextHelpWindow window = new TextHelpWindow()
        {
            DataContext = new TextHelpViewModel()
            {
                Title = ServiceProvider.GetService<ILocalizeService>()["Environment Variables"],
                CommandLine = builder.ToString()
            }
        };

        await window.ShowDialog(this);
    }

    private void OnChangeLauguage(object sender, RoutedEventArgs e)
    {
        var item = e.Source as MenuItem;

        if (item == null)
        {
            return;
        }

        var data = item.DataContext as CultureDataModel;

        if (data != null && data.Data != null)
        {
            ApplicationSettings.Default.AvailableCultures.SelectedValue = data.Data;
            ApplicationSettings.Default.Save();
        }
    }
    #endregion

    #region For Modern Title Bar
    private void CloseWindow(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Window hostWindow = (Window)this.VisualRoot;
        hostWindow.Close();
    }

    private void CloseWindowWhenTap(object sender, TappedEventArgs e)
    {
        CloseWindow(sender, e);
    }

    private void MaximizeWindow(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Window hostWindow = (Window)this.VisualRoot;

        if (hostWindow.WindowState == WindowState.Normal)
        {
            hostWindow.WindowState = WindowState.Maximized;
        }
        else
        {
            hostWindow.WindowState = WindowState.Normal;
        }
    }

    private void MinimizeWindow(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Window hostWindow = (Window)this.VisualRoot;
        hostWindow.WindowState = WindowState.Minimized;
    }

    private async void SubscribeToWindowState()
    {
        if (ViewModel.ModernStyle.IsWindowsStyle)
        {
            Window hostWindow = (Window)this.VisualRoot;

            while (hostWindow == null)
            {
                hostWindow = (Window)this.VisualRoot;
                await Task.Delay(50);
            }

            hostWindow.GetObservable(Window.WindowStateProperty).Subscribe(s =>
            {
                if (s != WindowState.Maximized)
                {
                    winMaximizeIcon.Data = Geometry.Parse("M2048 2048v-2048h-2048v2048h2048zM1843 1843h-1638v-1638h1638v1638z");
                    hostWindow.Padding = new Thickness(0, 0, 0, 0);
                    MaximizeToolTip.Content = "Maximize";
                }
                if (s == WindowState.Maximized)
                {
                    winMaximizeIcon.Data = Geometry.Parse("M2048 1638h-410v410h-1638v-1638h410v-410h1638v1638zm-614-1024h-1229v1229h1229v-1229zm409-409h-1229v205h1024v1024h205v-1229z");
                    hostWindow.Padding = new Thickness(7, 7, 7, 7);
                    MaximizeToolTip.Content = "Restore Down";

                    // This should be a more universal approach in both cases, but I found it to be less reliable, when for example double-clicking the title bar.
                    /*hostWindow.Padding = new Thickness(
                            hostWindow.OffScreenMargin.Left,
                            hostWindow.OffScreenMargin.Top,
                            hostWindow.OffScreenMargin.Right,
                            hostWindow.OffScreenMargin.Bottom);*/
                }
            });
        }
        else if (ViewModel.ModernStyle.IsMacOSStyle)
        {
            Window hostWindow = (Window)this.VisualRoot;

            while (hostWindow == null)
            {
                hostWindow = (Window)this.VisualRoot;
                await Task.Delay(50);
            }

            hostWindow.ExtendClientAreaTitleBarHeightHint = 44;
            hostWindow.GetObservable(Window.WindowStateProperty).Subscribe(s =>
            {
                if (s != WindowState.Maximized)
                {
                    hostWindow.Padding = new Thickness(0, 0, 0, 0);
                }
                if (s == WindowState.Maximized)
                {
                    hostWindow.Padding = new Thickness(7, 7, 7, 7);

                    // This should be a more universal approach in both cases, but I found it to be less reliable, when for example double-clicking the title bar.
                    /*hostWindow.Padding = new Thickness(
                            hostWindow.OffScreenMargin.Left,
                            hostWindow.OffScreenMargin.Top,
                            hostWindow.OffScreenMargin.Right,
                            hostWindow.OffScreenMargin.Bottom);*/
                }
            });
        }
    }
    #endregion
}
