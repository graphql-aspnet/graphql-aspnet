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
    using System.Collections.Generic;

    /// <summary>
    /// A DTO for serializing and transfering subscription related schema information known to
    /// a subscription server.
    /// </summary>
    public class SubscriptionOperationDTO
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionOperationDTO"/> class.
        /// </summary>
        public SubscriptionOperationDTO()
        {
            this.Fields = new List<SubscriptionFieldDTO>();
        }

        /// <summary>
        /// Gets or sets the friendly name of the target schema.
        /// </summary>
        /// <value>The name of the schema.</value>
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the target schema.
        /// </summary>
        /// <value>The schema identifier.</value>
        public string SchemaId { get; set; }

        /// <summary>
        /// Gets or sets the collection of top level fields supported by the subscription operation
        /// for a schema.
        /// </summary>
        /// <value>The fields.</value>
        public List<SubscriptionFieldDTO> Fields { get; set; }
    }
}