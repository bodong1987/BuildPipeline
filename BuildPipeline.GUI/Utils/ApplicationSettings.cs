using Avalonia;
using Avalonia.Themes.Fluent;
using BuildPipeline.Core.BuilderFramework;
using BuildPipeline.Core.Serialization;
using BuildPipeline.Core.Utils;
using BuildPipeline.Core.Extensions.IO;
using System.ComponentModel;
using BuildPipeline.Core.Framework;
using System.Xml.Serialization;
using BuildPipeline.Core.Services;
using PropertyModels.ComponentModel;
using BuildPipeline.Core;
using PropertyModels.Collections;
using PropertyModels.Localilzation;
using Avalonia.Styling;

namespace BuildPipeline.GUI.Utils
{
    /// <summary>
    /// Class ApplicationSettings.
    /// </summary>
    [PathConfiguration("Settings.config")]
    public class ApplicationSettings : Configuration<ApplicationSettings>, IReactiveObject
    {
        #region Root
        /// <summary>
        /// The default
        /// </summary>
        public static ApplicationSettings Default;

        static ApplicationSettings()
        {
            try
            {
                Default = ApplicationSettings.OverrideFromFile();
            }
            catch
            {
                Default = new ApplicationSettings();
            }
        }
        #endregion

        #region Properties
        CommandLineFormatMethod CmdFormatMethodCore = CommandLineFormatMethod.Simplify;
        /// <summary>
        /// Gets or sets the command format method.
        /// </summary>
        /// <value>The command format method.</value>
        [DisplayName("Format Method")]
        [Description("The format method for populate command line for a task")]
        [Category("Pipeline View")]
        public CommandLineFormatMethod CmdFormatMethod
        {
            get => CmdFormatMethodCore;
            set => this.RaiseAndSetIfChanged(ref  CmdFormatMethodCore, value);
        }

        /// <summary>
        /// The modern style core
        /// </summary>
        ModernStyleType ModernStyleCore = ModernStyleType.Default;

        /// <summary>
        /// Gets or sets the modern style.
        /// </summary>
        /// <value>The modern style.</value>
        [DisplayName("Modern Style")]        
        public ModernStyleType ModernStyle
        {
            get => ModernStyleCore;
            set => this.RaiseAndSetIfChanged(ref ModernStyleCore, value);
        }

        bool AutoExpandRequirementsCore = false;
        /// <summary>
        /// Gets or sets a value indicating whether [automatic expand requirements].
        /// </summary>
        /// <value><c>true</c> if [automatic expand requirements]; otherwise, <c>false</c>.</value>
        [DisplayName("Expand Requirements")]
        [Description("Auto expand all requirements in pipeline view")]
        [Category("Pipeline View")]
        public bool AutoExpandRequirements
        {
            get => AutoExpandRequirementsCore;
            set => this.RaiseAndSetIfChanged(ref AutoExpandRequirementsCore, value);
        }

        bool AutoExpandTaskOptionsPropertiesCore = false;

        /// <summary>
        /// Gets or sets a value indicating whether [automatic expand task options properties].
        /// </summary>
        /// <value><c>true</c> if [automatic expand task options properties]; otherwise, <c>false</c>.</value>
        [DisplayName("Expand Task Properties")]
        [Description("Auto expand all task option's properties")]
        [Category("Pipeline View")]
        public bool AutoExpandTaskOptionsProperties
        {
            get => AutoExpandTaskOptionsPropertiesCore;
            set => this.RaiseAndSetIfChanged(ref AutoExpandTaskOptionsPropertiesCore, value);
        }            

        bool EnableAudioNotificationCore = true;

        /// <summary>
        /// Gets or sets a value indicating whether [enable audio notification].
        /// </summary>
        /// <value><c>true</c> if [enable audio notification]; otherwise, <c>false</c>.</value>
        [DisplayName("Enable Audio")]
        [Description("Do you want to get sound notification when task execute finished or canceled?")]
        public bool EnableAudioNotification
        {
            get => EnableAudioNotificationCore;
            set => this.RaiseAndSetIfChanged(ref EnableAudioNotificationCore, value);
        }

        /// <summary>
        /// Gets or sets the available cultures.
        /// </summary>
        /// <value>The available cultures.</value>
        [DisplayName("Language")]
        [Description("Select your lauguage")]
        [XmlIgnore]
        public ISelectableList<CultureInfoData> AvailableCultures
        {
            get => ServiceProvider.GetService<ILocalizeService>().AvailableCultures;
            set
            {
            }
        }

        /// <summary>
        /// Gets or sets the name of the culture.
        /// </summary>
        /// <value>The name of the culture.</value>
        [Browsable(false)]
        public string CultureName
        {
            get => ServiceProvider.GetService<ILocalizeService>().AvailableCultures.SelectedValue?.CultureInfo?.Name;
            set
            {
                var info = ServiceProvider.GetService<ILocalizeService>().AvailableCultures.ToList().Find(x => x.CultureInfo.Name == value);
                ServiceProvider.GetService<ILocalizeService>().AvailableCultures.SelectedValue = info;
            }
        }

        StyleType StyleTypeCore = StyleType.Default;

        public StyleType Style
        {
            get => StyleTypeCore;
            set
            {
                if(StyleTypeCore != value)
                {
                    StyleTypeCore = value;

                    switch(StyleTypeCore)
                    {
                        case StyleType.Default:
                            {
                                try
                                {
                                    Application.Current!.RequestedThemeVariant = ThemeVariant.Default;
                                }
                                catch
                                {
                                }
                            }
                            break;
                        case StyleType.Dark:
                            {
                                try
                                {
                                    Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
                                }
                                catch
                                {

                                }
                            }
                            break;
                        case StyleType.Light:
                            {
                                try
                                {
                                    Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
                                }
                                catch
                                {

                                }
                            }
                            break;
                    }
                }
            }
        }


        #endregion

        #region Open Apis
        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            ApplicationSettings defaultOne = new ApplicationSettings();
            string text = defaultOne.SaveToText();

            var obj = this;
            ObjectXmlSerializer.OverrideFromText(ref obj, text, null);
        }

        /// <summary>
        /// Reloads this instance.
        /// </summary>
        public void Reload()
        {
            if(this.ExpectFullPath.IsFileExists())
            {
                var obj = this;
                ObjectXmlSerializer.OverrideFromFile(ref obj, this.ExpectFullPath);
            }
            else
            {
                Reset();
                Save();
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    /// <summary>
    /// Enum ModernStyleType
    /// </summary>
    public enum ModernStyleType
    {
        /// <summary>
        /// The default
        /// </summary>
        Default,

        /// <summary>
        /// The windows metro
        /// </summary>
        WindowsMetro,

        /// <summary>
        /// The mac os
        /// </summary>
        MacOS,

        /// <summary>
        /// The classic
        /// </summary>
        Classic
    }
}
