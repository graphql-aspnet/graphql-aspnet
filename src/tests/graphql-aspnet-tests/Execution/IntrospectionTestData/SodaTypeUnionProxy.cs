// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.IntrospectionTestData
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    public class SodaTypeUnionProxy : IGraphUnionProxy
    {
        public string Name => "SodaTypes";

        public string Description => "A list of soda types this fountain produces";

        public HashSet<Type> Types { get; } = new HashSet<Type> { typeof(SodaTypeA), typeof(SodaTypeB) };

        public bool Publish => true;

        public Type ResolveType(Type runtimeObjectType)
        {
            return runtimeObjectType;
        }
    }
}