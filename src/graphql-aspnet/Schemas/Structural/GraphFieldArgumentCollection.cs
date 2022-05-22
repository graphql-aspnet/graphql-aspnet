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
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A collection of allowed arguments defined for a <see cref="IGraphField"/> or <see cref="IDirective"/>.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class GraphFieldArgumentCollection : IGraphArgumentCollection
    {
        private readonly ISchemaItem _owner;
        private readonly OrderedDictionary<string, IGraphArgument> _arguments;
        private IGraphArgument _sourceArgument;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldArgumentCollection" /> class.
        /// </summary>
        /// <param name="owner">The owner, usually a field, that owns this
        /// argument collection.</param>
        public GraphFieldArgumentCollection(ISchemaItem owner)
        {
            _owner = Validation.ThrowIfNullOrReturn(owner, nameof(owner));
            _arguments = new OrderedDictionary<string, IGraphArgument>(StringComparer.Ordinal);
        }

        /// <inheritdoc />
        public IGraphArgument AddArgument(IGraphArgument argument)
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
        public IGraphArgument AddArgument(
            string name,
            string internalName,
            GraphTypeExpression typeExpression,
            Type concreteType)
        {
            var argument = new VirtualGraphFieldArgument(
                _owner,
                name,
                internalName,
                typeExpression,
                _owner.Route.CreateChild(name),
                concreteType,
                false);

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
        public IGraphArgument AddArgument(
            string name,
            string internalName,
            GraphTypeExpression typeExpression,
            Type concreteType,
            object defaultValue)
        {
            var argument = new VirtualGraphFieldArgument(
                _owner,
                name,
                internalName,
                typeExpression,
                _owner.Route.CreateChild(name),
                concreteType,
                true,
                defaultValue,
                GraphArgumentModifiers.None);

            return this.AddArgument(argument);
        }

        /// <inheritdoc />
        public bool ContainsKey(string argumentName)
        {
            if (argumentName == null)
                return false;

            return _arguments.ContainsKey(argumentName);
        }

        /// <inheritdoc />
        public IGraphArgument FindArgument(string argumentName)
        {
            if (this.ContainsKey(argumentName))
                return this[argumentName];

            return null;
        }

        /// <inheritdoc />
        public IGraphArgument this[string argumentName] => _arguments[argumentName];

        /// <inheritdoc />
        public IGraphArgument this[int index] => _arguments.GetItem(index).Value;

        /// <inheritdoc />
        public int Count => _arguments.Count;

        /// <inheritdoc />
        public IGraphArgument SourceDataArgument => _sourceArgument;

        /// <inheritdoc />
        public IEnumerator<IGraphArgument> GetEnumerator()
        {
            return _arguments.Values.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}