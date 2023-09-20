using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Utils;
using PropertyModels.Extensions;
using System.Text;

namespace BuildPipeline.Core.PythonScripts
{
    /// <summary>
    /// Class RedirectionOutStream.
    /// Implements the <see cref="System.IO.Stream" />
    /// </summary>
    /// <seealso cref="System.IO.Stream" />
    internal class RedirectionOutStream : Stream
    {
        /// <summary>
        /// Gets the level.
        /// </summary>
        /// <value>The level.</value>
        public LoggerLevel Level { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectionOutStream"/> class.
        /// </summary>
        /// <param name="messageLevel">The message level.</param>
        public RedirectionOutStream(LoggerLevel messageLevel)
        {
            Level = messageLevel;
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <value><c>true</c> if this instance can read; otherwise, <c>false</c>.</value>
        public override bool CanRead => false;


        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <value><c>true</c> if this instance can seek; otherwise, <c>false</c>.</value>
        public override bool CanSeek => false;


        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <value><c>true</c> if this instance can write; otherwise, <c>false</c>.</value>
        public override bool CanWrite => true;


        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <value>The length.</value>
        /// <exception cref="System.NotSupportedException">Cannot get the length of this Stream.</exception>
        public override long Length
        {
            get
            {
                throw new NotSupportedException("Cannot get the length of this Stream.");
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <value>The position.</value>
        /// <exception cref="System.NotSupportedException">Cannot get the position of this Stream.</exception>
        /// <exception cref="System.NotSupportedException">Cannot set the position of this Stream.</exception>
        public override long Position
        {
            get
            {
                throw new NotSupportedException("Cannot get the position of this Stream.");
            }
            set
            {
                throw new NotSupportedException("Cannot set the position of this Stream.");

            }
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        /// <exception cref="System.NotSupportedException">Cannot read from this Stream.</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Cannot read from this Stream.");
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        /// <exception cref="System.NotSupportedException">Cannot seek on this Stream.</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Cannot seek on this Stream.");
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="System.NotSupportedException">Cannot set the length of this Stream.</exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException("Cannot set the length of this Stream.");
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            var str = Encoding.Default.GetString(buffer, offset, count);
            
            if (str.Trim().IsNotNullOrEmpty())
            {
                Logger.Log(Level, "Script", str);
            }
        }
    }

    /// <summary>
    /// Class LoggerObserverRedirectionOutStream.
    /// Implements the <see cref="BuildPipeline.Core.PythonScripts.RedirectionOutStream" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.PythonScripts.RedirectionOutStream" />
    internal class LoggerObserverRedirectionOutStream : RedirectionOutStream
    {
        internal readonly ILoggerObserver Observer;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerObserverRedirectionOutStream"/> class.
        /// </summary>
        /// <param name="observer">The observer.</param>
        /// <param name="level">The level.</param>
        public LoggerObserverRedirectionOutStream(ILoggerObserver observer, LoggerLevel level) :
            base(level)
        {
            Observer = observer;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var str = Encoding.Default.GetString(buffer, offset, count);

            if (str.Trim().IsNotNullOrEmpty())
            {
                Observer.OnEvent(Level, "Script", str);
            }
        }
    }
}
