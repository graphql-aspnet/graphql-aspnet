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
    /// A base class that defines commonly used coding conventions for extracting the singular supplied
    /// value from an input union and defining useful defaults.
    /// </summary>
    public abstract class GraphInputUnion
    {
        // Note: ValueOrDefault() is an extension method so as to properly expose the runtime type
        // of the object that extended this class.

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