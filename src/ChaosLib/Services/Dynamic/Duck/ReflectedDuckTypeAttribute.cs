#if NETFRAMEWORK
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace ChaosLib.Dynamic
{
    /// <summary>
    /// Specifies that the interface this attribute is declared on can be used to create a duck type against a reflected type in a specified assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class ReflectedTypeAttribute : Attribute
    {
        private ConcurrentDictionary<string, Assembly> assemblyMap = new ConcurrentDictionary<string, Assembly>();

        //If more than 1 assembly name is specified, that indicates that certain Visual Studio versions require a different assembly name instead
        private string[] assemblyNames;
        private string typeName;

        private Assembly assembly;

        /// <summary>
        /// Gets or sets whether the specified assembly should be loaded if it is not already loaded.
        /// </summary>
        public bool AllowLoad { get; set; }

        /// <summary>
        /// Gets the assembly containing the type.
        /// </summary>
        public virtual Assembly Assembly
        {
            get
            {
                if (assembly == null)
                {
                    foreach (var assemblyName in assemblyNames)
                    {
                        if (!assemblyMap.TryGetValue(assemblyName, out assembly))
                        {
                            if (assemblyName.EndsWith("*"))
                            {
                                var name = assemblyName.TrimEnd('*');

                                assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name.StartsWith(name));
                            }
                            else
                            {
                                assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
                            }

                            if (assembly == null && AllowLoad)
                            {
                                try
                                {
                                    assembly = Assembly.Load(assemblyName);
                                }
                                catch
                                {
                                    if (assemblyNames.Length == 1)
                                        throw;
                                }
                            }

                            if (assembly != null)
                                assemblyMap[assemblyName] = assembly;
                        }
                    }

                    if (assembly == null)
                    {
                        if (assemblyNames.Length == 1)
                            throw new InvalidOperationException($"Could not find assembly '{assemblyNames[0]}'.");
                        else
                        {
                            var str = string.Join(", ", assemblyNames.Select(v => $"'{v}'"));

                            throw new InvalidOperationException($"Could not find assemblies {str}.");
                        }  
                    }
                }

                return assembly;
            }
        }

        private Type type;

        /// <summary>
        /// Gets the type to encapsulate in a duck type.
        /// </summary>
        public Type Type
        {
            get
            {
                if (type == null)
                {
                    type = Assembly.GetType(typeName);

                    if (type == null)
                        throw new InvalidOperationException($"Could not find type '{typeName}' in assembly '{Assembly.GetName().Name}'.");
                }

                return type;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectedTypeAttribute"/> class with several possible assemblies a given type could be located in.
        /// </summary>
        /// <param name="assemblyNames">The candidate names to search (in order).</param>
        /// <param name="typeName">The name of the type to find in one of the specified assemblies.</param>
        public ReflectedTypeAttribute(string[] assemblyNames, string typeName)
        {
            if (assemblyNames == null)
                throw new ArgumentNullException(nameof(assemblyNames));

            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            if (assemblyNames.Length == 1 && assemblyNames[0] == null)
                throw new ArgumentNullException("assemblyName");

            if (assemblyNames.Length == 0)
                throw new ArgumentException("No assembly names were specified", nameof(assemblyNames));

            if (!typeName.Contains("."))
                typeName = assemblyNames[0] + "." + typeName;

            this.assemblyNames = assemblyNames;
            this.typeName = typeName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectedTypeAttribute"/> class.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly containing the type.</param>
        /// <param name="typeName">The name of the type.</param>
        public ReflectedTypeAttribute(string assemblyName, string typeName) : this(new[] { assemblyName }, typeName)
        {
        }
    }
}
#endif