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

    public class BoxController : GraphController
    {
        [QueryRoot]
        public ISquare RetrieveSquareInterface()
        {
            return new Box()
            {
                Height = "height1",
                Width = "width1",
                Length = "length1",
            };
        }

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

        [TypeExtension(typeof(ISquare), "area")]
        public string SquareExtension(ISquare square)
        {
            return square.Length + "|" + square.Width;
        }

        [TypeExtension(typeof(Box), "corners")]
        public string SquareExtension(Box box)
        {
            return box.Length + "|6";
        }
    }
}