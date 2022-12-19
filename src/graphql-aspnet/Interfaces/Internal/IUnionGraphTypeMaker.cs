﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Internal.Interfaces
{
    using GraphQL.AspNet.Engine.TypeMakers;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A type maker targeting the generation of unions from pre-configured proxy classes.
    /// </summary>
    public interface IUnionGraphTypeMaker
    {
        /// <summary>
        /// Creates a union graph type from the given proxy.
        /// </summary>
        /// <param name="proxy">The proxy to convert to a union.</param>
        /// <returns>GraphTypeCreationResult.</returns>
        GraphTypeCreationResult CreateUnionFromProxy(IGraphUnionProxy proxy);
    }
}