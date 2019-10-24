// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A graph field that is to be bound to a graph type associated with some concrete type in the future.
    /// </summary>
    internal class UnregisteredGraphField
    {
        /// <summary>
        /// Gets the comparer for this class type.
        /// </summary>
        /// <value>The comparer.</value>
        public static IEqualityComparer<UnregisteredGraphField> Comparer
        {
            get
            {
                return new UnregisteredGraphFieldComparer();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnregisteredGraphField" /> class.
        /// </summary>
        /// <param name="field">The field to register.</param>
        /// <param name="concreteType">The concrete type representing the graph type to which this field should
        /// be added.</param>
        public UnregisteredGraphField(IGraphField field, Type concreteType)
        {
            this.Field = Validation.ThrowIfNullOrReturn(field, nameof(field));
            this.ConcreteType = Validation.ThrowIfNullOrReturn(concreteType, nameof(concreteType));
        }

        /// <summary>
        /// Gets the field represented by this instance.
        /// </summary>
        /// <value>The field.</value>
        public IGraphField Field { get; }

        /// <summary>
        /// Gets the concrete type associated with this extension field.
        /// </summary>
        /// <value>The type of the concrete.</value>
        public Type ConcreteType { get; }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return this.Field.Name.GetHashCode();
        }

        /// <summary>
        /// A comparer to evaluate the equality of two <see cref="UnregisteredGraphFieldComparer"/> to prevent
        /// duplicate registrations of extension fields.
        /// </summary>
        public class UnregisteredGraphFieldComparer : IEqualityComparer<UnregisteredGraphField>
        {
            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <param name="x">The first object of type T to compare.</param>
            /// <param name="y">The second object of type T to compare.</param>
            /// <returns>true if the specified objects are equal; otherwise, false.</returns>
            public bool Equals(UnregisteredGraphField x, UnregisteredGraphField y)
            {
                if (x == null && y == null)
                    return true;

                if (x != null && y != null)
                    return x.Field?.Name == y.Field?.Name;

                return false;
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <param name="obj">The <see cref="T:System.Object"></see> for which a hash code is to be returned.</param>
            /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
            public int GetHashCode(UnregisteredGraphField obj)
            {
                return obj?.GetHashCode() ?? 0;
            }
        }
    }
}