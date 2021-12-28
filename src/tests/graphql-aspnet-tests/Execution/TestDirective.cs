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
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// <para>A directive, applicable to a field, that defines additional logic to determine if
    /// the field inclusion should be included or not.</para>
    /// <para>Spec: https://graphql.github.io/graphql-spec/June2018/#sec--include .</para>
    /// </summary>
    [GraphType("FakeDirective")]
    public class TestDirective : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.FIELD | DirectiveLocation.FRAGMENT_SPREAD | DirectiveLocation.INLINE_FRAGMENT)]
        public IGraphActionResult Execute([FromGraphQL("if")] bool ifArgument)
        {
            return ifArgument ? this.Ok() : this.Cancel();
        }

        [DirectiveLocations(DirectiveLocation.FIELD_DEFINITION)]
        public ISchemaItem AlterSchemaItem(ISchemaItem schemaItem)
        {
            return schemaItem;
        }
    }
}