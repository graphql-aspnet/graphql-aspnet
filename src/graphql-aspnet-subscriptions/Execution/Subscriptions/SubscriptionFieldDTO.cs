// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Subscriptions
{
    using System;

    /// <summary>
    /// A DTO describing a single top-level field of a subscription operation on a schema.
    /// </summary>
    public class SubscriptionFieldDTO
    {
        /// <summary>
        /// Gets or sets the internal name of the event tied to the field represented
        /// by this DTO.
        /// </summary>
        /// <value>The name of the event.</value>
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the name of the field as its defined on the subscription operation.
        /// </summary>
        /// <value>The name of the field.</value>
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets the type of data returned by the field.
        /// </summary>
        /// <value>The type of the return.</value>
        public Type ReturnType { get; set; }
    }
}