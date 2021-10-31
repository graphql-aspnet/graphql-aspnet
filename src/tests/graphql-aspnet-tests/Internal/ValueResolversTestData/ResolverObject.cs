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

    public class ResolverObject : IResolverInterface
    {
        public string MethodRetrieveData()
        {
            return string.Empty;
        }

        public string MethodWithArgument(int arg1)
        {
            return string.Empty;
        }

        public string MethodThrowException()
        {
            throw new InvalidOperationException("resolver.method.throwException");
        }

        public Task<string> MethodRetrieveDataAsync()
        {
            return Task.FromResult(string.Empty);
        }

        public Task<string> MethodWithArgumentAsync(int arg1)
        {
            return Task.FromResult(string.Empty);
        }

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