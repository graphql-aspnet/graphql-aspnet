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
    /// An object that can inspect a type, usually returned by a resolver, and determine which type in the target
    /// union it should be interpreted as.
    /// </summary>
    public interface IUnionTypeMapper
    {
        /// <summary>
        /// This method attempts to resolve the provided <paramref name="runtimeObjectType"/> into
        /// one of the acceptable types of the target union. This method should return a member of
        /// <see cref="IUnionGraphType.PossibleConcreteTypes"/>. A returned type not in the union's type collection
        /// will be rejected.
        /// </summary>
        /// <param name="runtimeObjectType">The type of an object provided at runtime as the result
        /// of a controller or method operation executed by graphql.</param>
        /// <returns>A type registered to this union that <paramref name="runtimeObjectType"/> inherits from.</returns>
        Type MapType(Type runtimeObjectType);
    }
}