using BuildPipeline.Core.Framework;

namespace BuildPipeline.GUI.Services
{
    /// <summary>
    /// Interface IMediaPlayerService
    /// Implements the <see cref="IService" />
    /// </summary>
    /// <seealso cref="IService" />
    public interface IMediaPlayerService : IService
    {
        /// <summary>
        /// Plays the audio asynchronous.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Task.</returns>
        Task PlayAudioAsync(string path);
    }
}
