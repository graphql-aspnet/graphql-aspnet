// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Directives.DirectiveTestData
{
    using System;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class FieldSwapDirective : GraphDirective
    {
        [DirectiveLocations(AspNet.Schemas.TypeSystem.DirectiveLocation.FIELD)]
        public IGraphActionResult Execute()
        {
            // randomly ineject an argument into the active part
            if (this.DirectiveTarget is IFieldDocumentPart fp && fp.Name.ToString() == "property2")
            {
                var type2 = this.Schema.KnownTypes.FindGraphType(typeof(TwoPropertyObjectV2)) as IObjectGraphType;
                var type2Prop2 = type2?.Fields.FindField("property2");
                if (type2Prop2 != null)
                    fp.Field = type2Prop2;
            }

            return this.Ok();
        }
    }
}