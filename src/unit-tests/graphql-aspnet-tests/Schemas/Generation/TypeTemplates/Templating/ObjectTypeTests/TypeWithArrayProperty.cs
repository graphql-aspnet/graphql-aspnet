// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.ObjectTypeTests
{
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class TypeWithArrayProperty
    {
        public TwoPropertyObject[] PropertyA { get; set; }
    }
}