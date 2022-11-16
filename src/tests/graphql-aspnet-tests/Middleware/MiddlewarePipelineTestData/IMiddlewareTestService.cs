// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.Pipelining.Data
{
    /// <summary>
    /// A test service interface that will be mocked that is invoked by the middleware test objects
    /// to signify that they were called at various points in the executed pipeline.
    /// </summary>
    public interface IMiddlewareTestService
    {
        void BeforeNext(string middleware);

        void AfterNext(string middleware);
    }
}