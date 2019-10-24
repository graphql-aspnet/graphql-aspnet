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
    public class ObjectWithNoSetters
    {
        public int Property1 { get; set; }

        public int Property2 { get; }

        public int Property3 { get; }
    }
}