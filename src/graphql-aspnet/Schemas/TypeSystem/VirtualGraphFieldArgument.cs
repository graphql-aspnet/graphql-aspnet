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
        /// <param name="internalFullName">The fully qualified name of this field as it exists in the .NET code.</param>
        /// <param name="typeExpression">The graph type expression representing this field.</param>
        /// <param name="route">The route path for this argument.</param>
        /// <param name="concreteType">The concrete graph type in the server code that this argument is mapped to.</param>
        /// <param name="hasDefaultValue">if set to <c>true</c> indicates that this
        /// argument has a default value, even if its null.</param>
        /// <param name="defaultValue">The default value of this argument when not supplied, if any.</param>
        public VirtualGraphFieldArgument(
            ISchemaItem parent,
            string name,
            string internalFullName,
            GraphTypeExpression typeExpression,
            SchemaItemPath route,
            Type concreteType,
            bool hasDefaultValue,
            object defaultValue = null)
        {
            this.Parent = Validation.ThrowIfNullOrReturn(parent, nameof(parent));
            this.ObjectType = Validation.ThrowIfNullOrReturn(concreteType, nameof(concreteType));
            this.InternalName = Validation.ThrowIfNullWhiteSpaceOrReturn(internalFullName, nameof(internalFullName));
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(name, nameof(name));
            this.Route = Validation.ThrowIfNullOrReturn(route, nameof(route));
            this.ParameterName = this.Name;
            this.TypeExpression = Validation.ThrowIfNullOrReturn(typeExpression, nameof(typeExpression));

            // by definition (rule 5.4.2.1) a nullable type expression on an argument implies
            // an optional field. that is to say it has an implicit default value of 'null'
            this.HasDefaultValue = hasDefaultValue || this.TypeExpression.IsNullable;
            this.DefaultValue = defaultValue;

            this.AppliedDirectives = new AppliedDirectiveCollection(this);
        }

        /// <inheritdoc />
        public IGraphArgument Clone(ISchemaItem parent = null, string argumentName = null, GraphTypeExpression typeExpression = null)
        {
            parent = parent ?? this.Parent;

            argumentName = argumentName?.Trim() ?? this.Name;

            var parentRoute = parent?.Route ?? this.Route.Parent;
            var route = parentRoute.CreateChild(argumentName);

            var clonedItem = new VirtualGraphFieldArgument(
                parent,
                argumentName,
                this.InternalName,
                typeExpression ?? this.TypeExpression.Clone(),
                route,
                this.ObjectType,
                this.HasDefaultValue,
                this.DefaultValue);

            clonedItem.Description = this.Description;

            return clonedItem;
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

        /// <inheritdoc />
        public bool IsRequired => !this.HasDefaultValue;
    }
}