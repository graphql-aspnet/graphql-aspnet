// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Configuration.Templates
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Configuration;

    /// <summary>
    /// A marker templtae for any runtime-built schema item (field, directive etc.)
    /// being added to the schema.
    /// </summary>
    public interface IGraphQLRuntimeSchemaItemTemplate : IDictionary<string, object>
    {
        /// <summary>
        /// Gets the templated name that will be given to the item on the target schema.
        /// </summary>
        /// <value>The fully qualified template for this item.</value>
        /// <remarks>(e.g. '@myDirective', '/path1/path2').</remarks>
        string Template { get; }

        /// <summary>
        /// Gets the set of schema options under which this directive is being defined.
        /// </summary>
        /// <value>The schema options on which this directive is being defined.</value>
        SchemaOptions Options { get; }

        /// <summary>
        /// Gets a list of attributes that have been applied to this directive. This mimics
        /// the collection of applied attributes to a controller method.
        /// </summary>
        /// <value>The collection of applied attributes.</value>
        IList<Attribute> Attributes { get; }
    }
}