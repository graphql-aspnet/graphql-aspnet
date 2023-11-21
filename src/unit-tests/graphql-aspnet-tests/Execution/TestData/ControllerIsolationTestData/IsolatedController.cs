// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.ControllerIsolationTestData
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class IsolatedController : GraphController
    {
        private static SemaphoreSlim _slim = new SemaphoreSlim(1);

        [Query]
        public async Task<int> ExtractInt()
        {
            if (_slim.CurrentCount != 1)
                throw new InvalidOperationException("Other Controller action in progress. Thrown from ExtractInt. " + this.Context.Request.Origin.Path);

            try
            {
                await _slim.WaitAsync();
                await Task.Delay(15);
            }
            finally
            {
                _slim.Release();
            }

            return 3;
        }

        [Query]
        public async Task<int> ExtractSecondInt()
        {
            if (_slim.CurrentCount != 1)
                throw new InvalidOperationException("Other Controller action in progress. Thrown from ExtractSecondInt." + this.Context.Request.Origin.Path);
            try
            {
                await _slim.WaitAsync();
                await Task.Delay(15);
            }
            finally
            {
                _slim.Release();
            }

            return 4;
        }
    }
}