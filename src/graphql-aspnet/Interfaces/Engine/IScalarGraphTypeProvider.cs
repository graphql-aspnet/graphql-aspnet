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
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// An interface describing the scalar collection used by a schema, at runtime. Scalars are a
    /// fundimental unit of graphql and must be explicitly defined.
    /// </summary>
    /// <remarks>
    /// The object implementing this
    /// provider should be designed in a thread-safe, singleton fashion. Only one
    /// instance of this provider exists for the application instance.
    /// </remarks>
    public interface IScalarGraphTypeProvider
    {
        /// <summary>
        /// Determines whether the supplied type is considered a leaf type in the graph system
        /// (i.e. is the type a scalar or an enumeration).
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><c>true</c> if the specified type is a leaf type; otherwise, <c>false</c>.</returns>
        bool IsLeaf(Type type);

        /// <summary>
        /// Converts the given type to its formal reference type removing any
        /// nullability modifications that may be applied (e.g. converts <c>int?</c> to <c>int</c>).
        /// If the supplied type is already a a formal reference or if it is not a valid scalar type
        /// it is returned unchanged.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>The supplied type or the formal scalar type representation if the supplied type
        /// is a scalar.</returns>
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
        /// <param name="scalarName">Name of the scalar. This value is case-sensitive.</param>
        /// <returns><c>true</c> if the specified name is a scalar; otherwise, <c>false</c>.</returns>
        bool IsScalar(string scalarName);

        /// <summary>
        /// Retrieves the mapped concrete type assigned to the given scalar name or null if no
        /// scalar is registered.
        /// </summary>
        /// <remarks>
        /// This method converts a scalar name to its primary represented type. (e.g. "Int" => <c>int</c>).
        /// </remarks>
        /// <param name="scalarName">Name of the scalar.</param>
        /// <returns>The system type representing the scalar.</returns>
        Type RetrieveConcreteType(string scalarName);

        /// <summary>
        /// Retrieves the name of the scalar registered for the given concrete type.
        /// </summary>
        /// <remarks>
        /// This method converts a type representation to the scalar's common name. (e.g. <c>int</c> => "Int").
        /// </remarks>
        /// <param name="concreteType">The concrete type which is registered as a known scalars.</param>
        /// <returns>System.String.</returns>
        string RetrieveScalarName(Type concreteType);

        /// <summary>
        /// Creates a new instance of the scalar by its defined graph type name or null if no
        /// scalar is registered.
        /// </summary>
        /// <param name="scalarName">The common name of the scalar as it exists
        /// in a schema.</param>
        /// <returns>IScalarType.</returns>
        IScalarGraphType CreateScalar(string scalarName);

        /// <summary>
        /// Creates a new instance of the scalar by an assigned concrete type or null if no
        /// scalar is registered.
        /// </summary>
        /// <param name="concreteType">The concrete type representing the scalar (e.g. int, float, string etc.).</param>
        /// <returns>IScalarType.</returns>
        IScalarGraphType CreateScalar(Type concreteType);

        /// <summary>
        /// Registers the custom scalar type as a pre-parsed template to the provider. This type
        /// must implement <see cref="IScalarGraphType"/>.
        /// </summary>
        /// <param name="scalarType">The graph type definition of the scalar to register.
        /// This type must implement <see cref="IScalarGraphType"/>.</param>
        void RegisterCustomScalar(Type scalarType);

        /// <summary>
        /// Gets a list of all registered scalar instance types (i.e. the types that
        /// implement <see cref="IScalarGraphTypeProvider"/>).
        /// </summary>
        /// <value>An enumeration of all registered scalar instance types.</value>
        IEnumerable<Type> ScalarInstanceTypes { get; }
    }
}