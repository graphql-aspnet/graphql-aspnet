// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Helper methods for working with types.
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// <para>Scans the given assembly and returns a list of found types that are not abstract and is: </para>
        /// <para>1) If the type to locate is a class or interface the inspected type will be returned if it implements or inherits from the provided type.</para>
        /// <para>2) If the type to locate is an <see cref="Attribute"/> the inspected type will be returned if it implements the provided <see cref="Attribute"/> type.</para>
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="typesToLocate">A collection of types to use to locate any types in the assembly.</param>
        /// <returns>IReadOnlyList&lt;Type&gt;.</returns>
        public static IEnumerable<Type> LocateTypesInAssembly(this Assembly assembly, params Type[] typesToLocate)
        {
            if (assembly == null || typesToLocate == null || typesToLocate.Length == 0)
                return Enumerable.Empty<Type>();

            var lst = new List<Type>();

            var typesToImplement = typesToLocate.Where(x => !Validation.IsCastable(x, typeof(Attribute))).ToList();
            var attributesToCheckFor = typesToLocate.Where(x => Validation.IsCastable(x, typeof(Attribute))).ToList();

            foreach (Type type in assembly.GetTypes().Where(type => !type.IsAbstract))
            {
                if (typesToImplement.Any(typeToLocate => Validation.IsCastable(type, typeToLocate)))
                {
                    lst.Add(type);
                    continue;
                }

                foreach (var attributeType in attributesToCheckFor)
                {
                    if (type.HasAttribute(attributeType))
                    {
                        lst.Add(type);
                        break;
                    }
                }
            }

            return lst;
        }
    }
}