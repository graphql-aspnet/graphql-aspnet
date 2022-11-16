// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.ExecutionDirectiveTestData
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Schemas.TypeSystem;

    [GraphType("toSarcasticCase")]
    public class ToSarcasticCaseExecutionDirective : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.FIELD)]
        public IGraphActionResult Execute(bool startOnLowerCase = true)
        {
            var fieldPart = this.DirectiveTarget as IFieldDocumentPart;
            if (fieldPart != null)
            {
                if (fieldPart.Field?.ObjectType != typeof(string))
                    throw new Exception("ONLY STRINGS!"); // - hulk

                fieldPart.PostResolver = CreateResolver(startOnLowerCase);
            }

            return this.Ok();
        }

        private static Func<FieldResolutionContext, CancellationToken, Task> CreateResolver(bool startOnLowerCase)
        {
            return (FieldResolutionContext context, CancellationToken token) =>
            {
                if (context.Result != null && context.Result is string)
                {
                    // for a single string leaf field convert and return
                    var data = context.Result.ToString();
                    var builder = new StringBuilder();

                    var oddOrEven = startOnLowerCase ? 0 : 1;
                    for (var i = 0; i < data.Length; i++)
                    {
                        if ((i % 2) == oddOrEven)
                            builder.Append(data[i].ToString().ToLowerInvariant());
                        else
                            builder.Append(data[i].ToString().ToUpperInvariant());
                    }

                    context.Result = builder.ToString();
                }

                return Task.CompletedTask;
            };
        }
    }
}