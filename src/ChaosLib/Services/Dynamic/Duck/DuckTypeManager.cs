#if NETFRAMEWORK
using System;
using System.Collections.Generic;

namespace ChaosLib.Dynamic
{
    class DuckTypeManager
    {
        #region Static

        private static object lockObj = new object();

        private static Dictionary<Type, Type> typeMap = new Dictionary<Type, Type>();
        private static Dictionary<Type, Type> proxyInterfaceMap = new Dictionary<Type, Type>();
        private static HashSet<Type> reverseProxies = new HashSet<Type>();

        #endregion

        public static Type Build(Type outerType, Type innerType)
        {
            lock (lockObj)
            {
                Type result;

                if (typeMap.TryGetValue(innerType, out result))
                    return result;
            }

            var builder = new DuckTypeBuilder(outerType, innerType);

            Type type;

            try
            {
                type = builder.Builder.CreateType();
            }
            catch (TypeLoadException ex) when (ex.Message.Contains("inaccessible interface"))
            {
                throw new InvalidOperationException($"Failed to create duck type '{outerType.Name}' around type '{innerType.Name}'. This usually means the inner type is not visible to the dynamically generated assembly. Consider decorating assembly '{innerType.Assembly.GetName().Name}' with '[assembly: InternalsVisibleTo(\"ChaosLib.GeneratedCode\")]'", ex);
            }
            

            lock (lockObj)
            {
                typeMap[innerType] = type;
            }

            return type;
        }

        internal static void SetReverseProxy(Type type)
        {
            lock (lockObj)
            {
                reverseProxies.Add(type);
            }
        }

        internal static bool IsReverseProxy(Type type)
        {
            lock (lockObj)
            {
                return reverseProxies.Contains(type);
            }
        }

        public static bool TryGetIProxyTFromInterfaceType(Type outerInterfaceType, out Type proxyTType)
        {
            lock (lockObj)
            {
                if (proxyInterfaceMap.TryGetValue(outerInterfaceType, out proxyTType))
                    return true;
            }

            return false;
        }

        public static void RegisterProxy(Type outerInterfaceType, Type proxyTType)
        {
            lock (lockObj)
            {
                proxyInterfaceMap[outerInterfaceType] = proxyTType;
            }
        }
    }
}
#endif