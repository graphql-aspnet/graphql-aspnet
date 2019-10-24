// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.Common
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.Logging;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A structured graphql log entry that generates a common output format for handoff to a
    /// logging framement. This log entry is typically rendered to a JSON string.
    /// </summary>
    [DebuggerDisplay("{EventId}")]
    public class GraphLogEntry : GraphLogPropertyCollection, IGraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphLogEntry" /> class.
        /// </summary>
        /// <param name="message">The human-friendly message to add to the log entry, if any.</param>
        public GraphLogEntry(string message = null)
            : this(LogEventIds.General, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphLogEntry" /> class.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="message">The human-friendly message to add to the log entry, if any.</param>
        public GraphLogEntry(EventId eventId, string message = null)
             : this(eventId.Id, message)
        {
            if (!string.IsNullOrEmpty(eventId.Name))
                this.EventName = eventId.Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphLogEntry" /> class.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="message">The human-friendly message to add to the log entry, if any.</param>
        public GraphLogEntry(int eventId, string message = null)
        {
            this.EventId = eventId;
            this.DateTimeUTC = DateTimeOffset.UtcNow;
            this.LogEntryId = Guid.NewGuid().ToString("N");
            if (!string.IsNullOrWhiteSpace(message))
            {
                this.Message = message;
            }
        }

        /// <summary>
        /// Gets the unique id of this log entry.
        /// </summary>
        /// <value>The message.</value>
        public string LogEntryId
        {
            get => this.GetProperty<string>(LogPropertyNames.LOG_ENTRY_ID);
            private set => this.SetProperty(LogPropertyNames.LOG_ENTRY_ID, value);
        }

        /// <summary>
        /// Gets the date and time (in UTC-0) when this log entry was first instantiated.
        /// </summary>
        /// <value>The message.</value>
        public DateTimeOffset DateTimeUTC
        {
            get => this.GetProperty<DateTimeOffset>(LogPropertyNames.DATE_TIME_UTC);
            private set => this.SetProperty(LogPropertyNames.DATE_TIME_UTC, value);
        }

        /// <summary>
        /// Gets or sets the human-friendly message attached to this log entry.
        /// </summary>
        /// <value>The message.</value>
        public string Message
        {
            get => this.GetProperty<string>(LogPropertyNames.MESSAGE);
            set => this.SetProperty(LogPropertyNames.MESSAGE, value);
        }

        /// <summary>
        /// Gets or sets the log event type of this log entry.
        /// </summary>
        /// <value>The message.</value>
        public int EventId
        {
            get => this.GetProperty<int>(LogPropertyNames.EVENT_ID);
            set => this.SetProperty(LogPropertyNames.EVENT_ID, value);
        }

        /// <summary>
        /// Gets or sets the name of the event this entry relates to.
        /// </summary>
        /// <value>The name of the event.</value>
        public string EventName
        {
            get => this.GetProperty<string>(LogPropertyNames.EVENT_NAME);
            set => this.SetProperty(LogPropertyNames.EVENT_NAME, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            if (this.Message != null)
                return this.Message;

            if (this.EventName != null)
                return $"{this.EventName} (Id: {this.EventId})";

            return $"Id: {this.EventId}";
        }
    }
}