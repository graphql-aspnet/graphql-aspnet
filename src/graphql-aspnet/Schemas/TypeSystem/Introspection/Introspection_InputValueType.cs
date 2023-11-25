// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem.Introspection
{
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Introspection.Model;

    /// <summary>
    /// Represents the graph type '__InputValue'.
    /// </summary>
    [DebuggerDisplay("INTROSPECTION TYPE __InputValue")]
    internal class Introspection_InputValueType : BaseIntrospectionObjectType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_InputValueType"/> class.
        /// </summary>
        public Introspection_InputValueType()
            : base(Constants.ReservedNames.INPUT_VALUE_TYPE, nameof(Introspection_InputValueType))
        {
            // "__InputValue" type definition
            // https://graphql.github.io/graphql-spec/October2021/#sec-Introspection
            // -------------------------------------------------------------------------
            this.Description = "A single argument supplied to a field, a directive or a complex input object.";

            this.AddField<IntrospectedInputValueType, string>(
                "name",
                $"{this.InternalName}.{nameof(IntrospectedInputValueType.Name)}",
                new GraphTypeExpression(Constants.ScalarNames.STRING, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(SchemaItemPathCollections.Types, this.Name, "name"),
                (inputField) => inputField.Name.AsCompletedTask(),
                "The case-sensitive name of this argument as it should be declared in a queryt.");

            this.AddField<IntrospectedInputValueType, string>(
                "description",
                $"{this.InternalName}.{nameof(IntrospectedInputValueType.Description)}",
                new GraphTypeExpression(Constants.ScalarNames.STRING),
                new IntrospectedRoutePath(SchemaItemPathCollections.Types, this.Name, "description"),
                (inputField) => inputField.Description.AsCompletedTask(),
                "A human-friendly description of this argument and what it means to the item it is declared on.");

            this.AddField<IntrospectedInputValueType, IntrospectedType>(
                "type",
                $"{this.InternalName}.{nameof(IntrospectedInputValueType.IntrospectedGraphType)}",
                new GraphTypeExpression(Constants.ReservedNames.TYPE_TYPE, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(SchemaItemPathCollections.Types, this.Name, "type"),
                (inputField) => inputField.IntrospectedGraphType.AsCompletedTask(),
                "The graph type of this argument.");

            this.AddField<IntrospectedInputValueType, string>(
                "defaultValue",
                $"{this.InternalName}.{nameof(IntrospectedInputValueType.DefaultValue)}",
                new GraphTypeExpression(Constants.ScalarNames.STRING),
                new IntrospectedRoutePath(SchemaItemPathCollections.Types, this.Name, "defaultValue"),
                (inputField) => inputField.DefaultValue.AsCompletedTask(),
                "(optional) A default value that will be used if this input field is not provided in a query.");
        }
    }
}