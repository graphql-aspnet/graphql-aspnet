// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Configuration
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A builder that utilizies a key/value pair system to build up a set of component parts
    /// that the templating engine will use to generate a full fledged field in a schema.
    /// </summary>
    public interface IGraphQLFieldBuilder : IReadOnlyDictionary<string, object>
    {
        /// <summary>
        /// Gets the full path template that points to a location in
        /// the virtual field tree.
        /// </summary>
        /// <remarks>
        /// e.g. /path1/path2/path3
        /// </remarks>
        /// <value>The fully qualified path template for this builder.</value>
        string Template { get; }

        /// <summary>
        /// Gets the set of schema options under with this field is being defined.
        /// </summary>
        /// <value>The schema options on which this field is being defined.</value>
        SchemaOptions Options { get; }
    }
}