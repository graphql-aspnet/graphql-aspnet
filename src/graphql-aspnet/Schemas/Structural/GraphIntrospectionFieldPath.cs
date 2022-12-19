// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Schemas.Structural
{
    using GraphQL.AspNet.Execution;

    /// <summary>
    /// A field path that identifies an introspection item
    /// in the schema.
    /// </summary>
    internal class GraphIntrospectionFieldPath : SchemaItemPath
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphIntrospectionFieldPath" /> class.
        /// </summary>
        /// <param name="itemName">Name of the item.</param>
        public GraphIntrospectionFieldPath(string itemName)
            : base(GraphCollection.Introspection, itemName)
        {
        }
    }
}