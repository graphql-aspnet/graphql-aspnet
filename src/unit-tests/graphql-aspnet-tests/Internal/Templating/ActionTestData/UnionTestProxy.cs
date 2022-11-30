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
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.Schema;

    public class UnionTestProxy : IGraphUnionProxy
    {
        public string Name { get; set; } = "BobUnion";

        public string Description { get; set; } = "This is the Bob union";

        public HashSet<Type> Types { get; } = new HashSet<Type>()
        {
            typeof(UnionDataA),
            typeof(UnionDataB),
        };

        public bool Publish { get; set; }

        public Type MapType(Type runtimeObjectType)
        {
            return runtimeObjectType;
        }
    }
}