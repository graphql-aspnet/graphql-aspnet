// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A set of configurations that will be applied to any outgoing response to a graphql query made through
    /// this <see cref="ISchema" />.
    /// </summary>
    [DebuggerDisplay("Schema Response Configuration")]
    public class SchemaResponseConfiguration : ISchemaResponseConfiguration
    {
        /// <summary>
        /// Merges the specified configuration setttings into this instance.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public void Merge(ISchemaResponseConfiguration config)
        {
            if (config == null)
                return;

            this.IndentDocument = config.IndentDocument;
            this.ExposeExceptions = config.ExposeExceptions;
            this.ExposeMetrics = config.ExposeMetrics;
            this.AppendServerHeader = config.AppendServerHeader;
            this.MessageSeverityLevel = config.MessageSeverityLevel;
            this.TimeStampLocalizer = config.TimeStampLocalizer;
        }

        /// <summary>
        /// <para>
        /// Gets or sets a value indicating whether the output json document is indented with extra whitespace
        /// for readability (Default: true).
        /// </para>
        /// </summary>
        /// <value><c>true</c> if the document should be indented; otherwise, <c>false</c>.</value>
        public bool IndentDocument { get; set; } = true;

        /// <summary>
        /// <para>
        /// Gets or sets a value indicating whether internallly thrown exceptions should be be rendered
        /// as part of a query result for all user executed queries. (Default: false).
        /// </para>
        /// <para>NOTE: Enabling this in production could result in a security breach.</para>
        /// </summary>
        /// <value><c>true</c> if exceptions should be exposed; otherwise, <c>false</c>.</value>
        public bool ExposeExceptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether metrics, when enabled through, are delivered to the user as
        /// part of the graphql result. (Default: false).
        /// </summary>
        /// <value><c>true</c> if metrics should be exposed to the requestor; otherwise, <c>false</c>.</value>
        public bool ExposeMetrics { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether graphql-aspnet server information header are appended to
        /// an outgoing response (Default: true).
        /// </summary>
        /// <value><c>true</c> if the server information header should be added; otherwise, <c>false</c>.</value>
        public bool AppendServerHeader { get; set; } = true;

        /// <summary>
        /// Gets or sets the minimum severity level a generated message must be for it to be
        /// transmitted to the requestor. Critical messages will always be delivered regardless of this setting.
        /// (Default: Information).
        /// </summary>
        /// <value>The minimum message severity level to transmit to the requestor. All message severities with a value
        /// lower than this will be ignored.</value>
        public GraphMessageSeverity MessageSeverityLevel { get; set; } = GraphMessageSeverity.Information;

        /// <summary>
        /// <para>Gets or sets a factory method for performing any offset calculations, if needed, to any timestamps on messages
        /// generated during a query execution. Messages are generated and added to a query result in 'UTC-0' time.</para>
        /// <para>When a response is written back to a client each timestamp will be processed through this factory
        /// to allow for localization, if required.</para>
        /// <para>(Default: Timestamps are unaltered and delivered in UTC-0).</para>
        /// </summary>
        /// <value>The time stamp generator.</value>
        public Func<DateTimeOffset, DateTime> TimeStampLocalizer { get; set; } = (timeStamp) => timeStamp.DateTime;
    }
}