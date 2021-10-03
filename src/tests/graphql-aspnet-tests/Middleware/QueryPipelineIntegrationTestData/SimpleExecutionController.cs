// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Middleware.QueryPipelineIntegrationTestData
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    [GraphRoute("simple")]
    public class SimpleExecutionController : GraphController
    {
        [Query]
        public TwoPropertyObject SimpleQueryMethod(string arg1 = "default string", long arg2 = 5)
        {
            return new TwoPropertyObject()
            {
                Property1 = arg1,
                Property2 = arg2 > int.MaxValue ? int.MaxValue : Convert.ToInt32(arg2),
            };
        }

        /// <summary>
        /// Collections the query method.
        /// </summary>
        /// <returns>List&lt;TwoPropertyObject&gt;.</returns>
        [Query]
        public List<TwoPropertyObject> CollectionQueryMethod()
        {
            var list = new List<TwoPropertyObject>();
            for (var i = 0; i < 3; i++)
            {
                var obj = new TwoPropertyObject()
                {
                    Property1 = "string" + i,
                    Property2 = i,
                };
                list.Add(obj);
            }

            return list;
        }

        [Query]
        public async Task<string> TimedOutMethod()
        {
            await Task.Delay(2000);
            return "Task complete";
        }

        [Query]
        public Task<ObjectWithThrowMethod> ThrowFromController()
        {
            throw new Exception("Failure from Controller");
        }

        [Query]
        public Task<ObjectWithThrowMethod> Throwable()
        {
            return new ObjectWithThrowMethod().AsCompletedTask();
        }

        [Query(typeof(string))]
        public IGraphActionResult CustomMessage()
        {
            var message = new GraphExecutionMessage(GraphMessageSeverity.Critical, "fail text", "fail code");
            message.MetaData.Add("customKey1", "customValue1");
            return this.Error(message);
        }

        [Query(typeof(string))]
        public IGraphActionResult CustomMessageKeyClash()
        {
            var message = new GraphExecutionMessage(GraphMessageSeverity.Critical, "fail text", "fail code");
            message.MetaData.Add("severity", "gleam");
            return this.Error(message);
        }

        [Query(typeof(string))]
        public IGraphActionResult ThrowsException()
        {
            throw new InvalidOperationException("This is an invalid message");
        }
    }
}