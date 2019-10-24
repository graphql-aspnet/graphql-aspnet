// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Introspection.Fields
{
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Resolvers;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// <para>A meta field automatically added to all OBJECT graph types to return the type name of the object in question.
    /// Exposed as '__typeName' on any object. This field will not be published
    /// on any introspection queries.</para>
    /// <para>spec: https://graphql.github.io/graphql-spec/June2018/#sec-Type-Name-Introspection .</para>
    /// </summary>
    [DebuggerDisplay("Meta Field: " + Constants.ReservedNames.TYPENAME_FIELD)]
    public class Introspection_TypeNameMetaField : MethodGraphField
    {
        private static readonly GraphFieldPath FIELD_PATH = new GraphFieldPath(GraphCollection.Query, Constants.ReservedNames.TYPENAME_FIELD);

        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_TypeNameMetaField"/> class.
        /// </summary>
        /// <param name="graphTypeName">Name of the graph type.</param>
        public Introspection_TypeNameMetaField(string graphTypeName)
            : base(
                Constants.ReservedNames.TYPENAME_FIELD,
                new GraphTypeExpression(Constants.ScalarNames.STRING, MetaGraphTypes.IsNotNull),
                FIELD_PATH)
        {
            Validation.ThrowIfNull(graphTypeName, nameof(graphTypeName));
            this.UpdateResolver(new GraphDataValueResolver<object, string>((obj) => graphTypeName.AsCompletedTask()), FieldResolutionMode.PerSourceItem);
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IGraphField" /> is published
        /// in the schema delivered to introspection requests.
        /// </summary>
        /// <value><c>true</c> if publish; otherwise, <c>false</c>.</value>
        public override bool Publish => false;
    }
}