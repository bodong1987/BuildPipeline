using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using BuildPipeline.Core.BuilderFramework;
using BuildPipeline.Core.Framework;
using BuildPipeline.GUI.ViewModels;
using BuildPipeline.GUI.Views;
using System;
using Avalonia.Svg.Skia;
using BuildPipeline.GUI.Utils;

namespace BuildPipeline.GUI;

public partial class App : Application
{
    public override void Initialize()
    {
        // for support svg in preview...
        GC.KeepAlive(typeof(SvgImageExtension).Assembly);
        GC.KeepAlive(typeof(Avalonia.Svg.Skia.Svg).Assembly);

        ExtensibilityFramework.AddPart(typeof(App).Assembly);

        BuildFramework.Initialize(false);

        AppThemeUtils.BeforeInitialize();

        AvaloniaXamlLoader.Load(this);

        AppThemeUtils.AfterInitialize();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }
//         else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
//         {
//             singleViewPlatform.MainView = new MainView
//             {
//                 DataContext = new MainViewModel()
//             };
//         }

        base.OnFrameworkInitializationCompleted();
    }
}
