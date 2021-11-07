// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// <para>A directive, applicable to a field, that defines additional logic to determine if
    /// the field inclusion should be included or not.</para>
    /// <para>Spec: https://graphql.github.io/graphql-spec/June2018/#sec--include .</para>
    /// </summary>
    [GraphType("FakeDirective")]
    [DirectiveLocations(ExecutableDirectiveLocation.AllFieldSelections)]
    public class TestDirective : GraphDirective
    {
        public IGraphActionResult BeforeFieldResolution([FromGraphQL("if")] bool ifArgument)
        {
            return ifArgument ? this.Ok() : this.Cancel();
        }

        public ISchemaItem AlterSchemaItem(ISchemaItem schemaItem)
        {
            return schemaItem;
        }
    }
}