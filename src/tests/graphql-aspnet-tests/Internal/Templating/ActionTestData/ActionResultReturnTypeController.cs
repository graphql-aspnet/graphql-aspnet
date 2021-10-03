// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.ActionTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

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

        [Query("path3", typeof(List<TwoPropertyObject>), TypeExpression = TypeExpressions.IsList | TypeExpressions.IsNotNull | TypeExpressions.IsNotNullList)]
        public IGraphActionResult ActionResultMethodWithListReturnTypeAndOptions()
        {
            return null;
        }

        [Query("path4", typeof(TwoPropertyObject))]
        public TwoPropertyObjectV2 ActionResultMethodWithDeclaredReturnTypeAndMethodReturnType()
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