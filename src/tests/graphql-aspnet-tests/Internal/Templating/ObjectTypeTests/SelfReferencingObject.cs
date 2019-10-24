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
    using GraphQL.AspNet.Attributes;

    public class SelfReferencingObject
    {
        [GraphField]
        public string Name { get; set; }

        [GraphField]
        public SelfReferencingObject Parent { get; set; }
    }
}