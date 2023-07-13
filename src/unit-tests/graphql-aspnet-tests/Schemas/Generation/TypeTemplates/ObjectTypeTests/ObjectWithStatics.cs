// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ObjectTypeTests
{
    using GraphQL.AspNet.Attributes;

    public class ObjectWithStatics
    {
        [GraphField]
        public static int StaticMethod(int a, int b)
        {
            return a - b;
        }

        [GraphField]
        public static int StaticProperty { get; set; }

        [GraphField]
        public int InstanceMethod(int a, int b)
        {
            return a + b;
        }

        [GraphField]
        public int InstanceProperty { get; }
    }
}