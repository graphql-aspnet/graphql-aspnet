// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Configuration.SchemaItemExtensionTestData
{
    using GraphQL.AspNet.Attributes;

    public class MethodObject
    {
        [GraphField]
        public string Name(string whichName)
        {
            return string.Empty;
        }
    }
}