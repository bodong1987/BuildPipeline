using BuildPipeline.Core.Extensions;
using IronPython.Runtime.Operations;
using PropertyModels.Extensions;
using System.Text;

namespace BuildPipeline.Core.PythonScripts
{
    /// <summary>
    /// Class PythonExecuteException.
    /// Implements the <see cref="System.Exception" />
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class PythonExecuteException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PythonExecuteException"/> class.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        public PythonExecuteException(Exception innerException) :
            base(GetExceptionDetailMessage(innerException), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonExecuteException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public PythonExecuteException(string message) :
            base(message)
        {
        }

        /// <summary>
        /// Gets the last frame information.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns>System.String.</returns>
        public static string GetExceptionDetailMessage(Exception e)
        {
            if (e is Microsoft.Scripting.SyntaxErrorException se)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("SyntaxError:");
                sb.AppendLine(se.Message);                

                sb.Append("File:");
                sb.AppendLine(se.SourcePath.IsNotNullOrEmpty() ? se.SourcePath : "Unknown");
                
                sb.Append("Line:");
                sb.AppendLine(se.Line.ToString());

                sb.Append("Column:");
                sb.AppendLine(se.Column.ToString());

                sb.AppendLine("SourceCode:");
                sb.AppendLine(se.SourceCode);

                return sb.ToString();
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(e.Message);
            builder.Append(Environment.NewLine);

            builder.Append(GetExceptionFrames(e));

            return builder.ToString();
        }

        /// <summary>
        /// Gets the exception frames.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns>System.String.</returns>
        public static string GetExceptionFrames(Exception e)
        {
            var frames = PythonOps.GetDynamicStackFrames(e);

            StringBuilder builder = new StringBuilder();

            foreach (var f in frames)
            {
                builder.Append(f.ToString());
                builder.Append(Environment.NewLine);
            }

            return builder.ToString();
        }

    }
}
