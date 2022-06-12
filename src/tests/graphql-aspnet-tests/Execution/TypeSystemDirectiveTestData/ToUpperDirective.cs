// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveTestData
{
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class ToUpperDirective : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.FIELD_DEFINITION)]
        public IGraphActionResult UpdateResolver()
        {
            // ensure we are working with a graph field definition
            var item = this.DirectiveTarget as IGraphField;
            if (item != null)
            {
                if (item.ObjectType != typeof(string))
                    throw new System.Exception("ONLY STRINGS, NO STAIRS!"); // - hulk

                // update the resolver to execute the orignal
                // resolver then upper case any string result
                var resolver = item.Resolver.Extend(ConvertToupper);
                item.UpdateResolver(resolver);
            }

            return this.Ok();
        }

        private static Task ConvertToupper(FieldResolutionContext context, CancellationToken token)
        {
            if (context.Result is string)
                context.Result = context.Result?.ToString()?.ToUpper();

            return Task.CompletedTask;
        }
    }
}