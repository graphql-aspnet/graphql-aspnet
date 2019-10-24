// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// A collection of unique types. This collection is guaranteed to contain unique items
    /// and is read-only once created.
    /// </summary>
    public class TypeCollection : IEnumerable<Type>
    {
        /// <summary>
        /// Gets a collection representing no additional types.
        /// </summary>
        /// <value>The none.</value>
        public static TypeCollection Empty { get; }

        /// <summary>
        /// Initializes static members of the <see cref="TypeCollection"/> class.
        /// </summary>
        static TypeCollection()
        {
            Empty = new TypeCollection();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="TypeCollection"/> class from being created.
        /// </summary>
        private TypeCollection()
        {
            this.TypeSet = ImmutableHashSet.Create<Type>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeCollection"/> class.
        /// </summary>
        /// <param name="types">The types.</param>
        public TypeCollection(params Type[] types)
         : this()
        {
            if (types != null)
                this.TypeSet = ImmutableHashSet.Create(types);
        }

        /// <summary>
        /// Determines whether this instance contains the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the type is found; otherwise, <c>false</c>.</returns>
        public bool Contains(Type type)
        {
            return this.TypeSet.Contains(type);
        }

        /// <summary>
        /// Gets the count of types in this set.
        /// </summary>
        /// <value>The count of types.</value>
        public int Count => this.TypeSet.Count;

        /// <summary>
        /// Gets the set of <see cref="Type"/>s that represent the scalar.
        /// </summary>
        /// <value>The type set.</value>
        public IImmutableSet<Type> TypeSet { get; }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<Type> GetEnumerator()
        {
            return this.TypeSet.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}