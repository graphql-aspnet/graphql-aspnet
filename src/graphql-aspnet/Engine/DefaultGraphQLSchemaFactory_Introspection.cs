// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Introspection;
    using GraphQL.AspNet.Schemas.TypeSystem.Introspection.Fields;
    using GraphQL.AspNet.Schemas.TypeSystem.Introspection.Model;

    /// <summary>
    /// The default schema factory, capable of creating singleton instances of
    /// schemas, fully populated and ready to serve requests.
    /// </summary>
    public partial class DefaultGraphQLSchemaFactory<TSchema>
    {

        /// <summary>
        /// Clears, builds and caches the introspection metadata used to describe this schema. If introspection
        /// fields have not been added to the schema this method does nothing. No changes to the schema
        /// items themselves happens during this method call.
        /// </summary>
        protected virtual void RebuildIntrospectionData()
        {
            if (this.Schema.Configuration.DeclarationOptions.DisableIntrospection)
                return;

            this.EnsureGraphOperationType(GraphOperationType.Query);
            this.AddIntrospectionFields();

            var queryType = this.Schema.Operations[GraphOperationType.Query];
            if (!queryType.Fields.ContainsKey(Constants.ReservedNames.SCHEMA_FIELD))
                return;

            var field = queryType.Fields[Constants.ReservedNames.SCHEMA_FIELD] as Introspection_SchemaField;
            field.IntrospectedSchema.Rebuild();
        }

        /// <summary>
        /// Adds the internal introspection fields to the query operation type if and only if the contained schema allows
        /// it through its internal configuration. This method is idempotent.
        /// </summary>
        protected virtual void AddIntrospectionFields()
        {
            this.EnsureGraphOperationType(GraphOperationType.Query);
            var queryField = this.Schema.Operations[GraphOperationType.Query];

            // Note: introspection fields are defined by the graphql spec, no custom name or item formatting is allowed
            // for Type and field name formatting.
            // spec: https://graphql.github.io/graphql-spec/October2021/#sec-Schema-Introspection
            if (!queryField.Fields.ContainsKey(Constants.ReservedNames.SCHEMA_FIELD))
            {
                var introspectedSchema = new IntrospectedSchema(this.Schema);
                queryField.Extend(new Introspection_SchemaField(introspectedSchema));
                queryField.Extend(new Introspection_TypeGraphField(introspectedSchema));

                this.EnsureGraphType(typeof(string));
                this.EnsureGraphType(typeof(bool));
                this.Schema.KnownTypes.EnsureGraphType(new Introspection_DirectiveLocationType(), typeof(DirectiveLocation));
                this.Schema.KnownTypes.EnsureGraphType(new Introspection_DirectiveType(), typeof(IntrospectedDirective));
                this.Schema.KnownTypes.EnsureGraphType(new Introspection_EnumValueType(), typeof(IntrospectedEnumValue));
                this.Schema.KnownTypes.EnsureGraphType(new Introspection_FieldType(), typeof(IntrospectedField));
                this.Schema.KnownTypes.EnsureGraphType(new Introspection_InputValueType(), typeof(IntrospectedInputValueType));
                this.Schema.KnownTypes.EnsureGraphType(new Introspection_SchemaType(), typeof(IntrospectedSchema));
                this.Schema.KnownTypes.EnsureGraphType(new Introspection_TypeKindType(), typeof(TypeKind));
                this.Schema.KnownTypes.EnsureGraphType(new Introspection_TypeType(), typeof(IntrospectedType));
            }
        }
    }
}