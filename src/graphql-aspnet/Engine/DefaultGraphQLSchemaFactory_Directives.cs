// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System.Linq;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// The default schema factory, capable of creating singleton instances of
    /// schemas, fully populated and ready to serve requests.
    /// </summary>
    public partial class DefaultGraphQLSchemaFactory<TSchema>
    {
        /// <summary>
        /// Inspects all graph types, fields, arguments and directives for any pending
        /// type system directives. When found, applies each directive as approprate to the
        /// target schema item.
        /// </summary>
        /// <returns>The total number of type system directives across the entire schema.</returns>
        protected virtual int ApplyTypeSystemDirectives()
        {
            var processor = new DirectiveProcessorTypeSystem<TSchema>(
                this.ServiceProvider,
                new QuerySession());

            return processor.ApplyDirectives(this.Schema);
        }
    }
}