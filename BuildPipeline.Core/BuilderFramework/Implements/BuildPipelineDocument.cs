using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Serialization;
using BuildPipeline.Core.Utils;
using PropertyModels.Extensions;
using System.Text;
using System.Xml.Serialization;

namespace BuildPipeline.Core.BuilderFramework.Implements
{
    /// <summary>
    /// Class BuildPipelineDocument.
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.IBuildPipelineDocument" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.IBuildPipelineDocument" />
    [ObjectXmlSerializePolicy(XmlSerializationPolicy.IgnoreFields)]
    internal class BuildPipelineDocument : IBuildPipelineDocument
    {
        #region Serialization
        /// <summary>
        /// Gets the name of the factory.
        /// </summary>
        /// <value>The name of the factory.</value>
        public string FactoryName { get; private set; }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>The path.</value>
        [XmlIgnore]
        public string Path { get; private set; }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        public IBuildContext Context { get; internal set; }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>The settings.</value>
        public IBuildTaskSettings[] Settings { get; internal set; }

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildPipelineDocument"/> class.
        /// </summary>
        public BuildPipelineDocument()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildPipelineDocument" /> class.
        /// </summary>
        /// <param name="buildPipeline">The build pipeline.</param>
        /// <param name="forceAllTasks">if set to <c>true</c> [force all tasks].</param>
        public BuildPipelineDocument(IBuildPipeline buildPipeline, bool forceAllTasks)
        {            
            Context = buildPipeline.Context;
            FactoryName = Context.Name;

            List<IBuildTaskSettings> settings = new List<IBuildTaskSettings>();

            foreach(var task in forceAllTasks ? buildPipeline.Graph.GetAllTasks() : buildPipeline.Graph.GetIncludeTasks())
            {
                settings.Add(task.Settings);
            }

            Settings = settings.ToArray();
        }

        #endregion


        #region Stream Serialization
        /// <summary>
        /// Loads the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Load(Stream stream)
        {
            var obj = this;
            if(!ObjectXmlSerializer.OverrideFromStream(ref obj, stream, null))
            {
                return false;
            }

            if(this.FactoryName.IsNotNullOrEmpty() && Context != null)
            {
                var Factory = BuildFramework.FindFactory(this.FactoryName);

                Context.Factory = Factory;
            }

            return true;
        }

        /// <summary>
        /// Saves the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Save(Stream stream)
        {
            var text = ObjectXmlSerializer.SerializeToText(this);

            if(text == null)
            {
                return false;
            }

            var bytes = Encoding.UTF8.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);

            return true;
        }
        #endregion

        #region File Serialization
        /// <summary>
        /// Loads the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Load(string path)
        {
            if(!path.IsFileExists())
            {
                return false;
            }

            using(var stream = File.OpenRead(path))
            {
                Path = path;
                return Load(stream);
            }
        }

        /// <summary>
        /// Saves the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Save(string path)
        {
            try
            {
                if (path.IsNullOrEmpty())
                {
                    return false;
                }

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed delete file {0}, {1}", path, ex.Message);
            }

            using (MemoryStream stream = new MemoryStream())
            {
                if(!Save(stream))
                {
                    return false;
                }

                File.WriteAllBytes(path, stream.ToArray());

                Path = path;

                return true;
            }
        }

        /// <summary>
        /// Redirects to file.
        /// </summary>
        /// <param name="path">The path.</param>
        public void RedirectToFile(string path)
        {
            Path = path;
        }
        #endregion

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>IBuildPipelineDocument.</returns>
        public IBuildPipelineDocument Clone()
        {
            using(MemoryStream ms = new MemoryStream())
            {
                if(!Save(ms))
                {
                    return null;
                }

                BuildPipelineDocument doc = new BuildPipelineDocument();
                ms.Position = 0;
                if(!doc.Load(ms))
                {
                    return null;
                }

                return doc;
            }
        }
    }
}
