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
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Internal.Introspection.Model;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Represents the meta graph type '__InputValue.
    /// </summary>
    [DebuggerDisplay("INTROSPECTION TYPE __InputValue")]
    internal class Introspection_InputValueType : BaseIntrospectionObjectType
    {
        /// <summary>
        /// Gets the instance of this meta-type.
        /// </summary>
        /// <value>The instance.</value>
        public static Introspection_InputValueType Instance { get; } = new Introspection_InputValueType();

        /// <summary>
        /// Initializes static members of the <see cref="Introspection_InputValueType"/> class.
        /// </summary>
        static Introspection_InputValueType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_InputValueType"/> class.
        /// </summary>
        private Introspection_InputValueType()
            : base(Constants.ReservedNames.INPUT_VALUE_TYPE)
        {
            // "__InputValue" type definition
            // https://graphql.github.io/graphql-spec/June2018/#sec-Introspection
            // -------------------------------------------------------------------------
            this.Description = "A single argument supplied to a field, a directive or a complex input object.";

            this.GraphFieldCollection.AddField<IntrospectedInputValueType, string>(
                "name",
                new GraphTypeExpression(Constants.ScalarNames.STRING, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "name"),
                (arg) => arg.Name.AsCompletedTask(),
                "The case-sensitive name of this argument as it should be declared in a queryt.");

            this.GraphFieldCollection.AddField<IntrospectedInputValueType, string>(
                "description",
                new GraphTypeExpression(Constants.ScalarNames.STRING),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "description"),
                (arg) => arg.Description.AsCompletedTask(),
                "A human-friendly description of this argument and what it means to the item it is declared on.");

            this.GraphFieldCollection.AddField<IntrospectedInputValueType, IntrospectedType>(
                "type",
                new GraphTypeExpression(Constants.ReservedNames.TYPE_TYPE, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "type"),
                (arg) => arg.IntrospectedGraphType.AsCompletedTask(),
                "The graph type of this argument.");

            this.GraphFieldCollection.AddField<IntrospectedInputValueType, string>(
                "defaultValue",
                new GraphTypeExpression(Constants.ScalarNames.STRING),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "defaultValue"),
                (arg) => arg.DefaultValue.AsCompletedTask(),
                "(optional) a default value that will be used if this argument is not provided in a query.");
        }
    }
}