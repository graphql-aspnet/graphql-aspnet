// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Structural
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A collection of allowed arguments defined for a <see cref="IGraphField"/> or <see cref="IDirectiveGraphType"/>.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class GraphFieldArgumentCollection : IGraphFieldArgumentCollection
    {
        private readonly Dictionary<string, IGraphFieldArgument> _arguments;
        private IGraphFieldArgument _sourceArgument;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldArgumentCollection"/> class.
        /// </summary>
        public GraphFieldArgumentCollection()
        {
            _arguments = new Dictionary<string, IGraphFieldArgument>(StringComparer.Ordinal);
        }

        /// <summary>
        /// Adds the <see cref="IGraphFieldArgument" /> to the collection.
        /// </summary>
        /// <param name="argument">The argument to add.</param>
        /// <returns>IArgument.</returns>
        public IGraphFieldArgument AddArgument(IGraphFieldArgument argument)
        {
            Validation.ThrowIfNull(argument, nameof(argument));
            _arguments.Add(argument.Name, argument);
            if (argument.ArgumentModifiers.IsSourceParameter() && _sourceArgument == null)
                _sourceArgument = argument;

            return argument;
        }

        /// <summary>
        /// Adds the input argument to the growing collection.
        /// </summary>
        /// <param name="name">The name of the argument in the object graph.</param>
        /// <param name="internalName">Name of the argument as it was defined in the code.</param>
        /// <param name="typeExpression">The type expression representing how this value is represented in this graph schema.</param>
        /// <param name="concreteType">The concrete type this field is on the server.</param>
        /// <returns>IGraphFieldArgument.</returns>
        public IGraphFieldArgument AddArgument(
            string name,
            string internalName,
            GraphTypeExpression typeExpression,
            Type concreteType)
        {
            var argument = new VirtualGraphFieldArgument(
                name,
                internalName,
                typeExpression,
                concreteType);

            return this.AddArgument(argument);
        }

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
        public IGraphFieldArgument AddArgument(
            string name,
            string internalName,
            GraphTypeExpression typeExpression,
            Type concreteType,
            object defaultValue)
        {
            var argument = new VirtualGraphFieldArgument(
                name,
                internalName,
                typeExpression,
                concreteType,
                GraphArgumentModifiers.None,
                defaultValue);

            return this.AddArgument(argument);
        }

        /// <summary>
        /// Determines whether this collection contains a <see cref="IGraphFieldArgument" />. Argument
        /// names are case sensitive and should match the public name supplied for introspection
        /// requests...NOT the internal concrete parameter name if the argument is bound to a method parameter.
        /// </summary>
        /// <param name="argumentName">Name of the argument.</param>
        /// <returns><c>true</c> if this collection contains the type name; otherwise, <c>false</c>.</returns>
        public bool ContainsKey(string argumentName)
        {
            return _arguments.ContainsKey(argumentName);
        }

        /// <summary>
        /// Gets the <see cref="IGraphFieldArgument" /> with the specified name.
        /// </summary>
        /// <param name="argumentName">Name of the argument to return.</param>
        /// <returns>IGraphType.</returns>
        public IGraphFieldArgument this[string argumentName]
        {
            get
            {
                return _arguments[argumentName];
            }
        }

        /// <summary>
        /// Gets the total number of <see cref="IGraphFieldArgument"/> in this collection.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _arguments.Count;

        /// <summary>
        /// Gets the singular argument that is to receive the source data reference value for this field's
        /// resolution.
        /// </summary>
        /// <value>The source data argument.</value>
        public IGraphFieldArgument SourceDataArgument => _sourceArgument;

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<IGraphFieldArgument> GetEnumerator()
        {
            return _arguments.Values.GetEnumerator();
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