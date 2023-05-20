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
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Configuration;

    /// <summary>
    /// A field builder that can act as a parent for a set of fields. The defined
    /// template for this builder will prefix all fields or other field groups created
    /// underneath it. Also, all properties and settings, such as authorization requirements, for the "group", are carried
    /// into any fields generated.
    /// </summary>
    public interface IGraphQLFieldGroupBuilder : IDictionary<string, object>
    {
        // **********************
        // Implementation note (05/23) - Kevin
        // This interface is functionally identical to the single field builder
        // but is seperate to allow for divation in handling via extension methods and
        // potential future development
        // **********************

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

        /// <summary>
        /// Gets a list of attributes that have been applied to this builder. This mimics
        /// the collection of applied attributes to a controller method.
        /// </summary>
        /// <value>The collection of applied attributes.</value>
        IList<Attribute> Attributes { get; }
    }
}