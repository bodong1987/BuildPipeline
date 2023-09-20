using BuildPipeline.Core.Framework;

namespace BuildPipeline.Core.Services
{
    /// <summary>
    /// Interface ICompressServiceObserver
    /// </summary>
    public interface ICompressServiceObserver
    {
        /// <summary>
        /// Called when [progress].
        /// </summary>
        /// <param name="progress">The progress.</param>
        void OnProgress(int progress);
    }

    /// <summary>
    /// Interface ICompressService
    /// Implements the <see cref="IService" />
    /// </summary>
    /// <seealso cref="IService" />
    public interface ICompressService : IService
    {
        /// <summary>
        /// Compresses the files.
        /// Compress all contents specified in the inputList array
        /// If the output file extension is 7z, it will be compressed according to 7z. If the output file extension is zip, it will be compressed according to zip.
        /// Note that inputList is an array, which can contain file paths or folders.
        /// </summary>
        /// <param name="intputList">The intput list.</param>
        /// <param name="archiveFile">The archive file.</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="password">The password.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool Compress(
            string[] intputList, 
            string archiveFile, 
            ICompressServiceObserver observer, 
            CancellationTokenSource cancellationTokenSource,
            string password = null
            );
    }

    /// <summary>
    /// Class ICompresServiceExtensions.
    /// </summary>
    public static class ICompresServiceExtensions
    {
        /// <summary>
        /// Compresses the file.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="file">The file.</param>
        /// <param name="archiveFile">The archive file.</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="password">The password.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool CompressFile(
            this ICompressService service,
            string file,
            string archiveFile,
            ICompressServiceObserver observer,
            CancellationTokenSource cancellationTokenSource,
            string password = null
            )
        {
            return service.Compress(new string[] {file}, archiveFile, observer, cancellationTokenSource, password);
        }

        /// <summary>
        /// Compresses the directory.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="directory">The directory.</param>
        /// <param name="archiveFile">The archive file.</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="password">The password.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool CompressDirectory(
            this ICompressService service,
            string directory,
            string archiveFile,
            ICompressServiceObserver observer,
            CancellationTokenSource cancellationTokenSource,
            string password = null
            )
        {
            return service.Compress(new string[] { directory }, archiveFile, observer, cancellationTokenSource, password);
        }
    }

}
