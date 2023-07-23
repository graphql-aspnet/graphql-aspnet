// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ActionTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    [GraphRoute("path0")]
    public class ActionResultReturnTypeController : GraphController
    {
        [Query("path2")]
        public IGraphActionResult ActionResultMethod()
        {
            return null;
        }

        [Query("path3", typeof(TwoPropertyObject))]
        public IGraphActionResult ActionResultMethodWithDeclaredReturnType()
        {
            return null;
        }

        [Query("path3", typeof(List<TwoPropertyObject>))]
        public IGraphActionResult ActionResultMethodWithListReturnType()
        {
            return null;
        }

        [Query("path3", typeof(List<TwoPropertyObject>), TypeExpression = "[Type!]!")]
        public IGraphActionResult ActionResultMethodWithListReturnTypeAndOptions()
        {
            return null;
        }

        [Query("path4", typeof(TwoPropertyObject))]
        public TwoPropertyObjectV2 MethodWithDeclaredReturnTypeAndMethodReturnType()
        {
            return null;
        }

        [Query("path5", typeof(CustomNamedItem))]
        public IGraphActionResult ActionResultWithCustomNamedReturnedItem()
        {
            return null;
        }
    }
}