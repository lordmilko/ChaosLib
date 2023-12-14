#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ChaosLib.Dynamic
{
    /// <summary>
    /// Provides facilities for performing dynamic operations against objects.
    /// </summary>
    public static class DynamicExtensions
    {
        /// <summary>
        /// Creates a duck typed wrapper for encapsulating the specified value under the guise of a given interface.
        /// </summary>
        /// <typeparam name="T">The type of interface to present the given value as.</typeparam>
        /// <param name="value">The value to wrap.</param>
        /// <returns>A duck type that encapsulates the specified value.</returns>
        public static T As<T>(this object value)
        {
            if (value == null)
                return default(T);

            if (!typeof(T).IsInterface)
                throw new InvalidOperationException($"Cannot create a duck type that implements type '{typeof(T).Name}': type is not an interface.");

            Type innerType = GetDynamicTypeAttribute<ReflectedTypeAttribute>(typeof(T), false)?.Type ?? GetInnerType(typeof(T), value.GetType());

            var result = DuckTypeManager.Build(typeof(T), innerType);

            return (T) Activator.CreateInstance(result, value);
        }

        /// <summary>
        /// Creates a wrapper that implements a foreign, unreferenced interface type described in an <see cref="ReflectedTypeAttribute"/>
        /// that will relay all method calls to a type that is known by your application.<para/>
        /// This method should be used when you have to pass an instance of a type that you don't have to a foreign method. For example,
        /// a method may want a Contoso.ILogger, so this method will allow you to create a proxy that implements Contoso.ILogger which relays
        /// to your internal IFakeLogger.
        /// </summary>
        /// <typeparam name="TLocalInterface">The type of value to be encapsulated.</typeparam>
        /// <param name="value">The value to be encapsulated.</param>
        /// <returns>A type that implements the specified interface and encapsulates the specified value.</returns>
        public static object AsReverseProxy<TLocalInterface>(this TLocalInterface value)
        {
            var attrib = GetDynamicTypeAttribute<ReflectedTypeAttribute>(typeof(TLocalInterface), true);

            var result = DuckTypeManager.Build(attrib.Type, typeof(TLocalInterface));
            DuckTypeManager.SetReverseProxy(result);

            return Activator.CreateInstance(result, value);
        }

        /// <summary>
        /// Creates a new instance of a type defined in a <see cref="NewReflectedTypeAttribute"/> and encapsulates it in a duck typed wrapper.
        /// </summary>
        /// <typeparam name="T">The duck type interface containing a <see cref="NewReflectedTypeAttribute"/> specifying the type to be created.</typeparam>
        /// <param name="args">The arguments to pass to the specified type's constructor.</param>
        /// <returns>A duck type that encapsulates the newly created instance.</returns>
        public static T New<T>(params object[] args)
        {
            if (!typeof(T).IsInterface)
                throw new InvalidOperationException($"Cannot create a duck type that implements type '{typeof(T).Name}': type is not an interface.");

            var attrib = GetDynamicTypeAttribute<NewReflectedTypeAttribute>(typeof(T), true);

            var types = args.Select(a => a.GetType()).ToArray();
            var lambdaParameters = types.Select(Expression.Parameter).ToArray();

            var ctor = FindBestConstructor(attrib.Type, types);

            var ctorParameters = GetCtorArgs(ctor, lambdaParameters);

            var lambda = Expression.Lambda(
                Expression.New(ctor, ctorParameters),
                lambdaParameters
            );

            var compiled = lambda.Compile();

            var result = compiled.DynamicInvoke(args);

            return result.As<T>();
        }

        private static Expression[] GetCtorArgs(ConstructorInfo ctor, Expression[] lambdaParameters)
        {
            var results = new List<Expression>();

            var index = -1;
            var ctorParameters = ctor.GetParameters();

            foreach (var parameter in lambdaParameters)
            {
                index++;

                var proxyInterface = parameter.Type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDuckProxy<>));

                if (proxyInterface == null || DuckTypeManager.IsReverseProxy(parameter.Type))
                {
                    if (ctorParameters.Length > index)
                    {
                        if (ctorParameters[index].ParameterType.IsEnum && !parameter.Type.IsEnum)
                        {
                            results.Add(Expression.Convert(parameter, ctorParameters[index].ParameterType));
                            continue;
                        }
                            
                    }

                    results.Add(parameter);
                }
                else
                {
                    var getInner = parameter.Type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Single(m => m.Name == "GetInner" && m.ReturnType != typeof(object));

                    results.Add(Expression.Call(parameter, getInner));
                }
            }

            return results.ToArray();
        }

        private static ConstructorInfo FindBestConstructor(Type type, Type[] parameterTypes)
        {
            var candidates = type.GetConstructors();

            if (candidates.Length == 1)
                return candidates[0];

            var ctorsWithParameters = candidates.Select(c => new
            {
                Ctor = c,
                Parameters = c.GetParameters()
            }).ToArray();

            var lengthMatches = ctorsWithParameters.Where(v => v.Parameters.Length == parameterTypes.Length).ToArray();

            if (lengthMatches.Length == 0)
                throw new InvalidOperationException($"Could not find a constructor on type '{type.Name}' that takes {parameterTypes.Length} arguments");

            if (lengthMatches.Length == 1)
                return lengthMatches[0].Ctor;

            var parameterTypeMatches = new List<ConstructorInfo>();

            foreach (var candidate in lengthMatches)
            {
                var bad = false;

                for (var i = 0; i < candidate.Parameters.Length; i++)
                {
                    var candidateParameter = candidate.Parameters[i].ParameterType;
                    var valueType = parameterTypes[i];

                    if (!IsAssignableTo(candidateParameter, valueType))
                    {
                        bad = true;
                        break;
                    }
                }

                if (!bad)
                    parameterTypeMatches.Add(candidate.Ctor);
            }

            if (parameterTypeMatches.Count == 0)
                throw new InvalidOperationException($"Could not find a constructor on type '{type.Name}' that can accept values of type {string.Join(", ", parameterTypes.Select(t => t.Name))}.");

            if (parameterTypeMatches.Count == 1)
                return parameterTypeMatches[0];

            throw new InvalidOperationException($"Could not find an appropriate constructor to use on type '{type.Name}'.");
        }

        private static bool IsAssignableTo(Type parameterType, Type valueType)
        {
            if (parameterType == valueType)
                return true;

            if (parameterType.IsAssignableFrom(valueType))
                return true;

            var proxyInterface = valueType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDuckProxy<>));

            if (proxyInterface != null && !DuckTypeManager.IsReverseProxy(valueType))
                return IsAssignableTo(parameterType, proxyInterface.GetGenericArguments()[0]);

            if (parameterType.IsEnum && valueType == typeof(int))
                return true;

            if (!parameterType.IsInterface)
                return false;

            var ifaces = valueType.GetInterfaces();

            foreach (var iface in ifaces)
            {
                if (iface == parameterType)
                    return true;
            }

            return false;
        }

        private static Type GetInnerType(Type outerType, Type innerType)
        {
            var interfaces = innerType.GetInterfaces();

            foreach (var iface in interfaces)
            {
                if (iface.Name == outerType.Name)
                    return iface;
            }

            return innerType;
        }

        private static ReflectedTypeAttribute GetDynamicTypeAttribute<T>(Type type, bool mandatory) where T : ReflectedTypeAttribute
        {
            var attrib = (T) type.GetCustomAttributes().FirstOrDefault(a => a.GetType() == typeof(T));

            if (attrib == null && mandatory)
            {
                var candidates = new List<Type>();

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        candidates.AddRange(assembly.GetTypes().Where(t => t != type && t.Name == type.Name).ToArray());
                    }
                    catch
                    {
                    }
                }

                var str = string.Join(Environment.NewLine, candidates.Select(c => c.Assembly.GetName().Name + "!" + c.Name));

                throw new InvalidOperationException($"Type '{type.Name}' does not contain an '{typeof(T).Name}'. The following candidate types were found amongst all loaded assemblies: {str}");
            }

            return attrib;
        }
    }
}
#endif