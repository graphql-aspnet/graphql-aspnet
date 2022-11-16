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
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class IInterfaceAController : GraphController
    {
        [TypeExtension(typeof(IInterfaceA), "extendedFieldA")]
        public string ExtendedInterfaceAField()
        {
            return string.Empty;
        }
    }
}