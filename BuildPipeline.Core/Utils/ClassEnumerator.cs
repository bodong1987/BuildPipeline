using System.Reflection;

namespace BuildPipeline.Core.Utils
{
    /// <summary>
    /// Class ClassEnumerator.
    /// </summary>
    public class ClassEnumerator
    {
        /// <summary>
        /// The results
        /// </summary>
        protected List<Type> ResultsPrivate = new List<Type>();

        /// <summary>
        /// Gets the results.
        /// </summary>
        /// <value>The results.</value>
        public List<Type> Results { get { return ResultsPrivate; } }

        /// <summary>
        /// The attribute type
        /// </summary>
        private Type AttributeType;
        /// <summary>
        /// The interface type
        /// </summary>
        private Type InterfaceType;

        /// <summary>
        /// The assemblies
        /// </summary>
        public List<Assembly> Assemblies = new List<Assembly>();
        /// <summary>
        /// The assembly dictionary
        /// </summary>
        public Dictionary<Assembly, List<Type>> AssemblyDict = new Dictionary<Assembly, List<Type>>();

        /// <summary>
        /// Gets any assembly.
        /// </summary>
        /// <value>Any assembly.</value>
        public Assembly AnyAssembly
        {
            get
            {
                return Assemblies.Count > 0 ? Assemblies[0] : null;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ClassEnumerator" /> class.
        /// </summary>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="assembly">The assembly.</param>
        /// <param name="ignoreAbstract">if set to <c>true</c> [ignore abstract].</param>
        /// <param name="inheritAttribute">if set to <c>true</c> [inherit attribute].</param>
        public ClassEnumerator(
            Type attributeType,
            Type interfaceType,
            Assembly[] assembly = null,
            bool ignoreAbstract = true,
            bool inheritAttribute = false
            )
        {
            AttributeType = attributeType;
            InterfaceType = interfaceType;

            try
            {
                if (assembly == null || assembly.Length == 0)
                {
                    Assembly[] LocalAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                    Assemblies.AddRange(LocalAssemblies);
                }
                else
                {
                    Assemblies.AddRange(assembly);
                }

                foreach (var i in Assemblies)
                {
                    CheckInAssembly(i, ignoreAbstract, inheritAttribute);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error in enumerate classes :" + e.Message);
            }
        }

        /// <summary>
        /// Checks the in assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="ignoreAbstract">if set to <c>true</c> [ignore abstract].</param>
        /// <param name="inheritAttribute">if set to <c>true</c> [inherit attribute].</param>
        protected void CheckInAssembly(
            Assembly assembly,
            bool ignoreAbstract,
            bool inheritAttribute
            )
        {
            Type[] Types = assembly.GetTypes();
            List<Type> TmpResult = new List<Type>();

            if (Types != null)
            {
                for (int i = 0; i < Types.Length; ++i)
                {
                    var t = Types[i];

                    // test if it is implement from this interface
                    if (InterfaceType == null || InterfaceType.IsAssignableFrom(t))
                    {
                        // check if it is abstract
                        if (!ignoreAbstract || (ignoreAbstract && !t.IsAbstract))
                        {
                            // check if it have this attribute
                            if (t.GetCustomAttributes(AttributeType, inheritAttribute).Length > 0)
                            {
                                ResultsPrivate.Add(t);
                                TmpResult.Add(t);
                            }
                        }
                    }
                }
            }

            AssemblyDict.Add(assembly, TmpResult);
        }
    }

}
