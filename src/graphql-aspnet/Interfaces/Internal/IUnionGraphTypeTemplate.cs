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
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A template interface representing a UNION graph type.
    /// </summary>
    public interface IUnionGraphTypeTemplate : IGraphTypeTemplate
    {
        /// <summary>
        /// Gets the type declared as the union proxy; the type that implements <see cref="IGraphUnionProxy"/>.
        /// </summary>
        /// <value>The type of the union proxy.</value>
        Type ProxyType { get; }
    }
}