// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.PropertyTestData
{
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class ArrayPropertyObject
    {
        public TwoPropertyObject[] PropertyA { get; set; }
    }
}