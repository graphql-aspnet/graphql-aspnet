// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Schema
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A collection of <see cref="IGraphArgument"/> for a single <see cref="IGraphField"/> or <see cref="IDirective"/>.
    /// </summary>
    public interface IGraphArgumentCollection : IEnumerable<IGraphArgument>
    {
        /// <summary>
        /// Adds the <see cref="IGraphArgument" /> to the collection.
        /// </summary>
        /// <param name="argument">The argument to add.</param>
        /// <returns>IArgument.</returns>
        IGraphArgument AddArgument(IGraphArgument argument);

        /// <summary>
        /// Adds the input argument to the growing collection.
        /// </summary>
        /// <param name="name">The name of the argument in the object graph.</param>
        /// <param name="internalName">Name of the argument as it was defined in the code.</param>
        /// <param name="typeExpression">The type expression representing how this value is represented in this graph schema.</param>
        /// <param name="concreteType">The concrete type this field is on the server.</param>
        /// <returns>IGraphFieldArgument.</returns>
        IGraphArgument AddArgument(
            string name,
            string internalName,
            GraphTypeExpression typeExpression,
            Type concreteType);

        /// <summary>
        /// Adds the input argument to the growing collection.
        /// </summary>
        /// <param name="name">The name of the argument in the object graph.</param>
        /// <param name="internalName">Name of the argument as it was defined in the code.</param>
        /// <param name="typeExpression">The type expression representing how this value is represented in this graph schema.</param>
        /// <param name="concreteType">The concrete type this field is on the server.</param>
        /// <param name="defaultValue">The default value to set for the argument. If null, indicates
        /// the argument supplies null as the default value.</param>
        /// <returns>IGraphFieldArgument.</returns>
        IGraphArgument AddArgument(
            string name,
            string internalName,
            GraphTypeExpression typeExpression,
            Type concreteType,
            object defaultValue);

        /// <summary>
        /// Determines whether this collection contains a <see cref="IGraphArgument" />. Argument
        /// names are case sensitive and should match the public name as its defined on the target schema
        /// ...NOT the internal concrete parameter name if the argument is bound to a method parameter.
        /// </summary>
        /// <param name="argumentName">Name of the argument.</param>
        /// <returns><c>true</c> if this collection contains the type name; otherwise, <c>false</c>.</returns>
        bool ContainsKey(string argumentName);

        /// <summary>
        /// Finds the argument by the supplied name. Returns null if no
        /// match is found.
        /// </summary>
        /// <param name="argumentName">Name of the argument to locate.</param>
        /// <returns>IGraphArgument.</returns>
        IGraphArgument FindArgument(string argumentName);

        /// <summary>
        /// Gets the <see cref="IGraphArgument" /> with the specified name. Argument
        /// names are case sensitive and should match the public name as its defined on the target schema.
        /// </summary>
        /// <param name="argumentName">Name of the argument to return.</param>
        /// <returns>IGraphFieldArgument.</returns>
        IGraphArgument this[string argumentName] { get; }

        /// <summary>
        /// Gets the <see cref="IGraphArgument" /> with the specified index
        /// in the collection.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>IGraphFieldArgument.</returns>
        IGraphArgument this[int index] { get; }

        /// <summary>
        /// Gets the count of arguments in this collection.
        /// </summary>
        /// <value>The count.</value>
        int Count { get; }

        /// <summary>
        /// Gets the singular argument that is to recieve source data for the field resolution.
        /// </summary>
        /// <value>The source data argument.</value>
        IGraphArgument SourceDataArgument { get; }
    }
}