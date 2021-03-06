﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Structural
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.Schema.TypeSystem;

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
        /// <param name="route">The formal route to this field in the object graph.</param>
        /// <param name="mode">The execution mode of this field.</param>
        /// <param name="resolver">The resolver.</param>
        /// <param name="securityPolicies">The security policies.</param>
        /// <param name="eventName">Alterante name of the event that has been assigned to this field.</param>
        public SubscriptionMethodGraphField(
            string fieldName,
            GraphTypeExpression typeExpression,
            GraphFieldPath route,
            Execution.FieldResolutionMode mode = Execution.FieldResolutionMode.PerSourceItem,
            Interfaces.Execution.IGraphFieldResolver resolver = null,
            IEnumerable<Security.FieldSecurityGroup> securityPolicies = null,
            string eventName = null)
            : base(fieldName, typeExpression, route, mode, resolver, securityPolicies)
        {
            this.EventName = eventName;
        }

        /// <summary>
        /// Gets the alternate event name used to identify this subscription field via internal eventing.
        /// </summary>
        /// <value>The name of the event.</value>
        public string EventName { get; }
    }
}