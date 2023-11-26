// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A marker templtae for any runtime-built schema item (field, directive etc.)
    /// being added to the schema. (e.g. minimal api defined fields and directives).
    /// </summary>
    public interface IGraphQLRuntimeSchemaItemDefinition
    {
        /// <summary>
        /// Adds the given attribute to the collection for this schema item.
        /// </summary>
        /// <param name="attrib">The attribute to append.</param>
        void AddAttribute(Attribute attrib);

        /// <summary>
        /// Removes the specified attribute from the collection.
        /// </summary>
        /// <param name="attrib">The attribute to remove.</param>
        void RemoveAttribute(Attribute attrib);

        /// <summary>
        /// Gets the path name that will be given to the item on the target schema.
        /// </summary>
        /// <value>The fully qualified path name for this item.</value>
        /// <remarks>(e.g. '[directive]/@myDirective', '[query]/path1/path2').</remarks>
        ItemPath ItemPath { get; }

        /// <summary>
        /// Gets the set of schema options under which this directive is being defined.
        /// </summary>
        /// <value>The schema options on which this directive is being defined.</value>
        SchemaOptions Options { get; }

        /// <summary>
        /// Gets a set of attributes that have been applied to this directive. This mimics
        /// the collection of applied attributes to a controller method.
        /// </summary>
        /// <value>The collection of applied attributes.</value>
        IEnumerable<Attribute> Attributes { get; }
    }
}