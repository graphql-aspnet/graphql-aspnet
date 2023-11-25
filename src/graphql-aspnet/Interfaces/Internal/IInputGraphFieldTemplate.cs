// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Internal
{
    using System;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A base template definition for the metadata about a graph field belonging to a real type (as opposed to a virutal
    /// type).
    /// </summary>
    public interface IInputGraphFieldTemplate : ISchemaItemTemplate
    {
        /// <summary>
        /// Gets a value indicating whether this instance is marked as being a required field.
        /// </summary>
        /// <value><c>true</c> if this instance is required; otherwise, <c>false</c>.</value>
        public bool IsRequired { get;  }

        /// <summary>
        /// Gets the return type of this field as its declared in the C# code base with no modifications or
        /// coerions applied.
        /// </summary>
        /// <value>The type naturally returned by this field.</value>
        Type DeclaredReturnType { get; }

        /// <summary>
        /// Gets the name this field is declared as in the C# code (method name or property name).
        /// </summary>
        /// <value>The name of the declared.</value>
        string DeclaredName { get; }

        /// <summary>
        /// Gets the parent owner of this field.
        /// </summary>
        /// <value>The parent.</value>
        IGraphTypeTemplate Parent { get; }

        /// <summary>
        /// Gets the kind of graph type that should own fields created from this template.
        /// </summary>
        /// <value>The kind.</value>
        TypeKind OwnerTypeKind { get; }

        /// <summary>
        /// Gets the type expression that represents the data returned from this field (i.e. the '[SomeType!]'
        /// declaration used in schema definition language.)
        /// </summary>
        /// <value>The type expression.</value>
        GraphTypeExpression TypeExpression { get; }

        /// <summary>
        /// Gets the list type wrappers, if defined, used to generate a type expression for this field.
        /// This list represents the type requirements of the field. When null or not supplied, the type
        /// expression will be inferred by the return type of the field.
        /// </summary>
        /// <value>The custom wrappers.</value>
        MetaGraphTypes[] DeclaredTypeWrappers { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="TypeExpression"/>
        /// of this instance is custom or otherwise supplied by the devloper and not inferred
        /// by the code written.
        /// </summary>
        /// <value><c>true</c> if this instance is custom type expression; otherwise, <c>false</c>.</value>
        bool IsCustomTypeExpression { get; }
    }
}