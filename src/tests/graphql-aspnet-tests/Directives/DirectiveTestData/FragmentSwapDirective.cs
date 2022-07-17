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
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class FragmentSwapDirective : GraphDirective
    {
        [DirectiveLocations(AspNet.Schemas.TypeSystem.DirectiveLocation.FRAGMENT_SPREAD)]
        public IGraphActionResult Execute()
        {
            // randomly ineject an argument into the active part
            if (this.DirectiveTarget is IFragmentSpreadDocumentPart fp && fp.FragmentName.ToString() == "frag1")
            {
                var part = fp as IDocumentPart;
                IGraphQueryDocument topDoc = null;
                while (topDoc == null && part.Parent != null)
                {
                    if (part.Parent is IGraphQueryDocument gqp)
                    {
                        topDoc = gqp;
                        break;
                    }

                    part = part.Parent;
                }

                if(topDoc != null)
                {
                    var frag2 = topDoc.NamedFragments["frag2"];
                    fp.Fragment = frag2;
                }
            }

            return this.Ok();
        }
    }
}