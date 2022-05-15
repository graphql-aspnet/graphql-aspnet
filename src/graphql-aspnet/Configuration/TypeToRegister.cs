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
    public class TypeToRegister
    {
        /// <summary>
        /// Gets a comparer that can properly compare two <see cref="TypeToRegister"/> objects.
        /// </summary>
        /// <value>The default comparer.</value>
        public static IEqualityComparer<TypeToRegister> DefaultComparer { get; } = new TypeToRegisterComparer();

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeToRegister"/> class.
        /// </summary>
        /// <param name="type">The type to be registered to the schema.</param>
        public TypeToRegister(Type type)
        {
            this.Type = Validation.ThrowIfNullOrReturn(type, nameof(type));
            this.AppliedDirectives = new List<IAppliedDirective>();
        }

        /// <summary>
        /// Gets the set of directives to be applied to <see cref="Type"/> when
        /// its added to the target schema.
        /// </summary>
        /// <value>The applied directives.</value>
        public IEnumerable<IAppliedDirective> AppliedDirectives { get; }

        /// <summary>
        /// Gets the type that needs to be registered to the schema.
        /// </summary>
        /// <value>The type.</value>
        public Type Type { get; }

        /// <summary>
        /// A comparer to equate to <see cref="TypeToRegister"/> objects.
        /// </summary>
        public class TypeToRegisterComparer : IEqualityComparer<TypeToRegister>
        {
            /// <inheritdoc />
            public bool Equals(TypeToRegister x, TypeToRegister y)
            {
                if (x != null && y != null)
                    return x.Type == y.Type;
                else if (x == null && y == null)
                    return true;
                else
                    return false;
            }

            /// <inheritdoc />
            public int GetHashCode(TypeToRegister obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}