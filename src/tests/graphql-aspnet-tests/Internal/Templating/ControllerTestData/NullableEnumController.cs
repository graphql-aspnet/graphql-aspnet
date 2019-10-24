// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.ControllerTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class NullableEnumController : GraphController
    {
        public enum LengthType
        {
            Feet = 0,
            Inches = 1,
            Yards = 2,
        }

        [Query]
        public int ConvertUnit(LengthType? unit = LengthType.Yards)
        {
            return 0;
        }
    }
}