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
    public struct ParameterizedStruct
    {
        public ParameterizedStruct(string prop1Value)
        {
            this.Property1 = prop1Value;
        }

        public string Property1 { get; set; }
    }
}