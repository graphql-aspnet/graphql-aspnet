// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Engine.TypeMakers.TestData
{
    using GraphQL.AspNet.Attributes;

    [ApplyDirective(typeof(DirectiveWithArgs), 12, "object directive")]
    public class ObjectDirectiveTestItem
    {
        [ApplyDirective(typeof(DirectiveWithArgs), 13, "prop field arg")]
        public string Prop1 { get; set; }

        [GraphField]
        [ApplyDirective(typeof(DirectiveWithArgs), 14, "method field arg")]
        public string Method1()
        {
            return null;
        }

        [GraphField]
        public string MethodWithArgDirectives(
            [ApplyDirective(typeof(DirectiveWithArgs), 15, "arg arg")]
            int arg1)
        {
            return null;
        }
    }
}