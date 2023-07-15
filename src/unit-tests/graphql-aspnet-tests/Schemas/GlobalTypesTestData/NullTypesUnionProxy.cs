// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.GlobalTypesTestData
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.Schema;

    public class NullTypesUnionProxy : IGraphUnionProxy
    {
        public NullTypesUnionProxy()
        {
            this.Name = "Name";
            this.Types = null;
        }

        public HashSet<Type> Types { get; }

        public bool Publish { get; set; }

        public string Name { get; }

        public string Description { get; set; }

        public Type MapType(Type runtimeObjectType)
        {
            return null;
        }
    }
}