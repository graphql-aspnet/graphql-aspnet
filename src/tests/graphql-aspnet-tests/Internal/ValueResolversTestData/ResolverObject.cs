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

        public string Address1 { get; set; }

        public string PropertyThrowException
        {
            get
            {
                throw new InvalidOperationException("resolver.property.throwException");
            }
        }
    }
}