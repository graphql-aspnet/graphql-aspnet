// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.ExecutionEvents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// Recorded when the startup services generate a new pipeline chain for the target schema.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema for which the pipeline was generated.</typeparam>
    public class SchemaPipelineRegisteredLogEntry<TSchema> : GraphLogEntry
        where TSchema : class, ISchema
    {
        private readonly string _schemaTypeShortName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaPipelineRegisteredLogEntry{TSchema}" /> class.
        /// </summary>
        /// <param name="pipelineInstance">The pipeline instance that was created.</param>
        public SchemaPipelineRegisteredLogEntry(ISchemaPipeline pipelineInstance)
            : base(LogEventIds.SchemaPipelineInstanceCreated)
        {
            _schemaTypeShortName = typeof(TSchema).FriendlyName();
            this.SchemaTypeName = typeof(TSchema).FriendlyName(true);
            this.PipelineName = pipelineInstance.Name;
            this.MiddlewareCount = pipelineInstance.MiddlewareComponentNames.Count;
            this.MiddlewareComponents = pipelineInstance.MiddlewareComponentNames.ToList();
        }

        /// <summary>
        /// Gets the <see cref="Type" /> name of the schema instance the pipeline was generated for.
        /// </summary>
        /// <value>The name of the schema type.</value>
        public string SchemaTypeName
        {
            get => this.GetProperty<string>(LogPropertyNames.SCHEMA_TYPE_NAME);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_TYPE_NAME, value);
        }

        /// <summary>
        /// Gets the name of the pipeline that was created.
        /// </summary>
        /// <value>The name of the pipeline.</value>
        public string PipelineName
        {
            get => this.GetProperty<string>(LogPropertyNames.SCHEMA_PIPELINE_NAME);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_PIPELINE_NAME, value);
        }

        /// <summary>
        /// Gets the number of middleware components registered to the pipeline instance.
        /// </summary>
        /// <value>The middleware count.</value>
        public int MiddlewareCount
        {
            get => this.GetProperty<int>(LogPropertyNames.SCHEMA_PIPELINE_MIDDLEWARE_COUNT);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_PIPELINE_MIDDLEWARE_COUNT, value);
        }

        /// <summary>
        /// Gets a collection of names, in execution order, of the components registered to the schema pipeline.
        /// </summary>
        /// <value>The pipeline compoents.</value>
        public IList<string> MiddlewareComponents
        {
            get => this.GetProperty<IList<string>>(LogPropertyNames.SCHEMA_PIPELINE_MIDDLEWARE_COMPOENNTS);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_PIPELINE_MIDDLEWARE_COMPOENNTS, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"Schema '{this.PipelineName} Pipeline' Created | Schema Type: '{_schemaTypeShortName}', Components: {this.MiddlewareCount}";
        }
    }
}