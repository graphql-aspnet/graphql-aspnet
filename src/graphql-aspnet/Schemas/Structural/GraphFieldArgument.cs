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
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An argument defined on the object graph but not tied to any concrete item. It exists as a result
    /// of a programatic delcaration. The parameters of action methods on <see cref="GraphController"/> are generally mapped
    /// into a <see cref="GraphFieldArgument"/> for purposes of mapping, data coersion and introspection.
    /// </summary>
    [DebuggerDisplay("Argument: {Name}")]
    public class GraphFieldArgument : IGraphArgument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldArgument" /> class.
        /// </summary>
        /// <param name="parent">The parent schema item that owns this argument.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="typeExpression">The type expression.</param>
        /// <param name="route">The route path that identifies this argument.</param>
        /// <param name="modifiers">The modifiers.</param>
        /// <param name="parameterName">Name of the parameter as it is declared in the source code.</param>
        /// <param name="internalname">The fully qualified internal name identifiying this argument.</param>
        /// <param name="objectType">The concrete type of the object representing this argument.</param>
        /// <param name="hasDefaultValue">if set to <c>true</c> indicates that this
        /// argument has a default value assigned, even if that argument is <c>null</c>.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="description">The description about this argument.</param>
        /// <param name="directives">The directives to apply to this argument
        /// when its added to a schema.</param>
        public GraphFieldArgument(
            ISchemaItem parent,
            string argumentName,
            GraphTypeExpression typeExpression,
            GraphFieldPath route,
            GraphArgumentModifiers modifiers,
            string parameterName,
            string internalname,
            Type objectType,
            bool hasDefaultValue,
            object defaultValue = null,
            string description = null,
            IAppliedDirectiveCollection directives = null)
        {
            this.Parent = Validation.ThrowIfNullOrReturn(parent, nameof(parent));
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(argumentName, nameof(argumentName));
            this.Route = Validation.ThrowIfNullOrReturn(route, nameof(route));
            this.InternalName = Validation.ThrowIfNullWhiteSpaceOrReturn(internalname, nameof(internalname));
            this.ParameterName = Validation.ThrowIfNullWhiteSpaceOrReturn(parameterName, nameof(parameterName));
            this.TypeExpression = Validation.ThrowIfNullOrReturn(typeExpression, nameof(typeExpression));
            this.ObjectType = Validation.ThrowIfNullOrReturn(objectType, nameof(objectType));
            this.ArgumentModifiers = modifiers;
            this.HasDefaultValue = hasDefaultValue;
            this.DefaultValue = defaultValue;
            this.Description = description?.Trim();

            this.AppliedDirectives = directives?.Clone(this) ?? new AppliedDirectiveCollection(this);
        }

        /// <inheritdoc />
        public IGraphArgument Clone(ISchemaItem parent)
        {
            Validation.ThrowIfNull(parent, nameof(parent));
            return new GraphFieldArgument(
                parent,
                this.Name,
                this.TypeExpression,
                parent.Route.CreateChild(this.Name),
                this.ArgumentModifiers,
                this.ParameterName,
                this.InternalName,
                this.ObjectType,
                this.HasDefaultValue,
                this.DefaultValue,
                this.Description,
                this.AppliedDirectives);
        }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public GraphArgumentModifiers ArgumentModifiers { get; }

        /// <inheritdoc />
        public GraphTypeExpression TypeExpression { get; }

        /// <inheritdoc />
        public object DefaultValue { get; }

        /// <inheritdoc />
        public string ParameterName { get; }

        /// <inheritdoc />
        public Type ObjectType { get; }

        /// <inheritdoc />
        public string InternalName { get; }

        /// <inheritdoc />
        public IAppliedDirectiveCollection AppliedDirectives { get; }

        /// <inheritdoc />
        public bool HasDefaultValue { get; }

        /// <inheritdoc />
        public GraphFieldPath Route { get; }

        /// <inheritdoc />
        public ISchemaItem Parent { get; }
    }
}