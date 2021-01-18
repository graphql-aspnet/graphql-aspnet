// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.SchemaTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class DictionaryMethodObject : GraphController
    {
        [Query]
        public TwoPropertyObject MethodWithDictionaryInput(IDictionary<string, int> arg1)
        {
            return null;
        }
    }
}