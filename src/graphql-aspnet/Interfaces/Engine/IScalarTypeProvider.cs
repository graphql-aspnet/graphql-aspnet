// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Engine
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An interface describing the scalar collection classes used by a schema, at runtime. Scalars are a fundimental unit of graphql
    /// and must be explicitly defined. The object implementing this provider should be designed in a thread-safe, singleton fashion. Only one
    /// instance of this provider exists for the application instance.
    /// </summary>
    public interface IScalarTypeProvider
    {
        /// <summary>
        /// Determines whether the specified type is considered a leaf type in the graph system (Scalars and enumerations).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the specified type is leaf; otherwise, <c>false</c>.</returns>
        bool IsLeaf(Type type);

        /// <summary>
        /// Converts the given scalar reference to its formal reference type removing any
        /// nullability modifications that may be applied (e.g. converts 'int?' to 'int'). If the supplied
        /// type is already a a formal reference or is not a valid scalar type it is returned unchanged.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Type.</returns>
        Type EnsureBuiltInTypeReference(Type type);

        /// <summary>
        /// Determines whether the specified concrete type is a known scalar.
        /// </summary>
        /// <param name="concreteType">Type of the concrete.</param>
        /// <returns><c>true</c> if the specified concrete type is a scalar; otherwise, <c>false</c>.</returns>
        bool IsScalar(Type concreteType);

        /// <summary>
        /// Determines whether the specified name represents a known scalar.
        /// </summary>
        /// <param name="scalarName">Name of the scalar.</param>
        /// <returns><c>true</c> if the specified name is a scalar; otherwise, <c>false</c>.</returns>
        bool IsScalar(string scalarName);

        /// <summary>
        /// Retrieves the mapped concrete type assigned to the given scalar name or null if no
        /// scalar is registered.
        /// </summary>
        /// <param name="scalarName">Name of the scalar.</param>
        /// <returns>Type.</returns>
        Type RetrieveConcreteType(string scalarName);

        /// <summary>
        /// Retrieves the name of the scalar registered for the given concrete type.
        /// </summary>
        /// <param name="concreteType">The concrete type which is registered as a known scalars.</param>
        /// <returns>System.String.</returns>
        string RetrieveScalarName(Type concreteType);

        /// <summary>
        /// Creates a new instance of the scalar by its defined graph type name or null if no
        /// scalar is registered.
        /// </summary>
        /// <param name="scalarName">Name of the scalar.</param>
        /// <returns>IScalarType.</returns>
        IScalarGraphType CreateScalar(string scalarName);

        /// <summary>
        /// Creates a new instance of the scalar by an assigned concrete type or null if no
        /// scalar is registered.
        /// </summary>
        /// <param name="concreteType">Type of the concrete.</param>
        /// <returns>IScalarType.</returns>
        IScalarGraphType CreateScalar(Type concreteType);

        /// <summary>
        /// Registers the custom scalar type as a pre-parsed template to the provider.
        /// </summary>
        /// <param name="scalarType">Type of the scalar to register.</param>
        void RegisterCustomScalar(Type scalarType);

        /// <summary>
        /// Gets a list of all registered scalar instance types (i.e. the types that
        /// implement <see cref="IScalarTypeProvider"/>).
        /// </summary>
        /// <value>An enumeration of all registered scalar instance types.</value>
        IEnumerable<Type> ScalarInstanceTypes { get; }
    }
}