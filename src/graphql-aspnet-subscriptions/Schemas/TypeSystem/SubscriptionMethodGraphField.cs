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
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Schema.TypeSystem;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A <see cref="MethodGraphField"/> with additional properties related to subscriptions.
    /// </summary>
    public class SubscriptionMethodGraphField : MethodGraphField, ISubscriptionGraphField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionMethodGraphField" /> class.
        /// </summary>
        /// <param name="fieldName">Name of the field in the type declaration..</param>
        /// <param name="typeExpression">The meta data about how this type field is implemented.</param>
        /// <param name="itemPath">The formal path to this field in the object graph.</param>
        /// <param name="internalFullName">The fully qualified name of the method this field respresents, as it was declared
        /// in C# code.</param>
        /// <param name="declaredReturnType">The .NET type as it was declared on the property which generated this field..</param>
        /// <param name="objectType">The .NET type of the item or items that represent the graph type returned by this field.</param>
        /// <param name="mode">The execution mode of this field.</param>
        /// <param name="resolver">The resolver.</param>
        /// <param name="securityPolicies">The security policies applied to this field.</param>
        /// <param name="eventName">Alterante name of the event that has been assigned to this field.</param>
        /// <param name="directives">The directives to be applied to this field when its added to a schema.</param>
        public SubscriptionMethodGraphField(
            string fieldName,
            GraphTypeExpression typeExpression,
            ItemPath itemPath,
            string internalFullName,
            Type declaredReturnType = null,
            Type objectType = null,
            Execution.FieldResolutionMode mode = Execution.FieldResolutionMode.PerSourceItem,
            Interfaces.Execution.IGraphFieldResolver resolver = null,
            IEnumerable<AppliedSecurityPolicyGroup> securityPolicies = null,
            string eventName = null,
            IAppliedDirectiveCollection directives = null)
            : base(fieldName, internalFullName, typeExpression, itemPath, declaredReturnType, objectType, mode, resolver, securityPolicies, directives)
        {
            this.EventName = eventName;
        }

        /// <inheritdoc />
        protected override MethodGraphField CreateNewInstance()
        {
            return new SubscriptionMethodGraphField(
                this.Name,
                this.TypeExpression.Clone(),
                this.ItemPath.Clone(),
                this.InternalName,
                this.DeclaredReturnType,
                this.ObjectType,
                this.Mode,
                this.Resolver,
                this.SecurityGroups,
                this.EventName,
                this.AppliedDirectives);
        }

        /// <summary>
        /// Gets the alternate event name used to identify this subscription field via internal eventing.
        /// </summary>
        /// <value>The name of the event.</value>
        public string EventName { get; }
    }
}