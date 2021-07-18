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
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A representation of a object field as it would be defined in the graph type system. Contains
    /// special logic for handling graph fields generating from object properties.
    /// </summary>
    public class PropertyGraphField : MethodGraphField, ITypedItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGraphField" /> class.
        /// </summary>
        /// <param name="fieldName">Name of the field in the public graph.</param>
        /// <param name="typeExpression">The type expression declaring what type of data this field returns.</param>
        /// <param name="route">The route to this field in the graph.</param>
        /// <param name="propertyType">The property metadata.</param>
        /// <param name="propertyDeclaredName">The name of the property as it was declared on the <see cref="Type"/> (its internal name).</param>
        /// <param name="mode">The mode in which the runtime will process this field.</param>
        /// <param name="resolver">The resolver to be invoked to produce data when this field is called.</param>
        /// <param name="securityPolicies">The security policies that apply to this field.</param>
        public PropertyGraphField(
            string fieldName,
            GraphTypeExpression typeExpression,
            GraphFieldPath route,
            Type propertyType,
            string propertyDeclaredName,
            FieldResolutionMode mode = FieldResolutionMode.PerSourceItem,
            IGraphFieldResolver resolver = null,
            IEnumerable<FieldSecurityGroup> securityPolicies = null)
            : base(fieldName, typeExpression, route, mode, resolver, securityPolicies)
        {
            this.ObjectType = propertyType;
            this.InternalName = propertyDeclaredName;
        }

        /// <summary>
        /// Gets the type of the object this graph type was made from.
        /// </summary>
        /// <value>The type of the object.</value>
        public Type ObjectType { get; }

        /// <summary>
        /// Gets a fully qualified name of the type as it exists on the server (i.e.  Namespace.ClassName). This name
        /// is used in many exceptions and internal error messages.
        /// </summary>
        /// <value>The name of the internal.</value>
        public string InternalName { get; }
    }
}