// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Common.InstanceFactoryData
{
    public struct StructWithMethod
    {
        public int AddAndSet(string propValue, int val)
        {
            this.Property1 = propValue;
            return val + 1;
        }

        public string Property1 { get; set; }
    }
}