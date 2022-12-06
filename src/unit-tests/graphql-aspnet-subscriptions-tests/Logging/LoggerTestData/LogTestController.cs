// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Logging.LoggerTestData
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Controllers;

    public class LogTestController : GraphController
    {
        [QueryRoot("field1")]
        public string ExecuteField()
        {
            return "a string of data";
        }

        [QueryRoot]
        public Task<string> ExecuteField2()
        {
            return "a string of data".AsCompletedTask();
        }

        [QueryRoot("fieldException")]
        public string ExecuteFieldThrowsException()
        {
            throw new InvalidOperationException("failed");
        }

        [QueryRoot("fakeInvocationException")]
        public string ExecuteThrowsInvocationException()
        {
            throw new TargetInvocationException(null);
        }

        [QueryRoot]
        public bool ValidatableInputObject(ValidatableObject arg1)
        {
            return false;
        }

        [MutationRoot]
        public bool MutationMethod()
        {
            return true;
        }
    }
}