// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TestData.UnionTypeExecutionTestData
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class PersonOrTeacher : GraphUnionProxy
    {
        public PersonOrTeacher()
            : base()
        {
            this.AddType(typeof(Person));
            this.AddType(typeof(Teacher));
        }

        public override Type MapType(Type runtimeObjectType)
        {
            if (runtimeObjectType == typeof(Teacher))
                return runtimeObjectType;

            if (Validation.IsCastable<Person>(runtimeObjectType))
                return typeof(Person);

            if (runtimeObjectType == typeof(TwoPropertyObject))
                return runtimeObjectType;

            if (runtimeObjectType == typeof(TwoPropertyObjectV2))
                throw new InvalidOperationException("Union Threw an Exception");

            return null;
        }
    }
}