using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.PropertyGrid.Utils;
using BuildPipeline.Core;
using BuildPipeline.Core.BuilderFramework;
using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Services;
using BuildPipeline.GUI.Utils;
using BuildPipeline.GUI.ViewModels;
using PropertyModels.ComponentModel.DataAnnotations;
using PropertyModels.Extensions;
using System.ComponentModel;

namespace BuildPipeline.GUI.Views
{
    /// <summary>
    /// Class NewBuildContextWindow.
    /// Implements the <see cref="Window" />
    /// </summary>
    /// <seealso cref="Window" />
    public partial class NewBuildContextWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewBuildContextWindow"/> class.
        /// </summary>
        public NewBuildContextWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="change">The change.</param>
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if(change.Property == DataContextProperty)
            {
                if(change.OldValue is System.ComponentModel.INotifyPropertyChanged npc)
                {
                    npc.PropertyChanged -= OnContextPropertyChanged;
                }

                if(change.NewValue is System.ComponentModel.INotifyPropertyChanged npc2)
                {
                    npc2.PropertyChanged += OnContextPropertyChanged;
                }
            }
        }

        private void OnContextPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TaskPropertiesPanel.Children.Clear();

            var dataModel = DataContext as NewBuildContextViewModel;

            if (dataModel == null || dataModel.Document == null)
            {
                return;
            }

            foreach (var settings in dataModel.Settings)
            {
                TaskPropertyView view = new TaskPropertyView();

                TaskPropertiesPanel.Children.Add(view);

                view.DataContext = settings;
            }
        }

        /// <summary>
        /// Called when the <see cref="P:Avalonia.StyledElement.DataContext" /> property changes.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            OnContextPropertyChanged(this, new PropertyChangedEventArgs(nameof(DataContext)));
        }

        #region Event Handler
        private async void OnExport(object sender, RoutedEventArgs e)
        {
            var dataModel = DataContext as NewBuildContextViewModel;

            if (dataModel == null || dataModel.Context == null || !dataModel.CanExport)
            {
                await MessageBoxUtils.ShowErrorAsync(this,
                    $"Error",
                    "Current config can't be exported.");

                return;
            }
            
            var path = await PathBrowserUtils.ShowSaveFileDialogAsync(this, "Build Pipeline Document File(*.bpf)|*.bpf", "Select a file to save", dataModel.Context.Name + ".bpf");

            if (path.IsNullOrEmpty())
            {
                return;
            }

            if (!dataModel.Document.Save(path))
            {
                await MessageBoxUtils.ShowErrorAsync(this, "Error", "Failed save document, Please check log for more information.");
                return;
            }

            await MessageBoxUtils.ShowTipsAsync(this, ServiceProvider.GetService<ILocalizeService>()["Export Success"], $"{path}");
        }

        private async void OnOK(object sender, RoutedEventArgs e)
        {
            var dataModel = DataContext as NewBuildContextViewModel;

            if(dataModel == null || dataModel.Context == null)
            {
                await MessageBoxUtils.ShowErrorAsync(this, 
                    $"Error", 
                    "Please select a Pipeline name first.");

                return;
            }

            dataModel.ValidateDocument();

            if(dataModel.Document == null)
            {
                await MessageBoxUtils.ShowErrorAsync(this,
                    "Error",
                    "No valid document. \r\n\r\nIf there are key attributes in your BuildContext that may cause verification to fail, be sure to add the [ConditionProperty] attribute"
                    );
                return;
            }

            var result = dataModel.Context.CheckValidation();

            if(result.IsFailure())
            {
                await MessageBoxUtils.ShowErrorAsync(this, 
                    $"Failed create new Pipeline", 
                    $"{result.ErrorMessage}"
                    );

                return;
            }

            var pipeline = BuildFramework.NewBuildPipeline(dataModel.Document, false);

            if(pipeline == null)
            {
                await MessageBoxUtils.ShowErrorAsync(this,
                    $"Error",
                    "Failed create new pipeline");

                return;
            }

            result = pipeline.CheckValidation();

            if (result.IsFailure())
            {
                await MessageBoxUtils.ShowErrorAsync(this,
                    $"Pipeline is not in valid state",
                    $"{result.ErrorMessage}"
                    );

                return;
            }

            Close(new NewBuildPipelineResult(pipeline, dataModel.Document));
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            Close(null);
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
                Close(null);
            }
        }
        #endregion
    }
}
