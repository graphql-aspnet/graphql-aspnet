// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.SchemaTestData
{
    using GraphQL.AspNet.Internal;

    public class PersonUnionData : GraphUnionProxy
    {
        public PersonUnionData()
            : base("PersonUnion", typeof(PersonData), typeof(EmployeeData))
        {
        }
    }
}