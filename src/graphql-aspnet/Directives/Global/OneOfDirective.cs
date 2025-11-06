// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Directives.Global
{
    using System.ComponentModel;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// <para>
    /// A directive, applicable to an INPUT OBJECT type, that defines a validation scheme such that only
    /// one value of all available fields can be supplied in a document.
    /// </para>
    /// <para>
    /// https://spec.graphql.org/September2025/#sec-OneOf-Input-Objects
    /// </para>
    /// </summary>
    [GraphType(Constants.ReservedNames.ONEOF_DIRECTIVE)]
    [Description("A type system directive that enforces the 'one of' rule set for the target input object.")]
    public class OneOfDirective : GraphDirective
    {
        /// <summary>
        /// Executes the directive on the target input type.
        /// </summary>
        [DirectiveLocations(DirectiveLocation.INPUT_OBJECT)]
        public IGraphActionResult Execute()
        {
            var schemaItem = this.DirectiveTarget as ISchemaItem;
            if (schemaItem is IInputObjectGraphType inputItem)
            {
                var a = inputItem.Name;
            }

            // if not an input object, remove the applied directive if its present
            // this is an artifact of how applied directives move through the templating system.
            return this.Ok();
        }
    }
}