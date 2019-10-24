// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Configuration
{
    using System;
    using GraphQL.AspNet.Execution;

    /// <summary>
    /// A set of configurations that will be applied to any outgoing response to a graphql query made through
    /// this schema.
    /// </summary>
    public interface ISchemaResponseConfiguration
    {
        /// <summary>
        /// <para>
        /// Gets a value indicating whether the output json document is indented with extra whitespace
        /// for readability (Default: true).
        /// </para>
        /// </summary>
        /// <value><c>true</c> if the document should be indented; otherwise, <c>false</c>.</value>
        bool IndentDocument { get; }

        /// <summary>
        /// <para>
        /// Gets a value indicating whether internallly thrown exceptions should be be rendered
        /// as part of a query result for all user executed queries. (Default: false).
        /// </para>
        /// <para>NOTE: Enabling this in production could result in a security breach.</para>
        /// </summary>
        /// <value><c>true</c> if exceptions should be exposed; otherwise, <c>false</c>.</value>
        bool ExposeExceptions { get; }

        /// <summary>
        /// Gets a value indicating whether metrics, when enabled, are delivered to the user as
        /// part of the graphql result. (Default: false).
        /// </summary>
        /// <value><c>true</c> if metrics should be exposed to the requestor; otherwise, <c>false</c>.</value>
        bool ExposeMetrics { get; }

        /// <summary>
        /// Gets a value indicating whether graphql-aspnet server information header are appended to
        /// an outgoing response (Default: true).
        /// </summary>
        /// <value><c>true</c> if the server information header should be added; otherwise, <c>false</c>.</value>
        bool AppendServerHeader { get; }

        /// <summary>
        /// <para>Gets the minimum severity level a generated message must be for it to be
        /// transmitted to the requestor. Critical messages will always be delivered regardless of this setting.</para>
        ///
        /// <para>(Default: Information).</para>
        /// </summary>
        /// <value>The minimum message severity level to transmit to the requestor. All message severities with a value
        /// lower than this will be ignored.</value>
        GraphMessageSeverity MessageSeverityLevel { get; }

        /// <summary>
        /// <para>Gets a factory method for performing any offset calculations, if needed, to any timestamps on messages
        /// generated during a query execution. Messages are generated and added to a query result in 'UTC-0' time.</para>
        /// <para>When a response is written back to a client each timestamp will be processed through this factory
        /// to allow for localization, if required.</para>
        /// <para>(Default: Timestamps are unaltered and delivered in UTC-0).</para>
        /// </summary>
        /// <value>The time stamp generator.</value>
        Func<DateTimeOffset, DateTime> TimeStampLocalizer { get; }
    }
}