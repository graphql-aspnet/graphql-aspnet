// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.Execution
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An interface representing a group of <see cref="IGraphFieldInvocationContext" />.
    /// </summary>
    public interface IFieldInvocationContextCollection : IEnumerable<IGraphFieldInvocationContext>
    {
        /// <summary>
        /// Adds the specified context to the collection.
        /// </summary>
        /// <param name="context">The context.</param>
        void Add(IGraphFieldInvocationContext context);

        /// <summary>
        /// Determines whether any context in this collection could accept a source item
        /// of the given type as its source data value.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if an item in this instance can accept the specified type; otherwise, <c>false</c>.</returns>
        bool CanAcceptSourceType(Type type);

        /// <summary>
        /// Gets the number of contexts in this collection.
        /// </summary>
        /// <value>The count.</value>
        int Count { get; }

        /// <summary>
        /// Gets the <see cref="IGraphFieldInvocationContext"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>IGraphFieldExecutionContext.</returns>
        IGraphFieldInvocationContext this[int index] { get; }
    }
}