// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A set of extensions applied to <see cref="IGraphType" />.
    /// </summary>
    public static class GraphTypeExtensions
    {
        /// <summary>
        /// Inspects the <paramref name="graphType" /> and if it is also a schema coordinate, returns the coordinate formatted
        /// name, otherwise returns the default name.
        /// </summary>
        /// <param name="graphType">The graph type to inspect.</param>
        public static string NameOrCoordinate(this IGraphType graphType)
        {
            if (graphType is ISchemaCoordinateItem sci)
                return sci.SchemaCoordinate;

            return graphType?.Name;
        }
    }
}