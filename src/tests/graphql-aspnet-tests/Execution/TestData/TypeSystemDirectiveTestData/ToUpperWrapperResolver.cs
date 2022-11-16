// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveTestData
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;

    public class ToUpperWrapperResolver : IGraphFieldResolver
    {
        private IGraphFieldResolver _originalResolver;

        public ToUpperWrapperResolver(IGraphFieldResolver originalResolver)
        {
            _originalResolver = originalResolver;
        }

        public Type ObjectType { get; }

        public async Task Resolve(FieldResolutionContext context, CancellationToken cancelToken = default)
        {
            await _originalResolver.Resolve(context, cancelToken);
        }
    }
}