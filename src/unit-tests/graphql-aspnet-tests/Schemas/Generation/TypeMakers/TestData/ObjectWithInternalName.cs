// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeMakers.TestData
{
    using GraphQL.AspNet.Attributes;

    [GraphType(InternalName = "Object_Internal_Name")]
    public class ObjectWithInternalName
    {
        [GraphField(InternalName = "PropInternalName")]
        public int Prop1 { get; set; }

        [GraphField(InternalName = "MethodInternalName")]
        public int Method1()
        {
            return 0;
        }
    }
}