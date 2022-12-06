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
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class ToUpperCaseExecutionDirective : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.FIELD)]
        public IGraphActionResult UpdateResolver()
        {
            var fieldPart = this.DirectiveTarget as IFieldDocumentPart;
            if (fieldPart != null)
            {
                if (fieldPart.Field?.ObjectType != typeof(string))
                    throw new System.Exception("ONLY STRINGS!"); // - hulk

                // update the resolver used by the request
                // resolver then upper case any string result
                fieldPart.PostResolver = ConvertToUpper;
            }

            return this.Ok();
        }

        private static Task ConvertToUpper(FieldResolutionContext context, CancellationToken token)
        {
            var typeName = context.Result?.GetType().FriendlyName();
            if (context.Result is string)
                context.Result = context.Result?.ToString().ToUpperInvariant();

            return Task.CompletedTask;
        }
    }
}