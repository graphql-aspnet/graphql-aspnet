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

        /// <inheritdoc />
        public bool IndentDocument { get; set; } = true;

        /// <inheritdoc />
        public bool ExposeExceptions { get; set; }

        /// <inheritdoc />
        public bool ExposeMetrics { get; set; }

        /// <inheritdoc />
        public bool AppendServerHeader { get; set; } = true;

        /// <inheritdoc />
        public GraphMessageSeverity MessageSeverityLevel { get; set; } = GraphMessageSeverity.Information;

        /// <inheritdoc />
        public Func<DateTimeOffset, DateTime> TimeStampLocalizer { get; set; } = (timeStamp) => timeStamp.DateTime;
    }
}