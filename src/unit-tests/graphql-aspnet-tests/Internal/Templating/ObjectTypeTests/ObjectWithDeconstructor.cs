// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.ObjectTypeTests
{
    public class ObjectWithDeconstructor
    {
        public string Property1 { get; set; }

        public void Deconstruct(out string prop1)
        {
            prop1 = Property1;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}