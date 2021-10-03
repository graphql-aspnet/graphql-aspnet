// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.PropertyTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class ArrayKeyValuePairObject
    {
        public KeyValuePair<string, string>[] PropertyA { get; set; }
    }
}