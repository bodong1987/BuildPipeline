using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using BuildPipeline.Core;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Services;
using PropertyModels.ComponentModel;
using System.ComponentModel;

namespace BuildPipeline.GUI.Localization
{
    /// <summary>
    /// Class LocalizationExtensions.
    /// </summary>
    public static class LocalizationExtensions
    {
        /// <summary>
        /// Sets the localize binding.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="property">The property.</param>
        /// <param name="name">The name.</param>
        /// <param name="mode">The mode.</param>
        public static void SetLocalizeBinding(this Control control, AvaloniaProperty property, string name, BindingMode mode = BindingMode.Default)
        {
            var source = new LocalizedDataModel(name);
            Binding binding = new Binding
            {
                Source = source,
                Path = nameof(source.Value)
            };

            control.Bind(property, binding);
            control.DataContext = source;
        }
    }

    internal class LocalizedDataModel : ReactiveObject
    {
        public readonly string Name;

        public string Value 
        { 
            get
            {
                ILocalizeService localizeService = ServiceProvider.GetService<ILocalizeService>();

                // in design mode, service maybe unavailable...
                if(localizeService!=null)
                {
                    return localizeService[Name];
                }

                return Name;
            }
        }

        public LocalizedDataModel(string name)
        {
            Name = name;

            ILocalizeService localizeService = ServiceProvider.GetService<ILocalizeService>();

            if(localizeService!=null)
            {
                localizeService.PropertyChanged += OnPropertyChanged;
            }            
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Value));
        }
    }
}
