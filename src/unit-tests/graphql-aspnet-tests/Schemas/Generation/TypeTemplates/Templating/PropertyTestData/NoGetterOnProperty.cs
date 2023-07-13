// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.PropertyTestData
{
    using GraphQL.AspNet.Attributes;

    public class NoGetterOnProperty
    {
        [GraphField]
        public string Prop1
        {
            set { }
        }
    }
}