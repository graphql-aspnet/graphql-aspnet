// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.ExecutionDirectiveTestData
{
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class ToUpperCaseExecutionDirective : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.FIELD)]
        public IGraphActionResult UpdateResolver()
        {
            // ensure we are working with a graph field definition
            //var item = this.DirectiveTarget as IFieldDocumentPart;
            //if (item != null)
            //{
            //    if (item.Field?.ObjectType != typeof(string))
            //        throw new System.Exception("ONLY STRINGS!"); // - hulk

            //    // update the resolver used by the request
            //    // resolver then upper case any string result
            //    var clonedField = item.Field.Clone();
            //    var resolver = clonedField.Resolver.Extend(ConvertToUpper);
            //    clonedField.UpdateResolver(resolver);
            //    item.Field = clonedField;
            //}

            return this.Ok();
        }

        private static Task ConvertToUpper(FieldResolutionContext context, CancellationToken token)
        {
            if (context.Result is string)
                context.Result = context.Result?.ToString().ToLowerInvariant();

            return Task.CompletedTask;
        }
    }
}