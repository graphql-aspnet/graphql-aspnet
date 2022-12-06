// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Controllers.ControllerTestData
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;

    [GraphRoute("invoke")]
    public class InvokableController : GraphController
    {
        public InvokableController()
        {
            this.CapturedItems = new Dictionary<string, object>();
        }

        [Query(typeof(string))]
        public Task<IGraphActionResult> AsyncActionMethod(string arg1 = "default")
        {
            this.CapturedItems.Add(nameof(ModelState), this.ModelState);
            this.CapturedItems.Add(nameof(Request), this.Request);
            this.CapturedItems.Add(nameof(RequestServices), this.RequestServices);

            return Task.FromResult(this.Ok("data result"));
        }

        [Query(typeof(string))]
        public Task<IGraphActionResult> AsyncActionMethodToCauseException(string arg1 = "default")
        {
           throw new UserThrownException();
        }

        [Query(typeof(string))]
        public IGraphActionResult SyncronousActionMethod(string arg1 = "default")
        {
            return this.Ok("data result");
        }

        [Query(typeof(string))]
        public IGraphActionResult CreateNotFoundResult()
        {
            return this.NotFound("it was not found");
        }

        [Query(typeof(string))]
        public IGraphActionResult ErrorResult()
        {
            return this.Error("an error happened", "12345", new Exception("exception text"));
        }

        public Dictionary<string, object> CapturedItems { get; }
    }
}