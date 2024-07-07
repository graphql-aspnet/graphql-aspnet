// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Source;

    /// <summary>
    /// An error of some kind that occured during the resolution of any graph item.
    /// </summary>
    public interface IGraphMessage
    {
        /// <summary>
        /// Gets the time stamp (in UTC-0) when this message was created.
        /// </summary>
        /// <remarks>
        /// This value is immutable once the message is created.
        /// </remarks>
        /// <value>The time stamp.</value>
        DateTimeOffset TimeStamp { get; }

        /// <summary>
        /// Gets the origin in the provided source text, if any, this message relates to.
        /// This value is returned as part of a query response.
        /// </summary>
        /// <remarks>
        /// This value is immutable once the message is created.
        /// </remarks>
        /// <value>The location.</value>
        SourceOrigin Origin { get; }

        /// <summary>
        /// Gets or sets a standardized error code identifying this error. This value is returned as part of a query response.
        /// </summary>
        /// <value>The code that can idenify this message or message type to a user.</value>
        string Code { get; set; }

        /// <summary>
        /// Gets or sets a human-friendly message that conveys details about the error tht occured. This value is
        /// returned as part of a query response.
        /// </summary>
        /// <value>The message contents.</value>
        string Message { get; set; }

        /// <summary>
        /// Gets or sets an (optional) exception that may have occured to generate the error. The exception
        /// is only conveyed to the requestor if the request is configured to expose exceptions.
        /// </summary>
        /// <value>The exception that was originally thrown.</value>
        Exception Exception { get; set; }

        /// <summary>
        /// Gets the severity of this message that was generated.
        /// </summary>
        /// <remarks>
        /// This value is immutable once the message is created.
        /// </remarks>
        /// <value>The severity of the message.</value>
        GraphMessageSeverity Severity { get; }

        /// <summary>
        /// Gets additional metadata defined for this message. This data will be added as key/value pairs
        /// when the message is rendered to an graph output.
        /// </summary>
        /// <value>The meta data.</value>
        IDictionary<string, object> MetaData { get; }
    }
}