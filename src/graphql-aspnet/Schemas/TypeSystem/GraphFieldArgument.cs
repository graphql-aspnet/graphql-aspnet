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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// An argument defined on the object graph. This object is created as a result
    /// of a programatic delcaration. The parameters of action methods on <see cref="GraphController"/> and methods
    /// registered as fields on POCOs are generally mapped into a <see cref="GraphFieldArgument"/> for purposes of mapping,
    /// data coersion and introspection.
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
        /// <param name="parameterName">Name of the parameter as it is declared in the source code.</param>
        /// <param name="internalFullName">The fully qualified internal name identifiying this argument.</param>
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
            SchemaItemPath route,
            string parameterName,
            string internalFullName,
            Type objectType,
            bool hasDefaultValue,
            object defaultValue = null,
            string description = null,
            IAppliedDirectiveCollection directives = null)
        {
            this.Parent = Validation.ThrowIfNullOrReturn(parent, nameof(parent));
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(argumentName, nameof(argumentName));
            this.Route = Validation.ThrowIfNullOrReturn(route, nameof(route));
            this.InternalFullName = Validation.ThrowIfNullWhiteSpaceOrReturn(internalFullName, nameof(internalFullName));
            this.ParameterName = Validation.ThrowIfNullWhiteSpaceOrReturn(parameterName, nameof(parameterName));
            this.TypeExpression = Validation.ThrowIfNullOrReturn(typeExpression, nameof(typeExpression));
            this.ObjectType = Validation.ThrowIfNullOrReturn(objectType, nameof(objectType));

            // by definition (rule 5.4.2.1) a nullable type expression on an argument implies
            // an optional field. that is to say it has an implicit default value of 'null'
            this.HasDefaultValue = hasDefaultValue;
            this.IsRequired = !hasDefaultValue && this.TypeExpression.IsNonNullable;
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
                this.TypeExpression.Clone(),
                parent.Route.CreateChild(this.Name),
                this.ParameterName,
                this.InternalFullName,
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
        public GraphTypeExpression TypeExpression { get; }

        /// <inheritdoc />
        public object DefaultValue { get; }

        /// <inheritdoc />
        public string ParameterName { get; }

        /// <inheritdoc />
        public Type ObjectType { get; }

        /// <inheritdoc />
        public string InternalFullName { get; }

        /// <inheritdoc />
        public IAppliedDirectiveCollection AppliedDirectives { get; }

        /// <inheritdoc />
        public bool HasDefaultValue { get; }

        /// <inheritdoc />
        public SchemaItemPath Route { get; }

        /// <inheritdoc />
        public ISchemaItem Parent { get; }

        /// <inheritdoc />
        public bool IsRequired { get; }
    }
}