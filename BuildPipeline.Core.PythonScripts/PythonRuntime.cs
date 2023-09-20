using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Utils;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using System.Text;

namespace BuildPipeline.Core.PythonScripts
{
    /// <summary>
    /// Class PythonRuntime.
    /// Implements the <see cref="IDisposable" />
    /// </summary>
    /// <seealso cref="IDisposable" />
    public class PythonRuntime : IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the engine.
        /// </summary>
        /// <value>The engine.</value>
        public ScriptEngine Engine { get; private set; }

        /// <summary>
        /// Gets the global scope.
        /// </summary>
        /// <value>The global scope.</value>
        public ScriptScope GlobalScope { get; private set; }

        /// <summary>
        /// Gets the compiler.
        /// </summary>
        /// <value>The compiler.</value>
        public PythonScriptCompiler Compiler { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value><c>true</c> if this instance is initialized; otherwise, <c>false</c>.</value>
        public bool IsInitialized => Engine != null;

        /// <summary>
        /// Gets a value indicating whether this instance is debugger enabled.
        /// </summary>
        /// <value><c>true</c> if this instance is debugger enabled; otherwise, <c>false</c>.</value>
        public bool IsDebuggerEnabled
        {
            get; private set;
        }

        private Dictionary<string, ScriptSource> SourceDict = new Dictionary<string, ScriptSource>();

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="PythonRuntime" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        private PythonRuntime(string name)
        {
            Name = name;

            Dictionary<string, object> options = new Dictionary<string, object>();

            var pythonDebugVar = Environment.GetEnvironmentVariable("ENABLE_PYTHON_DEBUGGER");

#if DEBUG
            pythonDebugVar = "1";
#endif
            if (pythonDebugVar == "1" || Environment.CommandLine.Contains("--python-debugger", StringComparison.CurrentCultureIgnoreCase))
            {
                options.Add("Debug", true);
                IsDebuggerEnabled = true;
            }

            Logger.LogVerbose("Python Debugger Enabled = {0}", IsDebuggerEnabled);

            Engine = IronPython.Hosting.Python.CreateEngine(options);

            // avoid some warnings...
            // @reference https://github.com/IronLanguages/main/issues/1085
            var pc = HostingHelpers.GetLanguageContext(Engine) as PythonContext;
            var hooks = pc.SystemState.Get__dict__()["path_hooks"] as PythonList;

            // remove zipimporter
            if (hooks.Count > 0)
            {
                hooks.RemoveAt(0);
            }
             
            GlobalScope = Engine.CreateScope();

            Compiler = new PythonScriptCompiler(Engine, IsDebuggerEnabled);
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if(Engine != null)
            {
                GlobalScope = null;
                Compiler = null;
                Engine = null;
            }
        }

        /// <summary>
        /// Adds the search pathes.
        /// </summary>
        /// <param name="searchPaths">The search paths.</param>
        public void AddSearchPathes(IEnumerable<string> searchPaths)
        {
            if (Engine == null)
            {
                return;
            }

            var pathes = Engine.GetSearchPaths().ToList();

            foreach (var s in searchPaths)
            {
                if (s.IsDirectoryExists() && !pathes.Contains(s))
                {
                    Logger.LogVerbose("PythonRuntime {0}: Add search path:{1}", Name, s);
                    pathes.Add(s);
                }
            }

            Engine.SetSearchPaths(pathes);
        }

        /// <summary>
        /// Executes the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="scope">The scope.</param>
        /// <param name="useCache">if set to <c>true</c> [use cache].</param>
        /// <returns>dynamic.</returns>
        public dynamic Execute(string path, ScriptScope scope, bool useCache = false)
        {
            if (!path.IsFileExists())
            {
                Logger.LogError("{0} is not exists.", path);
                return -1;
            }

            ScriptSource source = Compiler.Compile(path, useCache);

            if(source == null)
            {
                Logger.LogError("Failed compile script file:{0}", path);
                return -1;
            }

            try
            {
                return source.Execute(scope);
            }
            catch(Exception e)
            {
                throw new PythonExecuteException(e);
            }
        }

        /// <summary>
        /// Executes the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="method">The method.</param>
        /// <param name="scope">The scope.</param>
        /// <param name="useCache">if set to <c>true</c> [use cache].</param>
        /// <returns>dynamic.</returns>
        /// <exception cref="BuildPipeline.Core.PythonScripts.PythonExecuteException"></exception>
        public dynamic Execute(string path, string method, ScriptScope scope, bool useCache = false)
        {
            Execute(path, scope, useCache);

            var members = method.Split('.');

            var target = scope.GetVariable(members[0]);
            if (target == null)
            {
                throw new PythonExecuteException($"{path} does not contain {method}");
            }

            if (members.Length > 1)
            {
                return Engine.Operations.InvokeMember(target, members[1]);
            }
            else
            {
                return Engine.Operations.Invoke(target);
            }
        }

        #endregion

        #region Factories
        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="redirection">if set to <c>true</c> [redirection].</param>
        /// <returns>PythonRuntime.</returns>
        public static PythonRuntime Create(string name, bool redirection)
        {
            PythonRuntime runtime = new PythonRuntime(name);

            if(redirection)
            {
                runtime.Engine.Runtime.IO.SetOutput(new RedirectionOutStream(LoggerLevel.Information), Encoding.Default);
                runtime.Engine.Runtime.IO.SetErrorOutput(new RedirectionOutStream(LoggerLevel.Error), Encoding.Default);
            }

            return runtime;
        }
        #endregion
    }
}
