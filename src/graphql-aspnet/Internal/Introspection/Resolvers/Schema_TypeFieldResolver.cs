// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Introspection.Resolvers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Introspection.Model;
    using GraphQL.AspNet.Middleware.FieldExecution;

    /// <summary>
    /// A resolver for the '__type' meta field returning an <see cref="IntrospectedType"/> model item
    /// representing any given <see cref="IGraphType"/> on a schema.
    /// </summary>
    internal class Schema_TypeFieldResolver : IGraphFieldResolver
    {
        private readonly IntrospectedSchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="Schema_TypeFieldResolver"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public Schema_TypeFieldResolver(IntrospectedSchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <summary>
        /// Processes the given <see cref="IGraphFieldRequest" /> against this instance
        /// performing the operation as defined by this entity and generating a response.
        /// </summary>
        /// <param name="resolutionContext">The resolution context containing the request and the
        /// runtime information needed to resolve it.</param>
        /// <param name="cancelToken">The cancel token monitoring the execution of a graph request.</param>
        /// <returns>Task&lt;IGraphPipelineResponse&gt;.</returns>
        public Task Resolve(FieldResolutionContext resolutionContext, CancellationToken cancelToken = default)
        {
            if (!resolutionContext.Arguments.TryGetArgument<string>("name", out var name))
            {
                resolutionContext.Messages.Critical("Required Argument 'name' not found.");
            }
            else
            {
                var type = _schema.FindIntrospectedType(name);
                if (type == null || !type.Publish)
                {
                    resolutionContext.Messages.Info($"Unknown graph type. The type '{name}' does not exist on the '{_schema.Name}' schema");
                }
                else
                {
                    resolutionContext.Result = type;
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the concrete type this resolver attempts to create as a during its invocation. If this
        /// resolver may generate a list, this type should represent a single list item. (i.e. 'string' not 'List[string]').
        /// </summary>
        /// <value>The type of the return.</value>
        public Type ObjectType => typeof(IntrospectedType);
    }
}