// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Interfaces
{
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A base interface identifiying an object as a graph type in the object type.
    /// </summary>
    public interface IGraphTypeTemplate : IGraphItemTemplate, ISecureItem
    {
        /// <summary>
        /// Gets the kind of graph type that can be made from this template.
        /// </summary>
        /// <value>The kind.</value>
        TypeKind Kind { get; }

        /// <summary>
        /// Gets the declaration requirements, if any, that this template defines as needing to be inforced for its specific templated
        /// type.
        /// </summary>
        /// <value>The declaration requirements.</value>
        TemplateDeclarationRequirements? DeclarationRequirements { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is marked such that graph types
        /// made from it are published in introspection queries.
        /// </summary>
        /// <value><c>true</c> if publish; otherwise, <c>false</c>.</value>
        bool Publish { get; }
    }
}