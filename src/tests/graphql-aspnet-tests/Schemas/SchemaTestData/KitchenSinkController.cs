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
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.CommonHelpers;

    [GraphRoute("path0/path1")]
    [Description("Kitchen sinks are great")]
    public class KitchenSinkController : GraphController
    {
        [Query]
        public TwoPropertyObject TestActionMethod(string arg1, ulong arg2, TwoPropertyObject itemA)
        {
            return new TwoPropertyObject();
        }

        [Query("TestAction2")]
        public TwoPropertyObjectV2 TestActionMethod2(IEnumerable<EmptyObject> arg1, DateTime arg2)
        {
            return new TwoPropertyObjectV2();
        }

        [QueryRoot("myActionOperation")]
        [Description("This is my\n Top Level Query Field")]
        public TestEnumerationOptions MethodAsTopLevelQuery()
        {
            return TestEnumerationOptions.EnumerationOption1;
        }

        [Mutation("path2/PAth3/PaTh4/PAT_H5/pathSix/deepNestedMethod")]
        [Description("This is a mutation")]
        [Deprecated("To be removed tomorrow")]
        public Task<CompletePropertyObject> DeepVirtualNestedMethod(long? param1 = null)
        {
            return Task.FromResult(new CompletePropertyObject());
        }

        [MutationRoot("SupeMutation")]
        [Description("This is my\n Top Level MUtation Field!@@!!")]
        public Task<Person> MethodAsTopLevelMutation()
        {
            return Task.FromResult(new Person());
        }

        public Task<string> UnMarkedMethod()
        {
            return Task.FromResult("This should never happen.");
        }
    }
}