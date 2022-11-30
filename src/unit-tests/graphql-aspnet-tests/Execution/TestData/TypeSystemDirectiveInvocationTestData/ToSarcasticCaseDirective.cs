// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TestData.TypeSystemDirectiveInvocationTestData
{
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class ToSarcasticCaseDirective : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.FIELD_DEFINITION)]
        public IGraphActionResult UpdateResolver()
        {
            // ensure we are working with a graph field definition
            var item = this.DirectiveTarget as IGraphField;
            if (item != null)
            {
                if (item.ObjectType != typeof(string))
                    throw new System.Exception("ONLY STRINGS!"); // - hulk

                // update the resolver to execute the orignal
                // resolver then upper case any string result
                var resolver = ExecutionExtensionMethods.Extend(item.Resolver, ConvertToSarcastic);
                item.UpdateResolver(resolver);
            }

            return this.Ok();
        }

        private static Task ConvertToSarcastic(FieldResolutionContext context, CancellationToken token)
        {
            if (context.Result is string)
            {
                var data = context.Result?.ToString();
                if (data.Length > 0)
                {
                    var builder = new StringBuilder();
                    for (var i = 0; i < data.Length; i++)
                    {
                        if ((i % 2) == 0)
                            builder.Append(data[i]);
                        else
                            builder.Append(data[i].ToString().ToUpper());
                    }

                    context.Result = builder.ToString();
                }
            }

            return Task.CompletedTask;
        }
    }
}