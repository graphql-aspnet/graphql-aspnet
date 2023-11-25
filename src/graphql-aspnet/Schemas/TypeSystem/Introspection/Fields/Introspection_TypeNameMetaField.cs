// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem.Introspection.Fields
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Resolvers;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// <para>A meta field automatically added to all OBJECT graph types to return the type name of the object in question.
    /// Exposed as '__typeName' on any object. This field will not be published
    /// on any introspection queries.</para>
    /// <para>spec: <see href="https://graphql.github.io/graphql-spec/October2021/#sec-Type-Name-Introspection" /> .</para>
    /// </summary>
    [DebuggerDisplay("Meta Field: " + Constants.ReservedNames.TYPENAME_FIELD)]
    public class Introspection_TypeNameMetaField : MethodGraphField
    {
        private static readonly SchemaItemPath FIELD_PATH = new SchemaItemPath(SchemaItemPathCollections.Query, Constants.ReservedNames.TYPENAME_FIELD);
        private readonly string _graphTypeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_TypeNameMetaField"/> class.
        /// </summary>
        /// <param name="graphTypeName">Name of the graph type.</param>
        public Introspection_TypeNameMetaField(string graphTypeName)
            : base(
                Constants.ReservedNames.TYPENAME_FIELD,
                nameof(Introspection_TypeNameMetaField),
                new GraphTypeExpression(Constants.ScalarNames.STRING, MetaGraphTypes.IsNotNull),
                FIELD_PATH,
                typeof(string),
                typeof(string),
                FieldResolutionMode.PerSourceItem)
        {
            _graphTypeName = Validation.ThrowIfNullWhiteSpaceOrReturn(graphTypeName, nameof(graphTypeName));
            this.UpdateResolver(new FunctionGraphFieldResolver<object, string>((obj) => graphTypeName.AsCompletedTask()), FieldResolutionMode.PerSourceItem);
        }

        /// <inheritdoc />
        public override IGraphField Clone(ISchemaItem parent = null, string fieldName = null, GraphTypeExpression typeExpression = null)
        {
            if (fieldName != null)
                throw new NotSupportedException($"{nameof(Introspection_TypeNameMetaField)} does not support field name changes.");
            if (typeExpression != null)
                throw new NotSupportedException($"{nameof(Introspection_TypeNameMetaField)} does not support type expression changes.");

            var item = new Introspection_TypeNameMetaField(_graphTypeName);
            item.Parent = parent ?? this.Parent;
            item.Description = this.Description;

            return item;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IGraphField" /> is published
        /// in the schema delivered to introspection requests.
        /// </summary>
        /// <value><c>true</c> if publish; otherwise, <c>false</c>.</value>
        public override bool Publish => false;
    }
}