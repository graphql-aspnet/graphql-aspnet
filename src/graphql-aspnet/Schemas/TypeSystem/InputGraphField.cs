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
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A standard field on any INPUT_OBJECT field.
    /// </summary>
    /// <seealso cref="IInputGraphField" />
    [DebuggerDisplay("Field: {Route.Path}")]
    public class InputGraphField : IInputGraphField
    {
        // *******************************************
        // implementation note:
        //
        // IsRequired here deviates from the input object field template (which keys off the [Required]
        // attribute).
        //
        // by definition (rule 5.6.4) a field is required if it is non-null and does not have a default value.
        // which is to say that all nullable fields are "not required" by the schema definition
        // *******************************************

        /// <summary>
        /// Initializes a new instance of the <see cref="InputGraphField" /> class.
        /// </summary>
        /// <param name="fieldName">Name of the field in the graph.</param>
        /// <param name="internalName">The internal name of this input field. Usually this is the property name, but it
        /// can be changed by the developer.</param>
        /// <param name="typeExpression">The meta data describing the type of data this field returns.</param>
        /// <param name="route">The formal route to this field in the object graph.</param>
        /// <param name="objectType">The .NET type of the item or items that represent the graph type returned by this field.</param>
        /// <param name="declaredPropertyName">The name of the property as it was declared on a <see cref="Type" /> (its internal name).</param>
        /// <param name="declaredReturnType">The .NET type as it was declared on the property which generated this field..</param>
        /// <param name="isRequired">if set to <c>true</c> this field was explicitly marked as being required being it has no
        /// explicitly declared default value. The value passsed on <paramref name="defaultValue" /> will be ignored. Note that
        /// the field will only truely be marked as required if it is has a non-nullable type expression.</param>
        /// <param name="defaultValue">When <paramref name="isRequired" /> is <c>false</c>, represents
        /// the value that should be used for this field when its not declared on a query document.</param>
        /// <param name="directives">The directives to apply to this field when its added to a schema.</param>
        public InputGraphField(
            string fieldName,
            string internalName,
            GraphTypeExpression typeExpression,
            SchemaItemPath route,
            Type objectType,
            string declaredPropertyName,
            Type declaredReturnType,
            bool isRequired,
            object defaultValue = null,
            IAppliedDirectiveCollection directives = null)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(fieldName, nameof(fieldName));
            this.InternalName = Validation.ThrowIfNullWhiteSpaceOrReturn(internalName, nameof(internalName));
            this.DeclaredName = Validation.ThrowIfNullWhiteSpaceOrReturn(declaredPropertyName, nameof(declaredPropertyName));
            this.TypeExpression = Validation.ThrowIfNullOrReturn(typeExpression, nameof(typeExpression));

            this.Route = Validation.ThrowIfNullOrReturn(route, nameof(route));
            this.ObjectType = Validation.ThrowIfNullOrReturn(objectType, nameof(objectType));
            this.DeclaredReturnType = Validation.ThrowIfNullOrReturn(declaredReturnType, nameof(declaredReturnType));

            this.AppliedDirectives = directives?.Clone(this) ?? new AppliedDirectiveCollection(this);

            this.HasDefaultValue = !isRequired;
            this.IsRequired = isRequired && this.TypeExpression.IsNonNullable;
            this.DefaultValue = defaultValue;
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

        /// <inheritdoc />
        public string InternalName { get; }

        /// <inheritdoc />
        public string DeclaredName { get; }

        /// <inheritdoc />
        public object DefaultValue { get; }

        /// <inheritdoc />
        public bool HasDefaultValue { get; }

        /// <inheritdoc />
        public bool IsRequired { get; }
    }
}