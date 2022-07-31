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
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class AdjustBatchDataDirective : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.FIELD)]
        public IGraphActionResult UpdateResolver()
        {
            var fieldPart = this.DirectiveTarget as IFieldDocumentPart;
            if (fieldPart != null)
            {
                if (fieldPart.Field?.ObjectType != typeof(TwoPropertyObject))
                    throw new System.Exception();

                // update the resolver used by the request
                // resolver then upper case any string result
                fieldPart.PostProcessor = AdjustTwoPropData;
            }

            return this.Ok();
        }

        private static Task AdjustTwoPropData(FieldResolutionContext context, CancellationToken token)
        {
            if (context.Result is IDictionary<TwoPropertyObject, object> dic)
            {
                foreach (var kvp in dic)
                {
                    var item = kvp.Value as TwoPropertyObject;
                    if (item != null)
                        item.Property1 = item.Property1?.ToUpperInvariant();
                }
            }

            return Task.CompletedTask;
        }
    }
}