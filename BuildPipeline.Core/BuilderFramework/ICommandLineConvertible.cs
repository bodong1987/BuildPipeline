using BuildPipeline.Core.CommandLine;
using BuildPipeline.Core.Extensions;
using PropertyModels.ComponentModel.DataAnnotations;
using PropertyModels.Extensions;

namespace BuildPipeline.Core.BuilderFramework
{
    /// <summary>
    /// Enum CommandLineFormatMethod
    /// How to get the command line
    /// </summary>
    public enum CommandLineFormatMethod
    {
        /// <summary>
        /// The complete
        /// Output regardless of whether the parameter is the same as the default value
        /// </summary>
        Complete,

        /// <summary>
        /// The simplify
        /// Only parameters that differ from the default value are output
        /// </summary>
        Simplify
    }

    #region Interfaces
    /// <summary>
    /// Interface ICommandLineConvertible
    /// </summary>
    public interface ICommandLineConvertible
    {
        /// <summary>
        /// Formats the command line.
        /// </summary>
        /// <returns>System.String.</returns>
        string FormatCommandLine(CommandLineFormatMethod method);

        /// <summary>
        /// Formats the command line arguments.
        /// </summary>
        /// <returns>System.String[].</returns>
        string[] FormatCommandLineArgs(CommandLineFormatMethod method);

        /// <summary>
        /// Overrides from command line.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="message">The message.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool OverrideFromCommandLine(string arguments, out string message);

        /// <summary>
        /// Overrides from command line arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="message">The message.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool OverrideFromCommandLineArgs(IEnumerable<string> args, out string message);

        /// <summary>
        /// Builds the help message.
        /// </summary>
        /// <param name="formatter">The formatter.</param>
        /// <returns>System.String.</returns>
        string BuildHelpMessage(IFormatter formatter = null);
    }
    #endregion

    #region Template Implements
    /// <summary>
    /// Class CommandLineConvertible.
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.ICommandLineConvertible" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.ICommandLineConvertible" />
    public class CommandLineConvertible : ICommandLineConvertible
    {
        /// <summary>
        /// Formats the command line.
        /// </summary>
        /// <returns>System.String.</returns>
        public virtual string FormatCommandLine(CommandLineFormatMethod method)
        {
            return Parser.FormatCommandLine(this, method);
        }

        /// <summary>
        /// Formats the command line arguments.
        /// </summary>
        /// <returns>System.String[].</returns>
        public string[] FormatCommandLineArgs(CommandLineFormatMethod method)
        {
            var text = this.FormatCommandLine(method);

            if (text.IsNullOrEmpty())
            {
                return new string[] { };
            }

            return Splitter.SplitCommandLineIntoArguments(text).ToArray();
        }

        /// <summary>
        /// Overrides from command line.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="message">The message.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool OverrideFromCommandLine(string arguments, out string message)
        {
            return OverrideFromCommandLineArgs(Splitter.SplitCommandLineIntoArguments(arguments), out message);
        }

        /// <summary>
        /// Overrides from command line arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="message">The message.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public virtual bool OverrideFromCommandLineArgs(IEnumerable<string> args, out string message)
        {
            var result = Parser.Default.Parse(args, this);

            message = result.ErrorMessage;
            if(result.Result == ParserResultType.Parsed)
            {
                return ValidatorUtils.TryValidateObject(result.Value, out message);
            }

            return false;
        }

        /// <summary>
        /// Builds the help message.
        /// </summary>
        /// <param name="formatter">The formatter.</param>
        /// <returns>System.String.</returns>
        public virtual string BuildHelpMessage(IFormatter formatter = null)
        {
            return Parser.GetHelpText(this, formatter);
        }
    }
    #endregion
}
