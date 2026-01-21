using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Serialization;
using PropertyModels.Extensions;
using System.ComponentModel;
using System.Xml.Serialization;

namespace BuildPipeline.Core.Utils
{
    /// <summary>
    /// Interface IConfiguration
    /// 
    /// To implement under this framework, the derived Configuration needs to implement the following things to support editing by the configuration editor
    /// 1. Define the static variable ConfigPath to indicate the default saving path
    /// 2. Define Save function
    /// 3. Implement PostSerialized
    /// 4. Implement the static function OverrideFromFile
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Gets the configuration full path.
        /// </summary>
        /// <value>The configuration full path.</value>
        string ConfigFullPath { get; }

        /// <summary>
        /// Posts the serialized.
        /// override from file
        /// </summary>
        void PostSerialized();

        /// <summary>
        /// Saves this instance.
        /// </summary>
        void Save();
    }

    /// <summary>
    /// Class AllowEndUserConfigAttribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AllowEndUserConfigAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        public string DisplayName { get; set; } = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="AllowEndUserConfigAttribute"/> class.
        /// </summary>
        public AllowEndUserConfigAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AllowEndUserConfigAttribute"/> class.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        public AllowEndUserConfigAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }

    /// <summary>
    /// Class ConfigurationChangedEventArgs.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ConfigurationChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The source
        /// </summary>
        public IConfiguration Source;
        /// <summary>
        /// The path
        /// </summary>
        public string Path;
    }

    /// <summary>
    /// Class AllowAutomanticReloadAttribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AllowAutomanticReloadAttribute : Attribute
    {
    }

    /// <summary>
    /// Class AbstractPathConfigurationAttribute.
    /// Implements the <see cref="System.Attribute" />
    /// </summary>
    /// <seealso cref="System.Attribute" />
    public abstract class AbstractPathConfigurationAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the configuration path.
        /// </summary>
        /// <value>The configuration path.</value>
        public abstract string ConfigPath { get; set; }
        /// <summary>
        /// Gets or sets the loading path.
        /// </summary>
        /// <value>The loading path.</value>
        public abstract string LoadingPath { get; set; }
        /// <summary>
        /// Gets or sets the expect path.
        /// </summary>
        /// <value>The expect path.</value>
        public abstract string ExpectPath { get; set; }
    }

    /// <summary>
    /// Class PathConfigurationAttribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PathConfigurationAttribute : AbstractPathConfigurationAttribute
    {
        /// <summary>
        /// Gets or sets the configuration path.
        /// </summary>
        /// <value>The configuration path.</value>
        public override string ConfigPath { get; set; }

        /// <summary>
        /// Record where the current file is loaded from
        /// </summary>
        /// <value>The loading path.</value>
        public override string LoadingPath { get; set; }

        /// <summary>
        /// Gets or sets the expect path.
        /// Expected path This path is always used for saving
        /// </summary>
        /// <value>The expect path.</value>
        public override string ExpectPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [temporary configuration].
        /// Identifies whether this configuration should be a temporary configuration. Temporary configurations are not generated in the executable directory.
        /// </summary>
        /// <value><c>true</c> if [temporary configuration]; otherwise, <c>false</c>.</value>
        private bool TemporaryConfiguration = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathConfigurationAttribute" /> class.
        /// </summary>
        /// <param name="fileRelativePath">Configuration file path is equivalent to the configuration directory</param>
        /// <param name="temporaryConfiguration">if set to <c>true</c> [temporary configuration].</param>
        public PathConfigurationAttribute(string fileRelativePath, bool temporaryConfiguration = false)
        {
            ConfigPath = fileRelativePath;
            TemporaryConfiguration = temporaryConfiguration;

            if (!TemporaryConfiguration)
            {
                foreach (var i in Enumerable.Reverse(AppFramework.ConfigurationDirectories))
                {
                    string TestPath = Path.Combine(i, ConfigPath);

                    if (!ExpectPath.IsNotNullOrEmpty()) //-V3128
                    {
                        ExpectPath = TestPath;
                    }

                    if (File.Exists(TestPath))
                    {
                        LoadingPath = TestPath;
                        return;
                    }
                }

                string documentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string appDocumentPath = documentPath.JoinPath("BuildPipeline/Config");

                try
                {
                    if (!appDocumentPath.IsDirectoryExists())
                    {
                        Directory.CreateDirectory(appDocumentPath);
                    }

                    var TestPath = appDocumentPath.JoinPath(ConfigPath);

                    ExpectPath = TestPath;

                    if(TestPath.IsFileExists())
                    {
                        LoadingPath = TestPath;
                        return;
                    }
                }
                catch
                {
                }
            }
            else
            {
                ExpectPath = Path.Combine(AppFramework.ApplicationTempConfigurationDirectory, "BuildPipeline", ConfigPath);

                if (File.Exists(ExpectPath))
                {
                    LoadingPath = ExpectPath;
                    return;
                }
            }

#if DEBUG
            Logger.LogWarning("missing configuration file : {0}", ConfigPath);
#endif
        }
    }

    /// <summary>
    /// Class Configuration.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Configuration<T> : IConfiguration
        where T : IConfiguration, new()
    {
        /// <summary>
        /// Gets or sets the attribute.
        /// Some special configurations are allowed to be empty
        /// </summary>
        /// <value>The attribute.</value>
        [XmlIgnore]
        [SerializationIgnore]
        [Browsable(false)]
        public AbstractPathConfigurationAttribute Attribute { get; protected set; }

        /// <summary>
        /// Occurs when [configuration changed].
        /// </summary>
        public static event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration{T}"/> class.
        /// </summary>
        public Configuration()
        {
            Attribute = this.GetType().GetAnyCustomAttribute<AbstractPathConfigurationAttribute>();

            if (this.GetType().IsDefined<AllowAutomanticReloadAttribute>())
            {
                ConfigurationChanged += OnExternalConfigurationChanged;
            }
        }

        /// <summary>
        /// Handles the <see cref="E:ExternalConfigurationChanged" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ConfigurationChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnExternalConfigurationChanged(object sender, ConfigurationChangedEventArgs e)
        {
            if (this != e.Source && this.ExpectFullPath == e.Path)
            {
                object target = this;
                ObjectXmlSerializer.OverrideFromFile(ref target, ExpectFullPath);
            }
        }

        /// <summary>
        /// Gets the configuration full path.
        /// </summary>
        /// <value>The configuration full path.</value>
        [XmlIgnore]
        [SerializationIgnore]
        [Browsable(false)]
        public virtual string ConfigFullPath
        {
            get
            {
                Logger.Assert(Attribute != null);
                return Attribute?.LoadingPath;
            }
        }

        /// <summary>
        /// Gets the expect full path.
        /// </summary>
        /// <value>The expect full path.</value>
        [XmlIgnore]
        [SerializationIgnore]
        [Browsable(false)]
        public virtual string ExpectFullPath
        {
            get
            {
                Logger.Assert(Attribute != null);
                return Attribute?.ExpectPath;
            }
        }

        /// <summary>
        /// Overrides from file.
        /// </summary>
        /// <returns>T.</returns>
        public static T OverrideFromFile()
        {
            var Attr = typeof(T).GetAnyCustomAttribute<AbstractPathConfigurationAttribute>();
            return Attr!=null?OverrideFromFile(Attr.LoadingPath):default(T);
        }

        /// <summary>
        /// Overrides from file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>T.</returns>
        public static T OverrideFromFile(string path)
        {
            if (File.Exists(path))
            {
                string text = File.ReadAllText(path);
                try
                {
                    T obj = ObjectXmlSerializer.DeSerializeFromText<T>(text);
                    if (obj != null)
                    {
                        obj.PostSerialized();
                        return obj;
                    }
                }
                catch (System.Xml.XmlException e)
                {
                    Logger.LogError("Failed load config file : {0}, {1}", path, e.Message);
                }

            }

            T Default = new T();
            Default.PostSerialized();
            return Default;
        }

        /// <summary>
        /// Posts the serialized.
        /// </summary>
        public virtual void PostSerialized()
        {
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public virtual void Save()
        {
            Save(ExpectFullPath);
        }

        /// <summary>
        /// Saves the specified fullpath.
        /// </summary>
        /// <param name="fullpath">The fullpath.</param>
        public virtual void Save(string fullpath)
        {
            try
            {
                try
                {
                    string directory = Path.GetDirectoryName(fullpath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                }
                catch
                {
                }

                if (!ObjectXmlSerializer.SerializeToFile(fullpath, this))
                {
                    Logger.LogError("Failed serialize config file {0}", fullpath);
                }
                else
                {
                    ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs()
                    {
                        Source = this,
                        Path = fullpath
                    });
                }
            }
            catch (Exception e)
            {
                Logger.LogError("{0}:{1}", e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// Saves to text.
        /// </summary>
        /// <returns>System.String.</returns>
        public virtual string SaveToText()
        {
            return ObjectXmlSerializer.SerializeToText(this);
        }

        /// <summary>
        /// Overrides from text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>T.</returns>
        public static T OverrideFromText(string text)
        {
            T obj = ObjectXmlSerializer.DeSerializeFromText<T>(text);

            if (obj != null)
            {
                obj.PostSerialized();
                return obj;
            }

            T Default = new T();
            Default.PostSerialized();

            return Default;
        }
    }
}
