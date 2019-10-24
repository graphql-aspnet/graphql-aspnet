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
    using System;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// An interface describing an arbitrary <see cref="Type"/> that will be injected into
    /// an object graph. This is the base interface that will universially capture all template types.
    /// </summary>
    public interface IGraphItemTemplate
    {
        /// <summary>
        /// When overridden in a child class, allows the template to perform some final validation checks
        /// on the integrity of itself. An exception should be thrown to stop the template from being
        /// persisted if the object is unusable or otherwise invalid in the manner its been built.
        /// </summary>
        void ValidateOrThrow();

        /// <summary>
        /// Parses the template contents according to the rules of the template.
        /// </summary>
        void Parse();

        /// <summary>
        /// Gets the formal name of this item as it exists in the object graph.
        /// </summary>
        /// <value>The publically referenced name of this field in the graph.</value>
        string Name { get; }

        /// <summary>
        /// Gets the human-readable description distributed with this field
        /// when requested. The description should accurately describe the contents of this field
        /// to consumers.
        /// </summary>
        /// <value>The publically referenced description of this field in the type system.</value>
        string Description { get; }

        /// <summary>
        /// Gets a the canonical path on the graph where this item sits.
        /// </summary>
        /// <value>The route.</value>
        GraphFieldPath Route { get; }

        /// <summary>
        /// Gets the singular concrete type this definition supplies to the object graph any Task or IEnumerable
        /// wrappers are removed.
        /// </summary>
        /// <value>The type of the object.</value>
        Type ObjectType { get; }

        /// <summary>
        /// Gets the fully qualified name, including namespace, of this item as it exists in the .NET code (e.g. 'Namespace.ObjectType.MethodName').
        /// </summary>
        /// <value>The internal name given to this item.</value>
        string InternalFullName { get; }

        /// <summary>
        /// Gets the name that defines this item within the .NET code of the application; typically a method name or property name.
        /// </summary>
        /// <value>The internal name given to this item.</value>
        string InternalName { get; }

        /// <summary>
        /// Gets a value indicating whether this instance was explictly declared as a graph item via acceptable attribution or
        /// if it was parsed as a matter of completeness.
        /// </summary>
        /// <value><c>true</c> if this instance is explictly declared; otherwise, <c>false</c>.</value>
        bool IsExplicitDeclaration { get; }
    }
}