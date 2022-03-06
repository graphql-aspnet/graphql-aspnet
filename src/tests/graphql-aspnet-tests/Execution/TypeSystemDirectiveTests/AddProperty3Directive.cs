// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveTests
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class AddProperty3Directive : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.OBJECT)]
        public IGraphActionResult AddProperty3()
        {
            // ensure we are working with a graph field definition
            var item = this.DirectiveTarget as IObjectGraphType;
            if (item != null)
            {
                var stringType = this.Schema.KnownTypes.FindGraphType(typeof(string), TypeKind.SCALAR);
                item.Extend(
                    "property3",
                    GraphTypeExpression.FromDeclaration(stringType.Name),
                    RetrieveProperty3,
                    "retrieves a prop 3");
            }

            return this.Ok();
        }

        private Task<string> RetrieveProperty3(object item)
        {
            var str = string.Empty;
            if (item is TestObjectWithFieldExtensionDirectiveByType obj)
            {
                str += obj.Property1 ?? string.Empty;
                str += " ";
            }

            str += "property 3";
            return str.Trim().AsCompletedTask();
        }
    }
}