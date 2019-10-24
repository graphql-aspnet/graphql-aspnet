// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Introspection.Types
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Internal.Introspection.Model;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Represents the meta-type called "__Directive".
    /// </summary>
    [DebuggerDisplay("INTROSPECTION TYPE __Directive")]
    internal class Introspection_DirectiveType : BaseIntrospectionObjectType
    {
        /// <summary>
        /// Gets the instance of this meta-type.
        /// </summary>
        /// <value>The instance.</value>
        public static Introspection_DirectiveType Instance { get; } = new Introspection_DirectiveType();

        /// <summary>
        /// Initializes static members of the <see cref="Introspection_DirectiveType"/> class.
        /// </summary>
        static Introspection_DirectiveType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_DirectiveType"/> class.
        /// </summary>
        private Introspection_DirectiveType()
            : base(Constants.ReservedNames.DIRECTIVE_TYPE)
        {
            // "__Directive" type definition
            // https://graphql.github.io/graphql-spec/June2018/#sec-Introspection
            // -------------------------------------------------------------------------
            this.GraphFieldCollection.AddField<IntrospectedDirective, string>(
                "name",
                new GraphTypeExpression(
                    Constants.ScalarNames.STRING,
                    GraphTypeExpression.RequiredSingleItem),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "name"),
                (directive) => directive.Name.AsCompletedTask(),
                "The case-sensitive name of this directive as it should appear in a query.");

            this.GraphFieldCollection.AddField<IntrospectedDirective, string>(
                "description",
                new GraphTypeExpression(Constants.ScalarNames.STRING),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "description"),
                (directive) => directive.Description.AsCompletedTask(),
                "A human-friendly description of the directive and how it functions.");

            this.GraphFieldCollection.AddField<IntrospectedDirective, IReadOnlyList<DirectiveLocation>>(
                "locations",
                new GraphTypeExpression(
                    Constants.ReservedNames.DIRECTIVE_LOCATION_ENUM,
                    GraphTypeExpression.RequiredListRequiredItem),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "locations"),
                (directive) => directive.Locations.AsCompletedTask(),
                "A collection of locations where this directive can be used.");

            this.GraphFieldCollection.AddField<IntrospectedDirective, IReadOnlyList<IntrospectedInputValueType>>(
                "args",
                new GraphTypeExpression(
                    Constants.ReservedNames.INPUT_VALUE_TYPE,
                    GraphTypeExpression.RequiredListRequiredItem),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "args"),
                (directive) => directive.Arguments.AsCompletedTask(),
                "A collection of input values provided to the directive in order to properly invoke it.");
        }
    }
}