// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.ValueResolversTestData
{
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;

    public class ResolverObject : IResolverInterface
    {
        [GraphField]
        public string MethodRetrieveData()
        {
            return string.Empty;
        }

        [GraphField]
        public string MethodWithArgument(int arg1)
        {
            return string.Empty;
        }

        [GraphField]
        public string MethodThrowException()
        {
            throw new InvalidOperationException("resolver.method.throwException");
        }

        [GraphField]
        public Task<string> MethodRetrieveDataAsync()
        {
            return Task.FromResult(string.Empty);
        }

        [GraphField]
        public Task<string> MethodWithArgumentAsync(int arg1)
        {
            return Task.FromResult(string.Empty);
        }

        [GraphField]
        public string MethodThrowExceptionAsync()
        {
            throw new InvalidOperationException("resolver.method.throwException");
        }

        public string PropertyThrowException
        {
            get
            {
                throw new InvalidOperationException("resolver.property.throwException");
            }
        }

        public string Address1 { get; set; }

        public Task<string> Address1Async
        {
            get
            {
                return Task.FromResult("AddressAsync");
            }
        }

        public Task<string> AsyncPropException
        {
            get
            {
                throw new InvalidOperationException("resolver.property.throwException");
            }
        }
    }
}