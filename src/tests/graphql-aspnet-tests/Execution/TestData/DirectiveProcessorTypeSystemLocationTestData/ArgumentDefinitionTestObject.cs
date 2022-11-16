// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.DirectiveProcessorTypeSystemLocationTestData
{
    using GraphQL.AspNet.Attributes;

    public class ArgumentDefinitionTestObject
    {
        [GraphField]
        public string MyField([ApplyDirective(typeof(LocationTestDirective))] int arg1)
        {
            return string.Empty;
        }
    }
}