// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration.Mvc
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A constructor manager for dynamically building instances of graph schemas
    /// from a service container. This accounts for an unknown setup of a user's specific schema (allowing them the freedom to do what they want)
    /// and still be able to get ahold of a solid instance to initialize via the default methods if the user chooses not to override.
    /// </summary>
    internal static class GraphSchemaBuilder
    {
        /// <summary>
        /// A list of known "good" constructors to use for any subsequent attempts to build a schema from this
        /// object. Saves time in reanalyzing the possible constructors for usable signatures.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, ConstructorInfo> SCHEMA_CONSTRUCTORS = new ConcurrentDictionary<Type, ConstructorInfo>();

        /// <summary>
        /// Builds the schema according to all options previously configured.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema to build.</typeparam>
        /// <param name="sp">The schema.</param>
        /// <returns>TSchema.</returns>
        public static TSchema BuildSchema<TSchema>(IServiceProvider sp)
            where TSchema : class, ISchema
        {
            Validation.ThrowIfNull(sp, nameof(sp));
            List<object> paramSet = null;
            if (!SCHEMA_CONSTRUCTORS.TryGetValue(typeof(TSchema), out var schemaConstructor))
            {
                // attempt to find a constructor for which all all its parameters are present on the DI container
                // then build the schema
                var constructors = typeof(TSchema).GetConstructors().OrderByDescending(x => x.GetParameters().Length);
                foreach (var constructor in constructors)
                {
                    paramSet = RetrieveConstructorParams(sp, constructor);
                    if (paramSet != null)
                    {
                        schemaConstructor = constructor;
                        break;
                    }
                }

                SCHEMA_CONSTRUCTORS.TryAdd(typeof(TSchema), schemaConstructor);
            }

            if (schemaConstructor != null && paramSet == null)
                paramSet = RetrieveConstructorParams(sp, schemaConstructor);

            if (paramSet == null)
            {
                throw new InvalidOperationException("No suitable constructors found " +
                                                    $"to properly instantiate schema '{typeof(TSchema).FriendlyName()}'.");
            }

            return InstanceFactory.CreateInstance(typeof(TSchema), paramSet.ToArray()) as TSchema;
        }

        /// <summary>
        /// Inspects the DI container attempting to resolve all the parameterse of the constructor. The parameter
        /// set is returned if all parameters are accounted for, otherwise null is returned.
        /// </summary>
        /// <param name="sp">The sp.</param>
        /// <param name="constructorInfo">The constructor information.</param>
        /// <returns>System.Collections.Generic.List&lt;System.Object&gt;.</returns>
        private static List<object> RetrieveConstructorParams(IServiceProvider sp, ConstructorInfo constructorInfo)
        {
            var paramSet = new List<object>();
            foreach (var param in constructorInfo.GetParameters())
            {
                var service = sp.GetService(param.ParameterType);
                if (service != null)
                {
                    paramSet.Add(service);
                }
                else
                {
                    if (!param.HasDefaultValue)
                    {
                        paramSet = null;
                        break;
                    }

                    paramSet.Add(param.DefaultValue);
                }
            }

            return paramSet;
        }
    }
}