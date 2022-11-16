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
    using System.Diagnostics;
    using System.Reflection;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A field argument not based on a <see cref="ParameterInfo"/> reference from a method or property.
    /// </summary>
    [DebuggerDisplay("Virtual Argument: {Name}")]
    public class VirtualGraphFieldArgument : IGraphArgument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualGraphFieldArgument" /> class.
        /// </summary>
        /// <param name="parent">The parent graph type that owns this virutal field.</param>
        /// <param name="name">The name of this field in the object graph.</param>
        /// <param name="internalName">The name of this field as it exists in the .NET code.</param>
        /// <param name="typeExpression">The graph type expression representing this field.</param>
        /// <param name="route">The route path for this argument.</param>
        /// <param name="concreteType">The concrete graph type in the server code that this argument is mapped to.</param>
        /// <param name="hasDefaultValue">if set to <c>true</c> indicates that this
        /// argument has a default value, even if its null.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="argModifiers">The argument modifiers.</param>
        public VirtualGraphFieldArgument(
            ISchemaItem parent,
            string name,
            string internalName,
            GraphTypeExpression typeExpression,
            SchemaItemPath route,
            Type concreteType,
            bool hasDefaultValue,
            object defaultValue = null,
            GraphArgumentModifiers argModifiers = GraphArgumentModifiers.None)
        {
            this.Parent = Validation.ThrowIfNullOrReturn(parent, nameof(parent));
            this.ObjectType = Validation.ThrowIfNullOrReturn(concreteType, nameof(concreteType));
            this.InternalName = Validation.ThrowIfNullWhiteSpaceOrReturn(internalName, nameof(internalName));
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(name, nameof(name));
            this.Route = Validation.ThrowIfNullOrReturn(route, nameof(route));
            this.ParameterName = this.Name;
            this.TypeExpression = Validation.ThrowIfNullOrReturn(typeExpression, nameof(typeExpression));
            this.ArgumentModifiers = argModifiers;
            this.HasDefaultValue = hasDefaultValue;
            this.DefaultValue = defaultValue;

            this.AppliedDirectives = new AppliedDirectiveCollection(this);
        }

        /// <inheritdoc />
        public IGraphArgument Clone(ISchemaItem parent)
        {
            throw new NotImplementedException("Virtual graph arguments cannot be cloned");
        }

        /// <inheritdoc />
        public Type ObjectType { get; }

        /// <inheritdoc />
        public string InternalName { get; }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public object DefaultValue { get; }

        /// <inheritdoc />
        public GraphArgumentModifiers ArgumentModifiers { get; }

        /// <inheritdoc />
        public GraphTypeExpression TypeExpression { get; }

        /// <inheritdoc />
        public string ParameterName { get; }

        /// <inheritdoc />
        public IAppliedDirectiveCollection AppliedDirectives { get; }

        /// <inheritdoc />
        public bool HasDefaultValue { get; }

        /// <inheritdoc />
        public SchemaItemPath Route { get; }

        /// <inheritdoc />
        public ISchemaItem Parent { get; }
    }
}