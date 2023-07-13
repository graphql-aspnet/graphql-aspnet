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
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class ArrayInputMethodController : GraphController
    {
        [Mutation]
        public string AddData(TwoPropertyObject[] data)
        {
            return string.Empty;
        }
    }
}