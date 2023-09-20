using Microsoft.Scripting.Hosting;
using System.Text;

namespace BuildPipeline.Core.PythonScripts
{
    /// <summary>
    /// Class PythonScriptCompiler.
    /// </summary>
    public class PythonScriptCompiler
    {
        ScriptEngine Engine;
        bool EnableDebugger;

        Dictionary<string, ScriptSource> ScriptDict = new Dictionary<string, ScriptSource>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonScriptCompiler" /> class.
        /// </summary>
        /// <param name="engine">The engine.</param>
        /// <param name="enableDebugger">if set to <c>true</c> [enable debugger].</param>
        public PythonScriptCompiler(ScriptEngine engine, bool enableDebugger)
        {
            Engine = engine;
            EnableDebugger = enableDebugger;
        }

        /// <summary>
        /// Compiles the specified script full path.
        /// </summary>
        /// <param name="scriptFullPath">The script full path.</param>
        /// <param name="useCache">if set to <c>true</c> [use cache].</param>
        /// <returns>ScriptSource.</returns>
        public ScriptSource Compile(string scriptFullPath, bool useCache)
        {
            ScriptSource Source;

            if (useCache && ScriptDict.TryGetValue(scriptFullPath, out Source))
            {
                return Source;
            }

            if (EnableDebugger)
            {
                Source = Engine.CreateScriptSourceFromFile(scriptFullPath, Encoding.UTF8, Microsoft.Scripting.SourceCodeKind.File);
            }
            else
            {
                string code = File.ReadAllText(scriptFullPath, Encoding.UTF8);
                Source = Engine.CreateScriptSourceFromString(code, Microsoft.Scripting.SourceCodeKind.File);
            }

            if(useCache)
            {
                ScriptDict.Add(scriptFullPath, Source);
            }            

            return Source;
        }
    }
}
