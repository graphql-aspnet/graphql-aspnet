// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Logging
{
    using System;

    /// <summary>
    /// An interface describing a log entry that needs to be serialized for the log provider. Simply a wrapper
    /// on a collection of KeyValuePairs with a few required fields.
    /// </summary>
    public interface IGraphLogEntry : IGraphLogPropertyCollection
    {
        /// <summary>
        /// Gets the unique id of this log entry.
        /// </summary>
        /// <value>The message.</value>
        string LogEntryId { get; }

        /// <summary>
        /// Gets the date and time (in UTC-0) when this log entry was first instantiated.
        /// </summary>
        /// <value>The message.</value>
        DateTimeOffset DateTimeUTC { get; }

        /// <summary>
        /// Gets or sets the human-friendly message attached to this log entry.
        /// </summary>
        /// <value>The message.</value>
        string Message { get; set; }

         /// <summary>
        /// Gets or sets the log event type of this log entry.
        /// </summary>
        /// <value>The message.</value>
        int EventId { get; set; }
    }
}