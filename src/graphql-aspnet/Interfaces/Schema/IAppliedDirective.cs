// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.Schema
{
    using System;
    using GraphQL.AspNet.Directives;

    /// <summary>
    /// An instance of a directive to be applied to an item (either a document or a type system item).
    /// </summary>
    public interface IAppliedDirective
    {
        /// <summary>
        /// Gets the concrete type of the directive to apply.
        /// </summary>
        /// <value>The type of the directive.</value>
        Type DirectiveType { get; }

        /// <summary>
        /// Gets the name of the directive to apply as it exists in the schema.
        /// </summary>
        /// <value>The name of the directive.</value>
        string DirectiveName { get; }

        /// <summary>
        /// Gets the collection of supplied, ordered argument values to be used on the directive
        /// invocation. These arguments are passed to the <see cref="GraphDirective"/>
        /// created to process the target that owns instance.
        /// </summary>
        /// <value>The arguments.</value>
        object[] ArgumentValues { get; }

        /// <summary>
        /// Clones this instance and creates an independent copy.
        /// </summary>
        /// <returns>IAppliedDirective.</returns>
        IAppliedDirective Clone();
    }
}