// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Configuration
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A representation of a type that needs to be registered to a schema when its
    /// initialied by the runtime.
    /// </summary>
    public class SchemaTypeToRegister
    {
        /// <summary>
        /// Gets a comparer that can properly compare two <see cref="SchemaTypeToRegister"/> objects.
        /// </summary>
        /// <value>The default comparer.</value>
        public static IEqualityComparer<SchemaTypeToRegister> DefaultComparer { get; } = new TypeToRegisterComparer();

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaTypeToRegister"/> class.
        /// </summary>
        /// <param name="type">The type to be registered to the schema.</param>
        public SchemaTypeToRegister(Type type)
        {
            this.Type = Validation.ThrowIfNullOrReturn(type, nameof(type));
        }

        /// <summary>
        /// Gets the type that needs to be registered to the schema.
        /// </summary>
        /// <value>The type.</value>
        public Type Type { get; }

        /// <summary>
        /// A comparer to equate to <see cref="SchemaTypeToRegister"/> objects.
        /// </summary>
        public class TypeToRegisterComparer : IEqualityComparer<SchemaTypeToRegister>
        {
            /// <inheritdoc />
            public bool Equals(SchemaTypeToRegister x, SchemaTypeToRegister y)
            {
                if (x != null && y != null)
                    return x.Type == y.Type;
                else if (x == null && y == null)
                    return true;
                else
                    return false;
            }

            /// <inheritdoc />
            public int GetHashCode(SchemaTypeToRegister obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}