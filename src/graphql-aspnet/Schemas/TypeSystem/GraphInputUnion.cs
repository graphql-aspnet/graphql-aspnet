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
    using GraphQL.AspNet.Attributes;

    /// <summary>
    /// A base type that defines an object as an input union (e.g. a '@oneOf') type. Inherting from this
    /// class provides access to extra metadata and value extraction methods at runtime. See documentation for details.
    /// </summary>
    /// <remarks>
    /// Classes that inherit from this type CANNOT be used as regular OBJECT types, only as INPUT OBJECT types.
    /// </remarks>
    public abstract class GraphInputUnion
    {
        /// <summary>
        /// Gets or sets the field name of the supplied value on the query, as its defined in the graph schema.
        /// </summary>
        [GraphSkip]
        public string GraphFieldName { get; set; }

        /// <summary>
        /// Gets or sets the field/property name of the supplied value on the query, as its defined in source code.
        /// </summary>
        [GraphSkip]
        public string TypeFieldName { get; set; }
    }
}