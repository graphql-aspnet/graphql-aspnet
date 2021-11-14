// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.Execution
{
    using System;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A resolver that can inspect a type, usually returned by a resolver, and determine which type of the
    /// union it should mimic.
    /// </summary>
    public interface IUnionValueTypeResolver
    {
        /// <summary>
        /// When overriden in a child class, attempts to resolve the provided <paramref name="runtimeObjectType"/> into
        /// one of the acceptable types of this union. This method should return a member of the
        /// <see cref="IUnionGraphType"/> types collection. A returned type not in the union's type collection
        /// will be rejected.
        /// </summary>
        /// <param name="runtimeObjectType">The type of an object provided at runtime as the result
        /// of a controller or method operation executed by graphql.</param>
        /// <returns>A type registered to this union that <paramref name="runtimeObjectType"/> inherits from.</returns>
        Type ResolveType(Type runtimeObjectType);
    }
}