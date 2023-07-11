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
    using GraphQL.AspNet.Engine.TypeMakers;
    using GraphQL.AspNet.Interfaces.Internal;

    /// <summary>
    /// An object that can create a specific graph type from its associated concrete type according to a set of rules
    /// it defines.
    /// </summary>
    public interface IGraphTypeMaker
    {
        /// <summary>
        /// Inspects the given type and, in accordance with the rules of this maker, will
        /// generate a complete a graph type and a complete set of dependencies required to support it.
        /// </summary>
        /// <param name="typeTemplate">The graph type template to use when creating
        /// a new graph type.</param>
        /// <returns>GraphTypeCreationResult.</returns>
        GraphTypeCreationResult CreateGraphType(IGraphTypeTemplate typeTemplate);
    }
}