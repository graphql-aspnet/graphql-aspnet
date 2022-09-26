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
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A representation of a field as it would be defined in an object graph that originated
    /// from a .NET property getter.
    /// </summary>
    public class PropertyGraphField : MethodGraphField, ITypedSchemaItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGraphField" /> class.
        /// </summary>
        /// <param name="fieldName">Name of the field in the public graph.</param>
        /// <param name="typeExpression">The type expression declaring what type of data this field returns.</param>
        /// <param name="route">The route to this field in the graph.</param>
        /// <param name="declaredPropertyName">The name of the property as it was declared on the <see cref="Type" /> (its internal name).</param>
        /// <param name="objectType">The .NET type of the item or items that represent the graph type returned by this field.</param>
        /// <param name="declaredReturnType">The .NET type as it was declared on the property which generated this field..</param>
        /// <param name="mode">The mode in which the runtime will process this field.</param>
        /// <param name="resolver">The resolver to be invoked to produce data when this field is called.</param>
        /// <param name="securityPolicies">The security policies that apply to this field.</param>
        /// <param name="directives">The directives to apply to this field when its added to a schema.</param>
        public PropertyGraphField(
            string fieldName,
            GraphTypeExpression typeExpression,
            SchemaItemPath route,
            string declaredPropertyName,
            Type objectType = null,
            Type declaredReturnType = null,
            FieldResolutionMode mode = FieldResolutionMode.PerSourceItem,
            IGraphFieldResolver resolver = null,
            IEnumerable<AppliedSecurityPolicyGroup> securityPolicies = null,
            IAppliedDirectiveCollection directives = null)
            : base(fieldName, typeExpression, route, objectType, declaredReturnType, mode, resolver, securityPolicies, directives)
        {
            this.InternalName = declaredPropertyName;
        }

        /// <summary>
        /// Creates a new instance of a graph field from this type.
        /// </summary>
        /// <param name="parent">The item to assign as the parent of the new field.</param>
        /// <returns>IGraphField.</returns>
        protected override IGraphField CreateNewInstance(IGraphType parent)
        {
            return new PropertyGraphField(
                this.Name,
                this.TypeExpression,
                parent.Route.CreateChild(this.Name),
                this.InternalName,
                this.ObjectType,
                this.DeclaredReturnType,
                this.Mode,
                this.Resolver,
                this.SecurityGroups,
                this.AppliedDirectives);
        }

        /// <summary>
        /// Gets a fully qualified name of the type as it exists on the server (i.e.  Namespace.ClassName.PropertyName). This name
        /// is used in many exceptions and internal error messages.
        /// </summary>
        /// <value>The fully qualified name of the proeprty this field was created from.</value>
        public string InternalName { get; }
    }
}