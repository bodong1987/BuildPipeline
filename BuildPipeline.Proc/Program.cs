using System.Reflection;

namespace BuildPipeline.Proc
{
    internal class Program
    {
        static readonly string TypeName = "BuildPipeline.Core.BuilderFramework.BuildFramework";
        static readonly string MethodName = "Main";

        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            string location = typeof(Program).Assembly.Location;
            string directory = Path.GetDirectoryName(location);

            string[] EntryPathes = new string[]{
                Path.Combine(directory, "libs/BuildPipeline.Core.dll"),
                Path.Combine(directory, "BuildPipeline.Core.dll")
            };

            foreach(var file in EntryPathes)
            {
                if(File.Exists(file))
                {
                    var asm = LoadEntryAssembly(file);

                    if(asm != null)
                    {
                        var type = asm.GetType(TypeName);

                        if(type == null)
                        {
                            Console.WriteLine($"Failed find type {TypeName} in {asm.Location}");

                            return -1;
                        }

                        var method = type.GetMethod(MethodName, BindingFlags.Static | BindingFlags.Public);

                        if(method == null)
                        {
                            Console.WriteLine($"Failed find method:{MethodName} on {TypeName} in {asm.Location}");
                            return -1;
                        }

                        return (int)method.Invoke(null, new object[] {args});
                    }
                }
            }

            return -1;
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string location = typeof(Program).Assembly.Location;
            string directory = Path.GetDirectoryName(location);

            string name = args.Name;
            int index = args.Name.IndexOf(',');
            if (index != -1)
            {
                name = args.Name.Substring(0, index);
            }

            var asm = GetAssemblyByName(name);

            if (asm != null)
            {
                return asm;
            }

            string p = Path.Combine(directory, name + ".dll");

            if(File.Exists(p))
            {
                return Assembly.LoadFile(p);
            }

            return null;
        }

        /// <summary>
        /// Gets the name of the assembly by.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns>Assembly.</returns>
        private static Assembly GetAssemblyByName(string assemblyName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GetName().Name.Equals(assemblyName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return asm;
                }
            }

            return null;
        }

        private static Assembly LoadEntryAssembly(string path)
        {
            return Assembly.LoadFile(path);
        }
    }
}