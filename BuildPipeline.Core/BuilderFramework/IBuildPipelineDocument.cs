namespace BuildPipeline.Core.BuilderFramework
{
    /// <summary>
    /// Interface IBuildPipelineDocument
    /// </summary>
    public interface IBuildPipelineDocument
    {
        #region Properties
        /// <summary>
        /// Gets the name of the factory.
        /// </summary>
        /// <value>The name of the factory.</value>
        public string FactoryName { get; }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>The path.</value>
        string Path { get; }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        IBuildContext Context { get; }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>The settings.</value>
        IBuildTaskSettings[] Settings { get; }
        #endregion

        #region Serialize Apis
        /// <summary>
        /// Saves the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool Save(Stream stream);

        /// <summary>
        /// Saves the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool Save(string path);

        /// <summary>
        /// Loads the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool Load(string path);

        /// <summary>
        /// Loads the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool Load(Stream stream);

        /// <summary>
        /// Redirects to file.
        /// </summary>
        /// <param name="path">The path.</param>
        void RedirectToFile(string path);
        #endregion

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>IBuildPipelineDocument.</returns>
        IBuildPipelineDocument Clone();
    }
}
