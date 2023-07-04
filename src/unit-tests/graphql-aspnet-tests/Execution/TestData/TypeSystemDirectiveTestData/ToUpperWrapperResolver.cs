// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.TypeSystemDirectiveTestData
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

        public async Task ResolveAsync(FieldResolutionContext context, CancellationToken cancelToken = default)
        {
            await _originalResolver.ResolveAsync(context, cancelToken);
        }

        public IGraphFieldResolverMetaData MetaData => _originalResolver.MetaData;
    }
}