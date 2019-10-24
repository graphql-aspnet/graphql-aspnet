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
    using GraphQL.AspNet.Defaults.TypeMakers;

    /// <summary>
    /// An object that can create a specific graph type from its associated concrete type according to a set of rules
    /// it defines.
    /// </summary>
    public interface IGraphTypeMaker
    {
        /// <summary>
        /// Inspects the given type and, in accordance with the rules of this maker, will
        /// generate a complete set of necessary graph types required to support it.
        /// </summary>
        /// <param name="concreteType">The concrete type to incorporate into the schema.</param>
        /// <returns>GraphTypeCreationResult.</returns>
        GraphTypeCreationResult CreateGraphType(Type concreteType);
    }
}