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
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A representation of a type that needs to be registered to a schema when its
    /// initialied by the runtime.
    /// </summary>
    [DebuggerDisplay("Type: {Type.Name}")]
    public class SchemaTypeToRegister
    {
        /// <summary>
        /// Gets a comparer that can properly compare two <see cref="SchemaTypeToRegister"/> objects.
        /// </summary>
        /// <value>The default comparer.</value>
        public static IEqualityComparer<SchemaTypeToRegister> DefaultEqualityComparer { get; } = new TypeToRegisterComparer();

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaTypeToRegister" /> class.
        /// </summary>
        /// <param name="type">The type to be registered to the schema.</param>
        /// <param name="typeKind">The graph type kind to register the type as.</param>
        public SchemaTypeToRegister(Type type, TypeKind? typeKind = null)
        {
            this.Type = Validation.ThrowIfNullOrReturn(type, nameof(type));
            this.TypeKind = typeKind;
        }

        /// <summary>
        /// Gets the type that needs to be registered to the schema.
        /// </summary>
        /// <value>The type.</value>
        public Type Type { get; }

        /// <summary>
        /// Gets the typekind to register this item as.
        /// </summary>
        /// <value>The kind of the type.</value>
        public TypeKind? TypeKind { get; }

        /// <summary>
        /// A comparer to equate to <see cref="SchemaTypeToRegister"/> objects.
        /// </summary>
        public class TypeToRegisterComparer : IEqualityComparer<SchemaTypeToRegister>
        {
            /// <inheritdoc />
            public bool Equals(SchemaTypeToRegister x, SchemaTypeToRegister y)
            {
                if (x != null && y != null)
                    return x.Type == y.Type && x.TypeKind == y.TypeKind;
                else if (x == null && y == null)
                    return true;
                else
                    return false;
            }

            /// <inheritdoc />
            public int GetHashCode(SchemaTypeToRegister obj)
            {
                return obj.Type.GetHashCode();
            }
        }
    }
}