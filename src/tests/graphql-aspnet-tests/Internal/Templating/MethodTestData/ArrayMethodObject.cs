// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.MethodTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class ArrayMethodObject
    {
        [GraphField]
        public TwoPropertyObject[] RetrieveData()
        {
            return null;
        }
    }
}