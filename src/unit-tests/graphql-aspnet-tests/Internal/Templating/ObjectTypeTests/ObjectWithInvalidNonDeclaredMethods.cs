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
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;

    public class ObjectWithInvalidNonDeclaredMethods
    {
        [GraphField]
        public string ValidMethodWithDeclaration()
        {
            return string.Empty;
        }

        public string ValidMethodWithoutDeclaration()
        {
            return string.Empty;
        }

        public IDictionary<string, int> InvalidMethodWithoutDeclaration()
        {
            return null;
        }

        public int InvalidParameterWithoutDeclaration(IDictionary<string, object> data)
        {
            return 0;
        }

        [GraphField]
        public int DeclaredProperty { get; set; }
    }
}