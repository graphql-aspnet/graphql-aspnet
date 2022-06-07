// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.InterfaceExtensionTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class BoxNoSquareController : GraphController
    {
        [QueryRoot]
        public IBox RetrieveBoxInterface()
        {
            return new Box()
            {
                Height = "height2",
                Width = "width2",
                Length = "length2",
            };
        }

        [QueryRoot]
        public Box RetrieveBox()
        {
            return new Box()
            {
                Height = "height3",
                Width = "width3",
                Length = "length3",
            };
        }
    }
}