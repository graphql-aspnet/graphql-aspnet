// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Resolvers.Introspeection
{
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem.Introspection.Model;

    /// <summary>
    /// A resolver for the '__type' meta field returning an <see cref="IntrospectedType"/> model item
    /// representing any given <see cref="IGraphType"/> on a schema.
    /// </summary>
    internal class Schema_TypeGraphFieldResolver : IGraphFieldResolver
    {
        private readonly IntrospectedSchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="Schema_TypeGraphFieldResolver"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public Schema_TypeGraphFieldResolver(IntrospectedSchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));

            this.MetaData = InternalFieldResolverMetaData.CreateMetadata(this.GetType());
        }

        /// <inheritdoc />
        public Task ResolveAsync(FieldResolutionContext resolutionContext, CancellationToken cancelToken = default)
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

        /// <inheritdoc />
        public IGraphFieldResolverMetaData MetaData { get; }
    }
}