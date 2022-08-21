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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A standard field on any INPUT_OBJECT field.
    /// </summary>
    /// <seealso cref="IInputGraphField" />
    [DebuggerDisplay("Field: {Route.Path}")]
    public class InputGraphField : IInputGraphField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputGraphField" /> class.
        /// </summary>
        /// <param name="fieldName">Name of the field in the graph.</param>
        /// <param name="typeExpression">The meta data describing the type of data this field returns.</param>
        /// <param name="route">The formal route to this field in the object graph.</param>
        /// <param name="declaredPropertyName">The name of the property as it was declared on a <see cref="Type" /> (its internal name).</param>
        /// <param name="objectType">The .NET type of the item or items that represent the graph type returned by this field.</param>
        /// <param name="declaredReturnType">The .NET type as it was declared on the property which generated this field..</param>
        /// <param name="isRequired">if set to <c>true</c> this field was explicitly marked as being required.</param>
        /// <param name="directives">The directives to apply to this field when its added to a schema.</param>
        public InputGraphField(
            string fieldName,
            GraphTypeExpression typeExpression,
            SchemaItemPath route,
            string declaredPropertyName,
            Type objectType,
            Type declaredReturnType,
            bool isRequired,
            IAppliedDirectiveCollection directives = null)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(fieldName, nameof(fieldName));
            this.TypeExpression = Validation.ThrowIfNullOrReturn(typeExpression, nameof(typeExpression));

            this.Route = Validation.ThrowIfNullOrReturn(route, nameof(route));
            this.ObjectType = Validation.ThrowIfNullOrReturn(objectType, nameof(objectType));
            this.DeclaredReturnType = Validation.ThrowIfNullOrReturn(declaredReturnType, nameof(declaredReturnType));

            this.AppliedDirectives = directives?.Clone(this) ?? new AppliedDirectiveCollection(this);

            this.InternalName = declaredPropertyName;
            this.IsRequired = isRequired;
            this.Publish = true;
        }

        /// <inheritdoc />
        public void AssignParent(IGraphType parent)
        {
            Validation.ThrowIfNull(parent, nameof(parent));
            this.Parent = parent;
        }

        /// <inheritdoc />
        public GraphTypeExpression TypeExpression { get; }

        /// <inheritdoc />
        public Type ObjectType { get; }

        /// <inheritdoc />
        public Type DeclaredReturnType { get; }

        /// <inheritdoc />
        public ISchemaItem Parent { get; private set; }

        /// <inheritdoc />
        public SchemaItemPath Route { get; }

        /// <inheritdoc />
        public IAppliedDirectiveCollection AppliedDirectives { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public bool Publish { get; set; }

        /// <summary>
        /// Gets a fully qualified name of the type as it exists on the server (i.e.  Namespace.ClassName.PropertyName). This name
        /// is used in many exceptions and internal error messages.
        /// </summary>
        /// <value>The fully qualified name of the proeprty this field was created from.</value>
        public string InternalName { get; }

        /// <inheritdoc />
        public bool IsRequired { get; }
    }
}