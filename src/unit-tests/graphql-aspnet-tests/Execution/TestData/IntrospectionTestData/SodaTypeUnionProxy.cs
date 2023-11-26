// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.IntrospectionTestData
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.Schema;

    public class SodaTypeUnionProxy : IGraphUnionProxy
    {
        public string Name { get; set; } = "SodaTypes";

        public string Description { get; set; } = "A list of soda types this fountain produces";

        public HashSet<Type> Types { get; } = new HashSet<Type> { typeof(SodaTypeA), typeof(SodaTypeB) };

        public bool Publish { get; set; } = true;

        public string InternalName { get; } = "InternalSodaTypes";

        public Type MapType(Type runtimeObjectType)
        {
            return runtimeObjectType;
        }
    }
}